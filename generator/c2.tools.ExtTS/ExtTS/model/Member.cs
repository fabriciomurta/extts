using System;
using System.Collections.Generic;

namespace ExtTS.model
{
    abstract class Member : Base
    {
        protected Member(Member member, Class cls)
            : base(member)
        {
            this.Class = cls;
            this.JsMember = member.JsMember;
            this.IsStatic = member.IsStatic;
            this.InheritMember = new Lazy<Member>(this.FindInheritMember, false);
        }

        protected Member(string name, string[] comments, Dictionary<string, HashSet<string>> @params, Class cls, jsduck.Member jsMember, Dictionary<string, jsduck.Class> jsClassMap)
            : base(name, comments, @params, jsClassMap)
        {
            this.Class = cls;
            this.JsMember = jsMember;
            this.IsStatic = this.Class.IsSingleton || this.JsMember.meta.@static;
            this.InheritMember = new Lazy<Member>(this.FindInheritMember, false);
        }

        public abstract string Type { get; }
        protected abstract int TypeOrder { get; }
        public readonly Class Class;
        public readonly jsduck.Member JsMember;
        public readonly bool IsStatic;
        public readonly Lazy<Member> InheritMember;
        public override string SortOrder { get { return $@"{(this.IsStatic ? 1 : 0)}{this.TypeOrder}{this.Name}"; } }
        protected override IEnumerable<Base> Children { get { return null; } }

        public override string[] Comments
        {
            get
            {
                var baseComments = base.Comments;
                if (baseComments.Length <= 0 && this.Params.ContainsKey("inheritdoc") && this.InheritMember.Value != null)
                    return this.InheritMember.Value.Comments;
                return baseComments;
            }
        }

        public override bool Generatable
        {
            get
            {
                if (this.InheritMember.Value != null)
                {
                    var member = this.InheritMember.Value;
                    for (; member.InheritMember.Value != null; member = member.InheritMember.Value) ;
                    if (this.Signature != member.Signature)
                    {
                        Console.WriteLine($@"Ignore inheritence member: '{this.Class.Name}.{this.Signature}' => '{member.Class.Name}.{member.Signature}'");
                        return false;
                    }
                }

                return base.Generatable;
            }
        }

        private Member FindInheritMember()
        {
            var classMap = this.Class.Module.FileTS.ClassMap;
            var baseClass = this.Class;
            for (baseClass = baseClass.BaseClass != null && classMap.ContainsKey(baseClass.BaseClass) ? classMap[baseClass.BaseClass] : null;
                baseClass != null && !baseClass.Members.ContainsKey(this.Name);
                baseClass = baseClass.BaseClass != null && classMap.ContainsKey(baseClass.BaseClass) ? classMap[baseClass.BaseClass] : null) ;
            if (baseClass != null && baseClass.Members.ContainsKey(this.Name))
                return baseClass.Members[this.Name];
            return null;
        }
    }
}
