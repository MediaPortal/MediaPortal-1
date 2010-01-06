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

using System;
using System.Windows;
using MediaPortal.GUI.Library;

namespace MediaPortal.Drawing.Layouts
{
  public class RingLayout : ILayout
  {
    #region Constructors

    public RingLayout() : this(0, 0) {}

    public RingLayout(int spacing) : this(spacing, spacing) {}

    public RingLayout(int horizontalSpacing, int verticalSpacing)
    {
      _spacing.Width = horizontalSpacing;
      _spacing.Height = verticalSpacing;
    }

    #endregion Constructors

    #region Methods

    private void ApplyAlignment(FrameworkElement element, Thickness t, double x, double y, double w, double h)
    {
      Rect rect = new Rect(x, y, element.Width, element.Height);

      switch (element.HorizontalAlignment)
      {
        case HorizontalAlignment.Center:
          rect.X = x + w / 2 - element.Width / 2;
          break;
        case HorizontalAlignment.Right:
          rect.X = x + w - element.Width;
          break;
        case HorizontalAlignment.Stretch:
          rect.Width = w - t.Right;
          break;
      }

      switch (element.VerticalAlignment)
      {
        case VerticalAlignment.Center:
          rect.Y = y + h / 2 - element.Height / 2;
          break;
        case VerticalAlignment.Bottom:
          rect.Y = h - element.Height;
          break;
        case VerticalAlignment.Stretch:
          rect.Height = h - t.Bottom;
          break;
      }

      element.Arrange(rect);
    }

    public void Arrange(GUIGroup element)
    {
      Point l = element.Location;
      Rect r = new Rect();
      Thickness t = element.Margin;

      int index = 0;

      foreach (FrameworkElement child in element.Children)
      {
        if (child.Visibility == Visibility.Collapsed)
        {
          continue;
        }

        double angle = (++index * 2 * Math.PI) / element.Children.Count;

        r.Width = child.Width;
        r.Height = child.Height;
        r.X = t.Left + _spacing.Width + ((_size.Width - t.Width - (_spacing.Width * 2)) / 2) +
              (int)(Math.Sin(angle) * _radius) - (r.Width / 2);
        r.Y = t.Top + _spacing.Height + ((_size.Height - t.Height - (_spacing.Height * 2)) / 2) -
              (int)(Math.Cos(angle) * _radius) - (r.Height / 2);

        child.Arrange(r);
      }
    }

    public Size Measure(GUIGroup element, Size availableSize)
    {
      double w = 0;
      double h = 0;

      foreach (FrameworkElement child in element.Children)
      {
        if (child.Visibility == Visibility.Collapsed)
        {
          continue;
        }

        child.Measure(availableSize);

        w = Math.Max(w, child.Width);
        h = Math.Max(h, child.Height);
      }

      Thickness t = element.Margin;

      _radius = (Math.Min(w + _spacing.Width * element.Children.Count, h + _spacing.Height * element.Children.Count) / 2);
      _radius -= Math.Max(w, h) / 2;
      _size.Width = (int)(2 * _radius) - w + t.Width;
      _size.Height = (int)(2 * _radius) - h + t.Height;

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

    private Size _size = Size.Empty;
    private Size _spacing = Size.Empty;
    private double _radius = 0;

    #endregion Fields
  }
}