using Avalonia.Controls;

namespace ChovySign_GUI.Global
{
    public partial class LabeledTextBox : UserControl
    {
        public string Label
        {
            get
            {
                string? lbl = this.lblTxt.Content as string;
                if (lbl is null) return "";
                else return lbl;
            }
            set
            {
                this.lblTxt.Content = value;
            }
        }

        public string Watermark
        {
            get
            {
                return this.txtBox.Watermark;
            }
            set
            {
                this.txtBox.Watermark = value;
            }
        }

        public string Text
        {
            get
            {
                return this.txtBox.Text;
            }
            set
            {
                this.txtBox.Text = value;
            }
        }
        public LabeledTextBox()
        {
            InitializeComponent();
        }
    }
}
