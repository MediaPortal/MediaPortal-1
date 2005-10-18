using System;
using System.ComponentModel;

namespace MediaPortal.Animation
{
	[TypeConverter(typeof(RepeatBehaviorConverter))]
	public struct RepeatBehavior
	{
		#region Constructors

		public RepeatBehavior(double iterationCount)
		{
			_count = Math.Max(1, iterationCount);
			_duration = null;
		}

		public RepeatBehavior(Duration duration)
		{
			if(duration == null)
				throw new ArgumentNullException("duration");

			_count = 0;
			_duration = duration;
		}

		#endregion Constructors

		#region Methods

		public static RepeatBehavior Parse(string text)
		{
			if(string.Compare(text, "Forever", true) == 0)
				return RepeatBehavior.Forever;

			return new RepeatBehavior(double.Parse((string)text));
		}

		#endregion Methods

		#region Properties

		public bool IsIterationCount
		{ 
			get { return _count != 0; }
		}

		public bool IsRepeatDuration 
		{ 
			get { return _duration != null; } 
		}

		public double IterationCount 
		{ 
			get { return _count; }
		}

		public Duration RepeatDuration 
		{ 
			get { return _duration; } 
		}

		#endregion Properties

		#region Members

		double									_count;
		Duration								_duration;
		public static readonly RepeatBehavior	Forever = new RepeatBehavior(double.PositiveInfinity);

		#endregion Members
	}
}