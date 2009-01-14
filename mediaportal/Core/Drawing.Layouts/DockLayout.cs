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
using System.Windows;
using MediaPortal.GUI.Library;

namespace MediaPortal.Drawing.Layouts
{
  public class DockLayout : ILayout
  {
    #region Constructors

    public DockLayout()
    {
      _spacing = new Size(0, 0);
    }

    public DockLayout(int horizontalSpacing, int verticalSpacing)
    {
      _spacing = new Size(Math.Max(0, horizontalSpacing), Math.Max(0, verticalSpacing));
    }

    #endregion Constructors

    #region Methods

    public void Arrange(GUIGroup element)
    {
      FrameworkElement l = null;
      FrameworkElement t = null;
      FrameworkElement r = null;
      FrameworkElement b = null;
      FrameworkElement f = null;

//			foreach(FrameworkElement child in element.Children)
      {
//				if(child.Dock == Dock.Left)
//					l = child;

//				if(child.Dock == Dock.Top)
//					t = child;

//				if(child.Dock == Dock.Right)
//					r = child;

//				if(child.Dock == Dock.Bottom)
//					b = child;

//				if(child.Dock == Dock.Fill)
//					f = child;
      }

      Thickness m = element.Margin;
      Size size = element.RenderSize;
      Point location = element.Location;

      double top = location.Y + m.Top;
      double bottom = location.Y + size.Height - m.Bottom;
      double left = location.X + m.Left;
      double right = location.X + size.Width - m.Right;

      if (t != null)
      {
        t.Arrange(new Rect(left, top, right - left, t.Height));

        top = top + t.Height + _spacing.Height;
      }

      if (b != null)
      {
        b.Arrange(new Rect(left, bottom - b.Height, right - left, b.Height));

        bottom = bottom - (b.Height + _spacing.Height);
      }

      if (r != null)
      {
        r.Arrange(new Rect(right - r.Width, top, r.Width, bottom - top));

        right = right - (r.Width + _spacing.Width);
      }

      if (l != null)
      {
        l.Arrange(new Rect(left, top, l.Width, bottom - top));

        left = left + l.Width + _spacing.Width;
      }

      if (f != null)
      {
        f.Arrange(new Rect(left, top, right - left, bottom - top));
      }
    }

    public Size Measure(GUIGroup element, Size availableSize)
    {
      FrameworkElement l = null;
      FrameworkElement t = null;
      FrameworkElement r = null;
      FrameworkElement b = null;
      FrameworkElement f = null;

//			foreach(FrameworkElement child in element.Children)
      {
//				if(child.Dock == Dock.Left)
//					l = child;

//				if(child.Dock == Dock.Top)
//					t = child;

//				if(child.Dock == Dock.Right)
//					r = child;

//				if(child.Dock == Dock.Bottom)
//					b = child;

//				if(child.Dock == Dock.Fill)
//					f = child;
      }

      double w = 0;
      double h = 0;

      Size s = Size.Empty;

      if (r != null)
      {
        r.Measure(availableSize);

        w = r.Width + _spacing.Width;
        h = Math.Max(h, r.Height);
      }

      if (l != null)
      {
        l.Measure(availableSize);

        w = l.Width + _spacing.Width;
        h = Math.Max(h, l.Height);
      }

      if (f != null)
      {
        f.Measure(availableSize);

        w = w + f.Width;
        h = Math.Max(h, f.Height);
      }

      if (t != null)
      {
        t.Measure(availableSize);

        w = Math.Max(w, t.Width);
        h = h + t.Height + _spacing.Height;
      }

      if (b != null)
      {
        b.Measure(availableSize);

        w = Math.Max(w, b.Width);
        h = h + b.Height + _spacing.Height;
      }

      Thickness m = element.Margin;

      _size.Width = w + m.Width;
      _size.Height = h + m.Height;

      return _size;
    }

    #endregion Methods

    #region Properties

    public Size Size
    {
      get { return _size; }
    }

    public Size Spacing
    {
      get { return _spacing; }
      set
      {
        if (Equals(_spacing, value) == false)
        {
          _spacing = value;
        }
      }
    }

    #endregion Properties

    #region Fields

    private Size _spacing = Size.Empty;
    private Size _size = Size.Empty;

    #endregion Fields
  }
}