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

using System.ComponentModel;
using System.Drawing;

//using mdobler.XPCommonControls.ListViewAPI; 

namespace Mediaportal.TV.Server.Plugins.ServerBlaster.Learn.XPListView
{
  [TypeConverter(typeof (XPListViewItemConverter))]
  public class XPListViewItem : System.Windows.Forms.ListViewItem
  {
    private int _groupIndex;

    public XPListViewItem()
    {}

    public XPListViewItem(string text) : base(text) {}

    public XPListViewItem(string text, int imageIndex) : base(text, imageIndex) {}

    public XPListViewItem(string[] items) : base(items) {}

    public XPListViewItem(string[] items, int imageIndex) : base(items, imageIndex) {}

    public XPListViewItem(XPListViewItem.ListViewSubItem[] subItems, int imageIndex) : base(subItems, imageIndex) {}

    public XPListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor, Font font)
      : base(items, imageIndex, foreColor, backColor, font) {}

    public XPListViewItem(string text, int imageIndex, int groupIndex) : base(text, imageIndex)
    {
      _groupIndex = groupIndex;
    }

    public XPListViewItem(string[] items, int imageIndex, int groupIndex) : base(items, imageIndex)
    {
      GroupIndex = groupIndex;
    }

    public XPListViewItem(XPListViewItem.ListViewSubItem[] subItems, int imageIndex, int groupIndex)
      : base(subItems, imageIndex)
    {
      GroupIndex = groupIndex;
    }

    public XPListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor, Font font, int groupIndex)
      : base(items, imageIndex, foreColor, backColor, font)
    {
      GroupIndex = groupIndex;
    }

    [Browsable(true), Category("Info")]
    public int GroupIndex
    {
      get { return _groupIndex; }
      set
      {
        _groupIndex = value;
        ListViewAPI.AddItemToGroup(((XPListView)base.ListView), base.Index, _groupIndex);
      }
    }

    [Browsable(false)]
    internal string[] SubItemsArray
    {
      get
      {
        if (SubItems.Count == 0)
        {
          return null;
        }

        string[] a = new string[SubItems.Count - 1];

        for (int i = 0; i <= SubItems.Count - 1; i++)
        {
          a[i] = SubItems[i].Text;
        }
        return a;
      }
    }
  }
}