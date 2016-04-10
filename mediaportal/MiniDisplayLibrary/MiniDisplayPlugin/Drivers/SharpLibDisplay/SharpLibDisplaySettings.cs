using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers.SharpLibDisplay
{

  /// <summary>
  /// Depends of combo box index
  /// </summary>
  public static class SingleLineMode
  {
    public const int TopLineOnly = 0;
    public const int BottomLineOnly = 1;
    public const int TopAndBottomLines = 2;
    public const int BottomAndTopLines = 3;
  }

  [Serializable]
  public class Settings
  {
    public Settings()
    {
      Default(this);
    }

    //Layout properties
    [XmlAttribute]
    public bool SingleLine { get; set; }

    [XmlAttribute]
    public int SingleLineMode { get; set; }

    [XmlAttribute]
    public string SingleLineSeparator { get; set; }

    #region Delegates

    public delegate void OnSettingsChangedHandler();

    #endregion

    private static Settings m_Instance;
    public const string m_Filename = "MiniDisplay_SharpLibDisplay.xml";

    public static Settings Instance
    {
      get
      {
        if (m_Instance == null)
        {
          m_Instance = Load();
        }
        return m_Instance;
      }
      set { m_Instance = value; }
    }



    public static event OnSettingsChangedHandler OnSettingsChanged;

    //Load default settings in the given instance
    private static void Default(Settings aSettings)
    {

      //Layout properties
      aSettings.SingleLine = false;
      aSettings.SingleLineMode = MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers.SharpLibDisplay.SingleLineMode.BottomLineOnly;
      aSettings.SingleLineSeparator = " - ";
    }

    public static Settings Load()
    {
      Settings settings=new Settings();      
      SoundGraphDisplay.LogDebug("SharpLibDisplay.Settings.Load(): started");
      if (File.Exists(Config.GetFile(Config.Dir.Config, m_Filename)))
      {
        SoundGraphDisplay.LogDebug("SharpLibDisplay.Settings.Load(): Loading settings from XML file");
        var serializer = new XmlSerializer(typeof(Settings));
        var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, m_Filename));
        settings = (Settings)serializer.Deserialize(xmlReader);
        xmlReader.Close();
      }
      else
      {
        SoundGraphDisplay.LogDebug("SharpLibDisplay.Settings.Load(): using default settings");
      }


      SoundGraphDisplay.LogDebug("SharpLibDisplay.Settings.Load(): completed");
      return settings;
    }

    public static void NotifyDriver()
    {
      if (OnSettingsChanged != null)
      {
        OnSettingsChanged();
      }
    }

    public static void Save()
    {
      Save(Instance);
    }

    public static void Save(Settings ToSave)
    {
      var serializer = new XmlSerializer(typeof(Settings));
      var writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, m_Filename),
                                     Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 2 };
      serializer.Serialize(writer, ToSave);
      writer.Close();
    }

    public static void SetDefaults()
    {
      Default(Instance);
    }
  }
}
