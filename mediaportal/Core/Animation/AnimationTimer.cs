using System;

namespace MediaPortal.Animation
{
	public sealed class AnimationTimer
	{
		#region Constructors

		static AnimationTimer()
		{
			long frequency = 0;

			if(NativeMethods.QueryPerformanceFrequency(ref frequency) == false)
				throw new NotSupportedException("Hi-res timer");

			_frequency = frequency;
		}

		private AnimationTimer()
		{
		}

		#endregion Constructors

		#region Properties

		public static double TickCount
		{
			get
			{
				long tick = 0;

				if(NativeMethods.QueryPerformanceCounter(ref tick) == false)
					throw new NotSupportedException("Hi-res timer");

				return ((double)tick / _frequency);
			}
		}

		#endregion Properties

		#region Fields

		static double				_frequency = 0;

		#endregion Fields
	}
}
