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

namespace Tst
{
  /// <summary>
  /// Defines a Ternary Search Tree node pair that can be set or retrieved.
  /// </summary>
  public class TstDictionaryEntry : ICloneable
  {
    private char splitChar;
    private bool isKey;
    private string key;
    private Object value;
    private TstDictionaryEntry parent;
    private TstDictionaryEntry lowChild;
    private TstDictionaryEntry eqChild;
    private TstDictionaryEntry highChild;

    /// <summary>
    /// Construct a tst node.
    /// </summary>
    /// <param name="parent">parent node</param>
    /// <param name="splitChar">split character</param>
    public TstDictionaryEntry(
      TstDictionaryEntry parent,
      char splitChar
      )
    {
      this.isKey = false;
      this.key = null;
      this.value = null;
      this.parent = parent;
      this.splitChar = splitChar;
      this.lowChild = null;
      this.eqChild = null;
      this.highChild = null;
    }

    /// <summary>
    /// Gets the parent of the node.
    /// </summary>	
    /// <value>
    /// The parent node.
    /// </value>	
    public TstDictionaryEntry Parent
    {
      get { return parent; }
    }

    /// <summary>
    /// Gets the split character.
    /// </summary>	
    /// <value>
    /// The split character.
    /// </value>	
    public char SplitChar
    {
      get { return splitChar; }
    }

    /// <summary>
    /// Gets a value indicating wheter the node is a key.
    /// </summary>	
    /// <value>
    /// true is the node is a key, false otherwize.
    /// </value>	
    public bool IsKey
    {
      get { return isKey; }
      set { isKey = value; }
    }

    /// <summary>
    /// Gets the node key, if it is a key.
    /// </summary>
    /// <value>
    /// The node key.
    /// </value>
    /// <exception cref="InvalidOperationException">The node does not hold a key-value pair.</exception>
    public String Key
    {
      get
      {
        if (!IsKey)
          throw new InvalidOperationException("node is not a key");
        return key;
      }
      set { key = value; }
    }

    /// <summary>
    /// Gets the node value.
    /// </summary>	
    /// <value>
    /// The node value.
    /// </value>
    /// <exception cref="InvalidOperationException">The node does not hold a key-value pair.</exception>
    public Object Value
    {
      get
      {
        if (!IsKey)
          throw new InvalidOperationException("node is not a key");
        return value;
      }
      set { this.value = value; }
    }

    /// <summary>
    /// Gets the node low child.
    /// </summary>	
    /// <value>
    /// The low child.
    /// </value>	
    public TstDictionaryEntry LowChild
    {
      get { return lowChild; }
      set { lowChild = value; }
    }

    /// <summary>
    /// Gets the node ep child.
    /// </summary>	
    /// <value>
    /// The eq child.
    /// </value>	
    public TstDictionaryEntry EqChild
    {
      get { return eqChild; }
      set { eqChild = value; }
    }

    /// <summary>
    /// Gets the node high child.
    /// </summary>	
    /// <value>
    /// The high child.
    /// </value>	
    public TstDictionaryEntry HighChild
    {
      get { return highChild; }
      set { highChild = value; }
    }

    /// <summary>
    /// Gets a value indicating wheter the node has children.
    /// </summary>	
    /// <value>
    /// true if the node has children, false otherwize.
    /// </value>	
    public bool HasChildren
    {
      get { return LowChild != null || EqChild != null || HighChild != null; }
    }

    /// <summary>
    /// Gets a value indicating wheter the node is a low child.
    /// </summary>	
    /// <value>
    /// true if the node is a low child, false otherwize.
    /// </value>
    public bool IsLowChild
    {
      get { return Parent != null && Parent.LowChild == this; }
    }

    /// <summary>
    /// Gets a value indicating wheter the node is a high child.
    /// </summary>	
    /// <value>
    /// true if the node is a high child, false otherwize.
    /// </value>
    public bool IsHighChild
    {
      get { return Parent != null && Parent.HighChild == this; }
    }

    /// <summary>
    /// Gets a value indicating wheter the node is a eq child.
    /// </summary>	
    /// <value>
    /// true if the node is a eq child, false otherwize.
    /// </value>
    public bool IsEqChild
    {
      get { return Parent != null && Parent.EqChild == this; }
    }

    #region ICloneable Members

    /// <summary>
    /// Creates a shallow copy of the entry
    /// </summary>
    /// <returns>entry shallow copy</returns>
    public Object Clone()
    {
      TstDictionaryEntry entry = new TstDictionaryEntry(Parent, SplitChar);
      if (LowChild != null)
        entry.LowChild = LowChild.Clone() as TstDictionaryEntry;
      if (EqChild != null)
        entry.EqChild = EqChild.Clone() as TstDictionaryEntry;
      if (HighChild != null)
        entry.HighChild = HighChild.Clone() as TstDictionaryEntry;
      return entry;
    }

    public override string ToString()
    {
      char c;
      if (this.IsEqChild)
        c = 'E';
      else if (this.IsLowChild)
        c = 'L';
      else if (this.IsHighChild)
        c = 'H';
      else
        c = 'R';

      if (this.IsKey)
        return String.Format("{0} {1} {2}", c, this.SplitChar, this.Key);
      else
        return String.Format("{0} {1}", c, this.SplitChar);
    }

    #endregion
  }
}