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
	public abstract class Timeline : AnimationBase, IAddChild
	{
		#region Constructors

		protected Timeline()
		{
			// stop compiled never used warnings
			if(CurrentGlobalSpeedInvalidated != null)
				CurrentGlobalSpeedInvalidated(this, EventArgs.Empty);

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

		public event EventHandler Changed;
		public event EventHandler CurrentGlobalSpeedInvalidated;
		public event EventHandler CurrentStateInvalidated;
		public event EventHandler CurrentTimeInvalidated;

		#endregion Events

		internal void Begin()
		{
			lock(this)
			{
//				_iterationCount = 0;
				_isReversed = false;
				_isAnimating = true;
//				_beginTime = AnimationTimer.Progress();
//				_beginTimeRepetition = _beginTime;

				if(CurrentStateInvalidated != null)
					CurrentStateInvalidated(this, EventArgs.Empty);
			}
		}

		#region Methods

		void RaiseChanged()
		{
			if(Changed != null)
				Changed(this, EventArgs.Empty);
		}

		void IAddChild.AddChild(object child)
		{
			AddChild(child);
		}

		protected virtual void AddChild(object child)
		{
			AnimationBase animation = child as AnimationBase;

			if(animation == null)
				return;

			if(_children == null)
				_children = new ArrayList();

			_children.Add(child);
		}

		void IAddChild.AddText(string text)
		{
			throw new NotSupportedException();
		}

		protected internal virtual Clock AllocateClock()
		{
			return new Clock(this);
		}

		public Timeline Copy()
		{
			return CopyOverride();
		}

		protected abstract Timeline CopyOverride();

		public Clock CreateClock()
		{
			return AllocateClock();
		}

		protected internal Duration GetNaturalDuration(Clock clock)
		{
			return GetNaturalDurationOverride(clock);
		}

		protected virtual Duration GetNaturalDurationOverride(Clock clock)
		{
			return Duration.Automatic;
		}

		#endregion Methods

		#region Properties

		public double AccelerationRatio
		{ 
			get { return _accelerationRatio; }
			set { if(!double.Equals(_accelerationRatio, value)) { _accelerationRatio = value; RaiseChanged(); } }
		}

		public bool AutoReverse
		{ 
			get { return _isAutoReverse; }
			set { if(!bool.Equals(_isAutoReverse, value)) { _isAutoReverse = value; RaiseChanged(); } }
		}

		public double BeginTime
		{
			get { return _beginTime; }
			set { if(!double.Equals(_beginTime, value)) { _beginTime = value; RaiseChanged(); } }
		}
		
		public double CutoffTime
		{
			get { return _cutoffTime; }
			set { if(!double.Equals(_cutoffTime, value)) { _cutoffTime = value; RaiseChanged(); } }
		}

		public double DecelerationRatio
		{ 
			get { return _decelerationRatio; }
			set { if(!double.Equals(_decelerationRatio, value)) { _decelerationRatio = value; RaiseChanged(); } }
		}

		public Duration Duration
		{
			get { return _duration; }
			set { if(!Duration.Equals(_duration, value)) { _duration = value; RaiseChanged(); } }
		}

		public FillBehavior FillBehavior
		{
			get { return _fillBehavior; }
			set { if(!FillBehavior.Equals(_fillBehavior, value)) { _fillBehavior = value; RaiseChanged(); } }
		}

		public PropertyPath Path
		{
			get { return _path; }
			set { if(!PropertyPath.Equals(_path, value)) { _path = value; RaiseChanged(); } }
		}

		public string Name
		{
			get { return _name; }
			set { if(!string.Equals(_name, value)) { _name = value; RaiseChanged(); } }
		}

		public RepeatBehavior RepeatBehavior
		{ 
			get { return _repeatBehavior; }
			set { if(!RepeatBehavior.Equals(_repeatBehavior, value)) { _repeatBehavior = value; RaiseChanged(); } }
		}

		public ClockController InteractiveController
		{
			get { if(_interactiveController == null) _interactiveController = new ClockController(); return _interactiveController; }
		}

		public bool IsAnimating
		{
			get { return _isAnimating; }
		}

		public bool IsReversed
		{
			get { return _isReversed; }
		}

		#endregion Properties

		#region Fields

		double						_accelerationRatio;
		double						_beginTime;
//		double						_beginTimeRepetition = 0;
		FillBehavior				_fillBehavior;
		bool						_isAutoReverse;
		bool						_isAnimating;
		ArrayList					_children;		
		double						_cutoffTime;
		double						_decelerationRatio;
		Duration					_duration = new Duration();
		PropertyPath				_path;
		string						_name;
		RepeatBehavior				_repeatBehavior;
		ClockController				_interactiveController;
		bool						_isReversed;
//		int							_iterationCount = 0;
//		bool						_isPaused = false;

		#endregion Fields
	}
}
