using Avalonia.Controls;
using Avalonia.Input;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Text;

namespace ChovySign_GUI.Global
{
    public partial class LabeledTextBox : UserControl
    {
        private string lastTxt;
        private string? allowedChars;

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
                return this.txtBox.PasswordChar == default(char);
            }
            set
            {
                if (value) this.txtBox.PasswordChar = 'X';
                else this.txtBox.PasswordChar = default(char);
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
        public string AllowedChars
        {
            get
            {
                if(allowedChars is null) return "";
                return allowedChars;
            }
            set
            {
                allowedChars = value.ToUpperInvariant();
            }
        }
        public string Text
        {
            get
            {
                if (this.txtBox.Text is null) return "";
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
            lastTxt = this.txtBox.Text;
            allowedChars = null;
            this.txtBox.KeyDown += onKeyUp;
            this.txtBox.KeyUp += onKeyUp;
        }

        private void onKeyUp(object? sender, KeyEventArgs e)
        {
            TextBox? txt = sender as TextBox;
            if(txt is null) return;

            if(allowedChars is not null)
            {
                StringBuilder s = new StringBuilder();
                foreach (char c in txt.Text.ToUpperInvariant())
                    if (allowedChars.Contains(c)) s.Append(c);

                txt.Text = s.ToString().ToUpperInvariant();
            }

            if (txt.Text != lastTxt) OnTextChanged(new EventArgs());
            lastTxt = txt.Text;
        }
    }
}
