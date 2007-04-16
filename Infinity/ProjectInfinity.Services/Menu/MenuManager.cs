using System.Collections.Generic;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  public class MenuManager : IMenuManager
  {
    #region IMenuManager Members

    public IList<IMenuItem> GetMenu()
    {
      IList<IMenuItem> menus = new List<IMenuItem>();
      foreach (MenuItem menuItem in ServiceScope.Get<IPluginManager>().BuildItems<MenuItem>("/Infinity/HomeMenu"))
      {
        if (menuItem.IsSubMenu)
        {
          IMenu submenu = new Menu(menuItem.Name);

          foreach (MenuItem subMenuItem in ServiceScope.Get<IPluginManager>().BuildItems<MenuItem>(menuItem.SubMenuPath))
          {
            submenu.Items.Add(new PluginItem(subMenuItem));
          }
          menus.Add(submenu);
        }
        else
        {
          menus.Add(new PluginItem(menuItem));
        }
      }
      return menus;

      ////TODO: call configuration to build up menu tree
      //IList<IMenuItem> menus = new List<IMenuItem>();
      //foreach (IPluginInfo info in ServiceScope.Get<IPluginManager>().GetAvailablePlugins())
      //{
      //  if (info.ListInMenu)
      //  {
      //    menus.Add(new PluginItem(info));
      //  }
      //}
      //return menus;
    }

    #endregion
  }
}