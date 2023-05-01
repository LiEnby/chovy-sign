using Avalonia.Controls;
using ChovySign_GUI.Global;
using LibChovy.Config;
using System;

namespace ChovySign_GUI.Settings
{
    public partial class ConfigDropDown : ConfigControl
    {
        internal override void init()
        {
            int? cfgInt = ChovyConfig.CurrentConfig.GetInt(ConfigKey);
            if (cfgInt is null) cfgInt = 0;
            this.configComboBox.SelectedIndex = (int)cfgInt;
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
            init();

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
