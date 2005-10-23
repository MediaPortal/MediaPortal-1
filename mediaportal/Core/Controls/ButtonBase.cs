using System;
using System.Windows;

namespace MediaPortal.Controls
{
	public class ButtonBase : FrameworkElement
	{
		#region Constructors

		public ButtonBase()
		{
		}

		#endregion Constructors

		#region Events

		public event RoutedEventHandler Click
		{
			add		{ AddHandler(ClickEvent, value); } 
			remove	{ RemoveHandler(ClickEvent, value); }
		}

		#endregion Events

		#region Events (Routed)

		public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));

		#endregion Events (Routed)
	}
}
