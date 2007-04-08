namespace ProjectInfinity.Menu
{
  /// <summary>
  /// ViewModel representing an <see cref="IMenu"/>
  /// </summary>
  /// <remarks>
  /// The purpose of this ViewModel is to be databound in UIs
  /// </remarks>
  public class MenuViewModel
  {
    private IMenu _menu;
    private MenuItemViewModel _menuItems;

    public MenuViewModel(IMenu model)
    {
      _menu = model;
      _menuItems = new MenuItemViewModel(_menu.Items);
    }

    /// <summary>
    /// Returns the <see cref="IMenu.Text"/> content.
    /// </summary>
    public string Text
    {
      get { return _menu.Text; }
    }

    /// <summary>
    /// Returns a <see cref="MenuItemViewModel"/> of the
    /// items in the <see cref="IMenu"/> this ViewModel is representing.
    /// </summary>
    public MenuItemViewModel Items
    {
      get { return _menuItems;}
    }
  }
}