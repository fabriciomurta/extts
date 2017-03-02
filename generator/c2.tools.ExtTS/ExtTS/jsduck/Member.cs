using System;
using System.Collections.Generic;

namespace ExtTS.jsduck
{
    [Serializable]
    public sealed class Member
    {
        #region Properties

        public string tagname;
        public string name;
        public string owner;
        public string id;
        public MemberMeta meta;

        public override string ToString()
        {
            return $@"{(tagname == "method" ? "" : ($@"[{tagname}] "))}{(meta.@static ? "static " : "")}{(meta.@private ? "private " : "")}{(meta.@protected ? "protected " : "")}{name}{(tagname == "method" ? "()" : "")}";
        }

        #endregion

        public Class Container;
        public Class Owner;

        public void Initialize(Dictionary<string, Class> classMap, Class container)
        {
            this.Container = container;
            this.Owner = !classMap.ContainsKey(this.owner) ? null : classMap[this.owner];
        }
    }

    #region Member classes

    [Serializable]
    public sealed class MemberMeta
    {
        public MemberMetaDeprecated deprecated;
        public bool @private;
        public bool @protected;
        public bool @static;
    }

    [Serializable]
    public sealed class MemberMetaDeprecated
    {
        public string text;
        public string version;
    }

    [Serializable]
    public sealed class MemberOverride
    {
        public string name;
        public string owner;
    }

    [Serializable]
    public sealed class Param
    {
        public string tagname;
        public string name;
        public string type;
        public bool optional;
        public Param[] properties;
    }

    #endregion
}
