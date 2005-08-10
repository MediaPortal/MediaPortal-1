using System;

namespace MediaPortal.Animations
{
	public class DoubleAnimation : Animation
	{
		#region Constructors

		public DoubleAnimation()
		{
			_type = AnimationType.Unknown;
		}

		public DoubleAnimation(double from)
		{
			_type = AnimationType.From;
			_from = from;
		}

		public DoubleAnimation(double from, double to)
		{
			_type = AnimationType.FromTo;
			_from = from;
			_to = to;
		}

		public DoubleAnimation(double from, double to, Duration duration)
		{
			_type = AnimationType.FromTo;
			_from = from;
			_to = to;

			this.Duration = duration;
		}

		#endregion Constructors

		#region Properties

		public double From
		{
			get { return _from; }
			set { _from = value; }
		}

		public double To
		{
			get { return _to; }
			set { _to = value; }
		}

		public override object Value
		{
			get
			{
				if(IsReversed)
					return Math.Max(TweenHelper.Interpolate(this.Easing, this.To, this.From, this.BeginTime, this.Duration), this.From);
				
				return Math.Min(TweenHelper.Interpolate(this.Easing, this.From, this.To, this.BeginTime, this.Duration), this.To);
			}
		}

		#endregion Properties

		#region Fields

		AnimationType				_type;
		double						_from;
		double						_to;

		#endregion Fields
	}
}
