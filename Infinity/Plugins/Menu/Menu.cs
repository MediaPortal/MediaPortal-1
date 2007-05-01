using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Menu
{
  public class Menu : IMenu
  {
    private readonly List<IMenuItem> _items = new List<IMenuItem>();
    private readonly IMenuItem _DefaultItem;

    public Menu(IMenuItem menuItem)
    {
      _DefaultItem = menuItem;
    }

    #region IMenu Members

    public List<IMenuItem> Items
    {
      get { return _items; }
    }

    public IMenuItem DefaultItem
    {
      get { return _DefaultItem; }
    }

    #endregion

    #region IMenuItem Members

    public string ImagePath
    {
      get { return _DefaultItem.ImagePath; }
    }

    public string Text
    {
      get { return _DefaultItem.Text; }
    }

    public void Accept(IMenuItemVisitor visitor)
    {
      visitor.Visit(this);
    }

    #endregion

    [Obsolete("This method will disappear very soon",true)]
    void IMenuItem.Execute()
    {
      throw new NotSupportedException();
    }
  }
}
