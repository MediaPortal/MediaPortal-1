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

using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;

namespace MediaPortal.UserInterface.Controls
{
  public class ImageTextComparer : IComparer
  {
    //private CaseInsensitiveComparer ObjectCompare;
    private readonly NumberCaseInsensitiveComparer ObjectCompare;

    public ImageTextComparer()
    {
      // Initialize the CaseInsensitiveComparer object
      ObjectCompare = new NumberCaseInsensitiveComparer();
    }

    public int Compare(object x, object y)
    {
      //int compareResult;
      // Cast the objects to be compared to ListViewItem objects
      ListViewItem listviewX = (ListViewItem)x;
      int image1 = listviewX.ImageIndex;
      ListViewItem listviewY = (ListViewItem)y;
      int image2 = listviewY.ImageIndex;
      if (image1 < image2)
      {
        return -1;
      }
      if (image1 == image2)
      {
        return ObjectCompare.Compare(listviewX.Text, listviewY.Text);
      }
      return 1;
    }
  }

  public class NumberCaseInsensitiveComparer : CaseInsensitiveComparer
  {
    public new int Compare(object x, object y)
    {
      // in case x,y are strings and actually number,
      // convert them to int and use the base.Compare for comparison
      if ((x is System.String) && IsWholeNumber((string)x)
          && (y is System.String) && IsWholeNumber((string)y))
      {
        return base.Compare(System.Convert.ToInt32(x),
                            System.Convert.ToInt32(y));
      }
      return base.Compare(x, y);
    }

    private static bool IsWholeNumber(string strNumber)
    {
      // use a regular expression to find out if string is actually a number
      Regex objNotWholePattern = new Regex("[^0-9]");
      return !objNotWholePattern.IsMatch(strNumber);
    }
  }
}