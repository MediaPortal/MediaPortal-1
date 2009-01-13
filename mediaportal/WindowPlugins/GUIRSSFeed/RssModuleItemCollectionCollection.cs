#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

/* RssModuleItemCollectionCollection.cs
 * ====================================
 * 
 * RSS.NET (http://rss-net.sf.net/)
 * Copyright © 2002, 2003 George Tsiokos. All Rights Reserved.
 * 
 * RSS 2.0 (http://blogs.law.harvard.edu/tech/rss)
 * RSS 2.0 is offered by the Berkman Center for Internet & Society at 
 * Harvard Law School under the terms of the Attribution/Share Alike 
 * Creative Commons license.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining 
 * a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
*/
using System;
using System.Collections;

namespace Rss
{
  /// <summary>A strongly typed collection of <see cref="RssModuleItemCollection"/> objects</summary>
  public class RssModuleItemCollectionCollection : CollectionBase
  {
    /// <summary>Gets or sets the item at a specified index.<para>In C#, this property is the indexer for the class.</para></summary>
    /// <param name="index">The index of the collection to access.</param>
    /// <value>An item at each valid index.</value>
    /// <remarks>This method is an indexer that can be used to access the collection.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">index is not a valid index.</exception>
    public RssModuleItemCollection this[int index]
    {
      get { return ((RssModuleItemCollection) (List[index])); }
      set { List[index] = value; }
    }

    /// <summary>Adds a specified item to this collection.</summary>
    /// <param name="rssModuleItemCollection">The item to add.</param>
    /// <returns>The zero-based index of the added item.</returns>
    public int Add(RssModuleItemCollection rssModuleItemCollection)
    {
      return List.Add(rssModuleItemCollection);
    }

    /// <summary>Determines whether the RssModuleItemCollectionCollection contains a specific element.</summary>
    /// <param name="rssModuleItemCollection">The RssModuleItemCollection to locate in the RssModuleItemCollectionCollection.</param>
    /// <returns>true if the RssModuleItemCollectionCollection contains the specified value; otherwise, false.</returns>
    public bool Contains(RssModuleItemCollection rssModuleItemCollection)
    {
      return List.Contains(rssModuleItemCollection);
    }

    /// <summary>Copies the entire RssModuleItemCollectionCollection to a compatible one-dimensional <see cref="Array"/>, starting at the specified index of the target array.</summary>
    /// <param name="array">The one-dimensional RssModuleItemCollection Array that is the destination of the elements copied from RssModuleItemCollectionCollection. The Array must have zero-based indexing.</param>
    /// <param name="index">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">array is a null reference (Nothing in Visual Basic).</exception>
    /// <exception cref="ArgumentOutOfRangeException">index is less than zero.</exception>
    /// <exception cref="ArgumentException">array is multidimensional. -or- index is equal to or greater than the length of array.-or-The number of elements in the source RssModuleItemCollectionCollection is greater than the available space from index to the end of the destination array.</exception>
    public void CopyTo(RssModuleItemCollection[] array, int index)
    {
      List.CopyTo(array, index);
    }

    /// <summary>Searches for the specified RssModuleItemCollection and returns the zero-based index of the first occurrence within the entire RssModuleItemCollectionCollection.</summary>
    /// <param name="rssModuleItemCollection">The RssModuleItemCollection to locate in the RssModuleItemCollectionCollection.</param>
    /// <returns>The zero-based index of the first occurrence of RssModuleItemCollection within the entire RssModuleItemCollectionCollection, if found; otherwise, -1.</returns>
    public int IndexOf(RssModuleItemCollection rssModuleItemCollection)
    {
      return List.IndexOf(rssModuleItemCollection);
    }

    /// <summary>Inserts an item into this collection at a specified index.</summary>
    /// <param name="index">The zero-based index of the collection at which to insert the item.</param>
    /// <param name="rssModuleItemCollection">The item to insert into this collection.</param>
    public void Insert(int index, RssModuleItemCollection rssModuleItemCollection)
    {
      List.Insert(index, rssModuleItemCollection);
    }

    /// <summary>Removes a specified item from this collection.</summary>
    /// <param name="rssModuleItemCollection">The item to remove.</param>
    public void Remove(RssModuleItemCollection rssModuleItemCollection)
    {
      List.Remove(rssModuleItemCollection);
    }
  }
}