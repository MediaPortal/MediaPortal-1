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

using System.Collections;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;
using WindowPlugins.GUIPrograms;
using MediaPortal.Utils.Services;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for Applist.
  /// </summary>
  public class Applist: ArrayList
  {
    public static SQLiteClient sqlDB = null;
    static ApplicationFactory appFactory = ApplicationFactory.AppFactory;

    static public event AppItem.FilelinkLaunchEventHandler OnLaunchFilelink = null;

    public Applist(SQLiteClient initSqlDB, AppItem.FilelinkLaunchEventHandler curHandler)
    {
      // constructor: save SQLiteDB object and load list from DB
      sqlDB = initSqlDB;
      OnLaunchFilelink += curHandler;
      LoadAll();
    }

    static private AppItem DBGetApp(SQLiteResultSet results, int recordIndex)
    {
      AppItem newApp = appFactory.GetAppItem(sqlDB, ProgramUtils.GetSourceType(results, recordIndex, "source_type"));
      newApp.OnLaunchFilelink += new AppItem.FilelinkLaunchEventHandler(LaunchFilelink);
      newApp.Enabled = ProgramUtils.GetBool(results, recordIndex, "enabled");
      newApp.AppID = ProgramUtils.GetIntDef(results, recordIndex, "appid",  - 1);
      newApp.FatherID = ProgramUtils.GetIntDef(results, recordIndex, "fatherID",  - 1);
      newApp.Title = ProgramUtils.Get(results, recordIndex, "title");
      newApp.ShortTitle = ProgramUtils.Get(results, recordIndex, "shorttitle");
      newApp.Filename = ProgramUtils.Get(results, recordIndex, "filename");
      newApp.Arguments = ProgramUtils.Get(results, recordIndex, "arguments");
      newApp.WindowStyle = ProgramUtils.GetProcessWindowStyle(results, recordIndex, "windowstyle");
      newApp.Startupdir = ProgramUtils.Get(results, recordIndex, "startupdir");
      newApp.UseShellExecute = ProgramUtils.GetBool(results, recordIndex, "useshellexecute");
      newApp.UseQuotes = ProgramUtils.GetBool(results, recordIndex, "usequotes");
      newApp.SourceType = ProgramUtils.GetSourceType(results, recordIndex, "source_type");
      newApp.Source = ProgramUtils.Get(results, recordIndex, "source");
      newApp.Imagefile = ProgramUtils.Get(results, recordIndex, "imagefile");
      newApp.FileDirectory = ProgramUtils.Get(results, recordIndex, "filedirectory");
      newApp.ImageDirectory = ProgramUtils.Get(results, recordIndex, "imagedirectory");
      newApp.ValidExtensions = ProgramUtils.Get(results, recordIndex, "validextensions");
      newApp.ImportValidImagesOnly = ProgramUtils.GetBool(results, recordIndex, "importvalidimagesonly");
      newApp.Position = ProgramUtils.GetIntDef(results, recordIndex, "iposition", 0);
      newApp.EnableGUIRefresh = ProgramUtils.GetBool(results, recordIndex, "enableGUIRefresh");
      newApp.ContentID = ProgramUtils.GetIntDef(results, recordIndex, "contentID", 100);
      newApp.SystemDefault = ProgramUtils.Get(results, recordIndex, "systemdefault");
      newApp.WaitForExit = ProgramUtils.GetBool(results, recordIndex, "waitforexit");
      newApp.Pincode = ProgramUtils.GetIntDef(results, recordIndex, "pincode",  - 1);
      newApp.PreLaunch = ProgramUtils.Get(results, recordIndex, "preLaunch");
      newApp.PostLaunch = ProgramUtils.Get(results, recordIndex, "postLaunch");
      return newApp;
    }

    public ArrayList appsOfFatherID(int FatherID)
    {
      ArrayList res = new ArrayList();
      foreach (AppItem curApp in this)
      {
        if (curApp.FatherID == FatherID)
        {
          res.Add(curApp);
        }
      }
      return res;
    }

    public ArrayList appsOfFather(AppItem father)
    {
      if (father == null)
      {
        return appsOfFatherID( - 1); // return children of root node!
      }
      else
      {
        return appsOfFatherID(father.AppID);
      }
    }


    public AppItem GetAppByID(int targetAppID)
    {
      foreach (AppItem curApp in this)
      {
        if (curApp.AppID == targetAppID)
        {
          return curApp;
        }
      }
      return null;
    }

    public AppItem CloneAppItem(AppItem sourceApp)
    {
      AppItem newApp = appFactory.GetAppItem(sqlDB, sourceApp.SourceType);
      newApp.Assign(sourceApp);
      newApp.AppID =  - 1; // to force a sql INSERT when written
      Add(newApp);
      return newApp;
    }


    static void LaunchFilelink(FilelinkItem curLink, bool mpGuiMode)
    {
      OnLaunchFilelink(curLink, mpGuiMode);
    }


    public int GetMaxPosition(int fatherID)
    {
      int res = 0;
      foreach (AppItem curApp in this)
      {
        if ((curApp.FatherID == fatherID) && (curApp.Position > res))
        {
          res = curApp.Position;
        }
      }
      return res;

    }

    public void LoadAll()
    {
      if (sqlDB == null)
        return ;
      try
      {
        Clear();
        if (null == sqlDB)
          return ;
        SQLiteResultSet results;
        results = sqlDB.Execute("select * from application order by iposition");
        if (results.Rows.Count == 0)
          return ;
        for (int row = 0; row < results.Rows.Count; row++)
        {
          AppItem curApp = DBGetApp(results, row);
          Add(curApp);
        }
      }
      catch (SQLiteException ex)
      {
        ServiceProvider services = GlobalServiceProvider.Instance;
        ILog log = services.Get<ILog>();
        log.Info("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

  }

}
