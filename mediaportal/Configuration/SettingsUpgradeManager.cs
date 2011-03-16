using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration
{
  internal sealed class SettingsUpgradeManager
  {
    #region Fields

    /// <summary>
    /// Singleton
    /// </summary>
    private static SettingsUpgradeManager _instance = new SettingsUpgradeManager();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the singleton instance
    /// </summary>
    internal static SettingsUpgradeManager Instance
    {
      get { return _instance; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Upgrades the specified settings collection to the latest structure
    /// </summary>
    /// <param name="settings"></param>
    internal void UpgradeToLatest(ISettingsProvider settings)
    {
      // Deleted entries
      RemoveEntry(settings, "general", "rtllang");
      RemoveEntry(settings, "dvdplayer", "autoplay");
      RemoveEntry(settings, "audioplayer", "autoplay");
      RemoveEntry(settings, "musicfiles", "showid3");
      RemoveEntry(settings, "musicfiles", "showSortButton");
      RemoveEntry(settings, "musicmisc", "enqueuenext");
      RemoveEntry(settings, "plugins", "Burner");
      RemoveEntry(settings, "plugins", "VideoEditor");
      RemoveEntry(settings, "plugins", "Foobar2000");
      RemoveEntry(settings, "plugins", "AutoCropper");
      RemoveEntry(settings, "plugins", "ISDN Caller-ID");
      RemoveEntry(settings, "plugins", "YAC Caller-ID");
      RemoveEntry(settings, "plugins", "MAME Devices");
      RemoveEntry(settings, "home", "Burner");
      RemoveEntry(settings, "home", "VideoEditor");
      RemoveEntry(settings, "myplugins", "Burner");
      RemoveEntry(settings, "myplugins", "VideoEditor");
      RemoveEntry(settings, "pluginswindows", "MediaPortal.GUI.GUIBurner.GUIBurner");
      RemoveEntry(settings, "pluginswindows", "WindowPlugins.VideoEditor.GUIVideoEditor");

      // Moved entries
      MoveEntry(settings, "general", "gui", "mousesupport");
      MoveEntry(settings, "general", "gui", "hideextensions");
      MoveEntry(settings, "general", "gui", "allowRememberLastFocusedItem");
      MoveEntry(settings, "general", "gui", "myprefix");
      MoveEntry(settings, "general", "gui", "startbasichome");
      MoveEntry(settings, "general", "gui", "autosize");
      MoveEntry(settings, "general", "gui", "enableguisounds");
      MoveEntry(settings, "general", "gui", "ScrollSpeedRight");
      MoveEntry(settings, "general", "gui", "ScrollSpeedDown");
      MoveEntry(settings, "skin", "gui", "language");
      MoveEntry(settings, "general", "gui", "useonlyonehome");

      //  blue3/wide is now default/wide
      UpdateEntryDefaultValue(settings, "skin", "name", "Blue3", "Default");
      UpdateEntryDefaultValue(settings, "skin", "name", "Blue3wide", "DefaultWide");

      settings.Save();
    }

    /// <summary>
    /// Removes a setting from the config file
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="section"></param>
    /// <param name="entry"></param>
    private void RemoveEntry(ISettingsProvider settings, string section, string entry)
    {
      try
      {
        //  Check if value exists
        var value = settings.GetValue(section, entry);

        if (value != null)
        {
          settings.RemoveEntry(section, entry);

          Log.Info("(Settings upgrade) Removed unused entry " + section + "/" + entry);
        }
      }
      catch (Exception ex)
      {
        Log.Error("(Settings upgrade) Unhandled exception when trying to remove entry " + section + "/" + entry + "\r\n\r\n" + ex.ToString());
      }
    }

    /// <summary>
    /// Moves a setting from one section to another
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="fromSection"></param>
    /// <param name="toSection"></param>
    /// <param name="entry"></param>
    private void MoveEntry(ISettingsProvider settings, string fromSection, string toSection, string entry)
    {
      try
      {
        //  Check if value needs moving
        var value = settings.GetValue(fromSection, entry);

        if (value != null)
        {
          settings.MoveEntry(fromSection, toSection, entry);

          Log.Info("(Settings upgrade) Moved entry " + entry + " from section " + fromSection + "->" + toSection);
        }
      }
      catch (Exception ex)
      {
        Log.Error("(Settings upgrade) Unhandled exception when trying to move entry " + entry + " from section " + fromSection + "->" + toSection + "\r\n\r\n" + ex.ToString());
      }
    }

    /// <summary>
    /// Updates an entries default value if is as expected
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="section"></param>
    /// <param name="entry"></param>
    /// <param name="fromValue"></param>
    /// <param name="toValue"></param>
    private void UpdateEntryDefaultValue(ISettingsProvider settings, string section, string entry, object fromDefaultValue, object toDefaultValue)
    {

      try
      {
        var currentEntryValue = settings.GetValue(section, entry);

        //  If the entry's current value is not the default, skip
        if (currentEntryValue == null || !currentEntryValue.Equals(fromDefaultValue))
          return;

        settings.SetValue(section, entry, toDefaultValue);

        Log.Info("(Settings upgrade) Updated default value of entry " + section + "/" + entry + " from " + fromDefaultValue + "->" + toDefaultValue + "");
      }
      catch(Exception ex)
      {
        Log.Error("(Settings upgrade) Unhandled exception when trying to update default value for entry " + section + "/" + entry + "\r\n\r\n" + ex.ToString());
      }
    }

    #endregion
  }
}
