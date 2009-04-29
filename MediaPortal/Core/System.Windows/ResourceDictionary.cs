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

using System.Collections;
using System.ComponentModel;
using System.Windows.Serialization;

namespace System.Windows
{
  public class ResourceDictionary : IDictionary, ICollection, IEnumerable, INameScope, ISupportInitialize
  {
    #region Constructors

    public ResourceDictionary()
    {
    }

    #endregion Constructors

    #region Methods

    public void Add(object key, object value)
    {
      _resources[key] = value;
    }

    public void BeginInit()
    {
      _beginInitCount++;
    }

    public void Clear()
    {
      _resources.Clear();
    }

    public bool Contains(object key)
    {
      return _resources.Contains(key);
    }

    public void CopyTo(Array array, int index)
    {
      _resources.CopyTo(array, index);
    }

    public void EndInit()
    {
      if (_source == null)
      {
        throw new ArgumentNullException("UriSource");
      }

      if (_source.IsFile == false)
      {
        throw new InvalidOperationException("");
      }

      XamlParser.LoadXml(_source.ToString());
    }

    public object FindName(string name)
    {
      return _resources[name];
    }

    public IDictionaryEnumerator GetEnumerator()
    {
      return _resources.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _resources.GetEnumerator();
    }

    public void RegisterName(string name, object ownerContext)
    {
      _resources[name] = ownerContext;
    }

    public void Remove(object key)
    {
      _resources.Remove(key);
    }

    public void UnregisterName(string name)
    {
      _resources.Remove(name);
    }

    #endregion Methods

    #region Properties

    public int Count
    {
      get { return _resources.Count; }
    }

    public bool IsFixedSize
    {
      get { return false; }
    }

    public bool IsReadOnly
    {
      get { return _resources.IsReadOnly; }
    }

    public bool IsSynchronized
    {
      get { return _resources.IsSynchronized; }
    }

    public object this[object key]
    {
      get { return _resources[key]; }
      set { _resources[key] = value; }
    }

    public ICollection Keys
    {
      get { return _resources.Keys; }
    }

    public ICollection Values
    {
      get { return _resources.Values; }
    }

    public ResourceDictionaryCollection MergedDictionaries
    {
      get { return _mergedDictionaries; }
    }

    public object SyncRoot
    {
      get { return _resources.SyncRoot; }
    }

    public Uri Source
    {
      get { return _source; }
      set { _source = value; }
    }

    #endregion Properties

    #region Fields

    private int _beginInitCount;
    private ResourceDictionaryCollection _mergedDictionaries = new ResourceDictionaryCollection();
    private Hashtable _resources = new Hashtable(20);
    private Uri _source;

    #endregion Fields
  }
}