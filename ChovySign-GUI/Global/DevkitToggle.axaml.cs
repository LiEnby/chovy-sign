using Avalonia.Controls;
using Avalonia.Interactivity;
using ChovySign_GUI.Popup.Global;
using LibChovy.Config;
using System;
using System.Collections.Generic;
using static ChovySign_GUI.Popup.Global.MessageBox;

namespace ChovySign_GUI.Global
{
    public partial class DevkitToggle : UserControl
    {
        private static List<DevkitToggle> instances = new List<DevkitToggle>();

        private const string useDevkitModeConfigKey = "USE_DEVKIT_ACCOUNT_ID";
        internal bool disableEvents = false;
        private async void onDevkitModeChecked(object? sender, RoutedEventArgs e)
        {
            if (disableEvents) return;

            CheckBox? checkBox = sender as CheckBox;
            if (checkBox is null) return;

            bool? devMode = checkBox.IsChecked;
            if (devMode is null) devMode = false;

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            MessageBoxResult res = await MessageBox.Show(currentWindow, "This option will force the CMA Account ID to be all 0's\nWhich is how it is on Devkit, Testkit and IDU Firmware\nEnabling this if you have a retail firmware will result in the games just *not* showing up\n\nIf you DON'T know what this means, DON'T enable this.\ndo you want to continue?", "Are you sure?", MessageBoxButtons.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                IsDevkitMode = true;
            }
            else
            {
                IsDevkitMode = false;
            }

        }

        private void onDevkitModeUnchecked(object? sender, RoutedEventArgs e)
        {
            if (disableEvents) return;

            CheckBox? checkBox = sender as CheckBox;
            if (checkBox is null) return;

            bool? devMode = checkBox.IsChecked;
            if (devMode is null) devMode = false;
            IsDevkitMode = (bool)devMode;
        }

        public bool IsDevkitMode
        {
            get
            {
                if (this.devkitCheckbox.IsChecked is null) return false;
                return (bool)this.devkitCheckbox.IsChecked;
            }
            set
            {
                foreach (DevkitToggle instance in instances)
                {
                    instance.disableEvents = true;
                    instance.devkitCheckbox.IsChecked = value;
                    instance.disableEvents = false;
                }

                ChovyConfig.CurrentConfig.SetBool(useDevkitModeConfigKey, value);
            }
        }

        public DevkitToggle()
        {
            InitializeComponent();
            bool? isDevkitMode = ChovyConfig.CurrentConfig.GetBool(useDevkitModeConfigKey);
            if (isDevkitMode is null) isDevkitMode = false;
            devkitCheckbox.IsChecked = isDevkitMode;
            devkitCheckbox.Unchecked += onDevkitModeUnchecked;
            devkitCheckbox.Checked += onDevkitModeChecked;

            instances.Add(this);
        }
    }
}
