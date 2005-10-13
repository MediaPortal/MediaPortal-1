using System;

namespace MediaPortal.Animation
{
	public sealed class AnimationTimer
	{
		#region Constructors

		static AnimationTimer()
		{
			if(NativeMethods.QueryPerformanceFrequency(ref _tickFrequency) == false)
				throw new NotSupportedException("Hi-res timer");
		}

		private AnimationTimer()
		{
		}

		#endregion Constructors

		#region Properties

		public static int Tick
		{
			get
			{
				long tick = 0;

				if(NativeMethods.QueryPerformanceCounter(ref tick) == false)
					throw new NotSupportedException("Hi-res timer");

				return TweenHelper.Tick = (int)((1000 * (tick / _tickFrequency)) + ((1000 * (tick % _tickFrequency)) / _tickFrequency));
			}
		}

		#endregion Properties

		#region Fields

		static long					_tickFrequency = 0;

		#endregion Fields
	}
}
