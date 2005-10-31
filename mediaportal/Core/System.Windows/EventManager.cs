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

namespace System.Windows
{
	public sealed class EventManager
	{
		#region Constructors
		
		private EventManager()
		{
		}

		#endregion Constructors

		#region Methods
		
		public static RoutedEvent GetRoutedEventFromName(string name, Type ownerType)
		{
			if(ownerType == typeof(MediaPortal.GUI.Library.GUIWindow))
				return (RoutedEvent)_routedEvents[typeof(System.Windows.Controls.Page) + name];

			return (RoutedEvent)_routedEvents[ownerType + name];
		}

		public static RoutedEvent[] GetRoutedEvents()
		{
			throw new NotImplementedException();
		}

		public static RoutedEvent[] GetRoutedEventsForOwner(Type ownerType)
		{
			throw new NotImplementedException();
		}

		public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler)
		{
			RegisterClassHandler(classType, routedEvent, handler, true);
		}
			
		public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
		{
			_classHandlers[classType.GetHashCode() ^ routedEvent.GetHashCode()] = new RoutedEventHandlerInfo(handler, handledEventsToo);
		}

		public static RoutedEvent RegisterRoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
		{
			RoutedEvent routedEvent = new RoutedEvent(name, routingStrategy, handlerType, ownerType);

			_routedEvents[ownerType + name] = routedEvent;

			return routedEvent;
		}

		#endregion Methods

		#region Fields

		static Hashtable			_routedEvents = new Hashtable(100);
		static Hashtable			_classHandlers = new Hashtable(100);

		#endregion Fields
	}
}
