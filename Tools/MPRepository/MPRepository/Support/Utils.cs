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
using System.Linq;
using System.Text;

namespace MPRepository.Support
{

  /// <summary>
  /// Support utilities and functions for the repository
  /// </summary>
  public static class Utils
  {

    public static int VersionCompare(string ver1, string ver2)
    {
      string[] ver1parts = ver1.Split('.');
      string[] ver2parts = ver2.Split('.');
      int i = 0;      

      while (i < ver1parts.Length && i < ver2parts.Length)
      {
        int partComparison = String.Compare(ver1parts[i], ver2parts[i]);
        if (partComparison == 0)
        {
           i++;
        }
        else
        {
          return partComparison;
        }
      }
      if (ver1parts.Length < ver2parts.Length)
      {
        return -1;
      }
      else if (ver1parts.Length == ver2parts.Length)
      {
        return 0;
      }
      else
      {
        return 1;
      }
    }

  }
}
