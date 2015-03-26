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
using MediaPortal.Player;
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

    public static bool ClearDB()
    {
      lock (typeof(VideoDatabase))
      {
        return _database.ClearDB();
      }
    }

    public static void Dispose()
    {
      if (_database != null)
      {
        _database.Dispose();
      }
      _database = null;
    }

    public static bool IsConnected()
    {
      return _database.IsConnected();
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
      lock (typeof(VideoDatabase)) 
      {
        return _database.AddFile(lMovieId, lPathId, strFileName);
      }
    }

    public static int GetFile(string strFilenameAndPath, out int lPathId, out int lMovieId, bool bExact)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.GetFile(strFilenameAndPath, out lPathId, out lMovieId, bExact);
      }
    }

    public static int AddMovieFile(string strFile)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.AddMovieFile(strFile);
      }
    }

    public static int AddPath(string strPath)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.AddPath(strPath);
      }
    }

    public static int GetPath(string strPath)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.GetPath(strPath);
      }
    }

    public static void DeleteFile(int iFileId)
    {
      lock (typeof(VideoDatabase))
      {
        _database.DeleteFile(iFileId);
      }
    }

    public static void RemoveFilesForMovie(int lMovieId)
    {
      lock (typeof(VideoDatabase))
      {
        _database.RemoveFilesForMovie(lMovieId);
      }
    }

    /// <summary>
    /// Returns -1 if file don't exists
    /// </summary>
    /// <param name="strFilenameAndPath"></param>
    /// <returns></returns>
    public static int GetFileId(string strFilenameAndPath)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.GetFileId(strFilenameAndPath);
      }
    }

    public static void GetFilesForMovie(int lMovieId, ref ArrayList files)
    {
      lock (typeof(VideoDatabase))
      {
        _database.GetFilesForMovie(lMovieId, ref files);
      }
    }

    #endregion

    #region MediaInfo

    public static void GetVideoFilesMediaInfo(string strFilenameAndPath, ref VideoFilesMediaInfo mediaInfo, bool refresh)
    {
      lock (typeof(VideoDatabase))
      {
        _database.GetVideoFilesMediaInfo(strFilenameAndPath, ref mediaInfo, refresh);
      }
    }

    public static bool HasMediaInfo(string fileName)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.HasMediaInfo(fileName);
      }
    }

    #endregion

    #region Genres

    public static int AddGenre(string strGenre1)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.AddGenre(strGenre1);
      }
    }

    public static void GetGenres(ArrayList genres)
    {
      lock (typeof(VideoDatabase))
      {
        _database.GetGenres(genres);
      }
    }

    public static string GetGenreById(int genreId)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.GetGenreById(genreId);
      }
    }

    public static void AddGenreToMovie(int lMovieId, int lGenreId)
    {
      lock (typeof(VideoDatabase))
      {
        _database.AddGenreToMovie(lMovieId, lGenreId);
      }
    }

    public static void DeleteGenre(string genre)
    {
      lock (typeof(VideoDatabase))
      {
        _database.DeleteGenre(genre);
      }
    }

    public static void RemoveGenresForMovie(int lMovieId)
    {
      lock (typeof(VideoDatabase))
      {
        _database.RemoveGenresForMovie(lMovieId);
      }
    }

    #endregion

    #region User groups

    public static int AddUserGroup(string userGroup)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.AddUserGroup(userGroup);
      }
    }

    public static int GetUserGroupId(string userGroup)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.GetUserGroupId(userGroup);
      }
    }

    public static void AddUserGroupDescription(string userGroup, string description)
    {
      lock (typeof(VideoDatabase))
      {
        _database.AddUserGroupDescription(userGroup, description);
      }
    }
    
    public static void GetUserGroups(ArrayList userGroups)
    {
      lock (typeof(VideoDatabase))
      {
        _database.GetUserGroups(userGroups);
      }
    }

    public static string GetUserGroupById(int groupId)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.GetUserGroupById(groupId);
      }
    }

    public static string GetUserGroupDescriptionById(int groupId)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.GetUserGroupDescriptionById(groupId);
      }
    }
    
    public static void GetMovieUserGroups(int movieId, ArrayList userGroups)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMovieUserGroups(movieId, userGroups);
      }
    }
    
    public static void AddUserGroupToMovie(int lMovieId, int lUserGroupId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.AddUserGroupToMovie(lMovieId, lUserGroupId);
      }
    }


    public static void GetMoviesByYear(ref ArrayList movies)
    {
      lock (typeof(VideoDatabase))
      {
        _database.GetMoviesByYear(ref movies);
      }
    }

    public static void AddUserGroupRuleByGroupId(int groupId, string rule)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.AddUserGroupRuleByGroupId(groupId, rule);
      }
    }

    public static void AddUserGroupRuleByGroupName(string groupName, string rule)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.AddUserGroupRuleByGroupName(groupName, rule);
      }
    }

    public static string GetUserGroupRule(string group)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetUserGroupRule(group);
      }
    }

    public static void RemoveUserGroupFromMovie(int lMovieId, int lUserGroupId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.RemoveUserGroupFromMovie(lMovieId, lUserGroupId);
      }
    }

    public static void DeleteUserGroup(string userGroup)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.DeleteUserGroup(userGroup);
      }
    }

    public static void RemoveUserGroupsForMovie(int lMovieId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.RemoveUserGroupsForMovie(lMovieId);
      }
    }

    public static void RemoveUserGroupRule(string groupName)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.RemoveUserGroupRule(groupName);
      }
    }

    #endregion

    #region Actors

    public static int AddActor(string strActorImdbId, string strActorName)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.AddActor(strActorImdbId, strActorName);
      }
    }

    public static void GetActors(ArrayList actors)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetActors(actors);
      }
    }

    public static string GetActorNameById(int actorId)
    {
      lock (typeof(VideoDatabase))
      {
        return _database.GetActorNameById(actorId);
      }
    }

    public static void GetActorByName(string actorName, ArrayList actors)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetActorByName(actorName, actors);
      }
    }

    public static void GetActorsByMovieID(int idMovie, ref ArrayList actorsByMovieID)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetActorsByMovieID(idMovie, ref actorsByMovieID);
      }
    }

    public static void AddActorToMovie(int lMovieId, int lActorId, string role)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.AddActorToMovie(lMovieId, lActorId, role);
      }
    }

    public static void DeleteActorFromMovie(int movieId, int actorId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.DeleteActorFromMovie(movieId, actorId);
      }
    }

    public static string GetRoleByMovieAndActorId (int lMovieId, int lActorId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetRoleByMovieAndActorId(lMovieId, lActorId);
      }
    }

    public static void DeleteActor(string actorImdbId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.DeleteActor(actorImdbId);
      }
    }

    public static void RemoveActorsForMovie(int lMovieId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.RemoveActorsForMovie(lMovieId);
      }
    }

    #endregion

    #region Bookmarks

    public static void ClearBookMarksOfMovie(string strFilenameAndPath)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.ClearBookMarksOfMovie(strFilenameAndPath);
      }
    }

    public static void AddBookMarkToMovie(string strFilenameAndPath, float fTime)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.AddBookMarkToMovie(strFilenameAndPath, fTime);
      }
    }

    public static void GetBookMarksForMovie(string strFilenameAndPath, ref ArrayList bookmarks)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetBookMarksForMovie(strFilenameAndPath, ref bookmarks);
      }
    }

    #endregion

    #region Movieinfo

    public static void SetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetMovieInfo(strFilenameAndPath, ref details);
      }
    }

    public static void SetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetMovieInfoById(lMovieId, ref details);
      }
    }

    public static void SetMovieInfoById(int lMovieId, ref IMDBMovie details, bool updateTimeStamp)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetMovieInfoById(lMovieId, ref details, updateTimeStamp);
      }
    }

    public static void DeleteMovieInfo(string strFileNameAndPath)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.DeleteMovieInfo(strFileNameAndPath);
      }
    }

    public static void DeleteMovieInfoById(long lMovieId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.DeleteMovieInfoById(lMovieId);
      }
    }

    public static bool HasMovieInfo(string strFilenameAndPath)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.HasMovieInfo(strFilenameAndPath);
      }
    }

    public static int GetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {     
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieInfo(strFilenameAndPath, ref details);
      }
    }

    public static void GetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMovieInfoById(lMovieId, ref details);
      }
    }

    #endregion

    #region Watched status, stoptime

    public static void SetWatched(IMDBMovie details)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetWatched(details);
      }
    }

    public static void SetDateWatched(IMDBMovie details)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetDateWatched(details);
      }
    }

    public static void DeleteMovieStopTime(int iFileId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.DeleteMovieStopTime(iFileId);
      }
    }

    public static int GetMovieStopTime(int iFileId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieStopTime(iFileId);
      }
    }

    public static void SetMovieStopTime(int iFileId, int stoptime)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetMovieStopTime(iFileId, stoptime);
      }
    }

    /// <summary>
    /// Deprecated Method (this one will not use the new Blu-ray Title mode resume)
    /// </summary>
    public static int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieStopTimeAndResumeData(iFileId, out resumeData, g_Player.BdDefaultTitle);
      }
    }

    /// <summary>
    /// Deprecated Method (this one will not use the new Blu-ray Title mode resume)
    /// </summary>
    public static void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetMovieStopTimeAndResumeData(iFileId, stoptime, resumeData, g_Player.BdDefaultTitle);
      }
    }

    public static int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData, int bdtitle)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieStopTimeAndResumeData(iFileId, out resumeData, bdtitle);
      }
    }

    public static void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData, int bdtitle)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetMovieStopTimeAndResumeData(iFileId, stoptime, resumeData, bdtitle);
      }
    }

    public static void SetMovieWatchedStatus(int iMovieId, bool watched, int percent)
    {      
      lock (typeof(VideoDatabase)) 
      {
       _database.SetMovieWatchedStatus(iMovieId, watched, percent);
      }
    }

    public static void MovieWatchedCountIncrease(int idMovie)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.MovieWatchedCountIncrease(idMovie);
      }
    }

    public static void SetIMDBMovies(string sql)
    {
      lock (typeof(VideoDatabase))
      {
        _database.SetIMDBMovies(sql);
      }
    }

    public static void SetIMDBActorId(int actorId, string IMDBActorID)
    {
      lock (typeof(VideoDatabase))
      {
        _database.SetIMDBActorId(actorId, IMDBActorID);
      }
    }

    public static void SetMovieTitle(string movieTitle, int movieId)
    {
      lock (typeof(VideoDatabase))
      {
        _database.SetMovieTitle(movieTitle, movieId);
      }
    }

    public static void SetMovieShortTitle(string movieTitle, int movieId)
    {
      lock (typeof(VideoDatabase))
      {
        _database.SetMovieShortTitle(movieTitle, movieId);
      }
    }

    public static void SetMovieWatchedCount(int movieId, int watchedCount)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetMovieWatchedCount(movieId, watchedCount);
      }
    }

    public static bool GetmovieWatchedStatus(int iMovieId, out int percent, out int timesWatched)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieWatchedStatus(iMovieId, out percent, out timesWatched);
      }
    }

    #endregion

    #region Duration

    public static int GetMovieDuration(int iMovieId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieDuration(iMovieId);
      }
    }

    public static int GetVideoDuration(int iFileId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetVideoDuration(iFileId);
      }
    }

    public static void SetVideoDuration(int iFileId, int duration)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetVideoDuration(iFileId, duration);
      }
    }

    public static void SetMovieDuration(int iMovieId, int duration)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetMovieDuration(iMovieId, duration);
      }
    }

    #endregion

    #region Movie

    public static void DeleteMovie(string strFilenameAndPath)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.DeleteMovie(strFilenameAndPath);
      }
    }

    public static int AddMovie(string strFilenameAndPath, bool bHassubtitles)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.AddMovie(strFilenameAndPath, bHassubtitles);
      }
    }

    public static void GetMovies(ref ArrayList movies)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMovies(ref movies);
      }
    }

    public static int GetMovieId(string strFilenameAndPath)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieId(strFilenameAndPath);
      }
    }

    public static int GetTitleBDId(int iFileId, out byte[] resumeData)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetTitleBDId(iFileId, out resumeData);
      }
    }

    public static bool HasSubtitle(string strFilenameAndPath)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.HasSubtitle(strFilenameAndPath);
      }
    }

    #endregion

    #region Images

    public static void SetThumbURL(int lMovieId, string thumbURL)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetThumbURL(lMovieId, thumbURL);
      }
    }

    public static void SetFanartURL(int lMovieId, string fanartURL)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.SetFanartURL(lMovieId, fanartURL);
      }
    }

    #endregion

    #region Movie queries

    public static void GetYears(ArrayList years)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetYears(years);
      }
    }

    public static void GetMoviesByGenre(string strGenre1, ref ArrayList movies)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMoviesByGenre(strGenre1, ref movies);
      }
    }

    public static void GetRandomMoviesByGenre(string strGenre1, ref ArrayList movies, int limit)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetRandomMoviesByGenre(strGenre1, ref movies, limit);
      }
    }

    public static string GetMovieTitlesByGenre(string strGenre)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieTitlesByGenre(strGenre);
      }
    }

    public static void GetMoviesByUserGroup(string strUserGroup, ref ArrayList movies)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMoviesByUserGroup(strUserGroup, ref movies);
      }
    }

    public static void GetRandomMoviesByUserGroup(string strUserGroup, ref ArrayList movies, int limit)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetRandomMoviesByUserGroup(strUserGroup, ref movies, limit);
      }
    }

    public static string GetMovieTitlesByUserGroup(int idGroup)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieTitlesByUserGroup(idGroup);
      }
    }

    public static void GetMoviesByActor(string strActor1, ref ArrayList movies)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMoviesByActor(strActor1, ref movies);
      }
    }

    public static void GetRandomMoviesByActor(string strActor1, ref ArrayList movies, int limit)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetRandomMoviesByActor(strActor1, ref movies, limit);
      }
    }

    public static string GetMovieTitlesByActor(int actorId)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieTitlesByActor(actorId);
      }
    }

    public static string GetMovieTitlesByDirector(int directorId)
    {     
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieTitlesByDirector(directorId);
      }
    }

    public static void GetMoviesByYear(string strYear, ref ArrayList movies)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMoviesByYear(strYear, ref movies);
      }
    }

    public static void GetRandomMoviesByYear(string strYear, ref ArrayList movies, int limit)
    {     
      lock (typeof(VideoDatabase)) 
      {
        _database.GetRandomMoviesByYear(strYear, ref movies, limit);
     }
    }

    public static string GetMovieTitlesByYear(string strYear)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieTitlesByYear(strYear);
      }
    }

    public static void GetMoviesByPath(string strPath1, ref ArrayList movies)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMoviesByPath(strPath1, ref movies);
      }
    }

    public static void GetRandomMoviesByPath(string strPath1, ref ArrayList movies, int limit)
    {  
      lock (typeof(VideoDatabase)) 
      {
        _database.GetRandomMoviesByPath(strPath1, ref movies, limit);
      }
    }

    public static void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable,
                                         bool genreTable, bool usergroupTable)
    { 
      lock (typeof(VideoDatabase)) 
      {
        _database.GetMoviesByFilter(sql, out movies, actorTable, movieinfoTable, genreTable, usergroupTable);
      }
    }

    public static void GetIndexByFilter(string sql, bool filterNonWordChar, out ArrayList movieList)
    { 
      lock (typeof(VideoDatabase)) 
      {
        _database.GetIndexByFilter(sql, filterNonWordChar, out movieList);
      }
    }

    public static string GetMovieTitlesByIndex(string sql)
    {
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetMovieTitlesByIndex(sql);
      }
    }

    #endregion

    #region CD/DVD labels

    public static void SetDVDLabel(int lMovieId, string strDVDLabel1)
    { 
      lock (typeof(VideoDatabase)) 
      {
        _database.SetDVDLabel(lMovieId, strDVDLabel1);
      }
    }

    public static void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel)
    {
      lock (typeof(VideoDatabase)) 
      {
        _database.UpdateCDLabel(movieDetails, CDlabel);
      }
    }

    public static string GetDVDLabel(string strFile)
    {
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetDVDLabel(strFile);
      }
    }

    #endregion

    #region Actor info

    public static void SetActorInfo(int idActor, IMDBActor actor)
    { 
      lock (typeof(VideoDatabase)) 
      {
        _database.SetActorInfo(idActor, actor);
      }
    }

    public static void AddActorInfoMovie(int idActor, IMDBActor.IMDBActorMovie movie)
    {
      lock (typeof(VideoDatabase)) 
      {
        _database.AddActorInfoMovie(idActor, movie);
      }
    }

    public static IMDBActor GetActorInfo(int idActor)
    { 
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetActorInfo(idActor);
      }
    }

    public static string GetActorImdbId(int idActor)
    { 
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetActorImdbId(idActor);
      }
    }

    #endregion

    #region Thumb blacklist

    public static bool IsVideoThumbBlacklisted(string path)
    { 
      lock (typeof(VideoDatabase)) 
      {
        return _database.IsVideoThumbBlacklisted(path);
      }
    }

    public static int VideoThumbBlacklist(string path, DateTime expiresOn)
    { 
      lock (typeof(VideoDatabase)) 
      {
        return _database.VideoThumbBlacklist(path, expiresOn);
      }
    }

    public static bool VideoThumbRemoveFromBlacklist(string path)
    {
      lock (typeof(VideoDatabase)) 
      {
        return _database.VideoThumbRemoveFromBlacklist(path);
      }
    }

    public static void RemoveExpiredVideoThumbBlacklistEntries()
    {
      lock (typeof(VideoDatabase)) 
      {
        _database.RemoveExpiredVideoThumbBlacklistEntries();
      }
    }

    public static void RemoveAllVideoThumbBlacklistEntries()
    { 
      lock (typeof(VideoDatabase)) 
      {
        _database.RemoveAllVideoThumbBlacklistEntries();
      }
    }

    #endregion

    #region nfo handler

    public static void ImportNfo(string nfoFile, bool skipExisting, bool refreshdbOnly)
    {
      lock (typeof(VideoDatabase)) 
      {
        _database.ImportNfo(nfoFile, skipExisting, refreshdbOnly);
      }
    }

    public static bool MakeNfo (int movieId)
    { 
      lock (typeof(VideoDatabase)) 
      {
        return _database.MakeNfo(movieId);
      }
    }

    public static void ImportNfoUsingVideoFile(string videoFile, bool skipExisting, bool refreshdbOnly)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.ImportNfoUsingVideoFile(videoFile, skipExisting, refreshdbOnly);
      }
    }

    #endregion

    public static SQLiteResultSet GetResults(string sql)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.GetResults(sql);
      }
    }

    public static void ExecuteSql(string sql, out bool error, out string errorMessage)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.ExecuteSQL(sql, out error, out errorMessage);
      }
    }

    public static ArrayList ExecuteRuleSql(string sql, string fieldName, out bool error, out string errorMessage)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.ExecuteRuleSQL(sql, fieldName, out error, out errorMessage);
      }
    }

    /// <summary>
    /// Check if id is valid IMDB movieId (tt1234567) including leading zeros (tt0004567)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool CheckMovieImdbId(string id)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.CheckMovieImdbId(id);
      }
    }

    /// <summary>
    /// Check if id is valid IMDB actoId (nm1234567) including leading zeros (nm0004567)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool CheckActorImdbId(string id)
    {      
      lock (typeof(VideoDatabase)) 
      {
        return _database.CheckActorImdbId(id);
      }
    }
    
    /// <summary>
    /// Returns all video files from path (for files with videoextension defined in MP)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="availableFiles"></param>
    public static void GetVideoFiles(string path, ref ArrayList availableFiles)
    {      
      lock (typeof(VideoDatabase)) 
      {
        _database.GetVideoFiles(path, ref availableFiles);
      }
    }

    /// <summary>
    /// Returns common non word characters including numbers
    /// </summary>
    /// <returns></returns>
    public static string NonwordCharacters()
    {
      string characters =
        @"'1','2','3','4','5','6','7','8','9','''','(',')','[',']','{      lock (typeof(VideoDatabase)) 
      {','}','""','!','#','$','%','&','/','+','-','<','>','.',',',':',';','§','|','_','\','@','€','~','^','ˇ','½','*'";
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

    public static bool DbHealth
    {
      get
      {
        return _database.DbHealth;
      }
    }
  }
}