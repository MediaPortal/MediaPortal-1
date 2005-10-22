using System;

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
			return new RoutedEvent();
		}

		public static RoutedEvent[] GetRoutedEvents()
		{
			return new RoutedEvent[0];
		}

		public static RoutedEvent[] GetRoutedEventsForOwner(Type ownerType)
		{
			return new RoutedEvent[0];
		}

		public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler)
		{
		}
			
		public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
		{
		}

		public static RoutedEvent RegisterRoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
		{
			return new RoutedEvent(name, routingStrategy, handlerType, ownerType);
		}

		#endregion Methods
	}
}
