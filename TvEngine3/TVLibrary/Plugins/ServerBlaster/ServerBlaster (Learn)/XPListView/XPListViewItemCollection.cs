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
using System.Collections;
using System.Windows.Forms;

namespace XPListview
{
  public class XPListViewItemCollection : System.Windows.Forms.ListView.ListViewItemCollection
  {
    public delegate void ItemAddedEventHandler(object sender, ListViewItemEventArgs e);

    public delegate void ItemRemovedEventHandler(object sender, ListViewItemEventArgs e);

    public event ItemAddedEventHandler ItemAdded;
    public event ItemRemovedEventHandler ItemRemoved;

    public XPListViewItemCollection(XPListView owner) : base(((ListView)owner)) {}

    public XPListViewItem Add(XPListViewItem item)
    {
      XPListViewItem itm;
      itm = ((XPListViewItem)base.Add(item));
      ListViewAPI.AddItemToGroup(((XPListView)itm.ListView), itm.Index, itm.GroupIndex);
      if (ItemAdded != null)
      {
        ItemAdded(this, new ListViewItemEventArgs(itm));
      }
      return itm;
    }

    public new XPListViewItem Add(string text)
    {
      XPListViewItem itm = new XPListViewItem(text);
      return Add(itm);
    }

    public XPListViewItem Add(string text, int imageIndex, int groupindex)
    {
      XPListViewItem itm = new XPListViewItem(text, imageIndex, groupindex);
      return Add(itm);
    }

    public void AddRange(XPListViewItem[] values)
    {
      base.AddRange(values);
      foreach (XPListViewItem itm in values)
      {
        ListViewAPI.AddItemToGroup(((XPListView)itm.ListView), itm.Index, itm.GroupIndex);
        if (ItemAdded != null)
        {
          ItemAdded(this, new ListViewItemEventArgs(itm));
        }
      }
    }

    public bool Contains(XPListViewItem item)
    {
      return base.Contains(item);
    }

    public int IndexOf(XPListViewItem item)
    {
      return base.IndexOf(item);
    }

    public XPListViewItem Insert(int index, XPListViewItem item)
    {
      return ((XPListViewItem)base.Insert(index, item));
    }

    public XPListViewItem this[int displayIndex]
    {
      get { return ((XPListViewItem)base[displayIndex]); //((XPListViewItem)this[displayIndex])
      }
      set { this[displayIndex] = value; }
    }

    public void Remove(XPListViewItem item)
    {
      if (ItemRemoved != null)
      {
        ItemRemoved(this, new ListViewItemEventArgs(item));
      }
      base.Remove(item);
    }

    public void CopyTo(XPListViewItem[] array, int index)
    {
      base.CopyTo(array, index);
    }
  }


  public class ListViewItemEventArgs : EventArgs
  {
    private XPListViewItem mItem = new XPListViewItem();

    public ListViewItemEventArgs(XPListViewItem item)
    {
      mItem = item;
    }

    public XPListViewItem Item
    {
      get { return mItem; }
      set { mItem = value; }
    }
  }
}