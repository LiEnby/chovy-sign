using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ChovySign_GUI.Global;
using ChovySign_GUI.Popup.Global;
using ChovySign_GUI.Settings;
using GameBuilder.Pops;
using LibChovy.Art;
using System;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable CS8601 // Possible null reference assignment.
// beacuse im checking if its null in the setter, but visual studio doesnt seem to understand :d

namespace ChovySign_GUI.Ps1
{
    public partial class GameInfoSelector : UserControl
    {
        private byte[] defaultIcon = LibChovy.Resources.ICON0;

        private byte[] iconCache;
        private byte[] pic0Cache;
        private byte[] pic1Cache;
        public string DiscId
        {
            get
            {
                if (this.discId.Text is null) return "";
                return this.discId.Text;
            }
            set
            {
                this.discId.Text = value;
            }
        }
        public string Title
        {
            get
            {
                if (this.gameTitle.Text is null) return "";
                return this.gameTitle.Text;
            }
            set
            {
                this.gameTitle.Text = value;
            }
        }
        public byte[] Icon0
        {
            get
            {
                return iconCache;
            }
            set
            {
                if (value is not null)
                    iconCache = value;
                else
                    iconCache = defaultIcon;

                loadIcon(iconCache);
            }
        }

        public byte[] Pic0
        {
            get
            {
                return pic0Cache;
            }
            set
            {
                if (value is not null)
                    pic0Cache = value;
                else
                    iconCache = LibChovy.Resources.PIC0;

            }
        }

        public byte[] Pic1
        {
            get
            {
                return pic1Cache;
            }
            set
            {
                if (value is not null)
                    pic1Cache = value;
                else
                    iconCache = LibChovy.Resources.PIC1;

            }
        }


        private void loadIcon(byte[] imageData)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                this.iconPreview.Source = new Bitmap(imageStream);
        }

        public async Task GetGameInfo(string cueFile)
        {
            Window? currentWindow = TopLevel.GetTopLevel(this) as Window;
            if (currentWindow is not Window) throw new Exception("Could not find current window");

            try
            {
                PSInfo disc = new PSInfo(cueFile);
                Title = disc.DiscName;
                DiscId = disc.DiscId;

                if (!File.Exists(this.iconFile.FilePath) && SettingsTab.Settings.DownloadPs1Covers)
                {
                    byte[] imgData = await Downloader.DownloadCover(disc);
                    if(imgData is not null)
                    {
                        defaultIcon = imgData;
                        Icon0 = imgData;
                        return;
                    }
                }
                defaultIcon = LibChovy.Resources.ICON0;

            }
            catch (FileNotFoundException e)
            {
                await MessageBox.Show(currentWindow, "Error while parsing cue sheet: " + Path.GetFileName(cueFile) + "\n\t" + e.Message, "Failed to read cue sheet", MessageBox.MessageBoxButtons.Ok);
            }
            catch (Exception e) {
                await MessageBox.Show(currentWindow, "Unknown exception occured while parsing the cue sheet: " + Path.GetFileName(cueFile) + "\n\t" + e.Message + "\n\nSTACKTRACE: " + e.StackTrace, "Failed to read cue sheet", MessageBox.MessageBoxButtons.Ok);  
            }
        }

        private async Task<byte[]?> doLoad(BrowseButton imgFile, int width, int height)
        {
            imgFile.IsEnabled = false;

            if (imgFile.FilePath != "")
            {
                try
                {
                    byte[] imageData = await Resizer.LoadImage(imgFile.FilePath, width, height);

                    imgFile.IsEnabled = true;
                    return imageData;
                }
                catch (Exception)
                {
                    Window? currentWindow = TopLevel.GetTopLevel(this) as Window;
                    if (currentWindow is not Window) throw new Exception("could not find current window");

                    await MessageBox.Show(currentWindow, "The image you selected is could not be loaded!", "Invalid image.", MessageBox.MessageBoxButtons.Ok);

                    imgFile.FilePath = "";
                };
            }

            imgFile.IsEnabled = true;
            return null;
        }

        private async void onIconChange(object? sender, EventArgs e)
        {
            BrowseButton? button = sender as BrowseButton;
            if (button is null) return;
            Icon0 = await doLoad(button, 80, 80);
        }
        private async void onPic0Change(object? sender, EventArgs e)
        {
            BrowseButton? button = sender as BrowseButton;
            if (button is null) return;
            Pic0 = await doLoad(button, 310, 180);
        }
        private async void onPic1Change(object? sender, EventArgs e)
        {
            BrowseButton? button = sender as BrowseButton;
            if (button is null) return;
            Pic1 = await doLoad(button, 480, 272);
        }

        public event EventHandler<EventArgs>? TitleChanged;
        protected virtual void OnTitleChanged(EventArgs e)
        {
            if (TitleChanged is not null)
                TitleChanged(this, e);
        }
        public event EventHandler<EventArgs>? DiscIdChanged;
        protected virtual void OnDiscIdChanged(EventArgs e)
        {
            if (DiscIdChanged is not null)
                DiscIdChanged(this, e);
        }

        public GameInfoSelector()
        {
            InitializeComponent();
            Icon0 = null;
            Pic0 = null;
            Pic1 = null;

            this.iconFile.FileChanged += onIconChange;
            this.pic0File.FileChanged += onPic0Change;
            this.pic1File.FileChanged += onPic1Change;

            this.iconFile.FileRemoved += onIconChange;
            this.pic0File.FileRemoved += onPic0Change;
            this.pic1File.FileRemoved += onPic1Change;


            this.gameTitle.TextChanged += onTitleChange;
            this.discId.TextChanged += onDiscIdChange;

        }

        private void onDiscIdChange(object? sender, EventArgs e)
        {
            OnDiscIdChanged(new EventArgs());
        }

        private void onTitleChange(object? sender, EventArgs e)
        {
            OnTitleChanged(new EventArgs());
        }
    }
}

#pragma warning restore CS8601 // Possible null reference assignment.
