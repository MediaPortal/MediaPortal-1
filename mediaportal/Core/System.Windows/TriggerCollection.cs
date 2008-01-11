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

namespace System.Windows
{
	public sealed class TriggerCollection : CollectionBase, IAddChild
	{
		#region Constructors

		public TriggerCollection()
		{
		}

		#endregion Constructors

		#region Methods

		public void Add(Trigger trigger)
		{
			if(trigger == null)
				throw new ArgumentNullException("trigger");

			List.Add(trigger);
		}

		public bool Contains(Trigger trigger)
		{
			if(trigger == null)
				throw new ArgumentNullException("trigger");

			return List.Contains(trigger);
		}

		public void CopyTo(Trigger[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public TriggerCollection GetCurrentValue()
		{
			throw new NotImplementedException();
		}
			
		void IAddChild.AddChild(object child)
		{
			if(child == null)
				throw new ArgumentNullException("child");

			if(child is TriggerBase == false)
				throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof(TriggerBase)));

			List.Add((TriggerBase)child);
		}

		void IAddChild.AddText(string text)
		{
		}

		public int IndexOf(Trigger trigger)
		{
			if(trigger == null)
				throw new ArgumentNullException("trigger");

			return List.IndexOf(trigger);
		}

		public void Insert(int index, Trigger trigger)
		{
			if(trigger == null)
				throw new ArgumentNullException("trigger");

			List.Insert(index, trigger);
		}

		public bool Remove(Trigger trigger)
		{
			if(trigger == null)
				throw new ArgumentNullException("trigger");
			
			if(List.Contains(trigger) == false)
				return false;

			List.Remove(trigger);

			return true;
		}

		#endregion Methods

		#region Properties

		public Trigger this[int index]
		{ 
			get { return (Trigger)List[index]; }
			set { List[index] = value; }
		}

		#endregion Properties
	}
}
