using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.Support;
using System.IO;

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
      RemoveEntry(settings, "musicmisc", "playnowjumpto");

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

      //  blue3/wide and blue4/blue4wide are now default/wide
      UpdateEntryDefaultValue(settings, "skin", "name", "Blue3", "Default");
      UpdateEntryDefaultValue(settings, "skin", "name", "Blue3wide", "DefaultWide");
      UpdateEntryDefaultValue(settings, "skin", "name", "Blue4", "Default");
      UpdateEntryDefaultValue(settings, "skin", "name", "Blue4wide", "DefaultWide");

      //Mantis 3772 - Weather.com API is not free any more
      //temporarily disable plugin
      UpdateEntryDefaultValue(settings, "pluginswindows", "MediaPortal.GUI.Weather.GUIWindowWeather", "yes", "no");
      UpdateEntryDefaultValue(settings, "plugins", "weather", "yes", "no");

      settings.Save();

      string skinbase = Config.GetFolder(Config.Dir.Skin) + "\\";

      //Zip Blue3/Blue3Wide skin folders 
      string[] skins3 = { "Blue3", "Blue3Wide" };

      foreach (string skin in skins3)
      {
        if (Directory.Exists(skinbase + skin))
        {
          Log.Info("Adding skin \"" + skinbase + skin + "\" to zip...");
          ZipDirectory(skinbase, skin);
        }
      }

      // Delete beta Blue4/Blue4Wide and outdated Blue3/Blue3Wide folders
      string[] skins3_4 = { "Blue3", "Blue3Wide", "Blue4", "Blue4Wide" };
      foreach (string skin in skins3_4)
      {
        if (Directory.Exists(skinbase + skin))
        {
          Log.Info("Deleting old skin \"" + skinbase + skin + "\"...");
          Directory.Delete(skinbase + skin, true);
        }
      }
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
      catch (Exception ex)
      {
        Log.Error("(Settings upgrade) Unhandled exception when trying to update default value for entry " + section + "/" + entry + "\r\n\r\n" + ex.ToString());
      }
    }

    /// <summary>
    /// Create a zip file from a directory
    /// </summary>
    /// <param name="skinbase"></param>
    /// <param name="skin"></param>
    private void ZipDirectory(string skinbase, string skin)
    {
      string _zipFile = skinbase + "Old-" + skin + "-" + DateTime.Now.ToString("dd_MM_yy") + ".zip";
      Archiver archiver = new Archiver();
      archiver.AddDirectory(skinbase + skin, _zipFile, true);
    }

    #endregion
  }
}
