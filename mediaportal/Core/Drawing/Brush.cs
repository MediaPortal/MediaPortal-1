#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using MediaPortal.Drawing.Transforms;

namespace MediaPortal.Drawing
{
  [TypeConverter(typeof (BrushConverter))]
  public abstract class Brush : Animatable, IFormattable
  {
    #region Constructors

    static Brush()
    {
      OpacityProperty = DependencyProperty.Register("Opacity", typeof (double), typeof (Brush),
                                                    new PropertyMetadata(1.0));
      RelativeTransformProperty = DependencyProperty.Register("RelativeTransform", typeof (Transform), typeof (Brush));
      TransformProperty = DependencyProperty.Register("Transform", typeof (Transform), typeof (Brush));
    }

    protected Brush() {}

    #endregion Constructors

    #region Methods

    public new Brush Copy()
    {
      return (Brush)base.Copy();
    }

    protected override void CopyCore(Freezable sourceFreezable) {}

    protected override void CopyCurrentValueCore(Animatable sourceAnimatable) {}

    protected override bool FreezeCore(bool isChecking)
    {
      throw new NotImplementedException();
    }

    public new Brush GetCurrentValue()
    {
      throw new NotImplementedException();
    }

    protected override void PropagateChangedHandlersCore(EventHandler handler, bool adding) {}

    string IFormattable.ToString(string format, IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public override string ToString()
    {
      return base.ToString();
    }

    public string ToString(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public double Opacity
    {
      get { return (double)GetValue(OpacityProperty); }
      set { SetValue(OpacityProperty, value); }
    }

    public Transform RelativeTransform
    {
      get { return (Transform)GetValue(RelativeTransformProperty); }
      set { SetValue(RelativeTransformProperty, value); }
    }

    public Transform Transform
    {
      get { return (Transform)GetValue(TransformProperty); }
      set { SetValue(TransformProperty, value); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty OpacityProperty;
    public static readonly DependencyProperty RelativeTransformProperty;
    public static readonly DependencyProperty TransformProperty;

    #endregion Properties (Dependency)
  }
}