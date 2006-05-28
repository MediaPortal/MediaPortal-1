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
using System.Data.OleDb;
using System.IO;
using MediaPortal.GUI.Library;
using Programs.Utils;
using ProgramsDatabase;
using SQLite.NET;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for MyGamebaseImporter.
  /// </summary>
  public class MyGamebaseImporter
  {
    private AppItem m_App = null;
    private SQLiteClient sqlDB = null;

    // event: read new file
    public delegate void GamebaseEventHandler(string strLine, int curPos, int maxPos);

    public event GamebaseEventHandler OnReadNewFile = null;


    public MyGamebaseImporter(AppItem objApp, SQLiteClient objDB)
    {
      m_App = objApp;
      sqlDB = objDB;
    }


    void DBImportGamebaseItem(OleDbDataReader myReader, string romFilename, string imgFilename, int curPos, int maxGames)
    {
      FileItem curFile = new FileItem(sqlDB);
      curFile.FileID =  - 1; // to force an INSERT statement when writing the item
      curFile.AppID = m_App.AppID;
      curFile.Title = myReader.GetString(1);
      curFile.Filename = romFilename;
      curFile.Imagefile = imgFilename;
      string strGenre1 = myReader.GetString(7);
      string strGenre2 = myReader.GetString(6);
      //      curFile.Genre = String.Format("{0} {1}", strGenre1, strGenre2);
      curFile.Genre = strGenre1;
      curFile.Genre2 = strGenre2;
      // todo: country
      curFile.Country = "";
      curFile.Manufacturer = myReader.GetString(8);
      curFile.Year = myReader.GetInt32(9);
      curFile.Rating = myReader.GetInt32(3) * 2;
      if (curFile.Rating > 10)
      {
        curFile.Rating = 10;
      }
      if (curFile.Rating < 1)
      {
        curFile.Rating = 5; // average / not rated....
      }
      curFile.Overview = myReader.GetString(5);
      curFile.System_ = m_App.Title;
      // not imported properties => set default values
      curFile.ManualFilename = "";
      curFile.LastTimeLaunched = DateTime.MinValue;
      curFile.LaunchCount = 0;
      curFile.Write();
      this.OnReadNewFile(curFile.Title, curPos, maxGames); // send event to whom it may concern....
      return ;
    }

    public void Start()
    {
      //      string strCon = "Provider=Microsoft.Jet.OLEDB.4.0 ;Data Source=C:\\media\\GameBase\\snes\\Snes.mdb";
      string strCon = String.Format("Provider=Microsoft.Jet.OLEDB.4.0 ;Data Source={0}", m_App.Source);
      OleDbConnection myCon = new OleDbConnection(strCon);
      //Make a Select Command for querying the gamebase-MDB-file
      string sqlStrCount = "SELECT count(*) FROM Games";
      string sqlStr = "SELECT Games.Filename, Games.Name, Games.Comment, Games.Rating, Games.Classic, Games.MemoText, Genres.Genre, PGenres.ParentGenre, Publishers.Publisher, Years.Year, Games.ScrnshotFilename "
        + "FROM Games, Genres, PGenres, Publishers, Years "
        + "WHERE Games.GE_Id = Genres.GE_Id "
        + "AND Genres.PG_Id = PGenres.PG_Id "
        + "AND Genres.PG_Id = PGenres.PG_Id "
        + "AND Publishers.PU_Id = Games.PU_Id "
        + "AND Years.YE_Id = Games.YE_Id "
        + "ORDER BY Games.Name ";

      string curRomname = "";
      string curFullRomname = "";
      string curTitleImage = "";
      bool bDoImport = false;
      OleDbCommand myCmd = new OleDbCommand(sqlStr, myCon);
      try
      {
        myCon.Open();
        int maxGames = CountGames(myCon, sqlStrCount);
        OleDbDataReader myReader = myCmd.ExecuteReader();
        try
        {
          int i = 0;
          while (myReader.Read())
          {
            i++;
            curRomname = myReader.GetString(0);
            curFullRomname = m_App.FileDirectory + "\\" + curRomname;

            if (m_App.ImageDirectory != "")
            {
              curTitleImage = m_App.imageDirs[0] + "\\"+ myReader.GetString(10);
            }
            else
            {
              curTitleImage = "";
            }

            if (File.Exists(curFullRomname))
            {
              // rom-name from gamebase exists in users filedirectory
              // => ready to import item
              bDoImport = true;

              if (m_App.ImportValidImagesOnly)
              {
                // skip item if no thumbnail image is found
                bDoImport = ((curTitleImage != null) && (curTitleImage != "") && (File.Exists(curTitleImage)));
              }

              if (bDoImport)
              {
                DBImportGamebaseItem(myReader, curFullRomname, curTitleImage, i, maxGames);
              }
              else
              {
                Log.Write("*skipped* gamebase game {0} image{1}", curRomname, curTitleImage);
              }

            }
            else
            {
              Log.Write("*missing* gamebase game {0}", curRomname);
            }
          }
        }
        finally
        {
          myReader.Close();
        }
      }
      catch (Exception er)
      {
        Log.Write("myProgams error in connecting to gamebase-mdb \n {0}", er.ToString());
      }
      finally
      {
        myCmd.Dispose();
        myCon.Close();
        myCon.Dispose();
      }
    }

    int CountGames(OleDbConnection myCon, string sqlStrCount)
    {
      int res = 0;
      OleDbCommand myCmd = new OleDbCommand(sqlStrCount, myCon);
      try
      {
        res = (int) myCmd.ExecuteScalar();
      }
      catch (Exception er)
      {
        Log.Write("myProgams error in connecting to gamebase-mdb \n {0}", er.ToString());
      }
      finally
      {
        myCmd.Dispose();
      }
      return res;
    }



  }
}