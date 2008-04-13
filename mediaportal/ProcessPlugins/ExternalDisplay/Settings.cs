#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using ExternalDisplay.Setting;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

using ProcessPlugins.ExternalDisplay.Drivers;
using ProcessPlugins.ExternalDisplay.Setting;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This class manages the settings of the External Display plugin
  /// </summary>
  /// <remarks>
  /// This class implements the Singleton pattern.
  /// Use the static <see cref="Settings.Instance"/> property, to reference the single instance of
  /// this class.
  /// </remarks>
  /// <author>JoeDalton</author>
  [Serializable]
  public class Settings
  {
    #region Static Fields

    private static Settings m_Instance; //Reference to the single instance of this task

    #endregion

    #region Fields

    /// <summary>
    /// String representation of the selected display type.  Used for (de)serializing
    /// </summary>
    [XmlAttribute] public string Type = null;


    private bool m_ShowPropertyBrowser = false;

    private string m_Port = "378"; //LPT1:0x378 LPT2:0x278 LPT3:3BC LPT4:178

    private int m_TextWidth = 16;

    private int m_TextHeight = 2;

    private int m_GraphicWidth = 320;

    private int m_GraphicHeight = 240;

    private int m_TextComDelay = 1;

    private int m_GraphicComDelay = 1;

    private bool m_BackLight = false;

    private int m_Contrast = 127;

    //        private string m_FontFile = "f8x11.fnt";
    //        [XmlAttribute]
    //        [DefaultValue("")]
    //        public string FontFile
    //        {
    //            get
    //            {
    //                if (m_Type!=DisplayType.SED133x)
    //                    return "";
    //                return m_FontFile;
    //            }
    //            set { m_FontFile = value; }
    //        }
    //

    private int m_ScrollDelay = 300;

    private int m_CharsToScroll = 1;

    private bool m_ExtensiveLogging = false;

    private string m_Font = "Arial Black";

    private int m_FontSize = 10;

    private bool m_ForceGraphicText = false;

    private int m_PixelsToScroll = 10;

    /// <summary>
    /// List of message rules
    /// </summary>
    [XmlElement("Message", typeof (Message))] public List<Message> Messages = new List<Message>();

    private List<IDisplay> m_Drivers = null;

    private string[] m_TranslateFrom;

    private string[] m_TranslateTo;

    private int[][] m_CustomCharacters = new int[0][];

    #endregion

    #region Properties

    /// <summary>
    /// Gets the single instance
    /// </summary>
    /// <value>The single instance of this class</value>
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
    }

    /// <summary>
    /// The selected display type
    /// </summary>
    [XmlIgnore]
    public IDisplay LCDType //Has to be a property, in order to be bindable to a control
    {
      get
      {
        if (ExtensiveLogging)
        {
          Log.Debug("ExternalDisplay: Determining configured display type...");
        }
        if (Type == null) //If no type selected, take the first one in the list
        {
          if (ExtensiveLogging)
          {
            Log.Debug("ExternalDisplay: Requested type was NULL.  Returning first type found...");
          }
          return Drivers[0];
        }
        if (ExtensiveLogging)
        {
          Log.Debug("ExternalDisplay: Searching type {0}...", Type);
        }
        foreach (IDisplay disp in Drivers) //otherwise get the instance with the correct name
        {
          if (string.Compare(disp.Name, Type, true, CultureInfo.InvariantCulture) == 0)
          {
            if (ExtensiveLogging)
            {
              Log.Debug("ExternalDisplay: Requested type was found.");
            }
            return disp;
          }
        }
        if (ExtensiveLogging)
        {
          Log.Error("ExternalDisplay: Requested type {0} not found.", Type);
        }
        return null;
      }
      set { Type = value.Name; }
    }

    /// <summary>
    /// Show the PropertyBrowser?
    /// </summary>
    [XmlAttribute]
    public bool ShowPropertyBrowser //Has to be a property, in order to be bindable to a control
    {
      get { return m_ShowPropertyBrowser; }
      set { m_ShowPropertyBrowser = value; }
    }

    /// <summary>
    /// The port the display is attached to
    /// </summary>
    [XmlAttribute]
    public string Port //Has to be a property, in order to be bindable to a control
    {
      get { return m_Port; }
      set { m_Port = value; }
    }

    [XmlIgnore]
    public string GUIPort
    {
      get
      {
        string p = Port;
        switch (p)
        {
          case "378":
            return "LPT1";
          case "278":
            return "LPT2";
          case "3BC":
            return "LPT3";
          case "178":
            return "LPT4";
          default:
            return p;
        }
      }
      set
      {
        switch (value)
        {
          case "LPT1":
            m_Port = "378";
            break;
          case "LPT2":
            m_Port = "278";
            break;
          case "LPT3":
            m_Port = "3BC";
            break;
          case "LPT4":
            m_Port = "178";
            break;
          default:
            m_Port = value;
            break;
        }
      }
    }

    /// <summary>
    /// Number of columns of display in text mode
    /// </summary>
    [XmlAttribute]
    public int TextWidth
    {
      get { return m_TextWidth; }
      set { m_TextWidth = value; }
    }

    /// <summary>
    /// Number of rows of display in text mode
    /// </summary>
    [XmlAttribute]
    public int TextHeight
    {
      get { return m_TextHeight; }
      set { m_TextHeight = value; }
    }

    /// <summary>
    /// Display width in pixels in graphic mode
    /// </summary>
    [XmlAttribute]
    public int GraphicWidth
    {
      get { return m_GraphicWidth; }
      set { m_GraphicWidth = value; }
    }

    /// <summary>
    /// Display height in pixels in graphic mode
    /// </summary>
    [XmlAttribute]
    public int GraphicHeight
    {
      get { return m_GraphicHeight; }
      set { m_GraphicHeight = value; }
    }

    /// <summary>
    /// Communication delay in text mode
    /// </summary>
    [XmlAttribute]
    public int TextComDelay
    {
      get { return m_TextComDelay; }
      set { m_TextComDelay = value; }
    }

    /// <summary>
    /// Communication delay in graphic mode
    /// </summary>
    [XmlAttribute]
    public int GraphicComDelay
    {
      get { return m_GraphicComDelay; }
      set { m_GraphicComDelay = value; }
    }

    /// <summary>
    /// Backlight on?
    /// </summary>
    [XmlAttribute]
    public bool BackLight
    {
      get { return m_BackLight; }
      set { m_BackLight = value; }
    }

    /// <summary>
    /// Contrast
    /// </summary>
    [XmlAttribute]
    public int Contrast
    {
      get { return m_Contrast; }
      set { m_Contrast = value; }
    }

    /// <summary>
    /// Scrolling delay
    /// </summary>
    [XmlAttribute]
    public int ScrollDelay
    {
      get { return m_ScrollDelay; }
      set { m_ScrollDelay = value; }
    }

    [XmlAttribute]
    public int CharsToScroll
    {
      get { return m_CharsToScroll; }
      set { m_CharsToScroll = value; }
    }

    [XmlAttribute]
    public bool ExtensiveLogging
    {
      get { return m_ExtensiveLogging; }
      set { m_ExtensiveLogging = value; }
    }

    [XmlAttribute]
    public string Font
    {
      get { return m_Font; }
      set
      {
        Font font = new Font(value, FontSize);
        m_Font = font.Name;
        font.Dispose();
      }
    }

    [XmlAttribute]
    public int FontSize
    {
      get { return m_FontSize; }
      set
      {
        Font font = new Font(Font, value);
        font.Dispose();
        m_FontSize = value;
      }
    }

    [XmlAttribute]
    public bool ForceGraphicText
    {
      get
      {
        if (!LCDType.SupportsGraphics)
          return false;
        return m_ForceGraphicText;
      }
      set { m_ForceGraphicText = value; }
    }

    [XmlAttribute]
    public int PixelsToScroll
    {
      get { return m_PixelsToScroll; }
      set { m_PixelsToScroll = value; }
    }

    /// <summary>
    /// List of display drivers
    /// </summary>
    [XmlIgnore]
    public List<IDisplay> Drivers
    {
      get
      {
        if (m_Drivers == null)
        {
          LoadDrivers();
        }
        return m_Drivers;
      }
    }

    [XmlArray]
    public string[] TranslateFrom
    {
      get { return m_TranslateFrom; }
      set { m_TranslateFrom = value; }
    }

    [XmlArray]
    public string[] TranslateTo
    {
      get { return m_TranslateTo; }
      set { m_TranslateTo = value; }
    }

    [XmlArray]
    [XmlArrayItem("CustomCharacter")]
    public int[][] CustomCharacters
    {
      get { return m_CustomCharacters; }
      set { m_CustomCharacters = value; }
    }

    #endregion

    public static void Reset()
    {
      m_Instance = Load();
    }

    /// <summary>
    /// Loads the settings from XML
    /// </summary>
    /// <returns>The loaded settings</returns>
    private static Settings Load()
    {
      Settings settings;
      if (File.Exists(Config.GetFile(Config.Dir.Config, "ExternalDisplay.xml")))
      {
        XmlSerializer ser = new XmlSerializer(typeof (Settings));
        XmlTextReader rdr = new XmlTextReader(Config.GetFile(Config.Dir.Config, "ExternalDisplay.xml"));
        settings = (Settings) ser.Deserialize(rdr);
        rdr.Close();
        return settings;
      }
      else
      {
        settings = new Settings();
        Default(settings);
      }
      return settings;
    }

    /// <summary>
    /// Saves the settings to XML
    /// </summary>
    public static void Save()
    {
      XmlSerializer ser = new XmlSerializer(typeof (Settings));
      XmlTextWriter w = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "ExternalDisplay.xml"), Encoding.UTF8);
      w.Formatting = Formatting.Indented;
      w.Indentation = 2;
      ser.Serialize(w, Instance);
      w.Close();
    }

    /// <summary>
    /// Creates wrapper classes for-, and loads all display drivers
    /// </summary>
    private void LoadDrivers()
    {
      Log.Info("ExternalDisplay: Loading drivers...");
      List<IDisplay> list = new List<IDisplay>();
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading DebugForm...");
      }
      list.Add(new DebugForm());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading VLSYSLis2...");
      }
      list.Add(new VLSYSLis2()); // Added by Nopap
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading iMON...");
      }
      list.Add(new iMON());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading iMONLCD Graphics...");
      }
      list.Add(new iMONLCDg());   // added by ralphy
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading ClipBoard...");
      }
      list.Add(new Clipboard());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading Girder...");
      }
      list.Add(new Girder());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading MediaPad...");
      }
      list.Add(new MediaPad());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading PropertySetter...");
      }
      list.Add(new None());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading CrystalFontz634...");
      }
      list.Add(new CrystalFontz634());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading HD44780JD...");
      }
      list.Add(new HD44780());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading T6963CJD...");
      }
      list.Add(new T6963C());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading NoritakeGU7000...");
      }
      list.Add(new NoritakeGU7000());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading ScaleoEV...");
      }
      list.Add(new ScaleoEV());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading DM-140GINK Demo...");
      }
      list.Add(new DM140GINK());
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading MatrixMX...");
      }
      list.Add(new MatrixMX());   // added by cybrmage
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading MatrixGX...");
      }
      list.Add(new MatrixGX());   // added by cybrmage
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Loading FICSpectra...");
      }
      list.Add(new FICSpectra());
      DirectoryInfo dinfo = new DirectoryInfo(Config.GetSubFolder(Config.Dir.Plugins, @"process\LCDDrivers"));
      if (!dinfo.Exists)
      {
        dinfo.Create();
      }
      foreach (FileInfo fi in dinfo.GetFiles("*.dll"))
      {
        if (ExtensiveLogging)
        {
          Log.Debug("ExternalDisplay: Loading {0}...", fi.FullName);
        }
        list.Add(new LCDHypeWrapper(fi.FullName));
      }
      m_Drivers = list;
      if (ExtensiveLogging)
      {
        Log.Debug("ExternalDisplay: Driver loading complete...");
      }
    }

    /// <summary>
    /// Creates the default settings when config file cannot be found
    /// </summary>
    /// <param name="_settings"></param>
    private static void Default(Settings _settings)
    {
      _settings.TranslateFrom = new string[] {"©", "®"};
      _settings.TranslateTo = new string[] {"(c)", "(R)"};

      // Data in ASCII code to generate custom char #0 and #1 (5x8)
      _settings.CustomCharacters = new int[][]
        {
          new int[]
            {
              0x0C, //____##__
              0x1E, //___####_
              0x1E, //___####_
              0x1E, //___####_
              0x1E, //___####_
              0x1E, //___####_
              0x1E, //___####_
              0x0C //____##__
            },
          new int[]
            {
              0x66, //_##__##_
              0xFF, //########
              0xFF, //########
              0xF6, //####_##_
              0xF0, //####____
              0xF0, //####____
              0xF0, //####____
              0x60 //_##_____
            }
        };


      Message msg;
      Line line;
      //
      // Idle
      //
      msg = new Message();
      msg.Status = Status.Idle;
      //do not use custom characters as default, as iMon displays don't like them
      msg.Lines.Add(new Line(new Text("MediaPortal"), Alignment.Centered));
      msg.Lines.Add(new Line(new Property("#time"), Alignment.Centered));
      //msg.Images.Add(new Image(0, 0, @".\Thumbs\ExternalDisplay\MPLogoText160x128BW.bmp"));
      _settings.Messages.Add(msg);
      //
      // Key pressed in TV
      //
      msg = new Message();
      msg.Status = Status.Action;
      msg.Windows.Add((int) GUIWindow.Window.WINDOW_TV);
      msg.Lines.Add(new Line(new Property("#currentmodule")));
      line = new Line();
      line.values.Add(new Property("#TV.View.channel"));
      line.values.Add(new Parse(": #TV.View.title", new NotNullCondition("#TV.View.title")));
      line.values.Add(new Text(": #736", new IsNullCondition("#TV.View.title")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Key pressed in Tetris
      //
      msg = new Message();
      msg.Status = Status.Action;
      msg.Windows.Add(19000); //Tetris
      msg.Lines.Add(new Line("#currentmodule, Level: #tetris_level"));
      msg.Lines.Add(new Line("Score: #tetris_score (#tetris_lines lines)"));
      _settings.Messages.Add(msg);
      //
      //Key pressed
      //
      msg = new Message();
      msg.Status = Status.Action;
      msg.Lines.Add(new Line(new Property("#currentmodule")));
      line = new Line();
      line.values.Add(new Property("#highlightedbutton"));
      line.values.Add(new Property("#selecteditem", new IsNullCondition("#highlightedbutton")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Playing Music
      //
      msg = new Message();
      msg.Status = Status.PlayingMusic;
      line = new Line();
      line.values.Add(new Property("#Play.Current.Title"));
      line.values.Add(new Parse(" by #Play.Current.Artist", new NotNullCondition("#Play.Current.Artist")));
      line.values.Add(
        new Parse(", from the album #Play.Current.Album", new NotNullCondition("#Play.Current.Album")));
      msg.Lines.Add(line);
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Playing Radio
      //
      msg = new Message();
      msg.Status = Status.PlayingRadio;
      line = new Line();
      line.values.Add(new Property("#Play.Current.Title"));
      msg.Lines.Add(line);
      line = new Line();
      line.values.Add(new Parse("#currentplaytime"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Playing TV
      //
      msg = new Message();
      msg.Status = Status.PlayingTV;
      msg.Lines.Add(new Line(new Property("#TV.View.channel")));
      line = new Line();
      line.values.Add(
        new Parse("#TV.View.title (#TV.View.start->#TV.View.stop)", new NotNullCondition("#TV.View.title")));
      line.values.Add(new Text(": #736", new IsNullCondition("#TV.View.title")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Playing Recording
      //
      msg = new Message();
      msg.Status = Status.PlayingRecording;
      msg.Lines.Add(new Line(new Property("#Play.Current.Title")));
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Timeshifting
      //
      msg = new Message();
      msg.Status = Status.Timeshifting;
      msg.Lines.Add(new Line(new Property("#TV.View.channel")));
      line = new Line();
      line.values.Add(
        new Parse("#TV.View.title (#TV.View.start->#TV.View.stop)", new NotNullCondition("#TV.View.title")));
      line.values.Add(
        new Text(" (#112)",
                 new AndCondition(new NotNullCondition("#paused"), new NotNullCondition("#TV.View.title"))));
      line.values.Add(new Text(": #736", new IsNullCondition("#TV.View.title")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Playing Video
      //
      msg = new Message();
      msg.Status = Status.PlayingVideo;
      msg.Lines.Add(new Line(new Property("#Play.Current.Title")));
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Playing DVD
      //
      msg = new Message();
      msg.Status = Status.PlayingDVD;
      msg.Lines.Add(new Line(new Text("DVD")));
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // TV Guide
      //
      msg = new Message();
      msg.Status = Status.Action;
      msg.Lines.Add(new Line("TV Programs on ##TV.Guide.Day"));
      _settings.Messages.Add(msg);
      //
      // Message to be shown in all other cases
      //
      msg = new Message();
      msg.Lines.Add(new Line(new Property("#currentmodule")));
      msg.Lines.Add(new Line(new Property("#selecteditem")));
      _settings.Messages.Add(msg);
    }
  }
}