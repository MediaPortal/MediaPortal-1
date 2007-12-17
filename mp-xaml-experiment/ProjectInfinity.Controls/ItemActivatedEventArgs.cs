using System.Windows;

namespace ProjectInfinity.Controls
{
  public sealed class ItemActivatedEventArgs : RoutedEventArgs
  {
    private readonly object _item;

    public object Item
    {
      get { return _item; }
    }

    internal ItemActivatedEventArgs(RoutedEvent routedEvent, object item)
      : base(routedEvent)
    {
      _item = item;
    }
  }
}