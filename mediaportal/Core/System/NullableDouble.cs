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
	[TypeConverter(typeof(NullableDoubleConverter))]
	public struct NullableDouble : INullable, IComparable, IFormattable
	{
		#region Constructors

		public NullableDouble(double d)
		{
			_hasValue = true;
			_value = d;
		}

		#endregion Constructors

		#region Properties

		public bool HasValue
		{
			get { return _hasValue; }
		}

		public double Value
		{
			get { if(_hasValue) throw new NullReferenceException(); return _value; }
			set { _value = value; }
		}

		object INullable.Value
		{
			get { if(_hasValue) throw new NullReferenceException(); return _value; }
			set { _value = (double)value; }
		}

		#endregion Properties

		#region Methods

		public int CompareTo(object other)
		{
			if(other == null)
				return 1;

			if(other is NullableDouble == false)
				throw new ArgumentException("");

			NullableDouble nullable = (NullableDouble)other;

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
			if(other is NullableDouble == false)
				return false;

			NullableDouble nullable = (NullableDouble)other;

			return _hasValue == nullable._hasValue && _value.Equals(nullable._value);
		}

		public override int GetHashCode() 
		{
			return _value.GetHashCode();
		}

		public static bool IsInfinity(NullableDouble d)
		{
			return d._hasValue && double.IsInfinity(d._value);
		}

		public static bool IsNan(NullableDouble d)
		{
			return d._hasValue && double.IsNaN(d._value);
		}

		public static bool IsNegativeInfinity(NullableDouble d)
		{
			return d._hasValue && double.IsNegativeInfinity(d._value);
		}

		public static bool IsPositiveInfinity(NullableDouble d)
		{
			return d._hasValue && double.IsPositiveInfinity(d._value);
		}

		public static NullableDouble Parse(string s)
		{
			return Parse(s, null);
		}

		public static NullableDouble Parse(string s, IFormatProvider provider)
		{
			return Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, provider);
		}
		
		public static NullableDouble Parse(string s, NumberStyles style)
		{
			return Parse(s, style, null);
		}

		public static NullableDouble Parse(string s, NumberStyles style, IFormatProvider provider)
		{
			if(string.Compare(s, "Null", true) == 0)
				return Null;

			return new NullableDouble(double.Parse(s, style, provider));
		}

		// TODO: Implement IConvertible
/*		TypeCode GetTypeCode()
		bool ToBoolean(IFormatProvider provider);
		byte ToByte(IFormatProvider provider);
		char ToChar(IFormatProvider provider);
		DateTime ToDateTime(IFormatProvider provider);
		decimal ToDecimal(IFormatProvider provider);
		double ToDouble(IFormatProvider provider);
		short ToInt16(IFormatProvider provider);
		int ToInt32(IFormatProvider provider);
		long ToInt64(IFormatProvider provider);
		sbyte ToSByte(IFormatProvider provider);
		float ToSingle(IFormatProvider provider);
		string ToString(IFormatProvider provider);
		object ToType(Type conversionType, IFormatProvider provider);
		ushort ToUInt16(IFormatProvider provider);
		uint ToUInt32(IFormatProvider provider);
		ulong ToUInt64(IFormatProvider provider);
*/
		public override string ToString()
		{
			return ToString("{0}");
		}

		public string ToString(string format)
		{
			return ToString(format, null);
		}

		public string ToString(IFormatProvider provider)
		{
			return ToString("{0}", provider);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			if(_hasValue == false)
				return "Null";

			return _value.ToString(format, provider);
		}

		public static bool TryParse(string s, out NullableDouble result)
		{
			return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, null, out result);
		}

		public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out NullableDouble result)
		{
			result = Null;

			if(string.Compare(s, "Null", true) == 0)
				return true;

			return double.TryParse(s, style, provider, out result._value);
		}

		#endregion Methods

		#region Operators

		public static explicit operator double(NullableDouble nullable) 
		{
			return nullable.Value;
		}

		public static bool operator == (NullableDouble l, NullableDouble r)
		{
			return l._hasValue && r._hasValue && l._value == r._value;
		}

		public static bool operator != (NullableDouble l, NullableDouble r) 
		{
			return l._hasValue && r._hasValue && l._value != r._value;
		}
		
		public static NullableDouble operator + (NullableDouble l, NullableDouble r) 
		{
			if(l._hasValue == false || r._hasValue == false)
				return Null;
			
			return new NullableDouble(l._value + r._value); 
		}

		public static NullableDouble operator - (NullableDouble l, NullableDouble r) 
		{
			if(l._hasValue == false || r._hasValue == false)
				return Null;
			
			return new NullableDouble(l._value - r._value); 
		}

		public static NullableDouble operator - (NullableDouble l) 
		{
			if(l._hasValue)
				return Null;

			return new NullableDouble(-l._value);
		}
		
		public static NullableDouble operator * (NullableDouble l, NullableDouble r) 
		{
			if(l._hasValue == false || r._hasValue == false)
				return Null;

			return new NullableDouble(l._value * r._value);
		}

		public static NullableDouble operator / (NullableDouble l, NullableDouble r) 
		{
			if(l._hasValue == false || r._hasValue == false)
				return Null;

			if(r._value == 0)
				throw new DivideByZeroException();

			return new NullableDouble(l._value / r._value);
		}

		public static bool operator > (NullableDouble l, NullableDouble r) 
		{
			return l._hasValue && r._hasValue && l._value > r._value;
		}

		public static bool operator >= (NullableDouble l, NullableDouble r) 
		{
			return l._hasValue && r._hasValue && l._value >= r._value;
		}

		public static bool operator < (NullableDouble l, NullableDouble r) 
		{
			return l._hasValue && r._hasValue && l._value < r._value;
		}

		public static bool operator <= (NullableDouble l, NullableDouble r) 
		{
			return l._hasValue && r._hasValue && l._value < r._value;
		}

		#endregion Operators

		#region Fields

		bool						_hasValue;
		double						_value;

		public static readonly NullableDouble Epsilon = new NullableDouble(double.Epsilon);
		public static readonly NullableDouble MaxValue = new NullableDouble(double.MaxValue);
		public static readonly NullableDouble MinValue = new NullableDouble(double.MinValue);
		public static readonly NullableDouble Nan = new NullableDouble(double.NaN);
		public static readonly NullableDouble NegativeInfinity = new NullableDouble(double.NegativeInfinity);
		public static readonly NullableDouble Null = new NullableDouble();
		public static readonly NullableDouble PositiveInfinity = new NullableDouble(double.PositiveInfinity);

		#endregion Fields
	}
}
