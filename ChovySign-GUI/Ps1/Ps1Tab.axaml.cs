using Avalonia.Controls;
using System.Linq;

namespace ChovySign_GUI.Ps1
{
    public partial class Ps1Tab : UserControl
    {
        public Ps1Tab()
        {
            InitializeComponent();
            discSelector.DiscsSelected += onDiscSelected;

            progressStatus.IsEnabled = false;
        }

        private void onDiscSelected(object? sender, System.EventArgs e)
        {
            CueSelector? cueSelector = sender as CueSelector;
            if (cueSelector is null) return;

            if (cueSelector.AnyDiscsSelected)
                _ = this.gameInfo.GetGameInfo(cueSelector.Discs.First());
            else
                progressStatus.IsEnabled = false;
        }
    }
}
