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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
using DVDPlayer = MediaPortal.Configuration.Sections.DVDPlayer;
using Keys = MediaPortal.Configuration.Sections.Keys;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for Settings.
  /// </summary>
  public class SettingsForm : MPConfigForm
  {
    #region DLL imports

    [DllImport("User32.")]
    public static extern int SendMessage(IntPtr window, int message, int wparam, int lparam);

    [DllImport("User32")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("User32")]
    public static extern int EnumWindows(IECallBack x, int y);

    [DllImport("User32")]
    public static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);

    [DllImport("User32")]
    public static extern void GetClassName(int h, StringBuilder s, int nMaxCount);

    [DllImport("User32", CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow(
      [MarshalAs(UnmanagedType.LPTStr)] string lpClassName,
      [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);

    [DllImport("User32")]
    private static extern int SetForegroundWindow(IntPtr hwnd);

    #endregion

    #region ConfigPage struct

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

    public delegate bool IECallBack(int hwnd, int lParam);

    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;
    private const string _windowName = "MediaPortal - Configuration";
    private int hintShowCount = 0;
    private LinkLabel linkLabel1;
    private ToolStrip toolStrip1;
    private ToolStripSplitButton helpToolStripSplitButton;
    private ToolStripSplitButton configToolStripSplitButton;
    private ToolStripMenuItem thumbsToolStripMenuItem;
    private ToolStripMenuItem logsToolStripMenuItem;
    private ToolStripMenuItem databaseToolStripMenuItem;
    private ToolStripMenuItem updateHelpToolStripMenuItem;
    private ToolStripMenuItem skinsToolStripMenuItem;
    private SectionSettings _previousSection = null;
    private MPButton cancelButton;
    private MPButton okButton;
    private MPBeveledLine beveledLine1;
    private TreeView sectionTree;
    private Panel holderPanel;
    private MPGradientLabel headerLabel;
    private RemoteDirectInput dinputRemote;
    private RemoteSerialUIR serialuir;
    private MPButton applyButton;

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

    #endregion

    #endregion

    public SettingsForm()
    {
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
        strLanguage = xmlreader.GetValueAsString("skin", "language", "English");
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
      AddTabMovies();
      AddTabDvd();
      AddTabTelevision();
      AddTabMusic();
      AddTabPictures();
      AddTabRemote();
      AddTabFilters();
      AddTabWeather();
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
      GUIGraphicsContext.Skin = Config.GetFile(Config.Dir.Skin, "Blue3", string.Empty);
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

    private void AddTabWeather()
    {
      //add weather section
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding weather section...");
      }

      Log.Info("add weather section");
      Weather weather = new Weather();
      AddSection(new ConfigPage(null, weather, false));
    }

    private void AddTabFilters()
    {
      //Look for Audio Decoders, if exist assume decoders are installed & present config option
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding filters section...");
      }

      FiltersSection filterSection = new FiltersSection();
      AddSection(new ConfigPage(null, filterSection, true));

      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
      if (availableAudioFilters.Count > 0)
      {
        if (splashScreen != null)
        {
          splashScreen.SetInformation("Adding audio filters...");
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
          if (filter.Contains("CyberLink Audio Decoder"))
          {
            FiltersPowerDVDDecoder pdvdConfig = new FiltersPowerDVDDecoder();
            AddSection(new ConfigPage(filterSection, pdvdConfig, true));
          }
          if (filter.Equals("MPC - MPA Decoder Filter"))
          {
            FiltersMPEG2DecAudio mpaConfig = new FiltersMPEG2DecAudio();
            AddSection(new ConfigPage(filterSection, mpaConfig, false));
          }
          if (filter.Equals("DScaler Audio Decoder"))
          {
            FiltersDScalerAudio dscalerConfig = new FiltersDScalerAudio();
            AddSection(new ConfigPage(filterSection, dscalerConfig, true));
          }
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
          // if we do not have the audio codec installed we want to see the video config nevertheless
          if (filter.Contains("CyberLink Video/SP Decoder"))
          {
            FiltersPowerDVDDecoder pdvdConfig = new FiltersPowerDVDDecoder();
            AddSection(new ConfigPage(filterSection, pdvdConfig, true));
          }
        }
      }


      //Add section for video renderer configuration
      FiltersVideoRenderer renderConfig = new FiltersVideoRenderer();
      AddSection(new ConfigPage(filterSection, renderConfig, true));

      //Look for Audio Encoders, if exist assume encoders are installed & present config option
      string[] audioEncoders = new string[] { "InterVideo Audio Encoder" };
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

      Log.Info("add DirectInput section");
      RemoteDirectInput dinputConf = new RemoteDirectInput();
      AddSection(new ConfigPage(remote, dinputConf, true));
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
        Log.Info("add television section");
        if (splashScreen != null)
        {
          splashScreen.SetInformation("Adding television section...");
        }

        SectionSettings television = new Television();
        AddSection(new ConfigPage(null, television, false));

        Log.Info("  add tv client section");
        AddSection(new ConfigPage(television, new TVClient(), false));
        Log.Info("  add tv postprocessing section");
        AddSection(new ConfigPage(television, new TVPostProcessing(), true));
        Log.Info("  add tv teletext section");
        AddSection(new ConfigPage(television, new TVTeletext(), true));
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

      AddSection(new ConfigPage(picture, new PictureShares(), false));
      AddSection(new ConfigPage(picture, new PictureThumbs(), true));
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
      Log.Info("  add music asio section");
      AddSection(new ConfigPage(music, new MusicASIO(), true));
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

      Log.Info("  add video extensions section");
      AddSection(new ConfigPage(movie, new MovieExtensions(), true));
      Log.Info("  add video views section");
      AddSection(new ConfigPage(movie, new MovieViews(), true));
      Log.Info("  add video postprocessing section");
      AddSection(new ConfigPage(movie, new MoviePostProcessing(), true));
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

      Log.Info("  add DVD codec section");
      AddSection(new ConfigPage(dvd, new DVDCodec(), false));

      Log.Info("  add DVD player section");
      AddSection(new ConfigPage(dvd, new DVDPlayer(), true));
      Log.Info("  add DVD postprocessing section");
      AddSection(new ConfigPage(dvd, new DVDPostProcessing(), true));
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

      //add skins section
      Log.Info("add skins section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding skins section...");
      }

      GeneralSkin skinConfig = new GeneralSkin();
      AddSection(new ConfigPage(general, skinConfig, false));

      AddSection(new ConfigPage(general, new GeneralThumbs(), false));
      AddSection(new ConfigPage(general, new GeneralVolume(), false));

      AddSection(new ConfigPage(general, new GeneralKeyboardControl(), true));
      AddSection(new ConfigPage(general, new Keys(), true));
      AddSection(new ConfigPage(general, new GeneralScreensaver(), true));
      AddSection(new ConfigPage(general, new GeneralOSD(), true));
      AddSection(new ConfigPage(general, new GeneralSkipSteps(), true));
      AddSection(new ConfigPage(general, new GeneralStartupDelay(), true));
      AddSection(new ConfigPage(general, new GeneralWatchdog(), true));
      AddSection(new ConfigPage(general, new GeneralDaemonTools(), true));
      AddSection(new ConfigPage(general, new GeneralFileMenu(), true));

      GeneralDynamicRefreshRate dynRRConfig = new GeneralDynamicRefreshRate();
      AddSection(new ConfigPage(general, dynRRConfig, true));

      // Removed because of various issues with DVD playback
      // AddSection(new ConfigPage(general, new GeneralCDSpeed(), true));
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
        hwnd = FindWindow(null, _windowName);
        Thread.Sleep(250);
      } while (hwnd == IntPtr.Zero);
      Thread.Sleep(100);
      ShowWindow(hwnd, SW_SHOW);
      SetForegroundWindow(hwnd);
    }

    public void AddSection(ConfigPage aSection)
    {
      if (settingSections.ContainsKey(aSection.SectionName))
      {
        return;
      }

      settingSections.Add(aSection.SectionName, aSection);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        /*if(components != null)
				{
					components.Dispose();
				}*/
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources =
        new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
      this.sectionTree = new System.Windows.Forms.TreeView();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.applyButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.helpToolStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
      this.updateHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.configToolStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
      this.thumbsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.logsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.skinsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripButtonSwitchAdvanced = new System.Windows.Forms.ToolStripButton();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // sectionTree
      // 
      this.sectionTree.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
           | System.Windows.Forms.AnchorStyles.Left)));
      this.sectionTree.FullRowSelect = true;
      this.sectionTree.HideSelection = false;
      this.sectionTree.HotTracking = true;
      this.sectionTree.Indent = 19;
      this.sectionTree.ItemHeight = 16;
      this.sectionTree.Location = new System.Drawing.Point(16, 28);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.Size = new System.Drawing.Size(184, 462);
      this.sectionTree.TabIndex = 2;
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      this.sectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.sectionTree_BeforeSelect);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(621, 513);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(540, 513);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular,
                                                      System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(216, 28);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(480, 24);
      this.headerLabel.TabIndex = 3;
      this.headerLabel.TabStop = false;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular,
                                                          System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // holderPanel
      // 
      this.holderPanel.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanel.AutoScroll = true;
      this.holderPanel.BackColor = System.Drawing.SystemColors.Control;
      this.holderPanel.Location = new System.Drawing.Point(216, 58);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(480, 432);
      this.holderPanel.TabIndex = 4;
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 503);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(696, 2);
      this.beveledLine1.TabIndex = 5;
      this.beveledLine1.TabStop = false;
      // 
      // applyButton
      // 
      this.applyButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.Location = new System.Drawing.Point(459, 513);
      this.applyButton.Name = "applyButton";
      this.applyButton.Size = new System.Drawing.Size(75, 23);
      this.applyButton.TabIndex = 6;
      this.applyButton.TabStop = false;
      this.applyButton.Text = "&Apply";
      this.applyButton.UseVisualStyleBackColor = true;
      this.applyButton.Visible = false;
      this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // linkLabel1
      // 
      this.linkLabel1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(12, 518);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(113, 13);
      this.linkLabel1.TabIndex = 9;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Donate to MediaPortal";
      this.linkLabel1.LinkClicked +=
        new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // toolStrip1
      // 
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
                                       {
                                         this.helpToolStripSplitButton,
                                         this.configToolStripSplitButton,
                                         this.toolStripButtonSwitchAdvanced
                                       });
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(712, 25);
      this.toolStrip1.TabIndex = 10;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // helpToolStripSplitButton
      // 
      this.helpToolStripSplitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.helpToolStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
                                                             {
                                                               this.updateHelpToolStripMenuItem
                                                             });
      this.helpToolStripSplitButton.Image = global::MediaPortal.Configuration.Properties.Resources.icon_help;
      this.helpToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.helpToolStripSplitButton.Name = "helpToolStripSplitButton";
      this.helpToolStripSplitButton.Size = new System.Drawing.Size(60, 22);
      this.helpToolStripSplitButton.Text = "Help";
      this.helpToolStripSplitButton.ToolTipText = "Opens the online wiki page for the active configuration section.";
      this.helpToolStripSplitButton.ButtonClick += new System.EventHandler(this.helpToolStripSplitButton_ButtonClick);
      // 
      // updateHelpToolStripMenuItem
      // 
      this.updateHelpToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_refresh;
      this.updateHelpToolStripMenuItem.Name = "updateHelpToolStripMenuItem";
      this.updateHelpToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
      this.updateHelpToolStripMenuItem.Text = "Update Help";
      this.updateHelpToolStripMenuItem.ToolTipText =
        "Online update for the help references file. Use it if an incorrect wiki page was " +
        "opened.";
      this.updateHelpToolStripMenuItem.Click += new System.EventHandler(this.updateHelpToolStripMenuItem_Click);
      // 
      // configToolStripSplitButton
      // 
      this.configToolStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
                                                               {
                                                                 this.thumbsToolStripMenuItem,
                                                                 this.logsToolStripMenuItem,
                                                                 this.databaseToolStripMenuItem,
                                                                 this.skinsToolStripMenuItem
                                                               });
      this.configToolStripSplitButton.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.configToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.configToolStripSplitButton.Name = "configToolStripSplitButton";
      this.configToolStripSplitButton.Size = new System.Drawing.Size(117, 22);
      this.configToolStripSplitButton.Text = "User Config files";
      this.configToolStripSplitButton.ButtonClick += new System.EventHandler(this.configToolStripSplitButton_ButtonClick);
      // 
      // thumbsToolStripMenuItem
      // 
      this.thumbsToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.thumbsToolStripMenuItem.Name = "thumbsToolStripMenuItem";
      this.thumbsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
      this.thumbsToolStripMenuItem.Text = "Open Thumbs directory";
      this.thumbsToolStripMenuItem.Click += new System.EventHandler(this.thumbsToolStripMenuItem_Click);
      // 
      // logsToolStripMenuItem
      // 
      this.logsToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.logsToolStripMenuItem.Name = "logsToolStripMenuItem";
      this.logsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
      this.logsToolStripMenuItem.Text = "Open Log directory";
      this.logsToolStripMenuItem.Click += new System.EventHandler(this.logsToolStripMenuItem_Click);
      // 
      // databaseToolStripMenuItem
      // 
      this.databaseToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
      this.databaseToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
      this.databaseToolStripMenuItem.Text = "Open Database directory";
      this.databaseToolStripMenuItem.Click += new System.EventHandler(this.databaseToolStripMenuItem_Click);
      // 
      // skinsToolStripMenuItem
      // 
      this.skinsToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_folder;
      this.skinsToolStripMenuItem.Name = "skinsToolStripMenuItem";
      this.skinsToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
      this.skinsToolStripMenuItem.Text = "Open Skins directory";
      this.skinsToolStripMenuItem.Click += new System.EventHandler(this.skinsToolStripMenuItem_Click);
      // 
      // toolStripButtonSwitchAdvanced
      // 
      this.toolStripButtonSwitchAdvanced.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.toolStripButtonSwitchAdvanced.AutoSize = false;
      this.toolStripButtonSwitchAdvanced.CheckOnClick = true;
      this.toolStripButtonSwitchAdvanced.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripButtonSwitchAdvanced.Image =
        ((System.Drawing.Image)(resources.GetObject("toolStripButtonSwitchAdvanced.Image")));
      this.toolStripButtonSwitchAdvanced.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButtonSwitchAdvanced.Name = "toolStripButtonSwitchAdvanced";
      this.toolStripButtonSwitchAdvanced.Size = new System.Drawing.Size(135, 22);
      this.toolStripButtonSwitchAdvanced.Text = "Switch to expert mode";
      this.toolStripButtonSwitchAdvanced.Click += new System.EventHandler(this.toolStripButtonSwitchAdvanced_Click);
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScroll = true;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(712, 544);
      this.Controls.Add(this.toolStrip1);
      this.Controls.Add(this.linkLabel1);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.sectionTree);
      this.Name = "SettingsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MediaPortal - Configuration";
      this.Load += new System.EventHandler(this.SettingsForm_Load);
      this.Closed += new System.EventHandler(this.SettingsForm_Closed);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

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
      if (null != dinputRemote)
      {
        // make sure the listener thread gets killed cleanly!
        dinputRemote.Dispose();
        dinputRemote = null;
      }
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
            Log.Info("  Load settings:{0}", loadNode.Text);
            LoadSectionSettings(loadNode);
          }
        }
      }

      Log.Info("Load settings done");
    }

    private void LoadSectionSettings(TreeNode currentNode)
    {
      Log.Info("LoadSectionSettings()");
      if (currentNode != null)
      {
        // Load settings for current node
        SectionTreeNode treeNode = currentNode as SectionTreeNode;
        if (treeNode != null)
        {
          treeNode.Section.LoadSettings();
        }
        // Load settings for all child nodes
        foreach (TreeNode childNode in treeNode.Nodes)
        {
          Log.Info("  Load settings:{0}", childNode.Text);
          LoadSectionSettings(childNode);
        }
      }
      Log.Info("LoadSectionSettings() done");
    }

    private void SaveSectionSettings(TreeNode currentNode)
    {
      Log.Info("SaveSectionSettings()");
      if (currentNode != null)
      {
        // Save settings for current node
        SectionTreeNode treeNode = currentNode as SectionTreeNode;
        if (treeNode != null)
        {
          treeNode.Section.SaveSettings();
        }
        // Load settings for all child nodes
        foreach (TreeNode childNode in treeNode.Nodes)
        {
          Log.Info("SaveSectionSettings:{0}", childNode.Text);
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
      Close();
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      applyButton_Click(sender, e);
      if (!AllFilledIn())
      {
        return;
      }
      Close();
    }

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
        // is last.fm enabled but audioscrobbler is not? 
        bool audioScrobblerOn = xmlreader.GetValueAsBool("plugins", "Audioscrobbler", false);
        bool lastFmOn = xmlreader.GetValueAsBool("plugins", "Last.fm Radio", false);
        if (lastFmOn && !audioScrobblerOn)
        {
          MessageBox.Show("Please configure the Audioscrobbler plugin to use Last.fm radio", "MediaPortal Settings",
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return false;
        }

        if (audioScrobblerOn)
        {
          // Does Audioscrobbler have a user but no password (due to DB upgrades, restores, etc)
          string asuser = xmlreader.GetValueAsString("audioscrobbler", "user", "");
          if (!string.IsNullOrEmpty(asuser))
          {
            Music.Database.MusicDatabase mdb = Music.Database.MusicDatabase.Instance;
            string AsPass = mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(asuser)), "");
            if (string.IsNullOrEmpty(AsPass))
            {
              MessageBox.Show("No password specified for current Audioscrobbler user", "MediaPortal Settings",
                              MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

    /// <summary>
    /// Checks whether a process is currently running
    /// </summary>
    /// <param name="aShouldExit">Indicate that a windows application should be closed gracefully. If it does not respond in 10 seconds a kill is performed</param>
    /// <returns>If the given process is still present.</returns>
    private bool CheckForRunningProcess(string aProcessName, bool aShouldExit)
    {
      bool mpRunning = false;
      string processName = aProcessName;
      foreach (Process process in Process.GetProcesses())
      {
        if (process.ProcessName.Equals(processName))
        {
          if (!aShouldExit)
          {
            mpRunning = true;
            break;
          }
          else
          {
            try
            {
              // Kill the MediaPortal process by finding window and sending ALT+F4 to it.
              IECallBack ewp = new IECallBack(EnumWindowCallBack);
              EnumWindows(ewp, 0);
              process.CloseMainWindow();
              // Wait for the process to die, we wait for a maximum of 10 seconds
              if (!process.WaitForExit(10000))
              {
                process.Kill();
              }
            }
            catch (Exception)
            {
              try
              {
                process.Kill();
              }
              catch (Exception)
              {
              }
            }

            mpRunning = CheckForRunningProcess(aProcessName, false);
            break;
          }
        }
      }
      return mpRunning;
    }

    private void applyButton_Click(object sender, EventArgs e)
    {
      try
      {
        // Check if MediaPortal is running, if so inform user that it needs to be restarted for the changes to take effect.
        if (CheckForRunningProcess("MediaPortal", false))
        {
          DialogResult dialogResult =
            MessageBox.Show("For the changes to take effect you need to restart MediaPortal, restart now?",
                            "MediaPortal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
          if (dialogResult == DialogResult.Yes)
          {
            if (!CheckForRunningProcess("MediaPortal", true))
            {
              SaveAllSettings();
              // Start the MediaPortal process
              Process.Start(Config.GetFile(Config.Dir.Base, "MediaPortal.exe"));
              return;
            }
          }
        }
      }
      catch (Exception)
      {
      }

      SaveAllSettings();
    }

    private bool EnumWindowCallBack(int hwnd, int lParam)
    {
      IntPtr windowHandle = (IntPtr)hwnd;
      StringBuilder sb = new StringBuilder(1024);
      GetWindowText((int)windowHandle, sb, sb.Capacity);
      string window = sb.ToString().ToLower();
      if (window.IndexOf("mediaportal") >= 0 || window.IndexOf("media portal") >= 0)
      {
        ShowWindow(windowHandle, SW_SHOWNORMAL);
      }
      return true;
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start((string)e.Link.LinkData);
    }

    private void helpToolStripSplitButton_ButtonClick(object sender, EventArgs e)
    {
      HelpSystem.ShowHelp(_previousSection.ToString());
    }

    private void updateHelpToolStripMenuItem_Click(object sender, EventArgs e)
    {
      HelpSystem.UpdateHelpReferences();
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