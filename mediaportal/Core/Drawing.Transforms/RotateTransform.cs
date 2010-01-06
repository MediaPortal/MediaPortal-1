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

namespace MediaPortal.Drawing.Transforms
{
  public sealed class RotateTransform : Transform
  {
    #region Constructors

    public RotateTransform() {}

    public RotateTransform(double angle)
    {
      _angle = angle;
    }

    public RotateTransform(double angle, Point center)
    {
      _angle = angle;
      _center = center;
    }

    #endregion Constructors

    #region Methods

    protected override Matrix PrepareValue()
    {
      Matrix matrix = Matrix.Translation((float)-_center.X, (float)-_center.Y, 0);

      matrix *= Matrix.RotationZ(Microsoft.DirectX.Direct3D.Geometry.DegreeToRadian((float)_angle));
      matrix *= Matrix.Translation((float)_center.X, (float)_center.Y, 0);

      return matrix;
    }

    #endregion Methods

    #region Properties

    public double Angle
    {
      get { return _angle; }
      set
      {
        if (Equals(_angle, value) == false)
        {
          _angle = value;
          RaiseChanged();
        }
      }
    }

    public Point Center
    {
      get { return _center; }
      set
      {
        if (Equals(_center, value) == false)
        {
          _center = value;
          RaiseChanged();
        }
      }
    }

    #endregion Properties

    #region Fields

    private double _angle;
    private Point _center = Point.Empty;

    #endregion Fields
  }
}