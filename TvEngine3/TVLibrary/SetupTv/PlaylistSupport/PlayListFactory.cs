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

using System.IO;

namespace MediaPortal.Playlists
{
  /// <summary>
  /// 
  /// </summary>
  public class PlayListFactory
  {
    public static IPlayListIO CreateIO(string fileName)
    {
      string extension = GetLowerCaseExtension(fileName);
      if (extension == ".m3u" || extension == ".m3u8")
      {
        return new PlayListM3uIO();
      }
      if (extension == ".pls")
      {
        return new PlayListPLSIO();
      }
      if (extension == ".b4s")
      {
        return new PlayListB4sIO();
      }
      if (extension == ".wpl")
      {
        return new PlayListWPLIO();
      }
      return null;
    }

    private static string GetLowerCaseExtension(string fileName)
    {
      string extension = Path.GetExtension(fileName);
      return extension.ToLowerInvariant();
    }

    public static bool IsPlayList(string fileName)
    {
      string extension = GetLowerCaseExtension(fileName);
      if (extension == ".m3u" || extension == ".m3u8")
        return true;
      if (extension == ".pls")
        return true;
      if (extension == ".b4s")
        return true;
      if (extension == ".wpl")
        return true;
      return false;
    }
  }
}