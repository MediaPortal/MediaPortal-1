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
using System.Threading;

namespace MediaPortal.Animation
{
	public class Clock : DispatcherObject
	{
		#region Constructors

		protected internal Clock(Timeline timeline)
		{
			_timeline = timeline;
			_timeline.CurrentGlobalSpeedInvalidated += new EventHandler(TimelineCurrentGlobalSpeedInvalidated);
			_timeline.CurrentStateInvalidated += new EventHandler(TimelineCurrentStateInvalidated);
			_timeline.CurrentTimeInvalidated += new EventHandler(TimelineCurrentTimeInvalidated);
		}

		#endregion Constructors

		#region Events

		public event EventHandler CurrentGlobalSpeedInvalidated;
		public event EventHandler CurrentStateInvalidated;
		public event EventHandler CurrentTimeInvalidated;
		
		#endregion Events

		#region Methods

		protected virtual void DiscontinuousTimeMovement()
		{
		}

		protected virtual void SpeedChanged()
		{
		}

		protected virtual void Stopped()
		{
		}
		
		private void TimelineCurrentGlobalSpeedInvalidated(object sender, EventArgs e)
		{
			if(CurrentGlobalSpeedInvalidated != null)
				CurrentGlobalSpeedInvalidated(sender, e);
		}

		private void TimelineCurrentStateInvalidated(object sender, EventArgs e)
		{
			if(CurrentStateInvalidated != null)
				CurrentStateInvalidated(sender, e);
		}

		private void TimelineCurrentTimeInvalidated(object sender, EventArgs e)
		{
			if(CurrentTimeInvalidated != null)
				CurrentTimeInvalidated(sender, e);
		}

		#endregion Methods

		#region Properties

		public ClockController Controller
		{ 
			get { return null; }
		}

		public double CurrentGlobalSpeed
		{
			get { return 1; }
		}

		public int CurrentIteration
		{ 
			// if the timeline is not active, the value of this property is only valid if the fill attribute specifies that the timing attributes should be extended. Otherwise, the property returns -1. 
			get { return 1; }
		}

		public double CurrentProgress
		{ 
			get { return 1; }
		}

		public ClockState CurrentState
		{ 
			get { return _currentState; }
		}

		public TimeSpan CurrentTime
		{
			get { return _currentTime; }
		}

		public bool IsPaused
		{ 
			get { return _isPaused; }
		}

		public Duration NaturalDuration
		{
			get { return _timeline.Duration; }
		}

		public Clock Parent
		{
			get { return _parent; }
		}

		public Timeline Timeline
		{
			get { return _timeline; }
		}

		#endregion Properties

		#region Fields

		ClockState					_currentState = ClockState.Stopped;
		TimeSpan					_currentTime = TimeSpan.Zero;
		bool						_isPaused = false;
		Duration					_naturalDuration = Duration.Automatic;
		Clock						_parent = null;
		Timeline					_timeline = null;

		#endregion Fields
	}
}
