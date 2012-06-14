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
      TimesPlayed = 14
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
      string strAlbum1 = "";
      string strAlbum2 = "";
      string strAlbumArtist1 = "";
      string strAlbumArtist2 = "";
      string strArtist1 = "";
      string strArtist2 = "";
      int iDisc1 = 0;
      int iDisc2 = 0;
      int iTrack1 = 0;
      int iTrack2 = 0;
      int iRating1 = 0;
      int iRating2 = 0;
      int iDuration1 = 0;
      int iDuration2 = 0;
      int iTimesPlayed1 = 0;
      int iTimesPlayed2 = 0;

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
            long compare = (item1.FileInfo.Length - item2.FileInfo.Length);
            return compare == 0 ? 0 : compare < 0 ? -1 : 1;
          }
          else
          {
            long compare = (item2.FileInfo.Length - item1.FileInfo.Length);
            return compare == 0 ? 0 : compare < 0 ? -1 : 1;
          }

        case SortMethod.Track:
          if (tag1 != null)
          {
            iTrack1 = tag1.Track;
            iDisc1 = tag1.DiscID;
          }
          if (tag2 != null)
          {
            iTrack2 = tag2.Track;
            iDisc2 = tag2.DiscID;
          }
          if (bAscending)
          {
            if (iDisc1 != iDisc2)
            {
              return iDisc1.CompareTo(iDisc2);
            }
            else
            {
              return iTrack1.CompareTo(iTrack2);
            }
          }
          else
          {
            if (iDisc1 != iDisc2)
            {
              return iDisc2.CompareTo(iDisc1);
            }
            else
            {
              return iTrack2.CompareTo(iTrack1);
            }
          }

        case SortMethod.Duration:
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
          if (tag1 != null)
          {
            strArtist1 = tag1.Artist;
          }
          if (tag2 != null)
          {
            strArtist2 = tag2.Artist;
          }
          if (bAscending)
          {
            return String.Compare(strArtist1, strArtist2, true);
          }
          else
          {
            return String.Compare(strArtist2, strArtist1, true);
          }

        case SortMethod.AlbumArtist:
          if (tag1 != null)
          {
            strAlbumArtist1 = tag1.AlbumArtist;
          }
          if (tag2 != null)
          {
            strAlbumArtist2 = tag2.AlbumArtist;
          }
          if (bAscending)
          {
            return String.Compare(strAlbumArtist1, strAlbumArtist2, true);
          }
          else
          {
            return String.Compare(strAlbumArtist2, strAlbumArtist1, true);
          }

        case SortMethod.Album:
        case SortMethod.DiscID:
          //sort by album => album artist => disc# => track
          if (tag1 != null)
          {
            strAlbum1 = tag1.Album;
            strAlbumArtist1 = tag1.AlbumArtist;
            iDisc1 = tag1.DiscID;
            iTrack1 = tag1.Track;
          }
          if (tag2 != null)
          {
            strAlbum2 = tag2.Album;
            strAlbumArtist2 = tag2.AlbumArtist;
            iDisc2 = tag2.DiscID;
            iTrack2 = tag2.Track;
          }
          if (bAscending)
          {
            if (strAlbum1 == strAlbum2)
            {
              if (strAlbumArtist1 == strAlbumArtist2)
              {
                if (iDisc1 == iDisc2)
                {
                  return (iTrack1 - iTrack2);
                }
                else
                {
                  return (iDisc1 - iDisc2);
                }
              }
              else
              {
                return String.Compare(strAlbumArtist1, strAlbumArtist2, true);
              }
            }
            else
            {
              return String.Compare(strAlbum1, strAlbum2, true);
            }
          }
          else
          {
            if (strAlbum1 == strAlbum2)
            {
              if (strAlbumArtist1 == strAlbumArtist2)
              {
                if (iDisc1 == iDisc2)
                {
                  return (iTrack2 - iTrack1);
                }
                else
                {
                  return (iDisc2 - iDisc1);
                }
              }
              else
              {
                return String.Compare(strAlbumArtist2, strAlbumArtist1, true);
              }
            }
            else
            {
              return String.Compare(strAlbum2, strAlbum1, true);
            }
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

        case SortMethod.TimesPlayed:
          if (tag1 != null)
          {
            iTimesPlayed1 = tag1.TimesPlayed;
          }
          if (tag2 != null)
          {
            iTimesPlayed2 = tag2.TimesPlayed;
          }

          if (bAscending)
          {
            return iTimesPlayed1.CompareTo(iTimesPlayed2);
          }
          else
          {
            return iTimesPlayed2.CompareTo(iTimesPlayed1);
          }

      }
      return 0;
    }
  }
}