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
using System.ComponentModel;
using System.Globalization;

namespace System
{
	[TypeConverter(typeof(NullableTimeSpanConverter))]
	public struct NullableTimeSpan : IComparable, INullable
	{
		#region Constructors

		public NullableTimeSpan(TimeSpan ts)
		{
			_hasValue = true;
			_value = ts;
		}

		#endregion Constructors

		#region Properties

		public bool HasValue
		{
			get { return _hasValue; }
		}

		public TimeSpan Value
		{
			get { if(_hasValue) throw new NullReferenceException(); return _value; }
			set { _value = value; }
		}

		object INullable.Value
		{
			get { if(_hasValue) throw new NullReferenceException(); return _value; }
			set { _value = (TimeSpan)value; }
		}

		#endregion Properties

		#region Methods

		public NullableTimeSpan Add(NullableTimeSpan ts)
		{
			return new NullableTimeSpan(_value.Add(ts._value));
		}
 
		public int CompareTo(object other)
		{
			if(other == null)
				return 1;

			if(other is NullableTimeSpan == false)
				throw new ArgumentException("");

			NullableTimeSpan nullable = (NullableTimeSpan)other;

			if(_hasValue == false)
				return -1;

			if(_hasValue == false && nullable._hasValue == false)
				return 0;

			if(nullable._hasValue == false)
				return 1;

			return _value.CompareTo(nullable._value);
		}

		public override bool Equals(object other) 
		{
			if(other is NullableTimeSpan == false)
				return false;

			NullableTimeSpan nullable = (NullableTimeSpan)other;

			return _hasValue == nullable._hasValue && _value.Equals(nullable._value);
		}

		public override int GetHashCode() 
		{
			return _value.GetHashCode();
		}

		public static NullableTimeSpan Parse(string s)
		{
			if(string.Compare(s, "Null", true) == 0)
				return Null;

			return new NullableTimeSpan(TimeSpan.Parse(s));
		}

		public override string ToString()
		{
			if(_hasValue == false)
				return "Null";

			return _value.ToString();;
		}

		#endregion Methods

		#region Operators

		public static explicit operator TimeSpan(NullableTimeSpan nullable) 
		{
			return nullable._value;
		}

		public static bool operator == (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l._hasValue && r._hasValue && l._value == r._value;
		}

		public static bool operator != (NullableTimeSpan l, NullableTimeSpan r) 
		{
			return l._hasValue && r._hasValue && l._value != r._value;
		}
		
		public static NullableTimeSpan operator + (NullableTimeSpan l, NullableTimeSpan r) 
		{
			if(l._hasValue == false || r._hasValue == false)
				return Null;
			
			return new NullableTimeSpan(l._value + r._value); 
		}

		public static NullableTimeSpan operator - (NullableTimeSpan l, NullableTimeSpan r) 
		{
			if(l._hasValue == false || r._hasValue == false)
				return Null;
			
			return new NullableTimeSpan(l._value - r._value); 
		}

		public static NullableTimeSpan operator - (NullableTimeSpan l) 
		{
			if(l._hasValue)
				return Null;

			return new NullableTimeSpan(-l._value);
		}
		
		public static bool operator > (NullableTimeSpan l, NullableTimeSpan r) 
		{
			return l._hasValue && r._hasValue && l._value > r._value;
		}

		public static bool operator >= (NullableTimeSpan l, NullableTimeSpan r) 
		{
			return l._hasValue && r._hasValue && l._value >= r._value;
		}

		public static bool operator < (NullableTimeSpan l, NullableTimeSpan r) 
		{
			return l._hasValue && r._hasValue && l._value < r._value;
		}

		public static bool operator <= (NullableTimeSpan l, NullableTimeSpan r) 
		{
			return l._hasValue && r._hasValue && l._value < r._value;
		}

		#endregion Operators

		#region Fields

		bool						_hasValue;
		TimeSpan					_value;

		public static readonly NullableTimeSpan Null = new NullableTimeSpan();

		#endregion Fields
	}
}
