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

using System.Collections;

namespace System.Windows
{
  public struct LocalValueEnumerator : IEnumerable, IEnumerator
  {
    #region Constructors

    internal LocalValueEnumerator(Hashtable properties)
    {
      _properties = properties;
      _enumerator = properties.GetEnumerator();
    }

    #endregion Constructors

    #region Methods

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this;
    }

    public bool MoveNext()
    {
      return _enumerator.MoveNext();
    }

    public void Reset()
    {
      _enumerator.Reset();
    }

    #endregion Methods

    #region Properties

    public int Count
    {
      get { return _properties.Count; }
    }

    public LocalValueEntry Current
    {
      get { return new LocalValueEntry((DependencyProperty)_enumerator.Key, _enumerator.Value); }
    }

    object IEnumerator.Current
    {
      get { return this.Current; }
    }

    #endregion Properties

    #region Fields

    private IDictionaryEnumerator _enumerator;
    private Hashtable _properties;

    #endregion Fields
  }
}