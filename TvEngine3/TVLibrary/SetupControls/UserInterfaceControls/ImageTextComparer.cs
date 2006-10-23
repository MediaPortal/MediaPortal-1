using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using System.Collections;

namespace MediaPortal.UserInterface.Controls
{
  public class ImageTextComparer : IComparer
  {
    //private CaseInsensitiveComparer ObjectCompare;
    private NumberCaseInsensitiveComparer ObjectCompare;

    public ImageTextComparer()
    {
      // Initialize the CaseInsensitiveComparer object
      ObjectCompare = new NumberCaseInsensitiveComparer();
    }
    public int Compare(object x, object y)
    {
      //int compareResult;
      int image1, image2;
      ListViewItem listviewX, listviewY;
      // Cast the objects to be compared to ListViewItem objects
      listviewX = (ListViewItem)x;
      image1 = listviewX.ImageIndex;
      listviewY = (ListViewItem)y;
      image2 = listviewY.ImageIndex;
      if (image1 < image2)
      {
        return -1;
      }
      else if (image1 == image2)
      {
        return ObjectCompare.Compare(listviewX.Text, listviewY.Text);
      }
      else
      {
        return 1;
      }
    }
  }

  public class NumberCaseInsensitiveComparer : CaseInsensitiveComparer
  {
    public NumberCaseInsensitiveComparer()
    {

    }
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
      else
      {
        return base.Compare(x, y);
      }
    }
    private bool IsWholeNumber(string strNumber)
    { // use a regular expression to find out if string is actually a number
      Regex objNotWholePattern = new Regex("[^0-9]");
      return !objNotWholePattern.IsMatch(strNumber);
    }
  }
}

