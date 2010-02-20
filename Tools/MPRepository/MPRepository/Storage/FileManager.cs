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
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace MPRepository.Storage
{

  /// <summary>
  /// This class manages access to the actual files
  /// </summary>
  public static class FileManager
  {


    public static string GetSaveLocation(String filename)
    {
      // TODO: Generate directory structure from filename with hashing and random
      // TODO: Check that filename doesn't already exists there, if it does, rehash

      //string tmpDir = ConfigurationManager.AppSettings.Get("TempDirectory");
      string filesBaseDir = ConfigurationManager.AppSettings.Get("FilesBaseDir");
      uint fileDirectories = 1;
      UInt32.TryParse(ConfigurationManager.AppSettings.Get("NumOfDirectoriesForFiles"), out fileDirectories);

      ulong subdir = ((ulong) filename.GetHashCode()) % fileDirectories;

      string targetDir = filesBaseDir + System.IO.Path.DirectorySeparatorChar + subdir.ToString();
      if (!System.IO.Directory.Exists(targetDir))
      {
        System.IO.Directory.CreateDirectory(targetDir);
      }

      return targetDir + System.IO.Path.DirectorySeparatorChar + filename + "." + DateTime.Now.ToString("yyyyddmmHHmm");

    }


  }
}
