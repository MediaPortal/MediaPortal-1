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
  public sealed class TranslateTransform : Transform
  {
    #region Constructors

    public TranslateTransform() {}

    public TranslateTransform(double x, double y)
    {
      _location.X = x;
      _location.Y = y;
    }

    #endregion Constructors

    #region Methods

    protected override Matrix PrepareValue()
    {
      return Matrix.Translation((float)_location.X, (float)_location.Y, 0);
    }

    #endregion Methods

    #region Properties

    public Point Location
    {
      get { return _location; }
      set
      {
        if (Equals(_location, value) == false)
        {
          _location = value;
          RaiseChanged();
        }
      }
    }

    public double X
    {
      get { return _location.X; }
      set
      {
        if (Equals(_location.X, value) == false)
        {
          _location.X = value;
          RaiseChanged();
        }
      }
    }

    public double Y
    {
      get { return _location.Y; }
      set
      {
        if (Equals(_location.Y, value) == false)
        {
          _location.Y = value;
          RaiseChanged();
        }
      }
    }

    #endregion Properties

    #region Fields

    private Point _location;

    #endregion Fields
  }
}