#region Copyright (C) 2005-2025 Team MediaPortal

// Copyright (C) 2005-2025 Team MediaPortal
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
using System.Collections.Generic;

using MediaPortal.Database;
using MediaPortal.GUI.Pictures;

namespace MediaPortal.Picture.Database
{
  public class PictureDatabase
  {
    public static IPictureDatabase _database = DatabaseFactory.GetPictureDatabase();

    private PictureDatabase() {}

    static PictureDatabase() {}

    public static void ReOpen()
    {
      Dispose();
      _database = DatabaseFactory.GetPictureDatabase();
    }

    public static void Dispose()
    {
      if (_database != null)
      {
        _database.Dispose();
      }
      //_database = null;
    }

    public static int AddPicture(string strPicture, int iRotation)
    {
      return _database.AddPicture(strPicture, iRotation);
    }

    public static int UpdatePicture(string strPicture, int iRotation)
    {
      return _database.UpdatePicture(strPicture, iRotation);
    }

    public static void DeletePicture(string strPicture)
    {
      _database.DeletePicture(strPicture);
    }

    public static string GetDateTaken(string strPicture)
    {
      return _database.GetDateTaken(strPicture);
    }

    public static DateTime GetDateTimeTaken(string strPicture)
    {
      return _database.GetDateTimeTaken(strPicture);
    }

    public static ExifMetadata.Metadata GetExifFromDB(string strPicture)
    {
      return _database.GetExifFromDB(strPicture);
    }

    public static ExifMetadata.Metadata GetExifFromFile(string strPicture)
    {
      return _database.GetExifFromFile(strPicture);
    }

    public static int GetRotation(string strPicture)
    {
      return _database.GetRotation(strPicture);
    }

    public static void SetRotation(string strPicture, int iRotation)
    {
      _database.SetRotation(strPicture, iRotation);
    }

    public static int EXIFOrientationToRotation(int orientation)
    {
      return _database.EXIFOrientationToRotation(orientation);
    }

    public static bool GetFavorite(string strPicture)
    {
      return _database.GetFavorite(strPicture);
    }

    public static void SetFavorite(string strPicture, bool Favorite)
    {
      _database.SetFavorite(strPicture, Favorite);
    }

    public static int GetRating(string strPicture)
    {
      return _database.GetRating(strPicture);
    }

    public static void SetRating(string strPicture, int Rating)
    {
      _database.SetRating(strPicture, Rating);
    }

    public static int GetViews(string strPicture)
    {
      return _database.GetViews(strPicture);
    }

    public static void SetViews(string strPicture, int Views)
    {
      _database.SetViews(strPicture, Views);
    }

    public static void AddViews(string strPicture)
    {
      _database.AddViews(strPicture);
    }

    public static List<string> ListKeywords()
    {
      return _database.ListKeywords();
    }

    public static List<string> ListPicsByKeyword(string Keyword)
    {
      return _database.ListPicsByKeyword(Keyword);
    }

    public static int CountPicsByKeyword(string Keyword)
    {
      return _database.CountPicsByKeyword(Keyword);
    }

    public static List<string> ListPicsByKeywordSearch(string Keyword)
    {
      return _database.ListPicsByKeywordSearch(Keyword);
    }

    public static int CountPicsByKeywordSearch(string Keyword)
    {
      return _database.CountPicsByKeywordSearch(Keyword);
    }

    public static List<string> ListPicsBySearch(string query)
    {
      return _database.ListPicsBySearch(query);
    }

    public static int CountPicsBySearch(string query)
    {
      return _database.CountPicsBySearch(query);
    }

    public static int ListYears(ref List<string> Years)
    {
      return _database.ListYears(ref Years);
    }

    public static int ListMonths(string Year, ref List<string> Months)
    {
      return _database.ListMonths(Year, ref Months);
    }

    public static int ListDays(string Month, string Year, ref List<string> Days)
    {
      return _database.ListDays(Month, Year, ref Days);
    }

    public static int ListPicsByDate(string Date, ref List<string> Pics)
    {
      return _database.ListPicsByDate(Date, ref Pics);
    }

    public static int CountPicsByDate(string Date)
    {
      return _database.CountPicsByDate(Date);
    }

    public static List<string> ListValueByMetadata(string Name)
    {
      return _database.ListValueByMetadata(Name);
    }

    public static int CountPicsByMetadata(string Name)
    {
      return _database.CountPicsByMetadata(Name);
    }

    public static List<string> ListPicsByMetadata(string Name, string Value)
    {
      return _database.ListPicsByMetadata(Name, Value);
    }

    public static int CountPicsByMetadataValue(string Name, string Value)
    {
      return _database.CountPicsByMetadataValue(Name, Value);
    }

    public static List<string> ListFavorites()
    {
      return _database.ListFavorites();
    }

    public static List<string> ListPicsByFavorites(string Value)
    {
      return _database.ListPicsByFavorites(Value);
    }

    public static int CountPicsByFavorites(string Value)
    {
      return _database.CountPicsByFavorites(Value);
    }

    public static List<PictureData> GetPicturesByFilter(string aSQL, string aFilter)
    {
      return _database.GetPicturesByFilter(aSQL, aFilter);
    }

    public static List<PictureData> GetPicturesByFilter(string aSQL, string aFilter, bool fullInfo)
    {
      return _database.GetPicturesByFilter(aSQL, aFilter, fullInfo);
    }

    public static int GetCountByFilter(string aSQL, string aFilter)
    {
      return _database.GetCountByFilter(aSQL, aFilter);
    }

    public static int Count()
    {
      return _database.Count();
    }

    public static bool DbHealth
    {
      get
      {
        return _database.DbHealth;
      }
    }

    public static string DatabaseName
    {
      get
      {
        if (_database != null)
        {
          return _database.DatabaseName;
        }
        return string.Empty;
      }
    }

    public static bool FilterPrivate
    {
      get
      {
        return _database.FilterPrivate;
      }
      set
      {
        _database.FilterPrivate = value;
      }
    }

    public static void Optimize()
    {
      _database.Optimize();
    }
  }
}