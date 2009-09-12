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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Serialization;
using System.Xml;
using MediaPortal.Player;

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
  public class GUIWindow : Page
  {
    #region window ids

    /// <summary>
    /// Enum of all standard windows in MP
    /// 
    /// IMPORTANT!!! WHEN ADDING NEW WINDOW IDs,
    /// ADD DIALOGS TO InputMappingForm.cs BLACKLIST!!!
    /// (Windows that may not be jumped to directly via InputMapper)
    /// </summary>
    public enum Window
    {
      WINDOW_INVALID = -1,
      WINDOW_HOME = 0,
      WINDOW_TV = 1,
      WINDOW_PICTURES = 2,
      WINDOW_FILES = 3,
      WINDOW_SETTINGS = 4,
      WINDOW_MUSIC = 5,
      WINDOW_VIDEOS = 6,
      WINDOW_SYSTEM_INFORMATION = 7,
      WINDOW_SETTINGS_GENERAL = 8,
      WINDOW_SETTINGS_SCREEN = 9,
      WINDOW_UI_CALIBRATION = 10,
      WINDOW_MOVIE_CALIBRATION = 11,
      WINDOW_SETTINGS_SLIDESHOW = 12,
      WINDOW_SETTINGS_FILTER = 13,
      WINDOW_SETTINGS_MUSIC = 14,
      WINDOW_SETTINGS_SUBTITLES = 15,
      WINDOW_SETTINGS_SCREENSAVER = 16,
      WINDOW_WEATHER_SETTINGS = 17,
      WINDOW_SETTINGS_OSD = 18,
      WINDOW_SCRIPTS = 20,
      WINDOW_VIDEO_GENRE = 21,
      WINDOW_VIDEO_ACTOR = 22,
      WINDOW_VIDEO_YEAR = 23,
      WINDOW_SETTINGS_PROGRAMS = 24,
      WINDOW_VIDEO_TITLE = 25,
      WINDOW_SETTINGS_CACHE = 26,
      WINDOW_SETTINGS_AUTORUN = 27,
      WINDOW_VIDEO_PLAYLIST = 28,
      WINDOW_SETTINGS_LCD = 29,
      WINDOW_RADIO = 30,
      WINDOW_SETTINGS_GUI = 31,
      WINDOW_MSN = 32,
      WINDOW_MSN_CHAT = 33,
      WINDOW_MYPLUGINS = 34,
      WINDOW_SECOND_HOME = 35,
      WINDOW_DIALOG_YES_NO = 100,
      WINDOW_DIALOG_PROGRESS = 101,
      WINDOW_DIALOG_PLAY_STOP = 102,
      WINDOW_MUSIC_PLAYLIST = 500,
      WINDOW_MUSIC_FILES = 501,
      WINDOW_MUSIC_ALBUM = 502,
      WINDOW_MUSIC_ARTIST = 503,
      WINDOW_MUSIC_GENRE = 504,
      WINDOW_MUSIC_TOP100 = 505,
      WINDOW_MUSIC_FAVORITES = 506,
      WINDOW_MUSIC_YEARS = 507,
      WINDOW_MUSIC_COVERART_GRABBER_RESULTS = 508,
      WINDOW_MUSIC_COVERART_GRABBER_PROGRESS = 509,
      WINDOW_MUSIC_PLAYING_NOW = 510,
      WINDOW_FULLSCREEN_MUSIC = 511, //SV Added by SteveV 2006-09-07
      WINDOW_TVGUIDE = 600,
      WINDOW_SCHEDULER = 601,
      WINDOW_TVFULLSCREEN = 602,
      WINDOW_RECORDEDTV = 603,
      WINDOW_SEARCHTV = 604,
      WINDOW_RECORDEDTVGENRE = 605,
      WINDOW_RECORDEDTVCHANNEL = 606,
      WINDOW_TV_SCHEDULER_PRIORITIES = 607,
      WINDOW_TV_CONFLICTS = 608,
      WINDOW_TV_COMPRESS_MAIN = 609,
      WINDOW_TV_COMPRESS_SETTINGS = 610,
      WINDOW_TV_COMPRESS_AUTO = 611,
      WINDOW_TV_COMPRESS_COMPRESS = 612,
      WINDOW_TV_COMPRESS_COMPRESS_STATUS = 613,
      WINDOW_VIDEO_ARTIST_INFO = 614,
      WINDOW_WIZARD_WELCOME = 615,
      WINDOW_WIZARD_WELCOME_TVE2 = 711,
      WINDOW_WIZARD_CARDS_DETECTED = 616,
      WINDOW_WIZARD_DVBT_COUNTRY = 617,
      WINDOW_WIZARD_DVBT_SCAN = 618,
      WINDOW_WIZARD_DVBC_COUNTRY = 619,
      WINDOW_WIZARD_DVBC_SCAN = 620,
      WINDOW_WIZARD_DVBS_SELECT_LNB = 621,
      WINDOW_WIZARD_DVBS_SELECT_DETAILS = 622,
      WINDOW_WIZARD_DVBS_SELECT_TRANSPONDER = 623,
      WINDOW_WIZARD_DVBS_SCAN = 624,
      WINDOW_WIZARD_ATSC_SCAN = 625,
      WINDOW_WIZARD_ANALOG_COUNTRY = 626,
      WINDOW_WIZARD_ANALOG_CITY = 627,
      WINDOW_WIZARD_ANALOG_IMPORTED = 628,
      WINDOW_WIZARD_ANALOG_MANUAL_TUNE = 629,
      WINDOW_WIZARD_ANALOG_TUNE = 630,
      WINDOW_WIZARD_ANALOG_RENAME = 631,
      WINDOW_WIZARD_ANALOG_SCAN_RADIO = 632,
      WINDOW_WIZARD_ANALOG_RENAME_RADIO = 633,
      WINDOW_WIZARD_REMOTE = 634,
      WINDOW_WIZARD_EPG_SELECT = 635,
      WINDOW_WIZARD_GENERAL = 636,
      WINDOW_WIZARD_FINISHED = 699,
      WINDOW_WIZARD_FINISHED_TVE2 = 710,
      WINDOW_SETTINGS_TVE2 = 712,
      WINDOW_SETTINGS_TV_TVE2 = 713,
      WINDOW_SETTINGS_TV = 700,
      WINDOW_SETTINGS_RECORDINGS = 701,
      WINDOW_SETTINGS_SORT_CHANNELS = 702,
      WINDOW_SETTINGS_MOVIES = 703,
      WINDOW_SETTINGS_DVD = 704,
      WINDOW_SETTINGS_SKIN = 705,
      WINDOW_SETTINGS_TV_EPG = 706,
      WINDOW_SETTINGS_TV_EPG_MAPPING = 707,
      WINDOW_SETTINGS_SKIPSTEPS = 708, // by rtv
      WINDOW_SETTINGS_TVENGINE = 709,
      WINDOW_TV_SEARCH = 747,
      WINDOW_TV_SEARCHTYPE = 746,
      WINDOW_TV_PROGRAM_INFO = 748,
      WINDOW_TV_NO_SIGNAL = 749,
      WINDOW_MY_RECIPIES = 750,
      WINDOW_STATUS = 755,
      WINDOW_STATUS_DETAILS = 756,
      WINDOW_STATUS_PREFS = 757,
      WINDOW_DIALOG_FILE = 758,
      WINDOW_TV_RECORDED_INFO = 759,
      WINDOW_MY_BURNER = 760,
      WINDOW_DIALOG_TVGUIDE = 761,
      WINDOW_RADIO_GUIDE = 762,
      WINDOW_EXTENSIONS = 800,
      WINDOW_VIRTUAL_KEYBOARD = 1002,
      WINDOW_VIRTUAL_WEB_KEYBOARD = 1003, // by Devo
      WINDOW_VIRTUAL_SMS_KEYBOARD = 1004,
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
      WINDOW_WEATHER = 2600,
      WINDOW_SCREENSAVER = 2900,
      WINDOW_OSD = 2901,
      WINDOW_MSNOSD = 2902,
      WINDOW_VIDEO_EDITOR = 2959,
      WINDOW_VIDEO_EDITOR_COMPRESSSETTINGS = 2960,
      WINDOW_VIDEO_OVERLAY = 3000,
      WINDOW_DVD = 3001, // for keymapping
      WINDOW_TV_OVERLAY = 3002,
      WINDOW_TVOSD = 3003,
      //WINDOW_TOPBARHOME = 3004,
      WINDOW_TOPBAR = 3005,
      WINDOW_TVMSNOSD = 3006,
      WINDOW_TVZAPOSD = 3007,
      WINDOW_VIDEO_OVERLAY_TOP = 3008,
      WINDOW_MINI_GUIDE = 3009,
      WINDOW_ACTIONMENU = 3010,
      WINDOW_TV_CROP_SETTINGS = 3011,
      WINDOW_TV_TUNING_DETAILS = 3012, // gemx 
      WINDOW_WEBBROWSER = 5500,
      WINDOW_PSCLIENTPLUGIN_UNATTENDED = 6666, // dero
      WINDOW_WIKIPEDIA = 4711,
      WINDOW_TELETEXT = 7700,
      WINDOW_FULLSCREEN_TELETEXT = 7701,
      WINDOW_CARTOONS = 7800,
      WINDOW_DIALOG_TEXT = 7900,
      WINDOW_SUNCLOCK = 8000,
      WINDOW_TRAILERS = 5900,
      WINDOW_TETRIS = 7776,
      WINDOW_NUMBERPLACE = 7777, // rtv - sudoku clone
      WINDOW_RADIO_LASTFM = 7890,
      WINDOW_MUSIC_MENU = 8888, // for harley


      // Please use IDs up to 9999 only. Let everything above be reserved for external Plugin developers without SVN access.

      // IMPORTANT!!! WHEN ADDING NEW WINDOW IDs,
      // ADD DIALOGS TO InputMappingForm.cs BLACKLIST!!!
      // (Windows that may not be jumped to directly via InputMapper)
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
    private Object instance;

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

    private VisualEffect _showAnimation = new VisualEffect(); // for dialogs
    private VisualEffect _closeAnimation = new VisualEffect();

    #endregion

    #region ctor

    /// <summary>
    /// The (emtpy) constructur of the GUIWindow
    /// </summary>
    public GUIWindow()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="strXMLFile">filename of xml skin file which belongs to this window</param>
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
    /// This method will call the OnInit() on each control belonging to this window
    /// this gives the control a way to do some pre-initalisation stuff
    /// </summary>
    public void InitControls()
    {
      try
      {
        for (int x = 0; x < Children.Count; ++x)
        {
          ((GUIControl) Children[x]).OnInit();
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
      FreeResources();
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

    #endregion

    #region load skin file

    /// <summary>
    /// Load the XML file for this window which 
    /// contains a definition of which controls the GUI has
    /// </summary>
    /// <param name="_skinFileName">filename of the .xml file</param>
    /// <returns></returns>
    public virtual bool Load(string _skinFileName)
    {
      _isSkinLoaded = false;
      if ((_skinFileName == null) || (_skinFileName == ""))
      {
        return false;
      }

      _windowXmlFileName = _skinFileName;

      // if windows supports delayed loading then do nothing
      if (SupportsDelayedLoad)
      {
        return true;
      }

      //else load xml file now
      return LoadSkin();
    }


    /// <summary>
    /// Loads the xml file for the window.
    /// </summary>
    /// <returns></returns>
    public bool LoadSkin()
    {
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
      _windowXmlFileName = GUIGraphicsContext.Skin + _windowXmlFileName.Substring(_windowXmlFileName.LastIndexOf("\\"));
      string strReferenceFile = GUIGraphicsContext.Skin + @"\references.xml";
      GUIControlFactory.LoadReferences(strReferenceFile);

      if (!File.Exists(_windowXmlFileName))
      {
        Log.Error("SKIN: Missing {0}", _windowXmlFileName);
        return false;
      }
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
            if (nodeAnimation.InnerText.ToLower() == "windowopen")
            {
              _showAnimation.Create(nodeAnimation);
            }
            if (nodeAnimation.InnerText.ToLower() == "windowclose")
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
          _windowId = (int) Int32.Parse(nodeId.InnerText);
        }
        catch (Exception)
        {
          // TODO Add some error when conversion fails message here.
        }
        // Convert the id of the default control to an int
        try
        {
          _defaultControlId = Int32.Parse(nodeDefault.InnerText);
        }
        catch (Exception)
        {
          // TODO Add some error when conversion fails message here.
        }

        // find any XAML complex/compound properties
        foreach (XmlNode node in doc.DocumentElement.SelectNodes("/window/*[contains(name(), '.')]"))
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

          XamlParser.LoadXml(xml, XmlNodeType.Element, this);
        }

        // Configure the overlay settings
        XmlNode nodeOverlay = doc.DocumentElement.SelectSingleNode("/window/allowoverlay");
        if (nodeOverlay != null)
        {
          if (nodeOverlay.InnerText != null)
          {
            string allowed = nodeOverlay.InnerText.ToLower();
            if (allowed == "yes" || allowed == "true")
            {
              _isOverlayAllowed = true;
            }
            else if (allowed == "no" || allowed == "false")
            {
              _isOverlayAllowed = false;
            }
            else
            {
              _isOverlayAllowedCondition = GUIInfoManager.TranslateString(nodeOverlay.InnerText);
            }
          }
        }

        IDictionary defines = LoadDefines(doc);

        // Configure the autohide setting
        XmlNode nodeAutoHideTopbar = doc.DocumentElement.SelectSingleNode("/window/autohidetopbar");
        if (nodeAutoHideTopbar != null)
        {
          if (nodeAutoHideTopbar.InnerText != null)
          {
            _autoHideTopbarType = AutoHideTopBar.UseDefault;
            string allowed = nodeAutoHideTopbar.InnerText.ToLower();
            if (allowed == "yes" || allowed == "true")
            {
              _autoHideTopbarType = AutoHideTopBar.Yes;
            }
            if (allowed == "no" || allowed == "false")
            {
              _autoHideTopbarType = AutoHideTopBar.No;
            }
          }
        }

        // Configure the Topbar disable setting
        XmlNode nodeDisableTopbar = doc.DocumentElement.SelectSingleNode("/window/disabletopbar");
        _disableTopBar = false;
        if (nodeDisableTopbar != null)
        {
          if (nodeDisableTopbar.InnerText != null)
          {
            string allowed = nodeDisableTopbar.InnerText.ToLower();
            if (allowed == "yes" || allowed == "true")
            {
              _disableTopBar = true;
            }
          }
        }
        _rememberLastFocusedControl = false;
        if (GUIGraphicsContext.AllowRememberLastFocusedItem)
        {
          XmlNode nodeRememberLastFocusedControl =
            doc.DocumentElement.SelectSingleNode("/window/rememberLastFocusedControl");
          if (nodeRememberLastFocusedControl != null)
          {
            string rememberLastFocusedControlStr = nodeRememberLastFocusedControl.InnerText.ToLower();
            if (rememberLastFocusedControlStr == "yes" || rememberLastFocusedControlStr == "true")
            {
              _rememberLastFocusedControl = true;
            }
          }
        }
        XmlNodeList nodeList = doc.DocumentElement.SelectNodes("/window/controls/*");

        foreach (XmlNode node in nodeList)
        {
          if (node.Name == null)
          {
            continue;
          }

          switch (node.Name)
          {
            case "control":
              LoadControl(node, defines);
              break;
            case "include":
            case "import":
              LoadInclude(node, defines);
              break;
          }
        }

        // TODO: remove this when all XAML parser or will result in double initialization
        ((ISupportInitialize) this).EndInit();

        //				PrepareTriggers();

        // initialize the controls
        OnWindowLoaded();
        _isSkinLoaded = true;
        return true;
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
    /// <param name="controls">on return this will contain an arraylist of all controls loaded</param>
    protected void LoadControl(XmlNode node, IDictionary defines)
    {
      if (node == null || Children == null)
      {
        return;
      }

      try
      {
        GUIControl newControl = GUIControlFactory.Create(_windowId, node, defines);
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

        Children.Add(newControl);
      }
      catch (Exception ex)
      {
        Log.Error("Unable to load control. exception:{0}", ex.ToString());
      }
    }

    private bool LoadInclude(XmlNode node, IDictionary defines)
    {
      if (node == null || Children == null)
      {
        return false;
      }

      if (File.Exists(_windowXmlFileName) == false)
      {
        Log.Error("SKIN: Missing {0}", _windowXmlFileName);
        return false;
      }

      try
      {
        XmlDocument doc = new XmlDocument();

        doc.Load(GUIGraphicsContext.Skin + "\\" + node.InnerText);

        if (doc.DocumentElement == null)
        {
          return false;
        }

        if (doc.DocumentElement.Name != "window")
        {
          return false;
        }

        foreach (XmlNode controlNode in doc.DocumentElement.SelectNodes("/window/controls/control"))
        {
          LoadControl(controlNode, defines);
        }

        return true;
      }
      catch (Exception e)
      {
        Log.Error("GUIWIndow.LoadInclude: {0}", e.Message);
      }

      return false;
    }

    private IDictionary LoadDefines(XmlDocument document)
    {
      Hashtable table = new Hashtable();

      try
      {
        foreach (XmlNode node in document.SelectNodes("/window/define"))
        {
          string[] tokens = node.InnerText.Split(':');

          if (tokens.Length < 2)
          {
            continue;
          }

          table[tokens[0]] = tokens[1];
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
    public virtual void PreInit()
    {
    }

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
    public virtual void OnDeviceRestored()
    {
    }

    /// <summary>
    /// Gets called when DirectX device has been lost. Any texture/font is now invalid
    /// </summary>
    public virtual void OnDeviceLost()
    {
    }

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

    public virtual void Process()
    {
    }

    public virtual void SetObject(object obj)
    {
      this.instance = obj;
    }

    private void UpdateOverlayAllowed()
    {
      if (_isOverlayAllowedCondition == 0)
      {
        return;
      }
      bool bWasAllowed = GUIGraphicsContext.Overlay;
      _isOverlayAllowed = GUIInfoManager.GetBool(_isOverlayAllowedCondition, GetID);
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

    protected virtual void PreLoadPage()
    {
    }

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
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, _rememberLastFocusedControlId, 0, 0, null);
            OnMessage(msg);
            msg = null;
        }
        int id = GetFocusControlId();
        if (id >= 0)
        {
          _previousFocusedControlId = id;
        }
      }

      SetControlVisibility();
      SetInitialOverlayAllowed();
      QueueAnimation(AnimationType.WindowOpen);
    }

    protected virtual void OnPageDestroy(int new_windowId)
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
        if (new_windowId != (int) Window.WINDOW_FULLSCREEN_VIDEO &&
            new_windowId != (int) Window.WINDOW_TVFULLSCREEN)
        {
          // Dialog animations are handled in Close() rather than here
          if (HasAnimation(AnimationType.WindowClose)) //&& !IsDialog)
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
              if (GUIGraphicsContext.Vmr9Active)
              {
                if (VMR9Util.g_vmr9 != null)
                {
                  if (!VMR9Util.g_vmr9.Enabled)
                  {
                    break;
                  }
                }
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

    protected virtual void OnShowContextMenu()
    {
    }

    protected virtual void OnPreviousWindow()
    {
      GUIWindowManager.ShowPreviousWindow();
    }

    protected virtual void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
    }

    protected virtual void OnClickedUp(int controlId, GUIControl control, Action.ActionType actionType)
    {
    }

    protected virtual void OnClickedDown(int controlId, GUIControl control, Action.ActionType actionType)
    {
    }

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
    public virtual void DeInit()
    {
    }

    /// <summary>
    /// Gets called by the runtime just before the window gets shown. It
    /// will ask every control of the window to allocate its (directx) resources 
    /// </summary>
    // 
    public virtual void AllocResources()
    {
      try
      {
        // tell every control we're gonna alloc the resources next

        foreach (GUIControl control in Children)
        {
          control.PreAllocResources();
        }

        // ask every control to alloc its resources
        foreach (GUIControl control in Children)
        {
          control.AllocResources();
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
    public virtual void FreeResources()
    {
      _windowAllocated = false;
      try
      {
        // tell every control to free its resources
        foreach (GUIControl control in Children)
        {
          control.FreeResources();
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIWindow: FreeResources exception - {0}", ex.ToString());
      }
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
      this._windowLoaded = false;
      _listPositions = new List<CPosition>();

      for (int i = 0; i < Children.Count; ++i)
      {
        GUIControl control = (GUIControl) Children[i];
        control.StorePosition();
        CPosition pos = new CPosition(ref control, control.XPosition, control.YPosition);
        _listPositions.Add(pos);
      }

      FieldInfo[] allFields = this.GetType().GetFields(BindingFlags.Instance
                                                       | BindingFlags.NonPublic
                                                       | BindingFlags.FlattenHierarchy
                                                       | BindingFlags.Public);
      foreach (FieldInfo field in allFields)
      {
        if (field.IsDefined(typeof (SkinControlAttribute), false))
        {
          SkinControlAttribute atrb =
            (SkinControlAttribute) field.GetCustomAttributes(typeof (SkinControlAttribute), false)[0];

          GUIControl control = GetControl(atrb.ID);
          if (control != null)
          {
            try
            {
              field.SetValue(this, control);
            }
            catch (Exception ex)
            {
              Log.Error("GUIWindow:OnWindowLoaded id:{0} ex:{1} {2} {3}", atrb.ID, ex.Message, ex.StackTrace,
                        this.ToString());
            }
          }
        }
      }
      this._windowLoaded = true;
    }

    /// <summary>
    /// get a control by the control ID
    /// </summary>
    /// <param name="iControlId">id of control</param>
    /// <returns>GUIControl or null if control is not found</returns>
    public virtual GUIControl GetControl(int iControlId)
    {
      for (int x = 0; x < Children.Count; x++)
      {
        GUIControl cntl = (GUIControl) Children[x];
        GUIControl cntlFound = cntl.GetControlById(iControlId);
        if (cntlFound != null)
        {
          return cntlFound;
        }
      }
      return null;
    }

    /// <summary>
    /// calls UpdateVisibility for all children components
    /// (also used for allowing to switch focus to a component that can only be 
    /// focused if the current component is not active)
    /// </summary>
    public virtual void UpdateVisibility()
    {
      for (int x = 0; x < Children.Count; ++x)
      {
        GUIGroup grp = Children[x] as GUIGroup;
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
          ((GUIControl) Children[x]).UpdateVisibility();
        }
      }
    }

    /// <summary>
    /// returns the ID of the control which has the focus
    /// </summary>
    /// <returns>id of control or -1 if no control has the focus</returns>
    public virtual int GetFocusControlId()
    {
      for (int x = 0; x < Children.Count; ++x)
      {
        GUIGroup grp = Children[x] as GUIGroup;
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
          if (((GUIControl) Children[x]).Focus)
          {
            return ((GUIControl) Children[x]).GetID;
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
      FreeResources();
      Children.Clear();
      _listPositions.Clear();
      Load(_windowXmlFileName);
      LoadSkin();

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
      //lock (this)
      {
        try
        {
          if (!_isSkinLoaded)
          {
            if (GUIGraphicsContext.IsFullScreenVideo)
            {
              return;
            }
            if (GetID == (int) Window.WINDOW_FULLSCREEN_VIDEO)
            {
              return;
            }
            if (GetID == (int) Window.WINDOW_TVFULLSCREEN)
            {
              return;
            }
            if (GetID == (int) Window.WINDOW_FULLSCREEN_MUSIC)
            {
              return; //SV Added by SteveV 2006-09-07
            }

            // Print an error message
            GUIFont font = GUIFontManager.GetFont(0);
            if (font != null)
            {
              float fW = 0f;
              float fH = 0f;
              string strLine = String.Format("Missing or invalid file:{0}", _windowXmlFileName);
              font.GetTextExtent(strLine, ref fW, ref fH);
              float x = (GUIGraphicsContext.Width - fW)/2f;
              float y = (GUIGraphicsContext.Height - fH)/2f;
              font.DrawText(x, y, 0xffffffff, strLine, GUIControl.Alignment.ALIGN_LEFT, -1);
              strLine = null;
            }
            font = null;
          }
          GUIGraphicsContext.SetScalingResolution(0, 0, false);
          //uint currentTime = (uint) (DXUtil.Timer(DirectXTimer.GetAbsoluteTime)*1000.0);
          uint currentTime = (uint)System.Windows.Media.Animation.AnimationTimer.TickCount;
          // render our window animation - returns false if it needs to stop rendering
          if (!RenderAnimation(currentTime))
          {
            return;
          }

          UpdateOverlayAllowed();
          foreach (GUIControl control in Children)
          {
            control.UpdateVisibility();
            control.DoRender(timePassed, currentTime);
          }

          GUIWaitCursor.Render();
        }
        catch (Exception ex)
        {
          Log.Error("render exception:{0}", ex.ToString());
        }
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
      int id;
      //lock (this)
      {
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
          msg = null;
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
      AnimationTrigger(message);
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
                OnClicked(iControlId, GetControl(iControlId), (Action.ActionType) message.Param1);
              }
              break;

            case GUIMessage.MessageType.GUI_MSG_CLICKED_DOWN:
              if (iControlId != 0)
              {
                OnClickedDown(iControlId, GetControl(iControlId), (Action.ActionType) message.Param1);
              }
              break;

            case GUIMessage.MessageType.GUI_MSG_CLICKED_UP:
              if (iControlId != 0)
              {
                OnClickedUp(iControlId, GetControl(iControlId), (Action.ActionType) message.Param1);
              }
              break;

              // Initialize the window.

            case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:

              // we do not want fullscreen video/TV dialogue ID's persisted.
              if (GUIWindowManager.ActiveWindow != (int) Window.WINDOW_FULLSCREEN_VIDEO &&
                  GUIWindowManager.ActiveWindow != (int) Window.WINDOW_TVFULLSCREEN)
              {
                GUIPropertyManager.SetProperty("#currentmoduleid", Convert.ToString(GUIWindowManager.ActiveWindow));
              }

              GUIPropertyManager.SetProperty("#itemcount", string.Empty);
              GUIPropertyManager.SetProperty("#selecteditem", string.Empty);
              GUIPropertyManager.SetProperty("#selecteditem2", string.Empty);
              GUIPropertyManager.SetProperty("#selectedthumb", string.Empty);
              if (_shouldRestore)
              {
                DoRestoreSkin();
              }
              else
              {
                LoadSkin();
                //AllocResources(); // Mantis 0002389 - Double AllocResources
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

              if (message.Param1 != (int) Window.WINDOW_INVALID)
              {
                if (message.Param1 != GetID)
                {
                  _previousWindowId = message.Param1;
                }
              }

              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, 0, _defaultControlId, 0, 0,
                                              null);
              OnMessage(msg);

              GUIPropertyManager.SetProperty("#currentmodule", GetModuleName());
              Log.Debug("Window: {0} init", this.ToString());

              _hasRendered = false;
              OnPageLoad();

              TemporaryAnimationTrigger();

              id = GetFocusControlId();
              if (id >= 0)
              {
                _previousFocusedControlId = id;
              }
              return true;
              // TODO BUG ! Check if this return needs to be in the case and if there needs to be a break statement after each case.

              // Cleanup and free resources
            case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
              {
                OnPageDestroy(message.Param1);
                if (_previousWindowId != (int) Window.WINDOW_INVALID)
                {
                  GUIPropertyManager.SetProperty("#currentmodule", GUIWindowManager.GetWindow(_previousWindowId).GetModuleName());
                }

                Log.Debug("Window: {0} deinit", this.ToString());
                FreeResources();
                DeInitControls();
                GUITextureManager.CleanupThumbs();
                //GC.Collect();
                //GC.Collect();
                //GC.Collect();
                //long lTotalMemory = GC.GetTotalMemory(true);
                //Log.Info("Total Memory allocated:{0}", MediaPortal.Util.Utils.GetSize(lTotalMemory));
                _shouldRestore = true;
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
                    msgLostFocus = null;
                    cntlFocused = null;
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
                  cntTarget = null;
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
        GUIControl control = (GUIControl) Children[i];
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
        else
        {
          // no control selected
          //LooseFocus();
          control.Focus = false;
        }
      }
    }

    protected virtual void OnMouseClick(int posX, int posY, Action action)
    {
      for (int i = Children.Count - 1; i >= 0; i--)
      {
        GUIControl control = (GUIControl) Children[i];
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
      return;
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

      foreach (UIElement element in _children)
      {
        if (element is GUIAnimation)
        {
          ((GUIAnimation) element).OnMessage(message);
        }
      }
    }

    #endregion

    public UIElementCollection controlList
    {
      get { return this.Children; }
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

    public virtual void OnAdded()
    {
    }

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

    private void UpdateStates(AnimationType type, AnimationProcess currentProcess, AnimationState currentState)
    {
    }

    private bool HasAnimation(AnimationType animType)
    {
      if (_showAnimation.AnimationType == AnimationType.WindowOpen)
      {
        return true;
      }
      else if (_closeAnimation.AnimationType == AnimationType.WindowClose)
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

    public UIElementCollection Children
    {
      get
      {
        if (_children == null)
        {
          _children = new UIElementCollection();
        }
        return _children;
      }
    }

    public StoryboardCollection Storyboards
    {
      get
      {
        if (_storyboards == null)
        {
          _storyboards = new StoryboardCollection();
        }
        return _storyboards;
      }
    }

    public bool WindowLoaded
    {
      get { return _windowLoaded; }
    }

    #endregion Properties

    #region Fields

    private UIElementCollection _children;
    private StoryboardCollection _storyboards;

    #endregion Fields
  }
}