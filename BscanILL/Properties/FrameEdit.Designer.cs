﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BscanILL.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class FrameEdit : global::System.Configuration.ApplicationSettingsBase {
        
        private static FrameEdit defaultInstance = ((FrameEdit)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new FrameEdit())));
        
        public static FrameEdit Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsContentChecked {
            get {
                return ((bool)(this["IsContentChecked"]));
            }
            set {
                this["IsContentChecked"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsDeskewChecked {
            get {
                return ((bool)(this["IsDeskewChecked"]));
            }
            set {
                this["IsDeskewChecked"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsBookfoldChecked {
            get {
                return ((bool)(this["IsBookfoldChecked"]));
            }
            set {
                this["IsBookfoldChecked"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsFingersChecked {
            get {
                return ((bool)(this["IsFingersChecked"]));
            }
            set {
                this["IsFingersChecked"] = value;
            }
        }
    }
}
