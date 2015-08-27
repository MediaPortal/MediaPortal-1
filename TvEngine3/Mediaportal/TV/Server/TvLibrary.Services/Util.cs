#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary
{
  /// <summary>
  /// 
  /// </summary>
  public class Utils
  {
    [DllImport("kernel32.dll")]
    private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out UInt64 lpFreeBytesAvailable,
                                                  out UInt64 lpTotalNumberOfBytes, out UInt64 lpTotalNumberOfFreeBytes);

    [DllImport("kernel32.dll")]
    public static extern long GetDriveType(string driveLetter);

    // singleton. Dont allow any instance of this class
    private Utils() {}

    public static int getDriveType(string drive)
    {
      if (drive == null) return 2;
      if ((GetDriveType(drive) & 5) == 5) return 5; //cd
      if ((GetDriveType(drive) & 3) == 3) return 3; //fixed
      if ((GetDriveType(drive) & 2) == 2) return 2; //removable
      if ((GetDriveType(drive) & 4) == 4) return 4; //remote disk
      if ((GetDriveType(drive) & 6) == 6) return 6; //ram disk
      return 0;
    }

    public static string MakeFileName(string strText)
    {
      if (string.IsNullOrEmpty(strText))
      {
        return string.Empty;
      }
      foreach (char c in Path.GetInvalidFileNameChars())
      {
        strText = strText.Replace(c, '_');
      }
      return strText;
    }

    public static string MakeDirectoryPath(string strText)
    {
      if (string.IsNullOrEmpty(strText))
      {
        return string.Empty;
      }
      foreach (char c in Path.GetInvalidPathChars())
      {
        strText = strText.Replace(c, '_');
      }
      return strText;
    }

    public static bool FileDelete(string strFile)
    {
      if (strFile == null) return true;
      if (strFile.Length == 0) return true;
      try
      {
        if (!System.IO.File.Exists(strFile)) return true;
        System.IO.File.Delete(strFile);
        return true;
      }
      catch (Exception) {}
      return false;
    }

    /// <summary>
    /// Deletes a file. After this the containing folder is deleted also, if it's empty
    /// </summary>
    /// <param name="strFile">filename</param>
    /// <returns>true if successful</returns>
    public static bool DeleteFileAndEmptyDirectory(string strFile)
    {
      if (String.IsNullOrEmpty(strFile)) return true;
      try
      {
        if (System.IO.File.Exists(strFile))
        {
          System.IO.File.Delete(strFile);
        }
        String FolderName = System.IO.Path.GetDirectoryName(strFile);
        DirectoryInfo ContainingFolder = new DirectoryInfo(FolderName);
        if (ContainingFolder != null)
        {
          DirectoryInfo[] subfolders = ContainingFolder.GetDirectories();
          FileInfo[] files = ContainingFolder.GetFiles();
          if (files.Length == 0 && subfolders.Length == 0)
          {
            System.IO.Directory.Delete(FolderName);
          }
        }
        return true;
      }
      catch (Exception) {}
      return false;
    }

    public static string RemoveTrailingSlash(string strLine)
    {
      if (strLine == null) return String.Empty;
      if (strLine.Length == 0) return String.Empty;
      string strPath = strLine;
      while (strPath.Length > 0)
      {
        if (strPath[strPath.Length - 1] == '\\' || strPath[strPath.Length - 1] == '/')
        {
          strPath = strPath.Substring(0, strPath.Length - 1);
        }
        else break;
      }
      return strPath;
    }

    public static string ReplaceTag(string line, string tag, string value, string defaultValue = "")
    {
      if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(tag))
        return defaultValue;

      // check for [*%tag%*]
      // +
      // escape % for regex parsing
      string strRegex = String.Format(@"\[[^%]*{0}[^\]]*[\]]", tag.Replace("%", "\\%"));

      Regex r;
      try
      {
        r = new Regex(strRegex);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "ReplaceTag: Regex generated the following error");
        return line;
      }

      Match match = r.Match(line);
      if (match.Success)
      {
        // RemoveUser [ xxx ] completly
        line = line.Remove(match.Index, match.Length);
        if (!String.IsNullOrEmpty(value))
        {
          // AddSubChannelOrUser again xxx if value != null
          string m = match.Value.Substring(1, match.Value.Length - 2);
          line = line.Insert(match.Index, m);
        }
      }
      // finally replace tag with value
      return line.Replace(tag, value);
    }

    public static ulong GetFreeDiskSpace(string drive)
    {
      if (drive.StartsWith(@"\"))
      {
        return GetFreeShareSpace(drive);
      }
      ulong freeBytesAvailable = 0;
      ulong totalNumberOfBytes = 0;
      ulong totalNumberOfFreeBytes = 0;

      GetDiskFreeSpaceEx(
        drive[0] + @":\",
        out freeBytesAvailable,
        out totalNumberOfBytes,
        out totalNumberOfFreeBytes);
      return freeBytesAvailable;
    }

    public static ulong GetFreeShareSpace(string UNCPath)
    {
      ulong freeBytesAvailable = 0;
      ulong totalNumberOfBytes = 0;
      ulong totalNumberOfFreeBytes = 0;

      GetDiskFreeSpaceEx(
        System.IO.Path.GetPathRoot(UNCPath),
        out freeBytesAvailable,
        out totalNumberOfBytes,
        out totalNumberOfFreeBytes);
      return freeBytesAvailable;
    }
  }
}