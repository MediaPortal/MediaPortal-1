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

namespace Tst
{
  /// <summary>
  /// Enumerates the elements of a <see cref="TstDictionary"/>.
  /// </summary>
  public sealed class TstDictionaryEnumerator : IDictionaryEnumerator
  {
    private long version;
    private Stack stack;
    private TstDictionaryEntry currentNode;
    private TstDictionary dictionary;

    /// <summary>Constructs an enumerator over <paramref name="tst"/></summary>
    /// <param name="tst">dictionary to enumerate.</param>
    /// <exception cref="ArgumentNullException">tst is null</exception>
    public TstDictionaryEnumerator(TstDictionary tst)
    {
      if (tst == null)
        throw new ArgumentNullException("tst");
      this.version = tst.Version;
      this.dictionary = tst;
      this.currentNode = null;
      this.stack = null;
    }

    /// <summary>
    /// Sets the enumerator to its initial position, which is before the first element in the collection.
    /// </summary>
    public void Reset()
    {
      this.ThrowIfChanged();
      this.stack.Clear();
      stack = null;
    }


    /// <summary>
    /// Gets the current element in the collection.
    /// </summary>
    /// <value>The current element in the collection.</value>
    public DictionaryEntry Current
    {
      get
      {
        this.ThrowIfChanged();
        return this.Entry;
      }
    }

    /// <summary>
    /// Gets the current element in the collection.
    /// </summary>
    /// <value>The current element in the collection.</value>	
    Object IEnumerator.Current
    {
      get { return this.Current; }
    }

    /// <summary>
    /// Gets both the key and the value of the current dictionary entry.
    /// </summary>
    /// <value>
    /// A <see cref="DictionaryEntry"/> containing both the key and the value of the current 
    /// dictionary entry.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// The enumerator is positioned before the first entry of the dictionary or after the last entry. 
    /// </exception>
    public DictionaryEntry Entry
    {
      get
      {
        this.ThrowIfChanged();
        if (currentNode == null)
          throw new InvalidOperationException();
        return new DictionaryEntry(currentNode.Key, currentNode.Value);
      }
    }

    /// <summary>
    /// Gets the key of the current dictionary entry.
    /// </summary>
    /// <value>
    /// The key of the current element of the enumeration.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// The enumerator is positioned before the first entry of the dictionary or after the last entry. 
    /// </exception>	
    public String Key
    {
      get
      {
        this.ThrowIfChanged();
        if (currentNode == null)
          throw new InvalidOperationException();
        return currentNode.Key;
      }
    }

    /// <summary>
    /// Gets the key of the current dictionary entry.
    /// </summary>
    /// <value>
    /// The key of the current element of the enumeration.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// The enumerator is positioned before the first entry of the dictionary or after the last entry. 
    /// </exception>		
    Object IDictionaryEnumerator.Key
    {
      get { return this.Key; }
    }

    /// <summary>
    /// Gets the value of the current dictionary entry.
    /// </summary>
    /// <value>
    /// The value of the current element of the enumeration.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// The enumerator is positioned before the first entry of the dictionary or after the last entry. 
    /// </exception>		
    public Object Value
    {
      get
      {
        this.ThrowIfChanged();
        if (currentNode == null)
          throw new InvalidOperationException();
        return currentNode.Value;
      }
    }

    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <returns>
    /// true if the enumerator was successfully advanced to the next element; 
    /// false if the enumerator has passed the end of the collection.
    /// </returns>
    public bool MoveNext()
    {
      this.ThrowIfChanged();

      // we are at the beginning
      if (stack == null)
      {
        stack = new Stack();
        currentNode = null;
        if (dictionary.Root != null)
          stack.Push(dictionary.Root);
      }
        // we are at the end node, finished
      else if (currentNode == null)
        throw new InvalidOperationException("out of range");

      if (stack.Count == 0)
        currentNode = null;

      while (stack.Count > 0)
      {
        currentNode = (TstDictionaryEntry)stack.Pop();
        if (currentNode.HighChild != null)
          stack.Push(currentNode.HighChild);
        if (currentNode.EqChild != null)
          stack.Push(currentNode.EqChild);
        if (currentNode.LowChild != null)
          stack.Push(currentNode.LowChild);

        if (currentNode.IsKey)
          break;
      }

      return currentNode != null;
    }

    internal void ThrowIfChanged()
    {
      if (version != dictionary.Version)
        throw new InvalidOperationException("Collection changed");
    }
  }
}