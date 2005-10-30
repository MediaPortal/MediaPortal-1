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

		public NullableTimeSpan(long ticks)
		{
			_hasValue = true;
			_value = TimeSpan.FromTicks(ticks);
		}

		public NullableTimeSpan(TimeSpan ts)
		{
			_hasValue = true;
			_value = ts;
		}

		public NullableTimeSpan(int hours, int minutes, int seconds)
		{
			_hasValue = true;
			_value = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromMinutes(minutes);
		}

		public NullableTimeSpan(int days, int hours, int minutes, int seconds)
		{
			_hasValue = true;
			_value = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
		}
 
		public NullableTimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
		{
			_hasValue = true;
			_value = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds) + TimeSpan.FromMilliseconds(milliseconds);
		}

		#endregion Constructors

		#region Methods

		public NullableTimeSpan Add(NullableTimeSpan timespan)
		{
			if(_hasValue == false || timespan._hasValue == false)
				return Null;

			return new NullableTimeSpan(_value.Add(timespan._value));
		}
 
		public static int Compare(NullableTimeSpan l, NullableTimeSpan r)
		{
			return l.CompareTo(r);
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

		public NullableTimeSpan Duration()
		{
			if(_hasValue == false)
				return Null;

			return new NullableTimeSpan(_value.Duration());
		}

		public override bool Equals(object other) 
		{
			if(other is NullableTimeSpan == false)
				return false;

			NullableTimeSpan nullable = (NullableTimeSpan)other;

			return _hasValue == nullable._hasValue && _value.Equals(nullable._value);
		}

		public static bool Equals(TimeSpan l, TimeSpan r)
		{
			return l == r;
		}

		public static NullableTimeSpan FromDays(double days)
		{
			return new NullableTimeSpan(TimeSpan.FromDays(days));
		} 

		public static NullableTimeSpan FromHours(double hours)
		{
			return new NullableTimeSpan(TimeSpan.FromHours(hours));
		} 

		public static NullableTimeSpan FromMilliseconds(double milliseconds)
		{
			return new NullableTimeSpan(TimeSpan.FromMilliseconds(milliseconds));
		}

		public static NullableTimeSpan FromMinutes(double minutes)
		{
			return new NullableTimeSpan(TimeSpan.FromMinutes(minutes));
		}

		public static NullableTimeSpan FromSeconds(double seconds)
		{
			return new NullableTimeSpan(TimeSpan.FromMilliseconds(seconds));
		}

		public static NullableTimeSpan FromTicks(double ticks)
		{
			return new NullableTimeSpan(TimeSpan.FromMilliseconds(ticks));
		}

		public override int GetHashCode() 
		{
			return _value.GetHashCode();
		}

		public NullableTimeSpan Negate()
		{
			if(_hasValue == false)
				return Null;

			return new NullableTimeSpan(_value.Negate());
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
			
			return l.Add(r); 
		}

		public static NullableTimeSpan operator - (NullableTimeSpan l, NullableTimeSpan r) 
		{
			return l.Subtract(r);
		}

		public static NullableTimeSpan operator - (NullableTimeSpan l) 
		{
			return l.Negate();
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

		public NullableTimeSpan Subtract(NullableTimeSpan timespan)
		{
			if(_hasValue == false || timespan._hasValue == false)
				return Null;
			
			return new NullableTimeSpan(_value.Subtract(timespan._value)); 
		}

		#endregion Operators

		#region Properties

		public int Days
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.Days; }
		}

		public int Hours
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.Hours; }
		}

		public int Milliseconds
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.Milliseconds; }
		}

		public int Minutes
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.Minutes; }
		}

		public int Seconds
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.Seconds; }
		}

		public double TotalDays
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.TotalDays; }
		}

		public double TotalHours
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.TotalHours; }
		}

		public double TotalMilliseconds
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.TotalMilliseconds; }
		}

		public double TotalMinutes
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.TotalMinutes; }
		}

		public double TotalSeconds
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.TotalSeconds; }
		}

		public long Ticks
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value.Ticks; }
		}

		public bool HasValue
		{
			get { return _hasValue; }
		}

		public TimeSpan Value
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value; }
			set { _value = value; }
		}

		object INullable.Value
		{
			get { if(!_hasValue) throw new NullReferenceException(); return _value; }
			set { _value = (TimeSpan)value; }
		}

		#endregion Properties

		#region Fields

		bool						_hasValue;
		TimeSpan					_value;

		public static readonly NullableTimeSpan Null = new NullableTimeSpan();
		public static readonly NullableTimeSpan MaxValue = new NullableTimeSpan(TimeSpan.MaxValue);
		public static readonly NullableTimeSpan MinValue = new NullableTimeSpan(TimeSpan.MaxValue);
		public static readonly NullableTimeSpan Zero = new NullableTimeSpan(0);
 
		public const long TicksPerDay = TimeSpan.TicksPerDay;
		public const long TicksPerHour = TimeSpan.TicksPerHour;
		public const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
		public const long TicksPerMinute = TimeSpan.TicksPerMinute;
		public const long TicksPerSecond = TimeSpan.TicksPerSecond;

		#endregion Fields
	}
}
