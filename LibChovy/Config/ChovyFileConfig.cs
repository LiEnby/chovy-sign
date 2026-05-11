using Ionic.Zlib;

namespace LibChovy.Config
{
    public class ChovyFileConfig : ChovyConfig
    {
        private const char SEPERATOR = ',';
        private static string configFileName = Path.Combine(Directory.GetCurrentDirectory(), Path.ChangeExtension(PRODUCT_NAME, ".cfg"));
        private Dictionary<string, object> config;

        private void saveDict()
        {
            using (StreamWriter cfgWriter = new StreamWriter(File.Open(configFileName, FileMode.Create, FileAccess.Write)))
            {
                foreach (KeyValuePair<string, object> configOption in config)
                {

                    string[] line = new string[3];
                    line[0] = configOption.Key.Replace(SEPERATOR.ToString(), "").ToUpperInvariant();
                    string? type = null;
                    string? value = null;

                    if (configOption.Value is string) { type = "string"; value = (string)(configOption.Value); }
                    if (configOption.Value is int) { type = "int"; value = ((int)(configOption.Value)).ToString(); }
                    ;
                    if (configOption.Value is UInt64) { type = "uint64"; value = ((UInt64)(configOption.Value)).ToString(); }
                    ;
                    if (configOption.Value is bool) { type = "bool"; value = (((bool)(configOption.Value)) ? "1" : "0"); }
                    if (configOption.Value is byte[]) { type = "byte"; value = Convert.ToBase64String(ZlibStream.CompressBuffer((byte[])configOption.Value)); }

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

            using (StreamReader cfgReader = new StreamReader(File.Open(configFileName, FileMode.Open, FileAccess.Read)))
            {
                for(string? line = cfgReader.ReadLine();
                    line is not null;
                    line = cfgReader.ReadLine())
                {
                    line = line.ReplaceLineEndings("");

                    string[] cfg = line.Split(SEPERATOR);
                    if (cfg.Length != 3) continue;

                    string key = cfg[0].Replace(SEPERATOR.ToString(), "").ToUpperInvariant();
                    string type = cfg[1];
                    string value = cfg[2];

                    try
                    {
                        switch (type)
                        {
                            case "int":
                                config[key] = Int32.Parse(value);
                                break;
                            case "uint64":
                                config[key] = UInt64.Parse(value);
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
            if (!config.ContainsKey(key.ToUpperInvariant())) return null;
            return config[key.ToUpperInvariant()] as bool?;
        }

        public override byte[]? GetBytes(string key)
        {
            if (!config.ContainsKey(key.ToUpperInvariant())) return null;
            return config[key.ToUpperInvariant()] as byte[];
        }

        public override int? GetInt(string key)
        {
            if (!config.ContainsKey(key.ToUpperInvariant())) return null;
            return config[key.ToUpperInvariant()] as int?;
        }
        public override UInt64? GetInt64(string key)
        {
            if (!config.ContainsKey(key.ToUpperInvariant())) return null;
            return config[key.ToUpperInvariant()] as UInt64?;
        }
        public override string? GetString(string key)
        {
            if (!config.ContainsKey(key.ToUpperInvariant())) return null;
            return config[key.ToUpperInvariant()] as string;
        }

        public override void SetBool(string key, bool value)
        {
            config[key.ToUpperInvariant()] = value;
            saveDict();
        }

        public override void SetBytes(string key, byte[] value)
        {
            config[key.ToUpperInvariant()] = value;
            saveDict();
        }

        public override void SetInt(string key, int value)
        {
            config[key.ToUpperInvariant()] = value;
            saveDict();

        }

        public override void SetInt64(string key, UInt64 value)
        {
            config[key.ToUpperInvariant()] = value;
            saveDict();

        }

        public override void SetString(string key, string value)
        {
            config[key.ToUpperInvariant()] = value;
            saveDict();
        }
    }
}
