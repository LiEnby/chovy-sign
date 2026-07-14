using Avalonia.Controls;

namespace ChovySign_GUI.Settings
{
    public abstract class ConfigControl : UserControl
    {
        private string? configKey = null;
        internal bool loaded = false;

        public string ConfigKey
        {
            get
            {
                if (configKey is null) return "";
                return configKey;
            }
            set
            {
                configKey = value;

                init();
            }
        }

        internal abstract void init();
    }
}
