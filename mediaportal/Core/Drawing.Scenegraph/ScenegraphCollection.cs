#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

namespace MediaPortal.Drawing.Scenegraph
{
	public sealed class ScenegraphCollection : CollectionBase
	{
		#region Constructors

		public ScenegraphCollection(IScenegraphElement parent)
		{
			_parent = parent;
		}

		#endregion Constructors

		#region Methods

		public void Add(object element)
		{
			if(element == null)
				throw new ArgumentNullException("element");

			List.Add(element);
		}

		public void Add(IScenegraphElement element)
		{
			if(element == null)
				throw new ArgumentNullException("element");

			List.Add(element);
		}

		public bool Contains(IScenegraphElement element)
		{
			if(element == null)
				throw new ArgumentNullException("element");

			return List.Contains(element);
		}

		public void CopyTo(IScenegraphElement[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public int IndexOf(IScenegraphElement element)
		{
			if(element == null)
				throw new ArgumentNullException("element");

			return List.IndexOf(element);
		}

		public void Insert(int index, IScenegraphElement element)
		{
			if(element == null)
				throw new ArgumentNullException("element");

			List.Insert(index, element);
		}

		public bool Remove(IScenegraphElement element)
		{
			if(element == null)
				throw new ArgumentNullException("element");
			
			if(List.Contains(element) == false)
				return false;

			List.Remove(element);

			return true;
		}

		#endregion Methods

		#region Properties

		public object this[int index]
		{ 
			get { return (object)(List[index] as IScenegraphElement); }
			set { List[index] = value; }
		}

		#endregion Properties

		#region Fields

		IScenegraphElement			_parent;

		#endregion Fields
	}
}
