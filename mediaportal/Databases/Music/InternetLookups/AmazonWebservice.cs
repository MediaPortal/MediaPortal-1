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
using System.Collections.Generic;
using System.Net;
using System.Threading;
using AWS;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using Image=System.Drawing.Image;

namespace MediaPortal.Music.Amazon
{
  public class AmazonWebservice
  {
    public delegate void FindCoverArtProgressHandler(AmazonWebservice aws, int progressPercent);

    public event FindCoverArtProgressHandler FindCoverArtProgress;

    public delegate void FindCoverArtDoneHandler(AmazonWebservice aws, EventArgs e);

    public event FindCoverArtDoneHandler FindCoverArtDone;
    private const string _AWSAccessKeyID = "0XCDYPB7YGRYE8T6G302";

    private int _MaxSearchResultItems = 8; // The max number of matching results we want to grab (-1 = unlimited)
    private bool _AbortGrab = false;
    //private bool _GrabberRunning = false;

    private string _ArtistName = string.Empty;
    private string _AlbumName = string.Empty;

    protected List<AlbumInfo> _AlbumInfoList = new List<AlbumInfo>();

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

    public AmazonWebservice()
    {
    }

    public AmazonWebservice(string artistName, string albumName) : this()
    {
      _ArtistName = artistName;
      _AlbumName = albumName;
    }

    public void GetAlbumInfoAsync()
    {
      //_GrabberRunning = true;
      _AbortGrab = false;

      ThreadStart threadStart = new ThreadStart(InternalGetAlbumInfo);
      Thread albumGrabberThread = new Thread(threadStart);
      albumGrabberThread.IsBackground = true;
      albumGrabberThread.Name = "AmazonGrabber";
      albumGrabberThread.Start();
    }

    private void InternalGetAlbumInfo()
    {
      GetAlbumInfo();
    }

    private void CheckForAppShutdown()
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
      {
        _AbortGrab = true;
      }
    }

    public bool GetAlbumInfo()
    {
      _AbortGrab = false;
      _AlbumInfoList.Clear();
      bool result = true;

      try
      {
        //bool validateFailed = false;

        if (_ArtistName.Length == 0 && _AlbumName.Length == 0)
        {
          return false;
        }
        ;

        DateTime startTime = DateTime.Now;

        AWSECommerceService service = new AWSECommerceService();
        ItemSearchRequest isr = new ItemSearchRequest();

        isr.Artist = _ArtistName;
        isr.Title = _AlbumName;
        isr.SearchIndex = "Music";

        string[] responseGroupString = new string[] {"Images", "Tracks", "ItemAttributes", "Large"};
        isr.ResponseGroup = responseGroupString;

        ItemSearch search = new ItemSearch();
        search.AWSAccessKeyId = _AWSAccessKeyID;
        search.Request = new ItemSearchRequest[] {isr};
        ItemSearchResponse response = service.ItemSearch(search);

        int imgCount = 0;
        int totResults = 0;
        int totPages = 0;
        bool resultsLimitExceeded = false;

        if (response.Items.Length > 0)
        {
          totResults = int.Parse(response.Items[0].TotalResults);
          totPages = int.Parse(response.Items[0].TotalPages);

          Log.Info("Cover art grabber:AWS Response Returned {0} total pages with {1} total results.", totPages,
                   totResults);
        }

        //int grabPass = 0;
        string recordsFound = string.Format("Cover art grabber:{0} matching records found.  Retrieving records...",
                                            totResults);

        if (_MaxSearchResultItems > 0)
        {
          recordsFound = string.Format("Cover art grabber:{0} matching records found.  Retrieving first {1} records.",
                                       totResults, _MaxSearchResultItems);
        }

        for (int curPage = 1; curPage <= totPages;)
        {
          // Yield thread...
          Thread.Sleep(1);
          GUIWindowManager.Process();

          CheckForAppShutdown();

          if (resultsLimitExceeded || _AbortGrab)
          {
            break;
          }

          for (int y = 0; y < response.Items.Length; y++)
          {
            // Yield thread...
            Thread.Sleep(1);
            GUIWindowManager.Process();

            CheckForAppShutdown();

            if (resultsLimitExceeded || _AbortGrab)
            {
              break;
            }

            int totalItemCount = response.Items[y].Item.Length;

            for (int i = 0; i < totalItemCount; i++)
            {
              CheckForAppShutdown();

              if (_MaxSearchResultItems != -1 && imgCount >= _MaxSearchResultItems)
              {
                resultsLimitExceeded = true;
                break;
              }

              if (_AbortGrab)
              {
                break;
              }

              Item item = null;

              try
              {
                item = response.Items[y].Item[i];
              }

              catch (Exception ex)
              {
                //Console.WriteLine("response.Items[y].Item[i] caused an exception");
                Log.Info("Cover art grabber exception:{0}", ex.ToString());
              }

              if (item == null || item.ImageSets == null || item.ImageSets.Length == 0)
              {
                ++imgCount;
                DoProgressUpdate(imgCount, totResults);
                continue;
              }

              AWS.Image amazonImg = item.LargeImage;

              if (amazonImg == null)
              {
                amazonImg = item.MediumImage;
              }

              if (amazonImg == null)
              {
                amazonImg = item.SmallImage;
              }

              if (amazonImg == null)
              {
                ++imgCount;
                DoProgressUpdate(imgCount, totResults);
                continue;
              }

              string imgURL = amazonImg.URL;

              if (imgURL.Length == 0)
              {
                ++imgCount;
                DoProgressUpdate(imgCount, totResults);
                continue;
              }

              ItemAttributes itemAttribs = item.ItemAttributes;

              if (itemAttribs == null || itemAttribs.Artist == null)
              {
                ++imgCount;
                DoProgressUpdate(imgCount, totResults);
                continue;
              }

              string musicStyles = string.Empty;
              string editotialReview = string.Empty;

              editotialReview = GetEditorialReview(item);
              string releaseDate = "";

              if (item.ItemAttributes.ReleaseDate != null)
              {
                releaseDate = item.ItemAttributes.ReleaseDate.Trim();
              }

              AlbumInfo albumInfo = new AlbumInfo();
              albumInfo.Artist = GetArtists(item);
              albumInfo.Album = item.ItemAttributes.Title;
              albumInfo.Image = imgURL;
              albumInfo.Tracks = GetTracks(item);
              musicStyles = GetStyles(item);

              albumInfo.Styles = musicStyles.Trim(new char[] {',', ' '}).Trim();
              ;

              albumInfo.Review = editotialReview;
              albumInfo.Rating = 0;

              if (item.CustomerReviews != null)
              {
                albumInfo.Rating = Math.Max(0, (int) (item.CustomerReviews.AverageRating + (decimal) .5));
              }

              albumInfo.Year = GetReleaseYear(item);

              _AlbumInfoList.Add(albumInfo);
              ++imgCount;
              Console.WriteLine("#{0}: Adding Album {1}", imgCount, albumInfo.Album);
              DoProgressUpdate(imgCount, totResults);
            }
          }

          if (_MaxSearchResultItems != -1 && imgCount >= _MaxSearchResultItems)
          {
            resultsLimitExceeded = true;
            break;
          }

          int nextPage = ++curPage;
          Console.WriteLine("Getting page: {0}", nextPage);
          isr.ItemPage = string.Format("{0}", nextPage);
          isr.Artist = _ArtistName;
          isr.Title = _AlbumName;
          isr.SearchIndex = "Music";
          isr.ResponseGroup = responseGroupString;

          search.Request = new ItemSearchRequest[] {isr};
          response = service.ItemSearch(search);
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
        float secondsPerImage = (float) totSeconds/(float) imgCount;
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

        int progressPercent = (int) (((float) imgCount/(float) totalCovers)*100f);
        FindCoverArtProgress(this, progressPercent);
      }
    }

    private string GetArtists(Item item)
    {
      if (item.ItemAttributes.Artist == null)
      {
        return "";
      }

      string artistName = "";

      for (int artistIndex = 0; artistIndex < item.ItemAttributes.Artist.Length; artistIndex++)
      {
        string curArtist = item.ItemAttributes.Artist[artistIndex];

        if (curArtist.Length > 0)
        {
          artistName += curArtist + ", ";
        }
      }

      artistName = artistName.Trim(new char[] {',', ' '}).Trim();

      return artistName.Trim();
    }

    private string GetEditorialReview(Item item)
    {
      if (item.EditorialReviews == null)
      {
        return string.Empty;
      }

      string review = "";

      if (item.EditorialReviews.Length > 0)
      {
        review = item.EditorialReviews[0].Content;
      }

      //for(int i = 0; i < HtmlTags.Length; i++)
      //    review = review.Replace(HtmlTags[i], "");

      //return review.Trim();

      return Util.Utils.stripHTMLtags(review);
    }

    private string GetTracks(Item item)
    {
      if (item.Tracks == null || item.Tracks.Length == 0)
      {
        return "";
      }

      string tracks = "";

      for (int i = 0; i < item.Tracks[0].Track.Length; i++)
      {
        tracks += string.Format("{0}@{1}@{2}|", item.Tracks[0].Track[i].Number, item.Tracks[0].Track[i].Value, 99);
      }

      return tracks.Trim(new char[] {'|'}).Trim();
    }

    private string GetStyles(Item item)
    {
      if (item.BrowseNodes == null)
      {
        return "";
      }

      SortedList stylesList = new SortedList();
      string musicStyles = "";

      for (int i = 0; i < item.BrowseNodes.BrowseNode.Length; i++)
      {
        if (!stylesList.ContainsValue(item.BrowseNodes.BrowseNode[i].Name))
        {
          stylesList[i] = item.BrowseNodes.BrowseNode[i].Name;
        }
      }

      for (int i = 0; i < stylesList.Count; i++)
      {
        if (stylesList.ContainsKey(i))
        {
          musicStyles += string.Format("{0}, ", (string) stylesList[i]);
        }
      }

      return musicStyles.Trim(new char[] {',', ' '}).Trim();
    }

    private int GetReleaseYear(Item item)
    {
      if (item.ItemAttributes.ReleaseDate == null || item.ItemAttributes.ReleaseDate.Length == 0)
      {
        return 0;
      }

      int releaseYear = 0;

      try
      {
        releaseYear = DateTime.Parse(item.ItemAttributes.ReleaseDate).Year;
      }

      catch
      {
        // do nothing
      }

      return releaseYear;
    }

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
        WebResponse webResp = webReq.GetResponse();
        img = Image.FromStream(webResp.GetResponseStream());
      }

      catch
      {
      }

      return img;
    }
  }
}