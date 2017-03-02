using System;
using System.Collections.Generic;

namespace ExtTS.model
{
    sealed class FileTS : Module
    {
        public FileTS(string extVersion, string appVersion)
            : base(null, new string[] {
                $"/*",
                $"Premium TypeScript type definitions for Sencha Ext JS",
                $"ExtTS for ExtJS {extVersion}",
                $"",
                $"Copyright (C) 2015-2016 ExtFX.NET",
                $"Contact: thanhptr@gmail.com",
                $"",
                $"Build date: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")} (GMT)",
                $"Version: {appVersion}",
                $"",
                $"*/",
                $"",
            }, null, null)
        {
        }

        public readonly Dictionary<string, model.Class> ClassMap = new Dictionary<string, Class>();
        public readonly Dictionary<string, model.Module> ModuleMap = new Dictionary<string, Module>();

        protected override string Signature
        {
            get { return null; }
        }
    }
}
