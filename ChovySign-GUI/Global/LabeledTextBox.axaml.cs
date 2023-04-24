using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JetBrains.Annotations;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Text;

namespace ChovySign_GUI.Global
{
    public partial class LabeledTextBox : UserControl
    {
        private string lastTxt;
        private string? allowedChars = null;

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
        public string? AllowedChars
        {
            get
            {
                if (allowedChars is null) return "";
                else return allowedChars;
            }
            set
            {
                allowedChars = value;
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

            this.txtBox.PastingFromClipboard += onPaste;
            this.txtBox.AddHandler(TextInputEvent, onTxtInput, RoutingStrategies.Tunnel);
        }

        private string filter(string original)
        {
            if (allowedChars is not null)
            {
                string newTxt = original.ToUpperInvariant();
                for (int i = 0; i < newTxt.Length; i++)
                {
                    if (!allowedChars.Contains(newTxt[i]))
                    {
                        newTxt = newTxt.Replace(newTxt[i].ToString(), "");
                    }
                }
                return newTxt;
            }
            else
            {
                return original;
            }
        }

        private async void onPaste(object? sender, RoutedEventArgs e)
        {
            TextBox? txt = sender as TextBox;
            if (txt is null) return;
            if (Application.Current is null) return;
            if (Application.Current.Clipboard is null) return;

            e.Handled = true;

            string? newTxt = await Application.Current.Clipboard.GetTextAsync();
            if (newTxt is null) newTxt = "";

            /*TextInputEventArgs txtInput = new TextInputEventArgs();
            txtInput.Text = filter(newTxt);*/

            //txt.RaiseEvent(new RoutedEventArgs(txtInput));
        }

        private void onTxtInput(object? sender, TextInputEventArgs e)
        {       
            string? newTxt = e.Text;
            if (newTxt is null) newTxt = "";

            newTxt = filter(newTxt);
            e.Text = newTxt;

            if (newTxt != lastTxt) OnTextChanged(new EventArgs());
            lastTxt = newTxt;
        }
    }
}
