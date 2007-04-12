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

  public class DataGridCollection : List<DataGridRow>, INotifyPropertyChanged, INotifyCollectionChanged
  {
    DataGrid _grid;
    DataGridCell _currentItem;
    public event PropertyChangedEventHandler PropertyChanged;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// Gets or sets the data grid.
    /// </summary>
    /// <value>The data grid.</value>
    public DataGrid DataGrid
    {
      get
      {
        return _grid;
      }
      set
      {
        _grid = value;
      }
    }
    /// <summary>
    /// Called when collection changed
    /// Updates the grid so it shows the new collection
    /// </summary>
    public void OnCollectionChanged()
    {
      if (CollectionChanged != null)
      {
        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }
      if (_grid != null)
      {
        _grid.UpdateGrid();
      }
    }
    /// <summary>
    /// Gets or sets the current item.
    /// </summary>
    /// <value>The current item.</value>
    public DataGridCell CurrentItem
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
