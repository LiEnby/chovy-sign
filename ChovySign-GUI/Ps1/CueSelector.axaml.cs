using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace ChovySign_GUI.Ps1
{
    public partial class CueSelector : UserControl
    {
        public event EventHandler<EventArgs>? DiscsSelected;

        protected virtual void OnDiscsSelected(EventArgs e)
        {
            if (DiscsSelected is not null)
                DiscsSelected(this, e);
        }


        public string[] Discs
        {
            get
            {
                List<string> discList = new List<string>();

                if (discCue1.ContainsFile) discList.Add(discCue1.FilePath);
                if (discCue2.ContainsFile) discList.Add(discCue2.FilePath);
                if (discCue3.ContainsFile) discList.Add(discCue3.FilePath);
                if (discCue4.ContainsFile) discList.Add(discCue4.FilePath);
                if (discCue5.ContainsFile) discList.Add(discCue5.FilePath);

                return discList.ToArray();
            }
        }

        public bool AnyDiscsSelected
        {
            get
            {
                return Discs.Length > 0;
            }
        }

        private void clearAllAfter(int amt)
        {
            if (amt < 1) { discCue1.FilePath = ""; discCue1.IsEnabled = false; }
            else discCue1.IsEnabled = true;
            
            if (amt < 2) { discCue2.FilePath = ""; discCue2.IsEnabled = false; }
            else discCue2.IsEnabled = true;

            if (amt < 3) { discCue3.FilePath = ""; discCue3.IsEnabled = false; }
            else discCue3.IsEnabled = true;

            if (amt < 4) { discCue4.FilePath = ""; discCue4.IsEnabled = false; }
            else discCue4.IsEnabled = true;

            if (amt < 5) { discCue5.FilePath = ""; discCue5.IsEnabled = false; }
            else discCue5.IsEnabled = true;
        }

        private void disableCueBoxes()
        {
            if (!discCue1.ContainsFile) clearAllAfter(1);
            else if (!discCue2.ContainsFile) clearAllAfter(2);
            else if (!discCue3.ContainsFile) clearAllAfter(3);
            else if (!discCue4.ContainsFile) clearAllAfter(4);
            else if (!discCue5.ContainsFile) clearAllAfter(5);
        }

        public CueSelector()
        {
            InitializeComponent();
            disableCueBoxes();

            discCue1.FileChanged += onFileChange;
            discCue2.FileChanged += onFileChange;
            discCue3.FileChanged += onFileChange;
            discCue4.FileChanged += onFileChange;
            discCue5.FileChanged += onFileChange;

        }

        private void onFileChange(object? sender, System.EventArgs e)
        {
            disableCueBoxes();
            OnDiscsSelected(new EventArgs());
        }
    }
}
