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

namespace System.Windows.Controls
{
  public class ColumnDefinition : DefinitionBase
  {
    #region Constructors

    static ColumnDefinition()
    {
      MaxWidthProperty = DependencyProperty.Register("MaxWidth", typeof (double), typeof (ColumnDefinition));
      MinWidthProperty = DependencyProperty.Register("MinWidth", typeof (double), typeof (ColumnDefinition));
      WidthProperty = DependencyProperty.Register("Width", typeof (GridLength), typeof (ColumnDefinition),
                                                  new PropertyMetadata(new GridLength(GridUnitType.Star)));
    }

    public ColumnDefinition() {}

    #endregion Constructors

    #region Properties

    public double ActualWidth
    {
      get { return ((GridLength)GetValue(WidthProperty)).Value; }
    }

    public double MaxWidth
    {
      get { return (double)GetValue(MaxWidthProperty); }
      set { SetValue(MaxWidthProperty, value); }
    }

    public double MinWidth
    {
      get { return (double)GetValue(MinWidthProperty); }
      set { SetValue(MinWidthProperty, value); }
    }

    public double Offset
    {
      get { return _offset; }
    }

    public GridLength Width
    {
      get { return (GridLength)GetValue(WidthProperty); }
      set { SetValue(WidthProperty, value); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty MaxWidthProperty;
    public static readonly DependencyProperty MinWidthProperty;
    public static readonly DependencyProperty WidthProperty;

    #endregion Properties (Dependency)

    #region Fields

    private double _offset = 0;

    #endregion Fields
  }
}