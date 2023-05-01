using Avalonia.Controls;
using ChovySign_GUI.Global;
using LibChovy.Config;
using System;

namespace ChovySign_GUI.Settings
{
    public partial class ConfigPath : ConfigControl
    {
        internal override void init()
        {
            string? cfgText = ChovyConfig.CurrentConfig.GetString(ConfigKey);
            if (cfgText is null) cfgText = "";
            configPath.FilePath = cfgText;
        }
        public string Label
        {
            get
            {
                return configPath.Label;
            }
            set
            {
                configPath.Label = value;
            }
        }
        public string Value
        {
            get
            {
                return this.configPath.FilePath;
            }
            set
            {
                this.configPath.FilePath = value;

                if(configPath.ContainsFile)
                    ChovyConfig.CurrentConfig.SetString(ConfigKey, this.configPath.FilePath);
            }
        }
        public bool IsDirectory
        {
            get
            {
                return this.configPath.IsDirectory;
            }
            set
            {
                this.configPath.IsDirectory = value;
            }
        }

        public string Extension
        {
            get
            {
                return this.configPath.Extension;
            }
            set
            {
                this.configPath.Extension = value;
            }
        }
        public string FileTypeName
        {
            get
            {
                return this.configPath.FileTypeName;
            }
            set
            {
                this.configPath.FileTypeName = value;
            }
        }
        public ConfigPath()
        {
            InitializeComponent();
            init();

            this.configPath.FileChanged += onFileChange;
        }

        private void onFileChange(object? sender, EventArgs e)
        {
            BrowseButton? browseBtn = sender as BrowseButton;
            if (browseBtn is null) return;
            if (!browseBtn.ContainsFile) return;

            ChovyConfig.CurrentConfig.SetString(ConfigKey, browseBtn.FilePath);
        }
    }
}
