using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  public class PluginItem : IPluginItem
  {
    private MenuCommand _pluginInfo;

    public PluginItem(MenuCommand pluginInfo)
    {
      _pluginInfo = pluginInfo;
      
    }
    #region IMenuItem Members

    public string Text
    {
      get { return _pluginInfo.Name; }
    }

    public string Description
    {
      get { return _pluginInfo.Description;}
    }

    public string ImagePath
    {
      get { return _pluginInfo.ImagePath; }
    }

    public void Execute()
    {
      _pluginInfo.Execute();
    }

    #endregion
  }
}
