using System.Collections.Generic;

namespace ExtTS.model
{
    class ConfigMember : PropertyMember
    {
        public new const string TAG = "cfg";

        public ConfigMember(ConfigMember member, Class cls)
            : base(member, cls)
        {
        }

        public ConfigMember(string name, string[] comments, Dictionary<string, HashSet<string>> @params, Class cls, jsduck.Member jsMember, Dictionary<string, jsduck.Class> jsClassMap)
            : base(name, comments, @params, cls, jsMember, jsClassMap)
        {
        }

        public override string Type { get { return TAG; } }
        protected override int TypeOrder { get { return 1; } }
    }

    sealed class ConfigInterfaceMember : ConfigMember
    {
        public ConfigInterfaceMember(ConfigMember configMember, ConfigInterface configInterface, Dictionary<string, jsduck.Class> jsClassMap)
            : base(configMember.Name, configMember.comments, configMember.Params, configInterface, configMember.JsMember, jsClassMap)
        {
            this.configMember = configMember;
        }

        private readonly ConfigMember configMember;

        protected override bool IsPropertyOptional { get { return true; } }

        public override bool Generatable
        {
            get { return this.configMember.Generatable && (this.configMember.Class.Members[this.Name].Type == TAG); }
        }
    }
}
