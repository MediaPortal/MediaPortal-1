using System;
using System.Collections.Generic;
using System.Text;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu
{
  public class PluginItem : IPluginItem
  {
    private IPluginInfo _pluginInfo;

    public PluginItem(IPluginInfo pluginInfo)
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
    #endregion
  }
}
