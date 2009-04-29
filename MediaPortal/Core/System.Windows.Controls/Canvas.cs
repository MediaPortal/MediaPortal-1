#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
  public class Canvas : Panel
  {
    #region Contructors

    static Canvas()
    {
      BottomProperty = DependencyProperty.Register("Bottom", typeof (double), typeof (Canvas));
      LeftProperty = DependencyProperty.Register("Left", typeof (double), typeof (Canvas));
      RightProperty = DependencyProperty.Register("Right", typeof (double), typeof (Canvas));
      TopProperty = DependencyProperty.Register("Top", typeof (double), typeof (Canvas));
    }

    public Canvas()
    {
    }

    #endregion Contructors

    #region Methods

    protected override Size ArrangeOverride(Rect finalRect)
    {
      return base.ArrangeOverride(finalRect);
    }

    public static double GetBottom(UIElement element)
    {
      return (double) element.GetValue(BottomProperty);
    }

//		protected override Geometry GetLayoutClip(Size layoutSlotSize)
//		{
//		}

    public static double GetLeft(UIElement element)
    {
      return (double) element.GetValue(LeftProperty);
    }

    public static double GetRight(UIElement element)
    {
      return (double) element.GetValue(RightProperty);
    }

    public static double GetTop(UIElement element)
    {
      return (double) element.GetValue(TopProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
/*			if(_isEmpty)
			{
				_l = point.X;
				_t = point.Y;
				_r = point.X;
				_b = point.Y;
				_isEmpty = false;

				return;
			}

//			foreach(UIElement element in 

			_l = Math.Min(_l, point.X);
			_t = Math.Min(_t, point.Y);
			_r = Math.Max(_r, point.X);
			_b = Math.Max(_b, point.Y);
			
*/
      return base.MeasureOverride(availableSize);
    }

    public static void SetBottom(UIElement element, double bottom)
    {
      element.SetValue(BottomProperty, bottom);
    }

    public static void SetLeft(UIElement element, double left)
    {
      element.SetValue(LeftProperty, left);
    }

    public static void SetRight(UIElement element, double right)
    {
      element.SetValue(BottomProperty, right);
    }

    public static void SetTop(UIElement element, double top)
    {
      element.SetValue(BottomProperty, top);
    }

    #endregion Methods

    #region Properties (Dependency)

    public static readonly DependencyProperty BottomProperty;
    public static readonly DependencyProperty LeftProperty;
    public static readonly DependencyProperty RightProperty;
    public static readonly DependencyProperty TopProperty;

    #endregion Properties (Dependency)
  }
}