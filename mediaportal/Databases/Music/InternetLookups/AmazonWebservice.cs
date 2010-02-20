#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
using BassRegistration;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using Image = System.Drawing.Image;

namespace MediaPortal.Music.Amazon
{
  public class AmazonWebservice
  {
    #region delegates

    public delegate void FindCoverArtProgressHandler(AmazonWebservice aws, int progressPercent);

    public event FindCoverArtProgressHandler FindCoverArtProgress;

    public delegate void FindCoverArtDoneHandler(AmazonWebservice aws, EventArgs e);

    public event FindCoverArtDoneHandler FindCoverArtDone;

    #endregion

    #region Variables

    private int _MaxSearchResultItems = 8; // The max number of matching results we want to grab (-1 = unlimited)
    private bool _AbortGrab = false;

    private string _ArtistName = string.Empty;
    private string _AlbumName = string.Empty;

    private const string itemSearch =
      "&Operation=ItemSearch&Artist={0}&Title={1}&SearchIndex=Music&ResponseGroup=Images,ItemAttributes,Tracks";

    protected List<AlbumInfo> _AlbumInfoList = new List<AlbumInfo>();

    #endregion

    #region Properties

    public int MaxSearchResultItems
    {
      get { return _MaxSearchResultItems; }
      set
      {
        if (value < -1)
        {
          value = -1;
        }

        _MaxSearchResultItems = value;
      }
    }

    public List<AlbumInfo> AlbumInfoList
    {
      get { return _AlbumInfoList; }
    }

    public bool HasAlbums
    {
      get { return _AlbumInfoList.Count > 0; }
    }

    public int AlbumCount
    {
      get { return _AlbumInfoList.Count; }
    }

    public string ArtistName
    {
      get { return _ArtistName; }
      set { _ArtistName = value; }
    }

    public string AlbumName
    {
      get { return _AlbumName; }
      set { _AlbumName = value; }
    }

    public bool AbortGrab
    {
      get { return _AbortGrab; }
      set { _AbortGrab = value; }
    }

    #endregion

    #region ctor

    public AmazonWebservice() {}

    public AmazonWebservice(string artistName, string albumName) : this()
    {
      _ArtistName = artistName;
      _AlbumName = albumName;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start the Grabber
    /// </summary>
    public void GetAlbumInfoAsync()
    {
      _AbortGrab = false;

      ThreadStart threadStart = new ThreadStart(InternalGetAlbumInfo);
      Thread albumGrabberThread = new Thread(threadStart);
      albumGrabberThread.IsBackground = true;
      albumGrabberThread.Name = "AmazonGrabber";
      albumGrabberThread.Start();
    }

    /// <summary>
    /// Return the Image from the given URL
    /// </summary>
    /// <param name="sURL"></param>
    /// <returns></returns>
    public static Image GetImageFromURL(string sURL)
    {
      if (sURL.Length == 0)
      {
        return null;
      }

      Image img = null;

      try
      {
        WebRequest webReq = null;
        webReq = WebRequest.Create(sURL);
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // wr.Proxy = WebProxy.GetDefaultProxy();
          webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) {}
        WebResponse webResp = webReq.GetResponse();
        img = Image.FromStream(webResp.GetResponseStream());
      }

      catch {}

      return img;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Invoke the ALbum Retrieval
    /// </summary>
    private void InternalGetAlbumInfo()
    {
      GetAlbumInfo();
    }

    /// <summary>
    /// Check, if the application is termanting and stop grabber then
    /// </summary>
    private void CheckForAppShutdown()
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
      {
        _AbortGrab = true;
      }
    }

    /// <summary>
    /// Retrieve Album Info using Amazon Web Services
    /// </summary>
    /// <returns></returns>
    public bool GetAlbumInfo()
    {
      _AbortGrab = false;
      _AlbumInfoList.Clear();
      bool result = true;

      try
      {
        if (_ArtistName.Length == 0 && _AlbumName.Length == 0)
        {
          return false;
        }

        DateTime startTime = DateTime.Now;

        // Build up a valid request
        BassRegistration.SignedRequestHelper helper = new SignedRequestHelper("com");
        // Use "US" site for cover art search
        string requestString =
          helper.Sign(string.Format(itemSearch, System.Web.HttpUtility.UrlEncode(_ArtistName),
                                    System.Web.HttpUtility.UrlEncode(_AlbumName)));

        // Connect to AWS
        HttpWebRequest request = null;
        try
        {
          request = (HttpWebRequest)WebRequest.Create(requestString);
          try
          {
            // Use the current user in case an NTLM Proxy or similar is used.
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;
          }
          catch (Exception) {}
        }
        catch (Exception e)
        {
          Log.Error("Cover Art grabber: Create request failed:  {0}", e.Message);
          return false;
        }

        // Get Response from AWS
        HttpWebResponse response = null;
        string responseXml = null;
        try
        {
          response = (HttpWebResponse)request.GetResponse();
          using (Stream responseStream = response.GetResponseStream())
          {
            using (StreamReader reader = new StreamReader(responseStream))
            {
              responseXml = reader.ReadToEnd();
            }
          }
        }
        catch (Exception e)
        {
          Log.Error("Cover Art grabber: Get Response failed:  {0}", e.Message);
          return false;
        }

        if (responseXml == null)
        {
          Log.Debug("Cover Art grabber: No data found for: {0} - {1}", _ArtistName, _AlbumName);
          return false;
        }

        XmlDocument xml = new XmlDocument();
        xml.LoadXml(responseXml);
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(xml.NameTable);
        nsMgr.AddNamespace("ns", "http://webservices.amazon.com/AWSECommerceService/2005-10-05");

        XmlNodeList nodes = xml.SelectNodes("/ns:ItemSearchResponse/ns:Items/ns:Item", nsMgr);
        if (nodes.Count == 0)
        {
          Log.Debug("Cover Art grabber: No data found for: {0} - {1}", _ArtistName, _AlbumName);
          return false;
        }

        int imgCount = 0;
        int totResults = nodes.Count;
        bool resultsLimitExceeded = false;

        Log.Info("Cover Art grabber: AWS Response Returned {1} total results.", totResults);

        // Now loop through all the items and extract the data
        foreach (XmlNode node in nodes)
        {
          // Yield thread...
          Thread.Sleep(1);
          GUIWindowManager.Process();

          CheckForAppShutdown();

          if (resultsLimitExceeded || _AbortGrab)
          {
            break;
          }

          if (_MaxSearchResultItems != -1 && imgCount >= _MaxSearchResultItems)
          {
            resultsLimitExceeded = true;
            break;
          }

          AlbumInfo albumInfo = FillAlbum(node);
          _AlbumInfoList.Add(albumInfo);
          ++imgCount;
          DoProgressUpdate(imgCount, totResults);
        }

        string resultsText = "";

        if (_AbortGrab)
        {
          resultsText =
            string.Format("AWS album cover art grab aborted by user before completetion. Retreived {0}/{1} records",
                          imgCount, totResults);
        }

        else if (resultsLimitExceeded)
        {
          resultsText = string.Format("AWS retreived {0}/{1} records (max search limit set to {2} images)", imgCount,
                                      totResults, _MaxSearchResultItems);
        }

        else
        {
          resultsText = string.Format("{0} records retrieved", imgCount);
        }

        DateTime stopTime = DateTime.Now;
        TimeSpan elapsedTime = stopTime - startTime;
        double totSeconds = elapsedTime.TotalSeconds;
        float secondsPerImage = (float)totSeconds / (float)imgCount;
        string et = "";

        if (imgCount > 0)
        {
          if (_AbortGrab)
          {
            et = string.Format("{0:d2}:{1:d2}:{2:d2}.{3:d3} ({4:f3} seconds per image)", elapsedTime.Hours,
                               elapsedTime.Minutes, elapsedTime.Seconds, elapsedTime.Milliseconds, secondsPerImage);
          }

          else
          {
            et = string.Format("in {0:d2}:{1:d2}:{2:d2}.{3:d3} ({4:f3} seconds per image)", elapsedTime.Hours,
                               elapsedTime.Minutes, elapsedTime.Seconds, elapsedTime.Milliseconds, secondsPerImage);
          }

          Log.Info("Cover art grabber:{0} {1}", resultsText, et);
        }
      }

      catch (Exception ex)
      {
        //string errMsg = string.Format("GetAlbumInfoAsync caused an exception: {0}\r\n{1}\r\n", ex.Message, ex.StackTrace);
        Log.Info("Cover art grabber exception:{0}", ex.ToString());
        result = false;
      }

      //_GrabberRunning = false;

      if (FindCoverArtDone != null)
      {
        FindCoverArtDone(this, EventArgs.Empty);
      }

      return result;
    }

    private void DoProgressUpdate(int imgCount, int itemCount)
    {
      if (FindCoverArtProgress != null)
      {
        int totalCovers = itemCount;

        if (_MaxSearchResultItems > 0)
        {
          totalCovers = Math.Min(itemCount, _MaxSearchResultItems);
        }

        int progressPercent = (int)(((float)imgCount / (float)totalCovers) * 100f);
        FindCoverArtProgress(this, progressPercent);
      }
    }

    private AlbumInfo FillAlbum(XmlNode node)
    {
      AlbumInfo album = new AlbumInfo();
      string largeImageUrl = null;
      string mediumImageUrl = null;
      string smallImageUrl = null;

      foreach (XmlNode childNode in node)
      {
        if (childNode.Name == "ASIN")
          album.Asin = childNode.InnerText;

        if (childNode.Name == "SmallImage")
        {
          smallImageUrl = GetNode(childNode, "URL");
        }

        if (childNode.Name == "MediumImage")
        {
          mediumImageUrl = GetNode(childNode, "URL");
        }

        if (childNode.Name == "LargeImage")
        {
          largeImageUrl = GetNode(childNode, "URL");
        }

        if (childNode.Name == "ItemAttributes")
        {
          foreach (XmlNode attributeNode in childNode)
          {
            if (attributeNode.Name == "Artist")
              album.Artist = attributeNode.InnerText;

            if (attributeNode.Name == "Title")
              album.Album = attributeNode.InnerText;

            if (attributeNode.Name == "ReleaseDate")
            {
              int releaseYear = 0;

              try
              {
                releaseYear = DateTime.Parse(attributeNode.InnerText).Year;
              }

              catch
              {
                // do nothing
              }
              album.Year = releaseYear;
            }
          }
        }

        if (childNode.Name == "Tracks")
        {
          // The node starts with a "<Disc Number Node" , we want all subnodes of it
          string tracks = "";

          foreach (XmlNode discNode in childNode.ChildNodes)
          {
            foreach (XmlNode trackNode in discNode)
            {
              tracks += string.Format("{0}@{1}@{2}|", Convert.ToInt32(trackNode.Attributes["Number"].Value),
                                      trackNode.InnerText, 99);
            }
          }
          album.Tracks = tracks.Trim(new char[] {'|'}).Trim();
        }
      }

      album.Image = largeImageUrl;
      if (album.Image == null)
      {
        album.Image = mediumImageUrl ?? smallImageUrl;
      }

      return album;
    }

    /// <summary>
    /// Get the Url node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private string GetNode(XmlNode node, string nodeString)
    {
      foreach (XmlNode child in node.ChildNodes)
      {
        if (child.Name == nodeString)
          return child.InnerText;
      }
      return "";
    }

    #endregion
  }
}