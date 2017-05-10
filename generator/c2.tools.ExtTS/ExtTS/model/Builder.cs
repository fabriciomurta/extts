using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using ExtTS.jsduck;
using System.IO;

namespace ExtTS.model
{
    sealed class Builder
    {
        private readonly jsduck.Class[] JsClasses;
        private readonly Dictionary<string, jsduck.Class> JsClassMap;
        private readonly Dictionary<string, jsduck.Class[]> JsModuleClasses;
        public readonly static string AppVersion = typeof(Builder).Assembly.GetName().Version.ToString();
        public readonly string ExtVersion;

        /// <summary>
        /// Build the model.FileTS from JsDuck generated document for ExtJS sourcecode
        /// </summary>
        /// <param name="docOutputPath">*.js</param>
        /// <param name="docSourcePath">*.html</param>
        /// <param name="version">ExtJS version</param>
        public Builder(string docOutputPath, string docSourcePath, string version)
        {
            if (!Directory.Exists(docOutputPath))
                throw new FileNotFoundException(docOutputPath);
            if (!Directory.Exists(docSourcePath))
                throw new FileNotFoundException(docSourcePath);

            // Load all document guides/comments from *.html markup sourcecode
            Console.Write("HTML.SOURCES: ");
            var jsFileHtmlMap = Utils.FindHtmls(docSourcePath).ToDictionary(o => o.Item1, o => o.Item2);
            ExtVersion = version;

            // Load all JsDuck.Classes meta data information from *.js
            Console.Write("JS.FILES");
            JsClasses = Utils.FindClasses(docOutputPath, "FILE").ToArray(); // LoadWithCache(CachePath, () => FindClasses(outputPath).ToArray());
            JsClassMap = JsClasses.SelectMany(o => o.alternateClassNames.Select(a => Tuple.Create(a, o)))
                .Concat(JsClasses.Select(c => Tuple.Create(c.name, c)))
                .ToDictionary(o => o.Item1, o => o.Item2);
            JsModuleClasses = JsClasses.GroupBy(o => o.name.IndexOf('.') <= 0 ? "" : o.name.Substring(0, o.name.LastIndexOf('.'))).OrderBy(o => o.Key).ToDictionary(o => o.Key, o => o.ToArray());

            // Initialize JsDuck.Classes
            Console.WriteLine($"JS.CLASSES[{JsClasses.Length}]");
            var count = 0;
            foreach (var jsClass in JsClasses)
            {
                Console.WriteLine($@"CLASS-{++count}/{JsClasses.Length}. {jsClass.name}[{jsClass.OwnMembers.Length}]");
                jsClass.Initialize(JsClassMap, jsFileHtmlMap);
            }
        }

        /// <summary>
        /// Build typescript model from Html source & js class meta information
        /// </summary>
        public model.FileTS Build()
        {
            var tsFile = new model.FileTS(this.ExtVersion, AppVersion);
            var count = 0;
            var total = this.JsModuleClasses.Sum(o => o.Value.Length);

            foreach (var jsModuleClass in this.JsModuleClasses)
            {
                // New module or using tsFile (a root module)
                var tsModule = jsModuleClass.Key.Length <= 0 ? tsFile : tsFile.Add(new Module(jsModuleClass.Key, null, tsFile, this.JsClassMap));

                // Build classes
                foreach (var jsClass in jsModuleClass.Value)
                {
                    Console.Write($@"BUILD-{++count}/{total}. {jsClass.name}");
                    this.BuildClass(tsModule, jsClass);
                }
            }

            // Load classes
            var tsClasses = tsFile.ClassMap.Values.ToArray();
            count = 0;
            total = tsClasses.Length;
            foreach (var tsClass in tsClasses)
            {
                Console.WriteLine($@"LOAD-{++count}/{total}. {tsClass.Name}");
                tsClass.Load(this.JsClassMap);
            }
            return tsFile;
        }

        /// <summary>
        /// Create new tsClass into tsModule with members from jsClass meta information 
        /// </summary>
        private Class BuildClass(Module tsModule, jsduck.Class jsClass)
        {
            Dictionary<string, HashSet<string>> jsHtmlParams;
            string[] jsHtmlComments;

            // Extract class document
            jsHtmlComments = JsDoc.ExtractComments(jsClass.Href, null, jsClass.docs, out jsHtmlParams);
            // Create class
            var tsClass = tsModule.Add(new Class(jsClass.name, jsHtmlComments, jsHtmlParams, jsClass.singleton ?? false, jsClass, this.JsClassMap, jsClass.extends));

            Console.WriteLine($@"[{jsClass.OwnMembers.Length}]");
            foreach (var jsMember in jsClass.OwnMembers)
            {
                // Extract member document
                jsHtmlComments = JsDoc.ExtractComments(jsClass.Href + '-' + jsMember.id, jsMember.id, jsClass.docs, out jsHtmlParams);
                // Create specific member
                Member member;
                switch (jsMember.tagname)
                {
                    case PropertyMember.TAG:
                        member = new PropertyMember(jsMember.name, jsHtmlComments, jsHtmlParams, tsClass, jsMember, this.JsClassMap);
                        break;
                    case MethodMember.TAG:
                        member = new MethodMember(jsMember.name, jsHtmlComments, jsHtmlParams, tsClass, jsMember, this.JsClassMap);
                        break;
                    case ConfigMember.TAG:
                        member = new ConfigMember(jsMember.name, jsHtmlComments, jsHtmlParams, tsClass, jsMember, this.JsClassMap);
                        tsClass.MemberConfigs.Add(member.Name, member);
                        break;
                    case EventMember.TAG:
                        member = new EventMember(jsMember.name, jsHtmlComments, jsHtmlParams, tsClass, jsMember, this.JsClassMap);
                        break;
                    default:
                        throw new NotImplementedException(jsMember.tagname);
                }
                if (!tsClass.Members.ContainsKey(member.Name))
                    tsClass.Members.Add(member.Name, member);
                else switch (jsMember.tagname)
                {
                    case PropertyMember.TAG:
                    case MethodMember.TAG:
                        if (!tsClass.MemberExs.ContainsKey(member.Name))
                            tsClass.MemberExs.Add(member.Name, new HashSet<Member>() { tsClass.Members[member.Name] });
                        else
                            tsClass.MemberExs[member.Name].Add(tsClass.Members[member.Name]);
                        tsClass.Members[member.Name] = member;
                        break;
                    case ConfigMember.TAG:
                    case EventMember.TAG:
                        if (!tsClass.MemberExs.ContainsKey(member.Name))
                            tsClass.MemberExs.Add(member.Name, new HashSet<Member>() { member });
                        else
                            tsClass.MemberExs[member.Name].Add(member);
                        break;
                    default:
                        throw new NotImplementedException(jsMember.tagname);
                }
            }

            return tsClass;
        }

        private string GetVersion(HtmlDocument document, string docSourcePath)
        {
            var text = document.DocumentNode.InnerText;
            // SEARCH for: Ext.setVersion('ext', '6.0.1.250'); Ext.setVersion('core', '6.0.1.250');
            // SEARCH for: var version = '4.2.1.883'
            const string SetVersionCode = "Ext.setVersion('ext','";
            const string SetVersionCode2 = "var version = '";
            int posStart, posEnd;
            var pos = text.IndexOf(SetVersionCode);
            if (pos >= 0)
            {
                posStart = pos + SetVersionCode.Length;
                posEnd = text.IndexOf('\'', posStart);
                return text.Substring(posStart, posEnd - posStart);
            }
            else
            {
                pos = text.IndexOf(SetVersionCode2);
                if (pos >= 0)
                {
                    posStart = pos + SetVersionCode2.Length;
                    posEnd = text.IndexOf('\'', posStart);
                    return text.Substring(posStart, posEnd - posStart);
                }
            }

            // Find version from source (if available)
            var version = Path.GetFileName(Path.GetFullPath(Path.Combine(docSourcePath, @"..")));
            if (version.StartsWith("ext-"))
                version = version.Substring("ext-".Length);
            var sourceVersionPath = Path.GetFullPath(Path.Combine(docSourcePath, $@"..\..\..\..\ext\ext-{version}\version.properties"));
            if (!File.Exists(sourceVersionPath))
                return version;
            text = File.ReadAllText(sourceVersionPath);
            const string SetVersionCode3 = "version.full=";
            pos = text.IndexOf(SetVersionCode3);
            if (pos <= 0)
                return version;
            posStart = pos + SetVersionCode3.Length;
            posEnd = text.IndexOfAny(new char[] { '\n', '\r' }, posStart);
            return text.Substring(posStart, posEnd - posStart);
        }

        public static bool IsSameTS(string ts1, string ts2)
        {
            var x = LoadTsContent(ts1);
            var y = LoadTsContent(ts2);
            return x != null && y != null && x == y;
        }

        public static string LoadTsContent(string ts)
        {
            const string BuildDateCode = "Build date: ";
            const string GTMCode = " (GMT)";
            string code = File.ReadAllText(ts);
            var start = code.IndexOf(BuildDateCode);
            if (start < 0)
                return null;
            start += BuildDateCode.Length;
            var end = code.IndexOf(GTMCode, start);
            if (end < 0)
                return null;
            if ((end - start) >= 20)
                return null;
            return code.Substring(0, start) + code.Substring(end);
        }
    }
}
