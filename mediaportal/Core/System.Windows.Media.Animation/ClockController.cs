#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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
using System.ComponentModel;

using MediaPortal.Dispatcher;

namespace System.Windows.Media.Animation
{
	public sealed class ClockController
	{
		#region Constructors

		internal ClockController(Clock clock)
		{
			_clock = clock;
		}

		#endregion Constructors

		#region Methods

		public void Begin()
		{
		}

		public void Pause()
		{
		}

		public void Resume()
		{
//			_isPaused = false;
		}

		public void Reverse()
		{
//			_isReversed = !_isReversed;
		}

		public void Seek(TimeSpan offset, TimeSeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public void SeekAlignedToLastTick(TimeSpan offset, TimeSeekOrigin origin)
		{
		}

		public void SkipToFill()
		{
//			if(blahblah == RepeatBehavior.Forever)
//				throw new InvalidOperationException();
		}

		public void Stop()
		{
		}

		#endregion Methods

		#region Properties
	
		public Clock Clock
		{
			get { return _clock; }
		}

		public double SpeedRatio
		{ 
			get { return _speedRatio; }
			set { _speedRatio = value; }
		}

		#endregion Properties
        
		#region Fields
		
		Clock					_clock = null;
		double					_speedRatio = 1;

		#endregion Fields
	}
}
