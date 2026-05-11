using Microsoft.Win32;
using System.Globalization;

namespace Vita.ContentManager
{
    public class SettingsReader
    {
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

        public static string GetPs1Folder(string backupsFolder)
        {
            return Path.Combine(backupsFolder, "PSGAME");
        }
        public static string GetPspFolder(string backupsFolder)
        {
            return Path.Combine(backupsFolder, "PGAME");
        }
        public static string GetAppFolder(string backupsFolder)
        {
            return Path.Combine(backupsFolder, "APP");
        }
        public static string GetPsmFolder(string backupsFolder)
        {
            return Path.Combine(backupsFolder, "PSM");
        }
        public static string GetSystemFolder(string backupsFolder)
        {
            return Path.Combine(backupsFolder, "SYSTEM");
        }
        public static string GetPspSaveFolder(string backupsFolder)
        {
            return Path.Combine(backupsFolder, "PSAVEDATA");
        }

        public static string AppFolder
        {
            get
            {
                return GetAppFolder(BackupsFolder);
            }
        }
        public static string PspSavedataFolder
        {
            get
            {
                return GetPspSaveFolder(BackupsFolder);
            }
        }

        public static string PsmFolder
        {
            get
            {
                return GetPsmFolder(BackupsFolder);
            }
        }
        public static string SystemFolder
        {
            get
            {
                return GetSystemFolder(BackupsFolder);
            }
        }


        public static string Ps1Folder
        {
            get
            {
                return GetPs1Folder(BackupsFolder);
            }
        }
        public static string PspFolder
        {
            get
            {
                return GetPspFolder(BackupsFolder);
            }
        }
        public static string BackupsFolder
        {
            get
            {
                string? cmaFolder = getQcmaPSVitaFolder();
                if (cmaFolder is not null) return cmaFolder;
                cmaFolder = getDevkitCmaPSVitaFolder();
                if (cmaFolder is not null) return cmaFolder;
                cmaFolder = getSonyCmaPSVitaFolder();
                if (cmaFolder is not null) return cmaFolder;
                return getDefaultCmaPSVitaFolder();
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
