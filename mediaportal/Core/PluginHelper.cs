using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;

namespace MediaPortal
{
  public class PluginHelper
  {
    private static List<string> _notifyPluginsFromResume = new List<string>();

    public static void AddPluginToListOfNotifyPluginsFromResume(string plugin)
    {
      _notifyPluginsFromResume.Add((plugin));
      Log.Debug("PluginHelper: {0} added to the list.", plugin);
    }

    public static bool IsPluginOnListOfNotifyPluginsFromResume(string plugin)
    {
      return _notifyPluginsFromResume.Contains(plugin);
    }: 

    public static void CleanListOfNotifyPluginsFromResume()
    {
      _notifyPluginsFromResume.Clear();
      Log.Debug("PluginHelper: list is cleaned.");
    }
       
  }
}
