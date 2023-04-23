using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ChovySign_GUI.Popup.Global;
using Li.Progress;
using LibChovy;
using System;
using System.Threading.Tasks;
using static ChovySign_GUI.Popup.Global.MessageBox;

namespace ChovySign_GUI.Global
{
    public partial class ProgressStatus : UserControl
    {
        public ChovySignParameters? Parameters = null;
        private ChovySign chovySign;
        public ProgressStatus()
        {
            InitializeComponent();

            chovySign = new ChovySign();
            chovySign.RegisterCallback(onProgress);
        }
        public event EventHandler<EventArgs>? BeforeStart;
        protected virtual void OnBeforeStart(EventArgs e)
        {
            if (BeforeStart is not null)
                BeforeStart(this, e);
        }

        public event EventHandler<EventArgs>? Finished;
        protected virtual void OnFinished(EventArgs e)
        {
            if (Finished is not null)
                Finished(this, e);
        }

        private async void goClick(object sender, RoutedEventArgs e)
        {

            Window? currentWindow = this.VisualRoot as Window;
            if (currentWindow is not Window) throw new Exception("could not find current window");

            this.goButton.IsEnabled = false;

            OnBeforeStart(new EventArgs());

            if(Parameters is null) { await MessageBox.Show(currentWindow, "ChovySignParameters was null, cannot start!", "Invalid Parameters", MessageBoxButtons.Ok); return; }

            await Task.Run(() => { chovySign.Go(Parameters); });

            OnFinished(new EventArgs());

            this.goButton.IsEnabled = true;

        }
        private void onProgress(ProgressInfo inf)
        {
            Dispatcher.UIThread.Post(() =>
            {
                this.statusLbl.Content = inf.CurrentProcess + " (" + inf.Done + "/" + inf.Remain + ") " + inf.ProgressInt + "%";
                this.progressVal.Value = inf.Progress;
            });
        }
    }
}
