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

namespace System.Windows.Dispatcher
{
	public class DispatcherTimer
	{
		#region Constructors

		public DispatcherTimer() : this(DispatcherPriority.Background)
		{
		}

		public DispatcherTimer(DispatcherPriority priority) : this(priority, null)
		{
		}

		public DispatcherTimer(DispatcherPriority priority, Dispatcher dispatcher) : this(TimeSpan.Zero, priority, null, dispatcher)
		{
		}

		public DispatcherTimer(TimeSpan interval, DispatcherPriority priority, EventHandler callback, Dispatcher dispatcher)
		{
			_interval = interval;
			_priority = priority;

			if(callback != null)
				Tick += callback;

			_dispatcher = dispatcher;
		}

		#endregion Constructors

		#region Events

		public event EventHandler Tick;

		#endregion Events

		#region Methods

		public void Start()
		{
		}

		public void Stop()
		{
		}

		#endregion Methods

		#region Properties

		public Dispatcher Dispatcher
		{
			get { return _dispatcher; }
		}

		public TimeSpan Interval
		{
			get { return _interval; }
			set { _interval = value; }
		}

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set { _isEnabled = true; }
		}
		
		public object Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		#endregion Properties

		#region Fields

		Dispatcher					_dispatcher;
		bool						_isEnabled;
		TimeSpan					_interval;
		DispatcherPriority			_priority;
		object						_tag;

		#endregion Fields
	}
}
