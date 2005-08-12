/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Collections;
using MediaPortal.GUI.Library;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for ProgramDBComparer.
  /// </summary>
  public class ProgramDBComparer: IComparer
  {
    enum SortMethod
    {
      SORT_NAME = 0, SORT_LAUNCHES = 1, SORT_RECENT = 2, SORT_RATING = 3
    } 

    SortMethod mCurrentSortMethod = SortMethod.SORT_NAME;
    public bool sortAscending = true;


    public ProgramDBComparer(){}

    public string currentSortMethodAsText
    {
      get
      {
        return GetCurrentSortMethodAsText();
      }
    }

    public int currentSortMethodIndex
    {
      get
      {
        return (int)mCurrentSortMethod;
      }
      set
      {
        SetCurrentSortMethodAsIndex(value);
      }
    }

    private string GetCurrentSortMethodAsText()
    {
      string strLine = "";
      SortMethod method = mCurrentSortMethod;
      switch (method)
      {
        case SortMethod.SORT_NAME:
          strLine = GUILocalizeStrings.Get(103);
          break;
        case SortMethod.SORT_LAUNCHES:
          strLine = GUILocalizeStrings.Get(13016); 
          break;
        case SortMethod.SORT_RECENT:
          strLine = GUILocalizeStrings.Get(13017);
          break;
        case SortMethod.SORT_RATING:
          strLine = GUILocalizeStrings.Get(13018);
          break;
      }
      return strLine;
    }

    private void SetCurrentSortMethodAsIndex(int value)
    {
      try
      {
        mCurrentSortMethod = (SortMethod)value;
      }
      catch 
      {
        mCurrentSortMethod = 0;
      }
    }



    public void updateState()
    {
      switch (mCurrentSortMethod)
      {
        //			SORT_NAME=0,
        //			SORT_LAUNCHES=1,
        //			SORT_RECENT=2,
        //			SORT_RATING=3

        case SortMethod.SORT_NAME:
          mCurrentSortMethod = SortMethod.SORT_LAUNCHES;
          sortAscending = false;
          break;
        case SortMethod.SORT_LAUNCHES:
          mCurrentSortMethod = SortMethod.SORT_RECENT;
          sortAscending = false;
          break;
        case SortMethod.SORT_RECENT:
          mCurrentSortMethod = SortMethod.SORT_RATING;
          sortAscending = false;
          break;
        case SortMethod.SORT_RATING:
          mCurrentSortMethod = SortMethod.SORT_NAME;
          sortAscending = true;
          break;
      }
    }

    public int Compare(object x, object y)
    {
      FileItem curFile1 = null;
      FileItem curFile2 = null;
      if (x == y)
        return 0;
      GUIListItem item1 = (GUIListItem)x;
      GUIListItem item2 = (GUIListItem)y;
      if (item1 == null)
        return  - 1;
      if (item2 == null)
        return  - 1;
      if (item1.MusicTag == null)
        return  - 1;
      if (item2.MusicTag == null)
        return  - 1;
      if (item1.IsFolder && item1.Label == "..")
        return  - 1;
      if (item2.IsFolder && item2.Label == "..")
        return  - 1;
      if (item1.IsFolder && !item2.IsFolder)
        return  - 1;
      if (item1.IsFolder && item2.IsFolder)
        return 0;
      //don't sort folders!
      else if (!item1.IsFolder && item2.IsFolder)
        return 1;


      if (mCurrentSortMethod != SortMethod.SORT_NAME)
      {
        curFile1 = (FileItem)item1.MusicTag;
        curFile2 = (FileItem)item2.MusicTag;
        if (curFile1 == null)
          return  - 1;
        if (curFile2 == null)
          return  - 1;
      }

      // ok let's start sorting :-)
      int nTemp;
      switch (mCurrentSortMethod)
      {
        case SortMethod.SORT_NAME:
          item1.Label2 = "";
          item2.Label2 = "";
          if (sortAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }
        case SortMethod.SORT_LAUNCHES:
          item1.Label2 = String.Format("{0}", curFile1.LaunchCount);
          item2.Label2 = String.Format("{0}", curFile2.LaunchCount);
          if (curFile1.LaunchCount == curFile2.LaunchCount)
          {
            // second sort always title ASC
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            if (curFile1.LaunchCount < curFile2.LaunchCount)
            {
              nTemp =  - 1;
            }
            else
            {
              nTemp = 1;
            }
            if (sortAscending)
            {
              return nTemp;
            }
            else
            {
              return  - nTemp;
            }
          }

        case SortMethod.SORT_RATING:
          item1.Label2 = String.Format("{0}", curFile1.Rating);
          item2.Label2 = String.Format("{0}", curFile2.Rating);
          if (curFile1.Rating == curFile2.Rating)
          {
            // second sort always title ASC
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            if (curFile1.Rating < curFile2.Rating)
            {
              nTemp =  - 1;
            }
            else
            {
              nTemp = 1;
            }
            if (sortAscending)
            {
              return nTemp;
            }
            else
            {
              return  - nTemp;
            }
          }

        case SortMethod.SORT_RECENT:
          if (curFile1.LastTimeLaunched > DateTime.MinValue)
          {
            item1.Label2 = curFile1.LastTimeLaunched.ToShortDateString();
          }
          else
          {
            item1.Label2 = "";
          }
          if (curFile2.LastTimeLaunched > DateTime.MinValue)
          {
            item2.Label2 = curFile1.LastTimeLaunched.ToShortDateString();
          }
          else
          {
            item2.Label2 = "";
          }

          if (curFile1.LastTimeLaunched == curFile2.LastTimeLaunched)
          {
            // second sort always title ASC
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            if (curFile1.LastTimeLaunched < curFile2.LastTimeLaunched)
            {
              nTemp =  - 1;
            }
            else
            {
              nTemp = 1;
            }
            if (sortAscending)
            {
              return nTemp;
            }
            else
            {
              return  - nTemp;
            }
          }
      }

      return 0;
    }

  }
}