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

using MediaPortal.Drawing;

namespace System.Windows.Controls
{
	public sealed class ContextMenuService
	{
		#region Constructors

		static ContextMenuService()
		{
			ContextMenuClosingEvent = EventManager.RegisterRoutedEvent("ContextMenuClosing", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ContextMenuService));
			ContextMenuOpeningEvent = EventManager.RegisterRoutedEvent("ContextMenuOpening", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ContextMenuService));

			ContextMenuProperty = DependencyProperty.RegisterAttached("ContextMenu", typeof(object), typeof(ContextMenuService), new FrameworkPropertyMetadata());
			HasDropShadowProperty = DependencyProperty.RegisterAttached("HasDropShadow", typeof(bool), typeof(ContextMenuService), new FrameworkPropertyMetadata(false));
			HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ContextMenuService), new FrameworkPropertyMetadata(0.0));
//			IsOpenProperty = DependencyProperty.RegisterAttached("IsOpen", typeof(bool), typeof(ContextMenuService), new FrameworkPropertyMetadata(false));
			PlacementProperty = DependencyProperty.RegisterAttached("Placement", typeof(PlacementMode), typeof(ContextMenuService), new FrameworkPropertyMetadata(PlacementMode.Bottom));
			PlacementRectangleProperty = DependencyProperty.RegisterAttached("PlacementRectangle", typeof(Rect), typeof(ContextMenuService), new FrameworkPropertyMetadata(Rect.Empty));
			PlacementTargetProperty = DependencyProperty.RegisterAttached("PlacementTarget", typeof(UIElement), typeof(ContextMenuService), new FrameworkPropertyMetadata(false));
//			StaysOpenProperty = DependencyProperty.RegisterAttached("StaysOpen", typeof(bool), typeof(ContextMenuService), new FrameworkPropertyMetadata(false));
			VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ContextMenuService), new FrameworkPropertyMetadata(0.0));
		}

		private ContextMenuService()
		{
		}

		#endregion Constructors

		#region Events (Routed)

		public static readonly RoutedEvent ContextMenuClosingEvent;
		public static readonly RoutedEvent ContextMenuOpeningEvent;

		#endregion Events (Routed)

		#region Properties (Dependency)

		public static readonly DependencyProperty ContextMenuProperty;
		public static readonly DependencyProperty HasDropShadowProperty;
		public static readonly DependencyProperty HorizontalOffsetProperty;
		public static readonly DependencyProperty IsEnabledProperty;
		public static readonly DependencyProperty PlacementProperty;
		public static readonly DependencyProperty PlacementRectangleProperty;
		public static readonly DependencyProperty PlacementTargetProperty;
		public static readonly DependencyProperty ShowOnDisabledProperty;
		public static readonly DependencyProperty VerticalOffsetProperty;

		#endregion Properties (Dependency)
	}
}
