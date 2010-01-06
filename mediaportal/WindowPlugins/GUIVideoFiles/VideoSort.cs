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
using System.Globalization;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for VideoSort.
  /// </summary>
  public class VideoSort : IComparer<GUIListItem>
  {
    public enum SortMethod
    {
      Name = 0,
      Date = 1,
      Size = 2,
      Year = 3,
      Rating = 4,
      Label = 5,
      Unwatched = 6
    }

    protected SortMethod currentSortMethod;
    protected bool sortAscending;

    public VideoSort(SortMethod sortMethod, bool ascending)
    {
      currentSortMethod = sortMethod;
      sortAscending = ascending;
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
      if (item1.IsFolder && item1.Label == "..")
      {
        return -1;
      }
      if (item2.IsFolder && item2.Label == "..")
      {
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      else if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }


      switch (currentSortMethod)
      {
        case SortMethod.Year:
          {
            if (sortAscending)
            {
              if (item1.Year > item2.Year)
              {
                return 1;
              }
              if (item1.Year < item2.Year)
              {
                return -1;
              }
            }
            else
            {
              if (item1.Year > item2.Year)
              {
                return -1;
              }
              if (item1.Year < item2.Year)
              {
                return 1;
              }
            }
            return 0;
          }
        case SortMethod.Rating:
          {
            if (sortAscending)
            {
              if (item1.Rating > item2.Rating)
              {
                return 1;
              }
              if (item1.Rating < item2.Rating)
              {
                return -1;
              }
            }
            else
            {
              if (item1.Rating > item2.Rating)
              {
                return -1;
              }
              if (item1.Rating < item2.Rating)
              {
                return 1;
              }
            }
            return 0;
          }

        case SortMethod.Name:

          if (sortAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }

        case SortMethod.Label:
          if (sortAscending)
          {
            return String.Compare(item1.DVDLabel, item2.DVDLabel, true);
          }
          else
          {
            return String.Compare(item2.DVDLabel, item1.DVDLabel, true);
          }
        case SortMethod.Size:
          if (item1.FileInfo == null || item2.FileInfo == null)
          {
            if (sortAscending)
            {
              return (int)(item1.Duration - item2.Duration);
            }
            else
            {
              return (int)(item2.Duration - item1.Duration);
            }
          }
          else
          {
            if (sortAscending)
            {
              return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
            }
            else
            {
              return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
            }
          }


        case SortMethod.Date:
          if (item1.FileInfo == null)
          {
            return -1;
          }
          if (item2.FileInfo == null)
          {
            return -1;
          }

          item1.Label2 = item1.FileInfo.ModificationTime.ToShortDateString() + " " +
                         item1.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 = item2.FileInfo.ModificationTime.ToShortDateString() + " " +
                         item2.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          if (sortAscending)
          {
            return DateTime.Compare(item1.FileInfo.ModificationTime, item2.FileInfo.ModificationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.ModificationTime, item1.FileInfo.ModificationTime);
          }
        case SortMethod.Unwatched:
          {
            int ret = 0;
            if (item1.IsPlayed && !item2.IsPlayed)
            {
              ret = 1;
              if (!sortAscending) ret = -1;
            }
            if (!item1.IsPlayed && item2.IsPlayed)
            {
              ret = -1;
              if (!sortAscending) ret = 1;
            }
            return ret;
          }
      }
      return 0;
    }
  }
}