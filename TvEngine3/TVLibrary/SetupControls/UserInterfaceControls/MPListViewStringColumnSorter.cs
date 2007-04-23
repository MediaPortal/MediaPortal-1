using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace MediaPortal.UserInterface.Controls
{
  public class MPListViewStringColumnSorter : IComparer
  {
    public enum OrderTypes
    {
      AsString,
      AsValue
    };
    public int SortColumn = 0;
    public SortOrder Order = SortOrder.Ascending;
    public OrderTypes OrderType = OrderTypes.AsString;

    public int Compare(object x, object y)
    {
      int compareResult = 0;
      ListViewItem listviewX, listviewY;
      // Cast the objects to be compared to ListViewItem objects
      listviewX = (ListViewItem)x;
      listviewY = (ListViewItem)y;
      switch (OrderType)
      {
        case OrderTypes.AsString:
          if (SortColumn == 0)
          {
            compareResult = String.Compare(listviewX.Text, listviewY.Text);
          }
          else
          {
            // Compare the two items
            compareResult = String.Compare(listviewX.SubItems[SortColumn].Text, listviewY.SubItems[SortColumn].Text);
          }
          break;
        case OrderTypes.AsValue:
          string line1, line2;
          if (SortColumn == 0)
            line1 = listviewX.Text;
          else
            line1 = listviewX.SubItems[SortColumn].Text;

          if (SortColumn == 0)
            line2 = listviewY.Text;
          else
            line2 = listviewY.SubItems[SortColumn].Text;
          int pos1 = line1.IndexOf("%"); line1 = line1.Substring(0, pos1);
          int pos2 = line2.IndexOf("%"); line2 = line2.Substring(0, pos2);
          float value1 = float.Parse(line1);
          float value2 = float.Parse(line2);
          if (value1 < value2)
            compareResult = -1;
          else if (value1 > value2)
            compareResult = 1;
          else
            compareResult = 0;
          break;
      }
      // Calculate correct return value based on object comparison
      if (Order == SortOrder.Ascending)
      {
        // Ascending sort is selected,
        // return normal result of compare operation
        return compareResult;
      }
      else if (Order == SortOrder.Descending)
      {
        // Descending sort is selected,
        // return negative result of compare operation
        return (-compareResult);
      }
      else
      {
        // Return '0' to indicate they are equal
        return 0;
      }
    }
  }
}
