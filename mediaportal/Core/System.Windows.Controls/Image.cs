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
  public class Image : FrameworkElement
  {
    #region Constructors

    static Image()
    {
      SourceProperty = DependencyProperty.Register("Source", typeof (ImageSource), typeof (Image));
      StretchDirectionProperty = DependencyProperty.Register("StretchDirection", typeof (StretchDirection),
                                                             typeof (Image));
      StretchProperty = DependencyProperty.Register("Stretch", typeof (Stretch), typeof (Image));
    }

    public Image() {}

    #endregion Constructors

    #region Methods

    protected override Size ArrangeOverride(Rect finalRect)
    {
      throw new NotImplementedException();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      throw new NotImplementedException();
    }

    protected override void OnRender(DrawingContext dc)
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public ImageSource Source
    {
      get { return (ImageSource)GetValue(SourceProperty); }
      set { SetValue(SourceProperty, value); }
    }

    public Stretch Stretch
    {
      get { return (Stretch)GetValue(StretchProperty); }
      set { SetValue(StretchProperty, value); }
    }

    public StretchDirection StretchDirection
    {
      get { return (StretchDirection)GetValue(StretchDirectionProperty); }
      set { SetValue(StretchDirectionProperty, value); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty SourceProperty;
    public static readonly DependencyProperty StretchDirectionProperty;
    public static readonly DependencyProperty StretchProperty;

    #endregion Properties (Dependency)
  }
}