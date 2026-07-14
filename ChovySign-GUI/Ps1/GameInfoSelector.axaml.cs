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
// beacuse im checking if its null in the setter, but msbuild doesnt seem to understand :d
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
// setting it as null is explicit behaviour inside the Setter/Getter, and does not really set underlying values to null
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
// we are setting this in the setter for Pic0/Icon0/Pic1, etc but msbuild doesnt seem to get that;


namespace ChovySign_GUI.Ps1
{
    public partial class GameInfoSelector : UserControl
    {
        private byte[] defaultIcon = LibChovy.Resources.ICON0;

        private byte[] iconCache = null;
        private byte[] pic0Cache = null;
        private byte[] pic1Cache = null;
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
                if (iconCache is null) return defaultIcon;
                return iconCache;
            }
            set
            {
                iconCache = value;
                loadIcon(iconCache);
            }
        }

        public byte[] Pic0
        {
            get
            {
                if(pic0Cache is null) return LibChovy.Resources.PIC0;
                return pic0Cache;
            }
            set
            {
                pic0Cache = value;
            }
        }

        public byte[] Pic1
        {
            get
            {
                if (pic1Cache is null) return LibChovy.Resources.PIC1;
                return pic1Cache;
            }
            set
            {
                pic1Cache = value;
            }
        }


        private void loadIcon(byte[] imageData)
        {
            if (imageData is null) imageData = defaultIcon;

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

                
                if (!File.Exists(this.iconFile.FilePath) && SettingsTab.Settings is not null && SettingsTab.Settings.DownloadPs1Covers)
                {
                    byte[] imgData = await Downloader.DownloadCover(disc);
                    if(imgData is not null)
                    {
                        defaultIcon = imgData;
                        Icon0 = imgData;
                        return;
                    }
                }
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
