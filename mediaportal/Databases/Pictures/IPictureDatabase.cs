#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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

using MediaPortal.GUI.Pictures;

namespace MediaPortal.Picture.Database
{
  public interface IPictureDatabase
  {
    int AddPicture(string strPicture, int iRotation);
    int UpdatePicture(string strPicture, int iRotation);
    void DeletePicture(string strPicture);
    string GetDateTaken(string strPicture);
    DateTime GetDateTimeTaken(string strPicture);
    ExifMetadata.Metadata GetExifFromFile(string strPicture);
    ExifMetadata.Metadata GetExifFromDB(string strPicture);
    int GetRotation(string strPicture);
    void SetRotation(string strPicture, int iRotation);
    int EXIFOrientationToRotation(int orientation);
    void Dispose();
    int ListKeywords(ref List<string> Keywords);
    int ListPicsByKeyword(string Keyword, ref List<string> Pics);
    int CountPicsByKeyword(string Keyword);
    int ListPicsByKeywordSearch(string Keyword, ref List<string> Pics);
    int CountPicsByKeywordSearch(string Keyword);
    int ListPicsBySearch(string query, ref List<string> Pics);
    int CountPicsBySearch(string query);
    int ListYears(ref List<string> Years);
    int ListMonths(string Year, ref List<string> Months);
    int ListDays(string Month, string Year, ref List<string> Days);
    int ListPicsByDate(string Date, ref List<string> Pics);
    int CountPicsByDate(string Date);
    string DatabaseName { get; }
    bool DbHealth { get; }
    void Optimize();
  }
}