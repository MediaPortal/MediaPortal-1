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
			
		void IAddChild.AddChild(object value)
		{
			List.Add((Setter)value);
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
