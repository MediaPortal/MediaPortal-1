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

namespace System.Collections
{
  public interface IPriorityQueue : ICollection, ICloneable, IList
  {
    int Enqueue(object O);
    object Dequeue();
    object Peek();
    void Update(int i);
  }

  public class PriorityQueue : IPriorityQueue, ICollection, ICloneable, IList
  {
    #region Contructors

    public PriorityQueue()
    {
      _comparer = Comparer.Default;
    }

    public PriorityQueue(IComparer comparer)
    {
      _comparer = comparer;
    }

    public PriorityQueue(int capacity)
    {
      _comparer = Comparer.Default;
      _innerList.Capacity = capacity;
    }

    public PriorityQueue(IComparer comparer, int capacity)
    {
      _comparer = comparer;
      _innerList.Capacity = capacity;
    }

    private PriorityQueue(ArrayList array, IComparer comparer, bool copy)
    {
      _innerList = copy ? (ArrayList) array.Clone() : array;
      _comparer = comparer;
    }

    #endregion

    private void Swap(int l, int r)
    {
      object temp = _innerList[l];

      _innerList[l] = _innerList[r];
      _innerList[r] = temp;
    }

    #region public methods

    /// <summary>
    /// Enqueue an object onto the PQ
    /// </summary>
    /// <param name="O">The new object</param>
    /// <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
    public int Enqueue(object O)
    {
      int p = _innerList.Count, p2;

      _innerList.Add(O);

      do
      {
        if (p == 0)
        {
          break;
        }

        p2 = (p - 1)/2;

        if (_comparer.Compare(_innerList[p], _innerList[p2]) < 0)
        {
          Swap(p, p2);
          p = p2;
        }
        else
        {
          break;
        }
      } while (true);
      return p;
    }

    /// <summary>
    /// Get the smallest object and remove it.
    /// </summary>
    /// <returns>The smallest object</returns>
    public object Dequeue()
    {
      object result = _innerList[0];
      int p = 0, p1, p2, pn;
      _innerList[0] = _innerList[_innerList.Count - 1];
      _innerList.RemoveAt(_innerList.Count - 1);
      do
      {
        pn = p;
        p1 = 2*p + 1;
        p2 = 2*p + 2;
        if (_innerList.Count > p1 && _comparer.Compare(_innerList[p], _innerList[p1]) > 0)
        {
          p = p1;
        }
        if (_innerList.Count > p2 && _comparer.Compare(_innerList[p], _innerList[p2]) > 0)
        {
          p = p2;
        }

        if (p == pn)
        {
          break;
        }
        Swap(p, pn);
      } while (true);
      return result;
    }

    /// <summary>
    /// Notify the PQ that the object at position i has changed
    /// and the PQ needs to restore order.
    /// Since you dont have access to any indexes (except by using the
    /// explicit IList.this) you should not call this function without knowing exactly
    /// what you do.
    /// </summary>
    /// <param name="i">The index of the changed object.</param>
    public void Update(int i)
    {
      int p = i, pn;
      int p1, p2;
      do
      {
        if (p == 0)
        {
          break;
        }
        p2 = (p - 1)/2;
        if (_comparer.Compare(_innerList[p], _innerList[p2]) < 0)
        {
          Swap(p, p2);
          p = p2;
        }
        else
        {
          break;
        }
      } while (true);
      if (p < i)
      {
        return;
      }
      do
      {
        pn = p;
        p1 = 2*p + 1;
        p2 = 2*p + 2;
        if (_innerList.Count > p1 && _comparer.Compare(_innerList[p], _innerList[p1]) > 0)
        {
          p = p1;
        }
        if (_innerList.Count > p2 && _comparer.Compare(_innerList[p], _innerList[p2]) > 0)
        {
          p = p2;
        }

        if (p == pn)
        {
          break;
        }

        Swap(p, pn);
      } while (true);
    }

    /// <summary>
    /// Get the smallest object without removing it.
    /// </summary>
    /// <returns>The smallest object</returns>
    public object Peek()
    {
      if (_innerList.Count > 0)
      {
        return _innerList[0];
      }
      return null;
    }

    public bool Contains(object value)
    {
      return _innerList.Contains(value);
    }

    public void Clear()
    {
      _innerList.Clear();
    }

    public int Count
    {
      get { return _innerList.Count; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _innerList.GetEnumerator();
    }

    public void CopyTo(Array array, int index)
    {
      _innerList.CopyTo(array, index);
    }

    public object Clone()
    {
      return new PriorityQueue(_innerList, _comparer, true);
    }

    public bool IsSynchronized
    {
      get { return _innerList.IsSynchronized; }
    }

    public object SyncRoot
    {
      get { return this; }
    }

    #endregion

    #region explicit implementation

    bool IList.IsReadOnly
    {
      get { return false; }
    }

    object IList.this[int index]
    {
      get { return _innerList[index]; }
      set
      {
        _innerList[index] = value;
        Update(index);
      }
    }

    int IList.Add(object o)
    {
      return Enqueue(o);
    }

    void IList.RemoveAt(int index)
    {
      throw new NotSupportedException();
    }

    void IList.Insert(int index, object value)
    {
      throw new NotSupportedException();
    }

    void IList.Remove(object value)
    {
      throw new NotSupportedException();
    }

    int IList.IndexOf(object value)
    {
      throw new NotSupportedException();
    }

    bool IList.IsFixedSize
    {
      get { return false; }
    }

    public static PriorityQueue Syncronized(PriorityQueue P)
    {
      return new PriorityQueue(ArrayList.Synchronized(P._innerList), P._comparer, false);
    }

    public static PriorityQueue ReadOnly(PriorityQueue P)
    {
      return new PriorityQueue(ArrayList.ReadOnly(P._innerList), P._comparer, false);
    }

    #endregion

    #region Fields

    private ArrayList _innerList = new ArrayList();
    private IComparer _comparer;

    #endregion Fields
  }
}