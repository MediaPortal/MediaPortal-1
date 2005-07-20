using System;
using System.Collections;

namespace MediaPortal.GUI.Library
{
	public sealed class GUIControlCollection : CollectionBase
	{
		#region Methods

		public void Add(GUIControl control)
		{
			if(control == null)
				throw new ArgumentNullException("control");

			List.Add(control);
		}

		public bool Contains(GUIControl control)
		{
			if(control == null)
				throw new ArgumentNullException("control");

			return List.Contains(control);
		}

		public void CopyTo(GUIControl[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			List.CopyTo(array, arrayIndex);
		}

		public int IndexOf(GUIControl control)
		{
			if(control == null)
				throw new ArgumentNullException("control");

			return List.IndexOf(control);
		}

		public void Insert(int index, GUIControl control)
		{
			if(control == null)
				throw new ArgumentNullException("control");

			List.Insert(index, control);
		}

		public bool Remove(GUIControl control)
		{
			if(control == null)
				throw new ArgumentNullException("control");

			if(List.Contains(control) == false)
				return false;

			List.Remove(control);

			return true;
		}

		#endregion Methods

		#region Properties

		public GUIControl this[int index]
		{ 
			get { return (GUIControl)List[index]; }
			set { List[index] = value; }
		}

		#endregion Properties
	}
}

