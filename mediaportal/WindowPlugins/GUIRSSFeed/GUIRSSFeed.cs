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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Linq;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Services;
using Rss;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.RSS
{
  /// <summary>
  /// A My News (RSS Feed) Plugin for MediaPortal
  /// </summary>
  public class GUIRSSFeed : GUIInternalWindow
  {
    private class FeedDetails
    {
      public string m_site;
      public string m_link;
      public string m_title;
      public string m_description;
      public DateTime m_PublishTime;
      public string m_Author;
      public string m_Thumb;
    };

    private class Site
    {
      public string m_Name;
      public string m_URL;
      public string m_Image;
      public string m_Description;
      public string m_Encoding;
      public List<FeedDetails> m_feed_details = new List<FeedDetails>();
      public DateTime m_RefreshTime = DateTime.MinValue; //for autorefresh
      public DateTime m_PublishTime = DateTime.MinValue; //latest publish time of items 
      public int m_RSSRefreshPeriod = 15;
      public bool m_RSSAutoRefresh = true;

      public Site()
      {
      }

      public bool IsRefreshInDue
      {
        get
        {
          return (DateTime.Now - this.m_RefreshTime.AddMinutes(this.m_RSSRefreshPeriod)).TotalSeconds > -30;
        }
      }
    }
        

    private enum Controls
    {
      CONTROL_BACKGROUND = 1,
      CONTROL_BTNREFRESH = 2,
      CONTROL_BTNCHANNEL = 4,
      CONTROL_LABELUPDATED = 11,
      CONTROL_LABELCHANNEL = 12,
      CONTROL_IMAGELOGO = 101,
      CONTROL_LIST = 50,
      CONTROL_FEEDTITLE = 500,
      CONTROL_STORY1 = 501,
      CONTROL_STORY2 = 502,
      CONTROL_STORY3 = 503,
      CONTROL_STORY4 = 504,
      CONTROL_STORY5 = 505,
      CONTROL_STORYTEXT = 506
    }

    public static int WINDOW_RSS = 2700;

    public static string DEFAULT_NEWS_ICON = "news.png";

    private const int NUM_STORIES = 100;
    private readonly List<Site> m_sites = new List<Site>();
    private Site m_SelectedSite = null;
    private GUIImage m_pSiteImage = null;
    private System.Timers.Timer m_RefreshTimer = null;

    public GUIRSSFeed()
    {
      GetID = WINDOW_RSS;
    }

    public override bool Init()
    {
      LoadSettings();
      InitRefreshTimer();
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\myrss.xml"));
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          break;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          break;

        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_LIST)
            {
              UpdateDetails();
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_BTNREFRESH)
            {
              UpdateNews(m_SelectedSite, true, true);
              UpdateGUI();
            }

            if (iControl == (int)Controls.CONTROL_LIST)
            {
              string story = DownloadMainStory();

              if (story != null)
              {
                GUIDialogText dlg = (GUIDialogText)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_TEXT);
                if (dlg != null)
                {
                  dlg.Reset();
                  dlg.ResetAllControls();
                  dlg.SetHeading("Story");
                  dlg.SetText(story);
                  dlg.DoModal(GetID);
                }
              }
            }

            if (iControl == (int)Controls.CONTROL_BTNCHANNEL)
            {
              OnSelectFeed();

              return true;
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    public override void DeInit()
    {
      if (m_RefreshTimer != null)
      {
        m_RefreshTimer.Stop();
        m_RefreshTimer.Dispose();
        m_RefreshTimer = null;
      }
      SaveSettings();
      base.DeInit();
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      string strLoadParam = this._loadParameterObject is NotifyMessageServiceEventArgs args ? args.Message.PluginArguments : this._loadParameter;
      Log.Debug("[GUIRSSFeed][OnPageLoad] LoadParameter: '{0}'", strLoadParam);

      if (!string.IsNullOrWhiteSpace(strLoadParam))
      {
        NameValueCollection prms = HttpUtility.ParseQueryString(strLoadParam);
        string strSite = prms["site"];
        string strItem = prms["item"];
        if (strSite != null && strItem != null)
        {
          Site site = m_sites.Find(p => p.m_Name == strSite);
          if (site != null)
          {
            if (site != m_SelectedSite)
              m_SelectedSite = site;

            if (site.IsRefreshInDue)
              UpdateNews(site, true, true);

            m_pSiteImage = (GUIImage)GetControl((int)Controls.CONTROL_IMAGELOGO);
            UpdateGUI();

            GUIListControl guiList = (GUIListControl)this.GetControl((int)Controls.CONTROL_LIST);
            if (guiList != null)
            {
              for (int i = 0; i < guiList.Count; i++)
              {
                GUIListItem guiItem = guiList[i];
                if (guiItem.Label == strItem)
                {
                  guiList.SelectedListItemIndex = i;
                  UpdateDetails();
                  return;
                }
              }
            }
          }
        }
      }

      m_pSiteImage = (GUIImage)GetControl((int)Controls.CONTROL_IMAGELOGO);
      if (m_SelectedSite != null && m_SelectedSite.IsRefreshInDue)
        UpdateNews(m_SelectedSite, true, true);

      UpdateGUI();
    }

    private void OnSelectFeed()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(496); // menu

      for (int i = 0; i < m_sites.Count; i++)
        dlg.Add(m_sites[i].m_Name);

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
        return;

      int nSelected = dlg.SelectedLabel;

      m_SelectedSite = m_sites[nSelected];

      if (m_SelectedSite.IsRefreshInDue)
        UpdateNews(m_SelectedSite, true, true);

      UpdateGUI();
    }

    private void UpdateNews(bool bForce)
    {
      for (int i = 0; i < m_sites.Count; i++)
      {
        Site site = m_sites[i];
        if (bForce || (site.m_RSSAutoRefresh && site.IsRefreshInDue))
          UpdateNews(site, false, false);
      }

      GUIWindowManager.SendThreadCallback((int param1, int param2, object data) =>
      {
        UpdateGUI();
        return 0;
      }, 0, 0, null);

    }
    private void UpdateNews(Site site, bool bShowWarning, bool bShowProgress)
    {
      if (site == null)
        return;

      lock (site)
      {

        GUIDialogProgress dlgProgress = bShowProgress ?
          (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS) : null;

        try
        {
          if (!site.m_URL.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
              !site.m_URL.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
              !site.m_URL.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
          {
            site.m_URL = "http://" + site.m_URL;
          }

          long lStartTime = Environment.TickCount;
          if (bShowProgress)
          {
            dlgProgress.Reset();
            dlgProgress.SetHeading(704);
            dlgProgress.SetLine(1, GUILocalizeStrings.Get(705) + " " + site.m_Name);
            dlgProgress.SetLine(2, string.Empty);
            dlgProgress.SetLine(3, string.Empty);
            dlgProgress.ShowProgressBar(false);
            dlgProgress.StartModal(GetID);
            dlgProgress.Progress();
          }

          Download(site);
          CheckForNewItems(site);

          if (bShowProgress)
          {
            // Leave dialog on screen for minimum of 1 seconds
            // to eliminate the horrible flash of dialog before user can reed it
            long lDiff = Environment.TickCount - lStartTime;
            if (lDiff < 1000)
              Thread.Sleep((int)(1000 - lDiff));

            dlgProgress.Close();
          }
        }
        catch (Exception e)
        {
          dlgProgress?.Close();
          if (bShowWarning)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0)
            {
              Param1 = 9, //my news
              Param2 = 912, //Unable to download latest news
              Param3 = 0,
              Label3 = site.m_URL
            };
            GUIWindowManager.SendMessage(msg);
            Log.Error(e);
          }
        }
      }
    }

    private void CheckForNewItems(Site site)
    {
      DateTime dtPublish = DateTime.MinValue;
      INotifyMessageService srvc = GlobalServiceProvider.Get<INotifyMessageService>();
      for (int i = 0; i < site.m_feed_details.Count; i++)
      {
        FeedDetails det = site.m_feed_details[i];
        if (det == null)
          break;

        if (det.m_PublishTime > DateTime.MinValue && det.m_PublishTime > site.m_PublishTime)
        {
          if (det.m_PublishTime > dtPublish)
            dtPublish = det.m_PublishTime;

          srvc.MessageRegister(
            det.m_title,
            "RSS news",
            WINDOW_RSS,
            det.m_PublishTime,
            out _,
            strDescription: det.m_description,
            strOriginLogo: DEFAULT_NEWS_ICON,
            strThumb: det.m_Thumb,
            strAuthor: det.m_Author,
            strPluginArgs: "site=" + HttpUtility.UrlEncode(site.m_Name) + "&item=" + HttpUtility.UrlEncode(det.m_title),
            level: NotifyMessageLevelEnum.Information,
            cls: NotifyMessageClassEnum.News
            );
        }
      }

      if (dtPublish != DateTime.MinValue)
        site.m_PublishTime = dtPublish;
    }

    #region Serialisation

    private void LoadSettings()
    {
      m_SelectedSite = null;
      m_sites.Clear();
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        for (int i = 0; i < NUM_STORIES; i++)
        {
          string strName = xmlreader.GetValueAsString("rss", "siteName" + i, string.Empty);
          string strURL = xmlreader.GetValueAsString("rss", "siteURL" + i, string.Empty);
          string strEncoding = xmlreader.GetValueAsString("rss", "siteEncoding" + i, "windows-1252");
          string strImage = xmlreader.GetValueAsString("rss", "siteImage" + i, DEFAULT_NEWS_ICON);
          string strDescription = xmlreader.GetValueAsString("rss", "siteDescription" + i, string.Empty);
          int iRefreshPeriod = xmlreader.GetValueAsInt("rss", "siteRefreshPeriod" + i, 15);
          bool bAutoRefresh = xmlreader.GetValueAsInt("rss", "siteAutoRefresh" + i, 0) != 0;
          if (!DateTime.TryParse(xmlreader.GetValueAsString("rss", "sitePublishTime" + i, string.Empty), out DateTime dtPublishTime))
            dtPublishTime = DateTime.MinValue;

          if (strName.Length > 0 && strURL.Length > 0)
          {
            if (strImage.Length == 0)
              strImage = DEFAULT_NEWS_ICON;

            Site site = new Site
            {
              m_Name = strName,
              m_URL = strURL,
              m_Image = strImage,
              m_Description = strDescription,
              m_Encoding = strEncoding,
              m_RSSRefreshPeriod = iRefreshPeriod,
              m_RSSAutoRefresh = bAutoRefresh,
              m_PublishTime = dtPublishTime
            };

            if (string.IsNullOrWhiteSpace(site.m_Description))
              site.m_Description =  site.m_Name;

            if (m_SelectedSite == null)
              m_SelectedSite = site;

            m_sites.Add(site);
          }
        }
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        for (int i = 0; i < m_sites.Count; i++)
        {
          Site site = m_sites[i];
          xmlwriter.SetValue("rss", "sitePublishTime" + i, site.m_PublishTime.ToString());
        }
      }
    }

    #endregion

    public void RefreshFeeds()
    {
      m_sites.Clear();
      LoadSettings();
      UpdateNews(true);
    }

    private void InitRefreshTimer()
    {
      if (m_RefreshTimer == null)
      {
        m_RefreshTimer = new System.Timers.Timer();
        m_RefreshTimer.Elapsed += this.RefreshTimer_Elapsed;
        m_RefreshTimer.AutoReset = false;
      }
      
      DateTime dtNextRefresh = m_sites.Min(p => p.m_RSSAutoRefresh ? p.m_RefreshTime.AddMinutes(p.m_RSSRefreshPeriod) : DateTime.MaxValue);
      if (dtNextRefresh < DateTime.MaxValue)
      {
        m_RefreshTimer.Interval = Math.Max(60000d, (dtNextRefresh - DateTime.Now).TotalMilliseconds);
        m_RefreshTimer.Start();
        Log.Debug("[GUIRSSFeed][InitRefreshTimer] Next refresh in {0}s", m_RefreshTimer.Interval / 1000);
      }
    }

    private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      UpdateNews(false);
      InitRefreshTimer();
    }

    private void UpdateGUI()
    {
      if (m_SelectedSite == null || this.GetControl((int)Controls.CONTROL_LIST) == null)
        return;

      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_BTNREFRESH, GUILocalizeStrings.Get(184)); //Refresh
      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LABELCHANNEL, m_SelectedSite.m_Description); //Channel name label
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(9) + @"/" + m_SelectedSite.m_Name);

      int posX = m_pSiteImage.XPosition;
      int posY = m_pSiteImage.YPosition;

      m_pSiteImage.SetPosition(posX, posY);
      m_pSiteImage.ColourDiffuse = 0xffffffff;
      m_pSiteImage.SetFileName(m_SelectedSite.m_Image);

      //			m_pSiteImage.Width = m_pSiteImage.TextureWidth;
      //			m_pSiteImage.Height = m_pSiteImage.TextureHeight;

      GUIControl.ClearControl(GetID, (int)Controls.CONTROL_LIST);

      int iTotalItems = 0;
      for (int i = 0; i < m_SelectedSite.m_feed_details.Count; i++)
      {
        FeedDetails det = m_SelectedSite.m_feed_details[i];
        if (det == null)
          break;

        if (det.m_title == string.Empty && det.m_description == string.Empty)
        {
          // Skip this empty item
          continue;
        }

        GUIListItem item = new GUIListItem
        {
          Label = det.m_title,
          IsFolder = false,
          MusicTag = det,
          ThumbnailImage = string.Empty,
          IconImage = "defaultMyNews.png"
        };

        GUIControl.AddListItemControl(GetID, (int)Controls.CONTROL_LIST, item);
        iTotalItems++;
      }

      //set object count label
      GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(iTotalItems));

      GUIControl.FocusControl(GetID, (int)Controls.CONTROL_LIST);

      GUIListItem selecteditem = GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST);
      if (selecteditem != null)
        GUIPropertyManager.SetProperty("#selecteditem", selecteditem.Label);

      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STORYTEXT, m_SelectedSite.m_feed_details.Count > 0 ? m_SelectedSite.m_feed_details[0].m_description : string.Empty);
    }

    private bool Download(Site site)
    {
      site.m_feed_details.Clear();

      try
      {
        RssFeed feed = RssFeed.Read(site.m_URL);
        RssChannel channel = feed.Channels[0];

        // Download the image from the feed if available/needed
        if (channel.Image != null && site.m_Image == DEFAULT_NEWS_ICON)
        {
          string strImage = channel.Image.Url.ToString();

          if (strImage.Length > 0)
            site.m_Image = strImage;
        }

        // Fill the items
        for (int iItem = 0; iItem < channel.Items.Count; iItem++)
        {
          RssItem item = channel.Items[iItem];

          // Check if there are HTML tags in the description, if so we need to "parse" the HTML
          // which is actually just stripping the tags.
          if (Regex.IsMatch(item.Description, @"<[^>]*>"))
          {
            // Strip \n's
            item.Description = Regex.Replace(item.Description, @"(\n|\r)", string.Empty, RegexOptions.Multiline);

            // Remove whitespace (double spaces)
            item.Description = Regex.Replace(item.Description, @"  +", string.Empty, RegexOptions.Multiline);

            // Replace <br/> with \n
            item.Description = Regex.Replace(item.Description, @"< *br */*>", "\n",
                                             RegexOptions.IgnoreCase & RegexOptions.Multiline);

            // Remove remaining HTML tags
            item.Description = Regex.Replace(item.Description, @"<[^>]*>", string.Empty, RegexOptions.Multiline);
          }

          // Strip newlines from titles because it looks funny to see multiple lines in the listbox
          item.Title = Regex.Replace(item.Title, @" *(\n|\r)+ *", " ", RegexOptions.Multiline);

          if (item.Description == string.Empty)
            item.Description = item.Title;
          else
            item.Description = item.Title + ": \n\n" + item.Description + "\n";

          string strImage = null;
          if (item.Image != null)
            strImage = item.Image.Url.ToString();

          FeedDetails det = new FeedDetails
          {
            m_site = channel.Title,
            m_title = HttpUtility.HtmlDecode(item.Title),
            m_description = HttpUtility.HtmlDecode(item.Description),
            m_link = item.Link.ToString(),
            m_Author = item.Author,
            m_PublishTime = item.PubDate,
            m_Thumb =  strImage?.Length > 0 ? strImage : site.m_Image
          };
          if (det.m_PublishTime <= DateTime.MinValue)
            det.m_PublishTime = DateTime.Now;

          site.m_feed_details.Add(det);

          if (site.m_feed_details.Count >= NUM_STORIES)
            break;
        }

        site.m_RefreshTime = DateTime.Now;
      }
      catch (WebException ex)
      {
        site.m_RefreshTime = DateTime.Now;
        throw ex;
      }

      return true;
    }

    private GUIListItem GetSelectedItem()
    {
      return GUIControl.GetSelectedListItem(GetID, (int)Controls.CONTROL_LIST);
    }

    private void UpdateDetails()
    {
      GUIListItem item = GetSelectedItem();
      if (item == null)
        return;

      GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_STORYTEXT, ((FeedDetails)item.MusicTag).m_description);
    }

    private string DownloadMainStory()
    {
      // Get the selected item
      GUIListItem item = GetSelectedItem();
      if (item == null || m_SelectedSite == null)
      {
        return null;
      }

      FeedDetails feed = (FeedDetails)item.MusicTag;

      // Download the story
      string text = null;
      try
      {
        string data = string.Empty;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(feed.m_link);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // request.Proxy = WebProxy.GetDefaultProxy();
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception ex)
        {
          Log.Error("GUIRSSFeed: DownloadMainStory {0}", ex.Message);
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        try
        {
          using (Stream stream = response.GetResponseStream())
          {
            Encoding enc;
            try
            {
              enc = Encoding.GetEncoding(response.ContentEncoding);
            }
            catch (Exception ex)
            {
              Log.Error("GUIRSSFeed: DownloadMainStory {0}", ex.Message);
              // Using Default Encoding
              enc = Encoding.GetEncoding(m_SelectedSite.m_Encoding);
            }
            using (StreamReader r = new StreamReader(stream, enc))
            {
              data = r.ReadToEnd();
            }
          }
          // Convert html to text
          HtmlToText html = new HtmlToText(data);
          text = html.ToString().Trim();
        }
        finally
        {
          if (response != null)
          {
            response.Close();
          }
        }
      }
      catch (Exception ex)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0)
        {
          Param1 = 9, //my news
          Param2 = 912, //Unable to download latest news
          Param3 = 0
        };

        string errtxt = ex.Message;
        int pos = errtxt.IndexOf(":");
        if (pos != -1)
        {
          errtxt = errtxt.Substring(pos + 1);
        }
        msg.Label3 = string.Format("{0}\n\n({1})", m_SelectedSite.m_URL, errtxt);
        GUIWindowManager.SendMessage(msg);

        // Log exception
        Log.Info("ex:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }

      return text;
    }
  }
}