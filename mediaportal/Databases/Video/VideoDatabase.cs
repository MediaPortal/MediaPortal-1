/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General static public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General static public License for more details.
 *   
 *  You should have received a copy of the GNU General static public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections;
using MediaPortal.Util;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using SQLite.NET;

namespace MediaPortal.Video.Database
{
  public class VideoDatabase 
  {
    static IVideoDatabase _database = DatabaseFactory.GetVideoDatabase();
    static public void ReOpen()
    {
        _database = DatabaseFactory.GetVideoDatabase();
    }
    static public void Dispose()
    {
      _database.Dispose();
      _database = null;
    }
    static public int AddFile(int lMovieId, int lPathId, string strFileName)
    {
      return _database.AddFile(lMovieId, lPathId, strFileName);
    }
    static public int GetFile(string strFilenameAndPath, out int lPathId, out int lMovieId, bool bExact)
    {
      return _database.GetFile(strFilenameAndPath, out lPathId, out lMovieId, bExact);
    }
    static public int AddMovieFile(string strFile)
    {
      return _database.AddMovieFile(strFile);
    }
    static public int AddPath(string strPath)
    {
      return _database.AddPath(strPath);
    }
    static public int GetPath(string strPath)
    {
      return _database.GetPath(strPath);
    }
    static public void DeleteFile(int iFileId)
    {
      _database.DeleteFile(iFileId);
    }
    static public void RemoveFilesForMovie(int lMovieId)
    {
      _database.RemoveFilesForMovie(lMovieId);
    }
    static public int GetFileId(string strFilenameAndPath)
    {
      return _database.GetFileId(strFilenameAndPath);
    }
    static public void GetFiles(int lMovieId, ref ArrayList movies)
    {
      _database.GetFiles(lMovieId, ref  movies);
    }
    static public int AddGenre(string strGenre1)
    {
      return _database.AddGenre(strGenre1);
    }
    static public void GetGenres(ArrayList genres)
    {
      _database.GetGenres(genres);
    }
    static public void AddGenreToMovie(int lMovieId, int lGenreId)
    {
      _database.AddGenreToMovie(lMovieId, lGenreId);
    }
    static public void DeleteGenre(string genre)
    {
      _database.DeleteGenre(genre);
    }
    static public void RemoveGenresForMovie(int lMovieId)
    {
      _database.RemoveGenresForMovie(lMovieId);
    }
    static public int AddActor(string strActor1)
    {
      return _database.AddActor(strActor1);
    }
    static public void GetActors(ArrayList actors)
    {
      _database.GetActors(actors);
    }
    static public void AddActorToMovie(int lMovieId, int lActorId)
    {
      _database.AddActorToMovie(lMovieId, lActorId);
    }
    static public void DeleteActor(string actor)
    {
      _database.DeleteActor(actor);
    }
    static public void RemoveActorsForMovie(int lMovieId)
    {
      _database.RemoveActorsForMovie(lMovieId);
    }
    static public void ClearBookMarksOfMovie(string strFilenameAndPath)
    {
      _database.ClearBookMarksOfMovie(strFilenameAndPath);
    }
    static public void AddBookMarkToMovie(string strFilenameAndPath, float fTime)
    {
      _database.AddBookMarkToMovie(strFilenameAndPath, fTime);
    }
    static public void GetBookMarksForMovie(string strFilenameAndPath, ref ArrayList bookmarks)
    {
      _database.GetBookMarksForMovie(strFilenameAndPath, ref  bookmarks);
    }
    static public void SetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      _database.SetMovieInfo(strFilenameAndPath, ref  details);
    }
    static public void SetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      _database.SetMovieInfoById(lMovieId, ref  details);
    }
    static public void DeleteMovieInfo(string strFileNameAndPath)
    {
      _database.DeleteMovieInfo(strFileNameAndPath);
    }
    static public void DeleteMovieInfoById(long lMovieId)
    {
      _database.DeleteMovieInfoById(lMovieId);
    }
    static public bool HasMovieInfo(string strFilenameAndPath)
    {
      return _database.HasMovieInfo(strFilenameAndPath);
    }
    static public int GetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      return _database.GetMovieInfo(strFilenameAndPath, ref  details);
    }
    static public void GetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      _database.GetMovieInfoById(lMovieId, ref  details);
    }
    static public void SetWatched(IMDBMovie details)
    {
      _database.SetWatched(details);
    }
    static public void DeleteMovieStopTime(int iFileId)
    {
      _database.DeleteMovieStopTime(iFileId);
    }
    static public int GetMovieStopTime(int iFileId)
    {
      return _database.GetMovieStopTime(iFileId);
    }
    static public void SetMovieStopTime(int iFileId, int stoptime)
    {
      _database.SetMovieStopTime(iFileId, stoptime);
    }
    static public int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData)
    {
      return _database.GetMovieStopTimeAndResumeData(iFileId, out  resumeData);
    }
    static public void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData)
    {
      _database.SetMovieStopTimeAndResumeData(iFileId, stoptime, resumeData);
    }
    static public int GetMovieDuration(int iFileId)
    {
      return _database.GetMovieDuration(iFileId);
    }
    static public void SetMovieDuration(int iFileId, int duration)
    {
      _database.SetMovieDuration(iFileId, duration);
    }
    static public void DeleteMovie(string strFilenameAndPath)
    {
      _database.DeleteMovie(strFilenameAndPath);
    }
    static public int AddMovie(string strFilenameAndPath, bool bHassubtitles)
    {
      return _database.AddMovie(strFilenameAndPath, bHassubtitles);
    }
    static public void GetMovies(ref ArrayList movies)
    {
      _database.GetMovies(ref  movies);
    }
    static public int GetMovieId(string strFilenameAndPath)
    {
      return _database.GetMovieId(strFilenameAndPath);
    }
    static public bool HasSubtitle(string strFilenameAndPath)
    {
      return _database.HasSubtitle(strFilenameAndPath);
    }
    static public void SetThumbURL(int lMovieId, string thumbURL)
    {
      _database.SetThumbURL(lMovieId, thumbURL);
    }
    static public void SetDVDLabel(int lMovieId, string strDVDLabel1)
    {
      _database.SetDVDLabel(lMovieId, strDVDLabel1);
    }
    static public void GetYears(ArrayList years)
    {
      _database.GetYears(years);
    }
    static public void GetMoviesByGenre(string strGenre1, ref ArrayList movies)
    {
      _database.GetMoviesByGenre(strGenre1, ref  movies);
    }
    static public void GetMoviesByActor(string strActor1, ref ArrayList movies)
    {
      _database.GetMoviesByActor(strActor1, ref  movies);
    }
    static public void GetMoviesByYear(string strYear, ref ArrayList movies)
    {
      _database.GetMoviesByYear(strYear, ref  movies);
    }
    static public void GetMoviesByPath(string strPath1, ref ArrayList movies)
    {
      _database.GetMoviesByPath(strPath1, ref  movies);
    }
    static public void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable, bool genreTable)
    {
      _database.GetMoviesByFilter(sql, out  movies, actorTable, movieinfoTable, genreTable);
    }
    static public void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel)
    {
      _database.UpdateCDLabel(movieDetails, CDlabel);
    }
    static public string GetDVDLabel(string strFile)
    {
      return _database.GetDVDLabel(strFile);
    }
    static public void AddActorInfo(int idActor, IMDBActor actor)
    {
      _database.AddActorInfo(idActor, actor);
    }
    static public void AddActorInfoMovie(int idActor, IMDBActor.IMDBActorMovie movie)
    {
      _database.AddActorInfoMovie(idActor, movie);
    }
    static public IMDBActor GetActorInfo(int idActor)
    {
      return _database.GetActorInfo(idActor);
    }
    static public SQLiteResultSet GetResults(string sql)
    {
      return _database.GetResults(sql);
    }
  }
}
