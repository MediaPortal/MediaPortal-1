using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Menu
{
  public class PluginMenuItem : MenuItem
  {
    IMenuItem _menu;

    public PluginMenuItem()
    {
    }
    public PluginMenuItem(IMenuItem menu)
    {
      _menu = menu;
      Label = _menu.Text;
      IPluginItem item = menu as IPluginItem;
      if (item != null)
      {
        Image = item.ImagePath;
      }
    }

    public IMenuItem Menu
    {
      get
      {
        return _menu;
      }
    }
  }
}
