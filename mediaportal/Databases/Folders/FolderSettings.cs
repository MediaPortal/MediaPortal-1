#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.IO;
using System.Threading;
using Databases.Folders;

namespace MediaPortal.Database
{
  public class FolderSettings
  {
    private static IFolderSettings _database = DatabaseFactory.GetFolderDatabase();

    public static void ReOpen()
    {
      Dispose();
      _database = DatabaseFactory.GetFolderDatabase();
    }

    public static void Dispose()
    {
      if (_database != null)
      {
        _database.Dispose();
      }

      _database = null;
    }

    private static bool WaitForPath(string pathName)
    {
      // while waking up from hibernation it can take a while before a network drive is accessible.   
      int count = 0;

      if (pathName.Length == 0 || pathName == "root")
      {
        return true;
      }

      //we cant be sure if pathName is a file or a folder, so we look for both.      
      while ((!Directory.Exists(pathName) && !File.Exists(pathName)) && count < 10)
      {
        Thread.Sleep(250);
        count++;
      }

      return (count < 10);
    }

    public static void DeleteFolderSetting(string path, string Key)
    {
      //bool res = WaitForPath(path);
      _database.DeleteFolderSetting(path, Key);
    }

    public static void AddFolderSetting(string path, string Key, Type type, object Value)
    {
      //bool res = WaitForPath(path);
      _database.AddFolderSetting(path, Key, type, Value);
    }

    public static void GetFolderSetting(string path, string Key, Type type, out object Value)
    {
      //bool res = WaitForPath(path);
      _database.GetFolderSetting(path, Key, type, out Value);
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
  }
}