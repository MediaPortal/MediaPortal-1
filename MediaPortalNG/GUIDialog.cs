using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaPortal;

namespace MediaPortal
{

    public partial class GUIDialog : Window
    {

        private ListView _popupLV;
        private string _dialogTitle;
        private int _selectedItem;
        private Button _closeButton;

        public int SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; }
        }


        public string DialogTitle
        {
            get { return _dialogTitle; }
            set { _dialogTitle = value; }
        }

        public GUIDialog(string titleText, Core parentWindow)
        {
            _selectedItem = -1;
            DialogTitle = titleText;
            this.Style = (Style)FindResource("ContextMenuStyle");
            this.Owner = parentWindow;
            this.WindowStyle = WindowStyle.None;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.AllowsTransparency = true;
            this.ApplyTemplate();
            // get the objects
            _closeButton = (Button)this.Template.FindName("CloseButton", this);
            _popupLV = (ListView)this.Template.FindName("ContextItemsControl", this);
            TextBlock tb = (TextBlock)this.Template.FindName("ContextTitle", this);

            if (tb != null)
                tb.Text = DialogTitle;

            if (_popupLV != null)
                _popupLV.SelectionChanged += new SelectionChangedEventHandler(_popupLV_SelectionChanged);

            if (_closeButton != null)
                _closeButton.Click += new RoutedEventHandler(_closeButton_Click);
        }

        void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = -1;
            this.DialogResult = false;
        }

        void _popupLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_popupLV != null)
            {
                _selectedItem=_popupLV.Items.IndexOf(_popupLV.SelectedItem);
            }

            this.DialogResult = true;
        }

        new public int ShowDialog()
        {
            if (this.Owner == null) return -1;
            this.Owner.Opacity = 0.3f;
            bool val = (bool)base.ShowDialog();
            this.Owner.Opacity = 1.0f;
            return _selectedItem;

        }

 
        public void AddMenuItem(string menuItem)
        {
            if (_popupLV == null)
                return;
            TextBlock b = new TextBlock();
            b.Text = menuItem;
            b.Tag = menuItem;
            _popupLV.Items.Add(b);
        }

        public void ClearMenuItems()
        {
            _popupLV.Items.Clear();
        }


 

    }
}
