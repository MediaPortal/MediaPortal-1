using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Menu
{
  public class Menu : IMenu
  {
    private List<IMenuItem> _items = new List<IMenuItem>();
    private IMenuItem _menuItem;

    public Menu(IMenuItem menuItem)
    {
      _menuItem = menuItem;
    }

    #region IMenu Members

    public List<IMenuItem> Items
    {
      get { return _items; }
    }

    public IMenuItem MenuItem
    {
      get { return _menuItem; }
    }

    #endregion

    #region IMenuItem Members

    public string Text
    {
      get { return _menuItem.Text; }
    }

    #endregion
  }
}
