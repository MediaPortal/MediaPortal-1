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
  public sealed class ScaleTransform : Transform
  {
    #region Constructors

    public ScaleTransform() {}

    public ScaleTransform(double scaleX, double scaleY)
    {
      _scaleX = scaleX;
      _scaleY = scaleY;
    }

    public ScaleTransform(double scaleX, double scaleY, Point center)
    {
      _scaleX = scaleX;
      _scaleY = scaleY;
      _center = center;
    }

    #endregion Constructors

    #region Methods

    protected override Matrix PrepareValue()
    {
      Matrix matrix = Matrix.Translation((float)-_center.X, (float)-_center.Y, 0);

      matrix *= Matrix.Scaling((float)_scaleX, (float)_scaleY, 1);
      matrix *= Matrix.Translation((float)_center.X, (float)_center.Y, 0);

      return matrix;
    }

    #endregion Methods

    #region Properties

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

    public double ScaleX
    {
      get { return _scaleX; }
      set
      {
        if (Equals(_scaleX, value) == false)
        {
          _scaleX = value;
          RaiseChanged();
        }
      }
    }

    public double ScaleY
    {
      get { return _scaleY; }
      set
      {
        if (Equals(_scaleY, value) == false)
        {
          _scaleY = value;
          RaiseChanged();
        }
      }
    }

    #endregion Properties

    #region Fields

    private Point _center = Point.Empty;
    private double _scaleX;
    private double _scaleY;

    #endregion Fields
  }
}