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
      Image = _menu.ImagePath;
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
