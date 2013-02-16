#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

// This code is based on http://code.google.com/p/catwalkagogo/source/browse/trunk/CatWalk/Collections/ObservableHashSet.cs. Many thanks to the original author!

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public class TrackableCollection<T> : ISerializable, IDeserializationCallback, ISet<T>, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
  {
    private readonly HashSet<T> _items;
    private readonly List<T> _list;

    public TrackableCollection() : this(new HashSet<T>()) { }
    public TrackableCollection(HashSet<T> hashSet)
    {
      _items = hashSet;
      _list = new List<T>();
    }

    #region ISerializable Members

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      _items.GetObjectData(info, context);
    }

    #endregion

    #region Reentrancy

    private readonly SimpleMonitor _monitor = new SimpleMonitor();

    protected IDisposable BlockReentrancy()
    {
      _monitor.Enter();
      return _monitor;
    }

    protected void CheckReentrancy()
    {
      if ((_monitor.IsBusy && (CollectionChanged != null)) && (CollectionChanged.GetInvocationList().Length > 1))
        throw new InvalidOperationException();
    }

    #endregion

    #region IDeserializationCallback Members

    public void OnDeserialization(object sender)
    {
      _items.OnDeserialization(sender);
    }

    #endregion

    #region ISet<T> Members

    public bool Add(T item)
    {
      CheckReentrancy();
      if (!_items.Add(item))
        return false;

      _list.Add(item);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      return true;
    }

    public void ExceptWith(IEnumerable<T> other)
    {
      CheckReentrancy();
      _items.ExceptWith(other);
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
    }

    public void IntersectWith(IEnumerable<T> other)
    {
      CheckReentrancy();
      _items.IntersectWith(other);
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
      return _items.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
      return _items.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
      return _items.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
      return _items.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<T> other)
    {
      return _items.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<T> other)
    {
      return _items.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
      CheckReentrancy();
      _items.SymmetricExceptWith(other);
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
    }

    public void UnionWith(IEnumerable<T> other)
    {
      CheckReentrancy();
      _items.UnionWith(other);
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
    }

    #endregion

    #region ICollection<T> Members

    void ICollection<T>.Add(T item)
    {
      CheckReentrancy();
      _items.Add(item);
      _list.Add(item);
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    public void Clear()
    {
      CheckReentrancy();
      _items.Clear();
      _list.Clear();
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
    }

    public bool Contains(T item)
    {
      return _items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      _items.CopyTo(array, arrayIndex);
    }

    public int Count
    {
      get { return _items.Count; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public bool Remove(T item)
    {
      CheckReentrancy();
      if (!_items.Remove(item))
        return false;
      _list.Remove(item);
      OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
      OnPropertyChanged(new PropertyChangedEventArgs("Count"));
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
      return true;
    }

    public int IndexOf(T item)
    {
      CheckReentrancy();
      return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      CheckReentrancy();
      _list.Insert(index, item); // Attention: no duplicate check!!!
      _items.Add(item);
    }

    public void RemoveAt(int index)
    {
      CheckReentrancy();
      T tvalue = _list[index];
      _list.RemoveAt(index);
      if (!_list.Contains(tvalue)) // Same item can occure multiple times in list, but only once in HashSet.
        _items.Remove(tvalue);
    }

    public T this[int index]
    {
      get
      {
        CheckReentrancy();
        return _list[index];
      }
      set
      {
        CheckReentrancy();
        RemoveAt(index);
        _list[index] = value;
        Add(value);
      }
    }
    #endregion

    #region IEnumerable<T> Members

    public IEnumerator<T> GetEnumerator()
    {
      return _items.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable) _items).GetEnumerator();
    }

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      var eh = CollectionChanged;
      if (eh == null) return;
      using (BlockReentrancy())
        eh(this, e);
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      var eh = PropertyChanged;
      if (eh == null) return;
      using (BlockReentrancy())
        eh(this, e);
    }

    #endregion
  }

  internal class SimpleMonitor : IDisposable
  {
    private int _busyCount;

    public void Dispose()
    {
      Interlocked.Decrement(ref _busyCount);
    }

    public void Enter()
    {
      Interlocked.Increment(ref _busyCount);
    }

    // Properties
    public bool IsBusy
    {
      get { return (_busyCount > 0); }
    }
  }
}