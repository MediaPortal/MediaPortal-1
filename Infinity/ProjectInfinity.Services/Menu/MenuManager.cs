using System.Collections.Generic;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  public class MenuManager : IMenuManager
  {
    #region IMenuManager Members

    public IList<IMenuItem> GetMenu()
    {
      //TODO: call configuration to build up menu tree
      IList<IMenuItem> menus = new List<IMenuItem>();
      foreach (IPluginInfo info in ServiceScope.Get<IPluginManager>().GetAvailablePlugins())
      {
        if (info.ListInMenu)
        {
          menus.Add(new PluginItem(info));
        }
      }
      return menus;
    }

    #endregion
  }
}