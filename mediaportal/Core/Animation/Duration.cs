using System;
using System.ComponentModel;

namespace MediaPortal.Animation
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

		#region Methods

		public static Duration Parse(string text)
		{
			if(string.Compare(text, "Automatic", true) == 0)
				return Duration.Automatic;

			return new Duration(TimeSpan.Parse(text).TotalMilliseconds);
		}

		#endregion Methods

		#region Operators
        
		public static implicit operator double(Duration duration) 
		{
			return duration._duration;
		}

		#endregion Operators

		#region Fields

		double _duration;

		public static readonly Duration Automatic = new Duration();

		#endregion Fields
	}
}
