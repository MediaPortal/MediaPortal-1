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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration.TVE2.Sections;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.UserInterface.Controls;
using Television = MediaPortal.Configuration.TVE2.Sections.Television;
using TVPostProcessing = MediaPortal.Configuration.TVE2.Sections.TVPostProcessing;
using TVTeletext = MediaPortal.Configuration.TVE2.Sections.TVTeletext;

namespace MediaPortal.Configuration.TVE2
{
  /// <summary>
  /// Summary description for Settings.
  /// </summary>
  public class TVE2_SettingsForm : MPConfigForm
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

      public ConfigPage(SectionSettings aParentsection, SectionSettings aConfigSection)
      {
        sectionName = aConfigSection.Text;
        parentsection = aParentsection;
        configSection = aConfigSection;
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

    }

    #endregion

    #region Variables

    public delegate bool IECallBack(int hwnd, int lParam);

    private SectionSettings _previousSection;
    private MPButton cancelButton;
    private MPButton okButton;
    private MPBeveledLine beveledLine1;
    private TreeView sectionTree;
    private Panel holderPanel;
    private MPGradientLabel headerLabel;
    private MPButton applyButton;


    #region Properties

    // Hashtable where we store each added tree node/section for faster access
    private static Dictionary<string, ConfigPage> settingSections = new Dictionary<string, ConfigPage>();

    public static Dictionary<string, ConfigPage> SettingSections
    {
      get { return settingSections; }
    }

    #endregion

    #endregion

    public TVE2_SettingsForm()
    {
      OnStartup();
    }

    private void OnStartup()
    {
      // start the splashscreen      
      Log.Info("SettingsForm constructor");
      // Required for Windows Form Designer support
      InitializeComponent();
      // Stop MCE services
      string strLanguage;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        strLanguage = xmlreader.GetValueAsString("skin", "language", "English");

      }
      GUILocalizeStrings.Load(strLanguage);
      // Register Bass.Net
      Log.Info("add project section");

      AddTabTelevision();
      AddTabRadio();

      ToggleSectionVisibility();

      // Select first item in the section tree
      if (sectionTree.Nodes.Count > 0)
      {
        sectionTree.SelectedNode = sectionTree.Nodes[0];
      }


      Log.Info("settingsform constructor done");
    }

    #region Section handling

    private void AddTabRadio()
    {

      SectionSettings radio = new Sections.Radio();
      AddSection(new ConfigPage(null, radio));
      Log.Info("  add radio stations section");
      AddSection(new ConfigPage(radio, new RadioStations()));
    }

    private void AddTabTelevision()
    {
      //add television section
      Log.Info("add television section");
      SectionSettings television = new Television();
      AddSection(new ConfigPage(null, television));

      Log.Info("  add tv capture cards section");
      AddSection(new ConfigPage(television, new TVCaptureCards()));
      Log.Info("  add tv channels section");
      AddSection(new ConfigPage(television, new TVChannels()));
      Log.Info("  add tv channel groups section");
      AddSection(new ConfigPage(television, new TVGroups()));
      Log.Info("  add tv program guide section");
      AddSection(new ConfigPage(television, new TVProgramGuide()));
      Log.Info("  add tv recording section");
      AddSection(new ConfigPage(television, new TVRecording()));

      Log.Info("  add tv postprocessing section");
      AddSection(new ConfigPage(television, new TVPostProcessing()));
      Log.Info("  add tv teletext section");
      AddSection(new ConfigPage(television, new TVTeletext()));
    }

    #endregion

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
      this.sectionTree.Location = new System.Drawing.Point(16, 12);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.Size = new System.Drawing.Size(184, 478);
      this.sectionTree.TabIndex = 2;
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      this.sectionTree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.sectionTree_BeforeSelect);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(216, 12);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(480, 24);
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
      this.holderPanel.Location = new System.Drawing.Point(216, 42);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(480, 448);
      this.holderPanel.TabIndex = 4;
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 503);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(696, 2);
      this.beveledLine1.TabIndex = 5;
      this.beveledLine1.TabStop = false;
      // 
      // applyButton
      // 
      this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
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
      // TVE2_SettingsForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScroll = true;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(712, 544);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.sectionTree);
      this.Name = "TVE2_SettingsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MediaPortal - TVEngine 2 - Configuration";
      this.Load += new System.EventHandler(this.SettingsForm_Load);
      this.Closed += new System.EventHandler(this.SettingsForm_Closed);
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
      } catch (Exception ex)
      {
        Log.Error(ex);
      }
      return true;
    }

    private void SettingsForm_Closed(object sender, EventArgs e)
    {
    }

    private void SettingsForm_Load(object sender, EventArgs e)
    {
      GUIGraphicsContext.form = this;
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

    private void cancelButton_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      applyButton_Click(sender, e);
      Close();
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
      SaveAllSettings();
    }
    private void ToggleSectionVisibility()
    {
      // using property so setter updates values..

      sectionTree.BeginUpdate();
      TreeNode currentSelected = (TreeNode)sectionTree.SelectedNode;
      sectionTree.Nodes.Clear();

      foreach (KeyValuePair<string, ConfigPage> singleConfig in settingSections)
      {
        ConfigPage currentSection = singleConfig.Value;

        SectionTreeNode treeNode = new SectionTreeNode(currentSection.ConfigSection);
        // If not parent is specified we add the section as root node.
        if (currentSection.Parentsection == null)
        {
          // Add to the root
          sectionTree.Nodes.Add(treeNode);
        } else
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
  }
}