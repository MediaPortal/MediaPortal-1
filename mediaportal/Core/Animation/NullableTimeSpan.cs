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

namespace MediaPortal.Animation
{
	[TypeConverter(typeof(TimeSpanConverter))]
	public struct NullableTimeSpan : IComparable
	{
		#region Constructors

		public NullableTimeSpan(long ticks)
		{
			_value = new TimeSpan(ticks);
			_hasValue = true;
		}
 
		public NullableTimeSpan(int hours, int minutes, int seconds)
		{
			_value = new TimeSpan(hours, minutes, seconds);
			_hasValue = true;
		}
 
		public NullableTimeSpan(int days, int hours, int minutes, int seconds)
		{
			_value = new TimeSpan(days, hours, minutes, seconds);
			_hasValue = true;
		}

		public NullableTimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
		{
			_value = new TimeSpan(days, hours, minutes, seconds, milliseconds);
			_hasValue = true;
		}
 
		public NullableTimeSpan(TimeSpan value)
		{
			_value = value;
			_hasValue = true;
		}

		#endregion Constructors

		#region Methods

		public NullableTimeSpan Add(TimeSpan ts)
		{
			return new NullableTimeSpan(_value.Add(ts._value));
		}

		public NullableTimeSpan Duration()
		{
			return new NullableTimeSpan(_value.Duration());
		}

		public static int Compare(NullableTimeSpan t1, NullableTimeSpan t2)
		{
			return NullableTimeSpan.Compare(t1._value, t2._value);
		}

		public int CompareTo(object other)
		{
			if(other is TimeSpan)
				return _value.CompareTo((TimeSpan)other);

			if(other is NullableTimeSpan)
				return _value.CompareTo(((NullableTimeSpan)other)._value);

			return 1;
		}

		public override bool Equals(object other)
		{
			return CompareTo(other) == 0;
		}

		public static NullableTimeSpan FromDays(double days)
		{
			return new NullableTimeSpan(TimeSpan.FromDays(days));
		}

		public static NullableTimeSpan FromHours(double hours)
		{
			return new NullableTimeSpan(TimeSpan.FromHours(hours));
		}
 
		public static TimeSpan FromMilliseconds(double milliseconds)
		{
			return new TimeSpan(TimeSpan.FromMilliseconds(milliseconds));
		}

		public static TimeSpan FromMinutes(double minutes)
		{
			return new TimeSpan(TimeSpan.FromMinutes(minutes));
		}

		public static TimeSpan FromSeconds(double seconds)
		{
			return new TimeSpan(TimeSpan.FromSeconds(seconds));
		}
 
		public static TimeSpan FromTicks(long ticks)
		{
			return new TimeSpan(TimeSpan.FromTicks(ticks));
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public TimeSpan Negate()
		{
			return new TimeSpan(_value.Negate());
		}

		public static TimeSpan Parse(string text)
		{
			return new TimeSpan(TimeSpan.Parse(text));
		}
 
		public TimeSpan Subtract(TimeSpan ts)
		{
			return new TimeSpan(_value.Subtract(ts));
		}
 
		public override string ToString()
		{
			return _value.ToString();
		}

		#endregion Methods

		#region Operators

		public static NullableTimeSpan operator + (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l.Add(r);
		}

		public static bool operator == (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l._value == r._value;
		}

		public static bool operator > (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l._value > r._value;
		}

		public static bool operator >= (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l._value >= r._value;
		}
 
		public static bool operator != (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l._value != r._value;
		}
 
		public static bool operator < (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l._value < r._value;
		}

		public static bool operator <= (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l._value <= r._value;
		}
 
		public static NullableTimeSpan operator - (NullableTimeSpan l, NullableTimeSpan r)
		{
			return l.Subtract(r._value);
		}
 
		public static NullableTimeSpan operator - (NullableTimeSpan t)
		{
			return new NullableTimeSpan(-t._value);
		}
 
		public static NullableTimeSpan operator + (NullableTimeSpan t)
		{
			return new NullableTimeSpan(t._value);
		}
 
		#endregion Operators

		#region Properties

		public int Days
		{
			get { return _value.Days; }
		}

		public int Hours
		{
			get { return _value.Hours; }
		}

		public int Milliseconds
		{
			get { return _value.Milliseconds; }
		}

		public int Minutes
		{
			get { return _value.Minutes; }
		}

		public int Seconds
		{
			get { return _value.Seconds; }
		}

		public long Ticks
		{
			get { return _value.Ticks; }
		}

		public double TotalDays
		{
			get { return _value.TotalDays; }
		}

		public double TotalHours
		{
			get { return _value.TotalHours; }
		}

		public double TotalMilliseconds
		{
			get { return _value.TotalMilliseconds; }
		}

		public double TotalMinutes
		{
			get { return _value.TotalMinutes; }
		}

		public double TotalSeconds
		{
			get { return _value.TotalSeconds; }
		}

		#endregion Properties

		#region Fields

		bool						_hasValue;
		TimeSpan					_value;
		public const long			TicksPerDay = TimeSpan.TicksPerDay;
		public const long			TicksPerHour = TimeSpan.TicksPerHour;
		public const long			TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
		public const long			TicksPerMinute = TimeSpan.TicksPerMinute;
		public const long			TicksPerSecond = TimeSpan.TicksPerSecond;

		public static readonly TimeSpan MaxValue = new TimeSpan(TimeSpan.MaxValue);
		public static readonly TimeSpan MinValue = new TimeSpan(TimeSpan.MinValue);
		public static readonly TimeSpan Null = new TimeSpan(TimeSpan.Zero);
		public static readonly TimeSpan Zero = new TimeSpan(TimeSpan.Zero);

		#endregion Fields
	}
}
