using System.Collections.Generic;
using System.Linq;

namespace ExtTS.model
{
    class Module : Class
    {
        public Module(string name, string[] comments, FileTS fileTS, Dictionary<string, jsduck.Class> jsClassMap)
            : base(name, comments, null, false, null, jsClassMap)
        {
            this.FileTS = fileTS ?? (FileTS)this;
        }

        public readonly FileTS FileTS;
        public readonly List<Class> Classes = new List<Class>();

        protected override string Signature
        {
            get { return $"declare module {this.Name}"; }
        }

        protected override IEnumerable<Base> Children
        {
            get { return this.Classes.OrderBy(o => o.SortOrder); }
        }

        public override string SortOrder
        {
            get { return this.Name; }
        }

        public Module Add(Module cls)
        {
            return (Module)this.Add((Class)cls);
        }

        public Class Add(Class cls)
        {
            cls.Module = this;
            this.Classes.Add(cls);
            if (cls is Module)
                this.FileTS.ModuleMap.Add(cls.Name, (Module)cls);
            else
                this.FileTS.ClassMap.Add(cls.Name, cls);
            return cls;
        }
    }
}
