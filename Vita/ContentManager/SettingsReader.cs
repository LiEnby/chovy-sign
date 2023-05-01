using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vita.ContentManager
{
    public class SettingsReader
    {
        private static string? overrideBackupsFolder = null;
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
        private static string? getQcmaPSVitaFolder()
        {
            if (OperatingSystem.IsWindows())
            {
                using(RegistryKey? qcmaKey = Registry.CurrentUser.OpenSubKey(@"Software\codestation\qcma"))
                {
                    return getRegistryKey(@"Software\codestation\qcma", "appsPath");
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                string qcmaConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "codestation", "qcma.conf");
                // TODO: read file
            }
            else if (OperatingSystem.IsMacOS())
            {
                string qcmaConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Preferences", "com.codestation.qcma.plist");
                // TODO: read file
            }
            return null;
        }
    }
}
