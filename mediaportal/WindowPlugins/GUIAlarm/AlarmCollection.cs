using System;
using System.Collections;

namespace MediaPortal.GUI.Alarm
{
	/// <summary>
	/// Summary description for AlarmCollection.
	/// </summary>
	public class AlarmCollection : CollectionBase
	{
		public enum AlarmField
		{
			Id,
			Name,
			Enabled,
			Time
		}

		public void Sort(AlarmField sortField, bool isAscending) 
		{
			switch (sortField) 
			{
				case AlarmField.Id:
					InnerList.Sort(new IdComparer());
					break;
				case  AlarmField.Name:
					InnerList.Sort(new NameComparer());
					break;
				case  AlarmField.Time:
					InnerList.Sort(new AlarmTimeComparer());
					break;
			}
			if (!isAscending) InnerList.Reverse();
		}

		
		private sealed class IdComparer : IComparer 
		{
			public int Compare(object x, object y) 
			{
				Alarm first = (Alarm) x;
				Alarm second = (Alarm) y;
				return first.Id - second.Id;
			}
		}

		private sealed class AlarmTimeComparer : IComparer 
		{
			public int Compare(object x, object y) 
			{
				Alarm first = (Alarm) x;
				Alarm second = (Alarm) y;
				return first.Time.TimeOfDay.CompareTo(second.Time.TimeOfDay);
			}
		}
		private sealed class NameComparer : IComparer 
		{
			public int Compare(object x, object y) 
			{
				Alarm first = (Alarm) x;
				Alarm second = (Alarm) y;
				return first.Name.CompareTo(second.Name);
			}
		}

		// Provide the strongly typed member for ICollection.
		public void CopyTo(Alarm[] array, int index)
		{
			((ICollection)this).CopyTo(array, index);
		}

		public Alarm this[ int index ] 
		{
			get  { return( (Alarm) List[index] );}
			set  { List[index] = value;}
		}

		public int Add( Alarm value ) 
		{
			return( List.Add( value ) );
		}

		public int IndexOf(Alarm value ) 
		{
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, Alarm value ) 
		{
			List.Insert( index, value );
		}

		public void Remove( Alarm value ) 
		{
			List.Remove( value );
		}

		public bool Contains( Alarm value ) 
		{
			// If value is not of type Alarm, this will return false.
			return( List.Contains( value ) );
		}

		protected override void OnInsert( int index, Object value ) 
		{
			if ( value.GetType() != System.Type.GetType("MediaPortal.GUI.Alarm.Alarm") )
				throw new ArgumentException( "value must be of type Alarm.", "value" );
		}

		protected override void OnRemove( int index, Object value ) 
		{
			if ( value.GetType() != System.Type.GetType("MediaPortal.GUI.Alarm.Alarm") )
				throw new ArgumentException( "value must be of type Alarm.", "value" );
		}

		protected override void OnSet( int index, Object oldValue, Object newValue ) 
		{
			if ( newValue.GetType() != System.Type.GetType("MediaPortal.GUI.Alarm.Alarm") )
				throw new ArgumentException( "newValue must be of type Alarm.", "newValue" );
		}

		protected override void OnValidate( Object value ) 
		{
			if ( value.GetType() != System.Type.GetType("MediaPortal.GUI.Alarm.Alarm") )
				throw new ArgumentException( "value must be of type Alarm." );
		}

	}
}
