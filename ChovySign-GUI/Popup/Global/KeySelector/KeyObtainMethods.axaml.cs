using Avalonia.Controls;
using Avalonia.Interactivity;
using LibChovy.VersionKey;

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

        private void noPspEmuDrmMethodClick(object sender, RoutedEventArgs e)
        {
            this.method = VersionKeyMethod.NOPSPEMUDRM_METHOD;
            this.Close(method);
        }


        public KeyObtainMethods()
        {
            InitializeComponent();
        }
    }
}
