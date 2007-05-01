using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ProjectInfinity.Controls
{
  public class MenuCollection : List<MenuItem>, INotifyPropertyChanged, INotifyCollectionChanged, ICurrentItem
  {
    private MenuItem _currentItem;

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion

    #region ICurrentItem Members

    /// <summary>
    /// Gets or sets the current item.
    /// </summary>
    /// <value>The current item.</value>
    public object CurrentItem
    {
      get { return _currentItem; }
      set
      {
        _currentItem = (MenuItem) value;
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs("CurrentItem"));
        }
      }
    }

    #endregion

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
    }
  }
}