#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public static class ByteArray
  {
    public static bool AreEqual(byte[] bytes1, byte[] bytes2)
    {
      if ((bytes1 != null) || (bytes2 != null))
      {
        if ((bytes1 == null) || (bytes2 == null))
        {
          return false;
        }
        if (!bytes1.Equals(bytes2))
        {
          if (bytes1.Length != bytes2.Length)
          {
            return false;
          }
          for (int i = 0; i < bytes1.Length; i++)
          {
            if (bytes1[i] != bytes2[i])
            {
              return false;
            }
          }
        }
      }
      return true;
    }
  }
}