#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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
using SQLite.NET;

namespace MediaPortal.Video.Database
{
  public interface IVideoDatabase
  {
    void Dispose();
    
    // Files & Path
    int AddFile(int lMovieId, int lPathId, string strFileName);
    int GetFile(string strFilenameAndPath, out int lPathId, out int lMovieId, bool bExact);
    int AddMovieFile(string strFile);
    int AddPath(string strPath);
    int GetPath(string strPath);
    void DeleteFile(int iFileId);
    void RemoveFilesForMovie(int lMovieId);
    int GetFileId(string strFilenameAndPath);
    void GetFilesForMovie(int lMovieId, ref ArrayList movies);
    
    // Genre
    int AddGenre(string strGenre1);
    void GetGenres(ArrayList genres);
    string GetGenreById(int genreId);
    void AddGenreToMovie(int lMovieId, int lGenreId);
    void DeleteGenre(string genre);
    void RemoveGenresForMovie(int lMovieId);
    string GetGenresForMovie(int lMovieId);
    void GetMovieGenres(int lMovieId, ArrayList movieGenres);
    
    // Movie Collection
    int AddCollection(string strCollection1);
    int GetCollectionId(string movieCollection);
    void AddCollectionDescription(string movieCollection, string description);
    string GetCollectionDescriptionById(int collectionId);
    void GetCollections(ArrayList collections);
    string GetCollectionById(int collectionId);
    void AddCollectionToMovie(int lMovieId, int lCollectionId);
    void DeleteCollection(string collection);
    void DeleteEmptyCollections();
    void RemoveCollectionFromMovie(int lMovieId, int lCollectionId);
    void RemoveCollectionsForMovie(int lMovieId);
    string GetCollectionsForMovie(int lMovieId);
    void GetMovieCollections(int lMovieId, ArrayList movieCollections);
    bool GetMovieCollectionWatchedStatus(string collection, out int percent);
    bool GetMovieCollectionWatchedStatus(int collection, out int percent);
    
    // User groups
    int AddUserGroup(string userGroup);
    void AddUserGroupDescription(string userGroup, string description);
    void GetUserGroups(ArrayList userGroups);
    int GetUserGroupId(string userGroup);
    string GetUserGroupById(int groupId);
    string GetUserGroupDescriptionById(int groupId);
    string GetUserGroupsForMovie(int lMovieId);
    void GetMovieUserGroups(int movieId, ArrayList userGroups);
    string GetUserGroupRule(string group);
    void AddUserGroupRuleByGroupId(int groupId, string rule);
    void AddUserGroupRuleByGroupName(string groupName, string rule);
    void AddUserGroupToMovie(int lMovieId, int lUserGroupId);
    void RemoveUserGroupFromMovie(int lMovieId, int lUserGroupId);
    void DeleteUserGroup(string userGroup);
    void RemoveUserGroupsForMovie(int lMovieId);
    void RemoveUserGroupRule(string groupName);
    bool GetUserGroupWatchedStatus(string group, out int percent);
    bool GetUserGroupWatchedStatus(int group, out int percent);
    
    // Actors
    int AddActor(string strActorImdbId, string strActorName);
    void GetActors(ArrayList actors);
    string GetActorNameById(int actorId);
    void GetActorByName(string actorName, ArrayList actors);
    void GetActorsByMovieID(int idMovie, ref ArrayList actors);
    string GetRoleByMovieAndActorId(int lMovieId, int lActorId);
    void AddActorToMovie(int lMovieId, int lActorId, string role);
    void DeleteActorFromMovie(int movieId, int actorId);
    void DeleteActor(string actorImdbId);
    void RemoveActorsForMovie(int lMovieId);
    
    // Actor info
    void SetActorInfo(int idActor, IMDBActor actor);
    void AddActorInfoMovie(int idActor, IMDBActor.IMDBActorMovie movie);
    IMDBActor GetActorInfo(int idActor);
    string GetActorImdbId(int idActor);
    
    // Bookmarks
    void ClearBookMarksOfMovie(string strFilenameAndPath);
    void AddBookMarkToMovie(string strFilenameAndPath, float fTime);
    void GetBookMarksForMovie(string strFilenameAndPath, ref ArrayList bookmarks);
    
    // Movie info
    void SetMovieInfo(string strFilenameAndPath, ref IMDBMovie details);
    void SetMovieInfoById(int lMovieId, ref IMDBMovie details);
    void SetMovieInfoById(int lMovieId, ref IMDBMovie details, bool updateTimeStamp);
    void DeleteMovieInfo(string strFileNameAndPath);
    void DeleteMovieInfoById(long lMovieId);
    bool HasMovieInfo(string strFilenameAndPath);
    int GetMovieInfo(string strFilenameAndPath, ref IMDBMovie details);
    void GetMovieInfoById(int lMovieId, ref IMDBMovie details);
    void SetWatched(IMDBMovie details);
    void SetDateWatched(IMDBMovie details);
    void SetMovieTitleById(int lMovieId, string lmovieTitle);
    void SetMovieTitleById(int lMovieId, string lmovieTitle, out bool error, out string errorMessage);
    void SetMovieSortTitleById(int lMovieId, string lmovieTitle);
    void SetMovieSortTitleById(int lMovieId, string lmovieTitle, out bool error, out string errorMessage);
    
    // Stop time & duration
    void DeleteMovieStopTime(int iFileId);
    int GetMovieStopTime(int iFileId);
    void SetMovieStopTime(int iFileId, int stoptime);
    int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData, int bdtitle);
    void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData, int bdtitle);
    int GetVideoDuration(int iFileId);
    int GetMovieDuration(int iMovieId);
    void SetVideoDuration(int iFileId, int duration);
    void SetMovieDuration(int iMovieId, int duration);
    
    // Watched status
    void SetMovieWatchedStatus(int iFileId, bool watched, int percent);
    void MovieWatchedCountIncrease(int idMovie);
    void SetMovieWatchedCount(int movieId, int watchedCount);
    bool GetMovieWatchedStatus(int iFileId, out int percent, out int timesWatched);
    void DeleteMovie(string strFilenameAndPath);
    
    // User Rating
    void SetUserRatingForMovie(int lMovieId, int lUserRating);
    int GetUserRatingForMovie(int lMovieId);
    int GetUserRatingForMovie(string lIMDBNumber);

    // Movie files and movies
    int AddMovie(string strFilenameAndPath, bool bHassubtitles);
    void GetMovies(ref ArrayList movies);
    int GetMovieId(string strFilenameAndPath);
    int GetTitleBDId(int iFileId, out byte[] resumeData);
    bool HasSubtitle(string strFilenameAndPath);
    void SetThumbURL(int lMovieId, string thumbURL);
    
    // Fanart
    void SetFanartURL(int lMovieId, string fanartURL);
    
    // Movies by filters
    void GetYears(ArrayList years);
    void GetMoviesByGenre(string strGenre1, ref ArrayList movies);
    void GetRandomMoviesByGenre(string strGenre1, ref ArrayList movies, int limit);
    void GetRandomMoviesByGenre(string strGenre1, ref ArrayList movies, int limit, string whereClause);
    string GetMovieTitlesByGenre(string strGenre);
    string GetMovieTitlesByGenre(string strGenre, string whereClause);
    void GetMoviesByCollection(string strCollection1, ref ArrayList movies);
    void GetRandomMoviesByCollection(string strCollection1, ref ArrayList movies, int limit);
    void GetRandomMoviesByCollection(string strCollection1, ref ArrayList movies, int limit, string whereClause);
    string GetMovieTitlesByCollection(string strCollection);
    string GetMovieTitlesByCollection(string strCollection, string whereClause);
    string GetMovieTitlesByCollection(int idCollection, string whereClause);
    void GetMoviesByUserGroup(string strUserGroup1, ref ArrayList movies);
    void GetRandomMoviesByUserGroup(string strUserGroup, ref ArrayList movies, int limit);
    void GetRandomMoviesByUserGroup(string strUserGroup, ref ArrayList movies, int limit, string whereClause);
    string GetMovieTitlesByUserGroup(int idGroup);
    string GetMovieTitlesByUserGroup(int idGroup, string whereClause);
    void GetMoviesByActor(string strActor1, ref ArrayList movies);
    void GetRandomMoviesByActor(string strActor1, ref ArrayList movies, int limit);
    void GetRandomMoviesByActor(string strActor1, ref ArrayList movies, int limit, string whereClause);
    void GetRandomMoviesByActorDirector(string strActor1, ref ArrayList movies, int limit, string whereClause);
    string GetMovieTitlesByActor(int actorId);
    string GetMovieTitlesByActor(int actorId, string whereClause);
    string GetMovieTitlesByDirector(int directorId);
    string GetMovieTitlesByDirector(int directorId, string whereClause);
    void GetMoviesByYear(string strYear, ref ArrayList movies);
    void GetRandomMoviesByYear(string strYear, ref ArrayList movies, int limit);
    void GetRandomMoviesByYear(string strYear, ref ArrayList movies, int limit, string whereClause);
    string GetMovieTitlesByYear(string strYear);
    string GetMovieTitlesByYear(string strYear, string whereClause);
    string GetFieldDataByIndex(string dbField, string dbValue, string whereClause);
    void GetMoviesByPath(string strPath1, ref ArrayList movies);
    void GetRandomMoviesByPath(string strPath1, ref ArrayList movies, int limit);
    void GetRandomMoviesByIndex(string strDBField, string strIndexValue, ref ArrayList movies, int limit, string whereClause);
    void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable, bool genreTable, bool usergroupTable);
    void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable, bool genreTable, bool usergroupTable, bool collectionTable);
    void GetIndexByFilter(string sql, bool filterNonWordChar, out ArrayList movieList);
    string GetMovieTitlesByIndex(string sql);

    // CD/DVD label
    void SetDVDLabel(int lMovieId, string strDVDLabel1);
    void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel);
    string GetDVDLabel(string strFile);
    
    // Blacklisted thumbs
    bool IsVideoThumbBlacklisted(string path);
    int VideoThumbBlacklist(string path, DateTime expiresOn);
    bool VideoThumbRemoveFromBlacklist(string path);
    void RemoveExpiredVideoThumbBlacklistEntries();
    void RemoveAllVideoThumbBlacklistEntries();

    // Search
    void SearchMoviesByView(string dbField, string dbValue, out ArrayList movies);
    void SearchActorsByView(string dbActor, out ArrayList movies, bool director);
    
    // Other
    SQLiteResultSet GetResults(string sql);
    void ExecuteSQL (string sql, out bool error, out string errorMessage);
    ArrayList ExecuteRuleSQL(string sql, string fieldName, out bool error, out string errorMessage);
    string DatabaseName { get; }
    void GetVideoFilesMediaInfo(string strFilenameAndPath, ref VideoFilesMediaInfo mediaInfo, bool refresh);
    bool HasMediaInfo(string fileName);
    bool CheckMovieImdbId(string id);
    bool CheckActorImdbId(string id);
    void ImportNfo(string nfoFile, bool skipExisting, bool refreshdbOnly);
    bool MakeNfo(int movieId);
    void ImportNfoUsingVideoFile(string videoFile, bool skipExisting, bool refreshdbOnly);
    void GetVideoFiles(string path, ref ArrayList availableFiles);
    void FlushTransactionsToDisk();
    void RevertFlushTransactionsToDisk();
    bool DbHealth { get; }
    string DefaultVideoViewFields {get; }
  }
}