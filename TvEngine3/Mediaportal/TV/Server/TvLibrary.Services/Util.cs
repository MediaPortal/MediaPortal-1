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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary
{
  public class Utils
  {
    [DllImport("kernel32.dll")]
    private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out UInt64 lpFreeBytesAvailable,
                                                  out UInt64 lpTotalNumberOfBytes, out UInt64 lpTotalNumberOfFreeBytes);

    // singleton. Dont allow any instance of this class
    private Utils() {}

    public static bool GetFreeDiskSpace(string disk, out ulong bytesTotal, out ulong bytesFreeAndAvailable)
    {
      bytesTotal = 0;
      bytesFreeAndAvailable = 0;
      ulong bytesFreeTotal = 0;
      try
      {
        bool result = GetDiskFreeSpaceEx(
          Path.GetPathRoot(disk),
          out bytesFreeAndAvailable,
          out bytesTotal,
          out bytesFreeTotal);
        if (result)
        {
          return true;
        }
        Log.Warn("utils: failed to determine free disk space, error code = {0}, disk = {1}", Marshal.GetLastWin32Error(), disk);
        bytesTotal = 0;
        bytesFreeAndAvailable = 0;
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "utils: failed to determine free disk space, disk = {0}", disk);
      }
      return false;
    }
  }
}