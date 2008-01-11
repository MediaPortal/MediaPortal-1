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

using System;
using System.Collections;
using System.Windows.Serialization;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Drawing.Transforms
{
	public sealed class TransformCollection : CollectionBase, IAddChild
	{
		#region Constructors

		public TransformCollection()
		{
		}

		#endregion Constructors

		#region Methods

		public void Add(Transform transform)
		{
			if(transform == null)
				throw new ArgumentNullException("transform");

			List.Add(transform);
		}

		public bool Contains(Transform transform)
		{
			if(transform == null)
				throw new ArgumentNullException("transform");

			return List.Contains(transform);
		}

		public void CopyTo(Transform[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public TransformCollection GetCurrentValue()
		{
			return null;
		}
			
		void IAddChild.AddChild(object child)
		{
			Add((Transform)child);
		}

		void IAddChild.AddText(string text)
		{
		}

		public int IndexOf(Transform transform)
		{
			if(transform == null)
				throw new ArgumentNullException("transform");

			return List.IndexOf(transform);
		}

		public void Insert(int index, Transform transform)
		{
			if(transform == null)
				throw new ArgumentNullException("transform");

			List.Insert(index, transform);
		}

		public bool Remove(Transform transform)
		{
			if(transform == null)
				throw new ArgumentNullException("transform");
			
			if(List.Contains(transform) == false)
				return false;

			List.Remove(transform);

			return true;
		}

		#endregion Methods

		#region Properties

		public Matrix Matrix
		{
			get { Matrix matrix = Matrix.Identity; /*foreach(Transform t in List) matrix.Multiply(t.Matrix);*/ return matrix; }
		}

		public Transform this[int index]
		{ 
			get { return (Transform)List[index]; }
			set { List[index] = value; }
		}

		#endregion Properties
	}
}
