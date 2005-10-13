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
			_count = Math.Max(1, Math.Min(double.MaxValue, iterationCount));
			_duration = null;
		}

		public RepeatBehavior(Duration repeatDuration)
		{
			if(repeatDuration == null)
				throw new ArgumentNullException("repeatDuration");

			_count = double.NaN;
			_duration = repeatDuration;
		}

		#endregion Constructors

		#region Properties

		public bool IsIterationCount
		{ 
			get { return _count != double.NaN; }
		}

		public bool IsRepeatDuration 
		{ 
			get { return _duration != null; } 
		}

		public double IterationCount 
		{ 
			get { return _count < 1 ? 1 : _count; }
		}

		public Duration RepeatDuration 
		{ 
			get { return _duration; } 
		}

		#endregion Properties

		#region Members

		double						_count;
		Duration					_duration;
		public static readonly RepeatBehavior		Forever = new RepeatBehavior(double.PositiveInfinity);

		#endregion Members
	}
}