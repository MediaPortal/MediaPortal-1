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
using System.Windows.Forms;
using System.Xml;
using System.Net;
using Mediaportal.TV.Server.Plugins.Base;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV
{
  /// <summary>
  /// Summary description for Settings.
  /// </summary>
  public class SetupTvSettingsForm : SettingsForm
  {
    private readonly PluginLoaderSetupTv _pluginLoader = new PluginLoaderSetupTv();
    private Sections.Plugins pluginsRoot;
    private Servers servers;
    private TvCards cardPage;
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
        Log.Error("Failed to startup cause of exception");
        Log.Write(ex);
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

      try
      {
        ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to open WCF connection to server");
        Log.Error("Unable to get list of servers");
        Log.Write(ex);
      }

      Project project = new Project();
      AddSection(project);
      
      servers = new Servers();
      AddSection(servers);

      IList<Card> cards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
      
      bool connected = false;
      while (!connected)
      {
        RemoteControl.HostName = ServiceAgents.Instance.SettingServiceAgent.GetSetting("hostname").Value;

        if (cards.Count > 0)
        {
          try
          {
            Card c = cards.First();
            ServiceAgents.Instance.ControllerServiceAgent.Type(c.IdCard);
            connected = true;
          }
          catch (Exception ex)
          {
            string localHostname = Dns.GetHostName();
            if (localHostname != RemoteControl.HostName)
            {
              DialogResult dlg = MessageBox.Show(String.Format("Unable to connect to <{0}>.\n" +
                                                                "Do you want to try the current computer name ({1}) instead?",
                                                                RemoteControl.HostName, localHostname),
                                                  "Wrong config detected",
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
              if (dlg == DialogResult.Yes)
              {
                Log.Info("Controller: server {0} changed to {1}", RemoteControl.HostName, localHostname);                      
                ServiceAgents.Instance.SettingServiceAgent.SaveSetting("hostname", localHostname);
                if (!ServiceHelper.IsRestrictedMode)
                {
                  ServiceHelper.Restart();
                  ServiceHelper.WaitInitialized(); 
                }                
              }
              else
              {
                MessageBox.Show("Setup will now close");
                Environment.Exit(-1);
              }
            }
            else
            {
              Log.Error("Cannot connect to server {0}", RemoteControl.HostName);
              Log.Write(ex);
              DialogResult dlg = MessageBox.Show("Unable to connect to <" + RemoteControl.HostName + ">.\n" +
                                                  "Please check the TV Server logs for details.\n\n" +
                                                  "Setup will now close.");
              Environment.Exit(-1);
            }
          }
        }
        
        AddServerTvCards(RemoteControl.HostName, false);

        Channels channels = new Channels("TV Channels", MediaTypeEnum.TV);            
        AddSection(channels);
        AddChildSection(channels, new ChannelCombinations("TV Combinations", MediaTypeEnum.TV));
        AddChildSection(channels, new ChannelMapping("TV Mapping", MediaTypeEnum.TV));

        Channels radioChannels = new Channels("Radio Channels", MediaTypeEnum.Radio);        
        AddSection(radioChannels);
        AddChildSection(radioChannels, new ChannelCombinations("Radio Combinations", MediaTypeEnum.Radio));
        AddChildSection(radioChannels, new ChannelMapping("Radio Mapping", MediaTypeEnum.Radio));

        Epg EpgSection = new Epg();
        AddSection(EpgSection);
        AddChildSection(EpgSection, new EpgGrabber("TV Epg grabber", MediaTypeEnum.TV));
        AddChildSection(EpgSection, new EpgGrabber("Radio Epg grabber", MediaTypeEnum.Radio)); ;

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
            Setting isActive = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(String.Format("plugin{0}", plugin.Name), "false");
            settings.Text = plugin.Name;
            if (isActive.Value == "true")
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
      }
      BringToFront();
    }

    private void AddServerTvCards(string hostName,  bool reloaded)
    {
      //foreach (TVDatabase.Gentle.Server server in dbsServers)
      {
        bool isLocal = (hostName.ToLowerInvariant() == Dns.GetHostName().ToLowerInvariant() ||
                        hostName.ToLowerInvariant() == Dns.GetHostName().ToLowerInvariant() + "."
                        +
                        System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName.
                          ToLowerInvariant());
        cardPage = new TvCards(hostName);
        cardPage.TvCardsChanged += OnTvCardsChanged;
        AddChildSection(servers, cardPage, 0);
        IList<Card> cards = ServiceAgents.Instance.CardServiceAgent.ListAllCards(CardIncludeRelationEnum.None);
        foreach (Card dbsCard in cards)
        {
          if (dbsCard.Enabled && ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(dbsCard.IdCard))
          {
            CardType type = ServiceAgents.Instance.ControllerServiceAgent.Type(dbsCard.IdCard);
            int cardId = dbsCard.IdCard;
            string cardName = dbsCard.Name;
            switch (type)
            {
              case CardType.Analog:
                cardName = String.Format("{0} Analog {1}", cardId, cardName);
                AddChildSection(cardPage, new CardAnalog(cardName, dbsCard.IdCard), 1);
                break;
              case CardType.DvbT:
                cardName = String.Format("{0} DVB-T {1}", cardId, cardName);
                AddChildSection(cardPage, new CardDvbT(cardName, dbsCard.IdCard), 1);
                break;
              case CardType.DvbC:
                cardName = String.Format("{0} DVB-C {1}", cardId, cardName);
                AddChildSection(cardPage, new CardDvbC(cardName, dbsCard.IdCard), 1);
                break;
              case CardType.DvbS:
                cardName = String.Format("{0} DVB-S {1}", cardId, cardName);
                AddChildSection(cardPage, new CardDvbS(cardName, dbsCard.IdCard), 1);
                break;
              case CardType.Atsc:
                cardName = String.Format("{0} ATSC {1}", cardId, cardName);
                AddChildSection(cardPage, new CardAtsc(cardName, dbsCard.IdCard), 1);
                break;
              case CardType.DvbIP:
                cardName = String.Format("{0} DVB-IP {1}", cardId, cardName);
                AddChildSection(cardPage, new CardDvbIP(cardName, dbsCard.IdCard), 1);
                break;
              case CardType.RadioWebStream:
                cardName = String.Format("{0} {1}", cardId, cardName);
                InfoPage RadioWebStreamInfo = new InfoPage(cardName);
                RadioWebStreamInfo.InfoText =
                  "The RadioWebStream card does not have any options.\n\n\nYou can add your favourite radio webstreams under:\n\n --> 'Radio Channels', 'Add', 'Web-Stream' or by importing a playlist.";
                AddChildSection(cardPage, RadioWebStreamInfo, 1);
                break;
              case CardType.Unknown:
                cardName = String.Format("{0} Unknown {1}", cardId, cardName);
                AddChildSection(cardPage, new CardAnalog(cardName, dbsCard.IdCard), 1);
                break;
            }
          }
        }
        if (isLocal)
        {
          Utils.CheckForDvbHotfix();
        }
        if (reloaded)
        {
          SectionTreeNode activeNode = (SectionTreeNode)settingSections[hostName];
          if (activeNode != null)
          {
            activeNode.Expand();
          }
        }
      }
    }

    /// <summary>
    /// called when tvcards were changed (add, remove, enable, disable)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnTvCardsChanged(object sender, EventArgs e)
    {
      bool isAnyUserTS;
      bool isRec;
      bool isUserTS;
      bool isRecOrTS = ServiceAgents.Instance.ControllerServiceAgent.IsAnyCardRecordingOrTimeshifting(new User().Name, out isUserTS, out isAnyUserTS,
                                                                               out isRec);

      if (!isAnyUserTS && !isRec && !isRecOrTS && !isUserTS)
      {
        NotifyForm dlgNotify = new NotifyForm("Restart TvService...", "This can take some time\n\nPlease be patient...");
        try
        {
          dlgNotify.Show();
          dlgNotify.WaitForDisplay();

          ServiceAgents.Instance.ControllerServiceAgent.Restart();

          // remove all tv servers / cards, add current ones back later
          RemoveAllChildSections((SectionTreeNode)settingSections[servers.Text]);

          // re-add tvservers and cards to tree          
          AddServerTvCards(ServiceAgents.Instance.SettingServiceAgent.GetSetting("hostname").Value, true);
        }
        finally
        {
          dlgNotify.Close();
        }
      }
      else
      {
        MessageBox.Show(this,
                        "In order to apply new settings - please restart tvservice manually when done timeshifting / recording.");
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
          settingSections.Remove(childNode.Text);
        }
        // first remove all children and sections, then nodes themself (otherwise collection changes during iterate)
        parentTreeNode.Nodes.Clear();       
      }
    }

    public void RemoveAllChildSections(SectionSettings parentSection)
    {
      // Remove section from tree
      if (parentSection != null)
      {
        SectionTreeNode parentTreeNode = (SectionTreeNode)settingSections[parentSection.Text];
        foreach (SectionTreeNode childNode in parentTreeNode.Nodes)
        {
          // recursive delete all children
          RemoveAllChildSections(childNode);

          //Remove the section from the hashtable in case we add it again
          settingSections.Remove(childNode.Text);
          parentTreeNode.Nodes.Remove(childNode);
        }
      }
    }

    public void RemoveChildSection(SectionSettings parentSection, SectionSettings section)
    {
      // Remove section from tree
      if (parentSection != null)
      {
        SectionTreeNode parentTreeNode = (SectionTreeNode)settingSections[parentSection.Text];

        for (int i = 0; i < parentTreeNode.GetNodeCount(true); i++)
        {
          if (parentTreeNode.Nodes[i].Name == section.Text)
          {
            //Remove the section from the hashtable in case we add it again
            settingSections.Remove(section.Text);
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
        SectionTreeNode parentTreeNode = (SectionTreeNode)settingSections[parentSection.Text];

        treeNode.ImageIndex = imageIndex;
        treeNode.SelectedImageIndex = imageIndex;
        parentTreeNode.Nodes.Add(treeNode);
      }

      settingSections.Add(section.Text, treeNode);

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
      string name = ((Setting)sender).Tag.Substring(6);

      foreach (ITvServerPlugin plugin in _pluginLoader.Plugins)
      {
        SectionSettings settings = plugin.Setup;
        if (settings != null && plugin.Name == name)
        {
          Setting isActive = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(((Setting)sender).Tag, "false");
          settings.Text = name;

          if (isActive.Value == "true")
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
        SectionTreeNode parentTreeNode = (SectionTreeNode)settingSections[parentSection.Text];
        treeNode.Name = section.Text;
        parentTreeNode.Nodes.Add(treeNode);
      }

      settingSections.Add(section.Text, treeNode);

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

        section.OnSectionActivated();
        if (section != _previousSection && _previousSection != null)
        {
          _previousSection.OnSectionDeActivated();
        }
        _previousSection = section;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
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
      SectionTreeNode parentTreeNode = (SectionTreeNode)settingSections[parentSection.Text];

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
        if (!ServiceHelper.IsStopped)
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
        Log.Write(ex);
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
      // 
      // SetupTvSettingsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(716, 537);
      this.MinimumSize = new System.Drawing.Size(724, 571);
      this.Name = "SetupTvSettingsForm";
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion
  }
}