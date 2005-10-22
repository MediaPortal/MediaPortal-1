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
using System.Windows.Serialization;

namespace System.Windows
{
	public class EventTrigger : TriggerBase, IAddChild
	{
		#region Constructors

		public EventTrigger()
		{
		}

		public EventTrigger(RoutedEvent routedEvent)
		{
			_routedEvent = routedEvent;
		}

		#endregion Constructors

		#region Methods

		void IAddChild.AddChild(object child)
		{
		}

		void IAddChild.AddText(string text)
		{
			throw new NotSupportedException("EventTrigger.IAddChild.AddText");
		}

		#endregion Methods

		#region Properties

		public TriggerActionCollection Actions
		{
			get { if(_actions == null) _actions = new TriggerActionCollection(); return _actions; }
		}

		public RoutedEvent RoutedEvent
		{
			get { return _routedEvent; }
			set { _routedEvent = value; }
		}

		public string SourceName
		{
			get { return _sourceName; }
			set { _sourceName = value; }
		}

		#endregion Properties

		#region Fields

		TriggerActionCollection		_actions;
		RoutedEvent					_routedEvent;
		string						_sourceName;

		#endregion Fields
	}
}
