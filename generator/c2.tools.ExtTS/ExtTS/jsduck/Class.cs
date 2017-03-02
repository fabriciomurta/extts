using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtTS.jsduck
{
    [Serializable]
    public sealed class Class
    {
        #region Properties

        public string tagname;
        public string name;
        public string[] alternateClassNames;
        public string extends;
        public bool? singleton;
        public Member[] members;
        public string[] mixins;
        public ClassEnum @enum;

        public ClassFile[] files;
        public Dictionary<string, HtmlDocument> docs;
        public string code_type;
        public string id;
        public bool component;
        public string short_doc;
        public string html;

        public Member[] OwnMembers
        {
            get { return ownMembers ?? (ownMembers = members.Where(m => name == m.owner && !m.meta.@private).ToArray()); }
        }
        private Member[] ownMembers;

        public string Href
        {
            get { return href ?? (href = name.Replace('.', '-')); }
        }
        private string href;

        public override string ToString()
        {
            return name;
        }

        #endregion

        #region Builder

        public void Initialize(Dictionary<string, Class> classMap, Dictionary<string, HtmlDocument> jsFileHtmlMap)
        {
            var docs = new Dictionary<string, HtmlDocument>(files.Distinct(ClassFile.Comparer.Default).ToDictionary(f => f.LocalFile, f => jsFileHtmlMap[f.LocalFile]));

            if (!String.IsNullOrEmpty(html))
            {
                var htmlDoc = new HtmlDocument();
                if (htmlDoc != null)
                    htmlDoc.LoadHtml(html);
                foreach (var member in OwnMembers)
                {
                    var aViewSource = htmlDoc.DocumentNode.SelectSingleNode($@"//div[@id = '{member.id}']").Descendants("a").Where(a => a.GetAttributeValue("class", null) == "view-source" && !String.IsNullOrEmpty(a.GetAttributeValue("href", null))).SingleOrDefault();
                    if (aViewSource != null)
                    {
                        var href = aViewSource.GetAttributeValue("href", null);
                        var pos = href.IndexOf('#');
                        href = pos > 0 ? href.Substring(0, pos) : href;
                        pos = href.LastIndexOf('/');
                        href = pos > 0 ? href.Substring(pos + 1) : href;
                        if (!docs.ContainsKey(href) && jsFileHtmlMap.ContainsKey(href))
                            docs.Add(href, jsFileHtmlMap[href]);
                    }
                }
            }
            this.docs = docs;
            foreach (var member in OwnMembers)
                member.Initialize(classMap, this);
        }

        #endregion
    }

    #region Class classes

    [Serializable]
    public sealed class ClassFile
    {
        public string filename;
        public string href;

        public string LocalFile
        {
            get
            {
                if (localfile == null)
                {
                    var index = href.IndexOf('#');
                    localfile = (index >= 0) ? href.Substring(0, index) : "";
                }

                return localfile;
            }
        }
        private string localfile;

        public override string ToString()
        {
            return LocalFile;
        }

        public class Comparer : IEqualityComparer<ClassFile>
        {
            public readonly static Comparer Default = new Comparer();

            private Comparer() { }

            public bool Equals(ClassFile x, ClassFile y)
            {
                return x.filename == y.filename && x.href == y.href;
            }

            public int GetHashCode(ClassFile obj)
            {
                return obj.filename.GetHashCode() ^ obj.href.GetHashCode();
            }
        }
    }

    [Serializable]
    public sealed class ClassEnum
    {
        public string type;
        public string @default;
        public bool doc_only;
    }

    #endregion
}
