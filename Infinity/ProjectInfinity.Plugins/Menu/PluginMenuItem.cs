using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Menu
{
  public class PluginMenuItem : MenuItem
  {
    IMenuItem _menu;

    public PluginMenuItem(IMenuItem menu)
    {
      _menu = menu;
      Label = _menu.Text;
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
