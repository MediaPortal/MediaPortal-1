using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectInfinity.Controls
{

  public class DataGridCollection : List<DataGridRow>, INotifyPropertyChanged
  {
    DataGridCell _currentItem;
    public  event PropertyChangedEventHandler PropertyChanged;


    public DataGridCell  CurrentItem
    {
      get
      {
        return _currentItem;
      }
      set
      {
        _currentItem = value;
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs("CurrentItem"));
        }
      }
    }




  }
}
