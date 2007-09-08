using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.WebEPG.Config.Grabber;
using System.Xml.Serialization;
using System.IO;
using MediaPortal.Services;
using MediaPortal.Utils.Web;
using MediaPortal.Utils.Time;
using MediaPortal.EPG;
using MediaPortal.WebEPG;
using MediaPortal.TV.Database;
using MediaPortal.EPG.config;

namespace MediaPortal.EPG.WebEPGTester
{
  public partial class fTester : Form
  {
    private TreeNode tGrabbers;
    private Hashtable hChannelConfigInfo;
    private Hashtable hGrabberConfigInfo;
    private SortedList CountryList;
    private List<TreeNode> _testList;
    ServiceProvider _services;
    private ILog _log;
    StringBuilder _sb;
    StringWriter _logString;
    string _webepgDir;

    private enum Status
    {
      unknown = 0,
      working = 1,
      ok = 2,
      warning = 3,
      error = 4
    }

    #region Constructor
    public fTester()
    {
      InitializeComponent();
      this.tvGrabbers.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvGrabbers_Checkbox);

      SetWebepgDir();

      _services = GlobalServiceProvider.Instance;
      _log = new TestLog("WebEPG-Tester", Level.Debug);

      _sb = new StringBuilder();
      _logString = new StringWriter(_sb);
      ILog webepgLog = new TestLog(_logString, Level.Debug);
      _services.Add<ILog>(webepgLog);
    }
    #endregion

    #region Private Methods
    private void GetTestList(TreeNode node)
    {
      if (node.Nodes.Count == 0)
      {
        if (node.Checked)
        {
          //TestChannel testInfo = new TestChannel();
          //testInfo.status = Status.ok;
          //testInfo.channelId = node.Text;
          //testInfo.grabber = node.Parent.Text;
          _testList.Add(node);
        }
      }
      else
      {
        for (int i = 0; i < node.Nodes.Count; i++)
          GetTestList(node.Nodes[i]);
      }
    }

    private long GetLongDateTime(DateTime dt)
    {
      long lDatetime;

      lDatetime = dt.Year;
      lDatetime *= 100;
      lDatetime += dt.Month;
      lDatetime *= 100;
      lDatetime += dt.Day;
      lDatetime *= 100;
      lDatetime += dt.Hour;
      lDatetime *= 100;
      lDatetime += dt.Minute;
      lDatetime *= 100;
      // no seconds

      return lDatetime;
    }

    private DateTime GetDateTime(long ldatetime)
    {
      int sec = (int)(ldatetime % 100L); ldatetime /= 100L;
      int minute = (int)(ldatetime % 100L); ldatetime /= 100L;
      int hour = (int)(ldatetime % 100L); ldatetime /= 100L;
      int day = (int)(ldatetime % 100L); ldatetime /= 100L;
      int month = (int)(ldatetime % 100L); ldatetime /= 100L;
      int year = (int)ldatetime;

      if (day < 0 || day > 31) throw new ArgumentOutOfRangeException();
      if (month < 0 || month > 12) throw new ArgumentOutOfRangeException();
      if (year < 1900 || year > 2100) throw new ArgumentOutOfRangeException();
      if (sec < 0 || sec > 59) throw new ArgumentOutOfRangeException();
      if (minute < 0 || minute > 59) throw new ArgumentOutOfRangeException();
      if (hour < 0 || hour > 23) throw new ArgumentOutOfRangeException();

      return new DateTime(year, month, day, hour, minute, 0, 0);
    }

    private void CheckChildren(bool check, TreeNode node)
    {
      node.Checked = check;

      for (int i = 0; i < node.Nodes.Count; i++)
        CheckChildren(check, node.Nodes[i]);
    }

    private void LoadConfig(string startDirectory)
    {
      //_log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Loading Grabbers");
      hChannelConfigInfo = new Hashtable();
      hGrabberConfigInfo = new Hashtable();
      CountryList = new SortedList();
      tGrabbers = new TreeNode("All Sites/All Channels");
      if (System.IO.Directory.Exists(startDirectory + "\\Grabbers"))
        GetTreeGrabbers(ref tGrabbers, startDirectory + "\\Grabbers");
      //else
      //_log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Cannot find grabbers directory");


      IDictionaryEnumerator Enumerator = hChannelConfigInfo.GetEnumerator();
      while (Enumerator.MoveNext())
      {
        ChannelConfigInfo info = (ChannelConfigInfo)Enumerator.Value;
        if (info.ChannelID != null && info.FullName != null)
        {
          if (info.GrabberList != null)
          {
            IDictionaryEnumerator grabEnum = info.GrabberList.GetEnumerator();
            while (grabEnum.MoveNext())
            {
              GrabberConfigInfo gInfo = (GrabberConfigInfo)grabEnum.Value;
              SortedList chList = (SortedList)CountryList[gInfo.Country];
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

    private void GetTreeGrabbers(ref TreeNode Main, string Location)
    {
      System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      System.IO.DirectoryInfo[] dirList = dir.GetDirectories();
      if (dirList.Length > 0)
      {
        if (dirList.Length == 1)
        {
          System.IO.DirectoryInfo g = dirList[0];
          if (g.Name == ".svn")
            GetGrabbers(ref Main, Location);
        }
        else
        {
          for (int i = 0; i < dirList.Length; i++)
          {
            //LOAD FOLDERS
            System.IO.DirectoryInfo g = dirList[i];
            TreeNode MainNext = new TreeNode(g.Name);
            GetTreeGrabbers(ref MainNext, g.FullName);
            Main.Nodes.Add(MainNext);
            //MainNext.Tag = (g.FullName);
          }
        }
      }
      else
      {
        GetGrabbers(ref Main, Location);
      }

    }

    private void GetGrabbers(ref TreeNode Main, string Location)
    {
      System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      //_log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Directory: {0}", Location);
      GrabberConfigInfo gInfo;
      foreach (System.IO.FileInfo file in dir.GetFiles("*.xml"))
      {
        gInfo = new GrabberConfigInfo();
        //XmlDocument xml = new XmlDocument();
        GrabberConfigFile grabberXml;
        try
        {
          //_log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File: {0}", file.Name);

          XmlSerializer s = new XmlSerializer(typeof(GrabberConfigFile));
          TextReader r = new StreamReader(file.FullName);
          grabberXml = (GrabberConfigFile)s.Deserialize(r);
        }
        catch (Exception)
        {
          //_log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File open failed - XML error");
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
          CountryList.Add(file.Directory.Name, new SortedList());

        TreeNode gNode = new TreeNode(GrabberSite);
        Main.Nodes.Add(gNode);
        //XmlNode cl=sectionList.Attributes.GetNamedItem("ChannelList");

        foreach (ChannelInfo channel in grabberXml.Channels)
        {
          if (channel.id != null)
          {
            ChannelConfigInfo info = (ChannelConfigInfo)hChannelConfigInfo[channel.id];
            if (info != null) // && info.GrabberList[gInfo.GrabberID] != null)
            {
              TreeNode tNode = new TreeNode(info.FullName);
              string[] tag = new string[2];
              tag[0] = info.ChannelID;
              tag[1] = gInfo.GrabberID;
              tNode.Tag = tag;
              gNode.Nodes.Add(tNode);
              if (info.GrabberList == null)
                info.GrabberList = new SortedList();
              if (info.GrabberList[gInfo.GrabberID] == null)
                info.GrabberList.Add(gInfo.GrabberID, gInfo);
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

    private void UpdateImage(Status newStatus, TreeNode node)
    {
      if (node.ImageIndex < (int)newStatus)
      {
        node.ImageIndex = (int)newStatus;
        node.SelectedImageIndex = (int)newStatus;
      }

      if (node.Parent != null)
        UpdateImage(newStatus, node.Parent);
    }

    private void ResetImages(TreeNode node)
    {
      node.ImageIndex = 0;
      node.SelectedImageIndex = 0;

      for (int i = 0; i < node.Nodes.Count; i++)
        ResetImages(node.Nodes[i]);
    }

    private void SetWebepgDir()
    {
      if (tbGrabberDir.Text == string.Empty)
      {
        _webepgDir = Environment.CurrentDirectory + "\\WebEPG";
      }
      else
      {
        _webepgDir = tbGrabberDir.Text;
      }

      tvGrabbers.Nodes.Clear();
      tvGrabbers.Refresh();

      LoadConfig(_webepgDir);

      tvGrabbers.Nodes.Add((TreeNode)tGrabbers.Clone());
      tvGrabbers.Refresh();
    }
    #endregion

    #region Events
    private void tvGrabbers_Checkbox(object sender, TreeViewEventArgs e)
    {
      this.tvGrabbers.AfterCheck -= new System.Windows.Forms.TreeViewEventHandler(this.tvGrabbers_Checkbox);
      CheckChildren(e.Node.Checked, e.Node);
      this.tvGrabbers.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvGrabbers_Checkbox);
    }

    private void bRun_Click(object sender, EventArgs e)
    {
      ResetImages(tvGrabbers.Nodes[0]);
      _testList = new List<TreeNode>();
      GetTestList(tvGrabbers.Nodes[0]);

      ChannelsList config = new ChannelsList(_webepgDir);

      string testDir = Environment.CurrentDirectory + "\\test";

      MediaPortal.Webepg.Profile.Xml xmlreader = new MediaPortal.Webepg.Profile.Xml(testDir + "\\GrabberTests.xml");

      HTMLCache cache;
      if (GlobalServiceProvider.IsRegistered<IHtmlCache>())
      {
        cache = (HTMLCache)GlobalServiceProvider.Get<IHtmlCache>();
      }
      else
      {
        cache = new HTMLCache();
        cache.WebCacheInitialise();
        GlobalServiceProvider.Add<IHtmlCache>(cache);
      }

      if (!System.IO.Directory.Exists(testDir))
        System.IO.Directory.CreateDirectory(testDir);

      string[] countries = config.GetCountries();

      XMLTVExport xmltv = null;

      for (int c = 0; c < _testList.Count; c++)
      {
        if (_testList[c].Parent == null || _testList[c].Parent.Parent == null)
          continue;

        string channelId = _testList[c].Text;
        string grabber = _testList[c].Parent.Text;
        string country = _testList[c].Parent.Parent.Text;

        string countryDir = testDir + "\\" + country;
        string grabberDir = countryDir + "\\" + grabber;
        string channelDir = grabberDir + "\\" + channelId;

        if (!System.IO.Directory.Exists(countryDir))
          System.IO.Directory.CreateDirectory(countryDir);

        if (!System.IO.Directory.Exists(grabberDir))
          System.IO.Directory.CreateDirectory(grabberDir);

        if (!System.IO.Directory.Exists(channelDir))
          System.IO.Directory.CreateDirectory(channelDir);

        WebListingGrabber m_EPGGrabber = new WebListingGrabber(_webepgDir + "\\grabbers\\");

        _log.Info("WebEPG: Grabber {0}\\{1}", country, grabber);

        xmltv = new XMLTVExport(channelDir);
        xmltv.Open();

        FileInfo grabberLogFile = new FileInfo(channelDir + "\\grab.log");
        if (grabberLogFile.Exists)
          grabberLogFile.Delete();

        UpdateImage(Status.working, _testList[c]);
        tvGrabbers.SelectedNode = _testList[c];
        tvGrabbers.Focus();
        tvGrabbers.Refresh();

        xmltv.WriteChannel(channelId, channelId);
        _log.Info("WebEPG: Getting Channel {0}", channelId);

        string countryGrabber = country + "\\" + grabber.Replace('.', '_') + ".xml";
        string grabTimeStr = xmlreader.GetValueAsString("Grabbers", countryGrabber, "");
        DateTime grabDateTime;

        if (grabTimeStr == "")
        {
          grabDateTime = DateTime.Now;
          long dtLong = GetLongDateTime(grabDateTime);
          xmlreader.SetValue("Grabbers", countryGrabber, dtLong.ToString());
          xmlreader.Save();
          cache.CacheMode = HTMLCache.Mode.Replace;
        }
        else
        {
          grabDateTime = GetDateTime(long.Parse(grabTimeStr));
          cache.CacheMode = HTMLCache.Mode.Enabled;
        }

        if (!cbCache.Checked)
          cache.CacheMode = HTMLCache.Mode.Disabled;

        if (m_EPGGrabber.Initalise(countryGrabber, (int)numDays.Value))
        {
          List<TVProgram> programs = m_EPGGrabber.GetGuide(channelId, false, new TimeRange("00:00", "23:00"), grabDateTime);
          if (programs != null)
          {
            for (int p = 0; p < programs.Count; p++)
            {
              xmltv.WriteProgram(programs[p], "Tester", false);
            }

          }
        }
        else
        {
          _log.Error("WebEPG: Grabber failed for: {0}", channelId);
          UpdateImage(Status.error, _testList[c]);
        }

        if (xmltv != null)
          xmltv.Close();

        UpdateImage(Status.ok, _testList[c]);

        if (_logString.ToString().IndexOf("[ERROR]") != -1)
        {
          _log.Error("WebEPG: Grabber error for: {0}", channelId);
          UpdateImage(Status.error, _testList[c]);
        }
        if (_logString.ToString().IndexOf("[Warn.]") != -1)
        {
          _log.Warn("WebEPG: Grabber warning for: {0}", channelId);
          UpdateImage(Status.warning, _testList[c]);
        }

        // Write log to disk
        _logString.Flush();
        TextWriter grabberLog = new StreamWriter(grabberLogFile.FullName);
        grabberLog.Write(_sb.ToString());
        grabberLog.Flush();

        tvGrabbers.Refresh();

        _sb.Remove(0, _sb.Length);
      }
    }

    private void bScan_Click(object sender, EventArgs e)
    {
      SetWebepgDir();
    }

    private void tvGrabbers_AfterSelect(object sender, TreeViewEventArgs e)
    {
      tbLog.Text = string.Empty;

      if (e.Node.Nodes.Count > 0 || e.Node.Parent == null || e.Node.Parent.Parent == null)
        return;

      string channelId = e.Node.Text;
      string grabber = e.Node.Parent.Text;
      string country = e.Node.Parent.Parent.Text;

      string testDir = Environment.CurrentDirectory + "\\test";
      string countryDir = testDir + "\\" + country;
      string grabberDir = countryDir + "\\" + grabber;
      string channelDir = grabberDir + "\\" + channelId;

      if (!System.IO.Directory.Exists(countryDir))
        return;

      if (!System.IO.Directory.Exists(grabberDir))
        return;

      if (!System.IO.Directory.Exists(channelDir))
        return;

      FileInfo grabberLogFile = new FileInfo(channelDir + "\\grab.log");
      if (grabberLogFile.Exists)
      {
        try
        {
          FileStream file = grabberLogFile.OpenRead();
          TextReader reader = new StreamReader(file);

          tbLog.Text = reader.ReadToEnd();

          reader.Close();
          file.Close();
          file.Dispose();
        }
        catch (IOException)
        {
        }
      }
    }
    #endregion
  }
}