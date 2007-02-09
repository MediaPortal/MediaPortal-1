#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using TvDatabase;
using TvEngine;
using TvLibrary.Log;


namespace SetupTv.Sections
{
  public partial class TvMovieSetup : SectionSettings
  {
    #region Membervariables

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


    private void treeViewStations_DoubleClick(object sender, EventArgs e)
    {
      MapStation();
    }


    private void treeViewChannels_DoubleClick(object sender, EventArgs e)
    {
      UnmapStation();
    }


    #endregion

    public TvMovieSetup()
      : this("TV Movie Clickfinder EPG import")
    {
    }

    public TvMovieSetup(string name)
      : base(name)
    {
      InitializeComponent();
    }

    public override void OnSectionDeActivated()
    {
      if (tabControlTvMovie.SelectedIndex == 1)
        SaveMapping();

      TvBusinessLayer layer = new TvBusinessLayer();

      Setting setting = layer.GetSetting("TvMovieEnabled", "false");
      if (checkBoxEnableImport.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      setting = layer.GetSetting("TvMovieUseDatabaseDate", "true");
//      if (checkBoxUseDatabaseDate.Checked)
        setting.Value = "true";
      //else
      //  setting.Value = "false";
      setting.Persist();

      setting = layer.GetSetting("TvMovieShortProgramDesc", "false");
      if (checkBoxUseShortDesc.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      setting = layer.GetSetting("TvMovieExtendDescription", "false");
      if (checkBoxAdditionalInfo.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      setting = layer.GetSetting("TvMovieShowAudioFormat", "false");
      if (checkBoxShowAudioFormat.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      setting = layer.GetSetting("TvMovieSlowImport", "true");
      if (checkBoxSlowImport.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      setting = layer.GetSetting("TvMovieRestPeriod", "24");
      setting.Value = GetRestPeriod();
      setting.Persist();

      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      checkBoxEnableImport.Checked = layer.GetSetting("TvMovieEnabled", "false").Value == "true";
      //checkBoxUseDatabaseDate.Checked = layer.GetSetting("TvMovieUseDatabaseDate", "true").Value == "true";
      checkBoxUseShortDesc.Checked = layer.GetSetting("TvMovieShortProgramDesc", "false").Value == "true";
      checkBoxAdditionalInfo.Checked = layer.GetSetting("TvMovieExtendDescription", "false").Value == "true";
      checkBoxShowAudioFormat.Checked = layer.GetSetting("TvMovieShowAudioFormat", "false").Value == "true";
      checkBoxSlowImport.Checked = layer.GetSetting("TvMovieSlowImport", "false").Value == "true";
      SetRestPeriod(layer.GetSetting("TvMovieRestPeriod", "24").Value);

      base.OnSectionActivated();
    }


    /// <summary>
    /// Load stations from databases and fill controls with that data
    /// </summary>
    private void LoadStations()
    {
      TvMovieDatabase database = new TvMovieDatabase();
      database.Connect();

      treeViewStations.BeginUpdate();
      treeViewStations.Nodes.Clear();

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
      treeViewChannels.Nodes.Clear();

      ArrayList mpChannelList = database.GetChannels();

      foreach (Channel channel in mpChannelList)
      {
        TreeNode stationNode = new TreeNode(channel.Name);
        treeViewChannels.Nodes.Add(stationNode);
      }

      treeViewChannels.EndUpdate();
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

      //TvBusinessLayer layer = new TvBusinessLayer();
      //Setting setting = layer.GetSetting("TvMovieLastUpdate");
      //setting.Value = "0";
      //setting.Persist();
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
    /// Save station-channel mapping to database
    /// </summary>
    private void SaveMapping()
    {
      IList mappingList = TvMovieMapping.ListAll();

      if (mappingList != null && mappingList.Count > 0)
      {
        foreach (TvMovieMapping mapping in mappingList)
          mapping.Remove();
      }
      else
        Log.Info("TvMovieSetup: SaveMapping - no mappingList items");

      TvBusinessLayer layer = new TvBusinessLayer();

      foreach (TreeNode channel in treeViewChannels.Nodes)
      {
        foreach (TreeNode station in channel.Nodes)
        {
          ChannelInfo channelInfo = (ChannelInfo)station.Tag;
          TvMovieMapping mapping = new TvMovieMapping(layer.GetChannelByName(channel.Text).IdChannel,
            channelInfo.Name, channelInfo.Start, channelInfo.End);
          //Log.Write("TvMovieSetup: SaveMapping - new mapping for {0}/{1}", channel.Text, channelInfo.Name);
          try
          {
            mapping.Persist();
          }
          catch (Exception ex)
          {
            Log.Error("TvMovieSetup: Error on mapping.Persist() {0},{1}", ex.Message, ex.StackTrace);
          }
        }
      }
    }


    /// <summary>
    /// Load station-channel mapping from database
    /// </summary>
    private void LoadMapping()
    {
      treeViewChannels.BeginUpdate();
      foreach (TreeNode treeNode in treeViewChannels.Nodes)
      {
        foreach (TreeNode childNode in treeNode.Nodes)
          childNode.Remove();
      }

      try
      {
        IList mappingDb = TvMovieMapping.ListAll();
        if (mappingDb != null && mappingDb.Count > 0)
        {
          foreach (TvMovieMapping mapping in mappingDb)
          {
            string channelName = Channel.Retrieve(mapping.IdChannel).Name;
            TreeNode channelNode = FindChannel(channelName);
            if (channelNode != null)
            {
              string stationName = mapping.StationName;
              if (FindStation(stationName) != null)
              {
                TreeNode stationNode = (TreeNode)FindStation(stationName).Clone();
                ChannelInfo channelInfo = new ChannelInfo();
                if (stationNode != null)
                {
                  string start = mapping.TimeSharingStart;
                  string end = mapping.TimeSharingEnd;

                  if (start != "00:00" || end != "00:00")
                    stationNode.Text = string.Format("{0} ({1}-{2})", stationName, start, end);
                  else
                    stationNode.Text = string.Format("{0}", stationName);

                  channelInfo.Start = start;
                  channelInfo.End = end;
                  channelInfo.Name = stationName;

                  stationNode.Tag = channelInfo;

                  channelNode.Nodes.Add(stationNode);
                  channelNode.Expand();
                }
              }
              else
                Log.Debug("TVMovie plugin: Channel {0} no longer present in Database - ignoring", stationName);
            }
          }
        }
        else
          Log.Debug("TVMovie plugin: LoadMapping failed - no TV channels found!");
      }
      catch (Exception ex)
      {
        Log.Debug("TVMovie plugin: LoadMapping failed - {0},{1}", ex.Message, ex.StackTrace);
      }
      ColorTree();
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


    private void ColorTree()
    {
      foreach (TreeNode parentNode in treeViewChannels.Nodes)
        foreach (TreeNode subNode in parentNode.Nodes)
        {
          if (parentNode.Nodes.Count > 1)
            subNode.ForeColor = Color.Green;
          else
            subNode.ForeColor = Color.Blue;
        }
    }


    private void ColorNode(TreeNode channelNode, Color color)
    {
      foreach (TreeNode stationNode in channelNode.Nodes)
      {
        stationNode.ForeColor = color;
      }
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

    private void linkLabelInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://www.tvmovie.de/ClickFinder.57.0.html");
    }

    private string GetRestPeriod()
    {
      if (radioButton6h.Checked)
        return "6"; else
      if (radioButton12h.Checked)
        return "12"; else
      if (radioButton24h.Checked)
        return "24"; else
      if (radioButton2d.Checked)
        return "48"; else
      if (radioButton7d.Checked)
        return "168";

      return "24";
    }

    private void SetRestPeriod(string RadioButtonSetting)
    {
      switch (RadioButtonSetting)
      {
        case "6":  radioButton6h.Checked = true;  break;
        case "12": radioButton12h.Checked = true; break;
        case "24": radioButton24h.Checked = true; break;
        case "48": radioButton2d.Checked = true;  break;
        case "168":radioButton7d.Checked = true;  break;
        default:   radioButton24h.Checked = true; break;
      }
    }

    private void tabControlTvMovie_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!checkBoxEnableImport.Checked)
        tabControlTvMovie.SelectedIndex = 0;
      else if (tabControlTvMovie.SelectedIndex == 0)
        SaveMapping();
    }

    private void checkBoxEnableImport_CheckedChanged(object sender, EventArgs e)
    {
      groupBoxDescriptions.Enabled = groupBoxImportTime.Enabled = checkBoxEnableImport.Checked;

      if (checkBoxEnableImport.Checked)
      {
        try
        {
          LoadStations();
        }
        catch (Exception ex1)
        {
          MessageBox.Show(this, "Please make sure a supported TV Movie Clickfinder release has been successfully installed.", "Error loading TV Movie stations", MessageBoxButtons.OK, MessageBoxIcon.Error);
          checkBoxEnableImport.Checked = false;
          Log.Debug("TVMovie plugin: Error enabling TV Movie import in LoadStations() - {0},{1}", ex1.Message, ex1.StackTrace);
          return;
        }

        //try
        //{
        //  TvMovieSql.CheckDatabase();
        //}
        //catch (Exception)
        //{
        //  MessageBox.Show(this, "Please make sure TV Movie Clickfinder has been installed and licensed locally.", "Error loading TV Movie database", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //  checkBoxEnableImport.Checked = false;
        //  return;
        //}

        try
        {
          LoadMapping();
        }
        catch (Exception ex2)
        {
          MessageBox.Show(this, "Please make sure your using a valid channel mapping.", "Error loading TVM <-> MP channel mapping", MessageBoxButtons.OK, MessageBoxIcon.Error);
          checkBoxEnableImport.Checked = false;
          Log.Debug("TVMovie plugin: Error enabling TV Movie import in LoadMapping() - {0},{1}", ex2.Message, ex2.StackTrace);
          return;
        }
      }
      else
        SaveMapping();
    }

    private void buttonImportNow_Click(object sender, EventArgs e)
    {
      buttonImportNow.Enabled = false;
      try
      {
        Thread manualThread = new Thread(new ThreadStart(ManualImportThread));
        manualThread.Priority = ThreadPriority.Normal;
        manualThread.IsBackground = false;
        manualThread.Start();
      }
      catch (Exception ex2)
      {
        Log.Error("TVMovie: Error spawing import thread - {0},{1}", ex2.Message, ex2.StackTrace);
        buttonImportNow.Enabled = true;
      }
    }

    private void ManualImportThread()
    {
      TvMovieDatabase _database = new TvMovieDatabase();
      try
      {
        _database.OnProgramsChanged += new TvMovieDatabase.ProgramsChanged(_database_OnProgramsChanged);
        _database.OnStationsChanged += new TvMovieDatabase.StationsChanged(_database_OnStationsChanged);
        _database.Connect();
        _database.Import();
        buttonImportNow.Enabled = true;
      }
      catch (Exception ex)
      {
        Log.Error("TvMovie plugin error:");
        Log.Write(ex);
      }
    }

    void _database_OnStationsChanged(int value, int maximum, string text)
    {
      progressBarImportTotal.Maximum = maximum;
      progressBarImportTotal.Value = value;
    }

    void _database_OnProgramsChanged(int value, int maximum, string text)
    {
      progressBarImportItem.Maximum = maximum;
      progressBarImportItem.Value = value;
    }
  }
}
