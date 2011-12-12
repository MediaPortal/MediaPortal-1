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
  /// Defines a <see cref="TstDictionary"/> tree traverser object.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class can be used to traverse the ternary tree. It defines two events:
  /// TreeEntry and DiscoverEntry.
  /// </para>
  /// </remarks>
  public class TstTraverser
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public TstTraverser() {}

    /// <summary>
    /// Tarverses the ternary search tree nodes of <paramref name="dic"/>.
    /// </summary>
    /// <param name="dic">Tree to explore</param>
    /// <exception cref="ArgumentNullException"><paramref name="dic"/> is null.</exception>
    public void Traverse(TstDictionary dic)
    {
      if (dic == null)
        throw new ArgumentNullException("dic");
      Traverse(dic.Root);
    }

    ///<summary>Event called when a node is integrated into the traversed tree.</summary>
    public event TstDictionaryEntryEventHandler TreeEntry;

    /// <summary>
    /// Raises the <see cref="TstTraverser.TreeEntry"/> event.
    /// </summary>
    /// <param name="p">tst node added to the tree</param>
    protected virtual void OnTreeEntry(TstDictionaryEntry p)
    {
      if (TreeEntry != null)
        TreeEntry(this,
                  new TstDictionaryEntryEventArgs(p)
          );
    }

    ///<summary>Event called when a node is discovered but not yet in the explore tree.</summary>
    public event TstDictionaryEntryEventHandler LowChild;

    /// <summary>
    /// Raises the <see cref="TstTraverser.HighChild"/> event.
    /// </summary>
    /// <param name="p">high child about to be added to the tree</param>	
    protected virtual void OnLowChild(TstDictionaryEntry p)
    {
      if (LowChild != null)
        LowChild(this,
                 new TstDictionaryEntryEventArgs(p)
          );
    }

    ///<summary>Event called when a node is discovered but not yet in the explore tree.</summary>
    public event TstDictionaryEntryEventHandler EqChild;

    /// <summary>
    /// Raises the <see cref="TstTraverser.HighChild"/> event.
    /// </summary>
    /// <param name="p">high child about to be added to the tree</param>	
    protected virtual void OnEqChild(TstDictionaryEntry p)
    {
      if (EqChild != null)
        EqChild(this,
                new TstDictionaryEntryEventArgs(p)
          );
    }

    ///<summary>Event called when a node is discovered but not yet in the explore tree.</summary>
    public event TstDictionaryEntryEventHandler HighChild;

    /// <summary>
    /// Raises the <see cref="TstTraverser.HighChild"/> event.
    /// </summary>
    /// <param name="p">high child about to be added to the tree</param>	
    protected virtual void OnHighChild(TstDictionaryEntry p)
    {
      if (HighChild != null)
        HighChild(this,
                  new TstDictionaryEntryEventArgs(p)
          );
    }

    /// <summary>
    /// Traverses the <paramref name="p"/> sub-tree.
    /// </summary>
    /// <param name="p">node to traverse.</param>
    protected void Traverse(TstDictionaryEntry p)
    {
      if (p == null)
        return;

      OnTreeEntry(p);

      OnLowChild(p.LowChild);
      Traverse(p.LowChild);
      OnEqChild(p.EqChild);
      Traverse(p.EqChild);
      OnHighChild(p.HighChild);
      Traverse(p.HighChild);
    }
  }
}