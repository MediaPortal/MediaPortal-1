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
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvControl;
using TvDatabase;
using SetupTv.Sections;
using TvEngine;

namespace SetupTv
{
  /// <summary>
  /// Summary description for Settings.
  /// </summary>
  public class SetupTvSettingsForm : SetupControls.SettingsForm
  {
    private readonly PluginLoader _pluginLoader = new PluginLoader();
    private Plugins pluginsRoot;
    private TvBusinessLayer layer;

    public SetupTvSettingsForm()
    {
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
        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\gentle.config", Log.GetPathName()));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
        XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");

        Gentle.Framework.ProviderFactory.ResetGentle(true);
        Gentle.Framework.GentleSettings.DefaultProviderName = nodeProvider.InnerText;
        Gentle.Framework.ProviderFactory.GetDefaultProvider();
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(node.InnerText);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Unable to open:" + String.Format(@"{0}\gentle.config", Log.GetPathName()));
        Log.Write(ex);
      }

      try
      {
        Server.ListAll();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to open database");
        Log.Error("Unable to get list of servers");
        Log.Write(ex);
      }

      Project project = new Project();
      AddSection(project);

      layer = new TvBusinessLayer();
      Servers servers = new Servers();
      AddSection(servers);
      IList dbsServers = Server.ListAll();

      if (dbsServers != null)
      {
        foreach (Server server in dbsServers)
        {
          if (server.IsMaster)
          {
            RemoteControl.HostName = server.HostName;

            if (server.ReferringCard().Count > 0)
            {
              try
              {
                Card c = (Card)server.ReferringCard()[0];
                RemoteControl.Instance.Type(c.IdCard);
              }
              catch
              {
                MessageBox.Show(this, "Unable to connect to " + RemoteControl.HostName);
              }
            }
            break;
          }
        }

        foreach (Server server in dbsServers)
        {
          int cardNo = 1;
          bool isLocal = server.HostName.ToLower() == Dns.GetHostName().ToLower();
          bool DvbCheck = false;
          TvCards cardPage = new TvCards(server.HostName);
          AddChildSection(servers, cardPage, 0);
          foreach (Card dbsCard in server.ReferringCard())
          {
            if (RemoteControl.Instance.CardPresent(dbsCard.IdCard))
            {
              CardType type = RemoteControl.Instance.Type(dbsCard.IdCard);
              string cardName = dbsCard.Name;
              switch (type)
              {
                case CardType.Analog:
                  cardName = String.Format("{0} Analog {1}", cardNo, cardName);
                  AddChildSection(cardPage, new CardAnalog(cardName, dbsCard.IdCard), 1);
                  break;
                case CardType.DvbT:
                  cardName = String.Format("{0} DVB-T {1}", cardNo, cardName);
                  AddChildSection(cardPage, new CardDvbT(cardName, dbsCard.IdCard), 1);
                  DvbCheck = true;
                  break;
                case CardType.DvbC:
                  cardName = String.Format("{0} DVB-C {1}", cardNo, cardName);
                  AddChildSection(cardPage, new CardDvbC(cardName, dbsCard.IdCard), 1);
                  DvbCheck = true;
                  break;
                case CardType.DvbS:
                  cardName = String.Format("{0} DVB-S {1}", cardNo, cardName);
                  AddChildSection(cardPage, new CardDvbS(cardName, dbsCard.IdCard), 1);
                  DvbCheck = true;
                  break;
                case CardType.Atsc:
                  cardName = String.Format("{0} ATSC {1}", cardNo, cardName);
                  AddChildSection(cardPage, new CardAtsc(cardName, dbsCard.IdCard), 1);
                  DvbCheck = true;
                  break;
                case CardType.Unknown:
                  cardName = String.Format("{0} Unknown {1}", cardNo, cardName);
                  AddChildSection(cardPage, new CardAnalog(cardName, dbsCard.IdCard), 1);
                  break;
              }
            }
            cardNo++;
          }
          if (isLocal)
            Utils.CheckPrerequisites(DvbCheck);
        }

        TvChannels tvChannels = new TvChannels();
        AddSection(tvChannels);
        AddChildSection(tvChannels, new TvCombinations());
        AddChildSection(tvChannels, new TvChannelMapping());
        AddChildSection(tvChannels, new TvEpgGrabber());
        AddChildSection(tvChannels, new TvGroups());

        RadioChannels radioChannels = new RadioChannels();
        AddSection(radioChannels);
        AddChildSection(radioChannels, new RadioCombinations("Radio Combinations"));
        AddChildSection(radioChannels, new RadioChannelMapping());
        AddChildSection(radioChannels, new RadioEpgGrabber());
        AddChildSection(radioChannels, new RadioGroups());

        AddSection(new ScanSettings());
        AddSection(new TvRecording());
        AddSection(new TvSchedules());
        AddSection(new StreamingServer());

        AddSection(new TestService("Manual Control"));

        _pluginLoader.Load();
        pluginsRoot = new Plugins("Plugins", _pluginLoader);
        AddSection(pluginsRoot);

        pluginsRoot.ChangedActivePlugins += SectChanged;

        foreach (ITvServerPlugin plugin in _pluginLoader.Plugins)
        {
          SectionSettings settings = plugin.Setup;
          if (settings != null)
          {
            Setting isActive = layer.GetSetting(String.Format("plugin{0}", plugin.Name), "false");
            settings.Text = plugin.Name;
            if (isActive.Value == "true")
            {
              AddChildSection(pluginsRoot, settings);
            }
          }
        }
        sectionTree.SelectedNode = sectionTree.Nodes[0];
        // make sure window is in front of mediaportal
      }
      BringToFront();
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
          Setting isActive = layer.GetSetting(((Setting)sender).Tag, "false");
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
        if (!ServiceHelper.IsRunning)
        {
          MessageBox.Show("TvService not started.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          e.Cancel = true;
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
          RemoteControl.Instance.EpgGrabberEnabled = false;
        }
        catch (Exception)
        {
        }
        //DatabaseManager.Instance.SaveChanges();
        //DatabaseManager.Instance.ClearQueryCache();
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


    public override void SettingsForm_Closed(object sender, EventArgs e)
    {
      try
      {
        if (RemoteControl.IsConnected)
        {
          RemoteControl.Instance.EpgGrabberEnabled = true;
          RemoteControl.Instance.OnNewSchedule();
        }
      }
      catch (Exception)
      { }

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
        applyButton_Click(sender, e);
        if (null != _previousSection)
        {
          _previousSection.OnSectionDeActivated();
          _previousSection = null;
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

    public override void updateHelpToolStripMenuItem_Click(object sender, EventArgs e)
    {
      HelpSystem.UpdateHelpReferences();
    }

    public override void configToolStripSplitButton_ButtonClick(object sender, EventArgs e)
    {
      Process process = new Process();
      process.StartInfo.FileName = "explorer.exe";
      process.StartInfo.Arguments = String.Format(@"{0}\log\", Log.GetPathName());
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
      this.Name = "SetupTvSettingsForm";
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion
  }
}