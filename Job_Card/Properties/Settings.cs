﻿namespace Job_Card.Properties
{
    using System;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0"), CompilerGenerated]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings) SettingsBase.Synchronized(new Settings()));

        public static Settings Default =>
            defaultInstance;

        [DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
        public string JobCardDatabasePath
        {
            get
            {
                return ((string)this["JobCardDatabasePath"]);
            } 
                
            set
            {
                this["JobCardDatabasePath"] = value;
            }
        }
    }
}

