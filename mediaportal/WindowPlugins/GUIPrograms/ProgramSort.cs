#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using MediaPortal.Util;

using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for ProgramSort.
  /// </summary>
  public class ProgramSort : IComparer<GUIListItem>
  {
    SortMethod currentSortMethod;
    bool sortAscending = true;

    public ProgramSort(SortMethod method, bool ascending)
    {
      currentSortMethod = method;
      sortAscending = ascending;
    }

    public enum SortMethod
    {
      Name = 0,
      Title = 1,
      Filename = 2,
      Rating = 3,
      LaunchCount = 4,
      LastTimeLaunched = 5
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2) return 0;
      if (item1 == null) return -1;
      if (item2 == null) return -1;
      if (item1.IsFolder && item1.Label == "..") return -1;
      if (item2.IsFolder && item2.Label == "..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1;

      string strSize1 = "";
      string strSize2 = "";
      if (item1.FileInfo != null) strSize1 = MediaPortal.Util.Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo != null) strSize2 = MediaPortal.Util.Utils.GetSize(item2.FileInfo.Length);

      SortMethod method = currentSortMethod;
      bool bAscending = sortAscending;

      switch (method)
      {
        case SortMethod.Name:
          if (bAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }

        case SortMethod.Title:
          string strTitle1 = item1.Label;
          string strTitle2 = item2.Label;
          if (item1.MusicTag != null) strTitle1 = ((FileItem)item1.MusicTag).Title;
          if (item2.MusicTag != null) strTitle2 = ((FileItem)item2.MusicTag).Title;
          if (bAscending)
          {
            return String.Compare(strTitle1, strTitle2, true);
          }
          else
          {
            return String.Compare(strTitle2, strTitle1, true);
          }

        case SortMethod.Filename:
          string strFile1 = MediaPortal.Util.Utils.GetFilename(item1.Path);
          string strFile2 = MediaPortal.Util.Utils.GetFilename(item2.Path);
          if (bAscending)
          {
            return String.Compare(strFile1, strFile2, true);
          }
          else
          {
            return String.Compare(strFile2, strFile1, true);
          }

        case SortMethod.Rating:
          int iRating1 = 0;
          int iRating2 = 0;
          if (item1.MusicTag != null) iRating1 = ((FileItem)item1.MusicTag).Rating;
          if (item2.MusicTag != null) iRating2 = ((FileItem)item2.MusicTag).Rating;
          if (bAscending)
          {
            return (int)(iRating1 - iRating2);
          }
          else
          {
            return (int)(iRating2 - iRating1);
          }

        case SortMethod.LaunchCount:
          int iLaunchCount1 = 0;
          int iLaunchCount2 = 0;
          if (item1.MusicTag != null) iLaunchCount1 = ((FileItem)item1.MusicTag).LaunchCount;
          if (item2.MusicTag != null) iLaunchCount2 = ((FileItem)item2.MusicTag).LaunchCount;
          if (bAscending)
          {
            return (int)(iLaunchCount1 - iLaunchCount2);
          }
          else
          {
            return (int)(iLaunchCount2 - iLaunchCount1);
          }

        case SortMethod.LastTimeLaunched:
          DateTime dateLastTimeLaunched1 = DateTime.MinValue;
          DateTime dateLastTimeLaunched2 = DateTime.MinValue;
          if (item1.MusicTag != null) dateLastTimeLaunched1 = ((FileItem)item1.MusicTag).LastTimeLaunched;
          if (item2.MusicTag != null) dateLastTimeLaunched2 = ((FileItem)item2.MusicTag).LastTimeLaunched;
          if (bAscending)
          {
            return DateTime.Compare(dateLastTimeLaunched1, dateLastTimeLaunched2);
          }
          else
          {
            return DateTime.Compare(dateLastTimeLaunched2, dateLastTimeLaunched1);
          }
      }
      return 0;
    }

  }
}
