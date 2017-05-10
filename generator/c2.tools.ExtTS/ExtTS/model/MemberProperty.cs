using ExtTS.jsduck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtTS.model
{
    class PropertyMember : Member
    {
        public const string TAG = "property";

        public PropertyMember(PropertyMember member, Class cls)
            : base(member, cls)
        {
            this.dataType = member.dataType;
            this.DefaultValue = member.DefaultValue;
            this.IsOptional = member.IsOptional;
            this.IsRest = member.IsRest;
        }

        public PropertyMember(string name, string[] comments, Dictionary<string, HashSet<string>> @params, Class cls, jsduck.Member jsMember, Dictionary<string, jsduck.Class> jsClassMap)
            : base(name, comments, @params, cls, jsMember, jsClassMap)
        {
            JsDoc.CommentParamArg commentParamArgs;

            if (!this.Params.ContainsKey(this.Type))
                commentParamArgs = null;
            else
            {
                if (this.Params[this.Type].Count != 1)
                    Console.WriteLine($@"Warning: multiple JsDoc properties for: {this.Class.Name}.{this.Name}");
                commentParamArgs = JsDoc.ExtractCommentParamArgs(this.Params[this.Type].FirstOrDefault(), jsClassMap);
            }

            if (commentParamArgs != null)
            {
                this.IsRest = false;
                this.dataType = commentParamArgs.Type;
                this.IsOptional = commentParamArgs.IsOptional;
                this.DefaultValue = commentParamArgs.DefaultValue == null ? String.Empty : commentParamArgs.DefaultValue;
            }
            else
            {
                this.IsRest = false;
                this.IsOptional = false;
                this.dataType = !this.Params.ContainsKey("type") || this.Params["type"].Count <= 0 || String.IsNullOrEmpty(this.Params["type"].FirstOrDefault()) ? null : JsDoc.ExtTypeToTS(this.Params["type"].FirstOrDefault(), ref this.IsOptional, ref this.IsRest, jsClassMap);
                this.DefaultValue = String.Empty;
            }
        }

        private readonly string dataType;
        public readonly string DefaultValue;
        public readonly bool IsOptional;
        public readonly bool IsRest;

        public override string Type { get { return TAG; } }
        protected override int TypeOrder { get { return 0; } }
        protected virtual bool IsPropertyOptional { get { return false; } }

        public string DataType
        {
            get { return this.dataType ?? ((this.InheritMember.Value as PropertyMember)?.DataType ?? "any"); }
        }

        public override string[] Comments
        {
            get
            {
                var baseComments = base.Comments;
                if (this.IsOptional || this.DefaultValue.Length > 0)
                {
                    var commentLine = $"{(this.IsOptional ? " * Optional" : " * ")}{(this.DefaultValue.Length <= 0 ? "" : this.IsOptional ? $", Defaults to: {this.DefaultValue}" : $" Defaults to: {this.DefaultValue}")}";
                    if (baseComments.Length <= 0)
                        return new string[] { "/**", commentLine, " */" };
                    else
                        return baseComments.Take(baseComments.Length - 1).Concat(Utils.Yield(" *", commentLine)).Concat(baseComments.Skip(baseComments.Length - 1)).ToArray();
                }
                return baseComments;
            }
        }

        protected override string Signature
        {
            get { return $"{(this.JsMember.meta.@private ? "private " : "")}{(this.JsMember.meta.@protected ? "protected " : "")}{(IsStatic ? "static " : "")}{(JsDoc.MemberNameValidator.IsMatch(Name) ? Name : $"'{Name}'")}{(this.IsPropertyOptional ? "?" : "")}: {DataType}{(IsRest ? "[]" : "")};"; }
        }

        //var listSources = new List<string>();
        //if (comment != null && comment.Length > 0)
        //    JsDoc.AddComments(listSources, comment);
        //listSources.Add(staticStr + this.name + (optional ? "?: " : ": ") + type + defaultValue + ';');
        //listSources.Add(String.Empty);
        //sourcecode = listSources.ToArray();

        //#region Generations

        //// Whether the visibility rules say we should emit this member
        //public bool IsVisible
        //{
        //    get { return this.meta.@protected ? (!this.Owner.singleton && !this.meta.@static) : !this.meta.@private; }
        //}

        //static Member lookupMember(jsduck.Member[] members, string name, HashSet<string> tagnames, bool? @static = null)
        //{
        //    for (var i = 0; i < members.Length; i++)
        //    {
        //        var member = members[i];
        //        var tagMatch = tagnames.Contains(member.tagname);
        //        var staticMatch = @static == null || member.meta.@static == @static;

        //        if (member.name == name && tagMatch && staticMatch)
        //        {
        //            return member;
        //        }
        //    }
        //    return null;
        //}

        //#endregion

    }
}
