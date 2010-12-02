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

using MediaPortal.GUI.Library;
using MediaPortal.Util;

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
    private string _mStrGenre = string.Empty;
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
    private string _mStrActor = string.Empty;
    private string _mStrgenre = string.Empty;
    // Movie DirectorID
    private int _mDirectorID = -1;
    // User review
    private string _mStrUserReview = string.Empty;
    // Fanart
    private string _mStrFanartURL = string.Empty;

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
      get { return _mStrGenre; }
      set { _mStrGenre = value; }
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
      set {_mStrTitle = value; }
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

    public void Reset()
    {
      _mStrDirector = string.Empty;
      _mStrWritingCredits = string.Empty;
      _mStrGenre = string.Empty;
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
    }

    public void SetProperties()
    {
      // Title suffix for problem with covers and movie with the same name
      string strThumb = GetStrThumb();

      GUIPropertyManager.SetProperty("#director", Director);
      GUIPropertyManager.SetProperty("#genre", Genre);
      GUIPropertyManager.SetProperty("#cast", Cast);
      GUIPropertyManager.SetProperty("#dvdlabel", DVDLabel);
      GUIPropertyManager.SetProperty("#imdbnumber", IMDBNumber);
      GUIPropertyManager.SetProperty("#file", File);
      GUIPropertyManager.SetProperty("#plot", Plot);
      GUIPropertyManager.SetProperty("#plotoutline", PlotOutline);
      GUIPropertyManager.SetProperty("#userreview", UserReview); // Added
      GUIPropertyManager.SetProperty("#rating", Rating.ToString());
      GUIPropertyManager.SetProperty("#tagline", TagLine);
      GUIPropertyManager.SetProperty("#votes", Votes);
      GUIPropertyManager.SetProperty("#credits", WritingCredits);
      GUIPropertyManager.SetProperty("#thumb", strThumb);
      GUIPropertyManager.SetProperty("#title", Title);
      GUIPropertyManager.SetProperty("#year", Year.ToString());
      GUIPropertyManager.SetProperty("#runtime", RunTime.ToString());
      GUIPropertyManager.SetProperty("#mpaarating", MPARating);
      string strValue = "no";
      if (Watched > 0)
      {
        strValue = "yes";
      }
      GUIPropertyManager.SetProperty("#iswatched", strValue);
    }

    public void SetPlayProperties()
    {
      // Title suffix for problem with covers and movie with the same name
      string strThumb = GetStrThumb();

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

    private string GetStrThumb()
    {
      string titleExt = Title + "{" + ID + "}";
      return Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, titleExt);
    }
  }
}