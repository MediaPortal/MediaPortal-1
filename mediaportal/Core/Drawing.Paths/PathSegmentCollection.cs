#region Copyright (C) 2005 Team MediaPortal

/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.ComponentModel;
using System.Windows.Serialization;

using MediaPortal.Drawing;

namespace MediaPortal.Drawing.Paths
{
	public sealed class PathSegmentCollection : CollectionBase //, IAddChild
	{
		#region Methods

/*		void IAddChild.AddChild(object child)
		{
			PathSegmentCollection segments = child as PathSegmentCollection;

			if(segments != null)
			{
				for(int index = 0; index < segments.Count; index++)
					Add(segments[index]);

				return;
			}

			Add((PathSegment)child);
		}
*/
/*		void IAddChild.AddText(string text)
		{
		}
*/
		public void Add(PathSegment segment)
		{
			if(segment == null)
				throw new ArgumentNullException("segment");

			List.Add(segment);
		}

		public bool Contains(PathSegment segment)
		{
			if(segment == null)
				throw new ArgumentNullException("segment");

			return List.Contains(segment);
		}

		public void CopyTo(PathSegment[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public int IndexOf(PathSegment segment)
		{
			if(segment == null)
				throw new ArgumentNullException("segment");

			return List.IndexOf(segment);
		}

		public void Insert(int index, PathSegment segment)
		{
			if(segment == null)
				throw new ArgumentNullException("segment");

			List.Insert(index, segment);
		}

		public bool Remove(PathSegment segment)
		{
			if(segment == null)
				throw new ArgumentNullException("segment");
			
			if(List.Contains(segment) == false)
				return false;

			List.Remove(segment);

			return true;
		}

		#endregion Methods

		#region Properties

		public PathSegment this[int index]
		{ 
			get { return List[index] as PathSegment; }
			set { List[index] = value; }
		}

		#endregion Properties
	}
}
