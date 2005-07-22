using System;
using System.Collections;
using System.Windows.Forms;

namespace MediaPortal.Layouts
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

