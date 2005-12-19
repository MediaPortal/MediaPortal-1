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
using System.Windows.Forms;

namespace MediaPortal.Drawing.Layouts
{
	internal sealed class ILayoutComponentCollection : CollectionBase
	{
		#region Methods

		public void Add(ILayoutComponent component)
		{
			if(component == null)
				throw new ArgumentNullException("component");

			List.Add(component);
		}
		
		public bool Contains(ILayoutComponent component)
		{
			if(component == null)
				throw new ArgumentNullException("component");

			return List.Contains(component);
		}

		public void CopyTo(ILayoutComponent[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public int IndexOf(ILayoutComponent component)
		{
			if(component == null)
				throw new ArgumentNullException("component");

			return List.IndexOf(component);
		}

		public void Insert(int index, ILayoutComponent component)
		{
			if(component == null)
				throw new ArgumentNullException("component");

			List.Insert(index, component);
		}

		public bool Remove(ILayoutComponent component)
		{
			if(component == null)
				throw new ArgumentNullException("component");

			if(List.Contains(component) == false)
				return false;

			List.Remove(component);

			return true;
		}

		#endregion Methods

		#region Properties

		public ILayoutComponent this[int index]
		{ 
			get { return (ILayoutComponent)List[index]; }
			set { List[index] = value; }
		}

		#endregion Properties
	}
}

