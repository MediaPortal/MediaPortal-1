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
using System.Windows.Forms;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using PluginsSection = Mediaportal.TV.Server.SetupTV.Sections.Plugins;

namespace Mediaportal.TV.Server.SetupTV
{
  public partial class SettingsForm : Form
  {
    private class SectionTreeNode : TreeNode
    {
      public SectionSettings Section
      {
        get { return _section; }
      }

      private readonly SectionSettings _section;

      public SectionTreeNode(SectionSettings section)
      {
        _section = section;
        Name = section.Text;
        Text = section.Text;
      }

      public override string ToString()
      {
        return _section.ToString();
      }
    }

    private SectionSettings _currentSection = null;
    private static IDictionary<string, SectionTreeNode> _sections = new Dictionary<string, SectionTreeNode>(100);
    private readonly PluginLoaderSetupTv _pluginLoader = new PluginLoaderSetupTv();
    private Tuners _sectionTuners = null;
    private Mediaportal.TV.Server.SetupTV.Sections.Plugins _sectionPlugins = null;

    public SettingsForm(bool showAdvancedSettings)
    {
      InitializeComponent();

      try
      {
        // TODO remove this when we don't have bad code doing cross thread calls
        CheckForIllegalCrossThreadCalls = false;

        Text = "MediaPortal - TV Server Configuration";
        linkLabelDonate.Links.Add(0, linkLabelDonate.Text.Length, "http://www.team-mediaportal.com/donate.html");

        // Build the tree on the left hand side.
        AddSection(new Project());
        AddSection(new General());

        _sectionTuners = new Tuners(OnServerConfigurationChanged);
        AddSection(_sectionTuners);
        AddServerTuners(false);

        Channels channels = new Channels("TV Channels", MediaType.Television);
        AddSection(channels);
        AddChildSection(channels, new ChannelMapping("Mapping", MediaType.Television));

        Channels radioChannels = new Channels("Radio Channels", MediaType.Radio);
        AddSection(radioChannels);
        AddChildSection(radioChannels, new ChannelMapping("Mapping ", MediaType.Radio));

        AddSection(new Epg());
        AddSection(new TvRecording(OnServerConfigurationChanged));
        AddSection(new TvTimeshifting(OnServerConfigurationChanged));
        AddSection(new TvSchedules());
        AddSection(new StreamingServer(OnServerConfigurationChanged));
        AddSection(new UserPriorities(OnServerConfigurationChanged));
        AddSection(new TestService());

        _pluginLoader.Load();
        _sectionPlugins = new PluginsSection(OnPluginEnabledOrDisabled, _pluginLoader);
        AddSection(_sectionPlugins);

        foreach (ITvServerPlugin plugin in _pluginLoader.Plugins)
        {
          SectionSettings settings = plugin.Setup;
          if (settings != null)
          {
            bool isActive = ServiceAgents.Instance.SettingServiceAgent.GetValue(PluginsSection.GetPluginEnabledSettingName(plugin), false);
            settings.Text = plugin.Name;
            if (isActive)
            {
              AddChildSection(_sectionPlugins, settings);
            }
          }
        }

        //AddSection(new ImportExport());
        AddSection(new ThirdPartyChecks());
        if (showAdvancedSettings)
        {
          AddSection(new TestChannels());
          AddSection(new DebugOptions());
        }

        sectionTree.SelectedNode = sectionTree.Nodes[0];

        BringToFront();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "settings form: failed to initialise sections");
        MessageBox.Show("Initialisation failed." + Environment.NewLine + SectionSettings.SENTENCE_CHECK_LOG_FILES, SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void AddServerTuners(bool reloaded)
    {
      IList<Tuner> tuners = ServiceAgents.Instance.TunerServiceAgent.ListAllTuners(TunerIncludeRelationEnum.None);
      foreach (Tuner tuner in tuners)
      {
        if (tuner.IsEnabled && ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(tuner.IdTuner))
        {
          if ((tuner.SupportedBroadcastStandards & (int)(BroadcastStandard.MaskAnalog | BroadcastStandard.ExternalInput)) != 0)
          {
            AddChildSection(_sectionTuners, new CardAnalog(string.Format("{0} Analog {1}", tuner.IdTuner, tuner.Name), tuner.IdTuner), 1);
          }
          else if ((tuner.SupportedBroadcastStandards & (int)BroadcastStandard.MaskDvb & (int)BroadcastStandard.MaskTerrestrial) != 0)
          {
            AddChildSection(_sectionTuners, new CardDvbT(string.Format("{0} DVB-T/T2 {1}", tuner.IdTuner, tuner.Name), tuner.IdTuner), 1);
          }
          else if ((tuner.SupportedBroadcastStandards & (int)BroadcastStandard.MaskDvb & (int)BroadcastStandard.MaskCable) != 0)
          {
            AddChildSection(_sectionTuners, new CardDvbC(string.Format("{0} DVB-C {1}", tuner.IdTuner, tuner.Name), tuner.IdTuner), 1);
          }
          else if ((tuner.SupportedBroadcastStandards & (int)BroadcastStandard.MaskSatellite) != 0)
          {
            AddChildSection(_sectionTuners, new CardDvbS(string.Format("{0} Satellite {1}", tuner.IdTuner, tuner.Name), tuner.IdTuner), 1);
          }
          else if ((tuner.SupportedBroadcastStandards & (int)(BroadcastStandard.Atsc | BroadcastStandard.Scte)) != 0)
          {
            AddChildSection(_sectionTuners, new CardAtsc(string.Format("{0} ATSC {1}", tuner.IdTuner, tuner.Name), tuner.IdTuner), 1);
          }
          else if (tuner.SupportedBroadcastStandards == (int)BroadcastStandard.DvbIp)
          {
            AddChildSection(_sectionTuners, new CardDvbIP(string.Format("{0} Stream {1}", tuner.IdTuner, tuner.Name), tuner.IdTuner), 1);
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

    public void AddSection(SectionSettings section, int imageIndex = -1)
    {
      AddChildSection(null, section, imageIndex);
    }

    public void AddChildSection(SectionSettings parentSection, SectionSettings section, int imageIndex = -1)
    {
      SectionTreeNode node = new SectionTreeNode(section);
      if (imageIndex >= 0)
      {
        node.ImageIndex = imageIndex;
        node.SelectedImageIndex = imageIndex;
      }
      if (parentSection == null)
      {
        // Add to the root.
        sectionTree.Nodes.Add(node);
      }
      else
      {
        // Add to the parent node.
        _sections[parentSection.Text].Nodes.Add(node);
      }

      _sections.Add(section.Text, node);
    }

    public void RemoveChildSection(SectionSettings parentSection, SectionSettings section)
    {
      if (parentSection != null)
      {
        SectionTreeNode parentTreeNode = (SectionTreeNode)_sections[parentSection.Text];
        TreeNode nodeToRemove = null;
        foreach (TreeNode node in parentTreeNode.Nodes)
        {
          if (node.Name == section.Text)
          {
            nodeToRemove = node;
            break;
          }
        }
        if (nodeToRemove != null)
        {
          _sections.Remove(section.Text);
          parentTreeNode.Nodes.Remove(nodeToRemove);
        }
      }
    }

    private void RemoveAllChildSections(SectionTreeNode parentTreeNode)
    {
      if (parentTreeNode != null)
      {
        foreach (SectionTreeNode childNode in parentTreeNode.Nodes)
        {
          RemoveAllChildSections(childNode);  // recursive
          _sections.Remove(childNode.Text);
        }

        // Remove all children and sections before the nodes (avoids collection changes during iteration).
        foreach (SectionTreeNode childNode in parentTreeNode.Nodes)
        {
          if (childNode != null)
          {
            parentTreeNode.Nodes.Remove(childNode);
          }
        }
      }
    }

    public void sectionTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
    {
      if (!ServiceHelper.IsAvailable)
      {
        MessageBox.Show("The TV service is not running.", SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
        e.Cancel = true;
      }
    }

    public void sectionTree_AfterSelect(object sender, TreeViewEventArgs e)
    {
      SectionTreeNode treeNode = e.Node as SectionTreeNode;
      if (treeNode == null || treeNode.Section == null)
      {
        return;
      }

      SectionSettings section = treeNode.Section;
      try
      {
        Cursor = Cursors.WaitCursor;
        section.Dock = DockStyle.Fill;

        // Deactive the previous section before activating a new one. Some code
        // (eg. CA menu handling) relies on the order.
        if (section != _currentSection && _currentSection != null)
        {
          _currentSection.SaveSettings();
          _currentSection.OnSectionDeActivated();
        }

        holderPanel.Controls.Clear();
        holderPanel.Controls.Add(section);

        section.OnSectionActivated();
        section.LoadSettings();
        headerLabel.Caption = section.Text;
        _currentSection = section;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "settings form: failed to load section {0}", section.Text);
        MessageBox.Show("Failed to load section." + Environment.NewLine + SectionSettings.SENTENCE_CHECK_LOG_FILES, SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        Cursor = Cursors.Default;
      }
    }

    public void closeButton_Click(object sender, EventArgs e)
    {
      try
      {
        if (ServiceHelper.IsAvailable && _currentSection != null)
        {
          _currentSection.SaveSettings();
          _currentSection.OnSectionDeActivated();
          _currentSection = null;
        }
        Close();
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
    }

    private void linkLabelDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start((string)e.Link.LinkData);
    }

    public void helpToolStripSplitButton_ButtonClick(object sender, EventArgs e)
    {
      HelpSystem.ShowHelp(_currentSection.ToString());
    }

    public void configToolStripSplitButton_ButtonClick(object sender, EventArgs e)
    {
      Process process = new Process();
      process.StartInfo.FileName = "explorer.exe";
      process.StartInfo.Arguments = String.Format(@"{0}\log\", PathManager.GetDataPath);
      process.StartInfo.UseShellExecute = true;
      process.Start();
    }

    public void OnServerConfigurationChanged(object sender, bool reloadConfigController, ICollection<int> reloadConfigTuners)
    {
      if (reloadConfigController)
      {
        this.LogInfo("settings form: reloading controller configuration");
        ServiceAgents.Instance.ControllerServiceAgent.ReloadControllerConfiguration();
      }
      if (reloadConfigTuners != null && reloadConfigTuners.Count > 0)
      {
        this.LogInfo("settings form: reloading configuration for tuners {0}", string.Join(", ", reloadConfigTuners));
        ServiceAgents.Instance.ControllerServiceAgent.ReloadTunerConfiguration(reloadConfigTuners);
      }
    }

    public void OnPluginEnabledOrDisabled(object sender, object pluginObject, bool isEnabled)
    {
      ITvServerPlugin plugin = pluginObject as ITvServerPlugin;
      if (plugin != null)
      {
        SectionSettings settings = plugin.Setup;
        if (settings != null)
        {
          settings.Text = plugin.Name;
          if (isEnabled)
          {
            AddChildSection(_sectionPlugins, settings);
          }
          else
          {
            RemoveChildSection(_sectionPlugins, settings);
          }
        }
      }
      OnServerConfigurationChanged(sender, true, null);
    }
  }
}