using System.Windows;

namespace ProjectInfinity.Controls
{
  public class SelectedItemChangedEventArgs : RoutedEventArgs
  {
    private readonly DataGridCell _item;

    public DataGridCell Item
    {
      get { return _item; }
    }

    internal SelectedItemChangedEventArgs(RoutedEvent routedEvent, DataGridCell item)
      : base(routedEvent)
    {
      _item = item;
    }
  }
}