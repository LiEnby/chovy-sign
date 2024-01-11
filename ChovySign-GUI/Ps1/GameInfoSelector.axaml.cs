using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ChovySign_GUI.Global;
using ChovySign_GUI.Popup.Global;
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
                if(value is not null)
                    iconCache = value;

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
            }
        }


        private void loadIcon(byte[] imageData)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
                this.iconPreview.Source = new Bitmap(imageStream);
        }

        public async Task GetGameInfo(string cueFile)
        {
            try
            {
                PSInfo disc = new PSInfo(cueFile);
                Title = disc.DiscName;
                DiscId = disc.DiscId;

                if (!File.Exists(this.iconFile.FilePath))
                {
                    byte[] newCover = await Downloader.DownloadCover(disc);
                    loadIcon(newCover);
                    iconCache = newCover;
                }
            }
            catch (Exception e) {
                Window? currentWindow = this.VisualRoot as Window;
                if (currentWindow is not Window) throw new Exception("could not find current window");

                await MessageBox.Show(currentWindow, "unable to read cue sheet: " + Path.GetFileName(cueFile) + "\n" + e.Message + "\n\nSTACKTRACE: " + e.StackTrace, "cannot load cue sheet", MessageBox.MessageBoxButtons.Ok);  
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
                    Window? currentWindow = this.VisualRoot as Window;
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
            iconCache = LibChovy.Resources.ICON0;
            pic0Cache = LibChovy.Resources.PIC0;
            pic1Cache = LibChovy.Resources.PIC1;

            loadIcon(iconCache);

            this.iconFile.FileChanged += onIconChange;
            this.pic0File.FileChanged += onPic0Change;
            this.pic1File.FileChanged += onPic1Change;

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
