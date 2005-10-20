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
			
		void IAddChild.AddChild(object value)
		{
			List.Add((Trigger)value);
		}

		void IAddChild.AddText(string text)
		{
			throw new NotImplementedException();
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
