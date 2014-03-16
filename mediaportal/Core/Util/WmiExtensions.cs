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

//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//
//************************************************************************************************

namespace MediaPortal.Util
{
  using System;
  using System.Management;


  public static class WmiExtensions
  {

    /// <summary>
    /// Fetch the first item from the search result collection.
    /// </summary>
    /// <param name="searcher"></param>
    /// <returns></returns>

    public static ManagementObject First(this ManagementObjectSearcher searcher)
    {
      ManagementObject result = null;
      foreach (ManagementObject item in searcher.Get())
      {
        result = item;
        break;
      }
      return result;
    }
  }
}
