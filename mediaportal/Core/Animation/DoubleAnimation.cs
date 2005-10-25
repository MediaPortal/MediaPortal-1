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
using System.Windows;

namespace MediaPortal.Animation
{
	public class DoubleAnimation : AnimationTimeline
	{
		#region Constructors

		public DoubleAnimation()
		{
			_type = AnimationType.None;
		}

		public DoubleAnimation(double from)
		{
			_type = AnimationType.From;
			_from = from;
		}

		protected DoubleAnimation(DoubleAnimation animation, CloneType cloneType) : base(animation, cloneType)
		{
			_from = animation._from;
			_to = animation._to;
			_type = animation._type;
		}

		public DoubleAnimation(double to, Duration duration)
		{
			_type = AnimationType.To;
			_to = to;

			this.Duration = duration;
		}

		public DoubleAnimation(double to, Duration duration, FillBehavior fillBehavior)
		{
			_type = AnimationType.To;
			_to = to;

			base.Duration = duration;
			base.FillBehavior = fillBehavior;
		}

		public DoubleAnimation(double from, double to, Duration duration, FillBehavior fillBehavior)
		{
			_type = AnimationType.FromTo;
			_from = from;
			_to = to;

			base.Duration = duration;
			base.FillBehavior = fillBehavior;
		}

		#endregion Constructors

		#region Methods

		protected override Freezable CreateInstanceCore()
		{
			return new DoubleAnimation();
		}

/*		protected override object GetCurrentValueOverride(object baseValue, AnimationClock clock)
		{
			if(_type == AnimationType.By)
			{
				// animation progresses from the base value, the previous animation's output value,
				// or a zero value (depending on how the animation is configured) to the sum of that
				// value and the value specified by the By property
			}

			if(_type == AnimationType.From)
			{
				// The animation progresses from the value specified by the From property to the base value, 
				// the previous animation's output value, or a zero value (depending upon how the animation is configured).
			}

			if(_type == AnimationType.FromBy)
			{
				// animation progresses from the value specified by the From property to the value 
				// specified by the sum of the From and By properties
			}

			if(_type == AnimationType.FromTo)
			{
				// The animation progresses from the value specified by the From property to the value 
				// specified by the To property.
			}

			if(_type == AnimationType.To)
			{
				// The animation progresses from the base value, the previous animation's output value,
				// or a zero value (depending on how the animation is configured) to the value 
				// specified by the To property.
			}

			return _from + clock.CurrentProgress * (_to - _from);
		}
*/
		#endregion Methods

		#region Properties

		public override Type BaseValueType
		{
			get { return typeof(double); }
		}

		public double By
		{
			get { return _to - _from; }
			set { if((_type & AnimationType.To) != 0) throw new InvalidOperationException(); _type |= AnimationType.By; _by = value; }
		}

		public double From
		{
			get { return _from; }
			set { _type |= AnimationType.From; _from = value; }
		}

		public double To
		{
			get { return _to; }
			set { _type &= ~AnimationType.By; _type |= AnimationType.To; _to = value; }
		}

		protected override bool UsesBaseValueCore
		{
			get { return false; }
		}

		#endregion Properties

		#region Fields

		double						_by = 1;
		double						_from;
		double						_to;
		AnimationType				_type = AnimationType.None;

		#endregion Fields
	}
}
