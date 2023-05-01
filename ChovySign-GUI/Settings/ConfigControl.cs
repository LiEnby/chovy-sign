using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChovySign_GUI.Settings
{
    public abstract class ConfigControl : UserControl
    {
        private string? configKey = null;
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
