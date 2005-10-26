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
using System.Collections;
using System.Windows;
using System.Windows.Serialization;

namespace MediaPortal.Animation
{
	public abstract class Timeline : Animatable, IAddChild
	{
		#region Constructors

		protected Timeline()
		{
			// stop compiled never used warnings
			if(CurrentGlobalSpeedInvalidated != null)
				CurrentGlobalSpeedInvalidated(this, EventArgs.Empty);

			if(CurrentStateInvalidated != null)
				CurrentStateInvalidated(this, EventArgs.Empty);

			if(CurrentTimeInvalidated != null)
				CurrentTimeInvalidated(this, EventArgs.Empty);
		}

		protected Timeline(TimeSpan beginTime)
		{
		}

		protected Timeline(TimeSpan beginTime, Duration duration)
		{
		}

		protected internal Timeline(Timeline timeline, CloneType cloneType)
		{
		}
		
		protected Timeline(TimeSpan beginTime, Duration duration, RepeatBehavior repeatBehavior)
		{
		}

		#endregion Constructors

		#region Events

		public event EventHandler CurrentGlobalSpeedInvalidated;
		public event EventHandler CurrentStateInvalidated;
		public event EventHandler CurrentTimeInvalidated;

		#endregion Events

		#region Methods

		void IAddChild.AddChild(object child)
		{
			AddChild(child);
		}

		protected virtual void AddChild(object child)
		{
			if(child == null)
				throw new ArgumentNullException("child");

			if(child is Timeline == false)
				throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof(Timeline)));

			if(_children == null)
				_children = new TimelineCollection();

			_children.Add((Timeline)child);
		}

		void IAddChild.AddText(string text)
		{
			throw new NotSupportedException();
		}

		protected internal virtual Clock AllocateClock()
		{
			return CreateClock();
		}

		public new Timeline Copy()
		{
			return (Timeline)base.Copy();
		}

		protected override void CopyCore(Freezable sourceFreezable)
		{
			base.CopyCore(sourceFreezable);
		}

		protected override void CopyCurrentValueCore(Animatable sourceAnimatable)
		{
			base.CopyCurrentValueCore(sourceAnimatable);
		}

		public Clock CreateClock()
		{
			return new Clock(this);
		}

		protected internal Duration GetNaturalDuration(Clock clock)
		{
			return GetNaturalDurationCore(clock);
		}

		protected virtual Duration GetNaturalDurationCore(Clock clock)
		{
			return clock.NaturalDuration;
		}

		protected override void OnPropertyInvalidated(DependencyProperty dp, PropertyMetadata metadata)
		{
		}

		#endregion Methods

		#region Properties

		public double AccelerationRatio
		{ 
			get { return _accelerationRatio; }
			set { if(value < 0 || value > 1) throw new ArgumentOutOfRangeException(); _accelerationRatio = value; }
		}

		public bool AutoReverse
		{ 
			get { return _isAutoReverse; }
			set { _isAutoReverse = value; }
		}

		public double BeginTime
		{
			get { return _beginTime; }
			set { _beginTime = value; }
		}
		
		public double CutoffTime
		{
			get { return _cutoffTime; }
			set { _cutoffTime = value; }
		}

		public double DecelerationRatio
		{ 
			get { return _decelerationRatio; }
			set { if(value < 0 || value > 1) throw new ArgumentOutOfRangeException(); _decelerationRatio = value; }
		}

		public Duration Duration
		{
			get { return _duration; }
			set { _duration = value; }
		}

		public FillBehavior FillBehavior
		{
			get { return _fillBehavior; }
			set { _fillBehavior = value; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public RepeatBehavior RepeatBehavior
		{ 
			get { return _repeatBehavior; }
			set { _repeatBehavior = value; }
		}

		public double SpeedRatio
		{
			get { return _speedRatio; }
			set { _speedRatio = value; }
		}

		#endregion Properties

		#region Fields

		double						_accelerationRatio = 0;
		double						_beginTime;
		double						_cutoffTime;
		double						_decelerationRatio = 0;
		Duration					_duration = new Duration();
		FillBehavior				_fillBehavior;
		bool						_isAutoReverse;
		string						_name = string.Empty;
		RepeatBehavior				_repeatBehavior = RepeatBehavior.Forever;
		double						_speedRatio = 1;
		TimelineCollection			_children = null;

		#endregion Fields
	}
}
