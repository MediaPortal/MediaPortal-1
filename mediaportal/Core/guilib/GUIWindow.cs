#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Media.Animation;
using System.Windows.Serialization;
using System.Xml;
using MediaPortal.Player;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// base class for every window. It contains all methods needed for basic window management like
  /// - initialization
  /// - deitialization
  /// - render itself onscreen
  /// - processing actions like keypresses, mouse clicks/movements
  /// - processing messages
  /// 
  /// Each window plugin should derive from this base class
  /// Pluginwindows should be copied in the plugins/windows folder
  /// </summary>
  public class GUIWindow : ISupportInitialize, IDisposable
  {
    #region window ids

    /// <summary>
    /// Enum of all standard windows in MP
    /// 
    /// IMPORTANT!!! WHEN ADDING NEW WINDOW IDs,
    /// if window may not be jumped to directly via InputMapper,
    /// add it to blacklist in InputMappingForm!!!
    /// (windows with DIALOG in the enum name are blacklisted automatically)
    /// </summary>
    public enum Window
    {
// ReSharper disable InconsistentNaming
      WINDOW_INVALID = -1,
      WINDOW_HOME = 0,
      WINDOW_TV = 1,
      WINDOW_PICTURES = 2,
      WINDOW_FILES = 3,
      WINDOW_SETTINGS = 4,
      WINDOW_MUSIC = 5,
      WINDOW_VIDEOS = 6,
      WINDOW_SYSTEM_INFORMATION = 7,
      [Obsolete("This Window name is obsolete; use WINDOW_SETTINGS_GUISCREENSETUP")]
      WINDOW_SETTINGS_SCREEN = 9,
      WINDOW_SETTINGS_GUISCREENSETUP = 9,
      WINDOW_UI_CALIBRATION = 10,
      WINDOW_MOVIE_CALIBRATION = 11,
      [Obsolete("This Window name is obsolete; use WINDOW_SETTINGS_PICTURES")]
      WINDOW_SETTINGS_SLIDESHOW = 12,
      WINDOW_SETTINGS_PICTURES = 12,
      WINDOW_SETTINGS_MUSIC = 14,
      WINDOW_SCRIPTS = 20,
      WINDOW_VIDEO_TITLE = 25,
      WINDOW_VIDEO_PLAYLIST = 28,
      WINDOW_RADIO = 30,
      [Obsolete("This Window name is obsolete; use WINDOW_SETTINGS_GUICONTROL")]
      WINDOW_SETTINGS_GUI = 31,
      WINDOW_SETTINGS_GUICONTROL = 31,
      WINDOW_MYPLUGINS = 34,
      WINDOW_SECOND_HOME = 35,
      WINDOW_DIALOG_YES_NO = 100,
      WINDOW_DIALOG_PROGRESS = 101,
      WINDOW_DIALOG_PLAY_STOP = 102,
      WINDOW_MUSIC_PLAYLIST = 500,
      WINDOW_MUSIC_FILES = 501,
      WINDOW_MUSIC_GENRE = 504,
      WINDOW_MUSIC_COVERART_GRABBER_RESULTS = 508,
      WINDOW_MUSIC_COVERART_GRABBER_PROGRESS = 509,
      WINDOW_MUSIC_PLAYING_NOW = 510,
      WINDOW_FULLSCREEN_MUSIC = 511, //SV Added by SteveV 2006-09-07
      WINDOW_DIALOG_LASTFM = 512,
      WINDOW_TVGUIDE = 600,
      WINDOW_SCHEDULER = 601,
      WINDOW_TVFULLSCREEN = 602,
      WINDOW_RECORDEDTV = 603,
      WINDOW_SEARCHTV = 604,
      WINDOW_TV_SCHEDULER_PRIORITIES = 607,
      WINDOW_TV_CONFLICTS = 608,
      WINDOW_VIDEO_ARTIST_INFO = 614,
      WINDOW_SETTINGS_TV = 700,
      WINDOW_SETTINGS_RECORDINGS = 701,
      WINDOW_SETTINGS_SORT_CHANNELS = 702,
      WINDOW_SETTINGS_MOVIES = 703,
      WINDOW_SETTINGS_DVD = 704,
      [Obsolete("This Window name is obsolete; use WINDOW_SETTINGS_GUISKIN")]
      WINDOW_SETTINGS_SKIN = 705,
      WINDOW_SETTINGS_GUISKIN = 705,
      WINDOW_SETTINGS_TV_EPG = 706,
      [Obsolete("This Window name is obsolete; use WINDOW_SETTINGS_GUISKIPSTEPS")]
      WINDOW_SETTINGS_SKIPSTEPS = 708, // by rtv
      WINDOW_SETTINGS_GUISKIPSTEPS = 708, // by rtv
      WINDOW_SETTINGS_TVENGINE = 709,
      WINDOW_TV_SEARCH = 747,
      WINDOW_TV_SEARCHTYPE = 746,
      WINDOW_TV_PROGRAM_INFO = 748,
      WINDOW_TV_NO_SIGNAL = 749,
      WINDOW_DIALOG_FILE = 758,
      WINDOW_TV_RECORDED_INFO = 759,
      WINDOW_DIALOG_TVGUIDE = 761,
      WINDOW_RADIO_GUIDE = 762,
      WINDOW_RECORDEDRADIO = 763,
      WINDOW_SETTINGS_FOLDERS = 1000,
      WINDOW_SETTINGS_EXTENSIONS = 1001,
      WINDOW_VIRTUAL_KEYBOARD = 1002,
      WINDOW_SETTINGS_GUITHUMBNAILS = 1005,
      WINDOW_SETTINGS_GUIONSCREEN_DISPLAY= 1006,
      WINDOW_SETTINGS_GENERALVOLUME = 1007,
      WINDOW_SETTINGS_GENERALREFRESHRATE = 1008,
      WINDOW_SETTINGS_VIDEODATABASE = 1010,
      WINDOW_SETTINGS_MUSICDATABASE = 1011,
      WINDOW_SETTINGS_PICTURESDATABASE = 1012,
      WINDOW_SETTINGS_MUSICNOWPLAYING = 1013,
      WINDOW_SETTINGS_PLAYLIST = 1014,
      WINDOW_SETTINGS_PICTURES_SLIDESHOW = 1015,
      WINDOW_SETTINGS_GENERALMAIN= 1016,
      WINDOW_SETTINGS_GENERALRESUME = 1017,
      WINDOW_SETTINGS_GENERALMP = 1018,
      WINDOW_SETTINGS_GENERALSTARTUP = 1019,
      WINDOW_SETTINGS_GUISCREENSAVER = 1020,
      WINDOW_SETTINGS_GUIMAIN = 1021,
      WINDOW_SETTINGS_GUIGENERAL = 1022,
      WINDOW_SETTINGS_VIDEOOTHERSETTINGS = 1023,
      WINDOW_SETTINGS_BLURAY = 1024,
      WINDOW_DIALOG_SELECT = 2000,
      WINDOW_MUSIC_INFO = 2001,
      WINDOW_DIALOG_OK = 2002,
      WINDOW_VIDEO_INFO = 2003,
      WINDOW_MUSIC_OVERLAY = 2004,
      WINDOW_FULLSCREEN_VIDEO = 2005,
      WINDOW_VISUALISATION = 2006,
      WINDOW_SLIDESHOW = 2007,
      WINDOW_DIALOG_FILESTACKING = 2008,
      WINDOW_DIALOG_SELECT2 = 2009,
      WINDOW_DIALOG_DATETIME = 2010,
      WINDOW_ARTIST_INFO = 2011,
      WINDOW_DIALOG_MENU = 2012,
      WINDOW_DIALOG_RATING = 2013,
      WINDOW_DIALOG_EXIF = 2014,
      WINDOW_DIALOG_MENU_BOTTOM_RIGHT = 2015,
      WINDOW_DIALOG_NOTIFY = 2016,
      WINDOW_DIALOG_TVCONFLICT = 2017,
      WINDOW_DIALOG_CIMENU = 2018,
      WINDOW_DIALOG_TVNOTIFYYESNO = 2019,
      WINDOW_DIALOG_OLD_SKIN = 2020,
      WINDOW_DIALOG_INCOMPATIBLE_PLUGINS = 2021,
      WINDOW_SCREENSAVER = 2900,
      WINDOW_OSD = 2901,
      WINDOW_VIDEO_OVERLAY = 3000,
      WINDOW_DVD = 3001, // for keymapping
      WINDOW_TV_OVERLAY = 3002,
      WINDOW_TVOSD = 3003,
      WINDOW_TOPBAR = 3005,
      WINDOW_TVZAPOSD = 3007,
      WINDOW_VIDEO_OVERLAY_TOP = 3008,
      WINDOW_MINI_GUIDE = 3009,
      WINDOW_TV_CROP_SETTINGS = 3011,
      WINDOW_TV_TUNING_DETAILS = 3012, // gemx 
      WINDOW_PSCLIENTPLUGIN_UNATTENDED = 6666, // dero
      WINDOW_WIKIPEDIA = 4711,
      WINDOW_TELETEXT = 7700,
      WINDOW_FULLSCREEN_TELETEXT = 7701,
      WINDOW_DIALOG_TEXT = 7900,
      WINDOW_TETRIS = 7776,
      WINDOW_NUMBERPLACE = 7777, // rtv - sudoku clone
      WINDOW_RADIO_LASTFM = 7890,
      WINDOW_MUSIC_MENU = 8888, // for harley
      WINDOW_SEARCH_RADIO = 8900 // gemx
// ReSharper restore InconsistentNaming

      // Please use IDs up to 9999 only. Let everything above be reserved for external Plugin developers without SVN access.

      // IMPORTANT!!! WHEN ADDING NEW WINDOW IDs,
      // if window may not be jumped to directly via InputMapper,
      // add it to blacklist in InputMappingForm!!!
      // (windows with DIALOG in the enum name are blacklisted automatically)
    }

    #endregion

    #region enums

    protected enum AutoHideTopBar
    {
      UseDefault,
      No,
      Yes,
    }

    #endregion

    #region variables

    private int _windowId = -1;
    private int _previousWindowId = -1;
    private int _previousFocusedControlId = -1;
    private int _rememberLastFocusedControlId = -1;
    private bool _rememberLastFocusedControl = false;
    protected int _defaultControlId = 0;
    protected List<CPosition> _listPositions = new List<CPosition>();
    protected string _windowXmlFileName = "";
    protected bool _isOverlayAllowed = true;
    protected int _isOverlayAllowedCondition = 0;
    protected int _isOverlayAllowedOriginalCondition = GUIInfoManager.SYSTEM_ALWAYS_TRUE;
    private Object instance;
    protected string _loadParameter = null;
    private bool _skipAnimation = false;

    //-1=default from topbar.xml 
    // 0=flase from skin.xml
    // 1=true  from skin.xml
    protected AutoHideTopBar _autoHideTopbarType = AutoHideTopBar.UseDefault;
    protected bool _autoHideTopbar = false;
    protected bool _disableTopBar = false; // skin file can hide Topbar when needed
    private bool _isSkinLoaded = false;
    protected bool _shouldRestore = false;
    private string _lastSkin = string.Empty;
    private bool _windowAllocated;
    private bool _hasRendered = false;
    private bool _windowLoaded = false;
    private static bool _hasWindowVisibilityUpdated;

    private VisualEffect _showAnimation = new VisualEffect(); // for dialogs
    private VisualEffect _closeAnimation = new VisualEffect();

    #endregion

    #region ctor

    /// <summary>
    /// The (emtpy) constructur of the GUIWindow
    /// </summary>
    public GUIWindow() {}

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="skinFile">filename of xml skin file which belongs to this window</param>
    public GUIWindow(string skinFile)
      : this()
    {
      if (skinFile == null)
      {
        return;
      }
      _previousWindowId = -1;
      _windowXmlFileName = skinFile;
    }

    #endregion

    #region methods

    /// <summary>
    /// Clear() method. This method gets called when user switches skin. It removes any static vars
    /// the GUIWindow class has
    /// </summary>
    public static void Clear()
    {
      GUIExpressionManager.ClearExpressionCache();
      GUIControlFactory.ClearReferences();
    }

    public int PreviousFocusedId
    {
      get { return _previousFocusedControlId; }
    }

    public bool RememberLastFocusedControl
    {
      get { return _rememberLastFocusedControl; }
      set { _rememberLastFocusedControl = value; }
    }

    /// <summary>
    /// add a new control to this window
    /// </summary>
    /// <param name="control">new control to add</param>
    public void Add(ref GUIControl control)
    {
      if (control == null)
      {
        return;
      }
      control.WindowId = GetID;
      Children.Add(control);
    }

    /// <summary>
    /// remove a control by its id from this window
    /// </summary>
    /// <param name="dwId">ID of the control</param>
    public void Remove(int dwId)
    {
      int index = 0;
      foreach (GUIControl control in Children)
      {
        GUIGroup grp = control as GUIGroup;
        if (grp != null)
        {
          grp.Remove(dwId);
        }
        else
        {
          if (control.GetID == dwId)
          {
            if (index >= 0 && index < Children.Count)
            {
              Children.RemoveAt(index);
            }
            return;
          }
        }
        index++;
      }
    }

    /// <summary>
    /// Move the control with the specified id to the end of the control list (will render last; in front of other controls).
    /// </summary>
    /// <param name="ctrl">ID of the control</param>
    public void SendToFront(ref GUIControl ctrl)
    {
      // Remove the control from the collection and add it back to the end of the collection.
      Remove(ctrl.GetID);
      Add(ref ctrl);
    }

    /// <summary>
    /// This method will call the OnInit() on each control belonging to this window
    /// this gives the control a way to do some pre-initalisation stuff
    /// </summary>
    public void InitControls()
    {
      try
      {
        for (int x = 0; x < Children.Count; ++x)
        {
          Children[x].OnInit();
        }
      }
      catch (Exception ex)
      {
        Log.Error("InitControls exception:{0}", ex.ToString());
      }
    }

    /// This method will call the OnDeInit() on each control belonging to this window
    /// this gives the control a way to do some de-initalisation stuff
    protected void DeInitControls()
    {
      try
      {
        foreach (GUIControl control in Children)
        {
          control.OnDeInit();
        }
      }
      catch (Exception ex)
      {
        Log.Error("DeInitControls exception:{0}", ex.ToString());
      }
    }


    /// <summary>
    /// return the id of the previous active window
    /// </summary>
    public int PreviousWindowId
    {
      get { return _previousWindowId; }
    }

    /// <summary>
    /// remove all controls from the window
    /// </summary>
    public void ClearAll()
    {
      Dispose();
      Children.Clear();
    }


    /// <summary>
    /// Restores the position of the control to its default position.
    /// </summary>
    /// <param name="iControl">The identifier of the control that needs to be restored.</param>
    public void RestoreControlPosition(int iControl)
    {
      foreach (GUIControl control in Children)
      {
        control.ReStorePosition();
      }
    }

    /// <summary>
    /// Restores window overlay status to default value from skin condition
    /// </summary>
    public void UpdateOverlay()
    {
      UpdateOverlayAllowed(true);
    }

    public bool InWindow(int x, int y)
    {
      for (int i = 0; i < controlList.Count; ++i)
      {
        GUIControl control = controlList[i];
        if (control.IsVisible)
        {
          int controlID;
          if (control.InControl(x, y, out controlID))
          {
            return true;
          }
        }
      }
      return false;
    }

    #endregion

    #region load skin file

    /// <summary>
    /// Load the XML file for this window which 
    /// contains a definition of which controls the GUI has
    /// </summary>
    /// <param name="skinFileName">filename of the .xml file</param>
    /// <returns></returns>
    public virtual bool Load(string skinFileName)
    {
      _isSkinLoaded = false;
      if (string.IsNullOrEmpty(skinFileName))
      {
        return false;
      }
      _windowXmlFileName = skinFileName;

      // if windows supports delayed loading then do nothing else load the xml file now
      if (SupportsDelayedLoad)
      {
        return true;
      }

      // else load xml file now
      LoadSkin();
      if (!_windowAllocated)
      {
        AllocResources();
      }

      return true;

    }

    /// <summary>
    /// Loads the xml file for the window.
    /// </summary>
    /// <returns></returns>
    public bool LoadSkin()
    {

      // add thread check to log calls not running in main thread/GUI
      #if DEBUG
      int iCurrentThread = System.Threading.Thread.CurrentThread.ManagedThreadId;
      if (iCurrentThread != 1)
      {
        Log.Error("LoadSkin: Running on thread <{0}> instead of main thread - StackTrace: '{1}'", iCurrentThread, Environment.StackTrace);
      }
      #endif

      _lastSkin = GUIGraphicsContext.Skin;
      // no filename is configured
      if (_windowXmlFileName == "")
      {
        return false;
      }
      // TODO what is the reason for this check
      if (Children.Count > 0)
      {
        return false;
      }

      _showAnimation.Reset();
      _closeAnimation.Reset();

      _defaultControlId = 0;
      // Load the reference controls
      //int iPos = _windowXmlFileName.LastIndexOf('\\');
      //string strReferenceFile = _windowXmlFileName.Substring(0, iPos);
      _windowXmlFileName = GUIGraphicsContext.GetThemedSkinFile(_windowXmlFileName.Substring(_windowXmlFileName.LastIndexOf("\\")));
      string strReferenceFile = GUIGraphicsContext.GetThemedSkinFile(@"\references.xml");
      GUIControlFactory.LoadReferences(strReferenceFile);

      try
      {
        // Load the XML file
        XmlDocument doc = new XmlDocument();
        doc.Load(_windowXmlFileName);
        if (doc.DocumentElement == null)
        {
          return false;
        }
        string root = doc.DocumentElement.Name;
        // Check root element
        if (root != "window")
        {
          return false;
        }

        XmlNodeList nodeListAnimations = doc.DocumentElement.SelectNodes("/window/animation");
        if (nodeListAnimations != null)
        {
          foreach (XmlNode nodeAnimation in nodeListAnimations)
          {
            if (nodeAnimation.InnerText.ToLowerInvariant() == "windowopen")
            {
              _showAnimation.Create(nodeAnimation);
            }
            if (nodeAnimation.InnerText.ToLowerInvariant() == "windowclose")
            {
              _closeAnimation.Create(nodeAnimation);
            }
          }
        }
        // Load id value
        XmlNode nodeId = doc.DocumentElement.SelectSingleNode("/window/id");
        if (nodeId == null)
        {
          return false;
        }
        // Set the default control that has the focus after loading the window
        XmlNode nodeDefault = doc.DocumentElement.SelectSingleNode("/window/defaultcontrol");
        if (nodeDefault == null)
        {
          return false;
        }
        // Convert the id to an int
        try
        {
          _windowId = Int32.Parse(nodeId.InnerText);
        }
        catch (Exception)
        {
          Log.Error("LoadSkin: error converting nodeid <{0}> to int", nodeId.InnerText);
        }
        // Convert the id of the default control to an int
        try
        {
          _defaultControlId = Int32.Parse(nodeDefault.InnerText);
        }
        catch (Exception)
        {
          Log.Error("LoadSkin: error converting nodeDefault <{0}> to int", nodeDefault.InnerText);
        }

        // find any XAML complex/compound properties
        var xmlNodeList = doc.DocumentElement.SelectNodes("/window/*[contains(name(), '.')]");
        if (xmlNodeList != null)
          foreach (XmlNode node in xmlNodeList)
          {
            string xml = node.OuterXml;

            if (xml.IndexOf("Button.") != -1)
            {
              xml = xml.Replace("Button.", "GUIControl.");
            }

            if (xml.IndexOf("Window.") != -1)
            {
              xml = xml.Replace("Window.", "GUIWindow.");
            }

            XamlParser.LoadXml(xml, XmlNodeType.Element, this, _windowXmlFileName);
          }

        // Configure the overlay settings
        XmlNode nodeOverlay = doc.DocumentElement.SelectSingleNode("/window/allowoverlay");
        if (nodeOverlay != null)
        {
          string allowed = nodeOverlay.InnerText.ToLowerInvariant();
          switch (allowed)
          {
            case "true":
            case "yes":
              _isOverlayAllowed = true;
              break;
            case "false":
            case "no":
              _isOverlayAllowed = false;
              break;
            default:
              _isOverlayAllowedCondition = GUIInfoManager.TranslateString(nodeOverlay.InnerText);
              break;
          }
          if (!string.IsNullOrEmpty(allowed.Trim()))
          {
            _isOverlayAllowedOriginalCondition = GUIInfoManager.TranslateString(nodeOverlay.InnerText);
          }
        }

        IDictionary<string, string> defines = LoadDefines(doc);

        // Configure the autohide setting
        XmlNode nodeAutoHideTopbar = doc.DocumentElement.SelectSingleNode("/window/autohidetopbar");
        if (nodeAutoHideTopbar != null)
        {
          _autoHideTopbarType = AutoHideTopBar.UseDefault;
          string allowed = nodeAutoHideTopbar.InnerText.ToLowerInvariant();
          if (allowed == "yes" || allowed == "true")
          {
            _autoHideTopbarType = AutoHideTopBar.Yes;
          }
          if (allowed == "no" || allowed == "false")
          {
            _autoHideTopbarType = AutoHideTopBar.No;
          }
        }

        // Configure the Topbar disable setting
        XmlNode nodeDisableTopbar = doc.DocumentElement.SelectSingleNode("/window/disabletopbar");
        _disableTopBar = false;
        if (nodeDisableTopbar != null)
        {
          string allowed = nodeDisableTopbar.InnerText.ToLowerInvariant();
          if (allowed == "yes" || allowed == "true")
          {
            _disableTopBar = true;
          }
        }
        _rememberLastFocusedControl = false;
        if (GUIGraphicsContext.AllowRememberLastFocusedItem)
        {
          XmlNode nodeRememberLastFocusedControl = doc.DocumentElement.SelectSingleNode("/window/rememberLastFocusedControl");
          if (nodeRememberLastFocusedControl != null)
          {
            string rememberLastFocusedControlStr = nodeRememberLastFocusedControl.InnerText.ToLowerInvariant();
            if (rememberLastFocusedControlStr == "yes" || rememberLastFocusedControlStr == "true")
            {
              _rememberLastFocusedControl = true;
            }
          }
        }
        XmlNodeList nodeList = doc.DocumentElement.SelectNodes("/window/controls/*");

        if (nodeList != null)
        {
          foreach (XmlNode node in nodeList)
          {
            switch (node.Name)
            {
              case "control":
                LoadControl(node, defines);
                break;
              case "include":
              case "import":

                // Allow an include to be conditionally loaded based on a 'condition' expression.
                bool loadInclude = true;

                if (node.Attributes["condition"] != null && !string.IsNullOrEmpty(node.Attributes["condition"].Value))
                {
                  try
                  {
                    loadInclude = bool.Parse(GUIPropertyManager.Parse(node.Attributes["condition"].Value, GUIExpressionManager.ExpressionOptions.EVALUATE_ALWAYS));
                  }
                  catch (FormatException)
                  {
                    // The include will not be loaded if the expression could not be evaluated.
                    loadInclude = false;
                    Log.Debug("LoadSkin: {0}, could not evaluate include expression '{1}' ", _windowXmlFileName, node.Attributes["condition"].Value);
                  }
                }

                if (loadInclude)
                {
                  LoadInclude(node, defines);
                }
                break;
            }
          }
        }

        // TODO: remove this when all XAML parser or will result in double initialization
        ((ISupportInitialize)this).EndInit();

        //				PrepareTriggers();

        // initialize the controls
        OnWindowLoaded();
        _isSkinLoaded = true;
        return true;
      }
      catch (FileNotFoundException e)
      {
        Log.Error("SKIN: Missing {0}", e.FileName);
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("exception loading window {0} err:{1}\r\n\r\n{2}\r\n\r\n", _windowXmlFileName, ex.Message,
                  ex.StackTrace);
        return false;
      }
    }

    /// <summary>
    /// This method will load a single control from the xml node
    /// </summary>
    /// <param name="node">XmlNode describing the control</param>
    /// <param name="defines">on return this will contain an arraylist of all controls loaded</param>
    protected void LoadControl(XmlNode node, IDictionary<string, string> defines)
    {
      if (node == null || Children == null)
      {
        return;
      }

      try
      {
        GUIControl newControl = GUIControlFactory.Create(_windowId, node, defines, _windowXmlFileName);
        newControl.WindowId = GetID;
        GUIImage img = newControl as GUIImage;
        if (img != null)
        {
          if (img.Width == 0 || img.Height == 0)
          {
            Log.Info("xml:{0} image id:{1} width:{2} height:{3} gfx:{4}",
                     _windowXmlFileName, img.GetID, img.Width, img.Height, img.FileName);
          }
        }
        lock (GUIGraphicsContext.RenderLock)
        {
          Children.Add(newControl);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Unable to load control. exception:{0}", ex.ToString());
      }
    }

    private bool LoadInclude(XmlNode node, IDictionary<string, string> defines)
    {
      if (node == null || Children == null)
      {
        return false;
      }

      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(GUIGraphicsContext.GetThemedSkinFile("\\" + node.InnerText));

        if (doc.DocumentElement == null)
        {
          return false;
        }

        // Load #defines specified in the included skin xml document and add them to the existing (parent document) set of #defines.
        // The dictionary merge replaces the value of duplicates.
        IDictionary<string, string> includeDefines = LoadDefines(doc);
        defines = defines.Merge(includeDefines);

        if (doc.DocumentElement.Name != "window")
        {
          return false;
        }

        var xmlNodeList = doc.DocumentElement.SelectNodes("/window/controls/control");
        if (xmlNodeList != null)
          foreach (XmlNode controlNode in xmlNodeList)
          {
            LoadControl(controlNode, defines);
          }

        return true;
      }
      catch (FileNotFoundException e)
      {
        Log.Error("SKIN: Missing {0}", e.FileName);
        return false;
      }
      catch (Exception e)
      {
        Log.Error("GUIWIndow.LoadInclude: {0}", e.Message);
      }

      return false;
    }

    private IDictionary<string, string> LoadDefines(XmlDocument document)
    {
      IDictionary<string, string> table = new Dictionary<string, string>();

      try
      {
        bool createAsProperty;
        bool evaluateNow;

        var xmlNodeList = document.SelectNodes("/window/define");
        if (xmlNodeList != null)
          foreach (XmlNode node in xmlNodeList)
          {
            string[] tokens = node.InnerText.Split(':');

            if (tokens.Length < 2)
            {
              continue;
            }

            // Determine if the define should be promoted to a property.
            createAsProperty = false;

            if (node.Attributes != null && (node.Attributes["property"] != null && !string.IsNullOrEmpty(node.Attributes["property"].Value)))
            {
              try
              {
                createAsProperty = bool.Parse(node.Attributes["property"].Value);
              }
              catch (FormatException)
              {
                Log.Debug("Window: LoadDefines() - failed to parse define attribute value for 'property'; {0} is not a boolean value", node.Attributes["property"].Value);
              }
            }

            // Determine if the define should be evaluated now.
            evaluateNow = false;
            if (node.Attributes != null && (node.Attributes["evaluateNow"] != null && !string.IsNullOrEmpty(node.Attributes["evaluateNow"].Value)))
            {
              try
              {
                evaluateNow = bool.Parse(node.Attributes["evaluateNow"].Value);
              }
              catch (FormatException)
              {
                Log.Debug("Window: LoadDefines() - failed to parse define attribute value for 'evaluateNow'; {0} is not a boolean value", node.Attributes["evaluateNow"].Value);
              }
            }

            // If evaluateNow then parse and evaluate the define value expression now.
            if (evaluateNow)
            {
              table[tokens[0]] = GUIExpressionManager.Parse(tokens[1], GUIExpressionManager.ExpressionOptions.EVALUATE_ALWAYS);
            }
            else
            {
              table[tokens[0]] = tokens[1];
            }

            // Promote the define to a property if specified.
            if (createAsProperty)
            {
              GUIPropertyManager.SetProperty(tokens[0], table[tokens[0]]);
            }
          }
      }
      catch (Exception e)
      {
        Log.Error("GUIWindow.LoadDefines: {0}", e.Message);
      }

      return table;
    }

    #endregion

    #region virtual methods

    /// <summary>
    /// This function gets called once by the runtime when everything is up & running
    /// directX is now initialized, but before the first window is activated. 
    /// It gives the window the oppertunity to allocate any (directx) resources
    /// it may need
    /// </summary>
    public virtual void PreInit() {}

    /// <summary>
    /// Restores all the (x,y) positions of the XML file to their original values
    /// </summary>
    public virtual void Restore()
    {
      _shouldRestore = true;
    }

    /// <summary>
    /// Property indicating if the window supports delay loading or not
    /// if a window returns true it means that its resources & XML will be loaded
    /// just before it gets activated
    /// for windows not supporting delayed loading, the xml is immediately loaded
    /// at startup of the application
    /// </summary>
    public virtual bool SupportsDelayedLoad
    {
      get { return true; }
    }

    /// <summary>
    ///  Gets called when DirectX device has been restored. 
    /// </summary>
    public virtual void OnDeviceRestored() {}

    /// <summary>
    /// Gets called when DirectX device has been lost. Any texture/font is now invalid
    /// </summary>
    public virtual void OnDeviceLost() {}

    /// <summary>
    /// Returns whether the music/video/tv overlay is allowed on this screen
    /// </summary>
    public virtual bool IsOverlayAllowed
    {
      get { return _isOverlayAllowed; }
      set { _isOverlayAllowed = value; }
    }

    /// <summary>
    /// Returns whether autohide the topbar is allowed on this screen
    /// </summary>
    public virtual bool AutoHideTopbar
    {
      get
      {
        // set topbar autohide 
        switch (_autoHideTopbarType)
        {
          case AutoHideTopBar.No:
            return false;
          case AutoHideTopBar.Yes:
            return true;
          default:
            return GUIGraphicsContext.DefaultTopBarHide;
        }
      }
    }

    public virtual void Process() {}

    public virtual void SetObject(object obj)
    {
      instance = obj;
    }

    private void UpdateOverlayAllowed()
    {
      UpdateOverlayAllowed(false);
    }

    private void UpdateOverlayAllowed(bool useOriginal)
    {
      int overlayCondition = useOriginal ? _isOverlayAllowedOriginalCondition : _isOverlayAllowedCondition;

      if (overlayCondition == 0)
      {
        return;
      }

      bool bWasAllowed = GUIGraphicsContext.Overlay;
      _isOverlayAllowed = GUIInfoManager.GetBool(overlayCondition, GetID);

      if (bWasAllowed != _isOverlayAllowed)
      {
        GUIGraphicsContext.Overlay = _isOverlayAllowed;
      }
    }

    private void SetInitialOverlayAllowed()
    {
      if (_isOverlayAllowedCondition == 0)
      {
        return;
      }
      _isOverlayAllowed = GUIGraphicsContext.Overlay = GUIInfoManager.GetBool(_isOverlayAllowedCondition, GetID);

      // no need to enquire every frame if overlays are always allowed or never allowed
      if (_isOverlayAllowedCondition == GUIInfoManager.SYSTEM_ALWAYS_TRUE ||
          _isOverlayAllowedCondition == GUIInfoManager.SYSTEM_ALWAYS_FALSE)
      {
        _isOverlayAllowedCondition = 0;
      }
    }

    private void SetControlVisibility()
    {
      // reset our info manager caches
      GUIInfoManager.ResetCache();
      foreach (GUIControl control in Children)
      {
        if (control.GetVisibleCondition() != 0)
        {
          control.SetInitialVisibility();
        }
      }
    }

    protected virtual void PreLoadPage() {}

    protected virtual void OnPageLoad()
    {
      if (_isSkinLoaded && (_lastSkin != GUIGraphicsContext.Skin))
      {
        LoadSkin();
      }

      if (_rememberLastFocusedControl && _rememberLastFocusedControlId >= 0)
      {
        GUIControl cntl = GetControl(_rememberLastFocusedControlId);
        if (cntl != null)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0,
                                          _rememberLastFocusedControlId, 0, 0, null);
          OnMessage(msg);
        }
        int id = GetFocusControlId();
        if (id >= 0)
        {
          _previousFocusedControlId = id;
        }
      }

      SetControlVisibility();
      SetInitialOverlayAllowed();

      if (!_skipAnimation)
      {
        QueueAnimation(AnimationType.WindowOpen);
      }
    }

    protected virtual void OnPageDestroy(int newWindowId)
    {
      if (_rememberLastFocusedControl)
      {
        int id = GetFocusControlId();
        if (id >= 0)
        {
          _rememberLastFocusedControlId = id;
        }
      }

      if (GUIGraphicsContext.IsFullScreenVideo == false)
      {
        if (newWindowId != (int)Window.WINDOW_FULLSCREEN_VIDEO &&
            newWindowId != (int)Window.WINDOW_TVFULLSCREEN)
        {
          // Dialog animations are handled in Close() rather than here
          if (HasAnimation(AnimationType.WindowClose) && !_skipAnimation) //&& !IsDialog)
          {
            // Perform the window out effect
            QueueAnimation(AnimationType.WindowClose);
            bool switching = GUIWindowManager.IsSwitchingToNewWindow;
            GUIWindowManager.IsSwitchingToNewWindow = false;
            while (IsAnimating(AnimationType.WindowClose))
            {
              if (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.RUNNING)
              {
                break;
              }
              if (GUIGraphicsContext.Vmr9Active && (VMR9Util.g_vmr9 != null && !VMR9Util.g_vmr9.Enabled))
              {
                break;
              }
              GUIWindowManager.Process();
            }
            GUIWindowManager.IsSwitchingToNewWindow = switching;
            foreach (GUIControl control in controlList)
            {
              control.ResetAnimations();
            }
          }
        }
      }
    }

    protected virtual void OnShowContextMenu() {}

    protected virtual void OnPreviousWindow()
    {
      GUIWindowManager.ShowPreviousWindow();
    }

    protected virtual void OnClicked(int controlId, GUIControl control, Action.ActionType actionType) {}

    protected virtual void OnClickedUp(int controlId, GUIControl control, Action.ActionType actionType) {}

    protected virtual void OnClickedDown(int controlId, GUIControl control, Action.ActionType actionType) {}

    /// <summary>
    /// Returns whether the user can goto full screen video,tv,visualisation from this window
    /// </summary>
    public virtual bool FullScreenVideoAllowed
    {
      get { return true; }
    }

    /// <summary>
    /// Gets called by the runtime when a new window has been created
    /// Every window window should override this method and load itself by calling
    /// the Load() method
    /// </summary>
    /// <returns>true if initialisation was succesfull 
    /// else false</returns>
    public virtual bool Init()
    {
      return false;
    }

    /// <summary>
    /// Gets called by the runtime when a  window will be destroyed
    /// Every window window should override this method and cleanup any resources
    /// </summary>
    /// <returns></returns>
    public virtual void DeInit() {}

    /// <summary>
    /// Gets called by the runtime just before the window gets shown. It
    /// will ask every control of the window to allocate its (directx) resources 
    /// </summary>
    // 
    public virtual void AllocResources()
    {
      try
      {
        if (_windowAllocated)
        {
          return;
        }

        Dispose();
        LoadSkin();
        HashSet<int> faultyControl = new HashSet<int>();
        // tell every control we're gonna alloc the resources next
        for (int i = 0; i < Children.Count; i++)
        {
          try
          {
            Children[i].PreAllocResources();
          }
          catch (Exception ex1)
          {
            faultyControl.Add(i);
            Log.Error("GUIWindow: Error in PreAllocResources for {0} - {1}", Children[i].ToString(), ex1.ToString());
          }
        }

        // ask every control to alloc its resources
        for (int i = 0; i < Children.Count; i++)
        {
          try
          {
            if (!faultyControl.Contains(i))
            {
              Children[i].AllocResources();
            }
            else
            {
              Log.Warn("GUIWindow: Did not AllocResources for Control # {0}", Children[i].GetID);
            }
          }
          catch (Exception ex2)
          {
            Log.Error("GUIWindow: Error in AllocResources for {0} - {1}", Children[i].ToString(), ex2.ToString());
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIWindow: AllocResources exception - {0}", ex.ToString());
      }
      _windowAllocated = true;
    }

    /// <summary>
    /// Gets called by the runtime when the window is not longer shown. It will
    /// ask every control of the window 2 free its (directx) resources
    /// </summary>
    public virtual void Dispose()
    {
      _windowAllocated = false;
      try
      {
        // tell every control to free its resources
        Children.DisposeAndClearCollection();
        _listPositions.DisposeAndClear();
      }
      catch (Exception ex)
      {
        Log.Error("GUIWindow: Dispose exception - {0}", ex.ToString());
      }
    }

    [Obsolete("method 'FreeResources' is obsolete, instead use dispose.")]
    public void FreeResources()
    {
      Dispose();
    }

    /// <summary>
    /// Resets all the controls to their original positions, width and height
    /// </summary>
    public virtual void ResetAllControls()
    {
      try
      {
        foreach (GUIControl control in Children)
        {
          control.DoUpdate();
        }
      }
      catch (Exception ex)
      {
        Log.Error("ResetAllControls exception:{0}", ex.ToString());
      }
    }

    /// <summary>
    /// Gets by the window manager when it has loaded the window
    /// default implementation stores the position of all controls
    /// in _listPositions
    /// </summary>
    protected virtual void OnWindowLoaded()
    {
      _windowLoaded = false;
      _listPositions = new List<CPosition>();

      foreach (GUIControl child in Children)
      {
        GUIControl control = child;
        control.StorePosition();
        CPosition pos = new CPosition(ref control, control.XPosition, control.YPosition);
        _listPositions.Add(pos);
      }

      FieldInfo[] allFields = GetType().GetFields(BindingFlags.Instance
                                                       | BindingFlags.NonPublic
                                                       | BindingFlags.FlattenHierarchy
                                                       | BindingFlags.Public);
      foreach (FieldInfo field in allFields)
      {
        if (field.IsDefined(typeof (SkinControlAttribute), false))
        {
          SkinControlAttribute atrb =
            (SkinControlAttribute)field.GetCustomAttributes(typeof (SkinControlAttribute), false)[0];

          GUIControl control = GetControl(atrb.ID);
          if (control != null)
          {
            try
            {
              field.SetValue(this, control);
            }
            catch (Exception ex)
            {
              Log.Error("GUIWindow:OnWindowLoaded '{0}' control id:{1} ex:{2} {3} {4}",
                        _windowXmlFileName,
                        atrb.ID,
                        ex.Message,
                        ex.StackTrace,
                        ToString());
            }
          }
          else
          {
            Log.Warn("GUIWindow:OnWindowLoaded: '{0}' is missing control id {1} (window property: {2})",
                     _windowXmlFileName, atrb.ID, field.Name);
          }
        }
      }
      _windowLoaded = true;
    }

    /// <summary>
    /// get a control by the control ID
    /// </summary>
    /// <param name="iControlId">id of control</param>
    /// <returns>GUIControl or null if control is not found</returns>
    public virtual GUIControl GetControl(int iControlId)
    {
      // this is a very hot method, called millions of times and all the virtual property calls costs
      return Children.GetControlById(iControlId);
      //for (int x = 0; x < Children.Count; x++)
      //{
      //    GUIControl cntl = Children[x];
      //    GUIControl cntlFound = cntl.GetControlById(iControlId);
      //    if (cntlFound != null)
      //    {
      //        return cntlFound;
      //    }
      //}
      //return null;
    }

    /// <summary>
    /// calls UpdateVisibility for all children components
    /// (also used for allowing to switch focus to a component that can only be 
    /// focused if the current component is not active)
    /// </summary>
    public virtual void UpdateVisibility()
    {
      foreach (GUIControl child in Children)
      {
        GUIGroup grp = child as GUIGroup;
        if (grp != null)
        {
          grp.UpdateVisibility();
          foreach (GUIControl control in grp.Children)
          {
            control.UpdateVisibility();
          }
        }
        else
        {
          child.UpdateVisibility();
        }
      }
    }

    /// <summary>
    /// returns the ID of the control which has the focus
    /// </summary>
    /// <returns>id of control or -1 if no control has the focus</returns>
    public virtual int GetFocusControlId()
    {
      foreach (GUIControl child in Children)
      {
        GUIGroup grp = child as GUIGroup;
        if (grp != null)
        {
          int iFocusedControlId = grp.GetFocusControlId();
          if (iFocusedControlId >= 0)
          {
            _previousFocusedControlId = iFocusedControlId;
            return iFocusedControlId;
          }
        }
        else
        {
          var guicontrol = child;
          if (guicontrol.Focus)
          {
            return guicontrol.GetID;
          }
        }
      }
      return -1;
    }

    /// <summary>
    /// This method will remove the focus from the currently focused control
    /// </summary>
    public virtual void LooseFocus()
    {
      GUIControl cntl = GetControl(GetFocusControlId());
      if (cntl != null)
      {
        cntl.Focus = false;
      }
    }

    /// <summary>
    /// Return the id of this window
    /// </summary>
    public virtual int GetID
    {
      get { return _windowId; }
      set { _windowId = value; }
    }

    public void DoRestoreSkin()
    {
      if (!_shouldRestore)
      {
        return;
      }
      _shouldRestore = false;

      Dispose();
      Load(_windowXmlFileName);
      AllocResources();
    }

    /// <summary>
    /// Render() method. This method draws the window by asking every control
    /// of the window to render itself
    /// </summary>
    public virtual void Render(float timePassed)
    {
      if (_shouldRestore)
      {
        DoRestoreSkin();
      }
      try
      {
        if (!_isSkinLoaded)
        {
          if (GUIGraphicsContext.IsFullScreenVideo ||
              GetID == (int)Window.WINDOW_FULLSCREEN_VIDEO ||
              GetID == (int)Window.WINDOW_TVFULLSCREEN ||
              GetID == (int)Window.WINDOW_FULLSCREEN_MUSIC)
          {
            return;
          }

          // Print an error message
          GUIFont font = GUIFontManager.GetFont(0);
          if (font != null)
          {
            float fW = 0f;
            float fH = 0f;
            string strLine = String.Format("Missing or invalid file:{0}", _windowXmlFileName);
            font.GetTextExtent(strLine, ref fW, ref fH);
            float x = (GUIGraphicsContext.Width - fW) / 2f;
            float y = (GUIGraphicsContext.Height - fH) / 2f;
            font.DrawText(x, y, 0xffffffff, strLine, GUIControl.Alignment.ALIGN_LEFT, -1);
          }
        }
        GUIGraphicsContext.SetScalingResolution(0, 0, false);
        //uint currentTime = (uint) (DXUtil.Timer(DirectXTimer.GetAbsoluteTime)*1000.0);
        uint currentTime = (uint)AnimationTimer.TickCount;
        // render our window animation - returns false if it needs to stop rendering
        if (!RenderAnimation(currentTime))
        {
          return;
        }

        UpdateOverlayAllowed();
        _hasWindowVisibilityUpdated = true;
        // TODO must do a proper fix
        for (int i = 0; i < Children.Count; i++)
        {
          GUIControl control = Children[i];
          control.UpdateVisibility();
          control.DoRender(timePassed, currentTime);
        }
        
        GUIWaitCursor.Render();
      }
      catch (Exception ex)
      {
        Log.Error("render exception:{0}", ex.ToString());
      }
      _hasRendered = true;
    }

    /// <summary>
    /// NeedRefresh() can be called to see if the windows needs 2 redraw itself or not
    /// some controls (for example the fadelabel) contain scrolling texts and need 2
    /// ne re-rendered constantly
    /// </summary>
    /// <returns>true or false</returns>
    public virtual bool NeedRefresh()
    {
      try
      {
        foreach (GUIControl control in Children)
        {
          if (control.NeedRefresh())
          {
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("NeedRefresh exception:{0}", ex.ToString());
      }
      return false;
    }

    /// <summary>
    /// OnAction() method. This method gets called when there's a new action like a 
    /// keypress or mousemove or... By overriding this method, the window can respond
    /// to any action
    /// </summary>
    /// <param name="action">action : contains the action</param>
    public virtual void OnAction(Action action)
    {
      if (action == null)
      {
        return;
      }
      if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
      {
        OnShowContextMenu();
        return;
      }
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        OnPreviousWindow();
        return;
      }

      try
      {
        GUIMessage msg;
        // mouse moved, check which control has the focus
        int id;
        if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE)
        {
          OnMouseMove((int) action.fAmount1, (int) action.fAmount2, action);
          id = GetFocusControlId();
          if (id >= 0)
          {
            _previousFocusedControlId = id;
          }
          return;
        }
        // mouse clicked if there is a hit pass the action
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
        {
          OnMouseClick((int) action.fAmount1, (int) action.fAmount2, action);
          id = GetFocusControlId();
          if (id >= 0)
          {
            _previousFocusedControlId = id;
          }
          return;
        }

        // send the action to the control which has the focus
        GUIControl cntlFoc = GetControl(GetFocusControlId());
        if (cntlFoc != null)
        {
          id = GetFocusControlId();
          if (id >= 0)
          {
            _previousFocusedControlId = id;
          }
          cntlFoc.OnAction(action);
          id = GetFocusControlId();
          if (id >= 0)
          {
            _previousFocusedControlId = id;
          }

          return;
        }

        // no control has focus?
        // set focus to the default control then
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, _defaultControlId, 0, 0, null);
        OnMessage(msg);
        id = GetFocusControlId();
        if (id >= 0)
        {
          _previousFocusedControlId = id;
        }
      }
      catch (Exception ex)
      {
        Log.Error("OnAction exception:{0}", ex.ToString());
      }
    }

    public virtual bool Focused
    {
      get { return false; }
      set { }
    }

    /// <summary>
    /// OnMessage() This method gets called when there's a new message. 
    /// Controls send messages to notify their parents about their state (changes)
    /// By overriding this method a window can respond to the messages of its controls
    /// </summary>
    /// <param name="message"></param>
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public virtual bool OnMessage(GUIMessage message)
    {
      if (message == null)
      {
        return true;
      }

      //lock (this)
      _skipAnimation = (message.Param2 != 0);
      if (!_skipAnimation)
      {
        AnimationTrigger(message);
      }
      int id;
      int iControlId = message.SenderControlId;
      {
        try
        {
          switch (message.Message)
          {
            case GUIMessage.MessageType.GUI_MSG_CLICKED:
              if (iControlId != 0)
              {
                OnClicked(iControlId, GetControl(iControlId), (Action.ActionType)message.Param1);
              }
              break;

            case GUIMessage.MessageType.GUI_MSG_CLICKED_DOWN:
              if (iControlId != 0)
              {
                OnClickedDown(iControlId, GetControl(iControlId), (Action.ActionType)message.Param1);
              }
              break;

            case GUIMessage.MessageType.GUI_MSG_CLICKED_UP:
              if (iControlId != 0)
              {
                OnClickedUp(iControlId, GetControl(iControlId), (Action.ActionType)message.Param1);
              }
              break;

              // Initialize the window.

            case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:

              // we do not want fullscreen video/TV dialogue ID's persisted.
              if (GUIWindowManager.ActiveWindow != (int)Window.WINDOW_FULLSCREEN_VIDEO &&
                  GUIWindowManager.ActiveWindow != (int)Window.WINDOW_TVFULLSCREEN)
              {
                GUIPropertyManager.SetProperty("#currentmoduleid", Convert.ToString(GUIWindowManager.ActiveWindow));
              }

              GUIPropertyManager.SetProperty("#itemcount", string.Empty);
              // TODO: in derived classes set #itemtype to an appropriate localized string
              GUIPropertyManager.SetProperty("#itemtype", GUILocalizeStrings.Get(507)); // items
              GUIPropertyManager.SetProperty("#selecteditem", string.Empty);
              GUIPropertyManager.SetProperty("#selecteditem2", string.Empty);
              GUIPropertyManager.SetProperty("#selectedthumb", string.Empty);
              GUIPropertyManager.SetProperty("#selectedindex", string.Empty);
              GUIPropertyManager.SetProperty("#facadeview.layout", string.Empty);
              if (_shouldRestore)
              {
                DoRestoreSkin();
              }
              else
              {
                LoadSkin();
                if (!_windowAllocated)
                {
                  AllocResources();
                }
              }

              InitControls();
              UpdateOverlayAllowed();
              GUIGraphicsContext.Overlay = _isOverlayAllowed;

              // set topbar autohide 
              switch (_autoHideTopbarType)
              {
                case AutoHideTopBar.No:
                  _autoHideTopbar = false;
                  break;
                case AutoHideTopBar.Yes:
                  _autoHideTopbar = true;
                  break;
                default:
                  _autoHideTopbar = GUIGraphicsContext.DefaultTopBarHide;
                  break;
              }
              GUIGraphicsContext.AutoHideTopBar = _autoHideTopbar;
              GUIGraphicsContext.TopBarHidden = _autoHideTopbar;
              GUIGraphicsContext.DisableTopBar = _disableTopBar;

              if (message.Param1 != (int) Window.WINDOW_INVALID && message.Param1 != GetID)
              {
                _previousWindowId = message.Param1;
              }

              int controlId = _defaultControlId;
              if (message.Param3 > 0)
              {
                controlId = message.Param3;
              }

              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, controlId, 0, message.Param2,
                                              null);
              OnMessage(msg);

              GUIPropertyManager.SetProperty("#currentmodule", GetModuleName());
              Log.Debug("Window: {0} init", ToString());

              _hasRendered = false;

              if (message.Object is string)
              {
                _loadParameter = (string)message.Object;
              }
              else
              {
                _loadParameter = null;
              }

              OnPageLoad();

              TemporaryAnimationTrigger();

              id = GetFocusControlId();
              if (id >= 0)
              {
                _previousFocusedControlId = id;
              }

              _skipAnimation = false;
              return true;
              // TODO BUG ! Check if this return needs to be in the case and if there needs to be a break statement after each case.

              // Cleanup and free resources
            case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
              {
                OnPageDestroy(message.Param1);

                Log.Debug("Window: {0} deinit", ToString());
                DeInitControls();
                Dispose();
                GUITextureManager.CleanupThumbs();
#if DEBUG
                //long lTotalMemory = GC.GetTotalMemory(true);
                //Log.Info("Total Memory allocated:{0}", MediaPortal.Util.Utils.GetSize(lTotalMemory));
#endif
                _shouldRestore = true;
                _skipAnimation = false;
                return true;
              }

              // Set the focus on the correct control
            case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
              {
                if (GetFocusControlId() == message.TargetControlId)
                {
                  return true;
                }

                if (message.TargetControlId > 0)
                {
                  GUIControl cntlFocused = GetControl(GetFocusControlId());
                  if (cntlFocused != null)
                  {
                    GUIMessage msgLostFocus = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LOSTFOCUS, GetID,
                                                             cntlFocused.GetID, cntlFocused.GetID, 0, 0, null);
                    cntlFocused.OnMessage(msgLostFocus);
                  }
                  GUIControl cntTarget = GetControl(message.TargetControlId);
                  if (cntTarget != null)
                  {
                    // recalculate visibility, so a invisible item that is becoming visible 
                    // becouse the previous component is no longer focused can be focused
                    // mantis issue #1755
                    //UpdateVisibility();   //Bav: reverting change, because it is creating a nonfocused status, when a focus is switching from one control to the other
                    cntTarget.OnMessage(message);
                  }
                }
                id = GetFocusControlId();
                if (id >= 0)
                {
                  _previousFocusedControlId = id;
                }
                return true;
              }
          }

          GUIControl cntlTarget = GetControl(message.TargetControlId);
          if (cntlTarget != null)
          {
            return cntlTarget.OnMessage(message);
          }
          id = GetFocusControlId();
          if (id >= 0)
          {
            _previousFocusedControlId = id;
          }
        }
        catch (Exception ex)
        {
          Log.Error("OnMessage exception:{0}", ex.ToString());
        }
        return false;
      }
    }

    protected virtual void OnMouseMove(int cx, int cy, Action action)
    {
      for (int i = Children.Count - 1; i >= 0; i--)
      {
        GUIControl control = Children[i];
        bool bFocus;
        int controlID;
        if (control.HitTest(cx, cy, out controlID, out bFocus))
        {
          if (!bFocus)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, controlID, 0, 0, null);
            OnMessage(msg);
            control.HitTest(cx, cy, out controlID, out bFocus);
          }
          control.OnAction(action);
          return;
        }
        control.Focus = false;
      }
    }

    protected virtual void OnMouseClick(int posX, int posY, Action action)
    {
      for (int i = Children.Count - 1; i >= 0; i--)
      {
        GUIControl control = Children[i];
        bool bFocus;
        int controlID;
        if (control.HitTest(posX, posY, out controlID, out bFocus))
        {
          GUIControl cntl = GetControl(controlID);
          if (cntl != null)
          {
            cntl.OnAction(action);
          }
          return;
        }
      }
    }

    private void TemporaryAnimationTrigger()
    {
      //BAV: Testing
      //if (_children == null)
      //  return;

      //// this method is a temporary fix to Harley's animation not starting on subsequent selection of a page
      //foreach (UIElement element in _children)
      //{
      //  if (element is GUIAnimation)
      //    ((GUIAnimation)element).Begin();
      //}
    }

    private void AnimationTrigger(GUIMessage message)
    {
      if (_children == null)
      {
        return;
      }

      foreach (var element in _children)
      {
        if (element is GUIAnimation)
        {
          element.OnMessage(message);
        }
      }
    }

    #endregion

    public GUIControlCollection controlList
    {
      get { return Children; }
    }

    public virtual bool IsDialog
    {
      get { return false; }
    }

    public virtual bool IsInstance(Object obj)
    {
      return obj == instance;
    }

    public virtual bool IsTv
    {
      get { return false; }
    }

    public virtual void OnAdded() {}

    public virtual string GetModuleName()
    {
      return string.Empty;
    }

    #region effects

    private bool RenderAnimation(uint time)
    {
      TransformMatrix transform = new TransformMatrix();
      // show animation
      _showAnimation.Animate(time, true);
      UpdateStates(_showAnimation.AnimationType, _showAnimation.CurrentProcess, _showAnimation.CurrentState);
      _showAnimation.RenderAnimation(ref transform);
      // close animation
      _closeAnimation.Animate(time, true);
      UpdateStates(_closeAnimation.AnimationType, _closeAnimation.CurrentProcess, _closeAnimation.CurrentState);
      _closeAnimation.RenderAnimation(ref transform);
      GUIGraphicsContext.SetWindowTransform(transform);
      return true;
    }

    private void UpdateStates(AnimationType type, AnimationProcess currentProcess, AnimationState currentState) {}

    private bool HasAnimation(AnimationType animType)
    {
      if (_showAnimation.AnimationType == AnimationType.WindowOpen)
      {
        return true;
      }
      if (_closeAnimation.AnimationType == AnimationType.WindowClose)
      {
        return true;
      }
      // Now check the controls to see if we have this animation
      foreach (GUIControl control in Children)
      {
        if (control.GetAnimation(animType, true) != null)
        {
          return true;
        }
      }
      return false;
    }

    public void QueueAnimation(AnimationType animType)
    {
      if (animType == AnimationType.WindowOpen)
      {
        if (_closeAnimation.CurrentProcess == AnimationProcess.Normal && _closeAnimation.IsReversible)
        {
          _closeAnimation.QueuedProcess = AnimationProcess.Reverse;
          _showAnimation.ResetAnimation();
        }
        else
        {
          if (0 == _showAnimation.Condition || GUIInfoManager.GetBool(_showAnimation.Condition, GetID))
          {
            _showAnimation.QueuedProcess = AnimationProcess.Normal;
          }
          _closeAnimation.ResetAnimation();
        }
      }
      if (animType == AnimationType.WindowClose)
      {
        if (!_windowAllocated || !_hasRendered) // can't render an animation if we aren't allocated or haven't rendered
        {
          return;
        }
        if (_showAnimation.CurrentProcess == AnimationProcess.Normal && _showAnimation.IsReversible)
        {
          _showAnimation.QueuedProcess = AnimationProcess.Reverse;
          _closeAnimation.ResetAnimation();
        }
        else
        {
          if (0 == _closeAnimation.Condition || GUIInfoManager.GetBool(_closeAnimation.Condition, GetID))
          {
            _closeAnimation.QueuedProcess = AnimationProcess.Normal;
          }
          _showAnimation.ResetAnimation();
        }
      }
      foreach (GUIControl control in Children)
      {
        control.QueueAnimation(animType);
      }
    }

    protected bool IsAnimating(AnimationType animType)
    {
      if (animType == AnimationType.WindowOpen)
      {
        if (_showAnimation.QueuedProcess == AnimationProcess.Normal)
        {
          return true;
        }
        if (_showAnimation.CurrentProcess == AnimationProcess.Normal)
        {
          return true;
        }
        if (_closeAnimation.QueuedProcess == AnimationProcess.Reverse)
        {
          return true;
        }
        if (_closeAnimation.CurrentProcess == AnimationProcess.Reverse)
        {
          return true;
        }
      }
      else if (animType == AnimationType.WindowClose)
      {
        if (_closeAnimation.QueuedProcess == AnimationProcess.Normal)
        {
          return true;
        }
        if (_closeAnimation.CurrentProcess == AnimationProcess.Normal)
        {
          return true;
        }
        if (_showAnimation.QueuedProcess == AnimationProcess.Reverse)
        {
          return true;
        }
        if (_showAnimation.CurrentProcess == AnimationProcess.Reverse)
        {
          return true;
        }
      }
      foreach (GUIControl control in Children)
      {
        if (control.IsEffectAnimating(animType))
        {
          return true;
        }
      }
      return false;
    }

    #endregion

    /// XAML related code follows

    #region Properties

    public GUIControlCollection Children
    {
      get { return _children ?? (_children = new GUIControlCollection()); }
    }

    public StoryboardCollection Storyboards
    {
      get { return _storyboards ?? (_storyboards = new StoryboardCollection()); }
    }

    public bool WindowLoaded
    {
      get { return _windowLoaded; }
    }

    public static bool HasWindowVisibilityUpdated
    {
      get { return _hasWindowVisibilityUpdated; }
      set { _hasWindowVisibilityUpdated = value; }
    }

    #endregion Properties

    #region Fields

    private GUIControlCollection _children;
    private StoryboardCollection _storyboards;

    #endregion Fields

    public void BeginInit() {}

    public void EndInit() {}
  }
}