using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace ChovySign_GUI.Global
{


    public partial class LabeledComboBox : UserControl
    {
        private List<string> items;
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
        public int SelectedIndex
        {
            get
            {
                return this.comboBox.SelectedIndex;
            }
            set
            {
                this.comboBox.SelectedIndex = value;
                OnSelectionChanged(new EventArgs());
            }
        }

        public string SelectedItem
        {
            get
            {
                string? itm = this.comboBox.SelectedItem as string;
                if (itm is null) return "";
                else return itm;
            }
            set
            {
                this.comboBox.SelectedItem = value;
                OnSelectionChanged(new EventArgs());
            }
        }

        public string[] Items
        {
            get
            {
                string[]? strings = this.comboBox.Items as string[];
                if (strings is null) return new string[0];
                return strings;
            }
            set
            {
                this.comboBox.Items = value;
            }
        }


        public event EventHandler<EventArgs>? SelectionChanged;

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged is not null)
                SelectionChanged(this, e);
        }

        public LabeledComboBox()
        {            
            InitializeComponent();
            this.comboBox.SelectionChanged += onComboBoxSelectionChange;
        }

        private void onComboBoxSelectionChange(object? sender, SelectionChangedEventArgs e)
        {
            OnSelectionChanged(new EventArgs());
        }
    }
}
