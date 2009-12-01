#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using SQLite.NET;

namespace MediaPortal.Video.Database
{
  public interface IVideoDatabase
  {
    void Dispose();
    int AddFile(int lMovieId, int lPathId, string strFileName);
    int GetFile(string strFilenameAndPath, out int lPathId, out int lMovieId, bool bExact);
    int AddMovieFile(string strFile);
    int AddPath(string strPath);
    int GetPath(string strPath);
    void DeleteFile(int iFileId);
    void RemoveFilesForMovie(int lMovieId);
    int GetFileId(string strFilenameAndPath);
    void GetFiles(int lMovieId, ref ArrayList movies);
    int AddGenre(string strGenre1);
    void GetGenres(ArrayList genres);
    void AddGenreToMovie(int lMovieId, int lGenreId);
    void DeleteGenre(string genre);
    void RemoveGenresForMovie(int lMovieId);
    int AddActor(string strActor1);
    void GetActors(ArrayList actors);
    void AddActorToMovie(int lMovieId, int lActorId);
    void DeleteActor(string actor);
    void RemoveActorsForMovie(int lMovieId);
    void ClearBookMarksOfMovie(string strFilenameAndPath);
    void AddBookMarkToMovie(string strFilenameAndPath, float fTime);
    void GetBookMarksForMovie(string strFilenameAndPath, ref ArrayList bookmarks);
    void SetMovieInfo(string strFilenameAndPath, ref IMDBMovie details);
    void SetMovieInfoById(int lMovieId, ref IMDBMovie details);
    void DeleteMovieInfo(string strFileNameAndPath);
    void DeleteMovieInfoById(long lMovieId);
    bool HasMovieInfo(string strFilenameAndPath);
    int GetMovieInfo(string strFilenameAndPath, ref IMDBMovie details);
    void GetMovieInfoById(int lMovieId, ref IMDBMovie details);
    void SetWatched(IMDBMovie details);
    void DeleteMovieStopTime(int iFileId);
    int GetMovieStopTime(int iFileId);
    void SetMovieStopTime(int iFileId, int stoptime);
    int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData);
    void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData);
    int GetMovieDuration(int iFileId);
    void SetMovieDuration(int iFileId, int duration);
    void DeleteMovie(string strFilenameAndPath);
    int AddMovie(string strFilenameAndPath, bool bHassubtitles);
    void GetMovies(ref ArrayList movies);
    int GetMovieId(string strFilenameAndPath);
    bool HasSubtitle(string strFilenameAndPath);
    void SetThumbURL(int lMovieId, string thumbURL);
    void SetDVDLabel(int lMovieId, string strDVDLabel1);
    void GetYears(ArrayList years);
    void GetMoviesByGenre(string strGenre1, ref ArrayList movies);
    void GetMoviesByActor(string strActor1, ref ArrayList movies);
    void GetMoviesByYear(string strYear, ref ArrayList movies);
    void GetMoviesByPath(string strPath1, ref ArrayList movies);
    void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable, bool genreTable);
    void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel);
    string GetDVDLabel(string strFile);
    void SetActorInfo(int idActor, IMDBActor actor);
    void AddActorInfoMovie(int idActor, IMDBActor.IMDBActorMovie movie);
    IMDBActor GetActorInfo(int idActor);
    bool IsVideoThumbBlacklisted(string path);
    int VideoThumbBlacklist(string path, DateTime expiresOn);
    bool VideoThumbRemoveFromBlacklist(string path);
    void RemoveExpiredVideoThumbBlacklistEntries();
    void RemoveAllVideoThumbBlacklistEntries();
    SQLiteResultSet GetResults(string sql);
    string DatabaseName
    {
      get;
    }
  }
}