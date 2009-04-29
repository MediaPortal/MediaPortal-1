#region Copyright (C) 2006-2009 Team MediaPortal

/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using TvDatabase;
using TvEngine;
using TvLibrary.Log;


namespace SetupTv.Sections
{
  public partial class TvMovieSetup : SetupTv.SectionSettings
  {
    #region ChannelInfo class
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
    #endregion

    #region Form Methods
    private void treeViewStations_DoubleClick(object sender, EventArgs e)
    {
      if (treeViewTvMStations.SelectedNode != null)
        treeViewTvMStations.SelectedNode.Collapse();
      MapStation();
    }

    private void treeViewChannels_DoubleClick(object sender, EventArgs e)
    {
      UnmapStation();
    }

    private void linkLabelInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start("http://www.tvmovie.de/ClickFinder.57.0.html");
    }
    #endregion

    #region Constructor
    public TvMovieSetup()
      : this("TV Movie Clickfinder EPG import")
    {
    }

    public TvMovieSetup(string name)
      : base(name)
    {
      InitializeComponent();
    }
    #endregion

    #region Serialisation

    public override void OnSectionDeActivated()
    {
      if (tabControlTvMovie.SelectedIndex == 2)
        SaveMapping();

      SaveDbSettings();

      base.OnSectionDeActivated();
    }

    private void SaveDbSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      TvMovieDatabase.DatabasePath = tbDbPath.Text;

      Setting setting = layer.GetSetting("TvMovieEnabled", "false");
      if (checkBoxEnableImport.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
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

      setting = layer.GetSetting("TvMovieShowRatings", "false");
      if (checkBoxShowRatings.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      setting = layer.GetSetting("TvMovieLimitActors", "5");
      if (checkBoxLimitActors.Checked)
        setting.Value = "5";
      else
        setting.Value = "0";
      setting.Persist();

      setting = layer.GetSetting("TvMovieRestPeriod", "24");
      setting.Value = GetRestPeriod();
      setting.Persist();
    }

    public override void OnSectionActivated()
    {
      LoadDbSettings();

      base.OnSectionActivated();
    }

    private void LoadDbSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      checkBoxEnableImport.Checked = layer.GetSetting("TvMovieEnabled", "false").Value == "true";
      checkBoxUseShortDesc.Checked = layer.GetSetting("TvMovieShortProgramDesc", "true").Value == "true";
      checkBoxAdditionalInfo.Checked = layer.GetSetting("TvMovieExtendDescription", "false").Value == "true";
      checkBoxShowRatings.Checked = layer.GetSetting("TvMovieShowRatings", "false").Value == "true";
      checkBoxShowAudioFormat.Checked = layer.GetSetting("TvMovieShowAudioFormat", "false").Value == "true";
      checkBoxSlowImport.Checked = layer.GetSetting("TvMovieSlowImport", "false").Value == "true";
      checkBoxLimitActors.Checked = Convert.ToInt32(layer.GetSetting("TvMovieLimitActors", "5").Value) > 0;
      SetRestPeriod(layer.GetSetting("TvMovieRestPeriod", "24").Value);
    }

    #endregion

    #region Mapping methods

    /// <summary>
    /// Load stations from databases and fill controls with that data
    /// </summary>
    private void LoadStations(bool localInstall)
    {
      TvMovieDatabase database = new TvMovieDatabase();
      if (database.Connect())
      {
        try
        {
          treeViewTvMStations.BeginUpdate();
          treeViewTvMStations.Nodes.Clear();
          imageListTvmStations.Images.Clear();
          treeViewTvMStations.ItemHeight = localInstall ? 24 : 16;

          string GifBasePath = TvMovieDatabase.TVMovieProgramPath + @"Gifs\";

          for (int i = 0; i < database.Stations.Count; i++)
          {
            try
            {
              TVMChannel station = database.Stations[i];

              if (localInstall)
              {
                string channelLogo = GifBasePath + station.TvmZeichen;
                if (!File.Exists(channelLogo))
                  channelLogo = GifBasePath + @"tvmovie_senderlogoplatzhalter.gif";

                // convert gif to ico
                Bitmap tvmLogo = new Bitmap(channelLogo);
                IntPtr iconHandle = tvmLogo.GetHicon();
                Icon stationThumb = Icon.FromHandle(iconHandle);
                imageListTvmStations.Images.Add(new Icon(stationThumb, new Size(32, 22)));
              }

              TreeNode stationNode = new TreeNode(station.TvmEpgDescription, i, i);//, subItems);
              ChannelInfo channelInfo = new ChannelInfo();
              channelInfo.Name = station.TvmEpgChannel;
              stationNode.Tag = channelInfo;
              treeViewTvMStations.Nodes.Add(stationNode);
            }
            catch (Exception exstat)
            {
              Log.Info("TvMovieSetup: Error loading TV Movie station - {0}", exstat.Message);
            }
          }

          treeViewTvMStations.EndUpdate();

          treeViewMpChannels.BeginUpdate();
          treeViewMpChannels.Nodes.Clear();
          try
          {
            List<Channel> mpChannelList = database.GetChannels();
            foreach (Channel channel in mpChannelList)
            {
              //TreeNode[] subItems = new TreeNode[] { new TreeNode(channel.IdChannel.ToString()), new TreeNode(channel.DisplayName) };
              TreeNode stationNode = new TreeNode(channel.DisplayName);
              stationNode.Tag = channel;
              treeViewMpChannels.Nodes.Add(stationNode);
            }
          }
          catch (Exception exdb)
          {
            Log.Info("TvMovieSetup: Error loading MP's channels from database - {0}", exdb.Message);
          }
          treeViewMpChannels.EndUpdate();
        }
        catch (Exception ex)
        {
          Log.Info("TvMovieSetup: Unhandled error in  LoadStations - {0}\n{1}", ex.Message, ex.StackTrace);
        }
      }
    }

    /// <summary>
    /// Map selected TVMovie station to a selected MP channel
    /// </summary>
    private void MapStation()
    {
      TreeNode selectedChannel = treeViewMpChannels.SelectedNode;
      if (selectedChannel == null)
        return;
      while (selectedChannel.Parent != null)
        selectedChannel = selectedChannel.Parent;

      TreeNode selectedStation = (TreeNode)treeViewTvMStations.SelectedNode.Clone();

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
      TreeNode selectedChannel = treeViewMpChannels.SelectedNode;
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
      IList<TvMovieMapping> mappingList = TvMovieMapping.ListAll();

      if (mappingList != null && mappingList.Count > 0)
      {
        foreach (TvMovieMapping mapping in mappingList)
          mapping.Remove();
      }
      else
        Log.Info("TvMovieSetup: SaveMapping - no mappingList items");

      TvBusinessLayer layer = new TvBusinessLayer();

      foreach (TreeNode channel in treeViewMpChannels.Nodes)
      {
        //Log.Debug("TvMovieSetup: Processing channel {0}", channel.Text);
        foreach (TreeNode station in channel.Nodes)
        {
          ChannelInfo channelInfo = (ChannelInfo)station.Tag;
          //Log.Debug("TvMovieSetup: Processing channelInfo {0}", channelInfo.Name);
          TvMovieMapping mapping = null;
          try
          {
            mapping = new TvMovieMapping(((Channel)channel.Tag).IdChannel, channelInfo.Name, channelInfo.Start, channelInfo.End);
          }
          catch (Exception exm)
          {
            Log.Error("TvMovieSetup: Error on new TvMovieMapping for channel {0} - {1}", channel.Text, exm.Message);
          }
          //Log.Write("TvMovieSetup: SaveMapping - new mapping for {0}/{1}", channel.Text, channelInfo.Name);
          try
          {
            Log.Debug("TvMovieSetup: Persisting TvMovieMapping for channel {0}", channel.Text);
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
      treeViewMpChannels.BeginUpdate();
      foreach (TreeNode treeNode in treeViewMpChannels.Nodes)
      {
        foreach (TreeNode childNode in treeNode.Nodes)
          childNode.Remove();
      }
      try
      {
        IList<TvMovieMapping> mappingDb = TvMovieMapping.ListAll();
        if (mappingDb != null && mappingDb.Count > 0)
        {
          foreach (TvMovieMapping mapping in mappingDb)
          {
            string MpChannelName = string.Empty;
            try
            {
              TreeNode channelNode = FindChannel(mapping.IdChannel);
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
            catch (Exception exInner)
            {
              Log.Debug("TVMovie plugin: Mapping of station {0} failed; maybe it has been deleted / changed ({1})", MpChannelName, exInner.Message);
            }
          }
        }
        else
          Log.Debug("TVMovie plugin: LoadMapping did not find any mapped channels");
      }
      catch (Exception ex)
      {
        Log.Debug("TVMovie plugin: LoadMapping failed - {0},{1}", ex.Message, ex.StackTrace);
      }
      ColorTree();
      treeViewMpChannels.EndUpdate();
    }

    private TreeNode FindChannel(int mpChannelId)
    {
      foreach (TreeNode MpNode in treeViewMpChannels.Nodes)
        if (MpNode.Tag != null)
        {
          Channel checkChannel = MpNode.Tag as Channel;
          if (checkChannel != null)
          {
            if (checkChannel.IdChannel == mpChannelId)
              return MpNode;
          }
          else
            Log.Debug("TVMovie plugin: FindChannel failed - no Channel in Node tag of {0}", MpNode.Text);
        }
      return null;
    }

    private TreeNode FindStation(string aTvMStationName)
    {
      foreach (TreeNode TvMNode in treeViewTvMStations.Nodes)
        if (TvMNode.Tag != null)
          if (((ChannelInfo)TvMNode.Tag).Name == aTvMStationName)
            return TvMNode;

      return null;
    }

    private void ColorTree()
    {
      foreach (TreeNode parentNode in treeViewMpChannels.Nodes)
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

    private string CleanInput(string input)
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
      ChannelInfo channelInfo = (ChannelInfo)treeViewMpChannels.SelectedNode.Tag;
      channelInfo.Start = CleanInput(maskedTextBoxTimeStart.Text);
      maskedTextBoxTimeStart.Text = CleanInput(maskedTextBoxTimeStart.Text);
      treeViewMpChannels.SelectedNode.Tag = channelInfo;
      if (channelInfo.Start != "00:00" || channelInfo.End != "00:00")
        treeViewMpChannels.SelectedNode.Text = string.Format("{0} ({1}-{2})", channelInfo.Name, channelInfo.Start, channelInfo.End);
      else
        treeViewMpChannels.SelectedNode.Text = string.Format("{0}", channelInfo.Name);
    }

    private void maskedTextBoxTimeEnd_Validated(object sender, EventArgs e)
    {
      ChannelInfo channelInfo = (ChannelInfo)treeViewMpChannels.SelectedNode.Tag;
      channelInfo.End = CleanInput(maskedTextBoxTimeEnd.Text);
      maskedTextBoxTimeEnd.Text = CleanInput(maskedTextBoxTimeEnd.Text);
      treeViewMpChannels.SelectedNode.Tag = channelInfo;
      if (channelInfo.Start != "00:00" || channelInfo.End != "00:00")
        treeViewMpChannels.SelectedNode.Text = string.Format("{0} ({1}-{2})", channelInfo.Name, channelInfo.Start, channelInfo.End);
      else
        treeViewMpChannels.SelectedNode.Text = string.Format("{0}", channelInfo.Name);
    }

    #endregion

    #region Form settings

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
        case "6": radioButton6h.Checked = true; break;
        case "12": radioButton12h.Checked = true; break;
        case "24": radioButton24h.Checked = true; break;
        case "48": radioButton2d.Checked = true; break;
        case "168": radioButton7d.Checked = true; break;
        default: radioButton24h.Checked = true; break;
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
      groupBoxDescriptions.Enabled = groupBoxImportTime.Enabled = groupBoxInstallMethod.Enabled = checkBoxEnableImport.Checked;

      if (checkBoxEnableImport.Checked)
      {
        rbLocal.Enabled = rbLocal.Checked = !string.IsNullOrEmpty(TvMovieDatabase.TVMovieProgramPath);
        rbManual.Checked = !rbLocal.Checked;
        tbDbPath.Text = TvMovieDatabase.DatabasePath;

        try
        {
          LoadStations(rbLocal.Checked);
        }
        catch (Exception ex1)
        {
          MessageBox.Show(this, "Please make sure a supported TV Movie Clickfinder release has been successfully installed.", "Error loading TV Movie stations", MessageBoxButtons.OK, MessageBoxIcon.Error);
          checkBoxEnableImport.Checked = false;
          Log.Info("TVMovie plugin: Error enabling TV Movie import in LoadStations() - {0},{1}", ex1.Message, ex1.StackTrace);
          return;
        }

        try
        {
          LoadMapping();
        }
        catch (Exception ex2)
        {
          MessageBox.Show(this, "Please make sure your using a valid channel mapping.", "Error loading TVM <-> MP channel mapping", MessageBoxButtons.OK, MessageBoxIcon.Error);
          checkBoxEnableImport.Checked = false;
          Log.Info("TVMovie plugin: Error enabling TV Movie import in LoadMapping() - {0},{1}", ex2.Message, ex2.StackTrace);
          return;
        }
      }
      else
        SaveMapping();
    }

    #endregion

    #region Manual import methods

    /// <summary>
    /// Inmediately updates and imports EPG data
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonImportNow_Click(object sender, EventArgs e)
    {
      buttonImportNow.Enabled = false;
      SaveDbSettings();
      try
      {
        Thread manualThread = new Thread(new ThreadStart(ManualImportThread));
        manualThread.Name = "TV Movie manual importer";
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
        _database.LaunchTVMUpdater(false);
        _database.OnStationsChanged += new TvMovieDatabase.StationsChanged(_database_OnStationsChanged);
        if (_database.Connect())
          _database.Import();
        buttonImportNow.Enabled = true;
      }
      catch (Exception ex)
      {
        Log.Info("TvMovie plugin error:");
        Log.Write(ex);
        buttonImportNow.Enabled = true;
      }
    }

    private void _database_OnStationsChanged(int value, int maximum, string text)
    {
      progressBarImportTotal.Maximum = maximum;
      if (value <= maximum && value >= 0)
        progressBarImportTotal.Value = value;
    }

    private void rbLocal_CheckedChanged(object sender, EventArgs e)
    {
      tbDbPath.Enabled = !rbLocal.Checked;
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      fileDialogDb.Filter = "Access database (*.mdb)|*.mdb|All files (*.*)|*.*";
      fileDialogDb.InitialDirectory = tbDbPath.Text;
      if (fileDialogDb.ShowDialog(this) == DialogResult.OK)
      {
        TvMovieDatabase.DatabasePath = tbDbPath.Text = fileDialogDb.FileName;
        checkBoxEnableImport_CheckedChanged(sender, null);
      }
    }

    #endregion
  }
}
