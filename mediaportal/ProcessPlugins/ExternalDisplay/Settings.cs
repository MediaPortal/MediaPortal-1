/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ExternalDisplay.Setting;
using MediaPortal.GUI.Library;
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
  public class Settings
  {
    #region Singleton implementation

    private static Settings m_Instance; //Reference to the single instance of this task
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

    #endregion

    #region Lifecycle

    /// <summary>
    /// Constructor
    /// </summary>
    /// <remarks>
    /// Never call this method yourself!!!  Please use the static <see cref="Instance"/ property to get a
    /// reference to the single instance of this class>
    /// </remarks>
    public Settings() //Has to be public for XmlSerializer
    {}

    #endregion

    #region Properties

    [XmlAttribute] public string Type = null; //String representation of the selected display type.  Used for (de)serializing

    /// <summary>
    /// The selected display type
    /// </summary>
    [XmlIgnore]
    public IDisplay LCDType //Has to be a property, in order to be bindable to a control
    {
      get
      {
        if (ExtensiveLogging) Log.Write("ExternalDisplay: Determining configured display type...");
        if (Type == null) //If no type selected, take the first one in the list
        {
          if (ExtensiveLogging) Log.Write("ExternalDisplay: Requested type was NULL.  Returning first type found...");
          return Drivers[0];
        }
        if (ExtensiveLogging) Log.Write("ExternalDisplay: Searching type {0}...",Type);
        foreach (IDisplay disp in Drivers) //otherwise get the instance with the correct name
        {
          if (string.Compare(disp.Name,Type,true,CultureInfo.InvariantCulture)==0)
          {
            if (ExtensiveLogging) Log.Write("ExternalDisplay: Requested type was found.");
            return disp;
          }
        }
        if (ExtensiveLogging) Log.Write("ExternalDisplay: Requested type not found.");
        return null;
      }
      set { Type = value.Name; }
    }

    private bool m_ShowPropertyBrowser = false;

    /// <summary>
    /// Show the PropertyBrowser?
    /// </summary>
    [XmlAttribute]
    public bool ShowPropertyBrowser //Has to be a property, in order to be bindable to a control
    {
      get { return m_ShowPropertyBrowser; }
      set { m_ShowPropertyBrowser = value; }
    }

    private string m_Port = "378"; //LPT1:0x378 LPT2:0x278 LPT3:3BC LPT4:178
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
        string p = this.Port;
        switch(p)
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
        switch(value)
        {
          case "LPT1":
            m_Port="378";
            break;
          case "LPT2":
            m_Port="278";
            break;
          case "LPT3":
            m_Port="3BC";
            break;
          case "LPT4":
            m_Port="178";
            break;
          default:
            m_Port=value;
            break;
        }
      }
    }

    private int m_TextWidth = 16;

    /// <summary>
    /// Number of columns of display in text mode
    /// </summary>
    [XmlAttribute]
    public int TextWidth
    {
      get { return m_TextWidth; }
      set { m_TextWidth = value; }
    }

    private int m_TextHeight = 2;

    /// <summary>
    /// Number of rows of display in text mode
    /// </summary>
    [XmlAttribute]
    public int TextHeight
    {
      get { return m_TextHeight; }
      set { m_TextHeight = value; }
    }

    private int m_GraphicWidth = 320;

    /// <summary>
    /// Display width in pixels in graphic mode
    /// </summary>
    [XmlAttribute]
    public int GraphicWidth
    {
      get { return m_GraphicWidth; }
      set { m_GraphicWidth = value; }
    }

    private int m_GraphicHeight = 240;

    /// <summary>
    /// Display height in pixels in graphic mode
    /// </summary>
    [XmlAttribute]
    public int GraphicHeight
    {
      get { return m_GraphicHeight; }
      set { m_GraphicHeight = value; }
    }

    private int m_TextComDelay = 1;

    /// <summary>
    /// Communication delay in text mode
    /// </summary>
    [XmlAttribute]
    public int TextComDelay
    {
      get { return m_TextComDelay; }
      set { m_TextComDelay = value; }
    }

    private int m_GraphicComDelay = 1;

    /// <summary>
    /// Communication delay in graphic mode
    /// </summary>
    [XmlAttribute]
    public int GraphicComDelay
    {
      get { return m_GraphicComDelay; }
      set { m_GraphicComDelay = value; }
    }

    private bool m_BackLight = false;

    /// <summary>
    /// Backlight on?
    /// </summary>
    [XmlAttribute]
    public bool BackLight
    {
      get { return m_BackLight; }
      set { m_BackLight = value; }
    }

    private int m_Contrast = 127;
    
    /// <summary>
    /// Contrast
    /// </summary>
    [XmlAttribute]
    public int Contrast
    {
      get { return m_Contrast; }
      set { m_Contrast = value; }
    }

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
    /// <summary>
    /// Scrolling delay
    /// </summary>
    [XmlAttribute]
    public int ScrollDelay
    {
      get { return m_ScrollDelay; }
      set { m_ScrollDelay = value; }
    }


    private bool m_ExtensiveLogging = true;
    [XmlAttribute]
    public bool ExtensiveLogging
    {
      get
      {
        return m_ExtensiveLogging;
      }
      set
      {
        m_ExtensiveLogging=value;
      }
    }

    /// <summary>
    /// List of message rules
    /// </summary>
    [XmlElement("Message", typeof (Message))] public IList Messages = new ArrayList();

    private IDisplay[] m_Drivers = null;
    /// <summary>
    /// List of display drivers
    /// </summary>
    [XmlIgnore]
    public IDisplay[] Drivers
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

    private string[] m_TranslateFrom;
    [XmlArray]
    public string[] TranslateFrom
    {
      get
      {
        return m_TranslateFrom;
      }
      set
      {
        m_TranslateFrom = value;
      }
    }

    private string[] m_TranslateTo;
    [XmlArray]
    public string[] TranslateTo
    {
      get
      {
        return m_TranslateTo;
      }
      set
      {
        m_TranslateTo = value;
      }
    }


    #endregion

    #region (De)Serializing

    /// <summary>
    /// Loads the settings from XML
    /// </summary>
    /// <returns>The loaded settings</returns>
    private static Settings Load()
    {
      Settings settings = null;
      if (File.Exists("ExternalDisplay.xml"))
      {
        XmlSerializer ser = new XmlSerializer(typeof (Settings));
        XmlTextReader rdr = new XmlTextReader("ExternalDisplay.xml");
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
      XmlTextWriter w = new XmlTextWriter("ExternalDisplay.xml", Encoding.UTF8);
      w.Formatting = Formatting.Indented;
      w.Indentation = 2;
      ser.Serialize(w, Settings.Instance);
      w.Close();
    }

    #endregion

    /// <summary>
    /// Creates wrapper classes for-, and loads all display drivers
    /// </summary>
    private void LoadDrivers()
    {
      Log.Write("ExternalDisplay: Loading drivers...");
      ArrayList list = new ArrayList();
      if (ExtensiveLogging) Log.Write("ExternalDisplay: Loading DebugForm...");
      list.Add(new DebugForm());
      if (ExtensiveLogging) Log.Write("ExternalDisplay: Loading VLSYSLis2...");
      list.Add(new VLSYSLis2()); // Added by Nopap
      if (ExtensiveLogging) Log.Write("ExternalDisplay: Loading iMON...");
      list.Add(new iMON());
      if (ExtensiveLogging) Log.Write("ExternalDisplay: Loading ClipBoard...");
      list.Add(new Clipboard());
      if (ExtensiveLogging) Log.Write("ExternalDisplay: Loading Girder...");
      list.Add(new Girder());
      if (ExtensiveLogging) Log.Write("ExternalDisplay: Loading MediaPad...");
      list.Add(new MediaPad());
      if (ExtensiveLogging) Log.Write("ExternalDisplay: Loading PropertySetter...");
      list.Add(new None());
      DirectoryInfo dinfo = new DirectoryInfo(@"plugins\process\LCDDrivers");
      if (!dinfo.Exists)
      {
        dinfo.Create();
      }
      foreach (FileInfo fi in dinfo.GetFiles("*.dll"))
      {
        if (ExtensiveLogging) Log.Write("ExternalDisplay: Loading {0}...",fi.FullName);
        list.Add(new LCDHypeWrapper(fi.FullName));
      }
      m_Drivers = new IDisplay[list.Count];
      list.CopyTo(m_Drivers);
      if (ExtensiveLogging) Log.Write("ExternalDisplay: Driver loading complete...");
    }

    /// <summary>
    /// Creates the default settings when config file cannot be found
    /// </summary>
    /// <param name="_settings"></param>
    private static void Default(Settings _settings)
    {
      _settings.TranslateFrom = new string[] {"©","®"};
      _settings.TranslateTo = new string[]   {"(c)","(R)"};

      Message msg;
      Line line;
      //
      // Idle
      //
      msg = new Message();
      msg.Status = Status.Idle;
      msg.Lines.Add(new Line(new Text("MediaPortal"), Alignment.Centered));
      msg.Lines.Add(new Line(new Property("#time"), Alignment.Centered));
      _settings.Messages.Add(msg);
      //
      // Key pressed in TV
      //
      msg = new Message();
      msg.Status = Status.Action;
      msg.Windows.Add(GUIWindow.Window.WINDOW_TV);
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
      line.values.Add(new Parse(", from the album #Play.Current.Album", new NotNullCondition("#Play.Current.Album")));
      msg.Lines.Add(line);
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Playing Radio
      //
      //TODO: Add Radio settings
      //
      // Playing TV
      //
      msg = new Message();
      msg.Status = Status.PlayingTV;
      msg.Lines.Add(new Line(new Property("#TV.View.channel")));
      line = new Line();
      line.values.Add(new Parse("#TV.View.title (#TV.View.start->#TV.View.stop)", new NotNullCondition("#TV.View.title")));
      line.values.Add(new Text(": #736", new IsNullCondition("#TV.View.title")));
      msg.Lines.Add(line);
      _settings.Messages.Add(msg);
      //
      // Timeshifting
      //
      msg = new Message();
      msg.Status = Status.Timeshifting;
      msg.Lines.Add(new Line(new Property("#TV.View.channel")));
      line = new Line();
      line.values.Add(new Parse("#TV.View.title (#TV.View.start->#TV.View.stop)", new NotNullCondition("#TV.View.title")));
      line.values.Add(new Text(" (#112)", new AndCondition(new NotNullCondition("#paused"), new NotNullCondition("#TV.View.title"))));
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
      //TODO: Radio playing
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