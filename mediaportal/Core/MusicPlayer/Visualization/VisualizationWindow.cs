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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Profile;
using MediaPortal.TagReader;
using MediaPortal.Util;

namespace MediaPortal.Visualization
{
  public partial class VisualizationWindow : UserControl
  {
    #region Interop

    private const int WM_MOVE = 0x3;
    private const int WM_SIZE = 0x5;
    private const int WM_WINDOWPOSCHANGED = 0x47;

    [DllImport("Gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("User32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("Gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("Gdi32.dll")]
    public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

    [DllImport("Gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("Gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("Gdi32.dll")]
    public static extern int SaveDC(IntPtr hdc);

    [DllImport("Gdi32.dll")]
    public static extern bool RestoreDC(IntPtr hdc, int nSavedDC);

    [DllImport("Gdi32.dll")]
    public static extern int BitBlt(IntPtr hdcdest, int nxdest, int nydest, int nwidth, int nheight, IntPtr hdcsrc,
                                    int nxsrc, int nysrc, int raster);

    private const Int32 SRCCOPY = 0xCC0020;

    [SuppressUnmanagedCodeSecurity] // We won't use this maliciously
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int x, int y, int w, int h, uint flags);

    #endregion

    #region enums

    private enum ControlID
    {
      OverlayImage = 1,
      CoverArtImage = 2,
      MissingCoverArtImage = 3,
      Label1 = 4,
      Label2 = 5,
      Label3 = 6,
      Label4 = 7,
      PauseIcon = 8,
      PlayIcon = 9,
      FFIcon = 10,
      RewIcon = 11,
      StopIcon = 12,
      CrossfadeIcon = 13,
      GapIcon = 14,
      GaplessIcon = 15,
    } ;

    private enum PlayBackType
    {
      NORMAL = 0,
      GAPLESS = 1,
      CROSSFADE = 2
    }

    #endregion

    #region Variables

    private BassAudioEngine Bass = null;
    private IVisualization Viz = null;
    private IntPtr _CompatibleDC = IntPtr.Zero;
    private IntPtr hNativeMemDCBmp = IntPtr.Zero;
    private Bitmap BackBuffer = null;

    private PlayListPlayer PlaylistPlayer = null;
    private MusicTag CurrentTrackTag = null;

    private int CurrentFrame = 0;
    private int FadeFrameCount = 15;
    private int ShowCoverArtFrameCount = 90;
    private int ShowVisualizationFrameCount = 150;
    private const int TrackInfoDisplayMS = 5 * 1000;
    private int ShowTrackInfoFrameCount = 250;
    private int _playBackType;

    private int ShowPlayStateFrameCount = 30;
    private const int PlayStateDisplayMS = 1 * 1000;
    private int CurrentPlayStateFrame = 0;

    private bool SeekingFF = false;
    private bool SeekingRew = false;
    private bool NewPlay = false;
    private bool PlayBackTypeChanged = false;

    private bool IsSoundSpectrumViz = false;
    private bool IsCursorMovedBottomRight = false;

    #region Overlay Image Variables

    private Image CurrentTrackInfoImage = null;
    private string CurrentThumbPath = string.Empty;
    private string CurrentFilePath = string.Empty;

    private Image CurrentThumbImage = null;
    private bool FullScreen = false;
    private bool CoverArtNeedsRefresh = false;
    private Image TrackInfoImage = null;
    private bool NewTrack = false;
    private bool ShowTrackOverlay = false;
    private bool _EnableStatusOverlays = false;

    private byte TrackInfoOverlayAlpha = 255;

    private int TrackInfoImageWidth = 400;
    private int TrackInfoImageHeight = 127;
    private int TrackInfoImageXPos = 20;
    private int TrackInfoImageYPos = 20;

    private int DefaultTrackInfoImageWidth = 0;
    private int DefaultTrackInfoImageHeight = 0;
    private int DefaultTrackInfoImageXPos = 0;
    private int DefaultTrackInfoImageYPos = 0;

    private string TrackInfoImageName = "VisualizationTrackInfo.png";

    private int CoverArtWidth = 87;
    private int CoverArtHeight = 87;
    private int CoverArtXOffset = 25;
    private int CoverArtYOffset = 25;

    private int DefaultCoverArtWidth = 0;
    private int DefaultCoverArtHeight = 0;
    private int DefaultCoverArtXOffset = 0;
    private int DefaultCoverArtYOffset = 0;

    private string PauseImageName = "logo_pause.png";
    private Image PauseImage = null;
    private int PauseImageWidth = 59;
    private int PauseImageHeight = 73;
    private int PauseImageX = 36;
    private int PauseImageY = 700;

    private int DefaultPauseImageWidth = 0;
    private int DefaultPauseImageHeight = 0;
    private int DefaultPauseImageX = 0;
    private int DefaultPauseImageY = 0;

    private string PlayImageName = "logo_play.png";
    private Image PlayImage = null;
    private int PlayImageWidth = 51;
    private int PlayImageHeight = 69;
    private int PlayImageX = 40;
    private int PlayImageY = 702;

    private int DefaultPlayImageWidth = 0;
    private int DefaultPlayImageHeight = 0;
    private int DefaultPlayImageX = 0;
    private int DefaultPlayImageY = 0;

    private string FFImageName = "logo_fastforward.png";
    private Image FFImage = null;
    private int FFImageWidth = 74;
    private int FFImageHeight = 69;
    private int FFImageX = 28;
    private int FFImageY = 702;

    private int DefaultFFImageWidth = 0;
    private int DefaultFFImageHeight = 0;
    private int DefaultFFImageX = 0;
    private int DefaultFFImageY = 0;

    private string RewImageName = "logo_rewind.png";
    private Image RewImage = null;
    private int RewImageWidth = 74;
    private int RewImageHeight = 69;
    private int RewImageX = 28;
    private int RewImageY = 702;

    private int DefaultRewImageWidth = 0;
    private int DefaultRewImageHeight = 0;
    private int DefaultRewImageX = 0;
    private int DefaultRewImageY = 0;

    //private string StopImageName = "logo_stop.png";
    private Image StopImage = null;
    private int StopImageWidth = 65;
    private int StopImageHeight = 65;
    private int StopImageX = 33;
    private int StopImageY = 704;

    private int DefaultStopImageWidth = 0;
    private int DefaultStopImageHeight = 0;
    private int DefaultStopImageX = 0;
    private int DefaultStopImageY = 0;

    private string CrossfadeImageName = "logo_crossfade.png";
    private Image CrossfadeImage = null;
    private int CrossfadeImageWidth = 54;
    private int CrossfadeImageHeight = 54;
    private int CrossfadeImageX = 28;
    private int CrossfadeImageY = 702;

    private int DefaultCrossfadeImageWidth = 0;
    private int DefaultCrossfadeImageHeight = 0;
    private int DefaultCrossfadeImageX = 0;
    private int DefaultCrossfadeImageY = 0;

    private string GapImageName = "logo_gap.png";
    private Image GapImage = null;
    private int GapImageWidth = 54;
    private int GapImageHeight = 54;
    private int GapImageX = 28;
    private int GapImageY = 702;

    private int DefaultGapImageWidth = 0;
    private int DefaultGapImageHeight = 0;
    private int DefaultGapImageX = 0;
    private int DefaultGapImageY = 0;

    private string GaplessImageName = "logo_gapless.png";
    private Image GaplessImage = null;
    private int GaplessImageWidth = 54;
    private int GaplessImageHeight = 54;
    private int GaplessImageX = 28;
    private int GaplessImageY = 702;

    private int DefaultGaplessImageWidth = 0;
    private int DefaultGaplessImageHeight = 0;
    private int DefaultGaplessImageX = 0;
    private int DefaultGaplessImageY = 0;

    private string MissingCoverArtImageName = "missing_coverart.png";
    private Image MissingCoverArtImage = null;

    private int Label1PosX;
    private int Label1PosY;
    private Font Label1Font;
    private Color Label1Color;
    private string Label1PropertyString;
    private string Label1ValueString;

    private int DefaultLabel1PosX = 0;
    private int DefaultLabel1PosY = 0;
    private float DefaultLabel1FontSize = 0;

    private int Label2PosX;
    private int Label2PosY;
    private Font Label2Font;
    private Color Label2Color;
    private string Label2PropertyString;
    private string Label2ValueString;

    private int DefaultLabel2PosX = 0;
    private int DefaultLabel2PosY = 0;
    private float DefaultLabel2FontSize = 0;

    private int Label3PosX;
    private int Label3PosY;
    private Font Label3Font;
    private Color Label3Color;
    private string Label3PropertyString;
    private string Label3ValueString;

    private int DefaultLabel3PosX = 0;
    private int DefaultLabel3PosY = 0;
    private float DefaultLabel3FontSize = 0;

    private int Label4PosX;
    private int Label4PosY;
    private Font Label4Font;
    private Color Label4Color;
    private string Label4PropertyString;
    private string Label4ValueString;

    private int DefaultLabel4PosX = 0;
    private int DefaultLabel4PosY = 0;
    private float DefaultLabel4FontSize = 0;

    //private int TrackInfoTextLeftMargin = 10;
    private int TrackInfoTextRightMargin = 35;

    private float DefaultFontSize = 11f;
    //private Color TextColor = Color.White;
    private Font TextFont = new Font("Arial", 11f, FontStyle.Regular);
    private StringFormat TextStringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();

    #endregion

    //private FormWindowState LastWindowState = FormWindowState.Normal;
    private Size OldSize = Size.Empty;
    private bool VizWindowNeedsResize = false;
    private bool OutputContextNeedsUpdating = false;
    private bool DialogWindowIsActive = false;
    private bool _autoHideMouse = false;

    private Thread VizRenderThread;
    private int VisualizationRenderInterval = 50;
    private bool VisualizationRunning = false;
    private bool ReadyToRender = false;
    private bool _IsPreviewVisualization = false;
    private bool _KeepCoverArtAspectRatio = true;

    private List<string> _ImagesPathsList = new List<string>();
    private List<Image> _CoverArtImages = new List<Image>();
    private bool UpdatingCoverArtImage = false;
    private bool UpdatingCoverArtImageList = false;
    private bool DoImageCleanup = false;

    #endregion

    #region Properties

    public IVisualization Visualization
    {
      get { return Viz; }
      set { Viz = value; }
    }

    public bool Run
    {
      set
      {
        if (value)
        {
          //if (_CompatibleDC == IntPtr.Zero)
          //{
          //    // Force the creation of the _CompatibleDC member...
          //    IntPtr pTemp = CompatibleDC;
          //}

          StartVisualization();
        }

        else
        {
          StopVisualization();
        }
      }
    }

    public IntPtr CompatibleDC
    {
      get
      {
        if (_IsPreviewVisualization)
        {
          return this.Handle;
        }

        if (!_EnableStatusOverlays && GUIGraphicsContext.IsFullScreenVideo)
        {
          return this.Handle;
        }

        if (_CompatibleDC != IntPtr.Zero)
        {
          return _CompatibleDC;
        }

        Graphics g = Graphics.FromHwnd(this.Handle);
        IntPtr hDC = g.GetHdc();
        _CompatibleDC = CreateCompatibleDC(hDC);
        g.ReleaseHdc(hDC);
        g.Dispose();
        return _CompatibleDC;
      }
    }

    public bool EnableStatusOverlays
    {
      get { return _EnableStatusOverlays; }
    }

    public VisualizationBase.OutputContextType OutputContextType
    {
      get
      {
        if (_IsPreviewVisualization || (FullScreen && !_EnableStatusOverlays))
        {
          return VisualizationBase.OutputContextType.WindowHandle;
        }

        else
        {
          return VisualizationBase.OutputContextType.DeviceContext;
        }
      }
    }

    public bool IsPreviewVisualization
    {
      get { return _IsPreviewVisualization; }
      set { _IsPreviewVisualization = value; }
    }

    public string CoverArtImagePath
    {
      set
      {
        CurrentThumbPath = value.ToLower();
        CoverArtNeedsRefresh = true;
      }
    }

    public bool KeepCoverArtAspectRatio
    {
      get { return _KeepCoverArtAspectRatio; }
      set { _KeepCoverArtAspectRatio = value; }
    }

    public List<string> ImagesPathsList
    {
      get { return _ImagesPathsList; }
    }

    public List<Image> CoverArtImages
    {
      get { return _CoverArtImages; }
    }

    public int SelectedPlaybackType
    {
      set
      {
        _playBackType = value;
        PlayBackTypeChanged = true;
      }
    }

    #endregion

    #region ctor
    public VisualizationWindow(BassAudioEngine bass)
    {
      Bass = bass;
      Init();
    }

    ~VisualizationWindow()
    {
      Dispose();
    }
    #endregion

    #region Init ReInit
    /// <summary>
    /// Initialise The Viz Window
    /// </summary>
    private void Init()
    {
      InitializeComponent();
      CheckForIllegalCrossThreadCalls = false;
      PlaylistPlayer = PlayListPlayer.SingletonPlayer;

      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      Bass.InternetStreamSongChanged += new BassAudioEngine.InternetStreamSongChangedDelegate(InternetStreamSongChanged);

      //if (GUIGraphicsContext.form != null)
      //    GUIGraphicsContext.form.Resize += new EventHandler(OnAppFormResize);

      GUIGraphicsContext.OnNewAction += new OnActionHandler(OnNewAction);

      SetStyle(ControlStyles.DoubleBuffer, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.UserPaint, true);

      TextStringFormat.Trimming = StringTrimming.Character;
      TextStringFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox;

      LoadSettings();

      // Soundspectrum Graphics always show the cursor, so let's hide it here
      if (GUIGraphicsContext.Fullscreen && _autoHideMouse)
      {
        Cursor.Hide();
      }
    }

    /// <summary>
    /// ReInit the Visz
    /// </summary>
    public void Reinit()
    {
      bool running = VisualizationRunning;
      Run = false;
      Dispose();
      Init();
      Run = running;
    }
    #endregion

    #region Event Handler
    private void OnNewAction(Action action)
    {
      if (!Visible)
      {
        return;
      }

      switch (action.wID)
      {
        case Action.ActionType.ACTION_SHOW_INFO:
          {
            if (ShowTrackOverlay)
            {
              if (CurrentFrame > 0 && CurrentFrame < FadeFrameCount + ShowTrackInfoFrameCount)
              {
                CurrentFrame = FadeFrameCount + ShowTrackInfoFrameCount;
              }

              else
              {
                CurrentFrame = 0;
                SeekingFF = false;
                SeekingRew = false;
                NewPlay = false;
                NewTrack = true;
                PlayBackTypeChanged = false;
              }
            }

            break;
          }

        case Action.ActionType.ACTION_STOP:
          {
            CurrentFrame = FadeFrameCount + ShowTrackInfoFrameCount;
            CurrentPlayStateFrame = 0;
            SeekingFF = false;
            SeekingRew = false;
            NewPlay = false;
            PlayBackTypeChanged = false;
            break;
          }

        case Action.ActionType.ACTION_PAUSE:
          {
            CurrentPlayStateFrame = 0;
            SeekingFF = false;
            SeekingRew = false;
            NewPlay = false;
            PlayBackTypeChanged = false;
            break;
          }

        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
          {
            CurrentPlayStateFrame = 0;
            SeekingFF = false;
            SeekingRew = false;
            NewPlay = true;
            PlayBackTypeChanged = false;
            break;
          }

        case Action.ActionType.ACTION_FORWARD:
        case Action.ActionType.ACTION_MUSIC_FORWARD:
          {
            CurrentPlayStateFrame = 0;
            SeekingRew = false;
            SeekingFF = true;
            NewPlay = false;
            PlayBackTypeChanged = false;
            break;
          }

        case Action.ActionType.ACTION_REWIND:
        case Action.ActionType.ACTION_MUSIC_REWIND:
          {
            CurrentPlayStateFrame = 0;
            SeekingFF = false;
            SeekingRew = true;
            NewPlay = false;
            PlayBackTypeChanged = false;
            break;
          }

        case Action.ActionType.ACTION_PREV_ITEM:
        case Action.ActionType.ACTION_NEXT_ITEM:
          {
            CurrentPlayStateFrame = 0;
            SeekingFF = false;
            SeekingRew = false;
            NewPlay = true;
            PlayBackTypeChanged = false;
            break;
          }
      }
    }

    /// <summary>
    /// Called when the Meta Data Tags of an Internet Stream changed
    /// </summary>
    /// <param name="sender"></param>
    private void InternetStreamSongChanged(object sender)
    {
      OnPlayBackStarted(g_Player.MediaType.Music, "http");
    }

    private void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      try
      {
        if (CurrentFilePath != filename || filename.ToLower().StartsWith("http") || filename.ToLower().StartsWith("mms"))
        {
          if (type != g_Player.MediaType.Music)
          {
            return;
          }

          CurrentFilePath = filename;
          PlayListItem curPlaylistItem = PlaylistPlayer.GetCurrentItem();
          if (curPlaylistItem == null)
          {
            return;
          }

          bool IsInternetStream = false;

          // When playing an Internet Stream, we need to clear the Coverart image, that is maybe there when music was played before
          if (CurrentFilePath.ToLower().StartsWith("http") || CurrentFilePath.ToLower().StartsWith("mms"))
          {
            IsInternetStream = true;
            if (CurrentThumbImage != null)
            {
              CurrentThumbImage.Dispose();
              CurrentThumbImage = null;
            }
          }

          CurrentTrackTag = (MusicTag)curPlaylistItem.MusicTag;

          // Make sure that Status Overlay gets displayed for new tracks
          CurrentFrame = 0;

          // We only need to get this data if we're going to display
          if (_EnableStatusOverlays)
          {
            Label1ValueString = GetPropertyStringValue(Label1PropertyString, IsInternetStream);
            Label2ValueString = GetPropertyStringValue(Label2PropertyString, IsInternetStream);
            Label3ValueString = GetPropertyStringValue(Label3PropertyString, IsInternetStream);
            Label4ValueString = GetPropertyStringValue(Label4PropertyString, IsInternetStream);
          }

          if (TrackInfoImage != null)
          {
            if (CurrentTrackInfoImage != null)
            {
              CurrentTrackInfoImage.Dispose();
              CurrentTrackInfoImage = null;
            }

            NewTrack = true;
          }

          if (CurrentTrackTag != null)
          {
            string thumbPath = GetAlbumOrFolderThumb(filename, CurrentTrackTag.Artist, CurrentTrackTag.Album);

            if (thumbPath.ToLower().CompareTo(CurrentThumbPath) != 0)
            {
              CurrentThumbPath = thumbPath.ToLower();
              CoverArtNeedsRefresh = true;
            }
          }

          // Get Coverart for the played internet Stream
          if (IsInternetStream)
          {
            string StationName = GetPropertyStringValue("#Play.Current.ArtistThumb", IsInternetStream);
            string thumbnail = Util.Utils.GetCoverArt(Thumbs.Radio, StationName);
            if (File.Exists(thumbnail))
            {
              CurrentThumbPath = thumbnail.ToLower();
              CoverArtNeedsRefresh = true;
            }
            else
            {
              // try with the name returned inside the "icy-name" tag, which we stored in the Album property
              StationName = GetPropertyStringValue("#Play.Current.Album", IsInternetStream);
              thumbnail = Util.Utils.GetCoverArt(Thumbs.Radio, StationName);
              if (File.Exists(thumbnail))
              {
                CurrentThumbPath = thumbnail.ToLower();
                CoverArtNeedsRefresh = true;
              }
            }
          }
        }
      }

      catch (Exception ex)
      {
        Log.Info("Visualization Window: OnPlayBackStarted caused an exception: {0}", ex.Message);
      }
    }
    #endregion

    #region Load Settings
    private void LoadSettings()
    {
      Log.Info("Visualization Window: Loading skin settings...");

      try
      {
        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          _EnableStatusOverlays = xmlreader.GetValueAsBool("musicvisualization", "enableStatusOverlays", false);
          ShowTrackOverlay = xmlreader.GetValueAsBool("musicvisualization", "showTrackInfo", true);
          _autoHideMouse = xmlreader.GetValueAsBool("general", "autohidemouse", true);
        }

        // No need to load the skin file if we're not going to be showing the 
        // TrackInfo overlay
        if (!_EnableStatusOverlays || !ShowTrackOverlay)
        {
          return;
        }

        // If we got heare as a result of the Configuration.exe app there's no skin file to load
        if (Process.GetCurrentProcess().ProcessName.Equals("Configuration"))
        {
          return;
        }

        string skinFilePath = Path.Combine(Application.StartupPath,
                                           GUIGraphicsContext.Skin + @"\MyMusicFullScreenVisualization.xml");

        XmlDocument doc = new XmlDocument();
        doc.Load(skinFilePath);

        if (doc.DocumentElement == null)
        {
          return;
        }

        XmlNodeList nodeList = doc.DocumentElement.SelectNodes("/controls/*");

        foreach (XmlNode node in nodeList)
        {
          if (node.Name == null)
          {
            continue;
          }

          switch (node.Name)
          {
            case "control":
              LoadSettings(node);
              break;

            default:
              break;
          }
        }

        Log.Info("Visualization Window: Done loading skin settings");
      }

      catch (Exception ex)
      {
        Log.Info("Visualization Window: An exception occured while loading skin settings: {0}", ex.Message);
      }
    }

    protected void LoadSettings(XmlNode node)
    {
      if (node == null)
      {
        return;
      }

      try
      {
        if (node["id"] == null)
        {
          return;
        }

        int ctrlID = int.Parse(node["id"].InnerText);

        switch (ctrlID)
        {
          case (int)ControlID.OverlayImage:
            LoadOverlayImageSettings(node);
            break;

          case (int)ControlID.CoverArtImage:
            LoadCoverArtSettings(node);
            break;

          case (int)ControlID.MissingCoverArtImage:
            LoadMissingCoverArtSettings(node);
            break;

          case (int)ControlID.Label1:
            LoadLabel1Settings(node);
            break;

          case (int)ControlID.Label2:
            LoadLabel2Settings(node);
            break;

          case (int)ControlID.Label3:
            LoadLabel3Settings(node);
            break;

          case (int)ControlID.Label4:
            LoadLabel4Settings(node);
            break;

          case (int)ControlID.PauseIcon:
          case (int)ControlID.PlayIcon:
          case (int)ControlID.FFIcon:
          case (int)ControlID.RewIcon:
          case (int)ControlID.StopIcon:
          case (int)ControlID.CrossfadeIcon:
          case (int)ControlID.GapIcon:
          case (int)ControlID.GaplessIcon:
            LoadPlayStateImage(node, ctrlID);
            break;
        }
      }

      catch (Exception ex)
      {
        Log.Info("Unable to load control. exception:{0}", ex.ToString());
      }
    }

    private void LoadOverlayImageSettings(XmlNode node)
    {
      if (node["type"] == null || node["type"].InnerText != "nowplayingoverlay")
      {
        return;
      }

      DefaultTrackInfoImageXPos = TrackInfoImageXPos = GetSettingIntValue(node["posX"], TrackInfoImageXPos);
      DefaultTrackInfoImageYPos = TrackInfoImageYPos = GetSettingIntValue(node["posY"], TrackInfoImageYPos);
      DefaultTrackInfoImageWidth = TrackInfoImageWidth = GetSettingIntValue(node["width"], TrackInfoImageWidth);
      DefaultTrackInfoImageHeight = TrackInfoImageHeight = GetSettingIntValue(node["height"], TrackInfoImageHeight);

      TrackInfoImageName = GetSettingStringValue(node["texture"], "VisualizationTrackInfo.png");
      TrackInfoOverlayAlpha = GetSettingByteValue(node["alpha"], TrackInfoOverlayAlpha);

      if (TrackInfoImage != null)
      {
        TrackInfoImage.Dispose();
        TrackInfoImage = null;
      }

      if (TrackInfoImageName.Length > 0)
      {
        string imagePath = Path.Combine(Application.StartupPath,
                                        string.Format(@"{0}\Media\{1}", GUIGraphicsContext.Skin, TrackInfoImageName));

        if (File.Exists(imagePath))
        {
          try
          {
            TrackInfoImage = Image.FromFile(imagePath);
          }

          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
        }
      }
    }

    private void LoadCoverArtSettings(XmlNode node)
    {
      if (node["type"] == null || node["type"].InnerText != "image")
      {
        return;
      }

      DefaultCoverArtXOffset = CoverArtXOffset = GetSettingIntValue(node["posX"], CoverArtXOffset);
      DefaultCoverArtYOffset = CoverArtYOffset = GetSettingIntValue(node["posY"], CoverArtYOffset);
      DefaultCoverArtWidth = CoverArtWidth = GetSettingIntValue(node["width"], CoverArtWidth);
      DefaultCoverArtHeight = CoverArtHeight = GetSettingIntValue(node["height"], CoverArtHeight);
    }

    private void LoadMissingCoverArtSettings(XmlNode node)
    {
      if (node["type"] == null || node["type"].InnerText != "image")
      {
        return;
      }

      MissingCoverArtImageName = GetSettingStringValue(node["texture"], "missing_coverart.png");

      if (MissingCoverArtImage != null)
      {
        MissingCoverArtImage.Dispose();
        MissingCoverArtImage = null;
      }

      if (MissingCoverArtImageName.Length > 0)
      {
        string imagePath = Path.Combine(Application.StartupPath,
                                        string.Format(@"{0}\Media\{1}", GUIGraphicsContext.Skin,
                                                      MissingCoverArtImageName));

        if (File.Exists(imagePath))
        {
          try
          {
            MissingCoverArtImage = Image.FromFile(imagePath);
          }

          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
          }
        }
      }
    }

    private void LoadLabel1Settings(XmlNode node)
    {
      if (node["type"] == null || node["type"].InnerText != "label")
      {
        return;
      }

      DefaultLabel1PosX = Label1PosX = GetSettingIntValue(node["posX"], Label1PosX);
      DefaultLabel1PosY = Label1PosY = GetSettingIntValue(node["posY"], Label1PosY);
      Label1PropertyString = GetSettingStringValue(node["label"], Label1PropertyString);
      Label1Font = GetSettingFontValue(node["font"], node["bold"], node["italic"], TextFont);
      DefaultLabel1FontSize = Label1Font.Size;

      Label1Color = GetSettingColorValue(node["textcolor"], Color.White);
    }

    private void LoadLabel2Settings(XmlNode node)
    {
      if (node["type"] == null || node["type"].InnerText != "label")
      {
        return;
      }

      DefaultLabel2PosX = Label2PosX = GetSettingIntValue(node["posX"], Label2PosX);
      DefaultLabel2PosY = Label2PosY = GetSettingIntValue(node["posY"], Label2PosY);
      Label2PropertyString = GetSettingStringValue(node["label"], Label2PropertyString);
      Label2Font = GetSettingFontValue(node["font"], node["bold"], node["italic"], TextFont);
      DefaultLabel2FontSize = Label2Font.Size;

      Label2Color = GetSettingColorValue(node["textcolor"], Color.White);
    }

    private void LoadLabel3Settings(XmlNode node)
    {
      if (node["type"] == null || node["type"].InnerText != "label")
      {
        return;
      }

      DefaultLabel3PosX = Label3PosX = GetSettingIntValue(node["posX"], Label3PosX);
      DefaultLabel3PosY = Label3PosY = GetSettingIntValue(node["posY"], Label3PosY);
      Label3PropertyString = GetSettingStringValue(node["label"], Label3PropertyString);
      Label3Font = GetSettingFontValue(node["font"], node["bold"], node["italic"], TextFont);
      DefaultLabel3FontSize = Label3Font.Size;

      Label3Color = GetSettingColorValue(node["textcolor"], Color.White);
    }

    private void LoadLabel4Settings(XmlNode node)
    {
      if (node["type"] == null || node["type"].InnerText != "label")
      {
        return;
      }

      DefaultLabel4PosX = Label4PosX = GetSettingIntValue(node["posX"], Label4PosX);
      DefaultLabel4PosY = Label4PosY = GetSettingIntValue(node["posY"], Label4PosY);
      Label4PropertyString = GetSettingStringValue(node["label"], Label4PropertyString);
      Label4Font = GetSettingFontValue(node["font"], node["bold"], node["italic"], TextFont);
      DefaultLabel4FontSize = Label4Font.Size;

      Label4Color = GetSettingColorValue(node["textcolor"], Color.White);
    }

    private void LoadPlayStateImage(XmlNode node, int ctrlID)
    {
      if (node["type"] == null || node["type"].InnerText != "image")
      {
        return;
      }

      string imgName = string.Empty;
      string imgPath = string.Empty;
      Image img = null;

      imgName = GetSettingStringValue(node["texture"], "");

      if (imgName.Length == 0)
      {
        return;
      }

      imgPath = Path.Combine(Application.StartupPath,
                             string.Format(@"{0}\Media\{1}", GUIGraphicsContext.Skin, imgName));

      if (!File.Exists(imgPath))
      {
        return;
      }

      try
      {
        img = Image.FromFile(imgPath);
      }

      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      int imgX = GetSettingIntValue(node["posX"], 0);
      int imgY = GetSettingIntValue(node["posY"], 0);
      int imgWidth = GetSettingIntValue(node["width"], 1);
      int imgHeight = GetSettingIntValue(node["height"], 1);

      if (ctrlID == (int)ControlID.PauseIcon)
      {
        if (PauseImage != null)
        {
          PauseImage.Dispose();
          PauseImage = null;
        }

        if (img == null)
        {
          return;
        }

        PauseImageName = imgName;
        DefaultPauseImageX = PauseImageX = imgX;
        DefaultPauseImageY = PauseImageY = imgY;
        DefaultPauseImageWidth = imgWidth;
        DefaultPauseImageHeight = imgHeight;

        PauseImage = img;
      }

      else if (ctrlID == (int)ControlID.PlayIcon)
      {
        if (PlayImage != null)
        {
          PlayImage.Dispose();
          PlayImage = null;
        }

        if (img == null)
        {
          return;
        }

        PlayImageName = imgName;
        DefaultPlayImageX = PlayImageX = imgX;
        DefaultPlayImageY = PlayImageY = imgY;
        DefaultPlayImageWidth = imgWidth;
        DefaultPlayImageHeight = imgHeight;
        PlayImage = img;
      }

      else if (ctrlID == (int)ControlID.FFIcon)
      {
        if (FFImage != null)
        {
          FFImage.Dispose();
          FFImage = null;
        }

        if (img == null)
        {
          return;
        }

        FFImageName = imgName;
        DefaultFFImageX = FFImageX = imgX;
        DefaultFFImageY = FFImageY = imgY;
        DefaultFFImageWidth = imgWidth;
        DefaultFFImageHeight = imgHeight;
        FFImage = img;
      }

      else if (ctrlID == (int)ControlID.RewIcon)
      {
        if (RewImage != null)
        {
          RewImage.Dispose();
          RewImage = null;
        }

        if (img == null)
        {
          return;
        }

        RewImageName = imgName;
        DefaultRewImageX = RewImageX = imgX;
        DefaultRewImageY = RewImageY = imgY;
        DefaultRewImageWidth = imgWidth;
        DefaultRewImageHeight = imgHeight;
        RewImage = img;
      }

      else if (ctrlID == (int)ControlID.StopIcon)
      {
        if (StopImage != null)
        {
          StopImage.Dispose();
          StopImage = null;
        }

        if (img == null)
        {
          return;
        }

        //StopImageName = imgName;
        DefaultStopImageX = StopImageX = imgX;
        DefaultStopImageY = StopImageY = imgY;
        DefaultStopImageWidth = imgWidth;
        DefaultStopImageHeight = imgHeight;
        StopImage = img;
      }

      else if (ctrlID == (int)ControlID.CrossfadeIcon)
      {
        if (CrossfadeImage != null)
        {
          CrossfadeImage.Dispose();
          CrossfadeImage = null;
        }

        if (img == null)
        {
          return;
        }

        CrossfadeImageName = imgName;
        DefaultCrossfadeImageX = CrossfadeImageX = imgX;
        DefaultCrossfadeImageY = CrossfadeImageY = imgY;
        DefaultCrossfadeImageWidth = imgWidth;
        DefaultCrossfadeImageHeight = imgHeight;
        CrossfadeImage = img;
      }

      else if (ctrlID == (int)ControlID.GapIcon)
      {
        if (GapImage != null)
        {
          GapImage.Dispose();
          GapImage = null;
        }

        if (img == null)
        {
          return;
        }

        GapImageName = imgName;
        DefaultGapImageX = GapImageX = imgX;
        DefaultGapImageY = GapImageY = imgY;
        DefaultGapImageWidth = imgWidth;
        DefaultGapImageHeight = imgHeight;
        GapImage = img;
      }

      else if (ctrlID == (int)ControlID.GaplessIcon)
      {
        if (GaplessImage != null)
        {
          GaplessImage.Dispose();
          GaplessImage = null;
        }

        if (img == null)
        {
          return;
        }

        GaplessImageName = imgName;
        DefaultGaplessImageX = GaplessImageX = imgX;
        DefaultGaplessImageY = GaplessImageY = imgY;
        DefaultGaplessImageWidth = imgWidth;
        DefaultGaplessImageHeight = imgHeight;
        GaplessImage = img;
      }
    }

    private int GetSettingIntValue(XmlElement element, int defaultValue)
    {
      try
      {
        if (element == null || element.InnerText.Length == 0)
        {
          return defaultValue;
        }

        int val = int.Parse(element.InnerText);
        return val;
      }

      catch
      {
        return defaultValue;
      }
    }

    private byte GetSettingByteValue(XmlElement element, byte defaultValue)
    {
      try
      {
        if (element == null || element.InnerText.Length == 0)
        {
          return defaultValue;
        }

        string sHexVal = element.InnerText.ToLower();

        if (sHexVal.Length > 2 && sHexVal[0] == '0' && sHexVal[1] == 'x')
        {
          sHexVal = sHexVal.Substring(2);
        }

        byte val = byte.Parse(sHexVal, NumberStyles.HexNumber);
        return val;
      }

      catch
      {
        return defaultValue;
      }
    }

    private string GetSettingStringValue(XmlElement element, string defaultValue)
    {
      try
      {
        if (element == null || element.InnerText.Length == 0 || element.InnerText == "-")
        {
          return defaultValue;
        }

        else
        {
          return element.InnerText;
        }
      }

      catch
      {
        return defaultValue;
      }
    }

    private Font GetSettingFontValue(XmlElement fontNameElement, XmlElement boldElement, XmlElement italicElement,
                                     Font defaultValue)
    {
      try
      {
        if (fontNameElement == null || fontNameElement.InnerText.Length == 0 || fontNameElement.InnerText == "-")
        {
          return defaultValue;
        }

        else
        {
          bool isBold = false;
          bool isItalic = false;

          try
          {
            if (boldElement != null && boldElement.InnerText.Length > 0)
            {
              isBold = boldElement.InnerText.ToLower() == "yes";
            }

            if (italicElement != null && italicElement.InnerText.Length > 0)
            {
              isItalic = italicElement.InnerText.ToLower() == "yes";
            }
          }

          catch (Exception ex)
          {
            Console.WriteLine("GetSettingFontValue caused an exception:{0}", ex);
          }

          string guiFontName = fontNameElement.InnerText;
          Font font = null;
          GUIFont tempGuiFont = GUIFontManager.GetFont(guiFontName);

          if (tempGuiFont == null)
          {
            return defaultValue;
          }

          string fontName = tempGuiFont.FileName;
          int fontSize = tempGuiFont.FontSize;
          FontStyle style = FontStyle.Regular;

          if (isBold)
          {
            style = FontStyle.Bold;
          }

          if (isItalic)
          {
            style |= FontStyle.Italic;
          }

          font = new Font(fontName, (float)fontSize, style);
          return font;
        }
      }

      catch
      {
        return defaultValue;
      }
    }

    private Color GetSettingColorValue(XmlElement element, Color defaultValue)
    {
      try
      {
        if (element == null || element.InnerText.Length == 0 || element.InnerText == "-")
        {
          return defaultValue;
        }

        else
        {
          Color color = ColorTranslator.FromHtml("#" + element.InnerText);
          return color;
        }
      }

      catch
      {
        return defaultValue;
      }
    }

    private float GetOpacity(byte alphaVal)
    {
      if (alphaVal == 0)
      {
        return 0f;
      }

      return (float)alphaVal / 255f;
    }
    #endregion

    private string GetPropertyStringValue(string propertyString, bool IsInternetStream)
    {
      try
      {
        if (propertyString == null || propertyString.Length == 0 || propertyString[0] != '#')
        {
          return string.Empty;
        }

        if (IsInternetStream)
        {
          return GUIPropertyManager.GetProperty(propertyString);
        }

        if (CurrentTrackTag == null)
        {
          return string.Empty;
        }

        // There's a potential timing issue here; if we call GUIPropertyManager.GetProperty right after
        // a g_Player.PlayBackStarted event, sometimes the properties haven't been set yet.  To avoid this 
        // we return the equivalent property from the MusicTag...

        switch (propertyString)
        {
          case "#Play.Current.Title":
            return CurrentTrackTag.Title;

          case "#Play.Current.Genre":
            return CurrentTrackTag.Genre;

          case "#Play.Current.Artist":
            return CurrentTrackTag.Artist;

          case "#Play.Current.Album":
            return CurrentTrackTag.Album;

          case "#Play.Current.Track":
            return CurrentTrackTag.Track.ToString();

          case "#Play.Current.Year":
            return CurrentTrackTag.Year.ToString();

          case "#Play.Current.Duration":
            return CurrentTrackTag.Duration.ToString();

          default:
            return string.Empty;
        }
      }

      catch (Exception ex)
      {
        Log.Info("Visualization Window: GetPropertyStringValue caused an exception: {0}", ex.Message);
      }

      return string.Empty;
    }

    public new void Dispose()
    {
      StopVisualization();

      if (VizRenderThread != null && VizRenderThread.IsAlive)
      {
        VizRenderThread.Abort();
      }

      if (_CompatibleDC != IntPtr.Zero)
      {
        DeleteDC(_CompatibleDC);
        _CompatibleDC = IntPtr.Zero;
      }

      if (BackBuffer != null)
      {
        BackBuffer.Dispose();
        BackBuffer = null;
      }
    }

    private bool NeedsResize()
    {
      if (OldSize.Equals(this.Size) && FullScreen == GUIGraphicsContext.IsFullScreenVideo)
      {
        return false;
      }

      else
      {
        return true;
      }
    }

    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);

      // Don't call DoResize unless the window size has really changed
      if (!NeedsResize())
      {
        return;
      }

      DoResize();
    }

    private void DoResize()
    {
      if (Size.Width <= 1 || Size.Height <= 1)
      {
        OldSize = Size;
        return;
      }

      Size oldSize = Size;

      if (hNativeMemDCBmp != IntPtr.Zero)
      {
        bool result = DeleteObject(hNativeMemDCBmp);
        hNativeMemDCBmp = IntPtr.Zero;
      }

      if (BackBuffer != null)
      {
        BackBuffer.Dispose();
        BackBuffer = null;
      }

      FullScreen = GUIGraphicsContext.IsFullScreenVideo;
      OutputContextNeedsUpdating = true;

      if (FullScreen)
      {
        // Make sure we always start at a fade-in frame when we go to fullscreen mode
        CurrentFrame = 0;
        if (GUIGraphicsContext.Fullscreen)
        {
          // Move the cursor to bottom right, as some vis don't allow to hide the cursor
          Cursor.Position = new Point(GUIGraphicsContext.form.Bounds.Width, GUIGraphicsContext.form.Bounds.Height);
          IsCursorMovedBottomRight = true;
        }
      }
      else
      {
        // Make sure we always start at a coverart frame when return from fullscreen mode
        CurrentFrame = FadeFrameCount;
        if (IsCursorMovedBottomRight)
        {
          // Set the cursor back to the old position
          Cursor.Position = new Point(GUIGraphicsContext.form.Bounds.Width / 2, GUIGraphicsContext.form.Bounds.Height / 2);
          IsCursorMovedBottomRight = false;
        }
      }

      try
      {
        if (_EnableStatusOverlays)
        {
          // Make sure we're using the original defaults...
          TrackInfoImageWidth = DefaultTrackInfoImageWidth;
          TrackInfoImageHeight = DefaultTrackInfoImageHeight;
          TrackInfoImageXPos = DefaultTrackInfoImageXPos;
          TrackInfoImageYPos = DefaultTrackInfoImageYPos;

          CoverArtWidth = DefaultCoverArtWidth;
          CoverArtHeight = DefaultCoverArtHeight;
          CoverArtXOffset = DefaultCoverArtXOffset;
          CoverArtYOffset = DefaultCoverArtYOffset;

          PauseImageWidth = DefaultPauseImageWidth;
          PauseImageHeight = DefaultPauseImageHeight;
          PauseImageX = DefaultPauseImageX;
          PauseImageY = DefaultPauseImageY;

          PlayImageWidth = DefaultPlayImageWidth;
          PlayImageHeight = DefaultPlayImageHeight;
          PlayImageX = DefaultPlayImageX;
          PlayImageY = DefaultPlayImageY;

          FFImageWidth = DefaultFFImageWidth;
          FFImageHeight = DefaultFFImageHeight;
          FFImageX = DefaultFFImageX;
          FFImageY = DefaultFFImageY;

          RewImageWidth = DefaultRewImageWidth;
          RewImageHeight = DefaultRewImageHeight;
          RewImageX = DefaultRewImageX;
          RewImageY = DefaultRewImageY;

          StopImageWidth = DefaultStopImageWidth;
          StopImageHeight = DefaultStopImageHeight;
          StopImageX = DefaultStopImageX;
          StopImageY = DefaultStopImageY;

          CrossfadeImageWidth = DefaultCrossfadeImageWidth;
          CrossfadeImageHeight = DefaultCrossfadeImageHeight;
          CrossfadeImageX = DefaultCrossfadeImageX;
          CrossfadeImageY = DefaultCrossfadeImageY;

          GapImageWidth = DefaultGapImageWidth;
          GapImageHeight = DefaultGapImageHeight;
          GapImageX = DefaultGapImageX;
          GapImageY = DefaultGapImageY;

          GaplessImageWidth = DefaultGaplessImageWidth;
          GaplessImageHeight = DefaultGaplessImageHeight;
          GaplessImageX = DefaultGaplessImageX;
          GaplessImageY = DefaultGaplessImageY;

          Label1PosX = DefaultLabel1PosX;
          Label1PosY = DefaultLabel1PosY;

          Label2PosX = DefaultLabel2PosX;
          Label2PosY = DefaultLabel2PosY;

          Label3PosX = DefaultLabel3PosX;
          Label3PosY = DefaultLabel3PosY;

          Label4PosX = DefaultLabel4PosX;
          Label4PosY = DefaultLabel4PosY;

          int fontSize = 0;

          // TrackInfo Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref TrackInfoImageWidth, ref TrackInfoImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref TrackInfoImageXPos);
          GUIGraphicsContext.ScaleVertical(ref TrackInfoImageYPos);

          // CoverArt Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref CoverArtWidth, ref CoverArtHeight);
          GUIGraphicsContext.ScaleHorizontal(ref CoverArtXOffset);
          GUIGraphicsContext.ScaleVertical(ref CoverArtYOffset);

          // Pause Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref PauseImageWidth, ref PauseImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref PauseImageX);
          GUIGraphicsContext.ScaleVertical(ref PauseImageY);

          // Play Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref PlayImageWidth, ref PlayImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref PlayImageX);
          GUIGraphicsContext.ScaleVertical(ref PlayImageY);

          // FF Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref FFImageWidth, ref FFImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref FFImageX);
          GUIGraphicsContext.ScaleVertical(ref FFImageY);

          // Rew Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref RewImageWidth, ref RewImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref RewImageX);
          GUIGraphicsContext.ScaleVertical(ref RewImageY);

          // Stop Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref StopImageWidth, ref StopImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref StopImageX);
          GUIGraphicsContext.ScaleVertical(ref StopImageY);

          // Crossfade Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref CrossfadeImageWidth, ref CrossfadeImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref CrossfadeImageX);
          GUIGraphicsContext.ScaleVertical(ref CrossfadeImageY);

          // Gap Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref GapImageWidth, ref GapImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref GapImageX);
          GUIGraphicsContext.ScaleVertical(ref GapImageY);

          // Gapless Image
          GUIGraphicsContext.ScalePosToScreenResolution(ref GaplessImageWidth, ref GaplessImageHeight);
          GUIGraphicsContext.ScaleHorizontal(ref GaplessImageX);
          GUIGraphicsContext.ScaleVertical(ref GaplessImageY);

          // Label1
          GUIGraphicsContext.ScaleHorizontal(ref Label1PosX);
          GUIGraphicsContext.ScaleVertical(ref Label1PosY);
          fontSize = (int)DefaultLabel1FontSize;
          GUIGraphicsContext.ScaleVertical(ref fontSize);
          Label1Font = new Font(Label1Font.FontFamily, (float)fontSize, Label1Font.Style);

          // Label2
          GUIGraphicsContext.ScaleHorizontal(ref Label2PosX);
          GUIGraphicsContext.ScaleVertical(ref Label2PosY);
          fontSize = (int)DefaultLabel2FontSize;
          GUIGraphicsContext.ScaleVertical(ref fontSize);
          Label2Font = new Font(Label2Font.FontFamily, (float)fontSize, Label2Font.Style);

          // Label3
          GUIGraphicsContext.ScaleHorizontal(ref Label3PosX);
          GUIGraphicsContext.ScaleVertical(ref Label3PosY);
          fontSize = (int)DefaultLabel3FontSize;
          GUIGraphicsContext.ScaleVertical(ref fontSize);
          Label3Font = new Font(Label3Font.FontFamily, (float)fontSize, Label3Font.Style);

          // Label4
          GUIGraphicsContext.ScaleHorizontal(ref Label4PosX);
          GUIGraphicsContext.ScaleVertical(ref Label4PosY);
          fontSize = (int)DefaultLabel4FontSize;
          GUIGraphicsContext.ScaleVertical(ref fontSize);
          Label4Font = new Font(Label4Font.FontFamily, (float)fontSize, Label4Font.Style);

          fontSize = (int)DefaultFontSize;
          GUIGraphicsContext.ScaleVertical(ref fontSize);
          TextFont = new Font(TextFont.FontFamily, (float)fontSize, TextFont.Style);

          if (CurrentTrackInfoImage != null)
          {
            CurrentTrackInfoImage.Dispose();
            CurrentTrackInfoImage = null;
          }
        }

        if (!VisualizationRunning)
        {
          if (Viz != null && Viz.Initialized)
          {
            Viz.WindowSizeChanged(Size);
          }

          else
          {
            VizWindowNeedsResize = true;
          }
        }

        else
        {
          Viz.WindowSizeChanged(Size);
        }
      }

      catch (Exception ex)
      {
        Console.WriteLine("DoResize caused an exception:{0}", ex.StackTrace);
      }

      finally
      {
        OldSize = oldSize;
      }
    }

    internal void SetVisualizationTimer(int targetFPS)
    {
      if (targetFPS < 0)
      {
        targetFPS = 20;
      }

      else if (targetFPS > 40)
      {
        targetFPS = 40;
      }

      float displayseconds = (float)PlayStateDisplayMS / 1000;
      ShowPlayStateFrameCount = (int)((float)targetFPS * (float)displayseconds);

      displayseconds = TrackInfoDisplayMS / 1000;
      ShowTrackInfoFrameCount = (int)((float)targetFPS * (float)displayseconds);

      float interval = 1000 / targetFPS;
      VisualizationRenderInterval = (int)interval;
    }

    internal void StartVisualization()
    {
      if (VisualizationRunning)
      {
        return;
      }

      Visible = true;
      Refresh();
      Application.DoEvents();

      // Soundspectrum Viz need special handling on Render
      if (Viz.IsEngineInstalled())
      {
        IsSoundSpectrumViz = true;
      }
      else
      {
        IsSoundSpectrumViz = false;
      }

      // The first Render call can take quite a long time to return so we use a seperate worker thread 
      // for the first call.  Once the call returns we let the main render thread handle the rendering
      if (!Viz.PreRenderRequired)
      {
        ReadyToRender = true;
      }

      if (!ReadyToRender)
      {
        Thread t;
        ThreadStart firstRenderTs = new ThreadStart(DoFirstRender);
        t = new Thread(firstRenderTs);
        t.IsBackground = true;
        t.Name = "Viz Window Starter";
        t.Start();
      }

      VisualizationRunning = true;
      ThreadStart renderTs = new ThreadStart(this.RunRenderThread);
      VizRenderThread = new Thread(renderTs);
      VizRenderThread.Priority = ThreadPriority.AboveNormal;
      VizRenderThread.IsBackground = true;
      VizRenderThread.Name = "VizRenderer";
      VizRenderThread.Start();
    }

    internal void StopVisualization()
    {
      if (VizRenderThread != null)
      {
        VisualizationRunning = false;

        int maxWaitMS = 1 * 1000;
        int sleepMS = 100;
        bool threadShutDown = true;

        while (VizRenderThread.IsAlive)
        {
          Thread.Sleep(sleepMS);
          maxWaitMS -= sleepMS;

          if (maxWaitMS <= 0)
          {
            threadShutDown = false;
            break;
          }
        }

        if (threadShutDown)
        {
          VizRenderThread = null;
        }

        else
        {
          if (VizRenderThread != null)
          {
            if (VizRenderThread.IsAlive)
            {
              VizRenderThread.Abort();
            }

            VizRenderThread = null;
          }
        }
      }

      VisualizationRunning = false;
    }

    private void RunRenderThread()
    {
      while (VisualizationRunning)
      {
        if (Visible && ReadyToRender)
        {
          if (OutputContextNeedsUpdating || FullScreen != GUIGraphicsContext.IsFullScreenVideo)
          {
            OutputContextNeedsUpdating = false;
            SetOutputContext();
          }

          if (VizWindowNeedsResize)
          {
            Viz.WindowSizeChanged(Size);
            VizWindowNeedsResize = false;
          }

          if (_IsPreviewVisualization)
          {
            int sleepMS = Viz.RenderVisualization();

            if (sleepMS < 0)
            {
              sleepMS = 1;
            }

            //System.Threading.Thread.Sleep(VisualizationRenderInterval + sleepMS);
            Thread.Sleep(VisualizationRenderInterval);
          }

          else
          {
            if (GUIWindowManager.IsRouted)
            {
              DialogWindowIsActive = true;
              this.Hide(); // Hide the Vis, so it doesn't overlay the Dialogue
              Invalidate();
              Thread.Sleep(VisualizationRenderInterval);
            }

            else
            {
              if (DialogWindowIsActive)
              {
                this.Show();
              }

              DialogWindowIsActive = false;
              Graphics g = Graphics.FromHwnd(Handle);
              int sleepMS = RenderVisualization(g);

              if (sleepMS < 0)
              {
                sleepMS = 0;
              }

              g.Dispose();

              // Is it a Soundspectrum Viz, then we use, what their render returned in sleepMS
              if (IsSoundSpectrumViz)
              {
                Thread.Sleep(sleepMS);
              }
              else
              {
                Thread.Sleep(VisualizationRenderInterval);
              }
            }
          }
        }

        else
        {
          Thread.Sleep(200);
        }
      }
    }

    private void DoFirstRender()
    {
      CurrentFrame = ShowTrackInfoFrameCount;
      Visible = true;
      Refresh();

      Viz.PreRenderVisualization();
      ReadyToRender = true;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
      // Do nothing
      //base.OnPaintBackground(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      if (_IsPreviewVisualization)
      {
        // The RunRenderThread method will take care of the viz rendering calls
        if (ReadyToRender)
        {
          return;
        }

        else
        {
          base.OnPaint(e);
        }
      }

      // If our render thread is still waiting for the first render to 
      // complete we'll let the main thread do the rendering
      if (!ReadyToRender || !DialogWindowIsActive)
      {
        if (_EnableStatusOverlays || !FullScreen)
        {
          RenderVisualization(e.Graphics);
        }

        //                else
        //                    base.OnPaint(e);
      }

      else
      {
        RenderVisualization(e.Graphics);
      }
    }

    private int RenderVisualization(Graphics g)
    {
      int sleepMS = 10;
      CurrentFrame++;

      try
      {
        if (_EnableStatusOverlays || !FullScreen)
        {
          if (DialogWindowIsActive)
          {
            return 10;
          }

          if (Width <= 1 || Height <= 1)
          {
            return 5;
          }

          if (CoverArtNeedsRefresh)
          {
            LoadThumbnail(CurrentThumbPath);
            CurrentFrame = 1;
          }

          // If the visualization engine isn't ready yet check if we have a cover art image.
          // If we do, draw the image and bail out...
          if (!ReadyToRender || Viz == null || !Viz.Initialized)
          {
            if (!_EnableStatusOverlays && !FullScreen)
            {
              if (CurrentThumbImage != null && !UpdatingCoverArtImage)
              {
                Bitmap bmp = new Bitmap(Width, Height);
                Graphics gBmp = Graphics.FromImage(bmp);
                gBmp.Clear(Color.Black);
                DrawThumbnailOverlay(gBmp, 1.0f);
                gBmp.Dispose();
                g.DrawImageUnscaled(bmp, 0, 0);
                bmp.Dispose();
                bmp = null;
              }

              else
              {
                g.Clear(Color.Black);
              }
            }

            else
            {
              if (!FullScreen && CurrentThumbImage != null && !UpdatingCoverArtImage)
              {
                DrawThumbnailOverlay(g, 1.0f);
              }

              else
              {
                g.Clear(Color.Black);
              }
            }

            return sleepMS;
          }

          if (Viz != null && Viz.Initialized)
          {
            if (hNativeMemDCBmp == IntPtr.Zero)
            {
              IntPtr hDestHDC = g.GetHdc();
              try
              {
                hNativeMemDCBmp = CreateCompatibleBitmap(hDestHDC, Width, Height);
              }
              finally
              {
                g.ReleaseHdc(hDestHDC);
              }
              BackBuffer = Bitmap.FromHbitmap(hNativeMemDCBmp);
            }

            Graphics gBackBuf = Graphics.FromImage(BackBuffer);
            int dcState = SaveDC(_CompatibleDC);

            SelectObject(_CompatibleDC, hNativeMemDCBmp);

            sleepMS = Viz.RenderVisualization();
            IntPtr hBackBufDC = gBackBuf.GetHdc();
            try
            {
              BitBlt(hBackBufDC, 0, 0, Width, Height, _CompatibleDC, 0, 0, SRCCOPY);
            }
            finally
            {
              gBackBuf.ReleaseHdc(hBackBufDC);
            }

            if (!FullScreen && CurrentThumbImage != null)
            {
              DoThumbnailOverlayFading(gBackBuf);
            }

            if (FullScreen && NewTrack && ShowTrackOverlay)
            {
              DoTrackInfoOverlayFading(gBackBuf);
            }

            if (ShowTrackOverlay)
            {
              DrawPlayStateIcon(gBackBuf);
            }

            try
            {
              gBackBuf.Dispose();

              RestoreDC(_CompatibleDC, dcState);

              // The BackBuffer image could be null if we're in the process of resizing
              if (BackBuffer != null)
              {
                g.DrawImageUnscaled(BackBuffer, 0, 0);
              }
            }

            catch (Exception)
            {
            }
          }

          return sleepMS;
        }

        else
        {
          sleepMS = Viz.RenderVisualization();
        }
      }

      catch (Exception)
      {
      }

      return sleepMS;
    }

    private string GetAlbumOrFolderThumb(string filename, string ArtistName, string AlbumName)
    {
      if (string.IsNullOrEmpty(ArtistName) || string.IsNullOrEmpty(AlbumName))
      {
        return string.Empty;
      }

      string thumbPath = Util.Utils.GetAlbumThumbName(ArtistName, AlbumName);

      if (File.Exists(thumbPath))
      {
        string largeThumb = Util.Utils.ConvertToLargeCoverArt(thumbPath);
        if (File.Exists(largeThumb))
        {
          return largeThumb;
        }
        else
        {
          return thumbPath;
        }
      }

      // Still no album art? Then look for the folder.jpg image
      string folderThumbPath = Util.Utils.GetLocalFolderThumb(filename);

      if (File.Exists(folderThumbPath))
      {
        string largeFolderThumb = Util.Utils.ConvertToLargeCoverArt(folderThumbPath);
        if (File.Exists(largeFolderThumb))
        {
          return largeFolderThumb;
        }
        else
        {
          return thumbPath;
        }
      }

      return string.Empty;
    }

    private void LoadThumbnail(string thumbPath)
    {
      try
      {
        UpdatingCoverArtImage = true;
        CoverArtNeedsRefresh = false;

        if (CurrentThumbImage != null)
        {
          CurrentThumbImage.Dispose();
          CurrentThumbImage = null;
        }

        if (thumbPath.Length > 0 && File.Exists(thumbPath))
        {
          CurrentThumbImage = Image.FromFile(thumbPath);

          if (CurrentThumbImage != null)
          {
            // Needs to be refreshed only if we weren't able to load an image
            CoverArtNeedsRefresh = CurrentThumbImage == null;
            CurrentThumbPath = thumbPath;
          }

          else
          {
            CurrentThumbPath = string.Empty;
          }
        }

        else
        {
          CurrentThumbPath = string.Empty;
          CoverArtNeedsRefresh = false;
        }
      }

      catch (Exception ex)
      {
        Console.WriteLine("LoadThumbnail caused an exception: {0}", ex.Message);
      }

      finally
      {
        UpdatingCoverArtImage = false;
      }
    }

    private bool CreateTrackInfoOverlayImage()
    {
      try
      {
        CurrentTrackInfoImage = new Bitmap(TrackInfoImageWidth, TrackInfoImageHeight);
        Graphics g = Graphics.FromImage(CurrentTrackInfoImage);
        g.Clear(Color.FromArgb(0, Color.Black));
        Rectangle imgRect = new Rectangle(0, 0, TrackInfoImageWidth, TrackInfoImageHeight);
        g.DrawImage(TrackInfoImage, imgRect, 0, 0, TrackInfoImage.Width, TrackInfoImage.Height, GraphicsUnit.Pixel);

        Image coverArtImage = CurrentThumbImage;

        if (coverArtImage == null)
        {
          coverArtImage = MissingCoverArtImage;
        }

        if (coverArtImage != null)
        {
          Rectangle coverArtRect = new Rectangle(CoverArtXOffset,
                                                 CoverArtYOffset,
                                                 TrackInfoImageHeight - (CoverArtYOffset * 2),
                                                 TrackInfoImageHeight - (CoverArtYOffset * 2));

          g.DrawImage(coverArtImage, coverArtRect, 0, 0, coverArtImage.Width, coverArtImage.Height, GraphicsUnit.Pixel);
        }

        SizeF stringSize = SizeF.Empty;
        int textTop = Label1PosY;
        int textLeft = Label1PosX;
        int textWidth = TrackInfoImageWidth - (textLeft + TrackInfoTextRightMargin);
        stringSize = g.MeasureString(Label1ValueString, Label1Font, textWidth, TextStringFormat);
        int textHeight = (int)(stringSize.Height + 1f);
        Rectangle textRect = new Rectangle(textLeft, textTop, textWidth, textHeight);
        DrawFadingText(g, stringSize, Label1ValueString, textRect, Label1Font, Label1Color);

        textTop = Label2PosY;
        textLeft = Label2PosX;
        textWidth = TrackInfoImageWidth - (textLeft + TrackInfoTextRightMargin);
        stringSize = g.MeasureString(Label2ValueString, Label2Font, textWidth, TextStringFormat);
        textHeight = (int)(stringSize.Height + 1f);
        textRect = new Rectangle(textLeft, textTop, textWidth, textHeight);
        DrawFadingText(g, stringSize, Label2ValueString, textRect, Label2Font, Label2Color);

        textTop = Label3PosY;
        textLeft = Label3PosX;
        textWidth = TrackInfoImageWidth - (textLeft + TrackInfoTextRightMargin);
        stringSize = g.MeasureString(Label3ValueString, Label3Font, textWidth, TextStringFormat);
        textHeight = (int)(stringSize.Height + 1f);
        textRect = new Rectangle(textLeft, textTop, textWidth, textHeight);
        DrawFadingText(g, stringSize, Label3ValueString, textRect, Label3Font, Label3Color);

        textTop = Label4PosY;
        textLeft = Label4PosX;
        textWidth = TrackInfoImageWidth - (textLeft + TrackInfoTextRightMargin);
        stringSize = g.MeasureString(Label4ValueString, Label4Font, textWidth, TextStringFormat);
        textHeight = (int)(stringSize.Height + 1f);
        textRect = new Rectangle(textLeft, textTop, textWidth, textHeight);
        DrawFadingText(g, stringSize, Label4ValueString, textRect, Label4Font, Label4Color);

        g.Dispose();

        return true;
      }

      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return false;
      }
    }

    private void DrawTrackInfoOverlay(Graphics g, float opacity)
    {
      if (TrackInfoImage == null)
      {
        return;
      }

      Rectangle rect = new Rectangle(TrackInfoImageXPos, TrackInfoImageYPos, TrackInfoImageWidth, TrackInfoImageHeight);

      if (CurrentTrackInfoImage == null)
      {
        if (!CreateTrackInfoOverlayImage())
        {
          return;
        }
      }

      float[][] matrixItems = {
                                new float[] {1, 0, 0, 0, 0},
                                new float[] {0, 1, 0, 0, 0},
                                new float[] {0, 0, 1, 0, 0},
                                new float[] {0, 0, 0, opacity, 0},
                                new float[] {0, 0, 0, 0, 1}
                              };

      ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
      ImageAttributes imgAttrib = new ImageAttributes();
      imgAttrib.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

      g.DrawImage(CurrentTrackInfoImage, rect, 0, 0,
                  CurrentTrackInfoImage.Width, CurrentTrackInfoImage.Height,
                  GraphicsUnit.Pixel, imgAttrib);
    }

    private void DoTrackInfoOverlayFading(Graphics g)
    {
      float maxOpacity = GetOpacity(TrackInfoOverlayAlpha);

      if (maxOpacity < (float)(FadeFrameCount - 1) / 100)
      {
        maxOpacity = (float)(FadeFrameCount - 1) / 100;
      }

      float fStep = maxOpacity / (float)(FadeFrameCount - 1);

      if (CurrentFrame < FadeFrameCount)
      {
        // Fade in the track info over the visualization
        float opacity = fStep * (float)CurrentFrame;
        DrawTrackInfoOverlay(g, opacity);
      }

      else if (CurrentFrame < FadeFrameCount + ShowTrackInfoFrameCount)
      {
        //show the track info for ShowCoverArtFrameCount frames and hide the visualisation
        DrawTrackInfoOverlay(g, maxOpacity);
      }

      else if (CurrentFrame < FadeFrameCount + ShowTrackInfoFrameCount + FadeFrameCount)
      {
        // Fade out the track info ;
        float opacity = maxOpacity - (fStep * (float)(CurrentFrame - (FadeFrameCount + ShowTrackInfoFrameCount)));
        DrawTrackInfoOverlay(g, opacity);
      }

      else
      {
        NewTrack = false;
        CurrentFrame = -1;
      }

      //CurrentFrame++;
    }

    private int CurrentCoverArtImageIndex = -1;

    private void DoThumbnailOverlayFading(Graphics g)
    {
      try
      {
        if (DoImageCleanup)
        {
          InternalClearImages();
          DoImageCleanup = false;
        }

        if (UpdatingCoverArtImage)
        {
          return;
        }

        if (GUIWindowManager.IsRouted)
        {
          return;
        }

        float fStep = 1.0f / (float)(FadeFrameCount - 1);
        bool paused = g_Player.Paused;
        // show a static thumb instead of the idle viz..
        if (paused)
        {
          DrawThumbnailOverlay(g, 1.0f);
        }
        else
        {
          if (CurrentFrame < FadeFrameCount)
          {
            // Fade in the album art over the visualization
            float opacity = fStep * (float)CurrentFrame;
            DrawThumbnailOverlay(g, opacity);
          }
          else if (CurrentFrame < FadeFrameCount + ShowCoverArtFrameCount)
          {
            //show the album art for ShowCoverArtFrameCount frames and hide the visualisation
            DrawThumbnailOverlay(g, 1.0f);
          }
          else if (CurrentFrame < FadeFrameCount + ShowCoverArtFrameCount + FadeFrameCount)
          {
            // Fade out the album art;
            float opacity = 1.0f - (fStep * (float)(CurrentFrame - (FadeFrameCount + ShowCoverArtFrameCount)));
            DrawThumbnailOverlay(g, opacity);
          }
          else if (CurrentFrame < FadeFrameCount + ShowCoverArtFrameCount + FadeFrameCount + ShowVisualizationFrameCount)
          {
            //show only the visualisation for ShowVisualizationFrameCount frames
            // so do nothing...
          }
          else
          {
            if (_ImagesPathsList.Count > 0)
            {
              if (CurrentCoverArtImageIndex + 1 >= _ImagesPathsList.Count)
              {
                if (CurrentThumbImage != null)
                {
                  CurrentCoverArtImageIndex = -1;
                }

                else
                {
                  CurrentCoverArtImageIndex = 0;
                }
              }
              else
              {
                CurrentCoverArtImageIndex++;
              }
            }
            else
            {
              CurrentCoverArtImageIndex = -1;
            }

            CurrentFrame = -1;
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("DoThumbnailOverlayFading caused an exception:{0}", ex);
      }
    }

    private void DrawThumbnailOverlay(Graphics g, float opacity)
    {
      try
      {
        if (UpdatingCoverArtImage || UpdatingCoverArtImageList)
        {
          return;
        }

        if (GUIWindowManager.IsRouted)
        {
          this.Hide();
          return;
        }
        else
        {
          this.Show();
        }

        if (CurrentThumbImage == null && _CoverArtImages.Count == 0)
        {
          return;
        }

        Image img = GetNextImage();

        if (img == null)
        {
          return;
        }

        Rectangle rect = new Rectangle(0, 0, Width, Height);
        SolidBrush fillBrush = new SolidBrush(Color.FromArgb((int)(255f * opacity), Color.Black));
        g.FillRectangle(fillBrush, rect);
        fillBrush.Dispose();

        int imgHeight = Height;
        int imgWidth = imgHeight;

        if (!_KeepCoverArtAspectRatio)
        {
          imgWidth = Width;
        }

        int left = (Width - imgHeight) / 2;

        float[][] matrixItems = {
                                  new float[] {1, 0, 0, 0, 0},
                                  new float[] {0, 1, 0, 0, 0},
                                  new float[] {0, 0, 1, 0, 0},
                                  new float[] {0, 0, 0, opacity, 0},
                                  new float[] {0, 0, 0, 0, 1}
                                };

        ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
        ImageAttributes imgAttrib = new ImageAttributes();
        imgAttrib.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

        //g.DrawImage(CurrentThumbImage, new Rectangle(left, 0, imgWidth, imgHeight), 0, 0,
        //    CurrentThumbImage.Width, CurrentThumbImage.Height, GraphicsUnit.Pixel, imgAttrib);

        g.DrawImage(img, new Rectangle(left, 0, imgWidth, imgHeight), 0, 0,
                    img.Width, img.Height, GraphicsUnit.Pixel, imgAttrib);
      }

      catch (Exception ex)
      {
        Console.WriteLine("DrawThumbnailOverlay caused an exception:{0}", ex);
      }
    }

    private Image GetNextImage()
    {
      if (CurrentCoverArtImageIndex == -1 || _CoverArtImages.Count == 0)
      {
        return CurrentThumbImage;
      }

      if (CurrentCoverArtImageIndex >= _CoverArtImages.Count)
      {
        if (CurrentThumbImage == null)
        {
          CurrentCoverArtImageIndex = 0;
          return _CoverArtImages[0];
        }

        else
        {
          CurrentCoverArtImageIndex = -1;
          return CurrentThumbImage;
        }
      }

      else
      {
        return _CoverArtImages[CurrentCoverArtImageIndex];
      }
    }

    private void DrawFadingText(Graphics g, SizeF stringSize, string text, Rectangle rect, Font font, Color color)
    {
      LinearGradientBrush fadingBrush = null;

      try
      {
        fadingBrush = new LinearGradientBrush(rect, Color.FromArgb(255, color), Color.FromArgb(128, color),
                                              LinearGradientMode.Horizontal);

        Blend fadeBlend = new Blend(4);
        fadeBlend.Factors = new float[] { 0, 0, .85f, 1 };
        float fadeStart = Math.Max(1, rect.Width - 50) / (float)rect.Width;
        float fadeMiddle = Math.Max(1, rect.Width - 15) / (float)rect.Width;
        float fadeEnd = 1;
        fadeBlend.Positions = new float[] { 0, fadeStart, fadeMiddle, fadeEnd };
        fadingBrush.Blend = fadeBlend;

        g.DrawString(text, font, fadingBrush, rect, TextStringFormat);
      }

      finally
      {
        if (fadingBrush != null)
        {
          fadingBrush.Dispose();
          fadingBrush = null;
        }
      }
    }

    private void DrawPlayStateIcon(Graphics g)
    {
      if (g_Player.Paused)
      {
        if (PauseImage == null)
        {
          return;
        }

        g.DrawImage(PauseImage, new Rectangle(PauseImageX, PauseImageY, PauseImageWidth, PauseImageHeight), 0, 0,
                    PauseImage.Width, PauseImage.Height, GraphicsUnit.Pixel);
      }

      else
      {
        if (!SeekingFF && !SeekingRew && !NewPlay && !PlayBackTypeChanged)
        {
          return;
        }

        ++CurrentPlayStateFrame;
        float fStep = 1.0f / (float)(FadeFrameCount - 1);
        float opacity = 1.0f;
        bool doFade = CurrentPlayStateFrame >= ShowPlayStateFrameCount;

        if (doFade)
        {
          int fadeFrame = (ShowPlayStateFrameCount + FadeFrameCount) - CurrentPlayStateFrame;
          opacity = (fadeFrame * fStep);

          if (opacity < 0)
          {
            opacity = 0;
          }

          else if (opacity > 1.0f)
          {
            opacity = 1.0f;
          }
        }

        if (CurrentPlayStateFrame >= ShowPlayStateFrameCount + FadeFrameCount)
        {
          CurrentPlayStateFrame = 0;
          SeekingFF = false;
          SeekingRew = false;
          NewPlay = false;
          PlayBackTypeChanged = false;
          return;
        }

        if (NewPlay && g_Player.Playing)
        {
          if (PlayImage == null)
          {
            return;
          }

          if (!doFade)
          {
            g.DrawImage(PlayImage, new Rectangle(PlayImageX, PlayImageY, PlayImageWidth, PlayImageHeight), 0, 0,
                        PlayImage.Width, PlayImage.Height, GraphicsUnit.Pixel);
          }

          else
          {
            DoPlayStateIconFading(g, opacity, PlayImage, PlayImageX, PlayImageY, PlayImageWidth, PlayImageHeight);
          }
        }

        else if (g_Player.Stopped)
        {
          if (StopImage == null)
          {
            return;
          }

          if (!doFade)
          {
            g.DrawImage(StopImage, new Rectangle(StopImageX, StopImageY, StopImageWidth, StopImageHeight), 0, 0,
                        StopImage.Width, StopImage.Height, GraphicsUnit.Pixel);
          }

          else
          {
            DoPlayStateIconFading(g, opacity, StopImage, StopImageX, StopImageY, StopImageWidth, StopImageHeight);
          }
        }

        else if (SeekingFF)
        {
          bool bStart;
          bool bEnd;
          int seekStep = g_Player.GetSeekStep(out bStart, out bEnd);

          if (seekStep == 0 || bStart || bEnd)
          {
            if (CurrentPlayStateFrame < ShowPlayStateFrameCount)
            {
              CurrentPlayStateFrame = ShowPlayStateFrameCount;
            }
          }

          if (FFImage == null)
          {
            return;
          }

          if (!doFade)
          {
            g.DrawImage(FFImage, new Rectangle(FFImageX, FFImageY, FFImageWidth, FFImageHeight), 0, 0,
                        FFImage.Width, FFImage.Height, GraphicsUnit.Pixel);
          }

          else
          {
            DoPlayStateIconFading(g, opacity, FFImage, FFImageX, FFImageY, FFImageWidth, FFImageHeight);
          }
        }


        else if (SeekingRew)
        {
          bool bStart;
          bool bEnd;
          int seekStep = g_Player.GetSeekStep(out bStart, out bEnd);

          if (seekStep == 0 || bStart || bEnd)
          {
            if (CurrentPlayStateFrame < ShowPlayStateFrameCount)
            {
              CurrentPlayStateFrame = ShowPlayStateFrameCount;
            }
          }

          if (RewImage == null)
          {
            return;
          }

          if (!doFade)
          {
            g.DrawImage(RewImage, new Rectangle(RewImageX, RewImageY, RewImageWidth, RewImageHeight), 0, 0,
                        RewImage.Width, RewImage.Height, GraphicsUnit.Pixel);
          }

          else
          {
            DoPlayStateIconFading(g, opacity, RewImage, RewImageX, RewImageY, RewImageWidth, RewImageHeight);
          }
        }


        else if (PlayBackTypeChanged)
        {
          Image img = null;
          int imgX = 0;
          int imgY = 0;
          int imgWidth = 0;
          int imgHeight = 0;

          switch (_playBackType)
          {
            case (int)PlayBackType.CROSSFADE:
              {
                img = CrossfadeImage;
                imgX = CrossfadeImageX;
                imgY = CrossfadeImageY;
                imgWidth = CrossfadeImageWidth;
                imgHeight = CrossfadeImageHeight;
                break;
              }
            case (int)PlayBackType.GAPLESS:
              {
                img = GaplessImage;
                imgX = GaplessImageX;
                imgY = GaplessImageY;
                imgWidth = GaplessImageWidth;
                imgHeight = GaplessImageHeight;
                break;
              }
            case (int)PlayBackType.NORMAL:
              {
                img = GapImage;
                imgX = GapImageX;
                imgY = GapImageY;
                imgWidth = GapImageWidth;
                imgHeight = GapImageHeight;
                break;
              }
          }

          if (img == null)
          {
            return;
          }

          if (!doFade)
          {
            g.DrawImage(img, new Rectangle(imgX, imgY, imgWidth, imgHeight), 0, 0,
                        img.Width, img.Height, GraphicsUnit.Pixel);
          }

          else
          {
            DoPlayStateIconFading(g, opacity, img, imgX, imgY, imgWidth, imgHeight);
          }
        }
      }
    }

    private void DoPlayStateIconFading(Graphics g, float opacity, Image img, int xPos, int yPos, int width, int height)
    {
      if (img == null)
      {
        return;
      }

      if (GUIWindowManager.IsRouted)
      {
        return;
      }

      Rectangle rect = new Rectangle(xPos, yPos, width, height);

      float[][] matrixItems = {
                                new float[] {1, 0, 0, 0, 0},
                                new float[] {0, 1, 0, 0, 0},
                                new float[] {0, 0, 1, 0, 0},
                                new float[] {0, 0, 0, opacity, 0},
                                new float[] {0, 0, 0, 0, 1}
                              };

      ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
      ImageAttributes imgAttrib = new ImageAttributes();
      imgAttrib.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

      g.DrawImage(img, rect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttrib);
    }


    protected override void WndProc(ref Message m)
    {
      switch (m.Msg)
      {
        case WM_MOVE:
        case WM_SIZE:
        case WM_WINDOWPOSCHANGED:
          {
            // Don't call DoResize unless the window size has really changed
            if (!NeedsResize())
            {
              break;
            }

            //                        OldSize = Size;
            DoResize();
            break;
          }
      }
      base.WndProc(ref m);
    }

    private void SetOutputContext()
    {
      if (Viz == null || !Viz.Initialized)
      {
        return;
      }

      if (GUIWindowManager.IsRouted)
      {
        return;
      }

      // If the status overlay option is disabled we render directly to the viz window
      // only when we're in fullscreen mode.  The music overlay is alway rendered to 
      // an off-screen context.  This allows us to do the album fade-in/out transition
      if (!_EnableStatusOverlays)
      {
        if (FullScreen)
        {
          Viz.SetOutputContext(VisualizationBase.OutputContextType.WindowHandle);
        }

        else
        {
          Viz.SetOutputContext(VisualizationBase.OutputContextType.DeviceContext);
        }
      }

      else
      {
        Viz.SetOutputContext(VisualizationBase.OutputContextType.DeviceContext);
      }
    }

    public bool AddImage(string imagePath)
    {
      bool result = false;
      imagePath = imagePath.ToLower();

      if (_ImagesPathsList == null)
      {
        return false;
      }

      if (_ImagesPathsList.Contains(imagePath))
      {
        return false;
      }

      if (CurrentThumbPath.IndexOf(Path.GetFileName(imagePath)) > 0)
      {
        return false;
      }

      if (imagePath.IndexOf("missing_coverart.png") > 0)
      {
        return false;
      }

      if (File.Exists(imagePath))
      {
        try
        {
          UpdatingCoverArtImageList = true;
          _ImagesPathsList.Add(imagePath);
          Image img = Image.FromFile(imagePath);

          if (img != null)
          {
            this._CoverArtImages.Add(img);
          }

          result = img != null;
        }

        catch
        {
          result = false;
        }

        finally
        {
          UpdatingCoverArtImageList = false;
        }
      }

      return result;
    }

    public void ClearImages()
    {
      DoImageCleanup = true;
    }

    // Make sure that this is called from the Rendering thread!
    private void InternalClearImages()
    {
      for (int i = 0; i < _CoverArtImages.Count; i++)
      {
        Image img = _CoverArtImages[i];

        if (img == null)
        {
          continue;
        }

        img.Dispose();
        img = null;
      }

      _CoverArtImages.Clear();
      _ImagesPathsList.Clear();
      CurrentCoverArtImageIndex = -1;
    }
  }
}