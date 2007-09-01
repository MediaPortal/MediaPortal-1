#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using Databases.Folders;
using Databases.Folders.SqlServer;
namespace MediaPortal.Database
{
  public class FolderSettings
  {
    static IFolderSettings _database = DatabaseFactory.GetFolderDatabase();

    static public void Dispose()
    {
      _database.Dispose();
      _database = null;
    }

    static private bool WaitForPath(string folderName)
    {
      // while waking up from hibernation it can take a while before a network drive is accessible.
      // lets wait 10 sec      
      int count = 0;
      bool validDir = false;
      try
      {
        string dir = System.IO.Path.GetDirectoryName(folderName);
        
        validDir = dir.Length > 0;
      }
      catch (Exception ex)
      {
        validDir = false;
      }

      if (validDir)
      {
        while (!Directory.Exists(folderName) && count < 100)
        {
          System.Threading.Thread.Sleep(100);
          count++;
        }
      }
      else
      {
        return true;
      }

      return (validDir && count < 100);
    }

    static public void DeleteFolderSetting(string path, string Key)
    {
      bool res = WaitForPath(path);
      _database.DeleteFolderSetting(path, Key);
    }
    static public void AddFolderSetting(string path, string Key, Type type, object Value)
    {
      bool res = WaitForPath(path);
      _database.AddFolderSetting( path,  Key,  type,  Value);
    }
    static public void GetFolderSetting(string path, string Key, Type type, out object Value)
    {
      bool res = WaitForPath(path);
      _database.GetFolderSetting(path, Key, type, out Value);
    }
  }
}
