using Microsoft.Win32;
using System.Globalization;

namespace Vita.ContentManager
{
    public class SettingsReader
    {
        private static string? overrideBackupsFolder = null;

        public static UInt64 AccountId
        {
            get
            {
                string? accountId = getQcmaLastAccount();
                if (accountId is not null) return UInt64.Parse(accountId, NumberStyles.HexNumber);
                accountId = Directory.EnumerateDirectories(AppFolder).FirstOrDefault(o => o.Length == 16);

                if (accountId is not null) return UInt64.Parse(accountId, NumberStyles.HexNumber);
                accountId = Directory.EnumerateDirectories(PspSavedataFolder).FirstOrDefault(o => o.Length == 16);

                if (accountId is not null) return UInt64.Parse(accountId, NumberStyles.HexNumber);
                accountId = Directory.EnumerateDirectories(PspFolder).FirstOrDefault(o => o.Length == 16);

                if (accountId is not null) return UInt64.Parse(accountId, NumberStyles.HexNumber);
                accountId = Directory.EnumerateDirectories(Ps1Folder).FirstOrDefault(o => o.Length == 16);

                if (accountId is not null) return UInt64.Parse(accountId, NumberStyles.HexNumber);
                accountId = Directory.EnumerateDirectories(PsmFolder).FirstOrDefault(o => o.Length == 16);


                return 0ul;
            }
        }
        public static string AppFolder
        {
            get
            {
                return Path.Combine(BackupsFolder, "APP");
            }
        }
        public static string PspSavedataFolder
        {
            get
            {
                return Path.Combine(BackupsFolder, "PSAVEDATA");
            }
        }

        public static string PsmFolder
        {
            get
            {
                return Path.Combine(BackupsFolder, "PSM");
            }
        }
        public static string SystemFolder
        {
            get
            {
                return Path.Combine(BackupsFolder, "SYSTEM");
            }
        }
        public static string Ps1Folder
        {
            get
            {
                return Path.Combine(BackupsFolder, "PSGAME");
            }
        }
        public static string PspFolder
        {
            get
            {
                return Path.Combine(BackupsFolder, "PGAME");
            }
        }
        public static string BackupsFolder
        {
            get
            {
                if (overrideBackupsFolder is not null) return overrideBackupsFolder;

                string? cmaFolder = getQcmaPSVitaFolder();
                if (cmaFolder is not null) return cmaFolder;
                cmaFolder = getDevkitCmaPSVitaFolder();
                if (cmaFolder is not null) return cmaFolder;
                cmaFolder = getSonyCmaPSVitaFolder();
                if (cmaFolder is not null) return cmaFolder;
                return getDefaultCmaPSVitaFolder();
            }
            set
            {
                overrideBackupsFolder = value;
            }
        }

        private static string getDefaultCmaPSVitaFolder()
        {
             return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PS Vita");
        }

        private static string getQcmaConfFile()
        {
            if (OperatingSystem.IsLinux())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "codestation", "qcma.conf");
            else if (OperatingSystem.IsMacOS())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Preferences", "com.codestation.qcma.plist");
            else
                throw new PlatformNotSupportedException("cannot open qcma config as i dont know where it is.");
        }
        private static string? getQcmaConfigSetting(string file, string key)
        {
            if (!File.Exists(file)) return null;

            if (OperatingSystem.IsLinux())
            {
                using (TextReader confFile = File.OpenText(file))
                {
                    for (string? ln = confFile.ReadLine();
                        ln is not null;
                        ln = confFile.ReadLine())
                    {
                        ln = ln.Trim();
                        if (ln.StartsWith("[")) continue;

                        string[] kvp = ln.Split('=');
                        if (kvp.Length < 2) continue;

                        string settingKey = kvp[0].Trim();
                        string settingValue = kvp[1].Trim();


                        if (settingKey == key)
                            return settingValue;
                    }
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                throw new PlatformNotSupportedException("TODO: Implement reading bplist file from mac os");
            }
            return null;
        }

        private static string? getRegistryKey(string registryPath, string keyName)
        {
            if (OperatingSystem.IsWindows())
            {
                using (RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(registryPath))
                {
                    if (regKey is null) return null;
                    string? keyData = (regKey.GetValue(keyName) as string);
                    if (keyData is null) return null;
                    return keyData;
                }
            }
            else
            {
                throw new PlatformNotSupportedException("cannot use registry on os other than windows");
            }
        }

        private static string? getSonyCmaPSVitaFolder()
        {
            if (OperatingSystem.IsWindows())
            {
                return getRegistryKey(@"Software\Sony Corporation\Sony Corporation\Content Manager Assistant\Settings", "ApplicationHomePath");
            }
            return null;
        }
        private static string? getDevkitCmaPSVitaFolder()
        {
            if (OperatingSystem.IsWindows())
            {
                return getRegistryKey(@"Software\SCE\PSP2\Content Manager Assistant for PlayStation(R)Vita DevKit\Settings", "ApplicationHomePath");
            }
            return null;
        }
        private static string? readQcmaConfig(string settingName)
        {
            if (OperatingSystem.IsWindows())
            {
                return getRegistryKey(@"Software\codestation\qcma", settingName);
            }
            else if (OperatingSystem.IsLinux())
            {
                string qcmaConf = getQcmaConfFile();
                return getQcmaConfigSetting(qcmaConf, settingName);
            }
            else if (OperatingSystem.IsMacOS())
            {
                string qcmaConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Preferences", "com.codestation.qcma.plist");
                // TODO: read file
            }
            return null;
        }
        private static string? getQcmaLastAccount()
        {
            return readQcmaConfig("lastAccountId");
        }
        private static string? getQcmaPSVitaFolder()
        {
            return readQcmaConfig("appsPath");
        }
    }
}
