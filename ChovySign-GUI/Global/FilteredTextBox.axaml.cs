using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ChovySign_GUI.Global
{
    public partial class FilteredTextBox : UserControl
    {

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
        public FilteredTextBox()
        {
            InitializeComponent();
            this.txtBox.PastingFromClipboard += onPaste;
            this.txtBox.AddHandler(TextInputEvent, onTxtInput, RoutingStrategies.Tunnel);
        }


        private string filter(string original)
        {
            if (allowedChars is not null)
            {
                StringBuilder str = new StringBuilder();

                for (int i = 0; i < original.Length; i++)
                {
                    if (allowedChars.Contains(original[i]))
                        str.Append(original[i]);
                }

                return str.ToString();
            }
            return original;
        }
        private async Task<bool> setClipboardText(string text)
        {
            if (Application.Current is null) return false;
            if (Application.Current.Clipboard is null) return false;
            await Application.Current.Clipboard.SetTextAsync(text);
            return true;
        }
        private async Task<string> getClipboardText()
        {
            if (Application.Current is null) return "";
            if (Application.Current.Clipboard is null) return "";
            string? clipboard = await Application.Current.Clipboard.GetTextAsync();
            if (clipboard is null) return "";
            return clipboard;
        }

        private async void onPaste(object? sender, RoutedEventArgs e)
        {
            TextBox? txt = sender as TextBox;
            if (txt is null) return;

            string clipboard = getClipboardText().Result;
            clipboard = filter(clipboard);
            _ = setClipboardText(clipboard).Result;
            
            // annoyingly, the text being pasted isnt actually in the textbox yet
            // and it wont trigger a textInput event when pasting; t-this really is the best can do 
            await Task.Delay(100);
            OnTextChanged(new EventArgs());
        }

        private void onTxtInput(object? sender, TextInputEventArgs e)
        {
            string? newTxt = e.Text;
            if (newTxt is null) newTxt = "";

            newTxt = filter(newTxt);
            e.Text = newTxt;

            OnTextChanged(new EventArgs());
        }
    }
}
