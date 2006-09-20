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
using SQLite.NET;
using WindowPlugins.GUIPrograms;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for ProgramSettings.
  /// </summary>
  public class ProgramSettings
  {
    static public SQLiteClient sqlDB = null;
    static public ProgramViewHandler viewHandler = null;

    // singleton. Dont allow any instance of this class
    private ProgramSettings(){}

    static ProgramSettings(){}


    static public string ReadSetting(string Key)
    {
      SQLiteResultSet results;
      string res = null;
      string SQL = "SELECT value FROM setting WHERE key ='" + Key + "'";
      results = sqlDB.Execute(SQL);
      if (results != null && results.Rows.Count > 0)
      {
        SQLiteResultSet.Row arr = results.Rows[0];
        res = arr.fields[0];
      }
      //Log.Write("dw read setting key:{0}\nvalue:{1}", Key, res);
      return res;
    }

    static int CountKey(string Key)
    {
      SQLiteResultSet results;
      int res = 0;
      results = sqlDB.Execute("SELECT COUNT(*) FROM setting WHERE key ='" + Key + "'");
      if (results != null && results.Rows.Count > 0)
      {
        SQLiteResultSet.Row arr = results.Rows[0];
        res = Int32.Parse(arr.fields[0]);
      }
      return res;
    }

    static public bool KeyExists(string Key)
    {
      return (CountKey(Key) > 0);
    }

    static public void WriteSetting(string Key, string Value)
    {
      if (KeyExists(Key))
      {
        sqlDB.Execute("update setting set value = '" + Value + "' where key = '" + Key + "'");
      }
      else
      {
        sqlDB.Execute("insert into setting (key, value) values ('" + Key + "', '" + Value + "')");
      }
    }

    static public void DeleteSetting(string Key)
    {
      sqlDB.Execute("delete from setting where key = '" + Key + "'");
    }



  }
}
