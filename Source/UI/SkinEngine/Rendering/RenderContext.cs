#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Drawing;
using MediaPortal.UI.SkinEngine.Utils;
using SlimDX;

namespace MediaPortal.UI.SkinEngine.Rendering
{
  public class RenderContext
  {
    #region Protected fields

    protected readonly float _zOrder = 1.0f;
    protected readonly double _opacity = 1.0f;
    protected Rectangle? _scissorRect = null;
    protected readonly Matrix _transform;

    #endregion

    #region Ctor

    public RenderContext(Matrix startingTransform)
    {
      _transform = startingTransform;
    }

    public RenderContext(Matrix transform, double opacity, float zOrder)
    {
      _zOrder = zOrder;
      _opacity = opacity;
      _transform = transform;
      _scissorRect = null;
    }

    #endregion

    #region Public methods

    public RenderContext Derive(RectangleF bounds, Matrix? localLayoutTransform,
        Matrix? localRenderTransform, Vector2? renderTransformOrigin,
        double localOpacity)
    {
      Matrix finalTransform = _transform.Clone();
      if (localLayoutTransform.HasValue && localLayoutTransform != Matrix.Identity)
      {
        // Layout transforms don't support translations, so center the transformation matrix at the start point
        // of the control and apply the layout transform without translation
        Vector2 origin = new Vector2(bounds.X + 0.5f*bounds.Width, bounds.Y + 0.5f*bounds.Height);
        Matrix transform = Matrix.Translation(new Vector3(-origin.X, -origin.Y, 0));
        transform *= localLayoutTransform.Value.RemoveTranslation();
        transform *= Matrix.Translation(new Vector3(origin.X, origin.Y, 0));
        finalTransform *= transform;
      }
      if (localRenderTransform.HasValue && localRenderTransform != Matrix.Identity)
      {
        Vector2 origin = renderTransformOrigin.HasValue ? new Vector2(
            bounds.X + bounds.Width * renderTransformOrigin.Value.X,
            bounds.Y + bounds.Height * renderTransformOrigin.Value.Y) : new Vector2(0, 0);
        Matrix transform = Matrix.Translation(new Vector3(-origin.X, -origin.Y, 0));
        transform *= localRenderTransform.Value;
        transform *= Matrix.Translation(new Vector3(origin.X, origin.Y, 0));
        finalTransform *= transform;
      }
      return new RenderContext(finalTransform, _opacity * localOpacity, _zOrder - 0.001f);
    }

    public void AddScissorRect(Rectangle localScissorRect)
    {
      if (GeometricHelper.HasExtends(localScissorRect))
        if (_scissorRect.HasValue)
          _scissorRect.Value.Intersect(localScissorRect);
        else
          _scissorRect = new Rectangle(localScissorRect.Location, localScissorRect.Size);
    }

    #endregion

    #region Public properties

    public float ZOrder
    {
      get { return _zOrder; }
    }

    public double Opacity
    {
      get { return _opacity; }
    }

    public Rectangle? ScissorRect
    {
      get { return _scissorRect; }
    }

    public Matrix Transform
    {
      get { return _transform; }
    }

    #endregion
  }
}
