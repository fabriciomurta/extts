using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtTS.model
{
    class Class : Base
    {
        public Class(string name, string[] comments, Dictionary<string, HashSet<string>> @params, bool isSingleton, jsduck.Class jsClass, Dictionary<string, jsduck.Class> jsClassMap, string baseClass = null)
            : base(name, comments, @params, jsClassMap)
        {
            this.IsSingleton = isSingleton;
            this.BaseClass = baseClass == null || !jsClassMap.ContainsKey(baseClass) ? null : jsClassMap[baseClass].name;
            this.JsClass = jsClass;
            this.constructor = new Lazy<MethodMember>(this.FindConstructor, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public Module Module;
        public readonly bool IsSingleton;
        public readonly jsduck.Class JsClass;
        public ConfigInterface ConfigInterface;
        public readonly string BaseClass;
        private Lazy<MethodMember> constructor;

        public readonly Dictionary<string, Member> Members = new Dictionary<string, Member>();
        public readonly Dictionary<string, HashSet<Member>> MemberExs = new Dictionary<string, HashSet<Member>>();
        public readonly Dictionary<string, Member> MemberConfigs = new Dictionary<string, Member>();

        public string ClassName
        {
            get
            {
                var pos = this.Name?.LastIndexOf('.');
                return (pos ?? 0) <= 0 ? this.Name : this.Name.Substring(pos.Value + 1);
            }
        }

        public override string SortOrder
        {
            get { return this.ConfigInterface == null ? this.Name : (this.ConfigInterface.Name + "_"); }
        }

        protected override string Signature
        {
            get { return $@"{(this.Module is FileTS ? "declare" : "export")} class {this.ClassName}{((this.IsSingleton || this.BaseClass == null) ? "" : this.Module.FileTS.ClassMap.ContainsKey(this.BaseClass) ? $@" extends {this.Module.FileTS.ClassMap[this.BaseClass].Name}" : $@" /* extends {this.BaseClass} */")}{(this.ConfigInterface == null ? "" : $@" implements {ConfigInterface.Name}")}"; }
        }

        protected override IEnumerable<Base> Children
        {
            get { return this.Members.Values.OrderBy(o => o.SortOrder); }
        }

        public void Load(Dictionary<string, jsduck.Class> jsClassMap)
        {
            if (!this.IsSingleton && this.MemberConfigs.Count > 0)
                this.ConfigInterface = (ConfigInterface)this.Module.Add(new ConfigInterface(this, jsClassMap));
            if (this.constructor.Value != null && !Object.ReferenceEquals(this, this.constructor.Value.Class))
            {
                this.Members.Add(this.constructor.Value.Name, new MethodMember(this.constructor.Value, this));
                this.constructor = new Lazy<MethodMember>(this.FindConstructor, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            }

            // Scan config params from MethodMembers
            if (this.ConfigInterface != null)
            {
                foreach (var method in this.Members.Values.OfType<MethodMember>())
                {
                    var configParam = method.ParamArgs?.FirstOrDefault(p => p.Variable == "config");
                    if (configParam != null)
                    {
                        var tsTypes = configParam.Type.Split('|');
                        if (tsTypes.Contains("any"))
                            configParam.Type = String.Join("|", tsTypes.Where(t => t != "any").Concat(jsduck.Utils.Yield(this.ConfigInterface.Name)));
                    }
                }
            }
        }

        private MethodMember FindConstructor()
        {
            if (this.Members.ContainsKey("constructor"))
                return (MethodMember)this.Members["constructor"];
            var classMap = this.Module.FileTS.ClassMap;
            var baseClass = this;
            for (baseClass = baseClass.BaseClass != null && classMap.ContainsKey(baseClass.BaseClass) ? classMap[baseClass.BaseClass] : null;
                baseClass != null && !baseClass.Members.ContainsKey("constructor");
                baseClass = baseClass.BaseClass != null && classMap.ContainsKey(baseClass.BaseClass) ? classMap[baseClass.BaseClass] : null) ;
            if (baseClass != null && baseClass.Members.ContainsKey("constructor"))
                return (MethodMember)baseClass.Members["constructor"];
            return null;
        }
    }

    sealed class ConfigInterface : Class
    {
        public ConfigInterface(Class clazz, Dictionary<string, jsduck.Class> jsClassMap)
            : base(clazz.Name + "Config", null, null, false, null, jsClassMap)
        {
            this.Class = clazz;
            this.members = this.Class.MemberConfigs.Values.Where(m => !m.JsMember.meta.@private && !m.JsMember.meta.@protected && !m.JsMember.meta.@static).Select(o => new ConfigInterfaceMember((ConfigMember)o, this, jsClassMap)).ToArray();
        }

        public readonly Class Class;

        protected override IEnumerable<Base> Children
        {
            get { return this.members; }
        }
        private readonly ConfigInterfaceMember[] members;

        protected override string Signature
        {
            get {
                var baseClass = this.Class.BaseClass;
                for (; baseClass != null && this.Class.Module.FileTS.ClassMap.ContainsKey(baseClass) && this.Class.Module.FileTS.ClassMap[baseClass].ConfigInterface == null;
                    baseClass = this.Class.Module.FileTS.ClassMap[baseClass].BaseClass) ;
                var baseInterface = baseClass != null && this.Class.Module.FileTS.ClassMap.ContainsKey(baseClass) && this.Class.Module.FileTS.ClassMap[baseClass].ConfigInterface != null ? this.Class.Module.FileTS.ClassMap[baseClass].ConfigInterface : null;
                return $@"interface {this.ClassName}{(baseInterface == null ? "" : $" extends {baseInterface.Name}")}";
            }
        }
    }
}
