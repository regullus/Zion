﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Coinpayments.Api {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.8.0.0")]
    internal sealed partial class CoinpaymentsSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static CoinpaymentsSettings defaultInstance = ((CoinpaymentsSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new CoinpaymentsSettings())));
        
        public static CoinpaymentsSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5cac858aaa00d23fef49d5de31393db228728f0976374e7fcf1e6f1ebdf703f9")]
        public string PublicKey {
            get {
                return ((string)(this["PublicKey"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3c6F0A65Bf91b3dDdAf7eC1dF6fd753a3B3834a4EA379Cb02EB1a65263286E38")]
        public string PrivateKey {
            get {
                return ((string)(this["PrivateKey"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6c5898f12a80f3b84a4e9151d0841b74")]
        public string MerchantId {
            get {
                return ((string)(this["MerchantId"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D38sksfj1236251A")]
        public string IpnSecret {
            get {
                return ((string)(this["IpnSecret"]));
            }
        }
    }
}