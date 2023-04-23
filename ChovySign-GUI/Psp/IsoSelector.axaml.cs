using Avalonia.Controls;
using GameBuilder.Psp;
using System;

namespace ChovySign_GUI.Psp
{
    public partial class IsoSelector : UserControl
    {
        public event EventHandler<EventArgs>? UmdChanged;
        protected virtual void OnUmdChanged(EventArgs e)
        {
            if (UmdChanged is not null)
                UmdChanged(this, e);
        }
        public bool Compress
        {
            get
            {
                if (this.compressPbp.IsChecked is null) return false;
                return (bool)this.compressPbp.IsChecked;
            }
            set
            {
                this.compressPbp.IsChecked = value;
            }
        }

        public bool HasUmd
        {
            get
            {
                return this.umdFile.ContainsFile;
            }
        }
        public UmdInfo? Umd
        {
            get
            {
                if (!this.umdFile.ContainsFile) return null;
                return new UmdInfo(this.umdFile.FilePath);
            }
        }
        public IsoSelector()
        {
            InitializeComponent();
            this.umdFile.FileChanged += onUmdFileChanged;
        }

        private void onUmdFileChanged(object? sender, EventArgs e)
        {
            OnUmdChanged(new EventArgs());
        }
    }
}
