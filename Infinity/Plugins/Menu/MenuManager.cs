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

      Plugins.Menu menuInfo = (Plugins.Menu) ServiceScope.Get<IPluginManager>().BuildItem<Plugins.Menu>("/Menus", id);
      if (menuInfo != null)
      {
        foreach (MenuItem menuItem in ServiceScope.Get<IPluginManager>().BuildItems<MenuItem>(menuInfo.Path))
        {
          if (menuItem.IsSubMenu)
          {
            IMenu submenu = new Menu(new PluginItem(menuItem));

            foreach (
              MenuItem subMenuItem in ServiceScope.Get<IPluginManager>().BuildItems<MenuItem>(menuItem.SubMenuPath))
            {
              //TODO: should be an IMenuItem implementation
              submenu.Items.Add(new PluginItem(subMenuItem));
            }
            //TODO: should be an IMenu implementation
            menus.Add(submenu);
          }
          else
          {
            //TODO: should be an IPluginItem or IMessageItem implementation
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
      Plugins.Menu menuInfo = (Plugins.Menu) ServiceScope.Get<IPluginManager>().BuildItem<Plugins.Menu>("/Menus", id);
      if (menuInfo != null)
      {
        return menuInfo.Name;
      }
      return "";
    }

    #endregion
  }
}