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
using System.Runtime.InteropServices;

//using mdobler.XPCommonControls.ListViewAPI; 

namespace XPListview
{
  public class XPListViewGroupCollection : System.Collections.CollectionBase
  {
    public delegate void GroupAddedEventHandler(object sender, ListViewGroupEventArgs e);

    public delegate void GroupRemovedEventHandler(object sender, ListViewGroupEventArgs e);

    public event GroupAddedEventHandler GroupAdded;
    public event GroupRemovedEventHandler GroupRemoved;
    private XPListView _owner;

    public XPListViewGroup this[int index]
    {
      get { return ((XPListViewGroup)List[index]); }
      set { List[index] = value; }
    }

    public XPListViewGroupCollection(XPListView owner)
    {
      _owner = owner;
    }

    public int Add(XPListViewGroup value)
    {
      ListViewAPI.AddListViewGroup(_owner, value.GroupText, value.GroupIndex);
      if (GroupAdded != null)
      {
        GroupAdded(this, new ListViewGroupEventArgs(value));
      }
      return List.Add(value);
    }

    public int Add(string text, int index)
    {
      XPListViewGroup itm = new XPListViewGroup(text, index);
      ListViewAPI.AddListViewGroup(_owner, text, index);
      if (GroupAdded != null)
      {
        GroupAdded(this, new ListViewGroupEventArgs(itm));
      }
      return List.Add(itm);
    }

    public int IndexOf(XPListViewGroup value)
    {
      return List.IndexOf(value);
    }

    public void Insert(int index, XPListViewGroup value)
    {
      List.Insert(index, value);
    }

    public void Remove(XPListViewGroup value)
    {
      ListViewAPI.RemoveListViewGroup(_owner, value.GroupIndex);
      if (GroupRemoved != null)
      {
        GroupRemoved(this, new ListViewGroupEventArgs(value));
      }
      List.Remove(value);
    }

    public bool Contains(XPListViewGroup value)
    {
      return List.Contains(value);
    }

    public new void Clear()
    {
      ListViewAPI.ClearListViewGroup(_owner);
      List.Clear();
    }

    public void CopyTo(XPListViewGroup[] array, int index)
    {
      List.CopyTo(array, index);
    }
  }

  public class ListViewGroupEventArgs : EventArgs
  {
    public ListViewGroupEventArgs(XPListViewGroup item)
    {
      mItem = item;
    }

    public XPListViewGroup Item
    {
      get { return mItem; }
      set { mItem = value; }
    }

    private XPListViewGroup mItem;
  }
}