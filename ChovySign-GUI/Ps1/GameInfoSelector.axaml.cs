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
                DiscInfo disc = new DiscInfo(cueFile);
                this.gameTitle.Text = disc.DiscName;

                byte[] newCover = await Downloader.DownloadCover(disc);
                loadIcon(newCover);
                iconCache = newCover;
            }
            catch (Exception) { }
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

        }

    }
}

#pragma warning restore CS8601 // Possible null reference assignment.
