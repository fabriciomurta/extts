using System.Collections.Generic;

namespace ExtTS.model
{
    sealed class EventMember : MethodMember
    {
        public new const string TAG = "event";

        public EventMember(EventMember member, Class cls)
            : base(member, cls)
        {
        }

        public EventMember(string name, string[] comments, Dictionary<string, HashSet<string>> @params, Class cls, jsduck.Member jsMember, Dictionary<string, jsduck.Class> jsClassMap)
            : base(name, comments, @params, cls, jsMember, jsClassMap)
        {
        }

        public override string Type { get { return TAG; } }
        protected override int TypeOrder { get { return 3; } }
    }
}
