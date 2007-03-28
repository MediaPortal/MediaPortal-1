using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  public class MenuManager : IMenuManager
  {
    #region IMenuManager Members

    public IList<IMenuItem> GetMenu()
    {
      //TODO: call pluginmanager and configuration to build up menu tree
      IList<IMenuItem> menus = new List<IMenuItem>();
      menus.Add(new PluginItem(new PluginInfo("Music", "Plays music")));
      Menu subMenu = new Menu("Settings");
      subMenu.Items.Add(new PluginItem(new PluginInfo("Obscure", "some obscure settings")));
      menus.Add(subMenu);
      return menus;
    }

    #endregion
  }
}
