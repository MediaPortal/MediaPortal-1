#region Copyright (C) 2005-2009 Team MediaPortal

/*
 *  Copyright (C) 2005-2009 Team MediaPortal
 *  http://www.team-mediaportal.com
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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UninstallFilelist
{
  public class FileLister
  {
    private DirectoryInfo _directory;
    private List<string> _ignoredDirectories;
    private List<string> _ignoredFiles;

    private StringBuilder strBuilder = new StringBuilder();


    public string FileList
    {
      get { return strBuilder.ToString(); }
    }

    public FileLister(string directory, string exclude)
    {
      _directory = new DirectoryInfo(directory);
      _ignoredDirectories = new List<string>();
      _ignoredFiles = new List<string>();

      foreach (string str in exclude.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
      {
        if (str.StartsWith(";")) continue;

        //add ignored dirs
        if (str.StartsWith(@".\"))
        {
          _ignoredDirectories.Add(str.Substring(2));
          continue;
        }

        //add ignored files
        _ignoredFiles.Add(str);
      }
    }

    public void UpdateAll()
    {
      // parse sub dirs
      SearchDirectory(_directory);

      // parse root dir
      AddFiles(_directory);

      strBuilder.AppendLine(String.Format(
                              "RMDir \"{0}\"",
                              GetMediaPortalPath(_directory.FullName)
                              ));
    }

    private void SearchDirectory(DirectoryInfo directory)
    {
      foreach (DirectoryInfo dir in directory.GetDirectories())
      {
        if (dir.Name != ".svn" && dir.Name != "obj" && dir.Name != "bin")
        {
          string tempString = GetMediaPortalPath(dir.FullName);
          if (_ignoredDirectories.Contains(tempString)) continue;

          SearchDirectory(dir);
          AddFiles(dir);

          strBuilder.AppendLine(String.Format("RMDir \"{0}\"", tempString));
        }
      }
    }

    private void AddFiles(DirectoryInfo directory)
    {
      foreach (FileInfo file in directory.GetFiles())
      {
        string tempString = GetMediaPortalPath(file.FullName);
        if (_ignoredFiles.Contains(tempString)) continue;

        //Delete /REBOOTOK "$MPdir.Base\AppStart.exe"
        //Delete /REBOOTOK "$MPdir.Base\AppStart.exe.config"
        strBuilder.AppendLine(String.Format("Delete \"{0}\"", tempString));
      }
    }

    private string GetMediaPortalPath(string path)
    {
      path = path.Replace(_directory.FullName, "$MPdir.Base");
      //Var MPdir.Base

      //Var MPdir.Config
      //Var MPdir.Plugins
      path = path.Replace(@"$MPdir.Base\plugins", "$MPdir.Plugins");
      //Var MPdir.Log
      //Var MPdir.CustomInputDevice
      //Var MPdir.CustomInputDefault
      path = path.Replace(@"$MPdir.Base\InputDeviceMappings\defaults", "$MPdir.CustomInputDefault");
      //Var MPdir.Skin
      path = path.Replace(@"$MPdir.Base\skin", "$MPdir.Skin");
      //Var MPdir.Language
      path = path.Replace(@"$MPdir.Base\language", "$MPdir.Language");
      //Var MPdir.Database
      //Var MPdir.Thumbs
      path = path.Replace(@"$MPdir.Base\thumbs", "$MPdir.Thumbs");
      //Var MPdir.Weather
      path = path.Replace(@"$MPdir.Base\weather", "$MPdir.Weather");
      //Var MPdir.Cache
      //Var MPdir.BurnerSupport

      return path;
    }
  }
}