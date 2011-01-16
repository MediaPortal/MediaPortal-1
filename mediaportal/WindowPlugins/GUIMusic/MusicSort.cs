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

using System;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.TagReader;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for MusicSort.
  /// </summary>
  public class MusicSort : IComparer<GUIListItem>
  {
    private SortMethod currentSortMethod;
    private bool sortAscending = true;

    public MusicSort(SortMethod method, bool ascending)
    {
      currentSortMethod = method;
      sortAscending = ascending;
    }

    public enum SortMethod
    {
      Name = 0,
      Date = 1, // Shares View = File Modification Date, Database View = Date Added
      Size = 2,
      Track = 3,
      Duration = 4,
      Title = 5,
      Artist = 6,
      Album = 7,
      Filename = 8,
      Rating = 9,
      AlbumArtist = 10, // Only used internally when albumartists or albums need to be sorted by Artist
      Year = 11, // Used Internally, when Sorting by Date is selected from GUI and Year defined as DefaultSort
      DiscID = 12,
      Composer = 13,
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

      MusicTag tag1 = (MusicTag)item1.MusicTag;
      MusicTag tag2 = (MusicTag)item2.MusicTag;
      
      string strSize1 = "";
      string strSize2 = "";
      if (item1.FileInfo != null)
      {
        strSize1 = Util.Utils.GetSize(item1.FileInfo.Length);
      }
      if (item2.FileInfo != null)
      {
        strSize2 = Util.Utils.GetSize(item2.FileInfo.Length);
      }

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


        case SortMethod.Date:
          if (item1.FileInfo == null || item2.FileInfo == null)
          {
            // We didn't get a FileInfo. So it's a DB View and we sort on Date Added from DB
            DateTime time1 = DateTime.MinValue;
            DateTime time2 = DateTime.MinValue;
            if (tag1 != null)
            {
              time1 = tag1.DateTimeModified;
            }
            if (tag2 != null)
            {
              time2 = tag2.DateTimeModified;
            }

            item1.Label2 = time1.ToShortDateString();
            item2.Label2 = time2.ToShortDateString();
            if (bAscending)
            {
              return DateTime.Compare(time1, time2);
            }
            else
            {
              return DateTime.Compare(time2, time1);
            }
          }
          else
          {
            // Do sorting on File Date. Needed for Shares View
            item1.Label2 = item1.FileInfo.CreationTime.ToShortDateString() + " " +
                           item1.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
            item2.Label2 = item2.FileInfo.CreationTime.ToShortDateString() + " " +
                           item2.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
            if (bAscending)
            {
              return DateTime.Compare(item1.FileInfo.CreationTime, item2.FileInfo.CreationTime);
            }
            else
            {
              return DateTime.Compare(item2.FileInfo.CreationTime, item1.FileInfo.CreationTime);
            }
          }

        case SortMethod.Year:
          item1.Label2 = item1.Year.ToString();
          item2.Label2 = item2.Year.ToString();

          // When sorting on Year, we need to take also the Label into account and sort on that as well
          string compVal1 = item1.Year.ToString() + item1.Label;
          string compVal2 = item2.Year.ToString() + item2.Label;
          if (bAscending)
          {
            if (item1.Year == item2.Year)
            {
              // When the Year is equal just sort on the Label
              return String.Compare(item1.Label, item2.Label, true);
            }
            return String.Compare(compVal1, compVal2, true);
          }
          else
          {
            if (item1.Year == item2.Year)
            {
              // When the Year is equal, sort on label ASCENDING, altough sorting on year is DESC
              return String.Compare(item1.Label, item2.Label, true);
            }
            return String.Compare(compVal2, compVal1, true);
          }

        case SortMethod.Rating:
          int iRating1 = 0;
          int iRating2 = 0;
          if (tag1 != null)
          {
            iRating1 = tag1.Rating;
          }
          if (tag2 != null)
          {
            iRating2 = tag2.Rating;
          }
          if (bAscending)
          {
            return (int)(iRating1 - iRating2);
          }
          else
          {
            return (int)(iRating2 - iRating1);
          }

        case SortMethod.Size:
          if (item1.FileInfo == null)
          {
            return -1;
          }
          if (item2.FileInfo == null)
          {
            return -1;
          }
          if (bAscending)
          {
            return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
          }
          else
          {
            return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
          }

        case SortMethod.Track:
          int iTrack1 = 0;
          int iTrack2 = 0;
          int iDisk1 = 0;
          int iDisk2 = 0;
          if (tag1 != null)
          {
            iTrack1 = tag1.Track;
            iDisk1 = tag1.DiscID;
          }
          if (tag2 != null)
          {
            iTrack2 = tag2.Track;
            iDisk2 = tag2.DiscID;
          }
          if (bAscending)
          {
            if (iDisk1 != iDisk2)
            {
              return iDisk1.CompareTo(iDisk2);
            }
            else
            {
              return iTrack1.CompareTo(iTrack2);
            }
          }
          else
          {
            if (iDisk1 != iDisk2)
            {
              return iDisk2.CompareTo(iDisk1);
            }
            else
            {
              return iTrack2.CompareTo(iTrack1);
            }
          }

        case SortMethod.Duration:
          int iDuration1 = 0;
          int iDuration2 = 0;
          if (tag1 != null)
          {
            iDuration1 = tag1.Duration;
          }
          if (tag2 != null)
          {
            iDuration2 = tag2.Duration;
          }
          if (bAscending)
          {
            return (int)(iDuration1 - iDuration2);
          }
          else
          {
            return (int)(iDuration2 - iDuration1);
          }

        case SortMethod.Title:
          string strTitle1 = item1.Label;
          string strTitle2 = item2.Label;
          if (tag1 != null)
          {
            strTitle1 = tag1.Title;
          }
          if (tag2 != null)
          {
            strTitle2 = tag2.Title;
          }
          if (bAscending)
          {
            return String.Compare(strTitle1, strTitle2, true);
          }
          else
          {
            return String.Compare(strTitle2, strTitle1, true);
          }

        case SortMethod.Artist:
          string artist1 = "";
          string artist2 = "";
          if (tag1 != null)
          {
            artist1 = tag1.Artist;
          }
          if (tag2 != null)
          {
            artist2 = tag2.Artist;
          }
          if (bAscending)
          {
            return String.Compare(artist1, artist2, true);
          }
          else
          {
            return String.Compare(artist2, artist1, true);
          }

        case SortMethod.AlbumArtist:
          string albumartist1 = "";
          string albumartist2 = "";
          if (tag1 != null)
          {
            albumartist1 = tag1.AlbumArtist;
          }
          if (tag2 != null)
          {
            albumartist2 = tag2.AlbumArtist;
          }
          if (bAscending)
          {
            return String.Compare(albumartist1, albumartist2, true);
          }
          else
          {
            return String.Compare(albumartist2, albumartist1, true);
          }

        case SortMethod.Album:
          string strAlbum1 = "";
          string strAlbum2 = "";
          if (tag1 != null)
          {
            strAlbum1 = tag1.Album;
          }
          if (tag2 != null)
          {
            strAlbum2 = tag2.Album;
          }
          if (bAscending)
          {
            return String.Compare(strAlbum1, strAlbum2, true);
          }
          else
          {
            return String.Compare(strAlbum2, strAlbum1, true);
          }


        case SortMethod.Filename:
          string strFile1 = Util.Utils.GetFilename(item1.Path);
          string strFile2 = Util.Utils.GetFilename(item2.Path);
          if (bAscending)
          {
            return String.Compare(strFile1, strFile2, true);
          }
          else
          {
            return String.Compare(strFile2, strFile1, true);
          }

        case SortMethod.DiscID:
          // sort to by album then album artist then disc#
          int disc1 = 0;
          int disc2 = 0;
          if (tag1 != null)
          {
            disc1 = tag1.DiscID;
          }
          if (tag2 != null)
          {
            disc2 = tag2.DiscID;
          }
          if (bAscending)
          {
            if(tag1.Album == tag2.Album)
            {
              if(tag1.AlbumArtist == tag2.AlbumArtist)
              {
                return (int)(disc1 - disc2);
              }
              else
              {
                return string.Compare(tag1.AlbumArtist, tag2.AlbumArtist);
              }
            }
            else
            {
              return string.Compare(tag1.Album, tag2.Album);
            }
          }
          else
          {
            if(tag1.Album == tag2.Album)
            {
              if(tag1.AlbumArtist == tag2.AlbumArtist)
              {
                return (int)(disc2 - disc1);
              }
              else
              {
                return string.Compare(tag2.AlbumArtist, tag1.AlbumArtist);
              }
            }
            else
            {
              return string.Compare(tag2.Album, tag1.Album);
            }
          }
          
        case SortMethod.Composer:
          string strComposer1 = "";
          string strComposer2 = "";
          if (tag1 != null)
          {
            strComposer1 = tag1.Composer;
          }
          if (tag2 != null)
          {
            strComposer2 = tag2.Composer;
          }
  
          if (bAscending)
          {
            return String.Compare(strComposer1, strComposer2, true);
          }
          else
          {
            return String.Compare(strComposer2, strComposer1, true);
          }          
      }
      return 0;
    }
  }
}