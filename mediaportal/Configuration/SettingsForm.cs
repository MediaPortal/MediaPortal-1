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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.Configuration.Sections;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;
using DVDPlayer = MediaPortal.Configuration.Sections.DVDPlayer;
using Keys = MediaPortal.Configuration.Sections.Keys;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for Settings.
  /// </summary>
  public partial class SettingsForm : MPConfigForm
  {
    #region ConfigPage struct

    public static bool audioplayer_mixing;
    public static bool debug_options;

    public struct ConfigPage
    {
      private string sectionName;
      private SectionSettings parentsection;
      private SectionSettings configSection;
      private bool isExpertSetting;

      public ConfigPage(SectionSettings aParentsection, SectionSettings aConfigSection, bool aIsExpertSetting)
      {
        sectionName = aConfigSection.Text;
        parentsection = aParentsection;
        configSection = aConfigSection;
        isExpertSetting = aIsExpertSetting;
      }

      public string SectionName
      {
        get { return sectionName; }
      }

      public SectionSettings Parentsection
      {
        get { return parentsection; }
      }

      public SectionSettings ConfigSection
      {
        get { return configSection; }
      }

      public bool IsVisible
      {
        // Show expert settings only in advanced mode.
        get { return AdvancedMode ? true : !isExpertSetting; }
      }
    }

    #endregion

    #region Variables

    private const string _windowName = "MediaPortal - Configuration";
    private int hintShowCount = 0;
    private SectionSettings _previousSection = null;
    // Disable DirectX Input (not compatible with NET4 and later)
    //private RemoteDirectInput dinputRemote;
    private RemoteSerialUIR serialuir;

    private static ConfigSplashScreen splashScreen = new ConfigSplashScreen();

    #region Properties

    // Hashtable where we store each added tree node/section for faster access
    private static Dictionary<string, ConfigPage> settingSections = new Dictionary<string, ConfigPage>();
    private ToolStripButton toolStripButtonSwitchAdvanced;

    public static Dictionary<string, ConfigPage> SettingSections
    {
      get { return settingSections; }
    }

    private static bool advancedMode = false;
    private bool _showDebugOptions;

    public static bool AdvancedMode
    {
      get { return advancedMode; }
      set
      {
        advancedMode = value;
        // Save the last state
        using (Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValueAsBool("general", "AdvancedConfigMode", advancedMode);
        }
      }
    }

    public static bool UseTvServer
    {
      get { return File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"); }
    }

    public bool ShowDebugOptions
    {
      get { return _showDebugOptions; }
    }

    #endregion

    #endregion

    public SettingsForm()
      : this(false) {}

    public SettingsForm(bool showDebugOptions)
    {
      _showDebugOptions = showDebugOptions;
      OnStartup();
    }

    private void OnStartup()
    {
      // start the splashscreen      
      string version = ConfigurationManager.AppSettings["version"];
      splashScreen.Version = version;
      splashScreen.Run();
      Log.Info("SettingsForm constructor");
      // Required for Windows Form Designer support
      InitializeComponent();
      this.linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://www.team-mediaportal.com/donate.html");
      // Build options tree
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Loading language...");
      }
      string strLanguage;
      using (Settings xmlreader = new MPSettings())
      {
        strLanguage = xmlreader.GetValueAsString("gui", "language", "English");
        hintShowCount = xmlreader.GetValueAsInt("general", "ConfigModeHintCount", 0);

        if (splashScreen != null)
        {
          splashScreen.SetInformation("Loading config options...");
        }
        CheckModeHintDisplay(hintShowCount);
        // The initial hint allows to choose a mode so we need to ask before loading that setting
        advancedMode = xmlreader.GetValueAsBool("general", "AdvancedConfigMode", false);
      }
      toolStripButtonSwitchAdvanced.Text = AdvancedMode ? "Switch to standard mode" : "Switch to expert mode";
      toolStripButtonSwitchAdvanced.Checked = AdvancedMode;
      GUILocalizeStrings.Load(strLanguage);
      // Register Bass.Net
      BassRegistration.BassRegistration.Register();
      Log.Info("add project section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding project section...");
      }
      Project project = new Project();
      AddSection(new ConfigPage(null, project, false));

      AddTabGeneral();
      AddTabGui();
      AddTabMovies();
      AddTabBD();
      AddTabDvd();
      AddTabTelevision();
      AddTabMusic();
      AddTabPictures();
      AddTabRemote();
      AddTabFilters();
      AddTabPlugins();
      AddTabThirdPartyChecks();

      // reset the last used state
      ToggleSectionVisibility(advancedMode);

      // Select first item in the section tree
      if (sectionTree.Nodes.Count > 0)
      {
        sectionTree.SelectedNode = sectionTree.Nodes[0];
      }

      if (splashScreen != null)
      {
        splashScreen.Stop(1000);
        splashScreen = null;
        BackgroundWorker FrontWorker = new BackgroundWorker();
        FrontWorker.DoWork += new DoWorkEventHandler(Worker_BringConfigToForeground);
        FrontWorker.RunWorkerAsync();
      }

      Log.Info("settingsform constructor done");
      GUIGraphicsContext.Skin = Config.GetFile(Config.Dir.Skin, "Default", string.Empty);
      Log.Info("SKIN : " + GUIGraphicsContext.Skin);
    }

    #region Section handling

    private void AddTabPlugins()
    {
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Loading plugins...");
      }

      Log.Info("add plugins section");
      PluginsNew pluginsNew = new PluginsNew();
      AddSection(new ConfigPage(null, pluginsNew, false));
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Finished plugin loading...");
      }
    }

    private void AddTabFilters()
    {
      //Look for Audio Decoders, if exist assume decoders are installed & present config option
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding filters section...");
      }

      FiltersSection filterSection = new FiltersSection();
      AddSection(new ConfigPage(null, filterSection, false));

      //Log.Info("  add Video codec section");
      //AddSection(new ConfigPage(filterSection, new MovieCodec(), false));
      //Log.Info("  add DVD codec section");
      //AddSection(new ConfigPage(filterSection, new DVDCodec(), false));
      //Log.Info("  add TV codec section");
      //AddSection(new ConfigPage(filterSection, new TVCodec(), false));

      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
      if (availableAudioFilters.Count > 0)
      {
        if (splashScreen != null)
        {
          splashScreen.SetInformation("Adding audio filters...");
        }

        ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.LATMAAC);
        if (availableAACAudioFilters.Count > 0)
        {
          foreach (string filter in availableAACAudioFilters)
          {
            if (filter.Equals("MONOGRAM AAC Decoder"))
            {
              FiltersMonogramAACDecoder monogramConfig = new FiltersMonogramAACDecoder();
              AddSection(new ConfigPage(filterSection, monogramConfig, true));
            }
          }
        }

        foreach (string filter in availableAudioFilters)
        {
          if (filter.Equals("NVIDIA Audio Decoder"))
          {
            FiltersPureVideoDecoder nvidiaConfig = new FiltersPureVideoDecoder();
            AddSection(new ConfigPage(filterSection, nvidiaConfig, true));
          }
          if (filter.Equals("InterVideo Audio Decoder"))
          {
            FiltersWinDVD7Decoder windvdConfig = new FiltersWinDVD7Decoder();
            AddSection(new ConfigPage(filterSection, windvdConfig, true));
          }          
          if (filter.Equals("DScaler Audio Decoder"))
          {
            FiltersDScalerAudio dscalerConfig = new FiltersDScalerAudio();
            AddSection(new ConfigPage(filterSection, dscalerConfig, true));
          }
          if (filter.Equals("MPC - MPA Decoder Filter"))
          {
            FiltersMPEG2DecAudio mpaConfig = new FiltersMPEG2DecAudio();
            AddSection(new ConfigPage(filterSection, mpaConfig, true));
          }
          if (filter.Contains("CyberLink Audio Decoder"))
          {
            FiltersPowerDVDAudioDecoder pdvdAudioConfig = new FiltersPowerDVDAudioDecoder();
            AddSection(new ConfigPage(filterSection, pdvdAudioConfig, true));
          }
        }
      }
      //Look for Video Decoders, if exist assume decoders are installed & present config option
      ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
      if (availableVideoFilters.Count > 0)
      {
        if (splashScreen != null)
        {
          splashScreen.SetInformation("Adding video filters...");
        }
        foreach (string filter in availableVideoFilters)
        {
          // if we do not have the audio codec installed we want to see the video config nevertheless
          if (filter.Contains("CyberLink Video/SP Decoder"))
          {
            FiltersPowerDVDDecoder pdvdConfig = new FiltersPowerDVDDecoder();
            AddSection(new ConfigPage(filterSection, pdvdConfig, true));
          }
          // if we do not have the audio codec installed we want to see the video config nevertheless
          if (filter.StartsWith("CyberLink Video Decoder (PDVD"))
          {
            FiltersPowerDVDDecoder10 pdvdConfig10 = new FiltersPowerDVDDecoder10();
            AddSection(new ConfigPage(filterSection, pdvdConfig10, true));
          }
          if (filter.Equals("MPC - MPEG-2 Video Decoder (Gabest)"))
          {
            FiltersMPEG2DecVideo mpvConfig = new FiltersMPEG2DecVideo();
            AddSection(new ConfigPage(filterSection, mpvConfig, true));
          }
          if (filter.Equals("DScaler Mpeg2 Video Decoder"))
          {
            FiltersDScalerVideo dscalervConfig = new FiltersDScalerVideo();
            AddSection(new ConfigPage(filterSection, dscalervConfig, true));
          }
        }
      }


      //Add section for video renderer configuration
      //FiltersVideoRenderer renderConfig = new FiltersVideoRenderer();
      //AddSection(new ConfigPage(filterSection, renderConfig, true));

      //Look for Audio Encoders, if exist assume encoders are installed & present config option
      string[] audioEncoders = new string[] {"InterVideo Audio Encoder"};
      FilterCollection legacyFilters = Filters.LegacyFilters;
      foreach (Filter audioCodec in legacyFilters)
      {
        for (int i = 0; i < audioEncoders.Length; ++i)
        {
          if (String.Compare(audioCodec.Name, audioEncoders[i], true) == 0)
          {
            EncoderFiltersSection EncoderfilterSection = new EncoderFiltersSection();
            AddSection(new ConfigPage(null, EncoderfilterSection, true));

            FiltersInterVideoEncoder windvdEncoderConfig = new FiltersInterVideoEncoder();
            AddSection(new ConfigPage(EncoderfilterSection, windvdEncoderConfig, true));
          }
        }
      }
    }

    private void AddTabRemote()
    {
      //add remotes section      
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding remote section...");
      }
      SectionSettings remote = new Remote();
      AddSection(new ConfigPage(null, remote, false));

      // Disable DirectX Input (not compatible with NET4 and later)
      //Log.Info("add DirectInput section");
      //RemoteDirectInput dinputConf = new RemoteDirectInput();
      //AddSection(new ConfigPage(remote, dinputConf, true));
      RemoteUSBUIRT usbuirtConf = new RemoteUSBUIRT();
      AddSection(new ConfigPage(remote, usbuirtConf, true));
      serialuir = new RemoteSerialUIR();
      AddSection(new ConfigPage(remote, serialuir, true));
      RemoteWinLirc winlircConf = new RemoteWinLirc();
      AddSection(new ConfigPage(remote, winlircConf, true));
      RemoteRedEye redeyeConf = new RemoteRedEye();
      AddSection(new ConfigPage(remote, redeyeConf, true));
    }

    private void AddTabTelevision()
    {
      if (UseTvServer)
      {
        //add television section
        Log.Info("add tv section");
        if (splashScreen != null)
        {
          splashScreen.SetInformation("Adding TV/Radio section...");
        }

        SectionSettings tvradio = new TVRadio();
        AddSection(new ConfigPage(null, tvradio, false));

        Log.Info("  add tv section");
        AddSection(new ConfigPage(tvradio, new TV(), false));
        Log.Info("  add radio section");
        AddSection(new ConfigPage(tvradio, new Radio(), false));
        Log.Info("  add tv zoom section");
        AddSection(new ConfigPage(tvradio, new TVZoom(), false));
        Log.Info("  add tv postprocessing section");
        AddSection(new ConfigPage(tvradio, new TVPostProcessing(), true));
        Log.Info("  add tv teletext section");
        AddSection(new ConfigPage(tvradio, new TVTeletext(), true));
        if (ShowDebugOptions)
        {
          Log.Info("  add tv debug options section");
          AddSection(new ConfigPage(tvradio, new TVDebugOptions(), true));
        }
      }
    }

    private void AddTabPictures()
    {
      //add pictures section
      Log.Info("add pictures section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding pictures section...");
      }

      SectionSettings picture = new Pictures();
      AddSection(new ConfigPage(null, picture, false));

      Log.Info("  add picture shares section");
      AddSection(new ConfigPage(picture, new PictureShares(), false));
      Log.Info("  add picture thumbs section");
      AddSection(new ConfigPage(picture, new PictureThumbs(), true));
      Log.Info("  add picture extensions section");
      AddSection(new ConfigPage(picture, new PictureExtensions(), true));
    }

    private void AddTabMusic()
    {
      //add music section
      Log.Info("add music section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding music section...");
      }

      SectionSettings music = new Sections.Music();
      AddSection(new ConfigPage(null, music, false));

      Log.Info("  add music shares section");
      AddSection(new ConfigPage(music, new MusicShares(), false));
      Log.Info("  add music database section");
      AddSection(new ConfigPage(music, new MusicDatabase(), false));
      Log.Info("  add music import section");
      AddSection(new ConfigPage(music, new MusicImport(), false));

      Log.Info("  add music extensions section");
      AddSection(new ConfigPage(music, new MusicExtensions(), true));
      Log.Info("  add music views section");
      AddSection(new ConfigPage(music, new MusicViews(), true));
      Log.Info("  add music sort section");
      AddSection(new ConfigPage(music, new MusicSort(), true));
      Log.Info("  add music dsp section");
      AddSection(new ConfigPage(music, new MusicDSP(), true));
    }


    private void AddTabMovies()
    {
      //add video section
      Log.Info("add video section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding video section...");
      }

      SectionSettings movie = new Movies();
      AddSection(new ConfigPage(null, movie, false));

      Log.Info("  add video shares section");
      AddSection(new ConfigPage(movie, new MovieShares(), false));
      Log.Info("  add video database section");
      MovieDatabase movieDbConfig = new MovieDatabase();
      AddSection(new ConfigPage(movie, movieDbConfig, false));
      Log.Info("  add video player section");
      AddSection(new ConfigPage(movie, new MoviePlayer(), false));
      Log.Info("  add video zoom section");
      AddSection(new ConfigPage(movie, new MovieZoom(), false));
      Log.Info("  add video extensions section");
      AddSection(new ConfigPage(movie, new MovieExtensions(), true));
      Log.Info("  add video views section");
      AddSection(new ConfigPage(movie, new MovieViews(), true));
      Log.Info("  add video postprocessing section");
      AddSection(new ConfigPage(movie, new MoviePostProcessing(), true));
    }

    private void AddTabBD()
    {
      //add BD video section
      Log.Info("add blu-ray section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding blu-ray section...");
      }

      SectionSettings bd = new BD();
      AddSection(new ConfigPage(null, bd, false));

      /*AddSection(new ConfigPage(null, movie, false));

      Log.Info("  add video shares section");
      AddSection(new ConfigPage(movie, new MovieShares(), false));
      Log.Info("  add video database section");
      MovieDatabase movieDbConfig = new MovieDatabase();
      AddSection(new ConfigPage(movie, movieDbConfig, false));
      Log.Info("  add video player section");
      AddSection(new ConfigPage(movie, new MoviePlayer(), false));*/
      Log.Info("  add blu-ray video zoom section");
      AddSection(new ConfigPage(bd, new BDZoom(), false));
      Log.Info("  add blu-ray postprocessing section");
      AddSection(new ConfigPage(bd, new BDPostProcessing(), true));
    }

    private void AddTabDvd()
    {
      //add DVD section
      Log.Info("add DVD section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding DVD section...");
      }

      SectionSettings dvd = new DVD();
      AddSection(new ConfigPage(null, dvd, false));

      Log.Info("  add DVD player section");
      AddSection(new ConfigPage(dvd, new DVDPlayer(), true));
      Log.Info("  add DVD zoom section");
      AddSection(new ConfigPage(dvd, new DVDZoom(), true));
      Log.Info("  add DVD postprocessing section");
      AddSection(new ConfigPage(dvd, new DVDPostProcessing(), true));
      Log.Info("  add DVD daemon tools section");
      AddSection(new ConfigPage(dvd, new GeneralDaemonTools(), true));
      Log.Info("  add DVD autoplay section");
      AddSection(new ConfigPage(dvd, new GeneralAutoplay(), true));
    }

    private void AddTabGeneral()
    {
      Log.Info("add general section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding general section...");
      }

      General general = new General();
      AddSection(new ConfigPage(null, general, false));

      Log.Info("  add general volume section");
      AddSection(new ConfigPage(general, new GeneralVolume(), false));
      Log.Info("  add general keyboard section");
      AddSection(new ConfigPage(general, new GeneralKeyboardControl(), true));
      Log.Info("  add general keys section");
      AddSection(new ConfigPage(general, new Keys(), true));
      Log.Info("  add general dynamic refresh section");
      AddSection(new ConfigPage(general, new GeneralDynamicRefreshRate(), true));
      Log.Info("  add general startup resume section");
      AddSection(new ConfigPage(general, new GeneralStartupResume(), false));

      // Removed because of various issues with DVD playback
      // AddSection(new ConfigPage(general, new GeneralCDSpeed(), true));
    }

    private void AddTabGui()
    {
      Log.Info("add gui section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding GUI section...");
      }

      Gui gui = new Gui();
      AddSection(new ConfigPage(null, gui, false));

      Log.Info("  add gui language section");
      AddSection(new ConfigPage(gui, new GuiLanguage(), false));
      Log.Info("  add gui controls section");
      AddSection(new ConfigPage(gui, new GuiControls(), false));
      Log.Info("  add gui thumbs section");
      AddSection(new ConfigPage(gui, new GuiThumbs(), false));
      Log.Info("  add gui screensaver section");
      AddSection(new ConfigPage(gui, new GuiScreensaver(), true));
      Log.Info("  add gui file menu section");
      AddSection(new ConfigPage(gui, new GuiFileMenu(), true));
      Log.Info("  add gui osd section");
      AddSection(new ConfigPage(gui, new GuiOSD(), true));
      Log.Info("  add gui skip steps section");
      AddSection(new ConfigPage(gui, new GuiSkipSteps(), true));
    }

    private void AddTabThirdPartyChecks()
    {
      Log.Info("add third party checks section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding third party checks section...");
      }
      SectionSettings thirdparty = new ThirdPartyChecks();
      AddSection(new ConfigPage(null, thirdparty, false));
    }

    private void ToggleSectionVisibility(bool aShowAdvancedOptions)
    {
      // using property so setter updates values..
      AdvancedMode = aShowAdvancedOptions;

      sectionTree.BeginUpdate();
      TreeNode currentSelected = (TreeNode)sectionTree.SelectedNode;
      sectionTree.Nodes.Clear();

      foreach (KeyValuePair<string, ConfigPage> singleConfig in settingSections)
      {
        ConfigPage currentSection = singleConfig.Value;

        // Add all loaded sections to the TreeView
        if (currentSection.IsVisible)
        {
          SectionTreeNode treeNode = new SectionTreeNode(currentSection.ConfigSection);
          // If not parent is specified we add the section as root node.
          if (currentSection.Parentsection == null)
          {
            // Add to the root
            sectionTree.Nodes.Add(treeNode);
          }
          else
          {
            // Find parent section (IndexOfKey is buggy)
            int parentPos = -1;
            // This limits usage to one level only - loop subitems if you want to build a tree
            for (int i = 0; i < sectionTree.Nodes.Count; i++)
            {
              if (sectionTree.Nodes[i].Text.CompareTo(currentSection.Parentsection.Text) == 0)
              {
                parentPos = i;
                break;
              }
            }

            if (parentPos > -1)
            {
              // Add to the parent node
              SectionTreeNode parentTreeNode = (SectionTreeNode)sectionTree.Nodes[parentPos];
              parentTreeNode.Nodes.Add(treeNode);
            }
          }
        }
      }
      if (currentSelected != null)
      {
        // Reselect the node we were editing before
        foreach (TreeNode parentNode in sectionTree.Nodes)
        {
          foreach (TreeNode node in parentNode.Nodes)
          {
            if (node.Text.CompareTo(currentSelected.Text) == 0)
            {
              sectionTree.SelectedNode = node;
              node.EnsureVisible();
              break;
            }
          }
        }
      }

      sectionTree.EndUpdate();
    }

    #endregion

    private void Worker_BringConfigToForeground(object sender, DoWorkEventArgs e)
    {
      Thread.CurrentThread.Name = "Config form waiter";
      IntPtr hwnd;
      // get the window handle of configuration.exe
      do
      {
        hwnd = Win32API.FindWindow(null, _windowName);
        Thread.Sleep(250);
      } while (hwnd == IntPtr.Zero);
      Thread.Sleep(100);
      Win32API.ShowWindow(hwnd, Win32API.ShowWindowFlags.Show);
      Win32API.SetForegroundWindow(hwnd);
    }

    public void AddSection(ConfigPage aSection)
    {
      if (settingSections.ContainsKey(aSection.SectionName))
      {
        return;
      }

      settingSections.Add(aSection.SectionName, aSection);
    }

    private void sectionTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
    {
      SectionTreeNode treeNode = e.Node as SectionTreeNode;

      if (treeNode != null)
      {
        e.Cancel = !treeNode.Section.CanActivate;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void sectionTree_AfterSelect(object sender, TreeViewEventArgs e)
    {
      SectionTreeNode treeNode = e.Node as SectionTreeNode;

      if (treeNode != null)
      {
        if (ActivateSection(treeNode.Section))
        {
          headerLabel.Caption = treeNode.Section.Text;
        }
      }
    }

    private bool ActivateSection(SectionSettings section)
    {
      try
      {
        if (section.CanActivate == false)
        {
          return false;
        }
        section.Dock = DockStyle.Fill;
        section.OnSectionActivated();
        if (section != _previousSection && _previousSection != null)
        {
          _previousSection.OnSectionDeActivated();
        }
        _previousSection = section;

        holderPanel.Controls.Clear();
        holderPanel.Controls.Add(section);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return true;
    }

    private void SettingsForm_Closed(object sender, EventArgs e)
    {
      try
      {
        // stop serial ir receiver thread
        serialuir.Close();
      }
      catch (Exception)
      {
        // Ignore
      }
      // Disable DirectX Input (not compatible with NET4 and later)
      //if (null != dinputRemote)
      //{
      //  // make sure the listener thread gets killed cleanly!
      //  dinputRemote.Dispose();
      //  dinputRemote = null;
      //}
    }

    private void SettingsForm_Load(object sender, EventArgs e)
    {
      GUIGraphicsContext.form = this;
      // Asynchronously pre-initialize the music engine if we're using the BassMusicPlayer
      if (BassMusicPlayer.IsDefaultMusicPlayer)
      {
        BassMusicPlayer.CreatePlayerAsync();
      }
      Log.Info("Load settings");

      // We load ALL sections - not just those which are visible currently
      foreach (KeyValuePair<string, ConfigPage> singleConfig in settingSections)
      {
        ConfigPage config = singleConfig.Value;
        TreeNode loadNode = new SectionTreeNode(config.ConfigSection) as TreeNode;
        if (loadNode != null)
        {
          // LoadSectionSettings will recursively load all settings
          if (loadNode.Parent == null)
          {
            LoadSectionSettings(loadNode);
          }
        }
      }

      Log.Info("Load settings done");
    }

    private void LoadSectionSettings(TreeNode currentNode)
    {
      if (currentNode != null)
      {
        // Load settings for current node
        SectionTreeNode treeNode = currentNode as SectionTreeNode;
        if (treeNode != null)
        {
          Log.Info("LoadSectionSettings() - {0}", treeNode.Text);
          treeNode.Section.LoadSettings();
        }
        // Load settings for all child nodes
        foreach (TreeNode childNode in treeNode.Nodes)
        {
          LoadSectionSettings(childNode);
        }
      }
      Log.Info("LoadSectionSettings() done");
    }

    private void SaveSectionSettings(TreeNode currentNode)
    {
      if (currentNode != null)
      {
        // Save settings for current node
        SectionTreeNode treeNode = currentNode as SectionTreeNode;
        if (treeNode != null)
        {
          Log.Info("SaveSectionSettings() - {0}", treeNode.Text);
          treeNode.Section.SaveSettings();
        }
        // Load settings for all child nodes
        foreach (TreeNode childNode in treeNode.Nodes)
        {
          SaveSectionSettings(childNode);
        }
      }
      Log.Info("SaveSectionSettings done()");
    }

    private void CheckModeHintDisplay(int aHintShowCount)
    {
      switch (aHintShowCount)
      {
        case 0:
          aHintShowCount = ShowConfigModeHint() ? 1 : 0;
          break;
        default:
          aHintShowCount++;
          break;
      }

      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("general", "ConfigModeHintCount", aHintShowCount);
      }
    }

    private bool ShowConfigModeHint()
    {
      DlgConfigModeHint hintForm = new DlgConfigModeHint();
      splashScreen.AllowWindowOverlay(hintForm);
      return (hintForm.ShowDialog(this) == DialogResult.OK);
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      if (MusicScanRunning())
      {
        return;
      }

      Close();
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      applyButton_Click(sender, e);
      if (!AllFilledIn() || MusicScanRunning())
      {
        return;
      }
      Close();
    }

    private bool MusicScanRunning()
    {
      SectionSettings section = SectionSettings.GetSection("Music Database");

      if (section != null)
      {
        bool scanRunning = (bool)section.GetSetting("folderscanning");
        if (scanRunning)
        {
          MessageBox.Show("Music Folderscan running in background.\r\nPlease wait for the scan to finish",
                          "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);

          if (ActivateSection(section))
          {
            headerLabel.Caption = section.Text;
          }
        }
        return scanRunning;
      }

      return false;
    }


    /// <summary>
    /// Do we have all required fields filled
    /// </summary>
    /// <returns></returns>
    private bool AllFilledIn()
    {
      int MaximumShares = 250;
      //Do we have 1 or more music,picture,video shares?
      using (Settings xmlreader = new MPSettings())
      {
        string playlistFolder = xmlreader.GetValueAsString("music", "playlists", "");
        if (playlistFolder == string.Empty)
        {
          MessageBox.Show("No music playlist folder specified", "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return false;
        }
        playlistFolder = xmlreader.GetValueAsString("movies", "playlists", "");
        if (playlistFolder == string.Empty)
        {
          MessageBox.Show("No video playlist folder specified", "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return false;
        }
        bool added = false;
        for (int index = 0; index < MaximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string sharePathData = xmlreader.GetValueAsString("music", sharePath, "");
          if (!Util.Utils.IsDVD(sharePathData) && sharePathData != string.Empty)
          {
            added = true;
            break;
          }
        }
        if (!added)
        {
          MessageBox.Show("No music folders specified", "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return false;
        }
        added = false;
        for (int index = 0; index < MaximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string shareNameData = xmlreader.GetValueAsString("movies", sharePath, "");
          if (!Util.Utils.IsDVD(shareNameData) && shareNameData != string.Empty)
          {
            added = true;
            break;
          }
        }
        if (!added)
        {
          MessageBox.Show("No video folders specified", "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return false;
        }
        added = false;
        for (int index = 0; index < MaximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string shareNameData = xmlreader.GetValueAsString("pictures", sharePath, "");
          if (!Util.Utils.IsDVD(shareNameData) && shareNameData != string.Empty)
          {
            added = true;
            break;
          }
        }
        if (!added)
        {
          MessageBox.Show("No pictures folders specified", "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return false;
        }

        // Check hostname for tv server (empty hostname is invalid)
        if (UseTvServer)
        {
          string hostName = xmlreader.GetValueAsString("tvservice", "hostname", "");
          if (string.IsNullOrEmpty(hostName))
          {
            // Show message box
            DialogResult result = MessageBox.Show("There is a problem with the hostname specified in the \"TV/Radio\" section. " +
              "It will not be saved." + Environment.NewLine + Environment.NewLine + "Do you want to review it before exiting?",
              "MediaPortal Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // If user wants to review hostname select "TV/Radio" section and return false
            if (result == DialogResult.Yes)
            {
              // Loop through the tree to find the "TV/Radio" node and select it
              foreach (TreeNode parentNode in sectionTree.Nodes)
              {
                if (parentNode.Text == "TV/Radio")
                {
                      sectionTree.SelectedNode = parentNode;
                      parentNode.EnsureVisible();
                      return false;
                }
              }
              return false;
            }

          }
        }
      }
      return true;
    }

    private void SaveAllSettings()
    {
      // We save ALL sections - not just those which are visible currently
      foreach (KeyValuePair<string, ConfigPage> singleConfig in settingSections)
      {
        ConfigPage config = singleConfig.Value;
        TreeNode saveNode = new SectionTreeNode(config.ConfigSection) as TreeNode;
        if (saveNode != null)
        {
          // SaveSectionSettings recursively saves all subnodes as well
          if (saveNode.Parent == null)
          {
            SaveSectionSettings(saveNode);
          }
        }
      }

      Settings.SaveCache();
    }

    private void applyButton_Click(object sender, EventArgs e)
    {
      try
      {
        // Check if MediaPortal is running, if so inform user that it needs to be restarted for the changes to take effect.
        if (Util.Utils.CheckForRunningProcess("MediaPortal", false))
        {
          DialogResult dialogResult =
            MessageBox.Show("For the changes to take effect you need to restart MediaPortal, restart now?",
                            "MediaPortal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
          if (dialogResult == DialogResult.Yes)
          {
            if (!Util.Utils.CheckForRunningProcess("MediaPortal", true))
            {
              SaveAllSettings();
              // Start the MediaPortal process
              Process.Start(Config.GetFile(Config.Dir.Base, "MediaPortal.exe"));
              return;
            }
          }
        }
      }
      catch (Exception) {}

      SaveAllSettings();
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start((string)e.Link.LinkData);
    }

    private void helpToolStripSplitButton_Click(object sender, EventArgs e)
    {
      HelpSystem.ShowHelp(_previousSection.ToString());
    }

    private void OpenMpDirectory(Config.Dir dir)
    {
      Process process = new Process();
      process.StartInfo.FileName = "explorer.exe";
      process.StartInfo.Arguments = Config.GetFolder(dir);
      process.StartInfo.UseShellExecute = true;
      process.Start();
    }

    private void configToolStripSplitButton_ButtonClick(object sender, EventArgs e)
    {
      OpenMpDirectory(Config.Dir.Config);
    }

    private void thumbsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenMpDirectory(Config.Dir.Thumbs);
    }

    private void logsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenMpDirectory(Config.Dir.Log);
    }

    private void databaseToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenMpDirectory(Config.Dir.Database);
    }

    private void skinsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenMpDirectory(Config.Dir.Skin);
    }

    private void toolStripButtonSwitchAdvanced_Click(object sender, EventArgs e)
    {
      ToggleSectionVisibility(toolStripButtonSwitchAdvanced.Checked);
      toolStripButtonSwitchAdvanced.Text = AdvancedMode ? "Switch to standard mode" : "Switch to expert mode";
    }
  }
}