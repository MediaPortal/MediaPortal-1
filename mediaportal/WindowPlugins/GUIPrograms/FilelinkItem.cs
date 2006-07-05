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
using MediaPortal.Utils.Services;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for FilelinkItem.
  /// </summary>
  public class FilelinkItem: FileItem
  {
    int mTargetAppID;
    new protected ILog _log;

    public FilelinkItem(SQLiteClient initSqlDB): base(initSqlDB)
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

    public int TargetAppID
    {
      get
      {
        return mTargetAppID;
      }
      set
      {
        mTargetAppID = value;
      }
    }

    public override void Clear()
    {
      base.Clear();
      mTargetAppID =  - 1;
    }

    public override void Write()
    {
      if (Exists())
      {
        Update();
      }
      else
      {
        Insert();
      }
    }

    public override void Delete()
    {
      try
      {
        //sqlDB.Execute("begin");
        string strSQL2 = String.Format(String.Format("DELETE FROM filterItem WHERE appid = {0} AND grouperAppID = {1} AND fileID = {2}", this.TargetAppID,
          this.AppID, this.FileID));
        sqlDB.Execute(strSQL2);
        //sqlDB.Execute("commit");
      }
      catch (SQLiteException ex)
      {
        sqlDB.Execute("rollback");
        _log.Info("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }


    private bool Exists()
    {
      SQLiteResultSet results;
      int res = 0;
      results = sqlDB.Execute(String.Format("SELECT COUNT(*) FROM filterItem WHERE appid = {0} AND grouperAppID = {1} AND fileID = {2}", this.TargetAppID,
        this.AppID, this.FileID));
      if (results != null && results.Rows.Count > 0)
      {
        SQLiteResultSet.Row arr = results.Rows[0];
        res = Int32.Parse(arr.fields[0]);
      }
      return (res > 0);
    }

    private void Insert()
    {
      try
      {
        //sqlDB.Execute("begin");
        string strSQL2 = String.Format(String.Format("INSERT INTO filterItem (appid, grouperAppID, fileID, filename) VALUES ({0}, {1}, {2}, '{3}')",
          this.TargetAppID, this.AppID, this.FileID, ProgramUtils.Encode(Filename)));
        // _log.Info("hi from filelinkiteminsert: {0}", strSQL2);
        sqlDB.Execute(strSQL2);
        //sqlDB.Execute("commit");
      }
      catch (SQLiteException ex)
      {
        sqlDB.Execute("rollback");
        _log.Info("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    private void Update()
    {
      // nothing to update (yet)
      //...... as all FILTERITEM fields are primary key fields...
    }

  }
}
