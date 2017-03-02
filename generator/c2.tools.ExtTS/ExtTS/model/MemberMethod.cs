using ExtTS.jsduck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtTS.model
{
    class MethodMember : Member
    {
        public const string TAG = "method";

        public MethodMember(MethodMember member, Class cls)
            : base(member, cls)
        {
            this.returnTypeOptional = member.returnTypeOptional;
            this.returnType = member.returnType;
            this.ParamArgs = member.ParamArgs;
        }

        public MethodMember(string name, string[] comments, Dictionary<string, HashSet<string>> @params, Class cls, jsduck.Member jsMember, Dictionary<string, jsduck.Class> jsClassMap)
            : base(name, comments, @params, cls, jsMember, jsClassMap)
        {
            if (!this.Params.ContainsKey("return"))
                this.returnType = null;
            else
            {
                if (this.Params["return"].Count != 1)
                    Console.WriteLine($@"Warning: multiple JsDoc return for: {this.Class.Name}.{this.Name}");
                var returnParamArgs = JsDoc.ExtractCommentParamArgs(this.Params["return"].FirstOrDefault(), jsClassMap);
                this.returnType = returnParamArgs == null ? null : returnParamArgs.Type;
                this.returnTypeOptional = returnParamArgs == null ? false : returnParamArgs.IsOptional;
            }

            if (this.Params.ContainsKey("param"))
            {
                this.ParamArgs = this.Params["param"].Select(p => JsDoc.ExtractCommentParamArgs(p, jsClassMap)).Where(p => p != null).ToArray();
                var isOptional = false;
                for (var i = 0; i < this.ParamArgs.Length; i++)
                {
                    if (!isOptional)
                    {
                        if (this.ParamArgs[i].IsOptional)
                            isOptional = true;
                    }
                    else
                    {
                        if (!this.ParamArgs[i].IsOptional)
                            this.ParamArgs[i].IsOptional = true;
                    }
                }

                var restIndex = this.ParamArgs.Length - 1;
                for (; restIndex >= 0 && !this.ParamArgs[restIndex].IsRest; restIndex--);
                for (; restIndex >= 0; restIndex--)
                    if (this.ParamArgs[restIndex].IsRest)
                        this.ParamArgs[restIndex].IsRest = false;
            }
        }

        private readonly bool returnTypeOptional;
        private readonly string returnType;
        public readonly JsDoc.CommentParamArg[] ParamArgs;

        public override string Type { get { return TAG; } }
        protected override int TypeOrder { get { return 2; } }

        public override bool Generatable
        {
            get
            {
                if (this.Name == "constructor")
                    return !this.IsStatic;
                return base.Generatable;
            }
        }

        protected override string Signature
        {
            get
            {
                if (this.signature == null)
                {
                    var parameters = this.ParamArgs == null ? String.Empty : String.Join<JsDoc.CommentParamArg>(", ", this.ParamArgs);
                    this.signature = $"{(this.JsMember.meta.@private ? "private " : "")}{(this.JsMember.meta.@protected ? "protected " : "")}{((IsStatic && this.Name != "constructor") ? "static " : "")}{(Name.Length <= 0 ? "_" : Name)}({parameters}){(this.Name == "constructor" ? "" : this.returnType == null ? "" : $": {this.returnType}")};";
                }
                return this.signature;
            }
        }
        private string signature;
    }
}
