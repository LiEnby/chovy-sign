using ChovySign_GUI.Global;
using LibChovy.Config;
using System;

namespace ChovySign_GUI.Settings
{
    public partial class ConfigU64 : ConfigControl
    {
        private UInt64 defaultSetting = 0;
        internal override void init()
        {
            loaded = false;
            UInt64? cfgValue = ChovyConfig.CurrentConfig.GetInt64(ConfigKey);
            if (cfgValue is not null) loaded = true;
            if (cfgValue is null) cfgValue = 0ul;
            configU64.Text = ((UInt64)cfgValue).ToString("X16");

        }
        public string Label
        {
            get
            {
                return configU64.Label;
            }
            set
            {
                configU64.Label = value;
            }
        }

        public UInt64 Default
        {
            get
            {
                return defaultSetting;
            }
            set
            {
                defaultSetting = value;
                init();

                if (!loaded)
                    this.Value = defaultSetting;
            }
        }
        public UInt64 Value
        {
            get
            {
                UInt64 output;
                if (!UInt64.TryParse(this.configU64.Text, out output)) return 0ul;

                return output;
            }
            set
            {
                this.configU64.Text = value.ToString("X16");

                ChovyConfig.CurrentConfig.SetInt64(ConfigKey, value);
            }
        }
        public ConfigU64()
        {
            InitializeComponent();
            this.configU64.TextChanged += onTextChange;
        }

        private void onTextChange(object? sender, EventArgs e)
        {
            UInt64 output;
            LabeledTextBox? txtBox = sender as LabeledTextBox;
            if (txtBox is null) return;
            if (txtBox.Text is null) return;
            if (!UInt64.TryParse(txtBox.Text, out output)) return;

            ChovyConfig.CurrentConfig.SetInt64(ConfigKey, output);
        }
    }
}
