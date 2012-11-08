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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Web;
using System.IO;

namespace MediaPortal.Video.Database
{
  internal class MatroskaTagInfo
  {
    public string Title;
    public string Description;
    public string Genre;
    public string ChannelName;
    public string EpisodeName;
    public DateTime StartTime;
    public DateTime EndTime;
  }

  internal class MatroskaTagHandler
  {
    #region Private members

    private static XmlNode AddSimpleTag(string tagName, string value, XmlDocument doc)
    {
      XmlNode rootNode = doc.CreateElement("SimpleTag");
      XmlNode nameNode = doc.CreateElement("name");
      nameNode.InnerText = tagName;
      XmlNode valueNode = doc.CreateElement("value");
      valueNode.InnerText = value;
      rootNode.AppendChild(nameNode);
      rootNode.AppendChild(valueNode);
      return rootNode;
    }

    #endregion

    #region Public members

    public static MatroskaTagInfo Fetch(string filename)
    {
      MatroskaTagInfo info = new MatroskaTagInfo();
      try
      {
        if (!File.Exists(filename))
        {
          return null;
        }

        XmlDocument doc = new XmlDocument();
        doc.Load(filename);
        XmlNodeList simpleTags = doc.SelectNodes("/tags/tag/SimpleTag");
        foreach (XmlNode simpleTag in simpleTags)
        {
          string tagName = simpleTag.ChildNodes[0].InnerText;
          switch (tagName)
          {
            case "TITLE":
              info.Title = simpleTag.ChildNodes[1].InnerText;
              break;
            case "COMMENT":
              info.Description = simpleTag.ChildNodes[1].InnerText;
              break;
            case "GENRE":
              info.Genre = simpleTag.ChildNodes[1].InnerText;
              break;
            case "CHANNEL_NAME":
              info.ChannelName = simpleTag.ChildNodes[1].InnerText;
              break;
            case "EPISODE_NAME":
              info.EpisodeName = simpleTag.ChildNodes[1].InnerText;
              break;
            case "START_TIME":
              info.StartTime = new DateTime(long.Parse(simpleTag.ChildNodes[1].InnerText));
              break;
            case "END_TIME":
              info.EndTime = new DateTime(long.Parse(simpleTag.ChildNodes[1].InnerText));
              break;
          }
        }
      }
      catch (Exception) { } // loading the XML doc could fail
      return info;
    }

    public static void Persist(string filename, MatroskaTagInfo taginfo)
    {
      try
      {
        if (!Directory.Exists(Path.GetDirectoryName(filename)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(filename));
        }

        XmlDocument doc = new XmlDocument();
        XmlDeclaration xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        XmlNode tagsNode = doc.CreateElement("tags");
        XmlNode tagNode = doc.CreateElement("tag");
        tagNode.AppendChild(AddSimpleTag("TITLE", taginfo.Title, doc));
        tagNode.AppendChild(AddSimpleTag("COMMENT", taginfo.Description, doc));
        tagNode.AppendChild(AddSimpleTag("GENRE", taginfo.Genre, doc));
        tagNode.AppendChild(AddSimpleTag("CHANNEL_NAME", taginfo.ChannelName, doc));
        tagNode.AppendChild(AddSimpleTag("EPISODE_NAME", taginfo.EpisodeName, doc));
        tagNode.AppendChild(AddSimpleTag("START_TIME", taginfo.StartTime.Ticks.ToString(), doc));
        tagNode.AppendChild(AddSimpleTag("END_TIME", taginfo.EndTime.Ticks.ToString(), doc));
        tagsNode.AppendChild(tagNode);
        doc.AppendChild(tagsNode);
        doc.InsertBefore(xmldecl, tagsNode);
        doc.Save(filename);
      }
      catch (Exception) { }
    }

    #endregion
  }

  /// <summary>
  /// @ 23.09.2004 by FlipGer
  /// new attribute string m_strDatabase, holds for example IMDB or OFDB
  /// </summary>
  public class IMDBMovie
  {
    private int _mID = -1;
    private string _mStrDirector = string.Empty;
    private string _mStrWritingCredits = string.Empty;
    private string _mStrSingleGenre = string.Empty;
    private string _mStrSingleUserGroup = string.Empty;
    private string _mStrTagLine = string.Empty;
    private string _mStrPlotOutline = string.Empty;
    private string _mStrPlot = string.Empty;
    private string _mStrPictureURL = string.Empty;
    private string _mStrTitle = string.Empty;
    private string _mStrSortTitle = string.Empty;
    private string _mStrVotes = string.Empty;
    private string _mStrCast = string.Empty;
    private string _mStrSearchString = string.Empty;
    private string _mStrFile = string.Empty;
    private string _mStrPath = string.Empty;
    private string _mStrDVDLabel = string.Empty;
    private string _mStrIMDBNumber = string.Empty;
    private string _mStrDatabase = string.Empty;
    private string _mStrCdLabel = string.Empty;
    private int _mITop250;
    private int _mIYear = 1900;
    private float _mFRating;
    private string _mStrMpaRating = string.Empty;
    private int _mIRunTime;
    private int _mIWatched;
    private int _mActorID = -1;
    private int _mGenreID = -1;
    private int _mUserGroupID = -1;
    private string _mStrActor = string.Empty;
    private string _mStrgenre = string.Empty;
    private int _mDirectorID = -1;
    private string _mStrUserReview = string.Empty;
    private string _mStrFanartURL = string.Empty;
    private string _dateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    private string _dateWatched = string.Empty;
    private string _mStrStudios = string.Empty;
    private string _mStrCountry= string.Empty;
    private string _mStrLanguage = string.Empty;
    private string _lastUpdate = string.Empty;
    private bool _isEmpty = true;
    // Variables for sharev view properties
    private VideoFilesMediaInfo _mediaInfo = new VideoFilesMediaInfo();
    private int _duration = 0;
    private int _watchedPercent = 0;
    private int _watchedCount = -1;
    private string _videoFileName = string.Empty;
    private string _videoFilePath = string.Empty;
    private string _userFanart = string.Empty;
    private string _movieNfoFile = string.Empty;
    
    public IMDBMovie() {}

    #region Get/Set

    public int ID
    {
      get { return _mID; }
      set { _mID = value; }
    }

    public bool IsEmpty
    {
      get
      {
        if ((_mStrTitle != string.Empty) && (_mStrTitle != Strings.Unknown))
        {
          _isEmpty = false;
          return _isEmpty;
        }
        _isEmpty = true;
        return _isEmpty;
      }
      set { _isEmpty = value; }
    }

    public int ActorID
    {
      get { return _mActorID; }
      set { _mActorID = value; }
    }

    public int GenreID
    {
      get { return _mGenreID; }
      set { _mGenreID = value; }
    }

    public int UserGroupID
    {
      get { return _mUserGroupID; }
      set { _mUserGroupID = value; }
    }

    public int DirectorID
    {
      get { return _mDirectorID; }
      set { _mDirectorID = value; }
    }

    public string Genre
    {
      get { return _mStrgenre; }
      set { _mStrgenre = value; }
    }

    public string Actor
    {
      get { return _mStrActor; }
      set { _mStrActor = value; }
    }

    public int RunTime
    {
      get { return _mIRunTime; }
      set { _mIRunTime = value; }
    }

    public int Watched
    {
      get { return _mIWatched; }
      set { _mIWatched = value; }
    }

    public string MPARating
    {
      get { return _mStrMpaRating; }
      set { _mStrMpaRating = value; }
    }

    public string Director
    {
      get { return _mStrDirector; }
      set { _mStrDirector = value; }
    }

    public string WritingCredits
    {
      get { return _mStrWritingCredits; }
      set { _mStrWritingCredits = value; }
    }

    public string SingleGenre
    {
      get { return _mStrSingleGenre; }
      set { _mStrSingleGenre = value; }
    }

    public string SingleUserGroup
    {
      get { return _mStrSingleUserGroup; }
      set { _mStrSingleUserGroup = value; }
    }

    public string TagLine
    {
      get { return _mStrTagLine; }
      set { _mStrTagLine = value; }
    }

    public string PlotOutline
    {
      get { return _mStrPlotOutline; }
      set { _mStrPlotOutline = value; }
    }

    public string Plot
    {
      get { return _mStrPlot; }
      set { _mStrPlot = value; }
    }

    public string UserReview
    {
      get { return _mStrUserReview; }
      set { _mStrUserReview = value; }
    }

    public string ThumbURL
    {
      get { return _mStrPictureURL; }
      set { _mStrPictureURL = value; }
    }

    public string FanartURL
    {
      get { return _mStrFanartURL; }
      set { _mStrFanartURL = value; }
    }

    public string Title
    {
      get { return _mStrTitle; }
      set { _mStrTitle = value; }
    }

    public string SortTitle
    {
      get { return _mStrSortTitle; }
      set { _mStrSortTitle = value; }
    }

    public string Votes
    {
      get { return _mStrVotes; }
      set { _mStrVotes = value; }
    }

    public string Cast
    {
      get { return _mStrCast; }
      set { _mStrCast = value; }
    }

    public string SearchString
    {
      get { return _mStrSearchString; }
      set { _mStrSearchString = value; }
    }

    public string File
    {
      get { return _mStrFile; }
      set { _mStrFile = value; }
    }

    public string Path
    {
      get { return _mStrPath; }
      set { _mStrPath = value; }
    }

    public string DVDLabel
    {
      get { return _mStrDVDLabel; }
      set { _mStrDVDLabel = value; }
    }

    public string CDLabel
    {
      get { return _mStrCdLabel; }
      set { _mStrCdLabel = value; }
    }

    public string IMDBNumber
    {
      get { return _mStrIMDBNumber; }
      set { _mStrIMDBNumber = value; }
    }

    public int Top250
    {
      get { return _mITop250; }
      set { _mITop250 = value; }
    }

    public int Year
    {
      get { return _mIYear; }
      set { _mIYear = value; }
    }

    public float Rating
    {
      get { return _mFRating; }
      set { _mFRating = value; }
    }

    public string Database
    {
      get { return _mStrDatabase; }
      set { _mStrDatabase = value; }
    }

    public string DateAdded
    {
      get { return _dateAdded; }
      set { _dateAdded = value; }
    }

    public string DateWatched
    {
      get { return _dateWatched; }
      set { _dateWatched = value; }
    }

    public string Studios
    {
      get { return _mStrStudios; }
      set { _mStrStudios = value; }
    }

    public string Country
    {
      get { return _mStrCountry; }
      set { _mStrCountry = value; }
    }

    public string Language
    {
      get { return _mStrLanguage; }
      set { _mStrLanguage = value; }
    }

    public string LastUpdate
    {
      get { return _lastUpdate; }
      set { _lastUpdate = value; }
    }

    public VideoFilesMediaInfo MediaInfo
    {
      get { return _mediaInfo; }
      set { _mediaInfo = value; }
    }

    public int Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }

    public int WatchedPercent
    {
      get { return _watchedPercent; }
      set { _watchedPercent = value; }
    }

    public int WatchedCount
    {
      get { return _watchedCount; }
      set { _watchedCount = value; }
    }

    public string VideoFileName
    {
      get { return _videoFileName; }
      set { _videoFileName = value; }
    }

    public string VideoFilePath
    {
      get { return _videoFilePath; }
      set { _videoFilePath = value; }
    }

    public string UserFanart
    {
      get { return _userFanart; }
      set { _userFanart = value; }
    }

    public string MovieNfoFile
    {
      get { return _movieNfoFile; }
      set { _movieNfoFile = value; }
    }

    #endregion

    public void Reset()
    {
      Reset(false);
    }

    public void Reset(bool resetId)
    {
      if (resetId)
      {
        _mID = -1;
      }
      _mStrDirector = string.Empty;
      _mStrWritingCredits = string.Empty;
      _mStrSingleGenre = string.Empty;
      _mStrSingleUserGroup = string.Empty;
      _mStrgenre = string.Empty;
      _mStrTagLine = string.Empty;
      _mStrPlotOutline = string.Empty;
      _mStrPlot = string.Empty;
      // Added userreview
      _mStrUserReview = string.Empty;
      _mStrPictureURL = string.Empty;
      // Fanart
      _mStrFanartURL = string.Empty;
      _mStrTitle = string.Empty;
      _mStrSortTitle = string.Empty;
      _mStrVotes = string.Empty;
      _mStrCast = string.Empty;
      _mStrSearchString = string.Empty;
      _mStrIMDBNumber = string.Empty;
      _mITop250 = 0;
      _mIYear = 1900;
      _mFRating = 0.0f;
      _mStrDatabase = string.Empty;
      _mStrMpaRating = string.Empty;
      _mIRunTime = 0;
      _mIWatched = 0;
      _dateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
      _dateWatched = string.Empty;
      _mStrStudios = string.Empty;
      _mStrCountry = string.Empty;
      _mStrLanguage = string.Empty;
      _lastUpdate = string.Empty;
      _isEmpty = true;
      _duration = 0;
      _watchedPercent = 0;
      _watchedCount = -1;
      _videoFileName = string.Empty;
      _videoFilePath = string.Empty;
      _userFanart = string.Empty;
      _movieNfoFile = string.Empty;
    }

    #region Database views skin properties

    [Obsolete("This method is obsolete; use method SetProperties(bool isFolder, string file) instead.")]
    public void SetProperties(bool isFolder)
    {
      SetProperties(isFolder, string.Empty);
    }

    public void SetProperties(bool isFolder, string file)
    {
      string strThumb = GetStrThumb();

      GUIPropertyManager.SetProperty("#director", Director);
      GUIPropertyManager.SetProperty("#genre", Genre.Replace(" /", ","));
      GUIPropertyManager.SetProperty("#cast", Cast);
      GUIPropertyManager.SetProperty("#dvdlabel", DVDLabel);
      GUIPropertyManager.SetProperty("#imdbnumber", IMDBNumber);
      GUIPropertyManager.SetProperty("#file", File);
      GUIPropertyManager.SetProperty("#plot", HttpUtility.HtmlDecode(Plot));
      GUIPropertyManager.SetProperty("#plotoutline", PlotOutline);
      GUIPropertyManager.SetProperty("#userreview", UserReview); // Added
      GUIPropertyManager.SetProperty("#rating", Rating.ToString());
      GUIPropertyManager.SetProperty("#strrating", Rating.ToString(CultureInfo.CurrentCulture) + "/10");
      GUIPropertyManager.SetProperty("#tagline", TagLine);
      GUIPropertyManager.SetProperty("#myvideosuserfanart", UserFanart);
      
      // Votes
      Int32 votes = 0;
      string strVotes = string.Empty;
      
      if (Int32.TryParse(Votes.Replace(".", string.Empty).Replace(",", string.Empty), out votes))
      {
        strVotes = String.Format("{0:N0}", votes);
      }

      GUIPropertyManager.SetProperty("#votes", strVotes);
      GUIPropertyManager.SetProperty("#credits", WritingCredits.Replace(" /", ","));
      GUIPropertyManager.SetProperty("#thumb", strThumb);
      GUIPropertyManager.SetProperty("#title", Title);
      GUIPropertyManager.SetProperty("#year", Year.ToString());
      // MPAA rating
      MPARating = Util.Utils.MakeFileName(MPARating);
      GUIPropertyManager.SetProperty("#mpaarating", MPARating);
      GUIPropertyManager.SetProperty("#studios", Studios.Replace(" /", ","));
      GUIPropertyManager.SetProperty("#country", Country);
      GUIPropertyManager.SetProperty("#language", Language);
      DateTime lastUpdate;
      DateTime.TryParseExact(LastUpdate, "yyyy-MM-dd HH:mm:ss", 
                             CultureInfo.CurrentCulture, 
                             DateTimeStyles.None, 
                             out lastUpdate);
      GUIPropertyManager.SetProperty("#lastupdate", lastUpdate.ToShortDateString());

      // Show/hide movie info labels and values in skin, set movie duration and runtime value
      if (ID == -1)
      {
        if (_isEmpty) // Could be xml recordings so we need to check if movie info data is empty (movieId is -1)
        {
          GUIPropertyManager.SetProperty("#hideinfo", "true");
        }
        else
        {
          GUIPropertyManager.SetProperty("#hideinfo", "false");
        }
        
        SetDurationProperty(VideoDatabase.GetMovieId(file));
      }
      else
      {
        SetDurationProperty(ID);
        GUIPropertyManager.SetProperty("#hideinfo", "false");
      }
      
      // Movie id property (to set random movieId property for movies not in the database -> not scanned)
      // FA handler use this property
      // movieId is independant and it is related to videofile
      if (!string.IsNullOrEmpty(file))
      {
        SetMovieIDProperty(file, isFolder);
      }
      else
      {
        GUIPropertyManager.SetProperty("#movieid", "-1");
      }

      // Watched property
      string strValue = "no";

      if (Watched > 0 && !isFolder)
      {
        strValue = "yes";
      }

      if (isFolder)
      {
        strValue = string.Empty;
      }
      GUIPropertyManager.SetProperty("#iswatched", strValue);

      // Watched percent property
      GUIPropertyManager.SetProperty("#watchedpercent", WatchedPercent.ToString());
      
      // Watched count
      if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file) || !string.IsNullOrEmpty(VideoFileName) && System.IO.File.Exists(VideoFileName))
      {
        GUIPropertyManager.SetProperty("#watchedcount", WatchedCount.ToString());
      }
      else
      {
        GUIPropertyManager.SetProperty("#watchedcount", "-1");
      }
      
      // MediaInfo Properties
      try
      {
        ResetMediaInfoProperties(); // Clear previous values

        if (!string.IsNullOrEmpty(file))
        {
          SetMediaInfoProperties(file);
        }
      }
        
      catch (Exception e)
      {
        Log.Error("IMDBMovie Media Info error: file:{0}, error:{1}", file, e);
      }
    }

    public void SetDurationProperty(int movieId) 
    {
      if (RunTime <= 0)
      {
        IMDBMovie movie = new IMDBMovie();
        VideoDatabase.GetMovieInfoById(movieId, ref movie);
        RunTime = movie.RunTime;
      }

      GUIPropertyManager.SetProperty("#runtime", RunTime + 
                              " " +
                              GUILocalizeStrings.Get(2998) +
                              " (" + 
                              Util.Utils.SecondsToHMString(RunTime * 60) + 
                              ")");

      if (Duration <= 0)
      {
        GUIPropertyManager.SetProperty("#videoruntime", string.Empty);
      }
      else
      {
        GUIPropertyManager.SetProperty("#videoruntime", Util.Utils.SecondsToHMSString(Duration));
      }
    }

    public void SetPlayProperties()
    {
      SetPlayProperties(false);
    }

    public void SetPlayProperties(bool useNfo)
    {
      // Title suffix for problem with covers and movie with the same name
      string strThumb = string.Empty;
      
      if (!useNfo)
      {
        strThumb = GetStrThumb();
      }
      else
      {
        strThumb = ThumbURL;
      }

      GUIPropertyManager.SetProperty("#Play.Current.Director", Director);
      GUIPropertyManager.SetProperty("#Play.Current.Genre", Genre);
      GUIPropertyManager.SetProperty("#Play.Current.Cast", Cast);
      GUIPropertyManager.SetProperty("#Play.Current.DVDLabel", DVDLabel);
      GUIPropertyManager.SetProperty("#Play.Current.IMDBNumber", IMDBNumber);
      GUIPropertyManager.SetProperty("#Play.Current.File", File);
      GUIPropertyManager.SetProperty("#Play.Current.Plot", Plot);
      GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", PlotOutline);
      GUIPropertyManager.SetProperty("#Play.Current.UserReview", UserReview); // Added
      GUIPropertyManager.SetProperty("#Play.Current.Rating", Rating.ToString());
      GUIPropertyManager.SetProperty("#Play.Current.TagLine", TagLine);
      GUIPropertyManager.SetProperty("#Play.Current.Votes", Votes);
      GUIPropertyManager.SetProperty("#Play.Current.Credits", WritingCredits);
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", strThumb);
      GUIPropertyManager.SetProperty("#Play.Current.Title", Title);
      GUIPropertyManager.SetProperty("#Play.Current.Year", Year.ToString());
      GUIPropertyManager.SetProperty("#Play.Current.Runtime", RunTime.ToString());
      GUIPropertyManager.SetProperty("#Play.Current.MPAARating", MPARating);
      string strValue = "no";
      if (Watched > 0)
      {
        strValue = "yes";
      }
      GUIPropertyManager.SetProperty("#Play.Current.IsWatched", strValue);
    }

    private void SetMovieIDProperty(string file, bool isFolder)
    {
      VirtualDirectory vDir = new VirtualDirectory();
      int pin = 0;
      vDir.LoadSettings("movies");

      if (isFolder && !vDir.IsProtectedShare(file, out pin))
      {
        ArrayList mList = new ArrayList();
        VideoDatabase.GetMoviesByPath(file, ref mList);
        
        if (mList.Count > 0)
        {
          Random rnd = new Random();
          int r = rnd.Next(mList.Count);
          IMDBMovie movieDetails = (IMDBMovie)mList[r];
          mList.Clear();
          VideoDatabase.GetFilesForMovie(movieDetails.ID, ref mList);

          if (mList.Count > 0 && System.IO.File.Exists(mList[0].ToString()))
          {
            GUIPropertyManager.SetProperty("#movieid", movieDetails.ID.ToString());
          }
          else
          {
            GUIPropertyManager.SetProperty("#movieid", "-1");
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#movieid", "-1");
        }
      }
      else if (isFolder && vDir.IsProtectedShare(file, out pin))
      {
        GUIPropertyManager.SetProperty("#movieid", "-1");
      }
      else
      {
        GUIPropertyManager.SetProperty("#movieid", ID.ToString());
      }
    }

    private void ResetMediaInfoProperties()
    {
      GUIPropertyManager.SetProperty("#VideoMediaSource", string.Empty);
      GUIPropertyManager.SetProperty("#VideoCodec", string.Empty);
      GUIPropertyManager.SetProperty("#VideoResolution", string.Empty);
      GUIPropertyManager.SetProperty("#AudioCodec", string.Empty);
      GUIPropertyManager.SetProperty("#AudioChannels", string.Empty);
      GUIPropertyManager.SetProperty("#HasSubtitles", "false");
      GUIPropertyManager.SetProperty("#AspectRatio", string.Empty);
    }

    private void SetMediaInfoProperties(string file)
    {
      SetMediaInfoProperties(file, false);
    }

    public void SetMediaInfoProperties(string file, bool refresh)
    {
      try
      {
        string hasSubtitles = "false";
        string videoMediaSource = string.Empty;

        if (MediaInfo.HasSubtitles)
        {
          hasSubtitles = "true";
        }
        
        GUIPropertyManager.SetProperty("#VideoMediaSource", videoMediaSource);
        GUIPropertyManager.SetProperty("#VideoCodec", Util.Utils.MakeFileName(MediaInfo.VideoCodec));
        GUIPropertyManager.SetProperty("#VideoResolution", MediaInfo.VideoResolution);
        GUIPropertyManager.SetProperty("#AudioCodec", Util.Utils.MakeFileName(MediaInfo.AudioCodec));
        GUIPropertyManager.SetProperty("#AudioChannels", MediaInfo.AudioChannels);
        GUIPropertyManager.SetProperty("#HasSubtitles", hasSubtitles);
        GUIPropertyManager.SetProperty("#AspectRatio", MediaInfo.AspectRatio);
      }
      catch (Exception) { }
    }

    private string GetStrThumb()
    {
      string titleExt = Title + "{" + ID + "}";
      return Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
    }

    #endregion

    #region Share view movieinfo and skin properties

    /// <summary>
    /// Use only in share view
    /// </summary>
    /// <param name="item"></param>
    public static void SetMovieData(GUIListItem item)
    {
      IMDBMovie info = new IMDBMovie();

      if (item == null)
      {
        return;
      }
      
      try
      {
        string path = string.Empty;
        string fileName = string.Empty;

        if (Util.Utils.IsVideo(item.Path))
        {
          Util.Utils.Split(item.Path, out path, out fileName);
        }
        else
        {
          path = item.Path;
        }

        if (item.Path != ".." && System.IO.File.Exists(item.Path + @"\VIDEO_TS\VIDEO_TS.IFO"))
        {
          fileName = item.Path + @"\VIDEO_TS\VIDEO_TS.IFO";
        }
        else if (item.Path != ".." && System.IO.File.Exists(item.Path + @"\BDMV\index.bdmv"))
        {
          fileName = item.Path + @"\BDMV\index.bdmv";
        }
        else
        {
          fileName = item.Path;
        }

        // Set
        VideoFilesMediaInfo mInfo = new VideoFilesMediaInfo();

        if (path == ".." || string.IsNullOrEmpty(path) || (!Directory.Exists(path) && !Util.Utils.IsVideo(fileName)))
        {
          info.MediaInfo = mInfo;
          item.AlbumInfoTag = info;
          return;
        }

        if (Directory.Exists(path) && !Util.Utils.IsVideo(fileName))
        {
          int rndMovieId = -1;

          VirtualDirectory vDir = new VirtualDirectory();
          int pin = 0;
          vDir.LoadSettings("movies");

          if (!vDir.IsProtectedShare(path, out pin))
          {
            ArrayList mList = new ArrayList();
            VideoDatabase.GetRandomMoviesByPath(path, ref mList, 1);

            if (mList.Count > 0)
            {
              IMDBMovie movieDetails = (IMDBMovie)mList[0];
              mList.Clear();
              rndMovieId = movieDetails.ID;
            }
            else
            {
              // User fanart (only for videos which do not have movie info in db -> not scanned)
              try
              {
                GetUserFanart(item, ref info);
              }
              catch (Exception ex)
              {
                Log.Error("IMDBMovie Set user fanart file property error: {0}", ex.Message);
              }
            }
          }

          info.ID = rndMovieId;
          info.MediaInfo = mInfo;
          item.AlbumInfoTag = info;
          return;
        }

        try
        {
          VideoDatabase.GetMovieInfo(fileName, ref info);

          // Get recording/nfo xml
          if (info.IsEmpty)
          {
            FetchMatroskaInfo(fileName, false, ref info);

            if (info.IsEmpty)
            {
              FetchMovieNfo(path, fileName, ref info);
            }
          }

          VideoDatabase.GetVideoFilesMediaInfo(fileName, ref mInfo, false);
          info.VideoFileName = fileName;

          if (string.IsNullOrEmpty(info.VideoFilePath) || info.VideoFilePath == Strings.Unknown)
          {
            string tmpFile = string.Empty;
            Util.Utils.Split(fileName, out path, out tmpFile);
            info.VideoFilePath = path;
          }

          info.Path = path;
          info.MediaInfo = mInfo;

          if (info.ID > 0)
          {
            info.Duration = VideoDatabase.GetMovieDuration(info.ID);
          }
          else
          {
            ArrayList files = new ArrayList();
            VideoDatabase.GetFilesForMovie(VideoDatabase.GetMovieId(info.VideoFileName), ref files);

            foreach (string file in files)
            {
              info.Duration += VideoDatabase.GetVideoDuration(VideoDatabase.GetFileId(file));
            }
          }

          int percent = 0;
          int watchedCount = 0;
          VideoDatabase.GetmovieWatchedStatus(VideoDatabase.GetMovieId(fileName), out percent, out watchedCount);
          info.WatchedPercent = percent;
          info.WatchedCount = watchedCount;

          // User fanart (only for videos which do not have movie info in db -> not scanned)
          try
          {
            if (info.ID < 1)
            {
              GetUserFanart(item, ref info);
            }
          }
          catch (Exception ex)
          {
            Log.Error("IMDBMovie Set user fanart file property error: {0}", ex.Message);
          }

          item.AlbumInfoTag = info;
        }
        catch (Exception ex)
        {
          Log.Error("IMDBMovie SetMovieData (GetMovieInfo) error: {0}", ex.Message);
          item.AlbumInfoTag = info;
        }
      }
      catch (Exception ex)
      {
        Log.Error("IMDBMovie SetMovieData error: {0}", ex.Message);
        item.AlbumInfoTag = info;
      }
    }

    private static void GetUserFanart(GUIListItem item, ref IMDBMovie info)
    {
      if (info == null)
      {
        return;
      }

      string strPath, strFilename;
      Util.Utils.Split(info.VideoFileName, out strPath, out strFilename);

      if (string.IsNullOrEmpty(strPath))
      {
        if (string.IsNullOrEmpty(item.Path))
        {
          return;
        }
        strPath = item.Path;
      }

      List<string> faFiles = new List<string>();
      string faFile = strPath + @"\fanart.jpg";
      faFiles.Add(faFile);
      faFile = strPath + @"\backdrop.jpg";
      faFiles.Add(faFile);


      if (item.IsBdDvdFolder) // dvd/blu-ray
      {
        strPath = strPath.Remove(strPath.LastIndexOf(@"\"));

        faFile = strPath + @"\" + Util.Utils.GetFilename(strFilename, true) + "-fanart.jpg";
        faFiles.Add(faFile);
        faFile = strPath + @"\" + "fanart.jpg";
        faFiles.Add(faFile);
        faFile = strPath + @"\" + "backdrop.jpg";
        faFiles.Add(faFile);

        string dvdBdPath = System.IO.Path.GetFileName(strPath);
        Util.Utils.RemoveStackEndings(ref dvdBdPath);

        faFile = strPath + @"\" + dvdBdPath.Trim() + "-fanart.jpg";
        faFiles.Add(faFile);

        foreach (string file in faFiles)
        {
          if (System.IO.File.Exists(file))
          {
            info.UserFanart = file;
            break;
          }
        }
      }
      else if (!item.IsFolder && !string.IsNullOrEmpty(strFilename)) // video file
      {
        Util.Utils.RemoveStackEndings(ref strFilename);
        faFile = strPath + @"\" + Util.Utils.GetFilename(strFilename, true) + "-fanart.jpg";
        faFiles.Add(faFile);
        faFile = strPath + @"\" + Util.Utils.GetFilename(strFilename, true) + "-backdrop.jpg";
        faFiles.Add(faFile);
        string cleanPath = System.IO.Path.GetFileName(strPath);
        Util.Utils.RemoveStackEndings(ref cleanPath);
        faFile = strPath + @"\" + cleanPath + "-fanart.jpg";
        faFiles.Add(faFile);
        faFile = strPath + @"\" + cleanPath + "-backdrop.jpg";
        faFiles.Add(faFile);

        foreach (string file in faFiles)
        {
          if (System.IO.File.Exists(file))
          {
            info.UserFanart = file;
            break;
          }
        }
      }
      else if (item.IsFolder && !VirtualDirectories.Instance.Movies.IsRootShare(strPath) &&
               Util.Utils.IsFolderDedicatedMovieFolder(strPath)) // folder & dedicated movie folder
      {
        string cleanPath = System.IO.Path.GetFileName(strPath);
        Util.Utils.RemoveStackEndings(ref cleanPath);
        faFile = strPath + @"\" + cleanPath + "-fanart.jpg";
        faFiles.Add(faFile);
        faFile = strPath + @"\" + cleanPath + "-backdrop.jpg";
        faFiles.Add(faFile);
        
        foreach (string file in faFiles)
        {
          if (System.IO.File.Exists(file))
          {
            info.UserFanart = file;
            break;
          }
        }
      }
    }

    /// <summary>
    /// Use for xml recordings
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pathIsDirectory"></param>
    /// <param name="movie"></param>
    private static void FetchMatroskaInfo(string path, bool pathIsDirectory, ref IMDBMovie movie)
    {
      try
      {
        string xmlFile = string.Empty;
        if (!pathIsDirectory)
        {
          xmlFile = System.IO.Path.ChangeExtension(path, ".xml");
        }

        MatroskaTagInfo minfo = MatroskaTagHandler.Fetch(xmlFile);
        if (minfo != null)
        {
          movie.Title = minfo.Title;
          movie.Plot = minfo.Description;
          movie.Genre = minfo.Genre;
        }
      }
      catch (Exception) { }
    }

    /// <summary>
    /// Check and set movie info data from nfo file (must be in the same directory where is videofile and must have
    /// the same name as video file.
    /// Exceptions are DVD and BluRay folders where nfo file should have name as main folder
    /// </summary>
    /// <param name="path">path is checked only for DVD/BD folders, in case of single video files it is the same as filename</param>
    /// <param name="filename">filename with full path</param>
    /// <param name="movie"></param>
    public static void FetchMovieNfo(string path, string filename, ref IMDBMovie movie)
    {
      try
      {
        string nfoFile = string.Empty;
        
        if (filename.ToUpperInvariant().Contains("VIDEO_TS.IFO") || filename.ToUpperInvariant().Contains("INDEX.BDMV"))
        {
          nfoFile = path + @"\" + System.IO.Path.GetFileNameWithoutExtension(filename) + ".nfo";

          if (!System.IO.File.Exists(nfoFile))
          {
            string noStackPath = path;
            Util.Utils.RemoveStackEndings(ref noStackPath);
            nfoFile = path + @"\" + System.IO.Path.GetFileNameWithoutExtension(noStackPath) + ".nfo";
          }
        }
        else
        {
          nfoFile = System.IO.Path.ChangeExtension(filename, ".nfo");
          Util.Utils.RemoveStackEndings(ref nfoFile);
        }
        
        if (!System.IO.File.Exists(nfoFile))
        {
          return;
        }

        XmlDocument doc = new XmlDocument();

        try
        {
          doc.Load(nfoFile);
        }
        catch (Exception)
        {
          Log.Info("GUIVideoFiles.Load nfo file error: {0} is not a valid XML document", nfoFile);
          return;
        }

        if (doc.DocumentElement != null)
        {
          XmlNodeList movieList = doc.DocumentElement.SelectNodes("/movie");

          if (movieList == null)
          {
            return;
          }

          foreach (XmlNode nodeMovie in movieList)
          {
            string genre = string.Empty;
            string cast = string.Empty;

            #region nodes

            XmlNode nodeTitle = nodeMovie.SelectSingleNode("title");
            XmlNode nodeRating = nodeMovie.SelectSingleNode("rating");
            XmlNode nodeYear = nodeMovie.SelectSingleNode("year");
            XmlNode nodeDuration = nodeMovie.SelectSingleNode("runtime");
            XmlNode nodePlotShort = nodeMovie.SelectSingleNode("outline");
            XmlNode nodePlot = nodeMovie.SelectSingleNode("plot");
            XmlNode nodeTagline = nodeMovie.SelectSingleNode("tagline");
            XmlNode nodeDirector = nodeMovie.SelectSingleNode("director");
            XmlNode nodeImdbNumber = nodeMovie.SelectSingleNode("imdb");
            XmlNode nodeMpaa = nodeMovie.SelectSingleNode("mpaa");
            XmlNode nodeTop250 = nodeMovie.SelectSingleNode("top250");
            XmlNode nodeVotes = nodeMovie.SelectSingleNode("votes");
            XmlNode nodeStudio = nodeMovie.SelectSingleNode("studio");
            XmlNode nodePoster = nodeMovie.SelectSingleNode("thumb");
            XmlNode nodeLanguage = nodeMovie.SelectSingleNode("language");
            XmlNode nodeCountry = nodeMovie.SelectSingleNode("country");
            XmlNode nodeReview = nodeMovie.SelectSingleNode("review");
            XmlNode nodeCredits = nodeMovie.SelectSingleNode("credits");


            #endregion

            #region Genre

            XmlNodeList genres = nodeMovie.SelectNodes("genre");

            foreach (XmlNode nodeGenre in genres)
            {
              if (nodeGenre.InnerText != null)
              {
                if (genre.Length > 0)
                {
                  genre += " / ";
                }
                genre += nodeGenre.InnerText;
              }
            }

            if (string.IsNullOrEmpty(genre))
            {
              genres = nodeMovie.SelectNodes("genres/genre");

              foreach (XmlNode nodeGenre in genres)
              {
                if (nodeGenre.InnerText != null)
                {
                  if (genre.Length > 0)
                  {
                    genre += " / ";
                  }
                  genre += nodeGenre.InnerText;
                }
              }
            }

            // Genre
            movie.Genre = genre;

            #endregion

            #region Credits (Writers)

            // Writers
            if (nodeCredits != null)
            {
              movie.WritingCredits = nodeCredits.InnerText;
            }
            #endregion

            #region Cast

            // Cast parse
            XmlNodeList actorsList = nodeMovie.SelectNodes("actor");
            
            foreach (XmlNode nodeActor in actorsList)
            {
              string name = string.Empty;
              string role = string.Empty;
              string line = string.Empty;
              XmlNode nodeActorName = nodeActor.SelectSingleNode("name");
              XmlNode nodeActorRole = nodeActor.SelectSingleNode("role");
              
              if (nodeActorName != null && nodeActorName.InnerText != null)
              {
                name = nodeActorName.InnerText;
              }
              if (nodeActorRole != null && nodeActorRole.InnerText != null)
              {
                role = nodeActorRole.InnerText;
              }
              
              if (!string.IsNullOrEmpty(name))
              {
                if (!string.IsNullOrEmpty(role))
                {
                  line = String.Format("{0} as {1}\n", name, role);
                }
                else
                {
                  line = String.Format("{0}\n", name);
                }
                cast += line;
              }
            }
            // Cast
            movie.Cast = cast;

            #endregion

            #region Moviefiles

            // Need to fake movie to see it's properties (id = 0 is not used in vdb for movies)
            movie.Path = path;
            movie.File = filename;
            movie.MovieNfoFile = nfoFile;
            int movieId = VideoDatabase.GetMovieId(filename);

            if (movieId < 0)
            {
              movie.ID = 0;
            }
            else
            {
              movie.ID = movieId;

              #region Watched status

              int percent = 0;
              int watchedCount = 0;
              VideoDatabase.GetmovieWatchedStatus(movieId, out percent, out watchedCount);

              if (watchedCount > 0)
              {
                movie.Watched = 1;
                movie.WatchedCount = watchedCount;
                movie.WatchedPercent = percent;
              }
              else
              {
                movie.Watched = 0;
                movie.WatchedCount = 0;
                movie.WatchedPercent = 0;
              }

              movie.ID = 0;

              #endregion
            }

            #endregion

            #region Title

            // Title
            if (nodeTitle != null)
            {
              movie.Title = nodeTitle.InnerText;
            }

            #endregion

            #region Language

            // Title
            if (nodeLanguage != null)
            {
              movie.Language = nodeLanguage.InnerText;
            }

            #endregion

            #region Country

            // Title
            if (nodeCountry != null)
            {
              movie.Country = nodeCountry.InnerText;
            }

            #endregion

            #region IMDB number

            // IMDB number
            if (nodeImdbNumber != null)
            {
              if (VideoDatabase.CheckMovieImdbId(nodeImdbNumber.InnerText))
              {
                movie.IMDBNumber = nodeImdbNumber.InnerText;
              }
            }

            #endregion

            #region Director

            // Director
            if (nodeDirector != null)
            {
              movie.Director = nodeDirector.InnerText;
            }
            #endregion

            #region Studio

            // Studio
            if (nodeStudio != null)
            {
              movie.Studios = nodeStudio.InnerText;
            }

            #endregion

            #region MPAA

            // MPAA
            if (nodeMpaa != null)
            {
              movie.MPARating = nodeMpaa.InnerText;
            }
            else
            {
              movie.MPARating = "NR";
            }

            #endregion

            #region Plot/Short plot

            // Plot
            if (nodePlot != null)
            {
              movie.Plot = nodePlot.InnerText;
            }
            else
            {
              movie.Plot = string.Empty;
            }
            // Short plot
            if (nodePlotShort != null)
            {
              movie.PlotOutline = nodePlotShort.InnerText;
            }
            else
            {
              movie.PlotOutline = string.Empty;
            }

            #endregion

            #region Review

            // Title
            if (nodeReview != null)
            {
              movie.UserReview = nodeReview.InnerText;
            }

            #endregion

            #region Rating (n.n/10)

            // Rating
            if (nodeRating != null)
            {
              double rating = 0;
              if (Double.TryParse(nodeRating.InnerText.Replace(".", ","), out rating))
              {
                movie.Rating = (float)rating;

                if (movie.Rating > 10.0f)
                {
                  movie.Rating /= 10.0f;
                }
              }
            }

            #endregion

            #region Duration

            // Duration
            if (nodeDuration != null)
            {
              int runtime = 0;
              if (Int32.TryParse(nodeDuration.InnerText, out runtime))
              {
                movie.RunTime = runtime;
              }
              else
              {
                string regex = "(?<h>[0-9]*)h.(?<m>[0-9]*)";
                MatchCollection mc = Regex.Matches(nodeDuration.InnerText, regex, RegexOptions.Singleline);
                if (mc.Count > 0)
                {
                  foreach (Match m in mc)
                  {
                    int hours = 0;
                    Int32.TryParse(m.Groups["h"].Value, out hours);
                    int minutes = 0;
                    Int32.TryParse(m.Groups["m"].Value, out minutes);
                    hours = hours * 60;
                    minutes = hours + minutes;
                    movie.RunTime = minutes;
                  }
                }
              }
            }
            else
            {
              movie.RunTime = 0;
            }

            #endregion

            #region Tagline

            // Tagline
            if (nodeTagline != null)
            {
              movie.TagLine = nodeTagline.InnerText;
            }

            #endregion

            #region TOP250

            // Top250
            if (nodeTop250 != null)
            {
              int top250 = 0;
              Int32.TryParse(nodeTop250.InnerText, out top250);
              movie.Top250 = top250;
            }
            else
            {
              movie.Top250 = 0;
            }


            #endregion

            #region votes

            // Votes
            if (nodeVotes != null)
            {
              movie.Votes = nodeVotes.InnerText;
            }

            #endregion

            #region Year

            // Year
            if (nodeYear != null)
            {
              int year = 0;
              Int32.TryParse(nodeYear.InnerText, out year);
              movie.Year = year;
            }

            #endregion

            #region poster

            // Poster
            string thumbJpgFile = string.Empty;
            string thumbTbnFile = string.Empty;
            string thumbJpgFileLocal = string.Empty;
            string thumbTbnFileLocal = string.Empty;

            if (nodePoster != null)
            {
              filename = System.IO.Path.GetFileNameWithoutExtension(filename);
              Util.Utils.RemoveStackEndings(ref filename);

              thumbJpgFile = path + @"\" + nodePoster.InnerText;
              thumbTbnFile = path + @"\" + nodePoster.InnerText;
              thumbJpgFileLocal = path + @"\" + filename + ".jpg";
              thumbTbnFileLocal = path + @"\" + filename + ".tbn";

              // local
              if (System.IO.File.Exists(thumbJpgFileLocal))
              {
                movie.ThumbURL = thumbJpgFileLocal;
              }
              else if (System.IO.File.Exists(thumbTbnFileLocal))
              {
                movie.ThumbURL = thumbTbnFileLocal;
              }
              // XML
              else if (System.IO.File.Exists(thumbJpgFile))
              {
                movie.ThumbURL = thumbJpgFile;
              }
              else if (System.IO.File.Exists(thumbTbnFile))
              {
                movie.ThumbURL = thumbTbnFile;
              }
            }
            else
            {
              filename = System.IO.Path.GetFileNameWithoutExtension(filename);
              Util.Utils.RemoveStackEndings(ref filename);

              thumbJpgFileLocal = path + @"\" + filename + ".jpg";
              thumbTbnFileLocal = path + @"\" + filename + ".tbn";

              if (System.IO.File.Exists(thumbJpgFileLocal))
              {
                movie.ThumbURL = thumbJpgFileLocal;
              }
              else if (System.IO.File.Exists(thumbTbnFileLocal))
              {
                movie.ThumbURL = thumbTbnFileLocal;
              }
            }

            #endregion

          }
        }
      }
      catch (ThreadAbortException)
      {
        // Will be logged in thread main code
      }
      catch (Exception ex)
      {
        Log.Error("GUIVideoFiles. Error in nfo xml document: {0}", ex.Message);
      }
    }

    /// <summary>
    /// Use only in share view
    /// </summary>
    /// <param name="item"></param>
    public static void SetMovieProperties(GUIListItem item)
    {
      try
      {
        IMDBMovie info = item.AlbumInfoTag as IMDBMovie;

        if (info == null)
        {
          return;
        }
      
        string titleExt = info.Title + "{" + info.ID + "}";
        string strThumb = Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);

        GUIPropertyManager.SetProperty("#director", info.Director);
        GUIPropertyManager.SetProperty("#genre", info.Genre.Replace(" /", ","));
        GUIPropertyManager.SetProperty("#cast", info.Cast);
        GUIPropertyManager.SetProperty("#dvdlabel", info.DVDLabel);
        GUIPropertyManager.SetProperty("#imdbnumber", info.IMDBNumber);
        GUIPropertyManager.SetProperty("#file", info.File);
        GUIPropertyManager.SetProperty("#plot", HttpUtility.HtmlDecode(info.Plot));
        GUIPropertyManager.SetProperty("#plotoutline", info.PlotOutline);
        GUIPropertyManager.SetProperty("#userreview", info.UserReview);
        GUIPropertyManager.SetProperty("#rating", info.Rating.ToString());
        GUIPropertyManager.SetProperty("#strrating", info.Rating.ToString(CultureInfo.CurrentCulture) + "/10");
        GUIPropertyManager.SetProperty("#tagline", info.TagLine);
        //Votes
        Int32 votes = 0;
        string strVotes = string.Empty;
        if (Int32.TryParse(info.Votes.Replace(".", string.Empty).Replace(",", string.Empty), out votes))
        {
          strVotes = String.Format("{0:N0}", votes);
        }
        GUIPropertyManager.SetProperty("#votes", strVotes);
        //
        GUIPropertyManager.SetProperty("#credits", info.WritingCredits.Replace(" /", ","));
        GUIPropertyManager.SetProperty("#thumb", strThumb);
        GUIPropertyManager.SetProperty("#title", info.Title);
        GUIPropertyManager.SetProperty("#year", info.Year.ToString());
        // MPAA
        info.MPARating = Util.Utils.MakeFileName(info.MPARating);
        GUIPropertyManager.SetProperty("#mpaarating", info.MPARating);
        //
        GUIPropertyManager.SetProperty("#studios", info.Studios.Replace(" /", ","));
        GUIPropertyManager.SetProperty("#country", info.Country);
        GUIPropertyManager.SetProperty("#language", info.Language);
        // Last update date
        DateTime lastUpdate;
        DateTime.TryParseExact(info.LastUpdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out lastUpdate);
        GUIPropertyManager.SetProperty("#lastupdate", lastUpdate.ToShortDateString());
        //
        GUIPropertyManager.SetProperty("#movieid", info.ID.ToString());

        if (info.ID == -1 || item.IsFolder)
        {
          if (info.IsEmpty)
          {
            GUIPropertyManager.SetProperty("#hideinfo", "true");

            if (item.Label == "..") // No id for GoToPreviousFolder item
            {
              GUIPropertyManager.SetProperty("#movieid", "-1");
            }
          }
          else
          {
            GUIPropertyManager.SetProperty("#hideinfo", "false");
          }

          GUIPropertyManager.SetProperty("#runtime", info.RunTime +
                                " " +
                                GUILocalizeStrings.Get(2998) +
                                " (" + Util.Utils.SecondsToHMString(info.RunTime * 60) + ")");

          if (info.Duration <= 0)
          {
            GUIPropertyManager.SetProperty("#videoruntime", string.Empty);
          }
          else
          {
            GUIPropertyManager.SetProperty("#videoruntime", Util.Utils.SecondsToHMSString(info.Duration));
          }
        }
        else
        {
          GUIPropertyManager.SetProperty("#hideinfo", "false");
          GUIPropertyManager.SetProperty("#runtime", info.RunTime +
                                " " +
                                GUILocalizeStrings.Get(2998) +
                                " (" + Util.Utils.SecondsToHMString(info.RunTime * 60) + ")");

          if (info.Duration <= 0)
          {
            GUIPropertyManager.SetProperty("#videoruntime", string.Empty);
          }
          else
          {
            GUIPropertyManager.SetProperty("#videoruntime", Util.Utils.SecondsToHMSString(info.Duration));
          }
        }

        // Watched property
        string strValue = "no";

        if (info.Watched > 0)
        {
          strValue = "yes";
        }
        GUIPropertyManager.SetProperty("#iswatched", strValue);

        if (!item.IsFolder && !VirtualDirectories.Instance.Movies.IsRootShare(info.VideoFileName))
        {
          // Watched percent property
          GUIPropertyManager.SetProperty("#watchedpercent", info.WatchedPercent.ToString());
          // Watched count
          GUIPropertyManager.SetProperty("#watchedcount", info.WatchedCount.ToString());
        }
        else
        {
          // Watched percent property
          GUIPropertyManager.SetProperty("#watchedpercent", "0");
          // Watched count
          GUIPropertyManager.SetProperty("#watchedcount", "-1");
        }
        string hasSubtitles = "false";
        string videoMediaSource = string.Empty;

        if (info.MediaInfo.HasSubtitles)
        {
          hasSubtitles = "true";
        }
        
        GUIPropertyManager.SetProperty("#VideoMediaSource", videoMediaSource);
        GUIPropertyManager.SetProperty("#VideoCodec", Util.Utils.MakeFileName(info.MediaInfo.VideoCodec));
        GUIPropertyManager.SetProperty("#VideoResolution", info.MediaInfo.VideoResolution);
        GUIPropertyManager.SetProperty("#AudioCodec", Util.Utils.MakeFileName(info.MediaInfo.AudioCodec));
        GUIPropertyManager.SetProperty("#AudioChannels", info.MediaInfo.AudioChannels);
        GUIPropertyManager.SetProperty("#HasSubtitles", hasSubtitles);
        GUIPropertyManager.SetProperty("#AspectRatio", info.MediaInfo.AspectRatio);
        GUIPropertyManager.SetProperty("#myvideosuserfanart", info.UserFanart);

        
      }
      catch (Exception ex)
      {
        Log.Error("IMDBMovie Set movie properties error: {0}", ex.Message);
      }
    }

    public static void ResetMovieProperties()
    {
      GUIPropertyManager.SetProperty("#director", string.Empty);
      GUIPropertyManager.SetProperty("#genre", string.Empty);
      GUIPropertyManager.SetProperty("#cast", string.Empty);
      GUIPropertyManager.SetProperty("#dvdlabel", string.Empty);
      GUIPropertyManager.SetProperty("#imdbnumber", string.Empty);
      GUIPropertyManager.SetProperty("#file", string.Empty);
      GUIPropertyManager.SetProperty("#plot", string.Empty);
      GUIPropertyManager.SetProperty("#plotoutline", string.Empty);
      GUIPropertyManager.SetProperty("#userreview", string.Empty);
      GUIPropertyManager.SetProperty("#rating", string.Empty);
      GUIPropertyManager.SetProperty("#strrating", string.Empty);
      GUIPropertyManager.SetProperty("#tagline", string.Empty);
      GUIPropertyManager.SetProperty("#votes", string.Empty);
      GUIPropertyManager.SetProperty("#credits", string.Empty);
      GUIPropertyManager.SetProperty("#thumb", string.Empty);
      GUIPropertyManager.SetProperty("#title", string.Empty);
      GUIPropertyManager.SetProperty("#year", string.Empty);
      GUIPropertyManager.SetProperty("#mpaarating", string.Empty);
      GUIPropertyManager.SetProperty("#studios", string.Empty);
      GUIPropertyManager.SetProperty("#country", string.Empty);
      GUIPropertyManager.SetProperty("#language", string.Empty);
      GUIPropertyManager.SetProperty("#lastupdate", string.Empty);
      GUIPropertyManager.SetProperty("#movieid", "-1");
      GUIPropertyManager.SetProperty("#hideinfo", "true");
      GUIPropertyManager.SetProperty("#runtime", string.Empty);
      GUIPropertyManager.SetProperty("#videoruntime", string.Empty);
      GUIPropertyManager.SetProperty("#iswatched", string.Empty);
      GUIPropertyManager.SetProperty("#watchedpercent", string.Empty);
      GUIPropertyManager.SetProperty("#watchedcount", string.Empty);
      GUIPropertyManager.SetProperty("#VideoMediaSource", string.Empty);
      GUIPropertyManager.SetProperty("#VideoCodec", string.Empty);
      GUIPropertyManager.SetProperty("#VideoResolution", string.Empty);
      GUIPropertyManager.SetProperty("#AudioCodec", string.Empty);
      GUIPropertyManager.SetProperty("#AudioChannels", string.Empty);
      GUIPropertyManager.SetProperty("#HasSubtitles", string.Empty);
      GUIPropertyManager.SetProperty("#AspectRatio", string.Empty);
      GUIPropertyManager.SetProperty("#myvideosuserfanart", string.Empty);
    }

    #endregion

    
  }
}