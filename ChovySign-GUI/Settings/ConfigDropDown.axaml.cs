using Avalonia.Controls;
using ChovySign_GUI.Global;
using LibChovy.Config;
using System;

namespace ChovySign_GUI.Settings
{
    public partial class ConfigDropDown : ConfigControl
    {
        private int defaultSetting = 0;
        internal override void init()
        {
            loaded = false;

            int? cfgInt = ChovyConfig.CurrentConfig.GetInt(ConfigKey);
            if (cfgInt is not null) loaded = true;
            if (cfgInt is null) cfgInt = defaultSetting;
            this.configComboBox.SelectedIndex = (int)cfgInt;
        }
        public int Default
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
                    this.SelectedIndex = defaultSetting;
            }
        }
        public string Label
        {
            get
            {
                return this.configComboBox.Label;
            }
            set
            {
                this.configComboBox.Label = value;
            }
        }
        public int SelectedIndex
        {
            get
            {
                return this.configComboBox.SelectedIndex;
            }
            set
            {
                this.configComboBox.SelectedIndex = value;
                if (this.configComboBox.SelectedIndex < 0) return;

                ChovyConfig.CurrentConfig.SetInt(ConfigKey, this.configComboBox.SelectedIndex);
            }
        }

        public string[] Items
        {
            get
            {
                return this.configComboBox.Items;
            }
            set
            {
                this.configComboBox.Items = value;
                init();
            }
        }
        public ConfigDropDown()
        {
            InitializeComponent();
            this.configComboBox.SelectionChanged += onSelectionChange;
        }

        private void onSelectionChange(object? sender, EventArgs e)
        {
            LabeledComboBox? comboBox = sender as LabeledComboBox;
            if (comboBox is null) return;
            if (comboBox.SelectedIndex < 0) return;

            ChovyConfig.CurrentConfig.SetInt(ConfigKey, comboBox.SelectedIndex);
        }
    }
}
