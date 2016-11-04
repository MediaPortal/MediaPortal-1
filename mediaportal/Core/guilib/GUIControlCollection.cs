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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  public sealed class GUIControlCollection : IList<GUIControl>, IDisposable
  {
    // we maintain ids to avoid expensive virtual property calls
    private List<GUIControl> list = new List<GUIControl>();
    private Dictionary<int, GUIControl> knownIDs = new Dictionary<int, GUIControl>();

    #region IList implementation

    public int IndexOf(GUIControl item)
    {
      return list.IndexOf(item);
    }

    public void Insert(int index, GUIControl item)
    {
      TryAdd(item);
      list.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
      knownIDs.Remove(list[index].GetID);
      list.RemoveAt(index);
    }

    public GUIControl this[int index]
    {
      get { return list[index]; }
      set
      {
        TryAdd(value);
        list[index] = value;
      }
    }

    public void Add(GUIControl item)
    {
      TryAdd(item);
      list.Add(item);
    }

    public void Clear()
    {
      knownIDs.Clear();
      list.Clear();
    }

    public bool Contains(GUIControl item)
    {
      return IndexOf(item) >= 0;
    }

    public void CopyTo(GUIControl[] array, int arrayIndex)
    {
      list.CopyTo(array, arrayIndex);
    }

    public int Count
    {
      get { return list.Count; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public bool Remove(GUIControl item)
    {
      var index = IndexOf(item);
      if (index >= 0)
      {
        RemoveAt(IndexOf(item));
        return true;
      }
      return false;
    }

    public IEnumerator<GUIControl> GetEnumerator()
    {
      return list.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return list.GetEnumerator();
    }

    #endregion

    #region public methods

    public GUIControl GetControlById(int id)
    {
      GUIControl knownControl;

      if (knownIDs.TryGetValue(id, out knownControl))
      {
        // looking good
        // make sure its id didn't change
        if (knownControl.GetID == id)
        {
          // hit
          return knownControl;
        }
        GUIControl sub;
        if ((sub = knownControl.GetControlById(id)) != null)
        {
          // control is a subcontrol of knowncontrol
          return sub;
        }
        else
        {
          // control has changed it's id since we stored it
          knownIDs.Remove(id);
          TryAdd(knownControl);
          // we have to retry from top since a control before this one might have the wanted id
        }
      }

      // we should very rarely reach here, means we didn't find it by stored id, most likely it doesnt exist
      // however we have to try in case a control changed its id since we stored it
      // it could also be a subcontrol
      foreach (GUIControl t in list.ToList())
      {
        GUIControl sub;
        if (t != null && (sub = t.GetControlById(id)) != null)
        {
          // control with id found, store item in this list, not potential subitem itself
          TryAdd(t);
          return sub;
        }
      }
      // not found, we can't remember that since a subcontrol might add it to its children which we wont know
      return null;
    }

    #endregion

    #region helpers

    private bool TryAdd(GUIControl control)
    {
      int id = control.GetID;
      if (knownIDs.ContainsKey(id))
        return false;
      try
      {
        knownIDs.Add(id, control);
        return true;
      }
      catch (Exception) {}

      return false;
    }

    #endregion

    #region IDisposable Members

    ~GUIControlCollection()
    {
      Dispose();
    }

    public void Dispose()
    {
      list.Dispose(); //only dispose items, do not remove collection.
    }

    #endregion

  }
}