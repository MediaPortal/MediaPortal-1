using System;
using System.ComponentModel;

namespace MediaPortal.Animations
{
	[TypeConverter(typeof(DurationConverter))]
	public class Duration
	{
		#region Constructors

		public Duration()
		{
		}

		public Duration(double duration)
		{
			_duration = duration;
		}

		#endregion Constructors

		#region Operators
        
		public static implicit operator double(Duration duration) 
		{
			return duration._duration;
		}

		#endregion Operators

		#region Fields

		double _duration;

		#endregion Fields
	}
}
