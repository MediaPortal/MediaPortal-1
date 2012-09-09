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
using MediaPortal.Database;
using SQLite.NET;

namespace MediaPortal.Video.Database
{
  public class VideoDatabase
  {
    private static IVideoDatabase _database = DatabaseFactory.GetVideoDatabase();
    public static readonly VideoDatabase Instance = new VideoDatabase();

    public static void ReOpen()
    {
      Dispose();
      _database = DatabaseFactory.GetVideoDatabase();
    }

    public static void Dispose()
    {
      if (_database != null)
      {
        _database.Dispose();
      }
      _database = null;
    }

    public static string DatabaseName
    {
      get
      {
        if (_database != null)
        {
          return _database.DatabaseName;
        }
        return "";
      }
    }

    #region Files

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

    /// <summary>
    /// Returns -1 if file don't exists
    /// </summary>
    /// <param name="strFilenameAndPath"></param>
    /// <returns></returns>
    public static int GetFileId(string strFilenameAndPath)
    {
      return _database.GetFileId(strFilenameAndPath);
    }

    public static void GetFilesForMovie(int lMovieId, ref ArrayList files)
    {
      _database.GetFilesForMovie(lMovieId, ref files);
    }

    #endregion

    #region MediaInfo

    public static void GetVideoFilesMediaInfo(string strFilenameAndPath, ref VideoFilesMediaInfo mediaInfo, bool refresh)
    {
      _database.GetVideoFilesMediaInfo(strFilenameAndPath, ref mediaInfo, refresh);
    }

    public static bool HasMediaInfo(string fileName)
    {
      return _database.HasMediaInfo(fileName);
    }

    #endregion

    #region Genres

    public static int AddGenre(string strGenre1)
    {
      return _database.AddGenre(strGenre1);
    }

    public static void GetGenres(ArrayList genres)
    {
      _database.GetGenres(genres);
    }

    public static string GetGenreById(int genreId)
    {
      return _database.GetGenreById(genreId);
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

    #endregion

    #region User groups

    public static int AddUserGroup(string userGroup)
    {
      return _database.AddUserGroup(userGroup);
    }

    public static void AddUserGroupDescription(string userGroup, string description)
    {
      _database.AddUserGroupDescription(userGroup, description);
    }
    
    public static void GetUserGroups(ArrayList userGroups)
    {
      _database.GetUserGroups(userGroups);
    }

    public static string GetUserGroupById(int groupId)
    {
      return _database.GetUserGroupById(groupId);
    }

    public static string GetUserGroupDescriptionById(int groupId)
    {
      return _database.GetUserGroupDescriptionById(groupId);
    }
    
    public static void GetMovieUserGroups(int movieId, ArrayList userGroups)
    {
      _database.GetMovieUserGroups(movieId, userGroups);
    }
    
    public static void AddUserGroupToMovie(int lMovieId, int lUserGroupId)
    {
      _database.AddUserGroupToMovie(lMovieId, lUserGroupId);
    }

    public static void AddUserGroupRuleByGroupId(int groupId, string rule)
    {
      _database.AddUserGroupRuleByGroupId(groupId, rule);
    }

    public static void AddUserGroupRuleByGroupName(string groupName, string rule)
    {
      _database.AddUserGroupRuleByGroupName(groupName, rule);
    }

    public static string GetUserGroupRule(string group)
    {
      return _database.GetUserGroupRule(group);
    }

    public static void RemoveUserGroupFromMovie(int lMovieId, int lUserGroupId)
    {
      _database.RemoveUserGroupFromMovie(lMovieId, lUserGroupId);
    }

    public static void DeleteUserGroup(string userGroup)
    {
      _database.DeleteUserGroup(userGroup);
    }

    public static void RemoveUserGroupsForMovie(int lMovieId)
    {
      _database.RemoveUserGroupsForMovie(lMovieId);
    }

    public static void RemoveUserGroupRule(string groupName)
    {
      _database.RemoveUserGroupRule(groupName);
    }

    #endregion

    #region Actors

    public static int AddActor(string strActorImdbId, string strActorName)
    {
      return _database.AddActor(strActorImdbId, strActorName);
    }

    public static void GetActors(ArrayList actors)
    {
      _database.GetActors(actors);
    }

    public static string GetActorNameById(int actorId)
    {
      return _database.GetActorNameById(actorId);
    }

    public static void GetActorByName(string actorName, ArrayList actors)
    {
      _database.GetActorByName(actorName, actors);
    }

    public static void GetActorsByMovieID(int idMovie, ref ArrayList actorsByMovieID)
    {
      _database.GetActorsByMovieID(idMovie, ref actorsByMovieID);
    }

    public static void AddActorToMovie(int lMovieId, int lActorId, string role)
    {
      _database.AddActorToMovie(lMovieId, lActorId, role);
    }

    public static void DeleteActorFromMovie(int movieId, int actorId)
    {
      _database.DeleteActorFromMovie(movieId, actorId);
    }

    public static string GetRoleByMovieAndActorId (int lMovieId, int lActorId)
    {
      return _database.GetRoleByMovieAndActorId(lMovieId, lActorId);
    }

    public static void DeleteActor(string actorImdbId)
    {
      _database.DeleteActor(actorImdbId);
    }

    public static void RemoveActorsForMovie(int lMovieId)
    {
      _database.RemoveActorsForMovie(lMovieId);
    }

    #endregion

    #region Bookmarks

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
      _database.GetBookMarksForMovie(strFilenameAndPath, ref bookmarks);
    }

    #endregion

    #region Movieinfo

    public static void SetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      _database.SetMovieInfo(strFilenameAndPath, ref details);
    }

    public static void SetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      _database.SetMovieInfoById(lMovieId, ref details);
    }

    public static void SetMovieInfoById(int lMovieId, ref IMDBMovie details, bool updateTimeStamp)
    {
      _database.SetMovieInfoById(lMovieId, ref details, updateTimeStamp);
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
      return _database.GetMovieInfo(strFilenameAndPath, ref details);
    }

    public static void GetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      _database.GetMovieInfoById(lMovieId, ref details);
    }

    #endregion

    #region Watched status, stoptime

    public static void SetWatched(IMDBMovie details)
    {
      _database.SetWatched(details);
    }

    public static void SetDateWatched(IMDBMovie details)
    {
      _database.SetDateWatched(details);
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
      return _database.GetMovieStopTimeAndResumeData(iFileId, out resumeData);
    }

    public static void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData)
    {
      _database.SetMovieStopTimeAndResumeData(iFileId, stoptime, resumeData);
    }

    public static void SetMovieWatchedStatus(int iMovieId, bool watched, int percent)
    {
      _database.SetMovieWatchedStatus(iMovieId, watched, percent);
    }

    public static void MovieWatchedCountIncrease(int idMovie)
    {
      _database.MovieWatchedCountIncrease(idMovie);
    }

    public static void SetMovieWatchedCount(int movieId, int watchedCount)
    {
      _database.SetMovieWatchedCount(movieId, watchedCount);
    }

    public static bool GetmovieWatchedStatus(int iMovieId, out int percent, out int timesWatched)
    {
      return _database.GetMovieWatchedStatus(iMovieId, out percent, out timesWatched);
    }

    #endregion

    #region Duration

    public static int GetMovieDuration(int iMovieId)
    {
      return _database.GetMovieDuration(iMovieId);
    }

    public static int GetVideoDuration(int iFileId)
    {
      return _database.GetVideoDuration(iFileId);
    }

    public static void SetVideoDuration(int iFileId, int duration)
    {
      _database.SetVideoDuration(iFileId, duration);
    }

    public static void SetMovieDuration(int iMovieId, int duration)
    {
      _database.SetMovieDuration(iMovieId, duration);
    }

    #endregion

    #region Movie

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
      _database.GetMovies(ref movies);
    }

    public static int GetMovieId(string strFilenameAndPath)
    {
      return _database.GetMovieId(strFilenameAndPath);
    }

    public static bool HasSubtitle(string strFilenameAndPath)
    {
      return _database.HasSubtitle(strFilenameAndPath);
    }

    #endregion

    #region Images

    public static void SetThumbURL(int lMovieId, string thumbURL)
    {
      _database.SetThumbURL(lMovieId, thumbURL);
    }

    public static void SetFanartURL(int lMovieId, string fanartURL)
    {
      _database.SetFanartURL(lMovieId, fanartURL);
    }

    #endregion

    #region Movie queries

    public static void GetYears(ArrayList years)
    {
      _database.GetYears(years);
    }

    public static void GetMoviesByGenre(string strGenre1, ref ArrayList movies)
    {
      _database.GetMoviesByGenre(strGenre1, ref movies);
    }

    public static void GetMoviesByUserGroup(string strUserGroup, ref ArrayList movies)
    {
      _database.GetMoviesByUserGroup(strUserGroup, ref movies);
    }

    public static void GetMoviesByActor(string strActor1, ref ArrayList movies)
    {
      _database.GetMoviesByActor(strActor1, ref movies);
    }

    public static void GetMoviesByYear(string strYear, ref ArrayList movies)
    {
      _database.GetMoviesByYear(strYear, ref movies);
    }

    public static void GetMoviesByPath(string strPath1, ref ArrayList movies)
    {
      _database.GetMoviesByPath(strPath1, ref movies);
    }

    public static void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable,
                                         bool genreTable, bool usergroupTable)
    {
      _database.GetMoviesByFilter(sql, out movies, actorTable, movieinfoTable, genreTable, usergroupTable);
    }

    public static void GetIndexByFilter(string sql, bool filterNonWordChar, out ArrayList movieList)
    {
      _database.GetIndexByFilter(sql, filterNonWordChar, out movieList);
    }

    #endregion

    #region CD/DVD labels

    public static void SetDVDLabel(int lMovieId, string strDVDLabel1)
    {
      _database.SetDVDLabel(lMovieId, strDVDLabel1);
    }

    public static void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel)
    {
      _database.UpdateCDLabel(movieDetails, CDlabel);
    }

    public static string GetDVDLabel(string strFile)
    {
      return _database.GetDVDLabel(strFile);
    }

    #endregion

    #region Actor info

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

    public static string GetActorImdbId(int idActor)
    {
      return _database.GetActorImdbId(idActor);
    }

    #endregion

    #region Thumb blacklist

    public static bool IsVideoThumbBlacklisted(string path)
    {
      return _database.IsVideoThumbBlacklisted(path);
    }

    public static int VideoThumbBlacklist(string path, DateTime expiresOn)
    {
      return _database.VideoThumbBlacklist(path, expiresOn);
    }

    public static bool VideoThumbRemoveFromBlacklist(string path)
    {
      return _database.VideoThumbRemoveFromBlacklist(path);
    }

    public static void RemoveExpiredVideoThumbBlacklistEntries()
    {
      _database.RemoveExpiredVideoThumbBlacklistEntries();
    }

    public static void RemoveAllVideoThumbBlacklistEntries()
    {
      _database.RemoveAllVideoThumbBlacklistEntries();
    }

    #endregion

    #region nfo handler

    public static void ImportNfo(string nfoFile, bool skipExisting, bool refreshdbOnly)
    {
      _database.ImportNfo(nfoFile, skipExisting, refreshdbOnly);
    }

    public static bool MakeNfo (int movieId)
    {
      return _database.MakeNfo(movieId);
    }

    public static void ImportNfoUsingVideoFile(string videoFile, bool skipExisting, bool refreshdbOnly)
    {
      _database.ImportNfoUsingVideoFile(videoFile, skipExisting, refreshdbOnly);
    }

    #endregion

    public static SQLiteResultSet GetResults(string sql)
    {
      return _database.GetResults(sql);
    }

    public static void ExecuteSql(string sql, out bool error)
    {
      _database.ExecuteSQL(sql, out error);
    }

    public static ArrayList ExecuteRuleSql(string sql, string fieldName, out bool error)
    {
      return _database.ExecuteRuleSQL(sql, fieldName, out error);
    }

    /// <summary>
    /// Check if id is valid IMDB movieId (tt1234567) including leading zeros (tt0004567)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool CheckMovieImdbId(string id)
    {
      return _database.CheckMovieImdbId(id);
    }

    /// <summary>
    /// Check if id is valid IMDB actoId (nm1234567) including leading zeros (nm0004567)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool CheckActorImdbId(string id)
    {
      return _database.CheckActorImdbId(id);
    }
    
    /// <summary>
    /// Returns all video files from path (for files with videoextension defined in MP)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="availableFiles"></param>
    public static void GetVideoFiles(string path, ref ArrayList availableFiles)
    {
      _database.GetVideoFiles(path, ref availableFiles);
    }

    /// <summary>
    /// Returns common non word characters including numbers
    /// </summary>
    /// <returns></returns>
    public static string NonwordCharacters()
    {
      string characters =
        @"'1','2','3','4','5','6','7','8','9','''','(',')','[',']','{','}','""','!','#','$','%','&','/','+','-','<','>','.',',',':',';','§','|','_','\','@','€','~','^','ˇ','½','*'";
      return characters;
    }

    public static void FlushTransactionsToDisk()
    {
      _database.FlushTransactionsToDisk();
    }

    public static void RevertFlushTransactionsToDisk()
    {
      _database.RevertFlushTransactionsToDisk();
    }
  }
}