using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaPortal;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Media;
using System.Windows.Media.Imaging;

namespace MediaPortal
{

    public class GUIPlugin : INotifyPropertyChanged
    {

        public GUIPlugin()
        {
           
        }

        private string _pluginText;
        private string _pluginName;
        private Type _guiPluginObject;
        private BitmapSource _pluginHover;

        public Type GUIPluginObject
        {
            get
            {
                return _guiPluginObject;
            }
            set
            {
                _guiPluginObject = value;
                OnPropertyChanged("GUIPluginObject");
            }
        }


        public string PluginText
        {
            get
            {
                return _pluginText;
            }
            set
            {
                _pluginText = value;
                OnPropertyChanged("PluginText");
            }
        }

        public string PluginName
        {
            get { 
                return _pluginName; 
            }
            set {
                _pluginName= value;
                OnPropertyChanged("PluginName");
            }
        }
        public BitmapSource PluginHover
        {
            get
            {
                return _pluginHover;
            }
            set
            {
                _pluginHover = value;
                OnPropertyChanged("PluginHover");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
        #endregion
    }

    public class GUIPluginList : ObservableCollection<GUIPlugin>
    {
        public GUIPluginList()
            : base()
        {
        }
    }
}
