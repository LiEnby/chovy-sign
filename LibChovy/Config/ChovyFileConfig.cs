using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibChovy.Config
{
    public class ChovyFileConfig : ChovyConfig
    {
        private const char SEPERATOR = ',';
        private static string configFileName = Path.Combine(Directory.GetCurrentDirectory(), Path.ChangeExtension(PRODUCT_NAME, ".cfg"));
        private Dictionary<string, object> config;

        private void saveDict()
        {
            using (StreamWriter cfgWriter = new StreamWriter(new ZlibStream(File.Open(configFileName, FileMode.Create, FileAccess.Write), CompressionMode.Compress)))
            {
                foreach(KeyValuePair<string, object> configOption in config)
                {

                    string[] line = new string[3];
                    line[0] = configOption.Key.Replace(SEPERATOR.ToString(), "").ToLowerInvariant();
                    string? type = null;
                    string? value = null;

                    if (configOption.Value is string) { type = "string"; value = (string)(configOption.Value); }
                    if (configOption.Value is int)    { type = "int";    value = ((int)(configOption.Value)).ToString(); };
                    if (configOption.Value is bool)   { type = "bool";   value = (((bool)(configOption.Value)) ? "1" : "0"); }
                    if (configOption.Value is byte[]) { type = "byte";   value = Convert.ToBase64String(ZlibStream.CompressBuffer((byte[])configOption.Value)); }

                    if (type is null || value is null) continue;

                    line[1] = type;
                    line[2] = value;

                    cfgWriter.WriteLine(String.Join(SEPERATOR, line));

                }
            }
        }
        private void loadDict()
        {
            if (!File.Exists(configFileName))
                saveDict();

            using (StreamReader cfgReader = new StreamReader(new ZlibStream(File.Open(configFileName, FileMode.Open, FileAccess.Read), CompressionMode.Decompress)))
            {
                for(string? line = cfgReader.ReadLine();
                    line is not null;
                    line = cfgReader.ReadLine())
                {
                    line = line.ReplaceLineEndings("");

                    string[] cfg = line.Split(SEPERATOR);
                    if (cfg.Length != 3) continue;

                    string key = cfg[0].Replace(SEPERATOR.ToString(), "").ToLowerInvariant();
                    string type = cfg[1];
                    string value = cfg[2];

                    try
                    {
                        switch (type)
                        {
                            case "int":
                                config[key] = Int32.Parse(value);
                                break;
                            case "string":
                                config[key] = value;
                                break;
                            case "bool":
                                config[key] = Int32.Parse(value) == 1;
                                break;
                            case "byte":
                                config[key] = ZlibStream.UncompressBuffer(Convert.FromBase64String(value));
                                break;
                        }
                    }
                    catch (Exception) { continue; }
                }
            }
        }

        public ChovyFileConfig()
        {
            config = new Dictionary<string, object>();

            loadDict();
        }

        public override bool? GetBool(string key)
        {
            if (!config.ContainsKey(key)) return null;
            return config[key] as bool?;
        }

        public override byte[]? GetBytes(string key)
        {
            if (!config.ContainsKey(key)) return null;
            return config[key] as byte[];
        }

        public override int? GetInt(string key)
        {
            if (!config.ContainsKey(key)) return null;
            return config[key] as int?;
        }

        public override string? GetString(string key)
        {
            if (!config.ContainsKey(key)) return null;
            return config[key] as string;
        }

        public override void SetBool(string key, bool value)
        {
            config[key] = value;
            saveDict();
        }

        public override void SetBytes(string key, byte[] value)
        {
            config[key] = value;
            saveDict();
        }

        public override void SetInt(string key, int value)
        {
            config[key] = value;
            saveDict();

        }

        public override void SetString(string key, string value)
        {
            config[key] = value;
            saveDict();
        }
    }
}
