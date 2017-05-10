using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtTS.model
{
    abstract class Base
    {
        protected const string TAB = "    ";
        protected readonly static Base[] EmptyBases = new Base[0];
        protected readonly static string[] EmptyComments = new string[0];

        protected Base(Base @base)
        {
            this.Name = @base.Name;
            this.comments = @base.comments;
            this.Params = @base.Params;
        }

        protected Base(string name, string[] comments, Dictionary<string, HashSet<string>> @params, Dictionary<string, jsduck.Class> jsClassMap)
        {
            this.Name = name;
            this.comments = comments ?? EmptyComments;
            this.Params = @params;
        }

        public readonly string Name;
        public readonly string[] comments;
        public readonly Dictionary<string, HashSet<string>> Params;

        public virtual bool Generatable { get { return true; } }
        protected abstract string Signature { get; }
        public virtual string[] Comments { get { return this.comments; } }
        protected abstract IEnumerable<Base> Children { get; }
        public virtual string SortOrder { get { return this.Name; } }

        public override string ToString()
        {
            return $@"{this.GetType().Name}: {this.Name}";
        }

        #region Code Generation

        public void Write(string path)
        {
            if (!this.Generatable)
                return;

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            using (var file = System.IO.File.OpenWrite(path))
            using (var writer = new System.IO.StreamWriter(file))
                this.Write(writer);
        }

        private void Write(System.IO.StreamWriter writer)
        {
            this.Write(writer, String.Empty);
        }

        protected virtual void Write(System.IO.StreamWriter writer, string indent)
        {
            // Comment
            foreach (var comment in this.Comments)
            {
                writer.Write(indent);
                writer.WriteLine(comment);
            }

            // Declaration
            var signature = this.Signature;
            if (signature != null)
            {
                writer.Write(indent);
                writer.Write(signature);
            }

            // Children
            var children = this.Children?.Where(c => c.Generatable);
            if (children != null)
            {
                // Open
                if (signature != null)
                    writer.WriteLine(" {");

                var count = children.Count();
                if (count > 0)
                {
                    // All preceding items (with a blank line in between)
                    var indent2 = signature != null ? (indent + TAB) : indent;
                    if (count > 1)
                        foreach (var child in children.Take(count - 1))
                        {
                            child.Write(writer, indent2);
                            writer.WriteLine();

                            // Separation blank line
                            writer.WriteLine();
                        }

                    // Last item
                    (count <= 1 ? children : children.Skip(count - 1)).Single().Write(writer, indent2);
                    writer.WriteLine();
                }

                // Close
                if (signature != null)
                {
                    writer.Write(indent);
                    writer.Write("}");
                }
            }
        }

        #endregion
    }
}
