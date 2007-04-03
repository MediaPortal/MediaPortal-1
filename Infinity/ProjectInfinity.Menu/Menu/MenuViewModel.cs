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
    private IMenu menu;

    public MenuViewModel(IMenu model)
    {
      menu = model;
    }

    /// <summary>
    /// Returns the <see cref="IMenu.Text"/> content.
    /// </summary>
    public string Text
    {
      get { return menu.Text; }
    }

    /// <summary>
    /// Returns a <see cref="MenuItemViewModel"/> of the
    /// items in the <see cref="IMenu"/> this ViewModel is representing.
    /// </summary>
    public MenuItemViewModel Items
    {
      get { return new MenuItemViewModel(menu.Items); }
    }
  }
}