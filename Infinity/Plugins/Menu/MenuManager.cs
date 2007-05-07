using System.Collections.Generic;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  public class MenuManager : IMenuManager
  {
    #region IMenuManager Members

    public IList<IMenuItem> GetMenu(string menuPath)
    {
      IList<IMenuItem> menus = new List<IMenuItem>();

      foreach (IMenuItem menuItem in ServiceScope.Get<IPluginManager>().BuildItems<IMenuItem>(menuPath))
      {
        if (menuItem is SubMenuItem)
        {
          IMenu submenu = new Menu(menuItem);

          foreach (MenuItem subMenuItem in ServiceScope.Get<IPluginManager>().BuildItems<MenuItem>(((SubMenuItem)menuItem).SubMenuPath))
          {

            submenu.Items.Add(subMenuItem);
          }
          //TODO: should be an IMenu implementation
          menus.Add(submenu);
        }
        else
        {
          menus.Add(menuItem);
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