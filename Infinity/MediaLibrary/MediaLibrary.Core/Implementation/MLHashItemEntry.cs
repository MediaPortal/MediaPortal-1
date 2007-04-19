using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;



namespace MediaLibrary
{
    // TODO: We have to make this serializable to xml like
    // <plugin-property name="show-controls">True</plugin-property> 
    // <plugin-property name="clear-playlist">True</plugin-property> 

    [Serializable]
    public class MLHashItemEntry : INotifyPropertyChanged 
    {
        private object _Value = null;

        [XmlAttribute]
        public string Key;
        
        [XmlText]
        public object Value
        {
            get
            {
                return this._Value;
            }

            set
            {
                if (value != this._Value)
                {
                    this._Value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
