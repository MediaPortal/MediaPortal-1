/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for FilelinkList.
  /// </summary>
  public class FilelinkList: ArrayList
  {
    public FilelinkList(SQLiteClient initSqlDB)
    {
      // constructor: save SQLiteDB object 
      sqlDB = initSqlDB;
    }

    static SQLiteClient sqlDB = null;


    static private FilelinkItem DBGetFilelinkItem(SQLiteResultSet results, int iRecord)
    {
      FilelinkItem newLink = new FilelinkItem(sqlDB);
      newLink.FileID = ProgramUtils.GetIntDef(results, iRecord, "fileid",  - 1);
      newLink.AppID = ProgramUtils.GetIntDef(results, iRecord, "grouperappid",  - 1);
      newLink.TargetAppID = ProgramUtils.GetIntDef(results, iRecord, "targetappid",  - 1);
      newLink.Title = ProgramUtils.Get(results, iRecord, "title");
      newLink.Filename = ProgramUtils.Get(results, iRecord, "filename");
      newLink.Filepath = ProgramUtils.Get(results, iRecord, "filepath");
      newLink.Imagefile = ProgramUtils.Get(results, iRecord, "imagefile");
      newLink.Genre = ProgramUtils.Get(results, iRecord, "genre");
      newLink.Genre2 = ProgramUtils.Get(results, iRecord, "genre2");
      newLink.Genre3 = ProgramUtils.Get(results, iRecord, "genre3");
      newLink.Genre4 = ProgramUtils.Get(results, iRecord, "genre4");
      newLink.Genre5 = ProgramUtils.Get(results, iRecord, "genre5");
      newLink.Country = ProgramUtils.Get(results, iRecord, "country");
      newLink.Manufacturer = ProgramUtils.Get(results, iRecord, "manufacturer");
      newLink.Year = ProgramUtils.GetIntDef(results, iRecord, "year",  - 1);
      newLink.Rating = ProgramUtils.GetIntDef(results, iRecord, "rating", 5);
      newLink.Overview = ProgramUtils.Get(results, iRecord, "overview");
      newLink.System_ = ProgramUtils.Get(results, iRecord, "system");
      newLink.ExtFileID = ProgramUtils.GetIntDef(results, iRecord, "external_id",  - 1);
      newLink.LastTimeLaunched = ProgramUtils.GetDateDef(results, iRecord, "lastTimeLaunched", DateTime.MinValue);
      newLink.LaunchCount = ProgramUtils.GetIntDef(results, iRecord, "launchcount", 0);
      newLink.IsFolder = ProgramUtils.GetBool(results, iRecord, "isfolder");
      return newLink;
    }


    public void Load(int nAppID, string strPath)
    {
      if (sqlDB == null)
        return ;
      try
      {
        Clear();
        if (null == sqlDB)
          return ;
        SQLiteResultSet results;
        string strSQL = "";
        // mFilepath = strPath;
        // app.
        // SPECIAL: the current application IS NOT the application with the launchinfo!
        strSQL = String.Format(
          "SELECT fi.appid AS targetappid, fi.grouperappid AS grouperappid, f.fileid AS fileid, title, uppertitle, f.filename as filename, filepath, imagefile, genre, genre2, genre3, genre4, genre5, country, manufacturer, YEAR, rating, overview, SYSTEM, import_flag, manualfilename, lasttimelaunched, launchcount, isfolder, external_id FROM tblFILE f, filteritem fi WHERE f.fileid = fi.fileid AND grouperappid = {0} ORDER BY filepath, uppertitle", nAppID);
        results = sqlDB.Execute(strSQL);
        if (results.Rows.Count == 0)
          return ;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          FilelinkItem curLink = DBGetFilelinkItem(results, iRow);
          Add(curLink);
        }
      }
      catch (SQLiteException ex)
      {
        Log.Write("Filedatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

  }
}
