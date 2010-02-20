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
    } ;

    public int SortColumn;
    public SortOrder Order = SortOrder.Ascending;
    public OrderTypes OrderType = OrderTypes.AsString;

    public int Compare(object x, object y)
    {
      int compareResult = 0;
      // Cast the objects to be compared to ListViewItem objects
      ListViewItem listviewX = (ListViewItem)x;
      ListViewItem listviewY = (ListViewItem)y;
      switch (OrderType)
      {
        case OrderTypes.AsString:
          compareResult = SortColumn == 0
                            ? String.Compare(listviewX.Text, listviewY.Text)
                            : String.Compare(listviewX.SubItems[SortColumn].Text, listviewY.SubItems[SortColumn].Text);
          break;
        case OrderTypes.AsValue:
          string line1 = SortColumn == 0 ? listviewX.Text : listviewX.SubItems[SortColumn].Text;
          string line2 = SortColumn == 0 ? listviewY.Text : listviewY.SubItems[SortColumn].Text;

          //not sure for what these % are good but it should be catched when its not in......
          int pos1 = line1.IndexOf("%");
          if (pos1 >= 0)
          {
            line1 = line1.Substring(0, pos1);
          }

          int pos2 = line2.IndexOf("%");
          if (pos2 >= 0)
          {
            line2 = line2.Substring(0, pos2);
          }

          float value1 = 0;
          float value2 = 0;

          float.TryParse(line1, out value1);
          float.TryParse(line2, out value2);

          if (value1 < value2)
            compareResult = -1;
          else
            compareResult = value1 > value2 ? 1 : 0;

          break;
      }
      // Calculate correct return value based on object comparison
      if (Order == SortOrder.Ascending)
      {
        // Ascending sort is selected,
        // return normal result of compare operation
        return compareResult;
      }
      if (Order == SortOrder.Descending)
      {
        // Descending sort is selected,
        // return negative result of compare operation
        return (-compareResult);
      }
      // Return '0' to indicate they are equal
      return 0;
    }
  }
}