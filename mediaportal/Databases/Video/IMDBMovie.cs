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
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Web;

namespace MediaPortal.Video.Database
{
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
    // Movie DirectorID
    private int _mDirectorID = -1;
    // User review
    private string _mStrUserReview = string.Empty;
    // Fanart
    private string _mStrFanartURL = string.Empty;
    // Date added
    private string _dateAdded = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    // Date watched
    private string _dateWatched = string.Empty;
    // Studios
    private string _mStrStudios = string.Empty;
    // Country
    private string _mStrCountry= string.Empty;
    // Language
    private string _mStrLanguage = string.Empty;
    // Info update date
    private string _lastUpdate = string.Empty;

    public IMDBMovie() {}

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
          return false;
        }
        return true;
      }
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

    // Added DirectorID
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

    // Added UserReview
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

    // Fanart
    public string FanartURL
    {
      get { return _mStrFanartURL; }
      set { _mStrFanartURL = value; }
    }

    // Strip title prefix if needed
    public string Title
    {
      get { return _mStrTitle; }
      set { _mStrTitle = value; }
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
    }

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
      MPARating = Util.Utils.MakeFileName(MPARating);
      GUIPropertyManager.SetProperty("#mpaarating", MPARating);
      GUIPropertyManager.SetProperty("#studios", Studios.Replace(" /", ","));
      GUIPropertyManager.SetProperty("#country", Country);
      GUIPropertyManager.SetProperty("#language", Language);
      DateTime lastUpdate;
      DateTime.TryParseExact(LastUpdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out lastUpdate);
      GUIPropertyManager.SetProperty("#lastupdate", lastUpdate.ToShortDateString());

      if (ID == -1)
      {
        if (IsEmpty)
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
      int percent = 0;
      int timesWatched = 0;
      VideoDatabase.GetmovieWatchedStatus(VideoDatabase.GetMovieId(file), out percent, out timesWatched);
      GUIPropertyManager.SetProperty("#watchedpercent", percent.ToString());
      
      // Watched count
      if (!string.IsNullOrEmpty(file) && System.IO.File.Exists(file))
      {
        GUIPropertyManager.SetProperty("#watchedcount", timesWatched.ToString());
      }
      else
      {
        GUIPropertyManager.SetProperty("#watchedcount", "-1");
      }
      
      // MediaInfo Properties
      try
      {
        ResetMediaInfoProperties();

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
                              " (" + Util.Utils.SecondsToHMString(RunTime * 60) + ")");

      int duration = VideoDatabase.GetMovieDuration(movieId);

      if (duration <= 0)
      {
        GUIPropertyManager.SetProperty("#videoruntime", string.Empty);
      }
      else
      {
        GUIPropertyManager.SetProperty("#videoruntime", Util.Utils.SecondsToHMSString(duration));
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
        VideoFilesMediaInfo mInfo = new VideoFilesMediaInfo();

        VideoDatabase.GetVideoFilesMediaInfo(file, ref mInfo, refresh);

        string hasSubtitles = "false";

        if (mInfo.HasSubtitles)
        {
          hasSubtitles = "true";
        }

        GUIPropertyManager.SetProperty("#VideoCodec", Util.Utils.MakeFileName(mInfo.VideoCodec));
        GUIPropertyManager.SetProperty("#VideoResolution", mInfo.VideoResolution);
        GUIPropertyManager.SetProperty("#AudioCodec", Util.Utils.MakeFileName(mInfo.AudioCodec));
        GUIPropertyManager.SetProperty("#AudioChannels", mInfo.AudioChannels);
        GUIPropertyManager.SetProperty("#HasSubtitles", hasSubtitles);
        GUIPropertyManager.SetProperty("#AspectRatio", mInfo.AspectRatio);
      }
      catch (Exception){}
    }

    private string GetStrThumb()
    {
      string titleExt = Title + "{" + ID + "}";
      return Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
    }
  }
}