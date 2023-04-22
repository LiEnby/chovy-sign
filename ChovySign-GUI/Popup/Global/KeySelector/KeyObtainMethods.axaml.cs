using Avalonia.Controls;
using Avalonia.Interactivity;
using GameBuilder.VersionKey;

namespace ChovySign_GUI.Popup.Global.KeySelector
{
    public partial class KeyObtainMethods : Window
    {
        private int keyIndex;
        private VersionKeyMethod method;
        public VersionKeyMethod Method
        {
            get
            {
                return method;
            }
        }
        public int KeyIndex
        {
            set
            {
                keyIndex = value;
                if (keyIndex == 1) { ebootMethodPspGraphic.IsVisible = false; ebootMethodPs1Graphic.IsVisible = true; }
                else { ebootMethodPspGraphic.IsVisible = true; ebootMethodPs1Graphic.IsVisible = false; }
            }
        }

        private void ebootMethodClick(object sender, RoutedEventArgs e)
        {
            this.method = VersionKeyMethod.EBOOT_PBP_METHOD;
            this.Close(method);
        }

        private void actRifMethodClick(object sender, RoutedEventArgs e)
        {
            this.method = VersionKeyMethod.ACT_RIF_METHOD;
            this.Close(method);
        }

        private void keysTxtMethodClick(object sender, RoutedEventArgs e)
        {
            this.method = VersionKeyMethod.KEYS_TXT_METHOD;
            this.Close(method);
        }


        public KeyObtainMethods()
        {
            InitializeComponent();
        }
    }
}
