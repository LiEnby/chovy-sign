using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChovySign_GUI.Global
{
    public partial class BrowseButton : UserControl
    {
        private string fileTypeName;
        private string extension;
        private bool directory;

        public event EventHandler<EventArgs>? FileChanged;
        public event EventHandler<EventArgs>? FileRemoved;

        protected virtual void OnFileChanged(EventArgs e)
        {
            if(FileChanged is not null)
                FileChanged(this, e);
        }

        protected virtual void OnFileRemoved(EventArgs e)
        {
            if (FileRemoved is not null)
                FileRemoved(this, e);
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

        public bool IsDirectory
        {
            get
            {
                return this.directory;
            }
            set
            {
                this.directory = value;
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
                if (!this.IsDirectory)
                    return (File.Exists(this.filePath.Text) && Path.GetExtension(this.filePath.Text).Equals("." + Extension, StringComparison.InvariantCultureIgnoreCase));
                else
                    return (Directory.Exists(this.filePath.Text));
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
                if (!this.IsDirectory)
                {
                    if (File.Exists(value))
                        this.filePath.Text = value;
                    else
                        this.filePath.Text = "";
                }
                else
                {
                    if (Directory.Exists(value))
                        this.filePath.Text = value;
                    else
                        this.filePath.Text = "";
                }
            }
        }

        private async void browseClick(object sender, RoutedEventArgs e)
        {
            Window? currentWindow = TopLevel.GetTopLevel(this) as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            Button? btn = sender as Button;

            if (btn is Button)
            {
                btn.IsEnabled = false;
    
                if (this.IsDirectory)
                {

                    FolderPickerOpenOptions selectdir = new FolderPickerOpenOptions();
                    selectdir.Title = "Select directory.";
                    selectdir.AllowMultiple = false;
                    if (this.ContainsFile) selectdir.SuggestedStartLocation = await currentWindow.StorageProvider.TryGetFolderFromPathAsync(this.FilePath);

                    // open directory
                    var folders = await currentWindow.StorageProvider.OpenFolderPickerAsync(selectdir);

                    var storageFolder = folders.FirstOrDefault();
                    if (storageFolder is not null)
                    {
                        string? localPath = storageFolder.TryGetLocalPath();

                        if (localPath is not null)
                            this.FilePath = localPath;
                    }
                }
                else
                {
                    FilePickerOpenOptions selectfile = new FilePickerOpenOptions();
                    selectfile.Title = "Select " + fileTypeName;
                    selectfile.AllowMultiple = false;

                    if (extension != "")
                    {
                        var filetypes = new List<FilePickerFileType>
                        {
                            new FilePickerFileType(fileTypeName)
                            {
                                Patterns = new[] { "*."+extension },
                            }
                        };
                        selectfile.FileTypeFilter = filetypes;
                    }

                    var files = await currentWindow.StorageProvider.OpenFilePickerAsync(selectfile);
                    var storageFiles = files.FirstOrDefault();
                    if(storageFiles is not null)
                    {
                        string? localPath = storageFiles.TryGetLocalPath();

                        if (localPath is not null)
                            this.FilePath = localPath;

                    }
                }

                btn.IsEnabled = true;
                OnFileChanged(new EventArgs());
            }
        }


        public BrowseButton()
        {
            InitializeComponent();

            this.filePath.PropertyChanged += onPropertyChanged;

            this.extension = "";
            this.fileTypeName = "All Files";
        }

        private void onPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            TextBox? txt = sender as TextBox;
            if (txt is null) return;

            if (e.Property.Name == "Text")
            {
                if (txt.Text is null) return;
                if (ContainsFile)
                    OnFileChanged(new EventArgs());
                else
                    OnFileRemoved(new EventArgs());                    
            }
        }
    }
}
