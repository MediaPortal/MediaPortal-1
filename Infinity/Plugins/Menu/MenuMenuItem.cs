using System;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Menu
{
  internal class MenuMenuItem : MenuItem
  {
    private readonly IMenuItem _menuItem;

    public MenuMenuItem(IMenuItem menuItem) : base(menuItem.Text)
    {
      _menuItem = menuItem;
    }

    public override string Image
    {
      get { return _menuItem.ImagePath; }
      set { throw new NotSupportedException(); }
    }

    public IMenuItem GetIMenuItem()
    {
      return _menuItem;
    }
  }
}