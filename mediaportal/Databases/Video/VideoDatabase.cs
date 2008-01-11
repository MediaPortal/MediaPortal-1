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
using MediaPortal.Util;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using SQLite.NET;

namespace MediaPortal.Video.Database
{
  public class VideoDatabase 
  {
    static IVideoDatabase _database = DatabaseFactory.GetVideoDatabase();
    public static void ReOpen()
    {
        _database = DatabaseFactory.GetVideoDatabase();
    }
    public static void Dispose()
    {
      _database.Dispose();
      _database = null;
    }
    public static int AddFile(int lMovieId, int lPathId, string strFileName)
    {
      return _database.AddFile(lMovieId, lPathId, strFileName);
    }
    public static int GetFile(string strFilenameAndPath, out int lPathId, out int lMovieId, bool bExact)
    {
      return _database.GetFile(strFilenameAndPath, out lPathId, out lMovieId, bExact);
    }
    public static int AddMovieFile(string strFile)
    {
      return _database.AddMovieFile(strFile);
    }
    public static int AddPath(string strPath)
    {
      return _database.AddPath(strPath);
    }
    public static int GetPath(string strPath)
    {
      return _database.GetPath(strPath);
    }
    public static void DeleteFile(int iFileId)
    {
      _database.DeleteFile(iFileId);
    }
    public static void RemoveFilesForMovie(int lMovieId)
    {
      _database.RemoveFilesForMovie(lMovieId);
    }
    public static int GetFileId(string strFilenameAndPath)
    {
      return _database.GetFileId(strFilenameAndPath);
    }
    public static void GetFiles(int lMovieId, ref ArrayList movies)
    {
      _database.GetFiles(lMovieId, ref  movies);
    }
    public static int AddGenre(string strGenre1)
    {
      return _database.AddGenre(strGenre1);
    }
    public static void GetGenres(ArrayList genres)
    {
      _database.GetGenres(genres);
    }
    public static void AddGenreToMovie(int lMovieId, int lGenreId)
    {
      _database.AddGenreToMovie(lMovieId, lGenreId);
    }
    public static void DeleteGenre(string genre)
    {
      _database.DeleteGenre(genre);
    }
    public static void RemoveGenresForMovie(int lMovieId)
    {
      _database.RemoveGenresForMovie(lMovieId);
    }
    public static int AddActor(string strActor1)
    {
      return _database.AddActor(strActor1);
    }
    public static void GetActors(ArrayList actors)
    {
      _database.GetActors(actors);
    }
    public static void AddActorToMovie(int lMovieId, int lActorId)
    {
      _database.AddActorToMovie(lMovieId, lActorId);
    }
    public static void DeleteActor(string actor)
    {
      _database.DeleteActor(actor);
    }
    public static void RemoveActorsForMovie(int lMovieId)
    {
      _database.RemoveActorsForMovie(lMovieId);
    }
    public static void ClearBookMarksOfMovie(string strFilenameAndPath)
    {
      _database.ClearBookMarksOfMovie(strFilenameAndPath);
    }
    public static void AddBookMarkToMovie(string strFilenameAndPath, float fTime)
    {
      _database.AddBookMarkToMovie(strFilenameAndPath, fTime);
    }
    public static void GetBookMarksForMovie(string strFilenameAndPath, ref ArrayList bookmarks)
    {
      _database.GetBookMarksForMovie(strFilenameAndPath, ref  bookmarks);
    }
    public static void SetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      _database.SetMovieInfo(strFilenameAndPath, ref  details);
    }
    public static void SetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      _database.SetMovieInfoById(lMovieId, ref  details);
    }
    public static void DeleteMovieInfo(string strFileNameAndPath)
    {
      _database.DeleteMovieInfo(strFileNameAndPath);
    }
    public static void DeleteMovieInfoById(long lMovieId)
    {
      _database.DeleteMovieInfoById(lMovieId);
    }
    public static bool HasMovieInfo(string strFilenameAndPath)
    {
      return _database.HasMovieInfo(strFilenameAndPath);
    }
    public static int GetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      return _database.GetMovieInfo(strFilenameAndPath, ref  details);
    }
    public static void GetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      _database.GetMovieInfoById(lMovieId, ref  details);
    }
    public static void SetWatched(IMDBMovie details)
    {
      _database.SetWatched(details);
    }
    public static void DeleteMovieStopTime(int iFileId)
    {
      _database.DeleteMovieStopTime(iFileId);
    }
    public static int GetMovieStopTime(int iFileId)
    {
      return _database.GetMovieStopTime(iFileId);
    }
    public static void SetMovieStopTime(int iFileId, int stoptime)
    {
      _database.SetMovieStopTime(iFileId, stoptime);
    }
    public static int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData)
    {
      return _database.GetMovieStopTimeAndResumeData(iFileId, out  resumeData);
    }
    public static void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData)
    {
      _database.SetMovieStopTimeAndResumeData(iFileId, stoptime, resumeData);
    }
    public static int GetMovieDuration(int iFileId)
    {
      return _database.GetMovieDuration(iFileId);
    }
    public static void SetMovieDuration(int iFileId, int duration)
    {
      _database.SetMovieDuration(iFileId, duration);
    }
    public static void DeleteMovie(string strFilenameAndPath)
    {
      _database.DeleteMovie(strFilenameAndPath);
    }
    public static int AddMovie(string strFilenameAndPath, bool bHassubtitles)
    {
      return _database.AddMovie(strFilenameAndPath, bHassubtitles);
    }
    public static void GetMovies(ref ArrayList movies)
    {
      _database.GetMovies(ref  movies);
    }
    public static int GetMovieId(string strFilenameAndPath)
    {
      return _database.GetMovieId(strFilenameAndPath);
    }
    public static bool HasSubtitle(string strFilenameAndPath)
    {
      return _database.HasSubtitle(strFilenameAndPath);
    }
    public static void SetThumbURL(int lMovieId, string thumbURL)
    {
      _database.SetThumbURL(lMovieId, thumbURL);
    }
    public static void SetDVDLabel(int lMovieId, string strDVDLabel1)
    {
      _database.SetDVDLabel(lMovieId, strDVDLabel1);
    }
    public static void GetYears(ArrayList years)
    {
      _database.GetYears(years);
    }
    public static void GetMoviesByGenre(string strGenre1, ref ArrayList movies)
    {
      _database.GetMoviesByGenre(strGenre1, ref  movies);
    }
    public static void GetMoviesByActor(string strActor1, ref ArrayList movies)
    {
      _database.GetMoviesByActor(strActor1, ref  movies);
    }
    public static void GetMoviesByYear(string strYear, ref ArrayList movies)
    {
      _database.GetMoviesByYear(strYear, ref  movies);
    }
    public static void GetMoviesByPath(string strPath1, ref ArrayList movies)
    {
      _database.GetMoviesByPath(strPath1, ref  movies);
    }
    public static void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable, bool genreTable)
    {
      _database.GetMoviesByFilter(sql, out  movies, actorTable, movieinfoTable, genreTable);
    }
    public static void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel)
    {
      _database.UpdateCDLabel(movieDetails, CDlabel);
    }
    public static string GetDVDLabel(string strFile)
    {
      return _database.GetDVDLabel(strFile);
    }
    public static void SetActorInfo(int idActor, IMDBActor actor)
    {
      _database.SetActorInfo(idActor, actor);
    }
    public static void AddActorInfoMovie(int idActor, IMDBActor.IMDBActorMovie movie)
    {
      _database.AddActorInfoMovie(idActor, movie);
    }
    public static IMDBActor GetActorInfo(int idActor)
    {
      return _database.GetActorInfo(idActor);
    }
    public static SQLiteResultSet GetResults(string sql)
    {
      return _database.GetResults(sql);
    }
  }
}
