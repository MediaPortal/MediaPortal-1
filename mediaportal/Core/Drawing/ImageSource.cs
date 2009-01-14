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
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace MediaPortal.Drawing
{
  [TypeConverter(typeof (ImageSourceConverter))]
  public abstract class ImageSource : Animatable, IFormattable
  {
    #region Constructors

    static ImageSource()
    {
      AreaOfInterestProperty = DependencyProperty.Register("AreaOfInterest", typeof (Rect), typeof (ImageSource));
      AreaOfInterestUnitsProperty = DependencyProperty.Register("AreaOfInterestUnits", typeof (BrushMappingMode),
                                                                typeof (ImageSource));
    }

    #endregion Constructors

    #region Methods

    public new ImageSource Copy()
    {
      return (ImageSource) base.Copy();
    }

    protected override void CopyCore(Freezable sourceFreezable)
    {
    }

    protected override void CopyCurrentValueCore(Animatable sourceAnimatable)
    {
    }

    protected override bool FreezeCore(bool isChecking)
    {
      throw new NotImplementedException();
    }

    public new ImageSource GetCurrentValue()
    {
      throw new NotImplementedException();
    }

    string IFormattable.ToString(string format, IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public override string ToString()
    {
      throw new NotImplementedException();
    }

    public string ToString(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public Rect AreaOfInterest
    {
      get { return (Rect) GetValue(AreaOfInterestProperty); }
      set { SetValue(AreaOfInterestProperty, value); }
    }

    public BrushMappingMode AreaOfInterestUnits
    {
      get { return (BrushMappingMode) GetValue(AreaOfInterestUnitsProperty); }
      set { SetValue(AreaOfInterestUnitsProperty, value); }
    }

    public abstract double Height { get; }

    public abstract double Width { get; }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty AreaOfInterestProperty;
    public static readonly DependencyProperty AreaOfInterestUnitsProperty;

    #endregion Properties (Dependency)
  }
}