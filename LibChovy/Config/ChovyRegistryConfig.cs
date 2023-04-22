using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable CA1416 // Validate platform compatibility
// platform is checked in constructor, however .net seems to wants me to check again in every function, No.

namespace LibChovy.Config
{
    public class ChovyRegistryConfig : ChovyConfig
    {
        private static string chovyKeyPath = Path.Combine("Software", PRODUCT_FAMILY, PRODUCT_NAME);
        private RegistryKey chovyRegistryKey;
        public ChovyRegistryConfig()
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Cannot use ChovyRegistryConfig on OS other than windows.");

            chovyRegistryKey = Registry.CurrentUser.CreateSubKey(chovyKeyPath, true);
        }

        public override bool? GetBool(string key)
        {
            int? v = chovyRegistryKey.GetValue(key) as int?;
            if (v is null) return null;

            if (v == 1) return true;
            if (v == 0) return false;
            
            return null;
        }

        public override byte[]? GetBytes(string key)
        {
            return (chovyRegistryKey.GetValue(key) as byte[]);
        }

        public override int? GetInt(string key)
        {
            return (chovyRegistryKey.GetValue(key) as int?);
        }

        public override string? GetString(string key)
        {
            return (chovyRegistryKey.GetValue(key) as string);
        }

        public override void SetBool(string key, bool value)
        {
            chovyRegistryKey.SetValue(key, value ? 1 : 0, RegistryValueKind.DWord);
        }

        public override void SetBytes(string key, byte[] value)
        {
            chovyRegistryKey.SetValue(key, value, RegistryValueKind.Binary);
        }

        public override void SetInt(string key, int value)
        {
            chovyRegistryKey.SetValue(key, value, RegistryValueKind.DWord);
        }

        public override void SetString(string key, string value)
        {
            chovyRegistryKey.SetValue(key, value, RegistryValueKind.String);
        }
    }
}

#pragma warning restore CA1416 // Validate platform compatibility
