#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration.Sections;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using Keys = MediaPortal.Configuration.Sections.Keys;
using MediaPortal.Utils.Services;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for Settings.
  /// </summary>
  public class SettingsForm : Form
  {
    public delegate bool IECallBack(int hwnd, int lParam);


    private const int SW_SHOWNORMAL = 1;
    //private const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr window, int message, int wparam, int lparam);

    //[DllImport("user32.dll")]
    //private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.Dll")]
    public static extern int EnumWindows(IECallBack x, int y);

    [DllImport("User32.Dll")]
    public static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);

    [DllImport("User32.Dll")]
    public static extern void GetClassName(int h, StringBuilder s, int nMaxCount);

    private MPButton cancelButton;
    private MPButton okButton;
    private MPBeveledLine beveledLine1;
    private TreeView sectionTree;
    private Panel holderPanel;
    private MPGradientLabel headerLabel;
    private SerialUIR serialuir;
    private RedEye redeye; //PB00//
    private DirectInputRemote dinputRemote;

    protected ILog _log;
    //
    // Hashtable where we store each added tree node/section for faster access
    //
    public static Hashtable SettingSections
    {
      get { return settingSections; }
    }

    private static Hashtable settingSections = new Hashtable();
    private MPButton applyButton;
    //private System.ComponentModel.IContainer components;

    public SettingsForm()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();

      _log.Info("SettingsForm constructor");
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      // Stop MCE services
      MediaPortal.Util.Utils.StopMCEServices();

      //
      // Set caption
      //
      Text = "MediaPortal - Setup";

      //
      // Build options tree
      //

      string strLanguage;
      using (Settings xmlreader = new Settings("MediaPortal.xml"))
      {
        strLanguage = xmlreader.GetValueAsString("skin", "language", "English");
      }
      GUILocalizeStrings.Load(@"language\" + strLanguage + @"\strings.xml");

      _log.Info("add project section");
      Project project = new Project();
      AddSection(project);

      _log.Info("add general section");
      General general = new General();
      AddSection(general);

      _log.Info("add skins section");
      AddChildSection(general, new Skin());
      AddChildSection(general, new KeyboardControl());
      AddChildSection(general, new Keys());
      AddChildSection(general, new GeneralOSD());
      AddChildSection(general, new GeneralSkipSteps());
      AddChildSection(general, new Sections.DaemonTools());
      AddChildSection(general, new FileMenu());
      AddChildSection(general, new Volume());
      
      _log.Info("add DVD section");
      SectionSettings dvd = new DVD();
      AddSection(dvd);

      _log.Info("  add DVD codec section");
      AddChildSection(dvd, new DVDCodec());

      _log.Info("  add DVD player section");
      AddChildSection(dvd, new DVDPlayer());

      _log.Info("  add DVD postprocessing section");
      AddChildSection(dvd, new DVDPostProcessing());

      _log.Info("add movie section");
      SectionSettings movie = new Movies();
      AddSection(movie);

      _log.Info("  add movie shares section");
      AddChildSection(movie, new MovieShares());
      _log.Info("  add movie player section");
      AddChildSection(movie, new MoviePlayer());
      _log.Info("  add movie extensions section");
      AddChildSection(movie, new MovieExtensions());
      _log.Info("  add movie postprocessing section");
      AddChildSection(movie, new MoviePostProcessing());
      AddChildSection(movie, new MovieDatabase());
      AddChildSection(movie, new MovieViews());

      _log.Info("add music section");
      SectionSettings music = new Sections.Music();
      AddSection(music);
      _log.Info("  add music shares section");
      AddChildSection(music, new MusicShares());
      _log.Info("  add music database section");
      AddChildSection(music, new MusicDatabase());
      _log.Info("  add music extension section");
      AddChildSection(music, new MusicExtensions());
      _log.Info("  add music views section");
      AddChildSection(music, new MusicViews());
      _log.Info("  add music import section");
      AddChildSection(music, new MusicImport());
      AddChildSection(music, new MusicSort());

      _log.Info("  add music misc section");
      AddChildSection(music, new MusicMisc());

      _log.Info("add pictures section");
      SectionSettings picture = new Pictures();
      AddSection(picture);
      _log.Info("  add pictures shares section");
      AddChildSection(picture, new PictureShares());
      _log.Info("  add pictures extensions section");
      AddChildSection(picture, new PictureExtensions());

      _log.Info("add radio section");
      SectionSettings radio = new Sections.Radio();
      AddSection(radio);
      _log.Info("  add radio stations section");
      AddChildSection(radio, new RadioStations());

      _log.Info("add television section");
      SectionSettings television = new Television();
      AddSection(television);
      _log.Info("  add tv capture cards section");
      AddChildSection(television, new TVCaptureCards());
      _log.Info("  add tv channels section");
      AddChildSection(television, new SectionTvChannels());
      _log.Info("  add tv channel groups section");
      AddChildSection(television, new SectionTvGroups());
      _log.Info("  add tv program guide section");
      AddChildSection(television, new TVProgramGuide());
      _log.Info("  add tv recording section");
      AddChildSection(television, new TVRecording());
      _log.Info("  add tv postprocessing section");
      AddChildSection(television, new TVPostProcessing());

      SectionSettings remote = new Remote();
      AddSection(remote);

      _log.Info("add USBUIRT section");
      AddChildSection(remote, new USBUIRT());
      _log.Info("add SerialUIR section");
      serialuir = new SerialUIR();
      AddChildSection(remote, serialuir);
      _log.Info("add WINLIRC section"); //sd00//
      AddChildSection(remote, new Sections.WINLIRC()); //sd00//
      _log.Info("add RedEye section"); //PB00//
      redeye = new RedEye(); //PB00//
      AddChildSection(remote, redeye); //PB00//

      _log.Info("add DirectInput section");
      dinputRemote = new DirectInputRemote();
      AddChildSection(remote, dinputRemote);

      //Look for Audio Decoders, if exist assume decoders are installed & present config option
      FiltersSection filterSection = new FiltersSection();
      AddSection(filterSection);
      ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Mpeg2Audio);
      if (availableAudioFilters.Count > 0)
      {
        foreach (string filter in availableAudioFilters)
        {
          if (filter.Equals("NVIDIA Audio Decoder"))
          {
            AddChildSection(filterSection, new PureVideoDecoderFilters());
          }
          if (filter.Equals("InterVideo Audio Decoder"))
          {
            AddChildSection(filterSection, new WinDVD7DecoderFilters());
          }
          if (filter.Equals("CyberLink Audio Decoder"))
          {
            AddChildSection(filterSection, new PowerDVD6DecoderFilters());
          }
          if (filter.Equals("MPA Decoder Filter"))
          {
            AddChildSection(filterSection, new MPEG2DecAudioFilter());
          }
          if (filter.Equals("DScaler Audio Decoder"))
          {
            AddChildSection(filterSection, new DScalerAudioFilter());
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
            AddChildSection(filterSection, new MPEG2DecVideoFilter());
          }
          if (filter.Equals("DScaler Mpeg2 Video Decoder"))
          {
            AddChildSection(filterSection, new DScalerVideoFilter());
          }
        }
      }

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
            AddChildSection(EncoderfilterSection, new InterVideoEncoderFilters());
          }
        }
      
      _log.Info("add weather section");
      AddSection(new Weather());
      _log.Info("add plugins section");
      AddSection(new PluginsNew());

      //
      // Select first item in the section tree
      //
      sectionTree.SelectedNode = sectionTree.Nodes[0];

      _log.Info("bring to front");
      // make sure window is in front of mediaportal
      BringToFront();
      _log.Info("settingsform constructor done");
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
      //
      // Make sure this section doesn't already exist
      //
      if (settingSections.ContainsKey(section.Text))
      {
        return;
      }

      //
      // Add section to tree
      //
      SectionTreeNode treeNode = new SectionTreeNode(section);

      if (parentSection == null)
      {
        //
        // Add to the root
        //
        sectionTree.Nodes.Add(treeNode);
      }
      else
      {
        //
        // Add to the parent node
        //
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
      this.sectionTree = new System.Windows.Forms.TreeView();
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.okButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.applyButton = new MediaPortal.UserInterface.Controls.MPButton();
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
      this.sectionTree.Location = new System.Drawing.Point(16, 16);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.Size = new System.Drawing.Size(184, 440);
      this.sectionTree.TabIndex = 2;
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      this.sectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.sectionTree_BeforeSelect);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(621, 479);
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
      this.okButton.Location = new System.Drawing.Point(542, 479);
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
      this.headerLabel.Location = new System.Drawing.Point(216, 16);
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
      this.holderPanel.Location = new System.Drawing.Point(216, 48);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(472, 408);
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
      this.applyButton.Location = new System.Drawing.Point(462, 479);
      this.applyButton.Name = "applyButton";
      this.applyButton.Size = new System.Drawing.Size(75, 23);
      this.applyButton.TabIndex = 6;
      this.applyButton.TabStop = false;
      this.applyButton.Text = "&Apply";
      this.applyButton.UseVisualStyleBackColor = true;
      this.applyButton.Visible = false;
      this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // SettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.AutoScroll = true;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(704, 510);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.sectionTree);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "SettingsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Settings";
      this.Closed += new System.EventHandler(this.SettingsForm_Closed);
      this.Load += new System.EventHandler(this.SettingsForm_Load);
      this.ResumeLayout(false);

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
      if (section.CanActivate == false)
      {
        return false;
      }

      section.Dock = DockStyle.Fill;
      section.OnSectionActivated();

      holderPanel.Controls.Clear();
      holderPanel.Controls.Add(section);

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
      _log.Info("Load settings");
      foreach (TreeNode treeNode in sectionTree.Nodes)
      {
        //
        // Load settings for all sections
        //

        _log.Info("  Load settings:{0}", treeNode.Text);
        LoadSectionSettings(treeNode);
      }
      _log.Info("Load settings done");
      GUIGraphicsContext.form = this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentNode"></param>
    private void LoadSectionSettings(TreeNode currentNode)
    {
      _log.Info("LoadSectionSettings()");
      if (currentNode != null)
      {
        //
        // Load settings for current node
        //
        SectionTreeNode treeNode = currentNode as SectionTreeNode;

        if (treeNode != null)
        {
          treeNode.Section.LoadSettings();
        }

        //
        // Load settings for all child nodes
        //
        foreach (TreeNode childNode in treeNode.Nodes)
        {
          _log.Info("  Load settings:{0}", childNode.Text);
          LoadSectionSettings(childNode);
        }
      }
      _log.Info("LoadSectionSettings() done");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentNode"></param>
    private void SaveSectionSettings(TreeNode currentNode)
    {
      _log.Info("SaveSectionSettings()");
      if (currentNode != null)
      {
        //
        // Save settings for current node
        //
        SectionTreeNode treeNode = currentNode as SectionTreeNode;

        if (treeNode != null)
        {
          treeNode.Section.SaveSettings();
        }

        //
        // Load settings for all child nodes
        //
        foreach (TreeNode childNode in treeNode.Nodes)
        {
          _log.Info("SaveSectionSettings:{0}", childNode.Text);
          SaveSectionSettings(childNode);
        }
      }
      _log.Info("SaveSectionSettings done()");
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
      using (Settings xmlreader = new Settings("MediaPortal.xml"))
      {
        string playlistFolder = xmlreader.GetValueAsString("music", "playlists", "");
        if (playlistFolder == String.Empty)
        {
          MessageBox.Show("No music playlist folder specified", "MediaPortal Settings", MessageBoxButtons.OK,
                          MessageBoxIcon.Exclamation);
          return false;
        }
        playlistFolder = xmlreader.GetValueAsString("movies", "playlists", "");
        if (playlistFolder == String.Empty)
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
          if (!MediaPortal.Util.Utils.IsDVD(sharePathData) && sharePathData != String.Empty)
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
          if (!MediaPortal.Util.Utils.IsDVD(shareNameData) && shareNameData != String.Empty)
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
          if (!MediaPortal.Util.Utils.IsDVD(shareNameData) && shareNameData != String.Empty)
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
      }
      return true;
    }

    private void SaveAllSettings()
    {
      foreach (TreeNode treeNode in sectionTree.Nodes)
      {
        //
        // Save settings for all sections
        //
        SaveSectionSettings(treeNode);
      }
      Settings.SaveCache();
    }

    private void applyButton_Click(object sender, EventArgs e)
    {
      try
      {
        //
        // Check if MediaPortal is running, if so inform user that it needs to be restarted
        // for the changes to take effect.
        //
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
                //
                // Kill the MediaPortal process by finding window and sending ALT+F4 to it.
                //
                IECallBack ewp = new IECallBack(EnumWindowCallBack);
                EnumWindows(ewp, 0);
                process.CloseMainWindow();

                //
                // Wait for the process to die, we wait for a maximum of 10 seconds
                //
                if (process.WaitForExit(10000))
                {
                  SaveAllSettings();
                  //
                  // Start the MediaPortal process
                  // 
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
  }
}
