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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils.ExtensionMethods;

namespace Mediaportal.TV.Server.SetupTV
{
  /// <summary>
  /// Summary description for Settings.
  /// </summary>
  public class SetupTvSettingsForm : SettingsForm
  {
    private readonly PluginLoaderSetupTv _pluginLoader = new PluginLoaderSetupTv();
    private Sections.Plugins pluginsRoot;
    private TvCards _sectionTuners;
    private bool showAdvancedSettings;

    public SetupTvSettingsForm()
      : this(false)
    {
    }

    public SetupTvSettingsForm(bool ShowAdvancedSettings)
      : base(ServiceHelper.IsRestrictedMode)
    {
      showAdvancedSettings = ShowAdvancedSettings;
      InitializeComponent();
      try
      {
        Init();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Failed to startup cause of exception");
      }
    }

    private void Init()
    {
      CheckForIllegalCrossThreadCalls = false;
      //
      // Set caption
      //
      Text = "MediaPortal - TV Server Configuration";

      //
      // Build options tree
      //    
      Project project = new Project();
      AddSection(project);
      
      _sectionTuners = new TvCards(OnServerConfigurationChanged);
      AddSection(_sectionTuners);
      AddServerTvCards(false);

      Channels channels = new Channels("TV Channels", MediaTypeEnum.TV);            
      AddSection(channels);
      AddChildSection(channels, new ChannelCombinations("TV Combinations", MediaTypeEnum.TV));
      AddChildSection(channels, new ChannelMapping("TV Mapping", MediaTypeEnum.TV));

      Channels radioChannels = new Channels("Radio Channels", MediaTypeEnum.Radio);        
      AddSection(radioChannels);
      AddChildSection(radioChannels, new ChannelCombinations("Radio Combinations", MediaTypeEnum.Radio));
      AddChildSection(radioChannels, new ChannelMapping("Radio Mapping", MediaTypeEnum.Radio));

      Epg dvbEpg = new Epg();
      AddSection(dvbEpg);
      AddChildSection(dvbEpg, new EpgGrabber("TV EPG Grabber", "epgLanguages", "epgStoreOnlySelected", MediaTypeEnum.TV));
      AddChildSection(dvbEpg, new EpgGrabber("Radio EPG Grabber", "radioLanguages", "epgRadioStoreOnlySelected", MediaTypeEnum.Radio));
      AddChildSection(dvbEpg, new EpgGenreMap());

      AddSection(new ScanSettings());
      AddSection(new TvRecording());
      AddSection(new TvTimeshifting());
      AddSection(new TvSchedules());
      AddSection(new StreamingServer());
      AddSection(new UserPriorities());

      AddSection(new TestService("Manual Control"));
      AddSection(new TestChannels("Test Channels"));

      _pluginLoader.Load();
      pluginsRoot = new Sections.Plugins("Plugins", _pluginLoader);
      AddSection(pluginsRoot);

      pluginsRoot.ChangedActivePlugins += SectChanged;

      foreach (ITvServerPlugin plugin in _pluginLoader.Plugins)
      {
        SectionSettings settings = plugin.Setup;
        if (settings != null)
        {
          bool isActive = ServiceAgents.Instance.SettingServiceAgent.GetValue(String.Format("plugin{0}", plugin.Name), false);
          settings.Text = plugin.Name;
          if (isActive)
          {
            AddChildSection(pluginsRoot, settings);
          }
        }
      }
      if (showAdvancedSettings)
      {
        AddSection(new DebugOptions());
      }
      //AddSection(new ImportExport());
      AddSection(new ThirdPartyChecks());

      sectionTree.SelectedNode = sectionTree.Nodes[0];

      // make sure window is in front of mediaportal
      BringToFront();
    }

    private void AddServerTvCards(bool reloaded)
    {
      IList<Card> cards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
      foreach (Card dbsCard in cards)
      {
        if (dbsCard.Enabled && ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(dbsCard.IdCard))
        {
          CardType type = ServiceAgents.Instance.ControllerServiceAgent.Type(dbsCard.IdCard);
          string cardName = string.Format("{0} {1} {2}", dbsCard.IdCard, type.GetDescription(), dbsCard.Name);
          switch (type)
          {
            case CardType.Analog:
              AddChildSection(_sectionTuners, new CardAnalog(cardName, dbsCard.IdCard), 1);
              break;
            case CardType.DvbT:
              AddChildSection(_sectionTuners, new CardDvbT(cardName, dbsCard.IdCard), 1);
              break;
            case CardType.DvbC:
              AddChildSection(_sectionTuners, new CardDvbC(cardName, dbsCard.IdCard), 1);
              break;
            case CardType.DvbS:
              AddChildSection(_sectionTuners, new CardDvbS(cardName, dbsCard.IdCard), 1);
              break;
            case CardType.Atsc:
              AddChildSection(_sectionTuners, new CardAtsc(cardName, dbsCard.IdCard), 1);
              break;
            case CardType.DvbIP:
              AddChildSection(_sectionTuners, new CardDvbIP(cardName, dbsCard.IdCard), 1);
              break;
          }
        }
      }
      if (reloaded)
      {
        SectionTreeNode activeNode = _sections[_sectionTuners.Text];
        if (activeNode != null)
        {
          activeNode.Expand();
        }
      }
    }

    /// <summary>
    /// called when tvcards were changed (add, remove, enable, disable)
    /// </summary>
    public void OnServerConfigurationChanged(object sender, bool reloadConfigController, HashSet<int> reloadConfigTuners)
    {
      if (reloadConfigController)
      {
        bool isUserTimeShifting;
        bool isAnyUserTimeShifting;
        bool isAnyUserRecording;
        bool isAnyTunerTimeShiftingOrRecording = ServiceAgents.Instance.ControllerServiceAgent.IsAnyCardRecordingOrTimeshifting(
                                                  new User().Name, out isUserTimeShifting, out isAnyUserTimeShifting, out isAnyUserRecording);
        if (!isAnyTunerTimeShiftingOrRecording && !isAnyUserTimeShifting && !isAnyUserRecording && !isUserTimeShifting)
        {
          Log.Info("base: reloading controller configuration");
          NotifyForm dlgNotify = new NotifyForm("Restarting TV service...", "This can take some time.\n\nPlease be patient...");
          try
          {
            dlgNotify.Show();
            dlgNotify.WaitForDisplay();

            ServiceAgents.Instance.ControllerServiceAgent.Restart();

            // remove all cards, add current ones back later
            RemoveAllChildSections(_sections[_sectionTuners.Text]);

            // re-add cards to tree          
            AddServerTvCards(true);
          }
          finally
          {
            dlgNotify.Close();
          }
        }
        else
        {
          MessageBox.Show(this, "TV Server is currently timeshifting and/or recording. Please restart the TV service manually to apply your changes.");
        }
      }
      else if (reloadConfigTuners != null && reloadConfigTuners.Count > 0)
      {
        Log.Info("base: reloading configuration for tuners {0}", string.Join(", ", reloadConfigTuners));
        ServiceAgents.Instance.ControllerServiceAgent.ReloadCardConfiguration(reloadConfigTuners);
      }
    }

    public void RemoveAllChildSections(SectionTreeNode parentTreeNode)
    {
      // Remove section from tree
      if (parentTreeNode != null)
      {
        foreach (SectionTreeNode childNode in parentTreeNode.Nodes)
        {
          // recursive delete all children
          RemoveAllChildSections(childNode);

          //Remove the section from the hashtable in case we add it again
          _sections.Remove(childNode.Text);
        }
        // first remove all children and sections, then nodes themself (otherwise collection changes during iterate)
        foreach (SectionTreeNode childNode in parentTreeNode.Nodes)
        {
          if (childNode != null)
          {
            parentTreeNode.Nodes.Remove(childNode);
          }
        }
      }
    }

    public void RemoveAllChildSections(SectionSettings parentSection)
    {
      // Remove section from tree
      if (parentSection != null)
      {
        SectionTreeNode parentTreeNode = (SectionTreeNode)_sections[parentSection.Text];
        foreach (SectionTreeNode childNode in parentTreeNode.Nodes)
        {
          // recursive delete all children
          RemoveAllChildSections(childNode);

          //Remove the section from the hashtable in case we add it again
          _sections.Remove(childNode.Text);
          parentTreeNode.Nodes.Remove(childNode);
        }
      }
    }

    public void RemoveChildSection(SectionSettings parentSection, SectionSettings section)
    {
      // Remove section from tree
      if (parentSection != null)
      {
        SectionTreeNode parentTreeNode = (SectionTreeNode)_sections[parentSection.Text];

        for (int i = 0; i < parentTreeNode.GetNodeCount(true); i++)
        {
          if (parentTreeNode.Nodes[i].Name == section.Text)
          {
            //Remove the section from the hashtable in case we add it again
            _sections.Remove(section.Text);
            parentTreeNode.Nodes.Remove(parentTreeNode.Nodes[i]);
          }
        }
      }
    }


    public void AddSection(SectionSettings section, int imageIndex)
    {
      AddChildSection(null, section, imageIndex);
    }

    public void AddChildSection(SectionSettings parentSection, SectionSettings section, int imageIndex)
    {
      //
      // Make sure this section doesn't already exist
      //

      //
      // Add section to tree
      //
      SectionTreeNode treeNode = new SectionTreeNode(section);

      if (parentSection == null)
      {
        //
        // Add to the root
        //
        treeNode.ImageIndex = imageIndex;
        treeNode.SelectedImageIndex = imageIndex;
        sectionTree.Nodes.Add(treeNode);
      }
      else
      {
        //
        // Add to the parent node
        //
        SectionTreeNode parentTreeNode = (SectionTreeNode)_sections[parentSection.Text];

        treeNode.ImageIndex = imageIndex;
        treeNode.SelectedImageIndex = imageIndex;
        parentTreeNode.Nodes.Add(treeNode);
      }

      _sections.Add(section.Text, treeNode);

      //treeNode.EnsureVisible();
    }


    public override void AddSection(SectionSettings section)
    {
      AddChildSection(null, section);
    }

    /// <summary>
    /// Called when a plugin is selected or deselected for activation in the plugins setting
    /// </summary>
    /// <param name="sender">a Setting parameter passed as object</param>
    /// <param name="e">eventarg will always retrun empty</param>
    public void SectChanged(object sender, EventArgs e)
    {
      string settingName = ((string)sender);
      string pluginName = settingName.Substring(6);

      foreach (ITvServerPlugin plugin in _pluginLoader.Plugins)
      {
        SectionSettings settings = plugin.Setup;
        if (settings != null && plugin.Name == pluginName)
        {
          bool isActive = ServiceAgents.Instance.SettingServiceAgent.GetValue(settingName, false);
          settings.Text = pluginName;

          if (isActive)
          {
            AddChildSection(pluginsRoot, settings);
            LoadChildSettingsFromNode(pluginsRoot, settings);
          }
          else
          {
            RemoveChildSection(pluginsRoot, settings);
            SaveChildSettingsFromNode(pluginsRoot, settings);
          }

          break;
        }
      }
    }

    public override void AddChildSection(SectionSettings parentSection, SectionSettings section)
    {
      //
      // Make sure this section doesn't already exist
      //

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
        SectionTreeNode parentTreeNode = (SectionTreeNode)_sections[parentSection.Text];
        treeNode.Name = section.Text;
        parentTreeNode.Nodes.Add(treeNode);
      }

      _sections.Add(section.Text, treeNode);

      //treeNode.EnsureVisible();
    }

    public override void sectionTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
    {
      base.sectionTree_BeforeSelect(sender, e);

      if (!e.Cancel)
      {
        if (!ServiceHelper.IsAvailable)
        {
          MessageBox.Show("TvService not started.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          e.Cancel = true;
        } 
      }        
    }

    public override bool ActivateSection(SectionSettings section)
    {
      try
      {
        if (section.CanActivate == false)
        {
          return false;
        }
        try
        {
          ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;
        }
        catch (Exception) {}
        //DatabaseManager.Instance.SaveChanges();
        //DatabaseManager.Instance.ClearQueryCache();
        Cursor = Cursors.WaitCursor;
        section.Dock = DockStyle.Fill;
        
        holderPanel.Controls.Clear();
        holderPanel.Controls.Add(section);

        // Deactive the previous section before activating a new one. Some code
        // (eg. CA menu handling) relies on the order.
        if (section != _previousSection && _previousSection != null)
        {
          _previousSection.OnSectionDeActivated();
        }
        section.OnSectionActivated();
        _previousSection = section;
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        Cursor = Cursors.Default;
      }
      return true;
    }


    public override void SettingsForm_Closed(object sender, EventArgs e)
    {
      try
      {
        if (RemoteControl.IsConnected)
        {
          ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
          ServiceAgents.Instance.ControllerServiceAgent.OnNewSchedule();
        }
      }
      catch (Exception) {}
    }

    public override void SettingsForm_Load(object sender, EventArgs e)
    {
      foreach (TreeNode treeNode in sectionTree.Nodes)
      {
        //
        // Load settings for all sections
        //

        LoadSectionSettings(treeNode);
      }
    }

    public SectionTreeNode GetChildNode(SectionSettings parentSection, SectionSettings section)
    {
      SectionTreeNode treeNode = new SectionTreeNode(section);
      SectionTreeNode parentTreeNode = (SectionTreeNode)_sections[parentSection.Text];

      for (int i = 0; i < parentTreeNode.GetNodeCount(true); i++)
      {
        if (parentTreeNode.Nodes[i].Name == section.Text)
          return treeNode;
      }
      return null;
    }

    public void LoadChildSettingsFromNode(SectionSettings parentSection, SectionSettings section)
    {
      if (parentSection != null)
        LoadSectionSettings(GetChildNode(parentSection, section));
    }

    public void SaveChildSettingsFromNode(SectionSettings parentSection, SectionSettings section)
    {
      if (parentSection != null)
        SaveSectionSettings(GetChildNode(parentSection, section));
    }

    public override void LoadSectionSettings(TreeNode currentNode)
    {
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
        if (treeNode != null)
          foreach (TreeNode childNode in treeNode.Nodes)
          {
            LoadSectionSettings(childNode);
          }
      }
    }

    public override void SaveSectionSettings(TreeNode currentNode)
    {
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
        if (treeNode != null)
          foreach (TreeNode childNode in treeNode.Nodes)
          {
            SaveSectionSettings(childNode);
          }
      }
    }

    public override void SaveAllSettings()
    {
      foreach (TreeNode treeNode in sectionTree.Nodes)
      {
        //
        // Save settings for all sections
        //
        SaveSectionSettings(treeNode);
      }
    }


    public override void cancelButton_Click(object sender, EventArgs e)
    {
      if (null != _previousSection)
      {
        _previousSection.OnSectionDeActivated();
        _previousSection = null;
      }
      Close();
    }

    public override void okButton_Click(object sender, EventArgs e)
    {
      try
      {
        if (!ServiceHelper.IsStopped || ServiceHelper.IsRestrictedMode)
        {
          applyButton_Click(sender, e);

          if (null != _previousSection)
          {
            _previousSection.OnSectionDeActivated();
            _previousSection = null;
          }
        }
        Close();
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    public override void applyButton_Click(object sender, EventArgs e)
    {
      SaveAllSettings();
    }

    public override void helpToolStripSplitButton_ButtonClick(object sender, EventArgs e)
    {
      HelpSystem.ShowHelp(_previousSection.ToString());
    }

    public override void configToolStripSplitButton_ButtonClick(object sender, EventArgs e)
    {
      Process process = new Process();
      process.StartInfo.FileName = "explorer.exe";
      process.StartInfo.Arguments = String.Format(@"{0}\log\", PathManager.GetDataPath);
      process.StartInfo.UseShellExecute = true;
      process.Start();
    }

    #region Windows Form Designer generated code

    private new void InitializeComponent()
    {
      this.SuspendLayout();
      // 
      // sectionTree
      // 
      this.sectionTree.LineColor = System.Drawing.Color.Black;
      this.sectionTree.Size = new System.Drawing.Size(184, 464);
      // 
      // holderPanel
      // 
      this.holderPanel.Size = new System.Drawing.Size(485, 434);
      // 
      // SetupTvSettingsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(717, 546);
      this.MinimumSize = new System.Drawing.Size(725, 580);
      this.Name = "SetupTvSettingsForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
  }
}