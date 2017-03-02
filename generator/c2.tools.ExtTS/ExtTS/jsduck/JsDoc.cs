using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ExtTS.jsduck
{
    public static class JsDoc
    {
        public readonly static string[] NEWLINES = new string[] { Environment.NewLine };
        public readonly static string[] EMPTYLINES = new string[] { };
        public readonly static char[] SPACES = new char[] { ' ' };
        public readonly static char[] COMMAS = new char[] { ',' };
        public readonly static char[] BLANKS = new char[] { ' ', '\t' };
        public readonly static char[] SEPARATORS = new char[] { ' ', '\t', '\n', '\r' };
        public readonly static char[] EXTTYPESEP = new char[] { '/', '|' };

        public readonly static HashSet<string> UnknownExtTypes = new HashSet<string>();

        public readonly static Regex MemberNameValidator = new Regex("^[a-zA-Z$_][a-zA-Z$_0-9]*$", RegexOptions.Compiled);

        /// <summary>
        /// Comment params is removed from comment
        /// </summary>
        public readonly static HashSet<string> IgnoredParams = new HashSet<string>()
        {
            "static", "singleton",
            "private", "protected", //"readonly",
            "template", "accessor", "preventable",
            "localdoc", "inheritdoc", "chainable", "declarativeHandler", "evented",
        };

        /// <summary>
        /// Only text after the param name is added to the comment.
        /// The text before from the param name will be considered as data
        /// Ex: @property {String/Number} value This is a value param of the Object
        /// </summary>
        public readonly static HashSet<string> ExtractableParams = new HashSet<string>()
        {
            "class", "cfg", "event", "property", "method", "type", "member",
            // TOTO: @alias, @inheritdoc
        };

        /// <summary>
        /// All the comment is kept. The text before from the param name will be considered as data
        /// Ex: @param {String/Number} value This is a value param of the Method
        /// </summary>
        public readonly static HashSet<string> ExtractableButKeepCommentParams = new HashSet<string>()
        {
            "param",
        };

        public delegate string CommentParamExtractor(string type, string data, string commentLine);

        public static string[] ExtractComments(string spanID, string itemID, Dictionary<string, HtmlDocument> docs, out Dictionary<string, HashSet<string>> jsHtmlParams)
        {
            var htmlParams = jsHtmlParams = new Dictionary<string, HashSet<string>>();
            var htmlComments = JsDoc.ExtractComments(spanID, itemID, docs,
                (string type, string data, string commentLine) =>
                {
                    if (IgnoredParams.Contains(type))
                        commentLine = null;
                    else if (ExtractableParams.Contains(type) || ExtractableButKeepCommentParams.Contains(type))
                    {
                        var sep1 = data == null || data.Length <= 0 ? -1 : data[0] != '{' ? data.IndexOf(' ') : (data.IndexOf('}') + 1);
                        if (sep1 <= 0 || sep1 >= data.Length)
                        {
                            commentLine = null;
                            if (sep1 > 0 && data[sep1 - 1] == '}' && data[0] == '{')
                                data = data.Substring(1, data.Length - 2);
                        }
                        else
                        {
                            var start = sep1 + 1;
                            for (; data[start] == ' ' && start < data.Length; start++) ;
                            var sep2 = data[start] == '[' ? (data.IndexOf(']', start) + 1) : data.IndexOf(' ', start);
                            if (sep2 <= 0)
                                commentLine = null;
                            else
                            {
                                if (ExtractableParams.Contains(type))
                                {
                                    if (data.Length <= sep2)
                                        commentLine = null;
                                    else
                                    {
                                        commentLine = data.Substring(sep2 + 1).TrimStart();
                                        if (commentLine.Length <= 0)
                                            commentLine = null;
                                        else
                                            commentLine = "* " + commentLine;
                                    }
                                }
                                data = data.Substring(0, sep2);
                            }
                        }
                    }
                    if (!htmlParams.ContainsKey(type))
                        htmlParams.Add(type, new HashSet<string>() { data });
                    else
                        htmlParams[type].Add(data);
                    return commentLine;
                }
            );
            return htmlComments;
        }

        public static string[] ExtractComments(string spanID, string itemID, Dictionary<string, HtmlDocument> docs, CommentParamExtractor paramExtractor = null)
        {
            var comments = new List<string>();
            for (var node = docs.Values.Select(d => d.DocumentNode.SelectSingleNode($@"//span[@id = '{spanID}']")).Where(n => n != null).FirstOrDefault();
                node != null; node = node.NextSibling == null || node.NextSibling.Name != "#text" ? null : node.NextSibling)
            {
                var lines = node.InnerText.Split(NEWLINES, StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray();
                foreach (var line in lines)
                {
                    if (comments.Count <= 0) // Search for BEGIN if EMPTY
                    {
                        var start = line.IndexOf("/**");
                        if (start < 0)
                        {
                            if (lines.Length > 1 || paramExtractor == null)
                                continue;
                            // If only 1 line without comments
                            // Examples:
                            //      - "constructor: function (config) {"
                            //      - "boundSeries: [],"
                            //      - "masterAxis: null,"
                            var sep = line.IndexOf(':');
                            if (sep < 0)
                                continue;
                            sep++;
                            while (sep < line.Length && line[sep] == ' ') sep++;
                            if (sep >= (line.Length - 1))
                                continue;
                            var value = line.Substring(sep);
                            if (value.StartsWith("function"))
                            {
                                var open = value.IndexOf('(');
                                if (open < 0)
                                    continue;
                                var close = value.IndexOf(')');
                                if (open < 0 || open >= (close - 1))
                                    continue;
                                foreach (var functionParam in value.Substring(open + 1, close - open - 1).Split(COMMAS, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
                                {
                                    var functionParam2 = functionParam;
                                    // /* private */ skipValidation
                                    if (functionParam2.StartsWith("/* "))
                                    {
                                        var closing = functionParam2.IndexOf("*/");
                                        if (closing > 0)
                                        {
                                            for (closing += 2; functionParam2[closing] == ' '; closing++) ;
                                            functionParam2 = functionParam2.Substring(closing);
                                        }
                                    }
                                    paramExtractor("param", $@"{{Object}} {functionParam2}", null);
                                }
                            }
                            else
                            {
                                //var comma = value.IndexOf(',');
                                //if (comma >= 0)
                                //    value = value.Substring(0, comma);
                                continue;
                            }
                        }
                        else
                        {
                            var commentLine = NormalizeComments(line.Substring(start));

                            // Continue search for ending
                            if (commentLine.EndsWith("*/"))
                                // Only 1 comment line
                                return new string[] { commentLine };
                            else
                                // 1st comment line, and continue to next BLOCK
                                comments.Add(commentLine);
                        }
                    }
                    else
                    {
                        var commentLine = NormalizeComments(line);

                        // If not ENDING: check for comment format from the follow lines
                        if (!commentLine.EndsWith("*/"))
                        {
                            if (!commentLine.StartsWith("* @") || commentLine.Length <= 3)
                                comments.Add(' ' + commentLine);
                            else
                            {
                                var pos = commentLine.IndexOfAny(BLANKS, 3);
                                string type, data;
                                if (pos > 0)
                                {
                                    type = commentLine.Substring(3, pos - 3);
                                    data = pos >= (commentLine.Length - 2) ? null : commentLine.Substring(pos + 1);
                                }
                                else
                                {
                                    type = commentLine.Substring(3);
                                    data = null;
                                }
                                if (paramExtractor != null)
                                    commentLine = paramExtractor(type, data, commentLine);
                                if (commentLine != null)
                                    comments.Add(' ' + commentLine);
                            }
                        }
                        else
                        {
                            // Check for EMPTY comment
                            if (commentLine == "*/" && (comments.ToString() == ("/**" + Environment.NewLine)))
                                return null;
                            else
                            {
                                comments.Add(' ' + commentLine);
                                var deletionCount = 0;
                                for (var i = 1; i < comments.Count; i++)
                                    if (comments[i] == " *")
                                        deletionCount++;
                                    else
                                        break;
                                if (deletionCount > 0)
                                    comments.RemoveRange(1, deletionCount);
                                for (var i = comments.Count - 2; i >= 0; i--)
                                    if (comments[i] == " *")
                                        comments.RemoveAt(i);
                                    else
                                        break;
                                return comments.Count <= 2 ? null : comments.ToArray();
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static void AddComments(List<string> listSources, string comments)
        {
            var lines = comments.Split(JsDoc.NEWLINES, StringSplitOptions.None);
            if (lines.Length > 0)
            {
                var start = 0;
                for (; start < lines.Length && lines[start].Length <= 0; start++) ;
                var end = lines.Length - 1;
                for (; end >= 0 && lines[end].Length <= 0; end--) ;
                if ((end - start + 1) > 0)
                    listSources.AddRange(lines.Skip(start).Take(end - start + 1));
            }
        }

        public static string NormalizeComments(string comment)
        {
            return WebUtility.HtmlDecode(comment.IndexOf('`') >= 0 ? comment.Replace('`', '\'') : comment);
        }

        public sealed class CommentParamArg
        {
            public CommentParamArg(string extType, string variable, bool isOptional, string defaultValue, Dictionary<string, jsduck.Class> jsClassMap)
            {
                this.IsRest = false;
                this.IsOptional = isOptional;
                this.Variable = NormalizeVariableName(variable);
                this.DefaultValue = defaultValue;
                this.Type = JsDoc.ExtTypeToTS(extType, ref this.IsOptional, ref this.IsRest, jsClassMap);
            }

            public string Type;
            public string Variable;
            public bool IsOptional;
            public bool IsRest;
            public string DefaultValue;

            public override string ToString()
            {
                return $@"{(IsRest ? "..." : "")}{Variable}{(!IsRest && IsOptional ? "?" : "")}: {Type}{(IsRest ? "[]" : "")}";//{(DefaultValue == null ? "" : $@" = {DefaultValue}")}";
            }

            private static string NormalizeVariableName(string name)
            {
                if (name == null || name.Length <= 0)
                    return null;

                switch (name)
                {
                    case "class":
                        return "clazz";
                    case "this":
                        return "that";
                    case "new":
                        return "nevv";
                    default:
                        return name.IndexOf('.') < 0 ? name : name.Replace('.', '_');
                }
            }
        }

        /// <summary>
        /// Extract [ExtType, Variable, Optional, Value] from JsDoc comment: // {ExtType} [x=v]
        /// </summary>
        public static CommentParamArg ExtractCommentParamArgs(string commentParam, Dictionary<string, jsduck.Class> jsClassMap)
        {
            if (commentParam == null || commentParam.Length <= 1)
                return null;
            if (commentParam[0] != '{')
                return null;
            var nextPos = commentParam.IndexOf('}');
            if (nextPos < 0)
                return null;
            var extType = commentParam.Substring(1, nextPos - 1);
            if (commentParam.Length <= (nextPos + 1))
                return new CommentParamArg(extType, null, false, null, jsClassMap);

            if (commentParam[nextPos + 1] == ' ')
            {
                for (nextPos++; commentParam[nextPos] == ' ' && nextPos < commentParam.Length; nextPos++) ;
                nextPos -= 2;
            }

            int nextSep;
            if (commentParam.Length < (nextPos + 7) || commentParam[nextPos + 1] != ' ' || commentParam[nextPos + 2] != '[')
            {
                if (commentParam[nextPos + 1] != ' ')
                    return new CommentParamArg(extType, commentParam.Substring(nextPos + 2), false, null, jsClassMap);
                var optional = commentParam[nextPos + 2] == '[';
                nextSep = commentParam.IndexOfAny(SEPARATORS, nextPos + 2);
                if (nextSep < 0)
                    return new CommentParamArg(extType, !optional ? commentParam.Substring(nextPos + 2) : commentParam.Substring(nextPos + 3, commentParam.Length - nextPos - 4), optional, null, jsClassMap);
                return new CommentParamArg(extType, !optional ? commentParam.Substring(nextPos + 2, nextSep - nextPos - 2) : commentParam.Substring(nextPos + 3, nextSep - nextPos - 4), optional, null, jsClassMap);
            }
            var equalPos = commentParam.IndexOf('=', nextPos + 3);
            var closePos = commentParam.IndexOf(']', nextPos + 3);
            if (equalPos < 0 || commentParam.Length < (equalPos + 2))
            {
                if (closePos >= 0)
                    return new CommentParamArg(extType, commentParam.Substring(nextPos + 3, closePos - nextPos - 3), true, null, jsClassMap);
                return new CommentParamArg(extType, commentParam.Substring(nextPos + 2), false, null, jsClassMap);
            }
            if (closePos >= 0)
                return new CommentParamArg(extType, commentParam.Substring(nextPos + 3, equalPos - nextPos - 3), true, commentParam.Substring(equalPos + 1, closePos - equalPos - 1), jsClassMap);
            nextSep = commentParam.IndexOfAny(SEPARATORS, nextPos + 2);
            if (nextSep < 0)
                return new CommentParamArg(extType, commentParam.Substring(nextPos + 2), false, null, jsClassMap);
            return new CommentParamArg(extType, commentParam.Substring(nextPos + 2, nextSep - nextPos - 2), false, null, jsClassMap);
        }

        public static string ExtTypeToTS(string extType, ref bool isOptional, ref bool isRest, Dictionary<string, jsduck.Class> jsClassMap)
        {
            if (extType[0] == ' ' || extType[extType.Length - 1] == ' ')
                extType = extType.Trim();
            if (extType.IndexOfAny(EXTTYPESEP) < 0)
                return JsDoc.ExtTypeToTSMap(extType, ref isOptional, ref isRest, jsClassMap) ?? "any";
            var list = new List<string>();
            foreach (var singeType in extType.Split(EXTTYPESEP))
            {
                var tsType = JsDoc.ExtTypeToTSMap(singeType, ref isOptional, ref isRest, jsClassMap);
                if (tsType != null)
                    list.Add(tsType);
            }
            return String.Join("|", !isRest ? list.Distinct() : list.Select(i => i.EndsWith("[]") ? i : (i + "[]")).Distinct());
        }

        private static string ExtTypeToTSMap(string extType, ref bool isOptional, ref bool isRest, Dictionary<string, jsduck.Class> jsClassMap)
        {
            if (extType.EndsWith("..."))
            {
                if (!isRest)
                    isRest = true;
                extType = extType.Substring(0, extType.Length - 3);
            }

            string arrayPortfix;
            if (!extType.EndsWith("[]"))
                arrayPortfix = String.Empty;
            else
            {
                var pos = extType.IndexOf("[]");
                extType = extType.Substring(0, pos);
                arrayPortfix = extType.Substring(pos);
            }

            if (jsClassMap.ContainsKey(extType))
                return jsClassMap[extType].name + arrayPortfix;

            if (StandardJsTypes.ContainsKey(extType))
            {
                var standardJsTypes = StandardJsTypes[extType];
                if (standardJsTypes.Item2 && !isOptional)
                    isOptional = true;
                if (standardJsTypes.Item3 && !isRest)
                    isRest = true;
                return standardJsTypes.Item1 == null ? null : (standardJsTypes.Item1 + arrayPortfix);
            }

            if (!UnknownExtTypes.Contains(extType))
                UnknownExtTypes.Add(extType);
            Console.WriteLine($@"Not found ExtType: '{extType}'");
            return "any" + arrayPortfix;
        }

        /// <summary>
        /// Map from JsType to [ExtType, optional, Rest]
        /// </summary>
        public readonly static Dictionary<string, Tuple<string, bool, bool>> StandardJsTypes = new Dictionary<string, Tuple<string, bool, bool>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Date", new Tuple<string, bool, bool>("Date", false, false) },
            { "Function", new Tuple<string, bool, bool>("Function", false, false) },
            { "HTMLElement", new Tuple<string, bool, bool>("HTMLElement", false, false) },
            { "Window", new Tuple<string, bool, bool>("Window", false, false) },
            { "Event", new Tuple<string, bool, bool>("Event", false, false) },
            { "Error", new Tuple<string, bool, bool>("Error", false, false) },
            { "RegExp", new Tuple<string, bool, bool>("RegExp", false, false) },
            { "String", new Tuple<string, bool, bool>("string", false, false) },
            { "Boolean", new Tuple<string, bool, bool>("boolean", false, false) },
            { "Number", new Tuple<string, bool, bool>("number", false, false) },
            { "null", new Tuple<string, bool, bool>(null, true, false) },
            { "undefined", new Tuple<string, bool, bool>(null, true, false) },
            { "Array", new Tuple<string, bool, bool>("any[]", false, false) },
            { "", new Tuple<string, bool, bool>("any", false, false) },
            { "*", new Tuple<string, bool, bool>("any", false, true) },
            { "Object", new Tuple<string, bool, bool>("any", false, false) },
            { "Mixed", new Tuple<string, bool, bool>("any", false, false) },
            { "Arguments", new Tuple<string, bool, bool>(null, false, false) },
        };
    }
}
