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

using MediaPortal.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Drawing.Paths
{
	public sealed class PathFigureCollection : CollectionBase
	{
		#region Methods

		public void Add(PathFigure figure)
		{
			if(figure == null)
				throw new ArgumentNullException("figure");

			List.Add(figure);
		}

		public bool Contains(PathFigure figure)
		{
			if(figure == null)
				throw new ArgumentNullException("figure");

			return List.Contains(figure);
		}

		public void CopyTo(PathFigure[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public int IndexOf(PathFigure figure)
		{
			if(figure == null)
				throw new ArgumentNullException("figure");

			return List.IndexOf(figure);
		}

		public void Insert(int index, PathFigure figure)
		{
			if(figure == null)
				throw new ArgumentNullException("figure");

			List.Insert(index, figure);
		}

		public bool Remove(PathFigure figure)
		{
			if(figure == null)
				throw new ArgumentNullException("figure");
			
			if(List.Contains(figure) == false)
				return false;

			List.Remove(figure);

			return true;
		}

		#endregion Methods

		#region Properties

		public PathFigure this[int index]
		{ 
			get { return List[index] as PathFigure; }
			set { List[index] = value; }
		}

		#endregion Properties
	}
}
