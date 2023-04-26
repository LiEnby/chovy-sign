using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Media;
using System.Threading.Tasks;

namespace ChovySign_GUI
{
    public partial class App : Application
    {
        public static async Task PlayFinishSound()
        {
            if (!OperatingSystem.IsWindows()) return;
            await Task.Run(() =>
            {
                using (SoundPlayer player = new SoundPlayer(Resource.FINISHSND))
                {
                    player.Play();
                }
            });
        }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}