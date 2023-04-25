using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChovySign_GUI.Global
{

    public partial class LabeledTextBox : UserControl
    {

        public event EventHandler<EventArgs>? TextChanged;
        protected virtual void OnTextChanged(EventArgs e)
        {
            if (TextChanged is not null)
                TextChanged(this, e);
        }
        public int MaxLength
        {
            get
            {
                return this.txtBox.MaxLength;
            }
            set
            {
                this.txtBox.MaxLength = value;
            }
        }

        public bool Password
        {
            get
            {
                return this.txtBox.Password;
            }
            set
            {
                this.txtBox.Password = value;
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
        public string? AllowedChars
        {
            get
            {
                return this.txtBox.AllowedChars;
            }
            set
            {
                this.txtBox.AllowedChars = value;
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

        public LabeledTextBox()
        {
            InitializeComponent();
            this.txtBox.TextChanged += onTxtBoxTextChange;
        }

        private void onTxtBoxTextChange(object? sender, EventArgs e)
        {
            OnTextChanged(e);
        }
    }
}
