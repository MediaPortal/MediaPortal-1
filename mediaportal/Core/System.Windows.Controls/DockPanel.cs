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
  public class DockPanel : Panel
  {
    #region Constructors

    static DockPanel()
    {
      DockProperty = DependencyProperty.RegisterAttached("Dock", typeof (Dock), typeof (DockPanel));
      LastChildFillProperty = DependencyProperty.Register("LastFillChild", typeof (bool), typeof (DockPanel),
                                                          new PropertyMetadata(true));
    }

    public DockPanel()
    {
    }

    #endregion Constructors

    #region Methods

    protected override Size ArrangeOverride(Rect finalRect)
    {
      FrameworkElement l = null;
      FrameworkElement t = null;
      FrameworkElement r = null;
      FrameworkElement b = null;
      FrameworkElement f = null;

      foreach (FrameworkElement element in Children)
      {
        if (GetDock(element) == Dock.Left)
        {
          l = element;
        }

        if (GetDock(element) == Dock.Top)
        {
          t = element;
        }

        if (GetDock(element) == Dock.Right)
        {
          r = element;
        }

        if (GetDock(element) == Dock.Bottom)
        {
          b = element;
        }

        if (GetDock(element) == Dock.Fill)
        {
          f = element;
        }
      }

      double top = Location.Y + Margin.Top;
      double bottom = Location.Y + Height - Margin.Bottom;
      double left = Location.X + Margin.Left;
      double right = Location.X + Width - Margin.Right;

      Size _spacing = Size.Empty;

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

      return Size.Empty;
    }

    public static Dock GetDock(UIElement element)
    {
      return (Dock) element.GetValue(DockProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      FrameworkElement l = null;
      FrameworkElement t = null;
      FrameworkElement r = null;
      FrameworkElement b = null;
      FrameworkElement f = null;

      foreach (FrameworkElement element in Children)
      {
        if (GetDock(element) == Dock.Left)
        {
          l = element;
        }

        if (GetDock(element) == Dock.Top)
        {
          t = element;
        }

        if (GetDock(element) == Dock.Right)
        {
          r = element;
        }

        if (GetDock(element) == Dock.Bottom)
        {
          b = element;
        }

        if (GetDock(element) == Dock.Fill)
        {
          f = element;
        }
      }

      double w = 0;
      double h = 0;

      Size s = Size.Empty;
      Size _spacing = Size.Empty;

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

      return new Size(w + Margin.Width, h + Margin.Height);
    }

    public static void SetDock(UIElement element, Dock dock)
    {
      element.SetValue(DockProperty, dock);
    }

    #endregion Methods

    #region Properties

    public bool LastChildFill
    {
      get { return (bool) GetValue(LastChildFillProperty); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty DockProperty;
    public static readonly DependencyProperty LastChildFillProperty;

    #endregion Properties (Dependency)
  }
}