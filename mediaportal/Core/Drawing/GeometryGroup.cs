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
using System.Windows.Serialization;

namespace MediaPortal.Drawing
{
  public sealed class GeometryGroup : Geometry, IAddChild
  {
    #region Constructors

    public GeometryGroup() {}

    #endregion Constructors

    #region Methods

    void IAddChild.AddChild(object child) {}

    void IAddChild.AddText(string text) {}

    #endregion Methods

    #region Fields

    public override Rect Bounds
    {
      get { throw new NotImplementedException(); }
    }

    public GeometryCollection Children
    {
      get { throw new NotImplementedException(); }
      set { }
    }

    public FillRule FillRule
    {
      get { throw new NotImplementedException(); }
      set { }
    }

    #endregion Fields
  }
}