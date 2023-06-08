#region Copyright (C) 2005-2023 Team MediaPortal

// Copyright (C) 2005-2023 Team MediaPortal
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

using System.Collections.Generic;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Home
{
  /// <summary>
  /// Summary description for PluginSort.
  /// </summary>
  public class PluginSort : IComparer<GUIListItem>
  {
    protected bool SortAscending;

    public PluginSort(bool ascending)
    {
      SortAscending = ascending;
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
      {
        return 0;
      }
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }

      if (SortAscending)
      {
        if (item1.ItemId > item2.ItemId)
        {
          return 1;
        }
        if (item1.ItemId < item2.ItemId)
        {
          return -1;
        }
      }
      else
      {
        if (item1.ItemId > item2.ItemId)
        {
          return -1;
        }
        if (item1.ItemId < item2.ItemId)
        {
          return 1;
        }
      }
      return 0;
    }
  }
}