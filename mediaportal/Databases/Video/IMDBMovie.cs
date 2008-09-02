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
    int m_id = -1;
    string m_strDirector = string.Empty;
    string m_strWritingCredits = string.Empty;
    string m_strGenre = string.Empty;
    string m_strTagLine = string.Empty;
    string m_strPlotOutline = string.Empty;
    string m_strPlot = string.Empty;
    string m_strPictureURL = string.Empty;
    string m_strTitle = string.Empty;
    string m_strVotes = string.Empty;
    string m_strCast = string.Empty;
    string m_strSearchString = string.Empty;
    string m_strFile = string.Empty;
    string m_strPath = string.Empty;
    string m_strDVDLabel = string.Empty;
    string m_strIMDBNumber = string.Empty;
    string m_strDatabase = string.Empty;
    string m_strCDLabel = string.Empty;
    int m_iTop250 = 0;
    int m_iYear = 1900;
    float m_fRating = 0.0f;
    string m_strMPARating = string.Empty;
    int m_iRunTime = 0;
    int m_iWatched = 0;
    int m_actorid = -1;
    int m_genreid = -1;
    string m_strActor = string.Empty;
    string m_strgenre = string.Empty;

    public IMDBMovie()
    {
    }
    public int ID
    {
      get { return m_id; }
      set { m_id = value; }
    }
    public bool IsEmpty
    {
      get
      {
        if ((m_strTitle != string.Empty) && (m_strTitle != Strings.Unknown)) return false;
        return true;
      }
    }
    public int actorId
    {
      get { return m_actorid; }
      set { m_actorid = value; }
    }
    public int genreId
    {
      get { return m_genreid; }
      set { m_genreid = value; }
    }
    public string Genre
    {
      get { return m_strgenre; }
      set { m_strgenre = value; }
    }
    public string Actor
    {
      get { return m_strActor; }
      set { m_strActor = value; }
    }
    public int RunTime
    {
      get { return m_iRunTime; }
      set { m_iRunTime = value; }
    }
    public int Watched
    {
      get { return m_iWatched; }
      set { m_iWatched = value; }
    }
    public string MPARating
    {
      get { return m_strMPARating; }
      set { m_strMPARating = value; }
    }
    public string Director
    {
      get { return m_strDirector; }
      set { m_strDirector = value; }
    }
    public string WritingCredits
    {
      get { return m_strWritingCredits; }
      set { m_strWritingCredits = value; }
    }
    public string SingleGenre
    {
      get { return m_strGenre; }
      set { m_strGenre = value; }
    }
    public string TagLine
    {
      get { return m_strTagLine; }
      set { m_strTagLine = value; }
    }
    public string PlotOutline
    {
      get { return m_strPlotOutline; }
      set { m_strPlotOutline = value; }
    }
    public string Plot
    {
      get { return m_strPlot; }
      set { m_strPlot = value; }
    }
    public string ThumbURL
    {
      get { return m_strPictureURL; }
      set { m_strPictureURL = value; }
    }
    public string Title
    {
      get { return m_strTitle; }
      set { m_strTitle = value; }
    }
    public string Votes
    {
      get { return m_strVotes; }
      set { m_strVotes = value; }
    }
    public string Cast
    {
      get { return m_strCast; }
      set { m_strCast = value; }
    }
    public string SearchString
    {
      get { return m_strSearchString; }
      set { m_strSearchString = value; }
    }
    public string File
    {
      get { return m_strFile; }
      set { m_strFile = value; }
    }
    public string Path
    {
      get { return m_strPath; }
      set { m_strPath = value; }
    }
    public string DVDLabel
    {
      get { return m_strDVDLabel; }
      set { m_strDVDLabel = value; }
    }

    public string CDLabel
    {
      get { return m_strCDLabel; }
      set { m_strCDLabel = value; }
    }
    public string IMDBNumber
    {
      get { return m_strIMDBNumber; }
      set { m_strIMDBNumber = value; }
    }
    public int Top250
    {
      get { return m_iTop250; }
      set { m_iTop250 = value; }
    }
    public int Year
    {
      get { return m_iYear; }
      set { m_iYear = value; }
    }
    public float Rating
    {
      get { return m_fRating; }
      set { m_fRating = value; }
    }
    public string Database
    {
      get { return m_strDatabase; }
      set { m_strDatabase = value; }
    }
    public void Reset()
    {
      m_strDirector = string.Empty;
      m_strWritingCredits = string.Empty;
      m_strGenre = string.Empty;
      m_strTagLine = string.Empty;
      m_strPlotOutline = string.Empty;
      m_strPlot = string.Empty;
      m_strPictureURL = string.Empty;
      m_strTitle = string.Empty;
      m_strVotes = string.Empty;
      m_strCast = string.Empty;
      m_strSearchString = string.Empty;
      m_strIMDBNumber = string.Empty;
      m_iTop250 = 0;
      m_iYear = 1900;
      m_fRating = 0.0f;
      m_strDatabase = string.Empty;
      m_strMPARating = string.Empty;
      m_iRunTime = 0;
      m_iWatched = 0;
    }

    public void SetProperties()
    {
      string strThumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, Title);
      GUIPropertyManager.SetProperty("#director", Director);
      GUIPropertyManager.SetProperty("#genre", Genre);
      GUIPropertyManager.SetProperty("#cast", Cast);
      GUIPropertyManager.SetProperty("#dvdlabel", DVDLabel);
      GUIPropertyManager.SetProperty("#imdbnumber", IMDBNumber);
      GUIPropertyManager.SetProperty("#file", File);
      GUIPropertyManager.SetProperty("#plot", Plot);
      GUIPropertyManager.SetProperty("#plotoutline", PlotOutline);
      GUIPropertyManager.SetProperty("#rating", Rating.ToString());
      GUIPropertyManager.SetProperty("#tagline", TagLine);
      GUIPropertyManager.SetProperty("#votes", Votes);
      GUIPropertyManager.SetProperty("#credits", WritingCredits);
      GUIPropertyManager.SetProperty("#thumb", strThumb);
      GUIPropertyManager.SetProperty("#title", Title);
      GUIPropertyManager.SetProperty("#year", Year.ToString());
      GUIPropertyManager.SetProperty("#runtime", RunTime.ToString());
      GUIPropertyManager.SetProperty("#mpaarating", MPARating.ToString());
      string strValue = "no";
      if (Watched > 0) strValue = "yes";
      GUIPropertyManager.SetProperty("#iswatched", strValue);
    }

    public void SetPlayProperties()
    {
      string strThumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MovieTitle, Title);
      GUIPropertyManager.SetProperty("#Play.Current.Director", Director);
      GUIPropertyManager.SetProperty("#Play.Current.Genre", Genre);
      GUIPropertyManager.SetProperty("#Play.Current.Cast", Cast);
      GUIPropertyManager.SetProperty("#Play.Current.DVDLabel", DVDLabel);
      GUIPropertyManager.SetProperty("#Play.Current.IMDBNumber", IMDBNumber);
      GUIPropertyManager.SetProperty("#Play.Current.File", File);
      GUIPropertyManager.SetProperty("#Play.Current.Plot", Plot);
      GUIPropertyManager.SetProperty("#Play.Current.PlotOutline", PlotOutline);
      GUIPropertyManager.SetProperty("#Play.Current.Rating", Rating.ToString());
      GUIPropertyManager.SetProperty("#Play.Current.TagLine", TagLine);
      GUIPropertyManager.SetProperty("#Play.Current.Votes", Votes);
      GUIPropertyManager.SetProperty("#Play.Current.Credits", WritingCredits);
      GUIPropertyManager.SetProperty("#Play.Current.Thumb", strThumb);
      GUIPropertyManager.SetProperty("#Play.Current.Title", Title);
      GUIPropertyManager.SetProperty("#Play.Current.Year", Year.ToString());
      GUIPropertyManager.SetProperty("#Play.Current.Runtime", RunTime.ToString());
      GUIPropertyManager.SetProperty("#Play.Current.MPAARating", MPARating.ToString());
      string strValue = "no";
      if (Watched > 0) strValue = "yes";
      GUIPropertyManager.SetProperty("#Play.Current.IsWatched", strValue);
    }

  }
}
