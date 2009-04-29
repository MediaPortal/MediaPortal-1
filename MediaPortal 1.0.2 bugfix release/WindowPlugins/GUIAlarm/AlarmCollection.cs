#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.Collections;

namespace MediaPortal.GUI.Alarm
{
  /// <summary>
  /// Summary description for AlarmCollection.
  /// </summary>
  public class AlarmCollection : CollectionBase
  {
    public enum AlarmField
    {
      Id,
      Name,
      Enabled,
      Time
    }

    public void Sort(AlarmField sortField, bool isAscending)
    {
      switch (sortField)
      {
        case AlarmField.Id:
          InnerList.Sort(new IdComparer());
          break;
        case AlarmField.Name:
          InnerList.Sort(new NameComparer());
          break;
        case AlarmField.Time:
          InnerList.Sort(new AlarmTimeComparer());
          break;
      }
      if (!isAscending)
      {
        InnerList.Reverse();
      }
    }


    private sealed class IdComparer : IComparer
    {
      public int Compare(object x, object y)
      {
        Alarm first = (Alarm) x;
        Alarm second = (Alarm) y;
        return first.Id - second.Id;
      }
    }

    private sealed class AlarmTimeComparer : IComparer
    {
      public int Compare(object x, object y)
      {
        Alarm first = (Alarm) x;
        Alarm second = (Alarm) y;
        return first.Time.TimeOfDay.CompareTo(second.Time.TimeOfDay);
      }
    }

    private sealed class NameComparer : IComparer
    {
      public int Compare(object x, object y)
      {
        Alarm first = (Alarm) x;
        Alarm second = (Alarm) y;
        return first.Name.CompareTo(second.Name);
      }
    }

    // Provide the strongly typed member for ICollection.
    public void CopyTo(Alarm[] array, int index)
    {
      ((ICollection) this).CopyTo(array, index);
    }

    public Alarm this[int index]
    {
      get { return ((Alarm) List[index]); }
      set { List[index] = value; }
    }

    public int Add(Alarm value)
    {
      return (List.Add(value));
    }

    public int IndexOf(Alarm value)
    {
      return (List.IndexOf(value));
    }

    public void Insert(int index, Alarm value)
    {
      List.Insert(index, value);
    }

    public void Remove(Alarm value)
    {
      List.Remove(value);
    }

    public bool Contains(Alarm value)
    {
      // If value is not of type Alarm, this will return false.
      return (List.Contains(value));
    }

    protected override void OnInsert(int index, Object value)
    {
      if (value.GetType() != Type.GetType("MediaPortal.GUI.Alarm.Alarm"))
      {
        throw new ArgumentException("value must be of type Alarm.", "value");
      }
    }

    protected override void OnRemove(int index, Object value)
    {
      if (value.GetType() != Type.GetType("MediaPortal.GUI.Alarm.Alarm"))
      {
        throw new ArgumentException("value must be of type Alarm.", "value");
      }
    }

    protected override void OnSet(int index, Object oldValue, Object newValue)
    {
      if (newValue.GetType() != Type.GetType("MediaPortal.GUI.Alarm.Alarm"))
      {
        throw new ArgumentException("newValue must be of type Alarm.", "newValue");
      }
    }

    protected override void OnValidate(Object value)
    {
      if (value.GetType() != Type.GetType("MediaPortal.GUI.Alarm.Alarm"))
      {
        throw new ArgumentException("value must be of type Alarm.");
      }
    }
  }
}