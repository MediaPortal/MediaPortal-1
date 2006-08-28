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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using Core.Util;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for MyMameImporter.
  /// </summary>
  public class MyMameImporter
  {
    AppItem curApp = null;
    SQLiteClient sqlDB = null;

    // event: read new file
    public delegate void MyEventHandler(string strLine, int curPos, int maxPos);

    public event MyEventHandler OnReadNewFile = null;
    public event MyEventHandler OnSendMessage = null;
    ConditionChecker Checker = new ConditionChecker();
    string mameDir;
    string catverIniFile;
    string historyDatFile;
    StringCollection listFull = new StringCollection();
    StringCollection listClones = new StringCollection();
    StringCollection catverIni = new StringCollection();
    StringCollection historyDat = new StringCollection();
    StringCollection cacheRomnames = new StringCollection();
    StringCollection cacheCloneRomnames = new StringCollection();
    StringCollection cacheHistoryRomnames = new StringCollection();
    string[] mameRoms;

    public MyMameImporter(AppItem objApp, SQLiteClient objDB)
    {

      curApp = objApp;
      sqlDB = objDB;
    }

    void ReadFileFromStream(string filename, StringCollection coll)
    {
      string line;
      coll.Clear();
      StreamReader sr = File.OpenText(filename);
      while (true)
      {
        line = sr.ReadLine();
//        Log.Info(line);
        if (line == null)
        {
          break;
        }
        else
        {
          coll.Add(line);
        }
      }
      sr.Close();
    }


    void ReadListFull()
    {
      string line;
      string rom;
      int romendpos;
      listFull.Clear();
      cacheRomnames.Clear();
      SendText("generating mame list (full)");

      Process myProcess = new Process();
      ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(curApp.Filename);
      myProcessStartInfo.Arguments = "-listfull";
      myProcessStartInfo.UseShellExecute = false;
      myProcessStartInfo.RedirectStandardOutput = true;
      myProcessStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
      myProcessStartInfo.CreateNoWindow = true;
      myProcess.StartInfo = myProcessStartInfo;
      myProcess.Start();

      StreamReader sr = myProcess.StandardOutput;
      while (true)
      {
        line = sr.ReadLine();
//        Log.Info(line);
        if (line == null)
        {
          break;
        }
        else
        {
          listFull.Add(line);
          romendpos = line.IndexOf(" ");
          rom = line.Substring(0, romendpos);
          cacheRomnames.Add(rom);
        }
      }

      myProcess.Close();
    }

    void ReadListClones()
    {
      string line;
      string rom;
      int romendpos;
      listClones.Clear();
      cacheCloneRomnames.Clear();

      if (((appItemMameDirect)curApp).ImportOriginalsOnly)
      {
        SendText("generating mame list (clones)");

        Process myProcess = new Process();
        ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(curApp.Filename);
        myProcessStartInfo.Arguments = "-listclones";
        myProcessStartInfo.UseShellExecute = false;
        myProcessStartInfo.RedirectStandardOutput = true;
        myProcessStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
        myProcessStartInfo.CreateNoWindow = true;
        myProcess.StartInfo = myProcessStartInfo;
        myProcess.Start();

        StreamReader sr = myProcess.StandardOutput;
        while (true)
        {
          line = sr.ReadLine();
          if (line == null)
          {
            break;
          }
          else
          {
            listClones.Add(line);
            romendpos = line.IndexOf(" ");
            rom = line.Substring(0, romendpos);
            cacheCloneRomnames.Add(rom);
          }
        }

        myProcess.Close();
      }
    }


    void ReadHistoryDat()
    {
      string line;
      historyDat.Clear();
      cacheHistoryRomnames.Clear();
      StreamReader sr = File.OpenText(historyDatFile);
      ArrayList roms;
      int linenum = -1;
      while (true)
      {
        line = sr.ReadLine();
        if (line == null)
        {
          break;
        }
        else
        {
          if (line != "")
          {
            historyDat.Add(line);
          }
          if (line.StartsWith("$info="))
          {
            roms = new ArrayList(line.Substring(6).Split(','));
            foreach (string curRom in roms)
            {
              if (curRom != "")
              {
                linenum = historyDat.Count - 1;
                cacheHistoryRomnames.Add(curRom + "#" + linenum.ToString());
              }
            }
          }
        }
      }
    }


    bool CheckPrerequisites()
    {
      Checker.Clear();
      Checker.DoCheck(System.IO.Directory.Exists(curApp.FileDirectory), "rom-directory doesn't exist!");
      if (Checker.DoCheck(System.IO.File.Exists(curApp.Filename), "mame-application not found!"))
      {
        mameDir = Path.GetDirectoryName(curApp.Filename);
        catverIniFile = mameDir + "\\catver.ini";
        historyDatFile = mameDir + "\\history.dat";
      }

      if (Checker.IsOk)
      {
        ReadListFull();
        ReadListClones();
        if (System.IO.File.Exists(catverIniFile))
        {
          SendText("reading catver.ini");
          ReadFileFromStream(catverIniFile, catverIni);
        }
        if (System.IO.File.Exists(historyDatFile))
        {
          SendText("reading history.dat");
          ReadHistoryDat();
        }
        mameRoms = System.IO.Directory.GetFiles(curApp.FileDirectory, "*.zip");
      }
      return Checker.IsOk;
    }

    void SendText(string msg)
    {
      if (OnSendMessage != null)
      {
        OnSendMessage(msg, -1, -1);
      }
    }

    public void Start()
    {
      if (!CheckPrerequisites())
      {
        OnSendMessage(Checker.Problems, -1, -1);
        Log.Info("MameImporter: import failed! Details: {0}", Checker.Problems);
        return;
      }
      int i = 0;
      foreach (string fileName in mameRoms)
      {
        FillFileItem(fileName, i);
        i++;
      }
    }

    int GetLinePos(StringCollection coll, string startOfLine, int startPos)
    {
      int res = -1;
      for (int i = startPos; i < coll.Count; i++)
      {
        if (coll[i].StartsWith(startOfLine))
        {
          res = i;
          break;
        }
      }
      return res;
    }

    void ProcessHistory(FileItem curFile, int index)
    {
      string res = "";
      string sep = "";
      string line;
      string firstLine = "";
      bool skipping = true;
      while (true)
      {
        line = this.historyDat[index];
        if (line.StartsWith("$end"))
        {
          break;
        }
        if (!skipping)
        {
          if ((res != "") || (line != ""))
          {
            res = res + sep + line;
            sep = "\r\n";
            if (firstLine == "")
            {
              firstLine = line;
            }
          }
        }
        if (line.StartsWith("$bio"))
        {
          skipping = false;
        }
        index++;
        if (index >= historyDat.Count)
        {
          break;
        }
      }
      curFile.Overview = res;
      // grab year and manufacturer out of the first line of the history.dat
      if (firstLine != "")
      {
        int copyrightPos = firstLine.IndexOf("(c)");
        if (copyrightPos > 1)
        {
          //Tetris (c) 1989 Sega.
          //result: 1989 Sega.
          string yearManu = firstLine.Substring(copyrightPos + 3).Trim();
          yearManu = yearManu.TrimEnd('.');
          if (yearManu.IndexOf("/") == 2)
          {
            // trim away month info like "04/1989 Sega
            yearManu = yearManu.Substring(3);
          }
          if (yearManu.Length >= 5)
          {
            string year = yearManu.Substring(0, 4);
            string manu = yearManu.Substring(5).Trim();
            curFile.Year = ProgramUtils.StrToIntDef(year, -1);
            curFile.Manufacturer = manu;
          }
        }
      }

    }

    void FillFileItem(string fullRomname, int count)
    {
      string curRomname = Path.GetFileNameWithoutExtension(fullRomname).ToLower();
      string fullEntry = "";
      string genreEntry = "";
      string versionEntry = "";
      bool onlyOriginals = ((appItemMameDirect)curApp).ImportOriginalsOnly;
      int historyIndex = -1;

      int linePos = cacheRomnames.IndexOf(curRomname);
      int cloneLinePos =  cacheCloneRomnames.IndexOf(curRomname);

      // is the rom a clone and if yes, are clones allowed to be imported?
      if ((!onlyOriginals) || (onlyOriginals && (cloneLinePos == -1)))
      {
        FileItem curFile = new FileItem(sqlDB);
        curFile.AppID = curApp.AppID;
        curFile.Filename = fullRomname;
        curFile.Imagefile = GetImageFile(curRomname);
        if ((curFile.Imagefile == "") && (curApp.ImportValidImagesOnly))
        {
          return;
        }
        if (linePos > 0)
        {
          fullEntry = listFull[linePos]; //  mspacman  "Ms. Pac-Man"
          linePos = GetLinePos(catverIni, curRomname + "=", 0);
          if (linePos >= 0)
          {
            genreEntry = catverIni[linePos]; // mspacman=Maze
            linePos = GetLinePos(catverIni, curRomname + "=", linePos + 1);
            if (linePos >= 0)
            {
              versionEntry = catverIni[linePos]; //mspacman=.37b16
            }
          }
          historyIndex = GetHistoryIndex(curRomname);
          if (historyIndex != -1)
          {
            // process History-Dat and set OVERVIEW / YEAR and MANUFACTURER fields
            ProcessHistory(curFile, historyIndex);
          }

          ProcessFullEntry(curFile, fullEntry);
          ProcessGenreEntry(curFile, genreEntry);
          ProcessVersionEntry(curFile, versionEntry);
          curFile.System_ = "Arcade";
          curFile.Rating = 5;
          curFile.Write();
          if (OnReadNewFile != null)
          {
            OnReadNewFile(curFile.Title, count, mameRoms.Length);
          }

        }
      }
    }

    int GetHistoryIndex(string rom)
    {
      // locate one rom in the lookup table
      // format of the entries: <romname>#<linenumber>
      int res = -1;
      int historyPos = GetLinePos(cacheHistoryRomnames, rom + "#", 0);
      if (historyPos != -1)
      {
        string historyEntry = cacheHistoryRomnames[historyPos];
        ArrayList temp = new ArrayList(historyEntry.Split('#'));
        if (temp.Count > 1)
        {
          res = ProgramUtils.StrToIntDef(temp[1].ToString(),  -1);
        }
      }
      return res;
    }

    void ProcessFullEntry(FileItem curFile, string fullEntry)
    {
      //  mspacman  "Ms. Pac-Man"
      ArrayList temp = new ArrayList(fullEntry.Split('"'));
      if (temp.Count == 3)
      {
        curFile.Title = temp[1].ToString();
      }
      else
      {
        Log.Info("myPrograms: mameImport(ProcessFullEntry): unexpected string '{0}'", fullEntry);
      }
    }

    void ProcessGenreEntry(FileItem curFile, string genreEntry)
    {
      if (genreEntry != "")
      {
        // mspacman=Maze
        ArrayList temp = new ArrayList(genreEntry.Split('='));
        if (temp.Count == 2)
        {
          string allGenres = temp[1].ToString();
          ArrayList temp2 = new ArrayList(allGenres.Split('/'));
          if (temp2.Count > 0)
          {
            curFile.Genre = temp2[0].ToString().Trim();
          }
          if (temp2.Count > 1)
          {
            curFile.Genre2 = temp2[1].ToString().Trim();
          }
          if (temp2.Count > 2)
          {
            curFile.Genre3 = temp2[2].ToString().Trim();
          }
          if (temp2.Count > 3)
          {
            curFile.Genre4 = temp2[3].ToString().Trim();
          }
          if (temp2.Count > 4)
          {
            curFile.Genre5 = temp2[4].ToString().Trim();
          }
        }
        else
        {
          Log.Info("myPrograms: mameImport(ProcessGenreEntry): unexpected string '{0}'", genreEntry);
        }
      }
      
    }

    void ProcessVersionEntry(FileItem curFile, string versionEntry)
    {
      if (versionEntry != "")
      {
        //mspacman=.37b16
        ArrayList temp = new ArrayList(versionEntry.Split('='));
        if (temp.Count == 2)
        {
          curFile.CategoryData = String.Format("version={0}", temp[1].ToString());
        }
        else
        {
          Log.Info("myPrograms: mameImport(ProcessVersionEntry): unexpected string '{0}'", versionEntry);
        }
      }
      
    }

    string GetImageFile(string curRomname)
    {
      string res = "";
      string imgFolder = "";
      int i = 0;
      while ((res == "") && (i < curApp.imageDirs.Length))
      {
        imgFolder = curApp.imageDirs[i];
        res = GetImageFileOfFolder(curRomname, imgFolder);
        i++;
      }
      return res;
    }

    string GetImageFileOfFolder(string curRomname, string imgFolder)
    {
      string res = "";
      if (Directory.Exists(imgFolder))
      {
        string filenameNoExtension = imgFolder + "\\" + curRomname;
        if (File.Exists(Path.ChangeExtension(filenameNoExtension, ".png")))
        {
          res = Path.ChangeExtension(filenameNoExtension, ".png");
        }
        else if (File.Exists(Path.ChangeExtension(filenameNoExtension, ".jpg")))
        {
          res = Path.ChangeExtension(filenameNoExtension, ".jpg");
        }
        else if (File.Exists(Path.ChangeExtension(filenameNoExtension, ".gif")))
        {
          res = Path.ChangeExtension(filenameNoExtension, ".gif");
        }
      }
      return res;
    }

  }
}