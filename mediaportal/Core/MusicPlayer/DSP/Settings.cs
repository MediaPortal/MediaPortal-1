#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;

namespace MediaPortal.Player.DSP
{
  /// <summary>
  /// Manage the Settings of DSP effects used by the BASS audio engine
  /// </summary>
  [Serializable]
  public class Settings
  {
    #region Enums

    private enum AmplificationPreset
    {
      Soft = 0,
      Medium = 1,
      Hard = 2
    }

    #endregion

    #region Variables

    // Private Variables
    private string m_MusicDirectory = "C:\\";

    // Protected Variables

    // Public Variables

    #endregion

    #region Singleton implementation

    private static Settings m_Instance;

    /// <summary>
    /// Retrieve single instance
    /// </summary>
    /// <value>Single instance of this class</value>
    public static Settings Instance
    {
      get
      {
        if (m_Instance == null)
        {
          m_Instance = LoadSettings();
        }
        return m_Instance;
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Initial Music Directory
    /// </summary>
    [XmlAttribute]
    public string MusicDirectory
    {
      get { return m_MusicDirectory; }
      set { m_MusicDirectory = value; }
    }

    /// <summary>
    /// List of Bass Effects
    /// </summary>
    [XmlElement("BASS", typeof (BassEffect))] public List<BassEffect> BassEffects = new List<BassEffect>();

    /// <summary>
    /// List of enabled VST Plugins
    /// </summary>
    [XmlElement("VST", typeof (VSTPlugin))] public List<VSTPlugin> VSTPlugins = new List<VSTPlugin>();

    /// <summary>
    /// List of enabled WinAMp Plugins
    /// </summary>
    [XmlElement("Winamp", typeof (WinAmpPlugin))] public List<WinAmpPlugin> WinAmpPlugins = new List<WinAmpPlugin>();

    #endregion

    #region Serializing / Deserializing

    /// <summary>
    /// Load settings from XML file
    /// </summary>
    /// <returns>The settings loaded from file</returns>
    private static Settings LoadSettings()
    {
      Settings settings;
      if (File.Exists(Config.GetFile(Config.Dir.Config, "musicdsp.xml")))
      {
        XmlSerializer serializer = new XmlSerializer(typeof (Settings));
        XmlTextReader reader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "musicdsp.xml"));
        settings = (Settings)serializer.Deserialize(reader);
        reader.Close();
        return settings;
      }
      else
      {
        settings = new Settings();
        //Default(settings);
      }
      return settings;
    }

    /// <summary>
    /// Saves the settings to the XML
    /// </summary>
    public static void SaveSettings()
    {
      XmlSerializer serializer = new XmlSerializer(typeof (Settings));
      XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "musicdsp.xml"), Encoding.UTF8);
      writer.Formatting = Formatting.Indented;
      writer.Indentation = 2;
      serializer.Serialize(writer, Instance);
      writer.Close();
    }

    #endregion
  }
}