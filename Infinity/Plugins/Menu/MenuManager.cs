using System.Collections.Generic;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  public class MenuManager : IMenuManager
  {
    #region IMenuManager Members

    public IList<IMenuItem> GetMenu(string id)
    {
      IList<IMenuItem> menus = new List<IMenuItem>();

      ProjectInfinity.Plugins.Menu menuInfo = (ProjectInfinity.Plugins.Menu)ServiceScope.Get<IPluginManager>().BuildItem<ProjectInfinity.Plugins.Menu>("/Menus", id);
      if (menuInfo != null)
      {
        foreach (MenuItem menuItem in ServiceScope.Get<IPluginManager>().BuildItems<MenuItem>(menuInfo.Path))
        {
          if (menuItem.IsSubMenu)
          {
            IMenu submenu = new Menu(new PluginItem(menuItem));

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

    public string GetMenuName(string id)
    {
      ProjectInfinity.Plugins.Menu menuInfo = (ProjectInfinity.Plugins.Menu)ServiceScope.Get<IPluginManager>().BuildItem<ProjectInfinity.Plugins.Menu>("/Menus", id);
      if (menuInfo != null)
      {
        return menuInfo.Name;
      }
      return "";
    }
    #endregion
  }
}