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
using System.Windows.Serialization;

namespace System.Windows
{
	public sealed class SetterCollection : CollectionBase, IAddChild
	{
		#region Constructors

		public SetterCollection()
		{
		}

		#endregion Constructors

		#region Methods

		public void Add(Setter setter)
		{
			if(setter == null)
				throw new ArgumentNullException("setter");

			List.Add(setter);
		}

		public bool Contains(Setter setter)
		{
			if(setter == null)
				throw new ArgumentNullException("setter");

			return List.Contains(setter);
		}

		public void CopyTo(Setter[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public SetterCollection GetCurrentValue()
		{
			throw new NotImplementedException();
		}
			
		void IAddChild.AddChild(object child)
		{
			Setter setter = child as Setter;

			if(setter == null)
				throw new ArgumentException("child");

			List.Add(setter);
		}

		void IAddChild.AddText(string text)
		{
			throw new NotImplementedException();
		}

		public int IndexOf(Setter setter)
		{
			if(setter == null)
				throw new ArgumentNullException("setter");

			return List.IndexOf(setter);
		}

		public void Insert(int index, Setter setter)
		{
			if(setter == null)
				throw new ArgumentNullException("setter");

			List.Insert(index, setter);
		}

		public bool Remove(Setter setter)
		{
			if(setter == null)
				throw new ArgumentNullException("setter");
			
			if(List.Contains(setter) == false)
				return false;

			List.Remove(setter);

			return true;
		}

		#endregion Methods

		#region Properties

		public Setter this[int index]
		{ 
			get { return (Setter)List[index]; }
			set { List[index] = value; }
		}

		#endregion Properties
	}
}
