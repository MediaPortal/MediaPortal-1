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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.Configuration.Sections;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using Keys = MediaPortal.Configuration.Sections.Keys;
using System.Xml;
using System.Threading;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for Settings.
  /// </summary>
  public class SettingsForm : MediaPortal.UserInterface.Controls.MPConfigForm
  {
    public delegate bool IECallBack(int hwnd, int lParam);
    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;
    private string _windowName = "MediaPortal - Setup";
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

    private MPButton cancelButton;
    private MPButton okButton;
    private MPBeveledLine beveledLine1;
    private TreeView sectionTree;
    private Panel holderPanel;
    private MPGradientLabel headerLabel;
    private RemoteSerialUIR serialuir;
    private RemoteRedEye redeye; //PB00//
    private RemoteDirectInput dinputRemote;
    private static ConfigSplashScreen splashScreen = new ConfigSplashScreen();
    // Hashtable where we store each added tree node/section for faster access
    public static Hashtable SettingSections
    {
      get { return settingSections; }
    }

    private static Hashtable settingSections = new Hashtable();
    private MPButton applyButton;

    public SettingsForm()
    {
      // start the splashscreen      
      string version = System.Configuration.ConfigurationManager.AppSettings["version"];
      splashScreen.Version = version;
      splashScreen.Run();
      Log.Info("SettingsForm constructor");
      // Required for Windows Form Designer support
      InitializeComponent();
      this.linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://www.team-mediaportal.com/donate.html");
      // Stop MCE services
      MediaPortal.Util.Utils.StopMCEServices();
      // Build options tree
      string strLanguage;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strLanguage = xmlreader.GetValueAsString("skin", "language", "English");
      }
      GUILocalizeStrings.Load(strLanguage);
      // Register Bass.Net
      BassRegistration.BassRegistration.Register();
      Log.Info("add project section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding project section...");
      }
      Project project = new Project();
      AddSection(project);
      Log.Info("add general section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding general section...");
      }
      General general = new General();
      AddSection(general);
      //add skins section
      Log.Info("add skins section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding skins section...");
      }
      AddChildSection(general, new GeneralStartupDelay());
      AddChildSection(general, new GeneralWatchdog());
      AddChildSection(general, new GeneralSkin());
      AddChildSection(general, new GeneralKeyboardControl());
      AddChildSection(general, new Keys());
      AddChildSection(general, new GeneralOSD());
      AddChildSection(general, new GeneralSkipSteps());
      AddChildSection(general, new GeneralThumbs());
      AddChildSection(general, new Sections.GeneralDaemonTools());
      AddChildSection(general, new GeneralFileMenu());
      AddChildSection(general, new GeneralVolume());
      AddChildSection(general, new GeneralCDSpeed());
      //add DVD section
      Log.Info("add DVD section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding DVD section...");
      }
      SectionSettings dvd = new DVD();
      AddSection(dvd);
      Log.Info("  add DVD codec section");
      AddChildSection(dvd, new DVDCodec());
      Log.Info("  add DVD player section");
      AddChildSection(dvd, new DVDPlayer());
      Log.Info("  add DVD postprocessing section");
      AddChildSection(dvd, new DVDPostProcessing());
      //add movie section
      Log.Info("add movie section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding movie section...");
      }
      SectionSettings movie = new Movies();
      AddSection(movie);
      Log.Info("  add movie shares section");
      AddChildSection(movie, new MovieShares());
      Log.Info("  add movie extensions section");
      AddChildSection(movie, new MovieExtensions());
      Log.Info("  add movie database section");
      AddChildSection(movie, new MovieDatabase());
      Log.Info("  add movie views section");
      AddChildSection(movie, new MovieViews());
      Log.Info("  add movie player section");
      AddChildSection(movie, new MoviePlayer());
      Log.Info("  add movie postprocessing section");
      AddChildSection(movie, new MoviePostProcessing());
      //add music section
      Log.Info("add music section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding music section...");
      }
      SectionSettings music = new Sections.Music();
      AddSection(music);
      Log.Info("  add music shares section");
      AddChildSection(music, new MusicShares());
      Log.Info("  add music extensions section");
      AddChildSection(music, new MusicExtensions());
      Log.Info("  add music database section");
      AddChildSection(music, new MusicDatabase());
      Log.Info("  add music views section");
      AddChildSection(music, new MusicViews());
      Log.Info("  add music sort section");
      AddChildSection(music, new MusicSort());
      Log.Info("  add music import section");
      AddChildSection(music, new MusicImport());
      Log.Info("  add music dsp section");
      AddChildSection(music, new MusicDSP());
      Log.Info("  add music asio section");
      AddChildSection(music, new MusicASIO());
      //add pictures section
      Log.Info("add pictures section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding pictures section...");
      }
      SectionSettings picture = new Pictures();
      AddSection(picture);
      Log.Info("  add pictures shares section");
      AddChildSection(picture, new PictureShares());
      Log.Info("  add pictures extensions section");
      AddChildSection(picture, new PictureExtensions());
      //add radio section
      if (System.IO.File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        Log.Info("radio section not added - tv server plugin installed");
      }
      else
      {
        Log.Info("add radio section");
        if (splashScreen != null)
        {
          splashScreen.SetInformation("Adding radio section...");
        }
        SectionSettings radio = new Sections.Radio();
        AddSection(radio);
        Log.Info("  add radio stations section");
        AddChildSection(radio, new RadioStations());
      }
      //add television section
      Log.Info("add television section");
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding television section...");
      }
      SectionSettings television = new Television();
      AddSection(television);
      if (System.IO.File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"))
      {
        Log.Info("  add tv client section");
        AddChildSection(television, new TVClient());
        Log.Info("  add tv postprocessing section");
        AddChildSection(television, new TVPostProcessing());
        Log.Info("  add tv teletext section");
        AddChildSection(television, new TVTeletext());
      }
      else
      {
        Log.Info("  add tv capture cards section");
        AddChildSection(television, new TVCaptureCards());
        Log.Info("  add tv channels section");
        AddChildSection(television, new TVChannels());
        Log.Info("  add tv channel groups section");
        AddChildSection(television, new TVGroups());
        Log.Info("  add tv program guide section");
        AddChildSection(television, new TVProgramGuide());
        Log.Info("  add tv recording section");
        AddChildSection(television, new TVRecording());
        Log.Info("  add tv postprocessing section");
        AddChildSection(television, new TVPostProcessing());
        Log.Info("  add tv teletext section");
        AddChildSection(television, new TVTeletext());
      }
      //add remotes section
      SectionSettings remote = new Remote();
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding remote...");
      }
      AddSection(remote);
      Log.Info("add USBUIRT section");
      AddChildSection(remote, new RemoteUSBUIRT());
      Log.Info("add SerialUIR section");
      serialuir = new RemoteSerialUIR();
      AddChildSection(remote, serialuir);
      Log.Info("add WINLIRC section"); //sd00//
      AddChildSection(remote, new Sections.RemoteWinLirc()); //sd00//
      Log.Info("add RedEye section"); //PB00//
      redeye = new RemoteRedEye(); //PB00//
      AddChildSection(remote, redeye); //PB00//
      Log.Info("add DirectInput section");
      dinputRemote = new RemoteDirectInput();
      AddChildSection(remote, dinputRemote);
      //Look for Audio Decoders, if exist assume decoders are installed & present config option
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding filters section...");
      }
      FiltersSection filterSection = new FiltersSection();
      AddSection(filterSection);
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
      if (availableAudioFilters.Count > 0)
      {
        foreach (string filter in availableAudioFilters)
        {
          if (filter.Equals("NVIDIA Audio Decoder"))
          {
            AddChildSection(filterSection, new FiltersPureVideoDecoder());
          }
          if (filter.Equals("InterVideo Audio Decoder"))
          {
            AddChildSection(filterSection, new FiltersWinDVD7Decoder());
          }
          if (filter.Equals("CyberLink Audio Decoder (PDVD7)") || filter.Equals("CyberLink Audio Decode (PDVD7.x)"))
          {
            AddChildSection(filterSection, new FiltersPowerDVD7Decoder());
          }
          if (filter.Equals("MPA Decoder Filter"))
          {
            AddChildSection(filterSection, new FiltersMPEG2DecAudio());
          }
          if (filter.Equals("DScaler Audio Decoder"))
          {
            AddChildSection(filterSection, new FiltersDScalerAudio());
          }
        }
        ArrayList availableAACAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.LATMAAC);
        if (availableAACAudioFilters.Count > 0)
        {
          foreach (string filter in availableAACAudioFilters)
          {
            if (filter.Equals("MONOGRAM AAC Decoder"))
            {
              AddChildSection(filterSection, new FiltersMonogramAACDecoder());
            }
          }
        }
      }
      //Look for Video Decoders, if exist assume decoders are installed & present config option
      ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubTypeEx.MPEG2);
      if (availableVideoFilters.Count > 0)
      {
        foreach (string filter in availableVideoFilters)
        {
          if (filter.Equals("MPV Decoder Filter"))
          {
            AddChildSection(filterSection, new FiltersMPEG2DecVideo());
          }
          if (filter.Equals("DScaler Mpeg2 Video Decoder"))
          {
            AddChildSection(filterSection, new FiltersDScalerVideo());
          }
        }
      }
      //Add section for video renderer configuration
      AddChildSection(filterSection, new FiltersVideoRenderer());
      //Look for Audio Encoders, if exist assume encoders are installed & present config option
      string[] audioEncoders = new string[] { "InterVideo Audio Encoder" };
      FilterCollection legacyFilters = Filters.LegacyFilters;
      foreach (Filter audioCodec in legacyFilters)
        for (int i = 0; i < audioEncoders.Length; ++i)
        {
          if (String.Compare(audioCodec.Name, audioEncoders[i], true) == 0)
          {
            EncoderFiltersSection EncoderfilterSection = new EncoderFiltersSection();
            AddSection(EncoderfilterSection);
            AddChildSection(EncoderfilterSection, new FiltersInterVideoEncoder());
          }
        }
      //add weather section
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Adding weather section...");
      }
      Log.Info("add weather section");
      AddSection(new Weather());
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Loading plugins...");
      }
      Log.Info("add plugins section");
      AddSection(new PluginsNew());
      // Select first item in the section tree
      sectionTree.SelectedNode = sectionTree.Nodes[0];

      if (splashScreen != null)
      {
        splashScreen.Stop(500);
        splashScreen = null;
        BackgroundWorker FrontWorker = new BackgroundWorker();
        FrontWorker.DoWork += new DoWorkEventHandler(FrontWorker_DoWork);
        FrontWorker.RunWorkerAsync();
      }

      Log.Info("settingsform constructor done");
    }

    void FrontWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      Thread.CurrentThread.Name = "Config form waiter";
      IntPtr hwnd;
      // get the window handle of configuration.exe
      do
      {
        hwnd = FindWindow(null, _windowName);
        System.Threading.Thread.Sleep(100);
      }
      while (hwnd == IntPtr.Zero);
      System.Threading.Thread.Sleep(50);
      ShowWindow(hwnd, SW_SHOW);
      SetForegroundWindow(hwnd);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="section"></param>
    public void AddSection(SectionSettings section)
    {
      AddChildSection(null, section);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentSection"></param>
    /// <param name="section"></param>
    public void AddChildSection(SectionSettings parentSection, SectionSettings section)
    {
      // Make sure this section doesn't already exist
      if (settingSections.ContainsKey(section.Text))
      {
        return;
      }
      // Add section to tree
      SectionTreeNode treeNode = new SectionTreeNode(section);
      if (parentSection == null)
      {
        // Add to the root
        sectionTree.Nodes.Add(treeNode);
      }
      else
      {
        // Add to the parent node
        SectionTreeNode parentTreeNode = (SectionTreeNode)settingSections[parentSection.Text];
        parentTreeNode.Nodes.Add(treeNode);
      }
      settingSections.Add(section.Text, treeNode);
      //treeNode.EnsureVisible();
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
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // sectionTree
      // 
      this.sectionTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.sectionTree.FullRowSelect = true;
      this.sectionTree.HideSelection = false;
      this.sectionTree.HotTracking = true;
      this.sectionTree.Indent = 19;
      this.sectionTree.ItemHeight = 16;
      this.sectionTree.Location = new System.Drawing.Point(16, 28);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.Size = new System.Drawing.Size(184, 428);
      this.sectionTree.TabIndex = 2;
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      this.sectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.sectionTree_BeforeSelect);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(613, 479);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.Location = new System.Drawing.Point(532, 479);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 0;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(216, 28);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(472, 24);
      this.headerLabel.TabIndex = 3;
      this.headerLabel.TabStop = false;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // holderPanel
      // 
      this.holderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanel.AutoScroll = true;
      this.holderPanel.BackColor = System.Drawing.SystemColors.Control;
      this.holderPanel.Location = new System.Drawing.Point(216, 58);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(472, 398);
      this.holderPanel.TabIndex = 4;
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 469);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(688, 2);
      this.beveledLine1.TabIndex = 5;
      this.beveledLine1.TabStop = false;
      // 
      // applyButton
      // 
      this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.Location = new System.Drawing.Point(451, 479);
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
      this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(12, 484);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(113, 13);
      this.linkLabel1.TabIndex = 9;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Donate to MediaPortal";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // toolStrip1
      // 
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripSplitButton,
            this.configToolStripSplitButton});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(704, 25);
      this.toolStrip1.TabIndex = 10;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // helpToolStripSplitButton
      // 
      this.helpToolStripSplitButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
      this.helpToolStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateHelpToolStripMenuItem});
      this.helpToolStripSplitButton.Image = global::MediaPortal.Configuration.Properties.Resources.icon_help;
      this.helpToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.helpToolStripSplitButton.Name = "helpToolStripSplitButton";
      this.helpToolStripSplitButton.Size = new System.Drawing.Size(60, 22);
      this.helpToolStripSplitButton.Text = "Help";
      this.helpToolStripSplitButton.ButtonClick += new System.EventHandler(this.helpToolStripSplitButton_ButtonClick);
      // 
      // updateHelpToolStripMenuItem
      // 
      this.updateHelpToolStripMenuItem.Image = global::MediaPortal.Configuration.Properties.Resources.icon_refresh;
      this.updateHelpToolStripMenuItem.Name = "updateHelpToolStripMenuItem";
      this.updateHelpToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
      this.updateHelpToolStripMenuItem.Text = "Update Help";
      this.updateHelpToolStripMenuItem.ToolTipText = "Online update for the help references file. Start an incorrect wiki page was open" +
          "ed.";
      this.updateHelpToolStripMenuItem.Click += new System.EventHandler(this.updateHelpToolStripMenuItem_Click);
      // 
      // configToolStripSplitButton
      // 
      this.configToolStripSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.thumbsToolStripMenuItem,
            this.logsToolStripMenuItem,
            this.databaseToolStripMenuItem,
            this.skinsToolStripMenuItem});
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
      // SettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScroll = true;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(704, 510);
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
      this.Text = "MediaPortal - Setup";
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="section"></param>
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
        Log.Write(ex);
      }
      return true;
    }

    private void SettingsForm_Closed(object sender, EventArgs e)
    {
      // Restart MCE services
      MediaPortal.Util.Utils.RestartMCEServices();
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SettingsForm_Load(object sender, EventArgs e)
    {
      GUIGraphicsContext.form = this;
      // Asynchronously pre-initialize the music engine if we're using the BassMusicPlayer
      if (MediaPortal.Player.BassMusicPlayer.IsDefaultMusicPlayer)
        MediaPortal.Player.BassMusicPlayer.CreatePlayerAsync();
      Log.Info("Load settings");
      foreach (TreeNode treeNode in sectionTree.Nodes)
      {
        // Load settings for all sections
        Log.Info("  Load settings:{0}", treeNode.Text);
        LoadSectionSettings(treeNode);
      }
      Log.Info("Load settings done");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentNode"></param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentNode"></param>
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
      int MaximumShares = 20;
      //Do we have 1 or more music,picture,video shares?
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
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
          MessageBox.Show("No movie playlist folder specified", "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return false;
        }
        bool added = false;
        for (int index = 0; index < MaximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string sharePathData = xmlreader.GetValueAsString("music", sharePath, "");
          if (!MediaPortal.Util.Utils.IsDVD(sharePathData) && sharePathData != string.Empty)
          {
            added = true;
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
          if (!MediaPortal.Util.Utils.IsDVD(shareNameData) && shareNameData != string.Empty)
          {
            added = true;
          }
        }
        if (!added)
        {
          MessageBox.Show("No movie folders specified", "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return false;
        }
        added = false;
        for (int index = 0; index < MaximumShares; index++)
        {
          string sharePath = String.Format("sharepath{0}", index);
          string shareNameData = xmlreader.GetValueAsString("pictures", sharePath, "");
          if (!MediaPortal.Util.Utils.IsDVD(shareNameData) && shareNameData != string.Empty)
          {
            added = true;
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
        bool lastFmOn = xmlreader.GetValueAsBool("plugins", "My Last.fm Radio", false);
        if (lastFmOn && !audioScrobblerOn)
        {
          MessageBox.Show("Please configure the Audioscrobbler plugin to use Last.fm radio", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          return false;
        }

        if (audioScrobblerOn)
        {
          // Does Audioscrobbler have a user but no password (due to DB upgrades, restores, etc)
          string asuser = xmlreader.GetValueAsString("audioscrobbler", "user", "");
          if (!string.IsNullOrEmpty(asuser))
          {
            MediaPortal.Music.Database.MusicDatabase mdb = MediaPortal.Music.Database.MusicDatabase.Instance;
            string AsPass = mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(asuser)), "");
            if (string.IsNullOrEmpty(AsPass))
            {
              MessageBox.Show("No password specified for current Audioscrobbler user", "MediaPortal Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
              return false;
            }
          }
        }
      }
      return true;
    }

    private void SaveAllSettings()
    {
      foreach (TreeNode treeNode in sectionTree.Nodes)
      {
        // Save settings for all sections
        SaveSectionSettings(treeNode);
      }
      Settings.SaveCache();
    }

    private void applyButton_Click(object sender, EventArgs e)
    {
      try
      {
        // Check if MediaPortal is running, if so inform user that it needs to be restarted
        // for the changes to take effect.
        string processName = "MediaPortal";
        foreach (Process process in Process.GetProcesses())
        {
          if (process.ProcessName.Equals(processName))
          {
            DialogResult dialogResult =
              MessageBox.Show("For the changes to take effect you need to restart MediaPortal, restart now?",
                              "MediaPortal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
              try
              {
                // Kill the MediaPortal process by finding window and sending ALT+F4 to it.
                IECallBack ewp = new IECallBack(EnumWindowCallBack);
                EnumWindows(ewp, 0);
                process.CloseMainWindow();
                // Wait for the process to die, we wait for a maximum of 10 seconds
                if (process.WaitForExit(10000))
                {
                  SaveAllSettings();
                  // Start the MediaPortal process
                  Process.Start(processName + ".exe");
                  return;
                }
              }
              catch
              {
                // Ignore
              }
              break;
            }
          }
        }
      }
      catch (Exception)
      { }
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
      System.Diagnostics.Process.Start((string)e.Link.LinkData);
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
  }
}