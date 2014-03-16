#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Xml.Linq;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;
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
    private static readonly SettingsUpgradeManager UpgradeManagerInstance = new SettingsUpgradeManager();

    #endregion

    #region Properties

    /// <summary>
    /// Gets the singleton instance
    /// </summary>
    internal static SettingsUpgradeManager Instance
    {
      get { return UpgradeManagerInstance; }
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
      RemoveEntry(settings, "plugins", "Audioscrobbler");
      RemoveEntry(settings, "plugins", "Last.fm Radio");
      RemoveEntry(settings, "home", "Burner");
      RemoveEntry(settings, "home", "VideoEditor");
      RemoveEntry(settings, "home", "Last.fm Radio");
      RemoveEntry(settings, "myplugins", "Burner");
      RemoveEntry(settings, "myplugins", "VideoEditor");
      RemoveEntry(settings, "myplugins", "Last.fm Radio");
      RemoveEntry(settings, "pluginswindows", "MediaPortal.GUI.GUIBurner.GUIBurner");
      RemoveEntry(settings, "pluginswindows", "WindowPlugins.VideoEditor.GUIVideoEditor");
      RemoveEntry(settings, "pluginswindows", "MediaPortal.GUI.RADIOLASTFM.GUIRadioLastFM");
      RemoveEntry(settings, "musicmisc", "playnowjumpto");
      RemoveEntry(settings, "gui", "autosize");
      RemoveEntry(settings, "debug", "useS3Hack");
      RemoveEntry(settings, "general", "enables3trick");
      RemoveEntry(settings, "general", "turnmonitoronafterresume");
      RemoveEntry(settings, "general", "restartonresume");
      RemoveEntry(settings, "audioplayer", "player");
      RemoveEntry(settings, "audioplayer", "asio");
      RemoveEntry(settings, "audioplayer", "asiodevice");
      RemoveEntry(settings, "audioplayer", "mixing");
      RemoveEntry(settings, "screenselector", "usescreenselector");
      RemoveEntry(settings, "audioscrobbler", "user");
      RemoveEntry(settings, "audioscrobbler", "usesimilarrandom");
      RemoveEntry(settings, "audioscrobbler", "EnableNowPlaying");
      RemoveEntry(settings, "audioscrobbler", "showtrayicon");
      RemoveEntry(settings, "audioscrobbler", "showballontips");
      RemoveEntry(settings, "audioscrobbler", "submitradiotracks");
      RemoveEntry(settings, "audioscrobbler", "directskip");
      RemoveEntry(settings, "audioscrobbler", "listentrycount");
      RemoveEntry(settings, "audioscrobbler", "streamplayertype");
      RemoveEntry(settings, "audioscrobbler", "oneclickstart");
      RemoveEntry(settings, "audioscrobbler", "usesmskeyboard");
      RemoveEntry(settings, "musicmisc", "fetchlastfmcovers");
      RemoveEntry(settings, "musicmisc", "fetchlastfmtopalbums");
      RemoveEntry(settings, "musicmisc", "lookupSimilarTracks");
      RemoveEntry(settings, "musicmisc", "switchArtistOnLastFMSubmit");
      RemoveEntry(settings, "musicfiles", "autoshuffle");

      // Moved entries
      MoveEntry(settings, "general", "gui", "mousesupport");
      MoveEntry(settings, "general", "gui", "hideextensions");
      MoveEntry(settings, "general", "gui", "allowRememberLastFocusedItem");
      MoveEntry(settings, "general", "gui", "myprefix");
      MoveEntry(settings, "general", "gui", "startbasichome");
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

      ApplyDeploySettingUpgrade(settings);

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
      string[] skins34 = { "Blue3", "Blue3Wide", "Blue4", "Blue4Wide" };
      foreach (string skin in skins34)
      {
        if (Directory.Exists(skinbase + skin))
        {
          Log.Info("Deleting old skin \"" + skinbase + skin + "\"...");
          Directory.Delete(skinbase + skin, true);
        }
      }
    }

    /// <summary>
    /// Checks for the existance of deploy.xml and if it exists it applies the settings to mediaportal.xml
    /// </summary>
    /// <param name="settings"></param>
    internal void ApplyDeploySettingUpgrade(ISettingsProvider settings)
    {

      var deployFile = Config.GetFile(Config.Dir.Config, "deploy.xml");
      if (!File.Exists(deployFile)) return;

      try
      {
        var deployXml = XDocument.Load(deployFile);
        foreach (var deployElement in deployXml.Elements("deploySettings").Elements("deploySetting"))
        {
          try
          {
            var section = (string) deployElement.Attribute("section");
            var entry = (string) deployElement.Attribute("entry");
            var value = (string) deployElement;
            if (!string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(entry))
            {
              Log.Info("Apply Deploy Setting: {0} - {1} to: {2}", section, entry, value);
            }
            settings.SetValue(section, entry, value);
          }
          catch (Exception ex)
          {
            Log.Error("Issue applying value from deploy.xml");
            Log.Error(deployElement.ToString());
            Log.Error(ex);
          }
        }
        settings.Save();
      }
      catch (Exception ex)
      {
        Log.Error("Error applying updates from deploy.xml");
        Log.Error(ex);
      }
    }

    /// <summary>
    /// Checks for the existance of deploy.xml and if it exists it applies the settings to mediaportal.xml
    /// </summary>
    internal void ApplyDeploySetting()
    {
      var deployFile = Config.GetFile(Config.Dir.Config, "deploy.xml");
      if (!File.Exists(deployFile)) return;

      using(var settings = new MPSettings())
      {
        var deployXml = XDocument.Load(deployFile);
        foreach (var deployElement in deployXml.Elements("deploySettings").Elements("deploySetting"))
        {
          try
          {
            var section = (string)deployElement.Attribute("section");
            var entry = (string)deployElement.Attribute("entry");
            var value = (string)deployElement;
            if (!string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(entry))
            {
              Log.Info("Apply Deploy Setting: {0} - {1} to: {2}", section, entry, value);
            }
            settings.SetValue(section, entry, value);
          }
          catch (Exception ex)
          {
            Log.Error("Issue applying value from deploy.xml");
            Log.Error(deployElement.ToString());
            Log.Error(ex);
          }
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
        Log.Error("(Settings upgrade) Unhandled exception when trying to remove entry " + section + "/" + entry + "\r\n\r\n" + ex);
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
        Log.Error("(Settings upgrade) Unhandled exception when trying to move entry " + entry + " from section " + fromSection + "->" + toSection + "\r\n\r\n" + ex);
      }
    }

    /// <summary>
    /// Updates an entries default value if is as expected
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="section"></param>
    /// <param name="entry"></param>
    /// <param name="fromDefaultValue"></param>
    /// <param name="toDefaultValue"></param>
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
        Log.Error("(Settings upgrade) Unhandled exception when trying to update default value for entry " + section + "/" + entry + "\r\n\r\n" + ex);
      }
    }

    /// <summary>
    /// Create a zip file from a directory
    /// </summary>
    /// <param name="skinbase"></param>
    /// <param name="skin"></param>
    private void ZipDirectory(string skinbase, string skin)
    {
      string zipFile = skinbase + "Old-" + skin + "-" + DateTime.Now.ToString("dd_MM_yy") + ".zip";
      var archiver = new Archiver();
      archiver.AddDirectory(skinbase + skin, zipFile, true);
    }

    #endregion
  }
}
