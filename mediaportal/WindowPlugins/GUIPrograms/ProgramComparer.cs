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
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for ProgramComparer.
  /// </summary>
  public class ProgramComparer: IComparer
  {

    private enum SortMethod
    {
      SORT_NAME = 0, SORT_DATE = 1, SORT_SIZE = 2
    } 

    private SortMethod mCurrentSortMethod = SortMethod.SORT_NAME;
    public bool bAsc = true;


    public ProgramComparer(){}

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
        case SortMethod.SORT_DATE:
          strLine = GUILocalizeStrings.Get(104);
          break;
        case SortMethod.SORT_SIZE:
          strLine = GUILocalizeStrings.Get(105);
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
        case SortMethod.SORT_NAME:
          mCurrentSortMethod = SortMethod.SORT_DATE;
          bAsc = true;
          break;
        case SortMethod.SORT_DATE:
          mCurrentSortMethod = SortMethod.SORT_SIZE;
          bAsc = true;
          break;
        case SortMethod.SORT_SIZE:
          mCurrentSortMethod = SortMethod.SORT_NAME;
          bAsc = true;
          break;
      }
    }


    public int Compare(object x, object y)
    {
      if (x == y)
        return 0;
      GUIListItem item1 = (GUIListItem)x;
      GUIListItem item2 = (GUIListItem)y;

      if (item1 == null)
        return  - 1;
      if (item2 == null)
        return  - 1;
      if (item1.IsFolder && item1.Label == "..")
        return  - 1;
      if (item2.IsFolder && item2.Label == "..")
        return  - 1;
      if (item1.IsFolder && !item2.IsFolder)
        return  - 1;
      else if (!item1.IsFolder && item2.IsFolder)
        return 1;

      string strSize1 = "";
      string strSize2 = "";
      if (item1.FileInfo != null)
        strSize1 = Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo != null)
        strSize2 = Utils.GetSize(item2.FileInfo.Length);

      SortMethod method = mCurrentSortMethod;

      switch (method)
      {
        case SortMethod.SORT_NAME:
          item1.Label2 = "";
          item2.Label2 = "";

          if (bAsc)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }


        case SortMethod.SORT_DATE:
          if (item1.FileInfo == null)
            return  - 1;
          if (item2.FileInfo == null)
            return  - 1;

          item1.Label2 = item1.FileInfo.ModificationTime.ToShortDateString() + " " + item1.FileInfo.ModificationTime.ToString("t",
            CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 = item2.FileInfo.ModificationTime.ToShortDateString() + " " + item2.FileInfo.ModificationTime.ToString("t",
            CultureInfo.CurrentCulture.DateTimeFormat);
          if (bAsc)
          {
            return DateTime.Compare(item1.FileInfo.ModificationTime, item2.FileInfo.ModificationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.ModificationTime, item1.FileInfo.ModificationTime);
          }

        case SortMethod.SORT_SIZE:
          if (item1.FileInfo == null)
            return  - 1;
          if (item2.FileInfo == null)
            return  - 1;
          item1.Label2 = strSize1;
          item2.Label2 = strSize2;
          if (bAsc)
          {
            return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
          }
          else
          {
            return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
          }
      }
      return 0;
    }
  }
}
