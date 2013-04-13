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
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Video.Database;

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
      NameAll = 1,
      Date = 2,
      Size = 3,
      Watched = 4,
      Year = 5,
      Rating = 6,
      Label = 7,
      Modified = 8,
      Created = 9,
    }

    protected SortMethod CurrentSortMethod;
    protected bool SortAscending;
    protected bool KeepFoldersTogether;
    protected bool UseSortTitle;
    
    public VideoSort(SortMethod sortMethod, bool ascending)
    {
      CurrentSortMethod = sortMethod;
      SortAscending = ascending;

      using (Profile.Settings xmlreader = new MPSettings())
      {
        KeepFoldersTogether = xmlreader.GetValueAsBool("movies", "keepfolderstogether", false);

        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_VIDEO_TITLE)
        {
          UseSortTitle = xmlreader.GetValueAsBool("moviedatabase", "usesorttitle", false);
        }
        else
        {
          UseSortTitle = false;
        }
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
      if (item1.IsFolder && !item2.IsFolder && CurrentSortMethod != SortMethod.NameAll)
      {
        return -1;
      }
      if (!item1.IsFolder && item2.IsFolder && CurrentSortMethod != SortMethod.NameAll)
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
              if (item1.IsFolder && item2.IsFolder)
              {
                return String.Compare(item1.Label, item2.Label, true);
              }
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
              if (item1.IsFolder && item2.IsFolder)
              {
                return String.Compare(item2.Label, item1.Label, true);
              }
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
        case SortMethod.NameAll:

          IMDBMovie movie1 = item1.AlbumInfoTag as IMDBMovie;
          IMDBMovie movie2 = item2.AlbumInfoTag as IMDBMovie;

          if (SortAscending)
          {
            if (!UseSortTitle)
            {
              return Util.StringLogicalComparer.Compare(item1.Label, item2.Label);
            }
            else
            {
              if (movie1 != null && movie2 != null && movie1.ID > 0 && movie2.ID > 0)
              {
                return Util.StringLogicalComparer.Compare(movie1.SortTitle, movie2.SortTitle);
              }
              else
              {
                return Util.StringLogicalComparer.Compare(item1.Label, item2.Label);
              }
            }
          }
          else
          {
            if (!UseSortTitle)
            {
              return Util.StringLogicalComparer.Compare(item2.Label, item1.Label);
            }
            else
            {
              if (movie1 != null && movie2 != null && movie1.ID > 0 && movie2.ID > 0)
              {
                return Util.StringLogicalComparer.Compare(movie2.SortTitle, movie1.SortTitle);
              }
              else
              {
                return Util.StringLogicalComparer.Compare(item2.Label, item1.Label);
              }
            }
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
            return Util.StringLogicalComparer.Compare(item1.DVDLabel, item2.DVDLabel);
          }
          else
          {
            return Util.StringLogicalComparer.Compare(item2.DVDLabel, item1.DVDLabel);
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
        
        case SortMethod.Watched:
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
              movie1 = item1.AlbumInfoTag as IMDBMovie;
              movie2 = item2.AlbumInfoTag as IMDBMovie;

              if (SortAscending)
              {
                if (!UseSortTitle)
                {
                  return Util.StringLogicalComparer.Compare(item1.Label, item2.Label);
                }
                else
                {
                  if (movie1 != null && movie2 != null && movie1.ID > 0 && movie2.ID > 0)
                  {
                    return Util.StringLogicalComparer.Compare(movie1.SortTitle, movie2.SortTitle);
                  }
                  else
                  {
                    return Util.StringLogicalComparer.Compare(item1.Label, item2.Label);
                  }
                }
              }
              else
              {
                if (!UseSortTitle)
                {
                  return Util.StringLogicalComparer.Compare(item2.Label, item1.Label);
                }
                else
                {
                  if (movie1 != null && movie2 != null && movie1.ID > 0 && movie2.ID > 0)
                  {
                    return Util.StringLogicalComparer.Compare(movie2.SortTitle, movie1.SortTitle);
                  }
                  else
                  {
                    return Util.StringLogicalComparer.Compare(item2.Label, item1.Label);
                  }
                }
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
          DateTime dateAddedWatched;
          DateTime.TryParseExact(item.Label2, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateAddedWatched);
          item.FileInfo.CreationTime = dateAddedWatched;
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