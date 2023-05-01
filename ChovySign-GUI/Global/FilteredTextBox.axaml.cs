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
            this.txtBox.PropertyChanged += onPropertyChanged;
            this.txtBox.AddHandler(TextInputEvent, onTxtInput, RoutingStrategies.Tunnel);
        }

        private void onPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            TextBox? txt = sender as TextBox;
            if (txt is null) return;

            if (e.Property.Name == "Text")
            {
                if (txt.Text is null) return;
                txt.Text = filter(txt.Text);

                OnTextChanged(new EventArgs());
            }

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
