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

using System.Drawing;

namespace MediaPortal.Drawing
{
  public abstract class GradientBrush : Brush
  {
    #region Constructors

    public GradientBrush() {}

    public GradientBrush(GradientStopCollection gradientStops)
    {
      _gradientStops = gradientStops;
    }

    #endregion Constructors

    #region Methods

    public void AddStop(Color color, double offset)
    {
      GradientStops.Add(new GradientStop(color, offset));
    }

    #endregion Methods

    #region Properties

//		public ColorInterpolationMode ColorInterpolationMode
//		{
//			get { return _colorInterpolationMode; }
//			set { if(_colorInterpolationMode.Equals(_colorInterpolationMode) == false) { _colorInterpolationMode = value; _isDirty = true; } } 
//		}

    public GradientStopCollection GradientStops
    {
      get
      {
        if (_gradientStops == null)
        {
          _gradientStops = new GradientStopCollection();
        }
        return _gradientStops;
      }
    }

    public GradientSpreadMethod SpreadMethod
    {
      get { return _spreadMethod; }
      set { _spreadMethod = value; }
    }

    #endregion Properties

    #region Fields

//		ColorInterpolationMode				_colorInterpolationMode = ColorInterpolationMode.PhysicallyLinearGamma;
    private GradientStopCollection _gradientStops;
    private GradientSpreadMethod _spreadMethod;

    #endregion Fields
  }
}