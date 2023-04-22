using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChovySign_GUI.Global
{
    public partial class BrowseButton : UserControl
    {
        private string fileTypeName;
        private string extension;

        public event EventHandler<EventArgs>? FileChanged;

        protected virtual void OnFileChanged(EventArgs e)
        {
            if(FileChanged is not null)
                FileChanged(this, e);
        }

        public string Extension
        {
            get
            {
                return extension.Replace(".", "");
            }
            set
            {
                extension = value;
            }
        }
        public string FileTypeName
        {
            get
            {
                return fileTypeName;
            }
            set
            {
                fileTypeName = value;
            }
        }
        public string Watermark
        {
            get
            {
                return this.filePath.Watermark;
            }
            set
            {
                this.filePath.Watermark = value;
            }
        }
        public string Label
        {
            get
            {
                string? lbl = this.browseLabel.Content as String;
                if (lbl is null) return "";
                else return lbl;
            }
            set
            {
                this.browseLabel.Content = value;
            }
        }
        public bool ContainsFile
        {
            get
            {
                return (File.Exists(this.filePath.Text) && Path.GetExtension(this.filePath.Text).Equals("." + Extension, StringComparison.InvariantCultureIgnoreCase));
            }
        }
        public string FilePath
        {
            get
            {
                if (!ContainsFile) return "";
                return this.filePath.Text;
            }
            set
            {
                if (File.Exists(value))
                    this.filePath.Text = value;
                else
                    this.filePath.Text = "";
            }
        }

        private async void browseClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn is Button)
            {
                btn.IsEnabled = false;

                OpenFileDialog browseDialog = new OpenFileDialog();
                if (extension != "")
                {
                    browseDialog.Filters = new List<FileDialogFilter>();
                    FileDialogFilter filter = new FileDialogFilter();
                    filter.Extensions.Add(extension);
                    filter.Name = fileTypeName;
                    browseDialog.Filters.Add(filter);
                    browseDialog.Title = "Select a " + fileTypeName + " file.";
                }
                else
                {
                    browseDialog.Title = "Select a file.";
                }


                Window? currentWindow = this.VisualRoot as Window;
                if (currentWindow is not Window) throw new Exception("could not find current window");

                string[]? selectedFiles = await browseDialog.ShowAsync(currentWindow);
                if (selectedFiles is not null && selectedFiles.Length > 0)
                    this.FilePath = selectedFiles.First();

                btn.IsEnabled = true;

                OnFileChanged(new EventArgs());
            }
        }

        public BrowseButton()
        {
            InitializeComponent();

            this.filePath.KeyUp += onKeyPress;

            this.extension = "";
            this.fileTypeName = "All Files";
        }

        private void onKeyPress(object? sender, KeyEventArgs e)
        {
            OnFileChanged(new EventArgs());
        }
    }
}
