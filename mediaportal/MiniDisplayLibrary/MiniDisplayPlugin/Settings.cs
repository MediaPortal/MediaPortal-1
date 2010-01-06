using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  [Serializable]
  public class Settings
  {
    private int m_Backlight = 0x7f;
    private bool m_BackLightControl = true;
    private bool m_BlankOnExit;
    private int m_CharsToScroll = 1;
    private int m_Contrast = 0x7f;
    private bool m_ContrastControl = true;
    private int[][] m_CustomCharacters = new int[0][];
    private bool m_DisableGUISetup;
    private List<IDisplay> m_Drivers;
    private bool m_EnableLCDHype = true;
    private bool m_ExtensiveLogging;
    private string m_Font = "Arial Black";
    private int m_FontSize = 10;
    private bool m_ForceGraphicText;
    private int m_GraphicComDelay = 1;
    private int m_GraphicHeight = 0x10;
    private int m_GraphicWidth = 0x60;
    private string m_IdleMessage = string.Empty;
    private static Settings m_Instance;
    private int m_PixelsToScroll = 10;
    private string m_Port = "378";
    private string m_PrefixChar = string.Empty;
    private int m_ScrollDelay = 300;
    private bool m_ShowPropertyBrowser;
    private string m_Shutdown1 = string.Empty;
    private string m_Shutdown2 = string.Empty;
    private int m_TextComDelay = 1;
    private int m_TextHeight = 2;
    private int m_TextWidth = 0x10;
    private string[] m_TranslateFrom;
    private string[] m_TranslateTo;
    [XmlElement("Message", typeof (Message))] public List<Message> Messages = new List<Message>();
    [XmlAttribute] public string Type;

    private static void Default(Settings _settings)
    {
      _settings.DisableGUISetup = false;
      _settings.TranslateFrom = new string[]
                                  {
                                    "\x00a9", "\x00ae", "\x00e9", "\x00e8", "\x00ea", "\x00fc", "\x00e4", "\x00f6",
                                    "\x00dc", "\x00c4", "\x00d6", "\x00df"
                                  };
      _settings.TranslateTo = new string[] {"(c)", "(R)", "e", "e", "e", "ue", "ae", "oe", "Ue", "Ae", "Oe", "ss"};
      _settings.BackLightControl = false;
      _settings.Backlight = 0x7f;
      _settings.ContrastControl = false;
      _settings.Contrast = 0x7f;
      _settings.CustomCharacters = new int[][]
                                     {
                                       new int[] {12, 30, 30, 30, 30, 30, 30, 12},
                                       new int[] {0x66, 0xff, 0xff, 0xf6, 240, 240, 240, 0x60}
                                     };
      Message item = new Message();
      item.Status = Status.Idle;
      item.Lines.Add(new Line(new Text("MediaPortal"), Alignment.Centered));
      item.Lines.Add(new Line(new Property("#time"), Alignment.Centered));
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.Dialog;
      item.Lines.Add(new Line(new Property("#DialogLabel"), Alignment.Centered));
      item.Lines.Add(new Line(new Property("#DialogItem"), Alignment.Centered));
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.Action;
      item.Windows.Add(1);
      item.Lines.Add(new Line(new Property("#currentmodule")));
      Line line = new Line();
      line.values.Add(new Property("#TV.View.channel"));
      line.values.Add(new Parse(": #TV.View.title", new NotNullCondition("#TV.View.title")));
      line.values.Add(new Text(": #736", new IsNullCondition("#TV.View.title")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.Action;
      item.Windows.Add(0x4a38);
      item.Lines.Add(new Line("#currentmodule, Level: #tetris_level"));
      item.Lines.Add(new Line("Score: #tetris_score (#tetris_lines lines)"));
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.Action;
      item.Lines.Add(new Line(new Property("#currentmodule")));
      line = new Line();
      line.values.Add(new Property("#highlightedbutton"));
      line.values.Add(new Property("#selecteditem", new IsNullCondition("#highlightedbutton")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.PlayingMusic;
      line = new Line();
      line.values.Add(new Property("#Play.Current.Title"));
      line.values.Add(new Parse(" by #Play.Current.Artist", new NotNullCondition("#Play.Current.Artist")));
      line.values.Add(new Parse(", from the album #Play.Current.Album", new NotNullCondition("#Play.Current.Album")));
      item.Lines.Add(line);
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.PlayingRadio;
      line = new Line();
      line.values.Add(new Property("#Play.Current.Title"));
      item.Lines.Add(line);
      line = new Line();
      line.values.Add(new Parse("#currentplaytime"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.PlayingTV;
      item.Lines.Add(new Line(new Property("#TV.View.channel")));
      line = new Line();
      line.values.Add(new Parse("#TV.View.title (#TV.View.start->#TV.View.stop)", new NotNullCondition("#TV.View.title")));
      line.values.Add(new Text(": #736", new IsNullCondition("#TV.View.title")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.PlayingRecording;
      item.Lines.Add(new Line(new Property("#Play.Current.Title")));
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.Timeshifting;
      item.Lines.Add(new Line(new Property("#TV.View.channel")));
      line = new Line();
      line.values.Add(new Parse("#TV.View.title (#TV.View.start->#TV.View.stop)", new NotNullCondition("#TV.View.title")));
      line.values.Add(new Text(" (#112)",
                               new AndCondition(new Condition[]
                                                  {
                                                    new NotNullCondition("#paused"),
                                                    new NotNullCondition("#TV.View.title")
                                                  })));
      line.values.Add(new Text(": #736", new IsNullCondition("#TV.View.title")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.PlayingVideo;
      item.Lines.Add(new Line(new Property("#Play.Current.Title")));
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.PlayingDVD;
      item.Lines.Add(new Line(new Text("DVD")));
      line = new Line();
      line.values.Add(new Parse("#currentplaytime/#duration"));
      line.values.Add(new Text(" (#112)", new NotNullCondition("#paused")));
      item.Lines.Add(line);
      _settings.Messages.Add(item);
      item = new Message();
      item.Status = Status.Action;
      item.Lines.Add(new Line("TV Programs on ##TV.Guide.Day"));
      _settings.Messages.Add(item);
      item = new Message();
      item.Lines.Add(new Line(new Property("#currentmodule")));
      item.Lines.Add(new Line(new Property("#selecteditem")));
      _settings.Messages.Add(item);
    }

    private static string FindIdleMessage(Settings currentSettings)
    {
      foreach (Message message in currentSettings.Messages)
      {
        if ((message.Status == Status.Idle) && (message.Windows.Count == 0))
        {
          string str = string.Empty;
          for (int i = 0; i < message.Lines[0].values.Count; i++)
          {
            str = str + message.Lines[0].values[i].Evaluate();
          }
          return str.Trim();
        }
      }
      return string.Empty;
    }

    private static Settings Load()
    {
      Settings settings;
      if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay.xml")))
      {
        Log.Info("MiniDisplay.Settings.Load() - Loading settings from configuration file");
        XmlSerializer serializer = new XmlSerializer(typeof (Settings));
        XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay.xml"));
        settings = (Settings)serializer.Deserialize(xmlReader);
        xmlReader.Close();
        settings.IdleMessage = FindIdleMessage(settings);
        return settings;
      }
      Log.Info("MiniDisplay.Settings.Load() - Loading default settings");
      settings = new Settings();
      Default(settings);
      Log.Info("MiniDisplay.Settings.Load() - Loaded default settings");
      settings.IdleMessage = FindIdleMessage(settings);
      return settings;
    }

    private void LoadDrivers()
    {
      Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading drivers...");
      List<IDisplay> list = new List<IDisplay>();
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading xPL_Connector...");
      }
      list.Add(new xPL_Connector());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading IOWarrior...");
      }
      list.Add(new IOWarrior());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading GenericSerial...");
      }
      list.Add(new GenericSerial());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading MCEDisplay...");
      }
      list.Add(new MCEDisplay());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading CFontz...");
      }
      list.Add(new CFontz());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading iMONLCD Graphics...");
      }
      list.Add(new iMONLCDg());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading MatrixMX...");
      }
      list.Add(new MatrixMX());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading MatrixGX...");
      }
      list.Add(new MatrixGX());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading MD8800...");
      }
      list.Add(new MD8800());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading VLSYS_Mplay...");
      }
      list.Add(new VLSYS_Mplay());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading DM140GINK...");
      }
      list.Add(new DM140GINK());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading FICSpectra...");
      }
      list.Add(new FICSpectra());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading MediaPad...");
      }
      list.Add(new MediaPad());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading ScaleoEV...");
      }
      list.Add(new ScaleoEV());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading ShuttlePF27...");
      }
      list.Add(new ShuttlePF27());
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading Debug Display...");
      }
      list.Add(new DebugForm());
      if (this.m_EnableLCDHype)
      {
        DirectoryInfo info = new DirectoryInfo(Config.GetSubFolder(Config.Dir.Plugins, @"process\LCDDrivers"));
        if (!info.Exists)
        {
          info.Create();
        }
        foreach (FileInfo info2 in info.GetFiles("*.dll"))
        {
          if (this.ExtensiveLogging)
          {
            Log.Info("MiniDisplay.Settings.LoadDrivers(): Loading LCDHype Driver {0}...", new object[] {info2.FullName});
          }
          list.Add(new LCDHypeWrapper(info2.FullName));
        }
      }
      this.m_Drivers = list;
      if (this.ExtensiveLogging)
      {
        Log.Info("MiniDisplay.Settings.LoadDrivers(): Driver loading complete...");
      }
    }

    public void ReloadDrivers()
    {
      this.m_Drivers.Clear();
      this.LoadDrivers();
    }

    public static void Save()
    {
      try
      {
        XmlSerializer serializer = new XmlSerializer(typeof (Settings));
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay.xml")))
        {
          XmlDocument xmldoc = new XmlDocument();
          xmldoc.Load(Config.GetFile(Config.Dir.Config, "MiniDisplay.xml"));
          XmlNode node = xmldoc.SelectSingleNode("//Settings");
          Settings settings = (Settings)serializer.Deserialize(new XmlNodeReader(node));
          m_Instance.Messages = settings.Messages;
          m_Instance.TranslateFrom = settings.TranslateFrom;
          m_Instance.TranslateTo = settings.TranslateTo;
          m_Instance.CustomCharacters = settings.CustomCharacters;
        }
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay.xml"), Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        writer.IndentChar = ' ';
        writer.WriteStartDocument(true);
        serializer.Serialize((XmlWriter)writer, m_Instance);
        writer.Close();
      }
      catch (Exception ex)
      {
        Log.Error("MiniDisplay.Settings.Save() exception: {0}", ex.Message);
      }
    }

    [XmlAttribute]
    public int Backlight
    {
      get { return this.m_Backlight; }
      set { this.m_Backlight = value; }
    }

    [XmlAttribute]
    public bool BackLightControl
    {
      get { return this.m_BackLightControl; }
      set { this.m_BackLightControl = value; }
    }

    [XmlAttribute]
    public bool BlankOnExit
    {
      get { return this.m_BlankOnExit; }
      set { this.m_BlankOnExit = value; }
    }

    [XmlAttribute]
    public int CharsToScroll
    {
      get { return this.m_CharsToScroll; }
      set { this.m_CharsToScroll = value; }
    }

    [XmlAttribute]
    public int Contrast
    {
      get { return this.m_Contrast; }
      set { this.m_Contrast = value; }
    }

    [XmlAttribute]
    public bool ContrastControl
    {
      get { return this.m_ContrastControl; }
      set { this.m_ContrastControl = value; }
    }

    [XmlArrayItem("CustomCharacter"), XmlArray]
    public int[][] CustomCharacters
    {
      get { return this.m_CustomCharacters; }
      set { this.m_CustomCharacters = value; }
    }

    [XmlAttribute]
    public bool DisableGUISetup
    {
      get { return this.m_DisableGUISetup; }
      set { this.m_DisableGUISetup = value; }
    }

    [XmlIgnore]
    public List<IDisplay> Drivers
    {
      get
      {
        if (this.m_Drivers == null)
        {
          this.LoadDrivers();
        }
        return this.m_Drivers;
      }
    }

    [XmlAttribute]
    public bool EnableLCDHype
    {
      get { return this.m_EnableLCDHype; }
      set { this.m_EnableLCDHype = value; }
    }

    [XmlAttribute]
    public bool ExtensiveLogging
    {
      get { return this.m_ExtensiveLogging; }
      set { this.m_ExtensiveLogging = value; }
    }

    [XmlAttribute]
    public string Font
    {
      get { return this.m_Font; }
      set
      {
        Font font = new Font(value, (float)this.FontSize);
        this.m_Font = font.Name;
        font.Dispose();
      }
    }

    [XmlAttribute]
    public int FontSize
    {
      get { return this.m_FontSize; }
      set
      {
        new Font(this.Font, (float)value).Dispose();
        this.m_FontSize = value;
      }
    }

    [XmlAttribute]
    public bool ForceGraphicText
    {
      get
      {
        if (!this.LCDType.SupportsGraphics)
        {
          return false;
        }
        return this.m_ForceGraphicText;
      }
      set { this.m_ForceGraphicText = value; }
    }

    [XmlAttribute]
    public int GraphicComDelay
    {
      get { return this.m_GraphicComDelay; }
      set { this.m_GraphicComDelay = value; }
    }

    [XmlAttribute]
    public int GraphicHeight
    {
      get { return this.m_GraphicHeight; }
      set { this.m_GraphicHeight = value; }
    }

    [XmlAttribute]
    public int GraphicWidth
    {
      get { return this.m_GraphicWidth; }
      set { this.m_GraphicWidth = value; }
    }

    [XmlIgnore]
    public string GUIPort
    {
      get
      {
        string port = this.Port;
        string str2 = port;
        if (str2 == null)
        {
          return port;
        }
        if (!(str2 == "378"))
        {
          if (str2 == "278")
          {
            return "LPT2";
          }
          if (str2 == "3BC")
          {
            return "LPT3";
          }
          if (str2 == "178")
          {
            return "LPT4";
          }
          return port;
        }
        return "LPT1";
      }
      set
      {
        switch (value)
        {
          case "LPT1":
            this.m_Port = "378";
            return;

          case "LPT2":
            this.m_Port = "278";
            return;

          case "LPT3":
            this.m_Port = "3BC";
            return;

          case "LPT4":
            this.m_Port = "178";
            return;
        }
        this.m_Port = value;
      }
    }

    public string IdleMessage
    {
      get { return this.m_IdleMessage; }
      set { this.m_IdleMessage = value; }
    }

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

    [XmlIgnore]
    public IDisplay LCDType
    {
      get
      {
        if (this.ExtensiveLogging)
        {
          Log.Debug("MiniDisplay.Settings.LCDType: Determining configured display type...");
        }
        if (this.Type == null)
        {
          if (this.ExtensiveLogging)
          {
            Log.Debug(
              "MiniDisplay.Settings.LCDType: Completed - Requested type was NULL.  Returning first type found...",
              new object[0]);
          }
          return this.Drivers[0];
        }
        if (this.ExtensiveLogging)
        {
          Log.Info("MiniDisplay.Settings.LCDType: Configured for display type: {0}", new object[] {this.Type});
        }
        foreach (IDisplay display in this.Drivers)
        {
          if (string.Compare(display.Name, this.Type, true, CultureInfo.InvariantCulture) == 0)
          {
            if (this.ExtensiveLogging)
            {
              Log.Debug("MiniDisplay.Settings.LCDType: Completed - Requested type was found.");
            }
            return display;
          }
        }
        if (this.ExtensiveLogging)
        {
          Log.Error("MiniDisplay.Settings.LCDType: Confleted - Requested type {0} NOT FOUND.", new object[] {this.Type});
        }
        return this.Drivers[0];
      }
      set { this.Type = value.Name; }
    }

    [XmlAttribute]
    public int PixelsToScroll
    {
      get { return this.m_PixelsToScroll; }
      set { this.m_PixelsToScroll = value; }
    }

    [XmlAttribute]
    public string Port
    {
      get { return this.m_Port; }
      set { this.m_Port = value; }
    }

    [XmlAttribute]
    public string PrefixChar
    {
      get { return this.m_PrefixChar; }
      set { this.m_PrefixChar = value; }
    }

    [XmlAttribute]
    public int ScrollDelay
    {
      get { return this.m_ScrollDelay; }
      set { this.m_ScrollDelay = value; }
    }

    [XmlAttribute]
    public bool ShowPropertyBrowser
    {
      get { return this.m_ShowPropertyBrowser; }
      set { this.m_ShowPropertyBrowser = value; }
    }

    [XmlAttribute]
    public string Shutdown1
    {
      get { return this.m_Shutdown1; }
      set { this.m_Shutdown1 = value; }
    }

    [XmlAttribute]
    public string Shutdown2
    {
      get { return this.m_Shutdown2; }
      set { this.m_Shutdown2 = value; }
    }

    [XmlAttribute]
    public int TextComDelay
    {
      get { return this.m_TextComDelay; }
      set { this.m_TextComDelay = value; }
    }

    [XmlAttribute]
    public int TextHeight
    {
      get { return this.m_TextHeight; }
      set { this.m_TextHeight = value; }
    }

    [XmlAttribute]
    public int TextWidth
    {
      get { return this.m_TextWidth; }
      set { this.m_TextWidth = value; }
    }

    [XmlArray]
    public string[] TranslateFrom
    {
      get { return this.m_TranslateFrom; }
      set { this.m_TranslateFrom = value; }
    }

    [XmlArray]
    public string[] TranslateTo
    {
      get { return this.m_TranslateTo; }
      set { this.m_TranslateTo = value; }
    }
  }
}