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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.TV.Database;
using System.Collections;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace ProcessPlugins.TvMovie
{
  public partial class TvMovieSettings : Form
  {

    #region Membervariables

    string _xmlFile; 
    #endregion

    class ChannelInfo
    {
      string _start = "00:00";
      string _end = "00:00";
      string _name = string.Empty;

      public string Start
      {
        get { return _start; }
        set { _start = value; }
      }

      public string End
      {
        get { return _end; }
        set { _end = value; }
      }

      public string Name
      {
        get { return _name; }
        set { _name = value; }
      }

      public ChannelInfo()
      {
        _start = "00:00";
        _end = "00:00";
      }
    }

    #region Form Methods


    /// <summary>
    /// Constructor
    /// </summary>
    public TvMovieSettings()
    {
      _xmlFile = Config.GetFile(Config.Dir.Config, "TVMovieMapping.xml");

      InitializeComponent();
      LoadStations();
      LoadMapping();
      LoadOptions();
      //this.mpTabControl1.Controls.Remove(this.tabPage2);
    }


    private void buttonMap_Click(object sender, EventArgs e)
    {
      MapStation();
    }


    private void listBoxTvMovieChannels_DoubleClick(object sender, EventArgs e)
    {
      MapStation();
    }


    private void treeViewStations_DoubleClick(object sender, EventArgs e)
    {
      UnmapStation();
    }


    private void buttonOk_Click(object sender, EventArgs e)
    {
      SaveMapping();
      SaveOptions();
      this.Close();
    }


    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }


    private void maskedTextBoxTimeStart_Validated(object sender, EventArgs e)
    {
      ChannelInfo channelInfo = (ChannelInfo)treeViewChannels.SelectedNode.Tag;
      channelInfo.Start = CleanInput(maskedTextBoxTimeStart.Text);
      maskedTextBoxTimeStart.Text = CleanInput(maskedTextBoxTimeStart.Text);
      treeViewChannels.SelectedNode.Tag = channelInfo;
      if (channelInfo.Start != "00:00" || channelInfo.End != "00:00")
        treeViewChannels.SelectedNode.Text = string.Format("{0} ({1}-{2})", channelInfo.Name, channelInfo.Start, channelInfo.End);
      else
        treeViewChannels.SelectedNode.Text = string.Format("{0}", channelInfo.Name);
    }


    private void maskedTextBoxTimeEnd_Validated(object sender, EventArgs e)
    {
      ChannelInfo channelInfo = (ChannelInfo)treeViewChannels.SelectedNode.Tag;
      channelInfo.End = CleanInput(maskedTextBoxTimeEnd.Text);
      maskedTextBoxTimeEnd.Text = CleanInput(maskedTextBoxTimeEnd.Text);
      treeViewChannels.SelectedNode.Tag = channelInfo;
      if (channelInfo.Start != "00:00" || channelInfo.End != "00:00")
        treeViewChannels.SelectedNode.Text = string.Format("{0} ({1}-{2})", channelInfo.Name, channelInfo.Start, channelInfo.End);
      else
        treeViewChannels.SelectedNode.Text = string.Format("{0}", channelInfo.Name);
    }


    private void checkBoxUseShortDesc_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxUseShortDesc.Checked)
      {
        checkBoxAdditionalInfo.Checked = false;
        checkBoxAdditionalInfo.Enabled = false;
      }
      checkBoxAdditionalInfo.Enabled = true;
    }


    private void checkBoxAdditionalInfo_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBoxAdditionalInfo.Checked)
        checkBoxUseShortDesc.Checked = false;
    }


    private void treeViewChannels_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (e.Node.Parent == null || e.Node.Tag == null)
      {
        panelTimeSpan.Visible = false;
        return;
      }
      panelTimeSpan.Visible = true;
      ChannelInfo channelInfo = (ChannelInfo)e.Node.Tag;
      maskedTextBoxTimeStart.Text = channelInfo.Start;
      maskedTextBoxTimeEnd.Text = channelInfo.End;
    }


    #endregion

    #region Implementation


    private void LoadOptions()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        checkBoxUseShortDesc.Checked = xmlreader.GetValueAsBool("tvmovie", "shortprogramdesc", false);
        checkBoxAdditionalInfo.Checked = xmlreader.GetValueAsBool("tvmovie", "extenddescription", false);
        checkBoxShowAudioFormat.Checked = xmlreader.GetValueAsBool("tvmovie", "showaudioformat", false);
        checkBoxSlowImport.Checked = xmlreader.GetValueAsBool("tvmovie", "slowimport", false);
        checkBoxImportSchedules.Checked = xmlreader.GetValueAsBool("tvmovie", "importschedules", true);
      }
    }


    private void SaveOptions()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("tvmovie", "shortprogramdesc", checkBoxUseShortDesc.Checked);
        xmlwriter.SetValueAsBool("tvmovie", "extenddescription", checkBoxAdditionalInfo.Checked);
        xmlwriter.SetValueAsBool("tvmovie", "showaudioformat", checkBoxShowAudioFormat.Checked);
        xmlwriter.SetValueAsBool("tvmovie", "slowimport", checkBoxSlowImport.Checked);
        xmlwriter.SetValueAsBool("tvmovie", "importschedules", checkBoxImportSchedules.Checked);
      }
    }


    /// <summary>
    /// Load stations from databases and fill controls with that data
    /// </summary>
    private void LoadStations()
    {
      try
      {
        TvMovieDatabase database = new TvMovieDatabase();

        treeViewStations.BeginUpdate();

        foreach (string station in database.Stations)
        {
          TreeNode stationNode = new TreeNode(station);
          ChannelInfo channelInfo = new ChannelInfo();
          channelInfo.Name = station;
          stationNode.Tag = channelInfo;
          treeViewStations.Nodes.Add(stationNode);
        }

        treeViewStations.EndUpdate();

        treeViewChannels.BeginUpdate();

        ArrayList mpChannelList = new ArrayList();
        TVDatabase.GetChannels(ref mpChannelList);

        foreach (TVChannel channel in mpChannelList)
        {
          TreeNode stationNode = new TreeNode(channel.Name);
          treeViewChannels.Nodes.Add(stationNode);
        }

        treeViewChannels.EndUpdate();
      }
      catch (Exception)
      {
        MessageBox.Show("Please make sure TV Movie Clickfinder has been installed and licensed locally.", "Error loading TV Movie database", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }


    /// <summary>
    /// Map selected TVMovie station to a selected MP channel
    /// </summary>
    private void MapStation()
    {
      TreeNode selectedChannel = treeViewChannels.SelectedNode;
      if (selectedChannel == null)
        return;
      while (selectedChannel.Parent != null)
        selectedChannel = selectedChannel.Parent;

      TreeNode selectedStation = (TreeNode)treeViewStations.SelectedNode.Clone();

      foreach (TreeNode stationNode in selectedChannel.Nodes)
        if (stationNode.Text == selectedStation.Text)
          return;

      if (selectedChannel.Nodes.Count > 0)
      {
        selectedChannel.Nodes[0].ForeColor = Color.Green;
        selectedStation.ForeColor = Color.Green;
      }
      else
        selectedStation.ForeColor = Color.Blue;

      selectedChannel.Nodes.Add(selectedStation);
      selectedChannel.Expand();
    }


    /// <summary>
    /// Remove TVMovie station mapping from selected MP channel
    /// </summary>
    private void UnmapStation()
    {
      TreeNode selectedChannel = treeViewChannels.SelectedNode;
      if (selectedChannel == null)
        return;
      if (selectedChannel.Parent != null)
      {
        if (selectedChannel.Parent.Nodes.Count == 2)
        {
          selectedChannel.Parent.Nodes[0].ForeColor = Color.Blue;
          selectedChannel.Parent.Nodes[1].ForeColor = Color.Blue;
        }
        selectedChannel.Remove();
      }
      else
        selectedChannel.Nodes.Clear();
    }


    /// <summary>
    /// Save station-channel mapping to XML
    /// </summary>
    private void SaveMapping()
    {
      XmlTextWriter writer = new XmlTextWriter(_xmlFile, System.Text.Encoding.UTF8);
      writer.Formatting = Formatting.Indented;
      writer.Indentation = 1;
      writer.IndentChar = (char)9;
      writer.WriteStartDocument(true);
      writer.WriteStartElement("channellist"); // <channellist>
      writer.WriteAttributeString("version", "1");

      foreach (TreeNode channel in treeViewChannels.Nodes)
      {
        writer.WriteStartElement("channel"); // <channel>
        writer.WriteAttributeString("name", channel.Text);
        foreach (TreeNode station in channel.Nodes)
        {
          ChannelInfo channelInfo = (ChannelInfo)station.Tag;
          writer.WriteStartElement("station"); // <station>
          writer.WriteAttributeString("name", channelInfo.Name);
          writer.WriteStartElement("timesharing"); // <timesharing>
          writer.WriteAttributeString("start", channelInfo.Start);
          writer.WriteAttributeString("end", channelInfo.End);
          writer.WriteEndElement(); // </timesharing>
          writer.WriteEndElement(); // </station>
        }
        writer.WriteEndElement(); // </channel>
      }

      writer.WriteEndElement(); // </channellist>
      writer.WriteEndDocument();
      writer.Close();
    }


    /// <summary>
    /// Load station-channel mapping from XML
    /// </summary>
    private void LoadMapping()
    {
      if (!File.Exists(_xmlFile))
      {
        Log.Info("TVMovie: Mapping file \"{0}\" does not exist, using empty list", _xmlFile);
        return;
      }

      treeViewChannels.BeginUpdate();

      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(_xmlFile);
        XmlNodeList listChannels = doc.DocumentElement.SelectNodes("//channellist/channel");
        foreach (XmlNode channel in listChannels)
        {
          string channelName = (string)channel.Attributes["name"].Value;
          TreeNode channelNode = FindChannel(channelName);
          if (channelNode != null)
          {
            XmlNodeList listStations = channel.SelectNodes("station");

            foreach (XmlNode station in listStations)
            {
              string stationName = (string)station.Attributes["name"].Value;
              if (FindStation(stationName) != null)
              {
                TreeNode stationNode = (TreeNode)FindStation(stationName).Clone();
                ChannelInfo channelInfo = new ChannelInfo();
                if (stationNode != null)
                {
                  XmlNode timesharing = station.SelectSingleNode("timesharing");
                  string start = "00:00";
                  string end = "00:00";
                  if (timesharing != null)
                  {
                    start = timesharing.Attributes["start"].Value;
                    end = timesharing.Attributes["end"].Value;
                  }

                  if (start != "00:00" || end != "00:00")
                    stationNode.Text = string.Format("{0} ({1}-{2})", stationName, start, end);
                  else
                    stationNode.Text = string.Format("{0}", stationName);

                  channelInfo.Start = start;
                  channelInfo.End = end;
                  channelInfo.Name = stationName;

                  stationNode.Tag = channelInfo;

                  if (listStations.Count > 1)
                    stationNode.ForeColor = Color.Green;
                  else
                    stationNode.ForeColor = Color.Blue;
                  channelNode.Nodes.Add(stationNode);
                  channelNode.Expand();
                }
              }
              else
                Log.Warn("TVMovie plugin: Channel {0} no longer present in Database - ignoring", stationName);
            }
          }
        }
      }
      catch (System.Xml.XmlException ex)
      {
        Log.Info("TVMovie: The mapping file \"{0}\" seems to be corrupt", _xmlFile);
        Log.Info("TVMovie: {0}", ex.Message);
      }
      treeViewChannels.EndUpdate();
    }


    private TreeNode FindChannel(string channelName)
    {
      foreach (TreeNode channel in treeViewChannels.Nodes)
        if (channel.Text == channelName)
          return channel;

      return null;
    }


    private TreeNode FindStation(string stationName)
    {
      foreach (TreeNode station in treeViewStations.Nodes)
        if (station.Tag != null)
          if (((ChannelInfo)station.Tag).Name == stationName)
            return station;

      return null;
    }


    private void ColorNode(TreeNode channelNode, Color color)
    {
      foreach (TreeNode stationNode in channelNode.Nodes)
      {
        stationNode.ForeColor = color;
      }
    }


    string CleanInput(string input)
    {
      int hours = 0;
      int minutes = 0;
      input = input.Trim();
      int index = input.IndexOf(':');
      if (index > 0)
        hours = Convert.ToInt16(input.Substring(0, index));
      if (index + 1 < input.Length)
        minutes = Convert.ToInt16(input.Substring(index + 1));

      if (hours > 23)
        hours = 0;

      if (minutes > 59)
        minutes = 0;

      return string.Format("{0:00}:{1:00}", hours, minutes);
    }


    #endregion

  }
}