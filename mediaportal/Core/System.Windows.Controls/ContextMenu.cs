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

using System.ComponentModel;
using MediaPortal.Drawing;

namespace System.Windows.Controls
{
  public class ContextMenu : MenuBase
  {
    #region Constructors

    static ContextMenu()
    {
      HasDropShadowProperty = ContextMenuService.HasDropShadowProperty.AddOwner(typeof (ContextMenu));
      HorizontalOffsetProperty = ContextMenuService.HorizontalOffsetProperty.AddOwner(typeof (ContextMenu));
      PlacementProperty = ContextMenuService.PlacementProperty.AddOwner(typeof (ContextMenu));
      PlacementRectangleProperty = ContextMenuService.PlacementRectangleProperty.AddOwner(typeof (ContextMenu));
      PlacementTargetProperty = ContextMenuService.PlacementTargetProperty.AddOwner(typeof (UIElement));
      VerticalOffsetProperty = ContextMenuService.VerticalOffsetProperty.AddOwner(typeof (ContextMenu));
    }

    public ContextMenu()
    {
    }

    #endregion Constructors

    #region Events

    public event RoutedEventHandler Closed
    {
      add { AddHandler(ClosedEvent, value); }
      remove { AddHandler(ClosedEvent, value); }
    }

    public event RoutedEventHandler Opened
    {
      add { AddHandler(OpenedEvent, value); }
      remove { AddHandler(OpenedEvent, value); }
    }

    #endregion Events

    #region Events (Routed)

    public static readonly RoutedEvent ClosedEvent;
    public static readonly RoutedEvent OpenedEvent;

    #endregion Events (Routed)

    #region Methods

/*		protected override void AddChild(object child)
		{
			if(child == null)
				throw new ArgumentNullException("child");

			if(child is MenuItem == false)
				throw new Exception(string.Format("Cannot convert '{0}' to type '{1}'", child.GetType(), typeof(MenuItem)));

			Items.Add((MenuItem)child);
		}
*/

    protected virtual void OnClosed(RoutedEventArgs e)
    {
    }

//		protected override void OnKeyDown(KeyEventArgs e)
//		{
//		}

    protected virtual void OnOpened(RoutedEventArgs e)
    {
    }

    protected internal override void OnVisualParentChanged(Visual oldParent)
    {
    }

//		protected override void PrepareContainerForItemOverride(DependencyObject element, Object item)
//		{
//		}

    #endregion Methods

    #region Properties

//		[BindableAttribute(false)] 
//		public CustomPopupPlacementCallback CustomPopupPlacementCallback
//		{
//			get { return (CustomPopupPlacementCallback)GetValue(CustomPopupPlacementCallbackProperty); }
//			set { SetValue(CustomPopupPlacementCallbackProperty, value); }
//		}

    public bool HasDropShadow
    {
      get { return (bool) GetValue(HasDropShadowProperty); }
      set { SetValue(HasDropShadowProperty, value); }
    }

    [Bindable(true)]
    public double HorizontalOffset
    {
      get { return (double) GetValue(HorizontalOffsetProperty); }
      set { SetValue(HorizontalOffsetProperty, value); }
    }

    [Bindable(true)]
    public bool IsOpen
    {
      get { return (bool) GetValue(IsOpenProperty); }
      set { SetValue(IsOpenProperty, value); }
    }

    [Bindable(true)]
    public PlacementMode Placement
    {
      get { return (PlacementMode) GetValue(PlacementProperty); }
      set { SetValue(PlacementProperty, value); }
    }

    [Bindable(true)]
    public Rect PlacementRectangle
    {
      get { return (Rect) GetValue(PlacementRectangleProperty); }
      set { SetValue(PlacementRectangleProperty, value); }
    }

    [Bindable(true)]
    public UIElement PlacementTarget
    {
      get { return (UIElement) GetValue(PlacementTargetProperty); }
      set { SetValue(PlacementTargetProperty, value); }
    }

    [Bindable(true)]
    public bool StaysOpen
    {
      get { return (bool) GetValue(StaysOpenProperty); }
      set { SetValue(StaysOpenProperty, value); }
    }

    [Bindable(true)]
    public double VerticalOffset
    {
      get { return (double) GetValue(VerticalOffsetProperty); }
      set { SetValue(VerticalOffsetProperty, value); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty CustomPopupPlacementCallbackProperty;
    public static readonly DependencyProperty HasDropShadowProperty;
    public static readonly DependencyProperty HorizontalOffsetProperty;
    public static readonly DependencyProperty IsOpenProperty;
    public static readonly DependencyProperty PlacementProperty;
    public static readonly DependencyProperty PlacementRectangleProperty;
    public static readonly DependencyProperty PlacementTargetProperty;
    public static readonly DependencyProperty StaysOpenProperty;
    public static readonly DependencyProperty VerticalOffsetProperty;

    #endregion Properties (Dependency)
  }
}