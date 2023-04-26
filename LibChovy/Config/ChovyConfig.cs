using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy.Config
{
    public abstract class ChovyConfig
    {
        public const string PRODUCT_NAME = "Chovy-Sign2";
        public const string PRODUCT_FAMILY = "CHOVYProject";

        public virtual object? this[string index] 
        { 
            get
            {
                string? str = GetString(index);
                if (str is not null) return str;
                
                int? v = GetInt(index);
                if (v is not null) return v;

                byte[]? byt = GetBytes(index);
                if (byt is not null) return byt;

                bool? b = GetBool(index);
                if (b is not null) return b;

                return null;
            }
            set
            {
                if (value is string) SetString(index, (string)value);
                if (value is int) SetInt(index, (int)value);
                if (value is byte[]) SetBytes(index, (byte[])value);
                if (value is bool) SetBool(index, (bool)value);
                // idk
            }
        }


        public abstract string? GetString(string key);
        public abstract bool? GetBool(string key);
        public abstract int? GetInt(string key);
        public abstract byte[]? GetBytes(string key);

        public abstract void SetString(string key, string value);
        public abstract void SetBool(string key, bool value);
        public abstract void SetInt(string key, int value);
        public abstract void SetBytes(string key, byte[] value);

        private static ChovyConfig? config = null;
        public static ChovyConfig CurrentConfig
        {
            get
            {
                if(config is null)
                {
                    if (OperatingSystem.IsWindows()) config = new ChovyRegistryConfig();
                    else config = new ChovyFileConfig();

                    return config;
                }
                else
                {
                    return config;
                }
            }
        }

    }
}
