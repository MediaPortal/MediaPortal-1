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
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Video.Database;
using System.Collections;

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
      Unwatched = 6,
      Modified = 7,
      Created = 8,
    }

    protected SortMethod CurrentSortMethod;
    protected bool SortAscending;
    protected bool KeepFoldersTogether;
    
    public VideoSort(SortMethod sortMethod, bool ascending)
    {
      CurrentSortMethod = sortMethod;
      SortAscending = ascending;

      using (Profile.Settings xmlreader = new MPSettings())
      {
        KeepFoldersTogether = xmlreader.GetValueAsBool("movies", "keepfolderstogether", false);
      }
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
      if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }
      if (KeepFoldersTogether)
      {
        if (item1.IsBdDvdFolder && !item2.IsBdDvdFolder)
        {
          return 1;
        }
        if (!item1.IsBdDvdFolder && item2.IsBdDvdFolder)
        {
          return -1;
        }
      }

      switch (CurrentSortMethod)
      {
        case SortMethod.Year:
          {
            if (SortAscending)
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
            if (SortAscending)
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

          if (SortAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }

        case SortMethod.Date: // Only recently added/watched->database view + date used for sort for title

          if (item1.FileInfo == null)
          {
            if (!this.TryGetFileInfo(ref item1))
            {
              return -1;
            }
          }

          if (item2.FileInfo == null)
          {
            if (!this.TryGetFileInfo(ref item2))
            {
              return -1;
            }
          }

          item1.Label2 = item1.FileInfo.CreationTime.ToShortDateString() + " " +
                          item1.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 = item2.FileInfo.CreationTime.ToShortDateString() + " " +
                          item2.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);

          if (item1.Label2 == string.Empty || item2.Label2 == string.Empty)
          {
            return -1;
          }

          if (SortAscending)
          {
            return DateTime.Compare(Convert.ToDateTime(item1.Label2), Convert.ToDateTime(item2.Label2));
          }
          else
          {
            return DateTime.Compare(Convert.ToDateTime(item2.Label2), Convert.ToDateTime(item1.Label2));
          }
          
        case SortMethod.Label:
          if (SortAscending)
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
            if (SortAscending)
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
            if (SortAscending)
            {
               long compare = (item1.FileInfo.Length - item2.FileInfo.Length);
               return compare == 0 ? 0 : compare < 0 ? -1 : 1;
            }
            else
            {
              long compare = (item2.FileInfo.Length - item1.FileInfo.Length);
              return compare == 0 ? 0 : compare < 0 ? -1 : 1;
            }
          }

        case SortMethod.Created:

          if (item1.FileInfo == null)
          {
            if (!this.TryGetFileInfo(ref item1))
            {
              return -1;
            }
          }

          if (item2.FileInfo == null)
          {
            if (!this.TryGetFileInfo(ref item2))
            {
              return -1;
            }
          }

          item1.Label2 = item1.FileInfo.CreationTime.ToShortDateString() + " " +
                         item1.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 = item2.FileInfo.CreationTime.ToShortDateString() + " " +
                         item2.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);

          if (SortAscending)
          {
            return DateTime.Compare(item1.FileInfo.CreationTime, item2.FileInfo.CreationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.CreationTime, item1.FileInfo.CreationTime);
          }
        
        case SortMethod.Modified:
        
          if (item1.FileInfo == null)
          {
            if (!this.TryGetFileInfo(ref item1))
            {
              return -1;
            }
          }

          if (item2.FileInfo == null)
          {
            if (!this.TryGetFileInfo(ref item2))
            {
              return -1;
            }
          }

          item1.Label2 = item1.FileInfo.ModificationTime.ToShortDateString() + " " +
                           item1.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 = item2.FileInfo.ModificationTime.ToShortDateString() + " " +
                           item2.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          
          if (SortAscending)
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
              ret = -1;
              if (!SortAscending) ret = 1;
            }
            else if (!item1.IsPlayed && item2.IsPlayed)
            {
              ret = 1;
              if (!SortAscending) ret = -1;
            }
            else
            {
              if (SortAscending)
              {
                return String.Compare(item1.Label, item2.Label, true);
              }
              else
              {
                return String.Compare(item2.Label, item1.Label, true);
              }
            }
            return ret;
          }
      }
      return 0;
    }

    /// <summary>
    /// In database view the file info isn't set. 
    /// This function trys to get the files from database and then creates the file info for it.
    /// </summary>
    /// <param name="item">Item to store the file info</param>
    /// <returns>True if FileInformation was created otherwise false</returns>
    private bool TryGetFileInfo(ref GUIListItem item)
    {
      if (item == null)
        return false;

      try
      {
        IMDBMovie movie1 = item.AlbumInfoTag as IMDBMovie;
        if (movie1 != null && movie1.ID > 0)
        {
          item.FileInfo = new Util.FileInformation();
          DateTime dateAdded;
          DateTime.TryParseExact(movie1.DateAdded, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateAdded);
          item.FileInfo.CreationTime = dateAdded;
        }
      }
      catch (Exception exp)
      {
        Log.Error("VideoSort::TryGetFileInfo -> Exception: {0}", exp.Message);
      }

      return item.FileInfo != null;
    }
  }
}