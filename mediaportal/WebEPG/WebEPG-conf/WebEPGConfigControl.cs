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
using MediaPortal.Configuration;
using MediaPortal.EPG.config;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.WebEPG.config;
using MediaPortal.WebEPG.Config.Grabber;
using MediaPortal.Webepg.Profile;
using TvDatabase;
using ChannelMap=MediaPortal.WebEPG.config.ChannelMap;

namespace WebEPG_conf
{
  public partial class WebEPGConfigControl : UserControl
  {
    private string _webepgFilesDir;
    private string _configFileDir;
    private WebepgConfigFile _configFile;
    private Dictionary<string, ChannelMap> _channelMapping;
    private fSelection selection;
    private MergedChannelDetails _mergeConfig;
    //private TreeNode tChannels;
    private TreeNode tGrabbers;
    private SortedList CountryList;
    private SortedList ChannelList;
    private Hashtable hChannelConfigInfo;
    private Hashtable hGrabberConfigInfo;
    private ILog _log;
    private ChannelsList _channelInfo;
    private Dictionary<string, string> _countryList;
    private ListViewColumnSorter lvwColumnSorter;

    public WebEPGConfigControl()
    {
      InitializeComponent();

      lvMapping.Columns.Add("EPG Name", 100, HorizontalAlignment.Left);
      lvMapping.Columns.Add("Channel Name", 100, HorizontalAlignment.Left);
      lvMapping.Columns.Add("Channel ID", 80, HorizontalAlignment.Left);
      lvMapping.Columns.Add("Grabber", 120, HorizontalAlignment.Left);

      lvwColumnSorter = new ListViewColumnSorter();
      lvMapping.ListViewItemSorter = lvwColumnSorter;

      lvMerged.Columns.Add("Channel", 100, HorizontalAlignment.Left);
      lvMerged.Columns.Add("Grabber", 105, HorizontalAlignment.Left);
      lvMerged.Columns.Add("Start", 50, HorizontalAlignment.Left);
      lvMerged.Columns.Add("End", 50, HorizontalAlignment.Left);

      cbSource.SelectedIndex = 0;

      _log = GlobalServiceProvider.Get<ILog>();

      _webepgFilesDir = Config.GetSubFolder(Config.Dir.Base, @"WebEPG\");

      if (!Directory.Exists(_webepgFilesDir))
      {
        throw new DirectoryNotFoundException("WebEPG: Files Directory Missing " + _webepgFilesDir);
      }

      if (File.Exists(Config.GetFile(Config.Dir.Base, "MediaPortalDirs.xml")))
      {
        _configFileDir = Config.GetSubFolder(Config.Dir.Config, @"WebEPG\");
      }
      else
      {
        _configFileDir = _webepgFilesDir;
      }

      if (!Directory.Exists(_configFileDir))
      {
        Directory.CreateDirectory(_configFileDir);
      }

      LoadCountries();
      LoadConfig();
      LoadWebepgConfigFile();

      RedrawList(null);
    }

    #region Private

    private void getMediaPortalTvChannels()
    {
      ArrayList channels = new ArrayList();

      TVDatabase.GetChannels(ref channels);
      for (int i = 0; i < channels.Count; i++)
      {
        TVChannel chan = (TVChannel) channels[i];
        if (!_channelMapping.ContainsKey(chan.Name))
        {
          ChannelMap channel = new ChannelMap();
          channel.displayName = chan.Name;
          _channelMapping.Add(chan.Name, channel);
        }
      }
    }

    private void getTvServerTvChannels()
    {
      IList<Channel> Channels = Channel.ListAll();

      foreach (Channel chan in Channels)
      {
        if (!_channelMapping.ContainsKey(chan.DisplayName))
        {
          ChannelMap channel = new ChannelMap();
          channel.displayName = chan.DisplayName;
          _channelMapping.Add(chan.DisplayName, channel);
        }
      }
    }

    private void RedrawList(string selectName)
    {
      int selectedIndex = 0;
      if (lvMapping.SelectedIndices.Count > 0)
      {
        selectedIndex = lvMapping.SelectedIndices[0];
      }

      lvMapping.Items.Clear();

      //add all channels
      foreach (ChannelMap channel in _channelMapping.Values)
      {
        ListViewItem channelItem = new ListViewItem(channel.displayName);
        string name = string.Empty;
        if (channel.id != null)
        {
          ChannelConfigInfo info = (ChannelConfigInfo) hChannelConfigInfo[channel.id];
          if (info != null)
          {
            name = info.FullName;
          }
        }
        else
        {
          if (channel.merged != null)
          {
            name = "[Merged]";
          }
        }
        channelItem.SubItems.Add(name);
        channelItem.SubItems.Add(channel.id);
        channelItem.SubItems.Add(channel.grabber);
        lvMapping.Items.Add(channelItem);
      }

      if (lvMapping.Items.Count > 0)
      {
        if (lvMapping.Items.Count > selectedIndex)
        {
          lvMapping.Items[selectedIndex].Selected = true;
        }
        else
        {
          lvMapping.Items[lvMapping.Items.Count - 1].Selected = true;
        }
      }

      tbCount.Text = lvMapping.Items.Count.ToString();
      lvMapping.Select();
    }

    private void UpdateList()
    {
      //update existing channels
      foreach (ListViewItem channel in lvMapping.Items)
      {
        if (_channelMapping.ContainsKey(channel.Text))
        {
          ChannelMap channelDetails = _channelMapping[channel.Text];
          string name = string.Empty;
          if (channelDetails.id != null)
          {
            ChannelConfigInfo info = (ChannelConfigInfo) hChannelConfigInfo[channelDetails.id];
            if (info != null)
            {
              name = info.FullName;
            }
          }
          else
          {
            if (channelDetails.merged != null)
            {
              name = "[Merged]";
            }
          }
          channel.SubItems[1].Text = name;
          channel.SubItems[2].Text = channelDetails.id;
          channel.SubItems[3].Text = channelDetails.grabber;
        }
        else
        {
          int selectedIndex = 0;
          if (lvMapping.SelectedIndices.Count > 0)
          {
            selectedIndex = lvMapping.SelectedIndices[0];
          }

          lvMapping.Items.Remove(channel);

          if (lvMapping.Items.Count > 0)
          {
            if (lvMapping.Items.Count > selectedIndex)
            {
              lvMapping.Items[selectedIndex].Selected = true;
            }
            else
            {
              lvMapping.Items[lvMapping.Items.Count - 1].Selected = true;
            }
          }
        }
      }
      lvMapping.Select();
    }

    private void UpdateMergedList(ChannelMap channelMap)
    {
      lvMerged.Items.Clear();

      if (channelMap.merged != null)
      {
        //add all channels
        foreach (MergedChannel channel in channelMap.merged)
        {
          ListViewItem channelItem = new ListViewItem(channel.id);
          channelItem.Tag = channel;
          channelItem.SubItems.Add(channel.grabber);
          channelItem.SubItems.Add(channel.start);
          channelItem.SubItems.Add(channel.end);
          lvMerged.Items.Add(channelItem);
        }
      }
    }

    private void LoadCountries()
    {
      _channelInfo = new ChannelsList(_webepgFilesDir);
      string[] countries = _channelInfo.GetCountries();
      _countryList = new Dictionary<string, string>();

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
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Channels");
      hChannelConfigInfo = new Hashtable();

      if (File.Exists(_webepgFilesDir + "\\channels\\channels.xml"))
      {
        _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Existing channels.xml");
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

      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Grabbers");
      hGrabberConfigInfo = new Hashtable();
      CountryList = new SortedList();
      tGrabbers = new TreeNode("Web Sites");
      if (Directory.Exists(_webepgFilesDir + "Grabbers"))
      {
        GetTreeGrabbers(ref tGrabbers, _webepgFilesDir + "Grabbers");
      }
      else
      {
        _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Cannot find grabbers directory");
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

      //tChannels = new TreeNode("Channels");
      //IDictionaryEnumerator countryEnum = CountryList.GetEnumerator();
      //while (countryEnum.MoveNext())
      //{
      //  SortedList chList = (SortedList)countryEnum.Value;
      //  TreeNode cNode = new TreeNode();
      //  cNode.Text = (string)countryEnum.Key;

      //  IDictionaryEnumerator chEnum = chList.GetEnumerator();
      //  while (chEnum.MoveNext())
      //  {
      //    TreeNode chNode = new TreeNode();

      //    ChannelConfigInfo info = (ChannelConfigInfo)hChannelConfigInfo[chEnum.Key];
      //    chNode.Text = info.FullName;
      //    string[] tag = new string[2];
      //    tag[0] = info.ChannelID;
      //    tag[1] = (string)chEnum.Value;
      //    chNode.Tag = tag;

      //    cNode.Nodes.Add(chNode);
      //  }

      //  tChannels.Nodes.Add(cNode);
      //}

      ChannelList = new SortedList();
    }

    private void LoadWebepgConfigFile()
    {
      if (File.Exists(_configFileDir + "\\WebEPG.xml"))
      {
        _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Existing WebEPG.xml");

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
          _log.Error(LogType.WebEPG, "WebEPG: Error loading config {0}: {1}", _configFileDir + "\\WebEPG.xml",
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

      _channelMapping = new Dictionary<string, ChannelMap>();

      foreach (ChannelMap channel in _configFile.Channels)
      {
        _channelMapping.Add(channel.displayName, channel);
        if (channel.merged != null && channel.merged.Count == 0)
        {
          channel.merged = null;
        }
      }

      nMaxGrab.Value = _configFile.Info.GrabDays;
    }

    private void LoadOldConfigFile()
    {
      _log.Info(LogType.WebEPG, "Trying to load old config file format");

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
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Directory: {0}", Location);
      GrabberConfigInfo gInfo;
      foreach (FileInfo file in dir.GetFiles("*.xml"))
      {
        gInfo = new GrabberConfigInfo();
        //XmlDocument xml = new XmlDocument();
        GrabberConfigFile grabberXml;
        try
        {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File: {0}", file.Name);

          XmlSerializer s = new XmlSerializer(typeof (GrabberConfigFile));
          TextReader r = new StreamReader(file.FullName);
          grabberXml = (GrabberConfigFile) s.Deserialize(r);
        }
        catch (Exception)
        {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File open failed - XML error");
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
              string[] tag = new string[2];
              tag[0] = info.ChannelID;
              tag[1] = gInfo.GrabberID;
              tNode.Tag = tag;
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
              string[] tag = new string[2];
              tag[0] = info.ChannelID;
              tag[1] = gInfo.GrabberID;
              tNode.Tag = tag;
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

    #endregion

    #region Event handlers

    private void bImport_Click(object sender, EventArgs e)
    {
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Import");
      try
      {
        switch (cbSource.SelectedItem.ToString())
        {
          case "MediaPortal":
            _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Importing from MediaPortal Database");
            getMediaPortalTvChannels();
            break;
          case "TV Server":
            _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Importing from TV Server Database");
            getTvServerTvChannels();
            break;
          default:
            break;
        }

        RedrawList(null);
      }
      catch (Exception ex)
      {
        _log.WriteFile(LogType.WebEPG, Level.Error, "WebEPG Config: Import failed - {0}", ex.Message);
        MessageBox.Show("An error occured while trying to import channels. See log for more details.", "Import Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
    }

    private void bSave_Click(object sender, EventArgs e)
    {
      _configFile.Info.GrabDays = (int) nMaxGrab.Value;

      _configFile.Channels = new List<ChannelMap>();

      foreach (ChannelMap channel in _channelMapping.Values)
      {
        _configFile.Channels.Add(channel);
      }

      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Save");
      string confFile = _configFileDir + "\\WebEPG.xml";
      FileInfo config = new FileInfo(confFile);
      if (config.Exists)
      {
        File.Delete(confFile.Replace(".xml", ".bak"));
        File.Move(confFile, confFile.Replace(".xml", ".bak"));
      }

      XmlSerializer s = new XmlSerializer(typeof (WebepgConfigFile));
      TextWriter w = new StreamWriter(confFile);
      s.Serialize(w, _configFile);
      w.Close();
    }

    private void cbSource_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (cbSource.SelectedItem.ToString())
      {
        case "MediaPortal":
          bImport.Enabled = true;
          break;
        case "TV Server":
          if (File.Exists("TVDatabase.dll"))
          {
            bImport.Enabled = true;
          }
          else
          {
            MessageBox.Show("TVDatabase.dll not found. Unable to import from database.", "TV Server file missing",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            bImport.Enabled = false;
          }
          break;
        default:
          break;
      }
    }

    private void bAdd_Click(object sender, EventArgs e)
    {
      if (!_channelMapping.ContainsKey(mtbNewChannel.Text))
      {
        ChannelMap channel = new ChannelMap();
        channel.displayName = mtbNewChannel.Text;
        _channelMapping.Add(channel.displayName, channel);
        RedrawList(channel.displayName);
      }
      else
      {
        MessageBox.Show("Channel with that name already exists", "Name Entry Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
      }
    }

    private void bRemove_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem channel in lvMapping.SelectedItems)
      {
        if (_channelMapping.ContainsKey(channel.Text))
        {
          _channelMapping.Remove(channel.Text);
        }
      }

      UpdateList();
    }

    private void bClearMapping_Click(object sender, EventArgs e)
    {
      foreach (ListViewItem channel in lvMapping.SelectedItems)
      {
        if (_channelMapping.ContainsKey(channel.Text))
        {
          ChannelMap channelMap = _channelMapping[channel.Text];
          channelMap.id = null;
          channelMap.grabber = null;
          _channelMapping.Remove(channel.Text);
          _channelMapping.Add(channel.Text, channelMap);
        }
      }

      UpdateList();
    }

    private void bChannelID_Click(object sender, EventArgs e)
    {
      if (selection == null)
      {
        selection = new fSelection(tGrabbers, true, this.DoSelect);
        selection.MinimizeBox = false;
        selection.Closed += new EventHandler(this.CloseSelect);
        selection.Show();
      }
      else
      {
        selection.BringToFront();
      }
    }

    private void bGrabber_Click(object sender, EventArgs e)
    {
      //if (selection == null)
      //{
      //  selection = new fSelection(tChannels, tGrabbers, false, this.DoSelect);
      //  selection.MinimizeBox = false;
      //  selection.Closed += new System.EventHandler(this.CloseSelect);
      //  selection.Show();
      //}
      //else
      //{
      //  selection.BringToFront();
      //}
    }

    private bool UpdateGrabberDetails(string channelId, string grabberId)
    {
      tbChannelName.Text = null;
      tbGrabSite.Text = null;
      tbGrabDays.Text = null;

      if (channelId != null && grabberId != null)
      {
        tbChannelName.Tag = channelId;
        ChannelConfigInfo info = (ChannelConfigInfo) hChannelConfigInfo[channelId];
        if (info != null)
        {
          tbChannelName.Text = info.FullName;
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Selection: {0}", info.FullName);

          GrabberConfigInfo gInfo = (GrabberConfigInfo) info.GrabberList[grabberId];
          if (gInfo != null)
          {
            tbGrabSite.Text = gInfo.GrabberName;
            //tbGrabSite.Tag = gInfo.GrabberID;
            tbGrabDays.Text = gInfo.GrabDays.ToString();
            return true;
          }
          else
          {
            tbGrabSite.Text = "(Unknown)";
          }
        }
      }
      return false;
    }

    private void DisplayChannelGrabberInfo(ChannelMap channel)
    {
      if (channel.merged != null && channel.merged.Count > 0)
      {
        tcMappingDetails.SelectedIndex = 1;
        UpdateMergedList(channel);
      }
      else
      {
        tcMappingDetails.SelectedIndex = 0;
        UpdateGrabberDetails(channel.id, channel.grabber);
      }

      lvMapping.Select();
    }

    private void DoSelect(Object source, EventArgs e)
    {
      //this.Activate(); -> form control
      string[] id = selection.Selected;

      if (id != null)
      {
        if (UpdateGrabberDetails(id[0], id[1]))
        {
          foreach (ListViewItem channel in lvMapping.SelectedItems)
          {
            if (_channelMapping.ContainsKey(channel.Text))
            {
              ChannelMap channelMap = _channelMapping[channel.Text];
              channelMap.id = id[0];
              channelMap.grabber = id[1];
              _channelMapping.Remove(channel.Text);
              _channelMapping.Add(channel.Text, channelMap);
            }
          }
        }

        UpdateList();
      }
    }

    private void CloseSelect(Object source, EventArgs e)
    {
      if (source == selection)
      {
        selection = null;
      }
    }

    private void lvMapping_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (lvMapping.SelectedItems.Count > 0 && _channelMapping.ContainsKey(lvMapping.SelectedItems[0].Text))
      {
        ChannelMap channel = _channelMapping[lvMapping.SelectedItems[0].Text];
        DisplayChannelGrabberInfo(channel);
      }
    }

    private void bAutoMap_Click(object sender, EventArgs e)
    {
      lvMapping.Select();
      if (cbCountry.SelectedItem != null)
      {
        Cursor.Current = Cursors.WaitCursor;
        List<ChannelGrabberInfo> channels =
          _channelInfo.GetChannelArrayList(_countryList[cbCountry.SelectedItem.ToString()]);
        foreach (ListViewItem channel in lvMapping.Items)
        {
          ChannelMap channelMap = _channelMapping[channel.Text];
          if (channelMap.id == null)
          {
            lvMapping.SelectedItems.Clear();
            channel.Selected = true;
            channel.EnsureVisible();
            lvMapping.Refresh();
            int channelNumb = _channelInfo.FindChannel(channel.Text, _countryList[cbCountry.SelectedItem.ToString()]);
            if (channelNumb >= 0)
            {
              ChannelGrabberInfo channelDetails = channels[channelNumb];
              if (channelDetails.GrabberList != null)
              {
                channelMap.id = channelDetails.ChannelID;
                channelMap.grabber = channelDetails.GrabberList[0].GrabberID;
                _channelMapping.Remove(channel.Text);
                _channelMapping.Add(channel.Text, channelMap);
              }
              UpdateList();
            }
          }
        }
        Cursor.Current = Cursors.Default;
      }
    }

    private void lvMapping_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Determine if clicked column is already the column that is being sorted.
      if (e.Column == lvwColumnSorter.SortColumn)
      {
        // Reverse the current sort direction for this column.
        if (lvwColumnSorter.Order == SortOrder.Ascending)
        {
          lvwColumnSorter.Order = SortOrder.Descending;
        }
        else
        {
          lvwColumnSorter.Order = SortOrder.Ascending;
        }
      }
      else
      {
        // Set the column number that is to be sorted; default to ascending.
        lvwColumnSorter.SortColumn = e.Column;
        lvwColumnSorter.Order = SortOrder.Ascending;
      }

      // Perform the sort with these new sort options.
      this.lvMapping.Sort();
    }

    #endregion

    private void bMergedAdd_Click(object sender, EventArgs e)
    {
      lvMerged.SelectedItems.Clear();
      _mergeConfig = new MergedChannelDetails(tGrabbers, null, this.bMergedOk_Click);
      _mergeConfig.MinimizeBox = false;
      _mergeConfig.Show();
    }

    private void bMergedOk_Click(object sender, EventArgs e)
    {
      if (lvMapping.SelectedItems.Count == 1)
      {
        ChannelMap channelMap = _channelMapping[lvMapping.SelectedItems[0].Text];
        if (lvMerged.SelectedItems.Count == 1)
        {
          MergedChannel channelDetails = (MergedChannel) lvMerged.SelectedItems[0].Tag;

          channelDetails.id = _mergeConfig.ChannelDetails.id;
          channelDetails.grabber = _mergeConfig.ChannelDetails.grabber;
          channelDetails.start = _mergeConfig.ChannelDetails.start;
          channelDetails.end = _mergeConfig.ChannelDetails.end;
        }
        else
        {
          channelMap.merged.Add(_mergeConfig.ChannelDetails);
        }
        UpdateMergedList(channelMap);
      }
      _mergeConfig.Close();
    }

    private void bMergedRemove_Click(object sender, EventArgs e)
    {
      if (lvMerged.SelectedItems.Count == 1 && lvMapping.SelectedItems.Count == 1)
      {
        ChannelMap channelMap = _channelMapping[lvMapping.SelectedItems[0].Text];
        channelMap.merged.Remove((MergedChannel) lvMerged.SelectedItems[0].Tag);
        UpdateMergedList(channelMap);
      }
    }

    private void bMergedEdit_Click(object sender, EventArgs e)
    {
      if (lvMerged.SelectedItems.Count == 1 && lvMapping.SelectedItems.Count == 1)
      {
        MergedChannel channel = (MergedChannel) lvMerged.SelectedItems[0].Tag;
        _mergeConfig = new MergedChannelDetails(tGrabbers, channel, this.bMergedOk_Click);
        _mergeConfig.MinimizeBox = false;
        _mergeConfig.Show();
      }
    }

    private void tcMappingDetails_Selecting(object sender, TabControlCancelEventArgs e)
    {
      if (tcMappingDetails.SelectedIndex == 1)
      {
        if (lvMapping.SelectedItems.Count == 1)
        {
          if (_channelMapping.ContainsKey(lvMapping.SelectedItems[0].Text))
          {
            ChannelMap channelMap = _channelMapping[lvMapping.SelectedItems[0].Text];
            if (channelMap.merged == null || channelMap.merged.Count == 0)
            {
              channelMap.merged = new List<MergedChannel>();
              if (channelMap.id != null)
              {
                MergedChannel channel = new MergedChannel();
                channel.id = channelMap.id;
                channelMap.id = null;
                channel.grabber = channelMap.grabber;
                channelMap.grabber = null;
                channelMap.merged.Add(channel);
              }
              //_channelMapping.Remove(channel.Text);
              //_channelMapping.Add(channel.Text, channelMap);
            }
            UpdateMergedList(channelMap);
            UpdateList();
          }
        }
        else
        {
          e.Cancel = true;
          MessageBox.Show("Only one channel can be mapped to multiple channels at a time.", "Multiple Selection Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
      else
      {
        if (lvMapping.SelectedItems.Count == 1)
        {
          if (_channelMapping.ContainsKey(lvMapping.SelectedItems[0].Text))
          {
            if (_channelMapping[lvMapping.SelectedItems[0].Text].merged == null ||
                _channelMapping[lvMapping.SelectedItems[0].Text].merged.Count <= 1)
            {
              ChannelMap channelMap = _channelMapping[lvMapping.SelectedItems[0].Text];
              if (channelMap.merged != null)
              {
                if (channelMap.merged.Count > 0)
                {
                  channelMap.id = channelMap.merged[0].id;
                  channelMap.grabber = channelMap.merged[0].grabber;
                }
                channelMap.merged = null;
              }
              UpdateMergedList(channelMap);
              UpdateList();
            }
            else
            {
              e.Cancel = true;
              MessageBox.Show("Cannot convert multiple channels to single channel. Please remove one.",
                              "Multiple Channel Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
          }
        }
      }
    }
  }
}