using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Popup.Global;
using LibChovy.Config;
using System;
using System.Collections.Generic;
using static ChovySign_GUI.Popup.Global.MessageBox;

namespace ChovySign_GUI.Settings
{
    public partial class ConfigToggle : ConfigControl
    {

        internal bool disableEvents = false;
        private string? promptMsg = null;
        private bool defaultSetting = false;
        internal override void init()
        {
            bool? isToggleChecked = ChovyConfig.CurrentConfig.GetBool(ConfigKey);
            if(isToggleChecked is null) isToggleChecked = defaultSetting;

            this.disableEvents = true;
            configCheckbox.IsChecked = isToggleChecked;
            this.disableEvents = false;
        }

        public bool Default
        {
            get
            {
                return defaultSetting;
            }
            set
            {
                defaultSetting = value;

                if (ChovyConfig.CurrentConfig.GetBool(ConfigKey) is null)
                    this.IsToggled = defaultSetting;
            }
        }
        public string Label
        {
            get
            {
                string? content = configCheckbox.Content as string;
                if (content is null) return "";
                return content;
            }
            set
            {
                configCheckbox.Content = value;
            }
        }

        public string Prompt
        {
            get
            {
                if (promptMsg is null) return "";
                return promptMsg;
            }
            set
            {
                promptMsg = value;
            }
        }
        private async void onToggleChecked(object? sender, RoutedEventArgs e)
        {
            if (disableEvents) return;

            CheckBox? checkBox = sender as CheckBox;
            if (checkBox is null) return;

            bool? toggled = checkBox.IsChecked;
            if (toggled is null) toggled = false;

            if (promptMsg is not null)
            {
                Window? currentWindow = this.VisualRoot as Window;
                if (currentWindow is not Window) throw new Exception("could not find current window");

                MessageBoxResult res = await MessageBox.Show(currentWindow, Prompt, "Are you sure?", MessageBoxButtons.YesNo);
                if (res == MessageBoxResult.Yes)
                    IsToggled = true;
                else
                    IsToggled = false;
            }
			else{
				IsToggled = true;
			}

        }

        private void onToggleUnchecked(object? sender, RoutedEventArgs e)
        {
            if (disableEvents) return;

            CheckBox? checkBox = sender as CheckBox;
            if (checkBox is null) return;

            bool? toggle = checkBox.IsChecked;
            if (toggle is null) toggle = false;
            IsToggled = (bool)toggle;
        }

        public bool IsToggled
        {
            get
            {
                if (this.configCheckbox.IsChecked is null) return false;
                return (bool)this.configCheckbox.IsChecked;
            }
            set
            {
                this.disableEvents = true;
                this.configCheckbox.IsChecked = value;
                this.disableEvents = false;

                ChovyConfig.CurrentConfig.SetBool(ConfigKey, value);
            }
        }

        public ConfigToggle()
        {
            InitializeComponent();
            init();

            configCheckbox.Unchecked += onToggleUnchecked;
            configCheckbox.Checked += onToggleChecked;

        }
    }
}
