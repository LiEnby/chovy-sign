using Avalonia.Controls;
using ChovySign_GUI.Global;
using LibChovy.Config;
using System;

namespace ChovySign_GUI.Settings
{
    public partial class ConfigText : ConfigControl
    {
        private string defaultSetting = "";
        internal override void init()
        {
            loaded = false;

            string? cfgText = ChovyConfig.CurrentConfig.GetString(ConfigKey);
            if (cfgText is null) cfgText = defaultSetting;
            if (cfgText is not null) loaded = true;

            configText.Text = cfgText;
        }


        public String Default
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
        public string Label
        {
            get
            {
                return configText.Label;
            }
            set
            {
                configText.Label = value;
            }
        }
        public string Value
        {
            get
            {
                if (this.configText.Text is null) return "";
                return (string)this.configText.Text;
            }
            set
            {
                this.configText.Text = value;

                ChovyConfig.CurrentConfig.SetString(ConfigKey, value);
            }
        }
        public ConfigText()
        {
            InitializeComponent();
            this.configText.TextChanged += onTextChange;
        }

        private void onTextChange(object? sender, EventArgs e)
        {
            LabeledTextBox? txtBox = sender as LabeledTextBox;
            if (txtBox is null) return;
            if (txtBox.Text is null) return;

            ChovyConfig.CurrentConfig.SetString(ConfigKey, txtBox.Text);
        }
    }
}
