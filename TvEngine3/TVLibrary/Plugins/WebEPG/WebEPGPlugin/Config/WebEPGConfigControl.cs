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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using MediaPortal.EPG;
using MediaPortal.EPG.config;
using MediaPortal.WebEPG.Config;
using MediaPortal.WebEPG.Config.Grabber;
using MediaPortal.WebEPG.Profile;
using SetupTv.Sections.WebEPGConfig;
using TvDatabase;
using TvEngine.PowerScheduler;
using TvLibrary.Log;
using ChannelMap = MediaPortal.WebEPG.Config.ChannelMap;

namespace SetupTv.Sections
{
  public partial class WebEPGSetup : SectionSettings
  {

    private class CBChannelGroup
    {
      public string groupName;
      public int idGroup;

      public CBChannelGroup(string groupName, int idGroup)
      {
        this.groupName = groupName;
        this.idGroup = idGroup;
      }

      public override string ToString()
      {
        return groupName;
      }
    }
    
    private delegate void ShowStatusHandler(WebEPG.Stats status);

    private string _webepgFilesDir;
    private string _configFileDir;
    private WebepgConfigFile _configFile;
    private fSelection selection;
    private TreeNode tGrabbers;
    private SortedList CountryList;
    private Hashtable hChannelConfigInfo;
    private Hashtable hGrabberConfigInfo;
    private ChannelsList _channelInfo;
    private Dictionary<string, string> _countryList;
    private bool _initialized = false;

    public WebEPGSetup()
      : this("WebEPG")
    {
    }

    public WebEPGSetup(string name)
      : base(name)
    {
      InitializeComponent();

      _webepgFilesDir = Log.GetPathName() + @"\WebEPG\";

      if (!Directory.Exists(_webepgFilesDir))
      {
        throw new DirectoryNotFoundException("WebEPG: Files Directory Missing " + _webepgFilesDir);
      }

      _configFileDir = _webepgFilesDir;

      if (!Directory.Exists(_configFileDir))
      {
        Directory.CreateDirectory(_configFileDir);
      }
    }

    public override void OnSectionActivated()
    {
      Initialize();
      TvMappings.LoadGroups();
      RadioMappings.LoadGroups();

      ShowStatus();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      if (selection != null)
      {
        selection.Close();
        selection = null;
      }

      base.OnSectionDeActivated();
    }
    
    public override void LoadSettings()
    {
      base.LoadSettings();

      Initialize();

      TvBusinessLayer layer = new TvBusinessLayer();
      switch (layer.GetSetting("webepgDestination", "db").Value)
      {
        case "db":
          DestinationComboBox.SelectedIndex = 0;
          break;
        case "defxmltv":
          DestinationComboBox.SelectedIndex = 1;
          break;
        case "xmltv":
          DestinationComboBox.SelectedIndex = 2;
          break;
        default:
          DestinationComboBox.SelectedIndex = 0;
          break;
      }

      textBoxFolder.Text = layer.GetSetting("webepgDestinationFolder").Value;
      checkBoxDeleteBeforeImport.Checked = Convert.ToBoolean(layer.GetSetting("webepgDeleteBeforeImport", "true").Value);
      checkBoxDeleteOnlyOverlapping.Checked = Convert.ToBoolean(layer.GetSetting("webepgDeleteOnlyOverlapping", "true").Value);

      LoadWebepgConfigFile();
      //RedrawList(null);

      // Schedule
      ScheduleGrabCheckBox.Checked = Convert.ToBoolean(layer.GetSetting("webepgScheduleEnabled", "true").Value);
      EPGWakeupConfig config = new EPGWakeupConfig(layer.GetSetting("webepgSchedule", String.Empty).Value);
      foreach (EPGGrabDays day in config.Days)
      {
        switch (day)
        {
          case EPGGrabDays.Monday:
            MondayCheckBox.Checked = true;
            break;
          case EPGGrabDays.Tuesday:
            TuesdayCheckBox.Checked = true;
            break;
          case EPGGrabDays.Wednesday:
            WednesdayCheckBox.Checked = true;
            break;
          case EPGGrabDays.Thursday:
            ThursdayCheckBox.Checked = true;
            break;
          case EPGGrabDays.Friday:
            FridayCheckBox.Checked = true;
            break;
          case EPGGrabDays.Saturday:
            SaturdayCheckBox.Checked = true;
            break;
          case EPGGrabDays.Sunday:
            SundayCheckBox.Checked = true;
            break;
        }
      }
      grabTimeTextBox.Text = String.Format("{0:00}:{1:00}", config.Hour, config.Minutes);

    }

    public override void SaveSettings()
    {
      base.SaveSettings();

      _configFile.Info.GrabDays = (int)nMaxGrab.Value;

      _configFile.Channels = new List<ChannelMap>();

      foreach (ChannelMap channel in TvMappings.ChannelMapping.Values)
      {
        _configFile.Channels.Add(channel);
      }
      foreach (ChannelMap channel in RadioMappings.ChannelMapping.Values)
      {
        _configFile.RadioChannels.Add(channel);
      }

      Log.Info("WebEPG Config: Button: Save");
      string confFile = _configFileDir + "\\WebEPG.xml";
      FileInfo config = new FileInfo(confFile);
      if (config.Exists)
      {
        File.Delete(confFile.Replace(".xml", ".bak"));
        File.Move(confFile, confFile.Replace(".xml", ".bak"));
      }

      XmlSerializer s = new XmlSerializer(typeof(WebepgConfigFile));
      TextWriter w = new StreamWriter(confFile);
      s.Serialize(w, _configFile);
      w.Close();

      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("webepgDestination", "db");
      switch (DestinationComboBox.SelectedIndex)
      {
        case 0:
          setting.Value = "db";
          break;
        case 1:
          setting.Value = "defxmltv";
          break;
        case 2:
          setting.Value = "xmltv";
          break;
      }
      setting.Persist();

      setting = layer.GetSetting("webepgDestinationFolder");
      setting.Value = textBoxFolder.Text;
      setting.Persist();

      setting = layer.GetSetting("webepgDeleteBeforeImport", "true");
      setting.Value = checkBoxDeleteBeforeImport.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("webepgDeleteOnlyOverlapping", "true");
      setting.Value = checkBoxDeleteOnlyOverlapping.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("webepgScheduleEnabled", "true");
      setting.Value = ScheduleGrabCheckBox.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("webepgSchedule", String.Empty);
      EPGWakeupConfig cfg = new EPGWakeupConfig(setting.Value);
      EPGWakeupConfig newcfg = new EPGWakeupConfig();
      newcfg.Hour = cfg.Hour;
      newcfg.Minutes = cfg.Minutes;
      // newcfg.Days = cfg.Days;
      newcfg.LastRun = cfg.LastRun;
      string[] time = grabTimeTextBox.Text.Split(System.Globalization.DateTimeFormatInfo.CurrentInfo.TimeSeparator[0]);
      newcfg.Hour = Convert.ToInt32(time[0]);
      newcfg.Minutes = Convert.ToInt32(time[1]);
      CheckDay(newcfg, EPGGrabDays.Monday, MondayCheckBox.Checked);
      CheckDay(newcfg, EPGGrabDays.Tuesday, TuesdayCheckBox.Checked);
      CheckDay(newcfg, EPGGrabDays.Wednesday, WednesdayCheckBox.Checked);
      CheckDay(newcfg, EPGGrabDays.Thursday, ThursdayCheckBox.Checked);
      CheckDay(newcfg, EPGGrabDays.Friday, FridayCheckBox.Checked);
      CheckDay(newcfg, EPGGrabDays.Saturday, SaturdayCheckBox.Checked);
      CheckDay(newcfg, EPGGrabDays.Sunday, SundayCheckBox.Checked);

      if (!cfg.Equals(newcfg))
      {
        setting.Value = newcfg.SerializeAsString();
        setting.Persist();
      }

    }

    private void CheckDay(EPGWakeupConfig cfg, EPGGrabDays day, bool enabled)
    {
      if (enabled)
        cfg.Days.Add(day);
    }

    #region Private

    private void Initialize()
    {
      if (!_initialized)
      {
        LoadCountries();
        LoadConfig();
      }
      _initialized = true;
    }

    private void ShowStatus(WebEPG.Stats status)
    {
      labelLastImport.Text = status.StartTime.ToString();
      labelChannels.Text = status.Channels.ToString();
      labelPrograms.Text = status.Programs.ToString();
      labelStatus.Text = status.Status;

      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("webepgResultLastImport", "");
      setting.Value = status.StartTime.ToString();
      setting.Persist();
      setting = layer.GetSetting("webepgResultChannels", "");
      setting.Value = status.Channels.ToString();
      setting.Persist();
      setting = layer.GetSetting("webepgResultPrograms", "");
      setting.Value = status.Programs.ToString();
      setting.Persist();
      setting = layer.GetSetting("webepgResultStatus", "");
      setting.Value = status.Status;
      setting.Persist();

    }

    private void ShowStatus()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      labelLastImport.Text = layer.GetSetting("webepgResultLastImport", "").Value;
      labelChannels.Text = layer.GetSetting("webepgResultChannels", "").Value;
      labelPrograms.Text = layer.GetSetting("webepgResultPrograms", "").Value;
      labelStatus.Text = layer.GetSetting("webepgResultStatus", "").Value;
    }

    private void LoadCountries()
    {
      _channelInfo = new ChannelsList(_webepgFilesDir);
      string[] countries = _channelInfo.GetCountries();
      _countryList = new Dictionary<string, string>();
      cbCountry.Items.Clear();

      if (countries != null)
      {
        foreach (string country in countries)
        {
          try
          {
            RegionInfo region = new RegionInfo(country);
            cbCountry.Items.Add(region.EnglishName);
            _countryList.Add(region.EnglishName, country);
          }
          catch (Exception)
          {
          }
        }
      }

      for (int i = 0; i < cbCountry.Items.Count; i++)
      {
        if (cbCountry.Items[i].ToString() == RegionInfo.CurrentRegion.EnglishName)
        {
          cbCountry.SelectedIndex = i;
        }
      }
    }

    private void LoadConfig()
    {
      Log.Info("WebEPG Config: Loading Channels");
      hChannelConfigInfo = new Hashtable();
      TvMappings.HChannelConfigInfo = hChannelConfigInfo;
      RadioMappings.HChannelConfigInfo = hChannelConfigInfo;

      if (File.Exists(_webepgFilesDir + "\\channels\\channels.xml"))
      {
        Log.Info("WebEPG Config: Loading Existing channels.xml");
        Xml xmlreader = new Xml(_webepgFilesDir + "\\channels\\channels.xml");
        int channelCount = xmlreader.GetValueAsInt("ChannelInfo", "TotalChannels", 0);

        for (int i = 0; i < channelCount; i++)
        {
          ChannelConfigInfo channel = new ChannelConfigInfo();
          channel.ChannelID = xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
          channel.FullName = xmlreader.GetValueAsString(i.ToString(), "FullName", "");
          hChannelConfigInfo.Add(channel.ChannelID, channel);
        }
      }

      Log.Info("WebEPG Config: Loading Grabbers");
      hGrabberConfigInfo = new Hashtable();
      CountryList = new SortedList();
      tGrabbers = new TreeNode("Web Sites");
      if (Directory.Exists(_webepgFilesDir + "Grabbers"))
      {
        GetTreeGrabbers(ref tGrabbers, _webepgFilesDir + "Grabbers");
      }
      else
      {
        Log.Info("WebEPG Config: Cannot find grabbers directory");
      }

      IDictionaryEnumerator Enumerator = hChannelConfigInfo.GetEnumerator();
      while (Enumerator.MoveNext())
      {
        ChannelConfigInfo info = (ChannelConfigInfo) Enumerator.Value;
        if (info.ChannelID != null && info.FullName != null)
        {
          if (info.GrabberList != null)
          {
            IDictionaryEnumerator grabEnum = info.GrabberList.GetEnumerator();
            while (grabEnum.MoveNext())
            {
              GrabberConfigInfo gInfo = (GrabberConfigInfo) grabEnum.Value;
              SortedList chList = (SortedList) CountryList[gInfo.Country];
              if (chList[info.ChannelID] == null)
              {
                chList.Add(info.ChannelID, gInfo.GrabberID);
                //CountryList.Remove(gInfo.Country);
                //CountryList.Add(gInfo.Country, chList);
              }
            }
          }
        }
      }
    }

    private void LoadWebepgConfigFile()
    {
      if (File.Exists(_configFileDir + "\\WebEPG.xml"))
      {
        Log.Info("WebEPG Config: Loading Existing WebEPG.xml");

        XmlSerializer s = new XmlSerializer(typeof (WebepgConfigFile));
        TextReader r = null;
        try
        {
          r = new StreamReader(_configFileDir + "\\WebEPG.xml");
          _configFile = (WebepgConfigFile) s.Deserialize(r);
          r.Close();
        }
        catch (InvalidOperationException ex)
        {
          if (r != null)
          {
            r.Close();
          }
          Log.Error("WebEPG: Error loading config {0}: {1}", _configFileDir + "\\WebEPG.xml",
                     ex.Message);
          LoadOldConfigFile();
        }
      }

      // no file found set defaults
      if (_configFile == null)
      {
        _configFile = new WebepgConfigFile();
        _configFile.Channels = new List<ChannelMap>();
        _configFile.Info = new WebepgInfo();
        _configFile.Info.GrabDays = 2;
      }

      TvMappings.ChannelMapping = new Dictionary<string, ChannelMap>();
      foreach (ChannelMap channel in _configFile.Channels)
      {
        TvMappings.ChannelMapping.Add(channel.displayName, channel);
        if (channel.merged != null && channel.merged.Count == 0)
        {
          channel.merged = null;
        }
      }
      TvMappings.OnChannelMappingChanged();

      RadioMappings.ChannelMapping = new Dictionary<string, ChannelMap>();
      foreach (ChannelMap channel in _configFile.RadioChannels)
      {
        RadioMappings.ChannelMapping.Add(channel.displayName, channel);
        if (channel.merged != null && channel.merged.Count == 0)
        {
          channel.merged = null;
        }
      }
      RadioMappings.OnChannelMappingChanged();

      nMaxGrab.Value = _configFile.Info.GrabDays;
    }

    private void LoadOldConfigFile()
    {
      Log.Info("Trying to load old config file format");

      _configFile = new WebepgConfigFile();

      Xml xmlreader = new Xml(_configFileDir + "\\WebEPG.xml");

      _configFile.Info = new WebepgInfo();
      _configFile.Info.GrabDays = xmlreader.GetValueAsInt("General", "MaxDays", 2);
      _configFile.Info.GrabberDir = xmlreader.GetValueAsString("General", "GrabberDir", null);

      int AuthCount = xmlreader.GetValueAsInt("AuthSites", "Count", 0);
      if (AuthCount > 0)
      {
        _configFile.Sites = new List<SiteAuth>();
        for (int i = 1; i <= AuthCount; i++)
        {
          SiteAuth site = new SiteAuth();
          site.id = xmlreader.GetValueAsString("Auth" + i.ToString(), "Site", "");
          site.username = xmlreader.GetValueAsString("Auth" + i.ToString(), "Login", "");
          site.password = xmlreader.GetValueAsString("Auth" + i.ToString(), "Password", "");
          _configFile.Sites.Add(site);
        }
      }

      _configFile.Channels = new List<ChannelMap>();

      int channelCount = xmlreader.GetValueAsInt("ChannelMap", "Count", 0);

      for (int i = 1; i <= channelCount; i++)
      {
        ChannelMap channel = new ChannelMap();
        channel.displayName = xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");
        string grabber = xmlreader.GetValueAsString(i.ToString(), "Grabber1", "");
        ;
        //if (mergedList.ContainsKey(channel.displayName))
        //{
        //  channel.merged = mergedList[channel.displayName];
        //  foreach (MergedChannel mergedChannel in channel.merged)
        //    mergedChannel.grabber = grabber;
        //}
        //else
        //{
        channel.id = xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
        channel.grabber = grabber;
        //}
        _configFile.Channels.Add(channel);
      }

      int mergeCount = xmlreader.GetValueAsInt("MergeChannels", "Count", 0);
      Dictionary<string, List<MergedChannel>> mergedList = new Dictionary<string, List<MergedChannel>>();

      if (mergeCount > 0)
      {
        for (int i = 1; i <= mergeCount; i++)
        {
          int channelcount = xmlreader.GetValueAsInt("Merge" + i.ToString(), "Channels", 0);
          if (channelcount > 0)
          {
            List<MergedChannel> mergedChannels = new List<MergedChannel>();
            ChannelMap channel = new ChannelMap();
            channel.displayName = xmlreader.GetValueAsString("Merge" + i.ToString(), "DisplayName", "");
            channel.merged = new List<MergedChannel>();
            for (int c = 1; c <= channelcount; c++)
            {
              MergedChannel mergedChannel = new MergedChannel();
              mergedChannel.id = xmlreader.GetValueAsString("Merge" + i.ToString(), "Channel" + c.ToString(), "");
              mergedChannel.start = xmlreader.GetValueAsString("Merge" + i.ToString(), "Start" + c.ToString(), "0:0");
              mergedChannel.end = xmlreader.GetValueAsString("Merge" + i.ToString(), "End" + c.ToString(), "0:0");
              channel.merged.Add(mergedChannel);
            }

            _configFile.Channels.Add(channel);
          }
        }
      }

      xmlreader.Clear();
      xmlreader.Dispose();
    }

    private void GetGrabbers(ref TreeNode Main, string Location)
    {
      DirectoryInfo dir = new DirectoryInfo(Location);
      Log.Debug("WebEPG Config: Directory: {0}", Location);
      GrabberConfigInfo gInfo;
      foreach (FileInfo file in dir.GetFiles("*.xml"))
      {
        gInfo = new GrabberConfigInfo();
        //XmlDocument xml = new XmlDocument();
        GrabberConfigFile grabberXml;
        try
        {
          Log.Debug("WebEPG Config: File: {0}", file.Name);

          XmlSerializer s = new XmlSerializer(typeof (GrabberConfigFile));
          TextReader r = new StreamReader(file.FullName);
          grabberXml = (GrabberConfigFile) s.Deserialize(r);
        }
        catch (Exception)
        {
          Log.Info("WebEPG Config: File open failed - XML error");
          return;
        }

        gInfo.GrabDays = grabberXml.Info.GrabDays;

        string GrabberSite = file.Name.Replace(".xml", "");
        GrabberSite = GrabberSite.Replace("_", ".");

        gInfo.GrabberID = file.Directory.Name + "\\" + file.Name;
        gInfo.GrabberName = GrabberSite;
        gInfo.Country = file.Directory.Name;
        hGrabberConfigInfo.Add(gInfo.GrabberID, gInfo);

        if (CountryList[file.Directory.Name] == null)
        {
          CountryList.Add(file.Directory.Name, new SortedList());
        }

        TreeNode gNode = new TreeNode(GrabberSite);
        Main.Nodes.Add(gNode);
        //XmlNode cl=sectionList.Attributes.GetNamedItem("ChannelList");

        foreach (ChannelInfo channel in grabberXml.Channels)
        {
          if (channel.id != null)
          {
            ChannelConfigInfo info = (ChannelConfigInfo) hChannelConfigInfo[channel.id];
            if (info != null) // && info.GrabberList[gInfo.GrabberID] != null)
            {
              TreeNode tNode = new TreeNode(info.FullName);
              tNode.Tag = new GrabberSelectionInfo(info.ChannelID, gInfo.GrabberID);
              gNode.Nodes.Add(tNode);
              if (info.GrabberList == null)
              {
                info.GrabberList = new SortedList();
              }
              if (info.GrabberList[gInfo.GrabberID] == null)
              {
                info.GrabberList.Add(gInfo.GrabberID, gInfo);
              }
            }
            else
            {
              info = new ChannelConfigInfo();
              info.ChannelID = channel.id;
              info.FullName = info.ChannelID;
              info.GrabberList = new SortedList();
              info.GrabberList.Add(gInfo.GrabberID, gInfo);
              hChannelConfigInfo.Add(info.ChannelID, info);

              TreeNode tNode = new TreeNode(info.FullName);
              tNode.Tag = new GrabberSelectionInfo(info.ChannelID, gInfo.GrabberID);
              gNode.Nodes.Add(tNode);
            }
          }
        }
      }
    }

    private void GetTreeGrabbers(ref TreeNode Main, string Location)
    {
      foreach (string countryName in _countryList.Keys)
      {
        string countryLocation = Location + "\\" + _countryList[countryName];
        if (Directory.Exists(countryLocation))
        {
          TreeNode MainNext = new TreeNode(countryName);
          GetGrabbers(ref MainNext, countryLocation);
          Main.Nodes.Add(MainNext);
        }
      }
      //DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      //DirectoryInfo[] dirList = dir.GetDirectories();
      //if (dirList.Length > 0)
      //{
      //  if (dirList.Length == 1)
      //  {
      //    System.IO.DirectoryInfo g = dirList[0];
      //    if (g.Name == ".svn")
      //      GetGrabbers(ref Main, Location);
      //  }
      //  else
      //  {
      //    for (int i = 0; i < dirList.Length; i++)
      //    {
      //      //LOAD FOLDERS
      //      System.IO.DirectoryInfo g = dirList[i];
      //      TreeNode MainNext = new TreeNode(g.Name);
      //      GetTreeGrabbers(ref MainNext, g.FullName);
      //      Main.Nodes.Add(MainNext);
      //      //MainNext.Tag = (g.FullName);
      //    }
      //  }
      //}
      //else
      //{
      //  GetGrabbers(ref Main, Location);
      //}
    }


    private void AutoMapChannels(ICollection<ChannelMap> ChannelMapping)
    {
      if (cbCountry.SelectedItem != null)
      {
        string countryCode = _countryList[cbCountry.SelectedItem.ToString()];
        List<ChannelGrabberInfo> channels =
          _channelInfo.GetChannelArrayList(countryCode);
        foreach (ChannelMap channelMap in ChannelMapping)
        {
          if (channelMap.id == null)
          {
            int channelNumb = _channelInfo.FindChannel(channelMap.displayName, countryCode);
            if (channelNumb >= 0)
            {
              ChannelGrabberInfo channelDetails = channels[channelNumb];
              if (channelDetails.GrabberList != null)
              {
                channelMap.id = channelDetails.ChannelID;
                channelMap.grabber = channelDetails.GrabberList[0].GrabberID;
              }
            }
          }
        }
      }
    }

    #endregion

    #region Event handlers

    private void bSave_Click(object sender, EventArgs e)
    {
      SaveSettings();
    }

    private void DoSelect(Object source, GrabberSelectedEventArgs e)
    {
      GrabberSelectionInfo id = e.Selection;
      switch (tabMain.SelectedIndex)
      {
        case 1:
          TvMappings.OnGrabberSelected(source, e);
          break;
        case 2:
          RadioMappings.OnGrabberSelected(source, e);
          break;
      }
    }

    private void CloseSelect(Object source, EventArgs e)
    {
      if (source == selection)
      {
        selection = null;
      }
    }

    private void bAutoMap_Click(object sender, EventArgs e)
    {
      if (cbCountry.SelectedItem != null)
      {
        Cursor.Current = Cursors.WaitCursor;
        tabMain.SelectedIndex = 2;
        if (TvMappings.ChannelMapping.Count == 0)
        {
          TvMappings.DoImportChannels();
        }
        AutoMapChannels(TvMappings.ChannelMapping.Values);

        tabMain.SelectedIndex = 3;
        if (RadioMappings.ChannelMapping.Count == 0)
        {
          RadioMappings.DoImportChannels();
        }
        AutoMapChannels(TvMappings.ChannelMapping.Values);
      }
      return;
    }

    #endregion

    private fSelection ShowGrabberSelection()
    {
      if (selection == null)
      {
        selection = new fSelection(tGrabbers); //, true, this.DoSelect);
        selection.GrabberSelected += this.DoSelect;
        selection.MinimizeBox = false;
        selection.Closed += new EventHandler(this.CloseSelect);
        selection.Show();
      }
      else
      {
        selection.BringToFront();
      }
      return selection;
    }

    private void buttonManualImport_Click(object sender, EventArgs e)
    {
      TvEngine.WebEPGImport importer = new TvEngine.WebEPGImport();

      importer.ForceImport(ShowImportProgress);
    }

    private void ShowImportProgress(WebEPG.Stats status)
    {
      this.Invoke(new ShowStatusHandler(ShowStatus), new object[]{status});
    }

    private void StatusTimer_Tick(object sender, EventArgs e)
    {
      ShowStatus();
    }

    private void DestinationComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool enable = (DestinationComboBox.SelectedIndex == 2);

      textBoxFolder.Enabled = enable;
      buttonBrowse.Enabled = enable;
    }

    private void checkBoxDeleteBeforeImport_CheckedChanged(object sender, EventArgs e)
    {
      checkBoxDeleteOnlyOverlapping.Enabled = checkBoxDeleteBeforeImport.Checked;
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      folderBrowserDialogTVGuide.ShowDialog();
      textBoxFolder.Text = folderBrowserDialogTVGuide.SelectedPath;
    }

    private void Mappings_SelectGrabberClick(object sender, EventArgs e)
    {
      ShowGrabberSelection();
    }

    private void Mappings_AutoMapChannels(object sender, EventArgs e)
    {
      WebEPGMappingControl mappings = sender as WebEPGMappingControl;
      AutoMapChannels(mappings.ChannelMapping.Values);
    }
  }
}