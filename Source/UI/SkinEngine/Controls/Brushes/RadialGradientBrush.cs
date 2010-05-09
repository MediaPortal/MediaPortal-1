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

using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  // TODO: Implement Freezable behaviour
  public class RadialGradientBrush : GradientBrush
  {
    #region Private fields

    EffectAsset _effect;

    AbstractProperty _centerProperty;
    AbstractProperty _gradientOriginProperty;
    AbstractProperty _radiusXProperty;
    AbstractProperty _radiusYProperty;
    bool _refresh = false;
    bool _singleColor = true;
    EffectHandleAsset _handleRelativeTransform;
    EffectHandleAsset _handleFocus;
    EffectHandleAsset _handleCenter;
    EffectHandleAsset _handleRadius;
    EffectHandleAsset _handleOpacity;
    EffectHandleAsset _handleColor;
    EffectHandleAsset _handleAlphaTexture;
    GradientBrushTexture _gradientBrushTexture;
    float[] g_focus;
    float[] g_center;
    float[] g_radius;

    #endregion

    #region Ctor

    public RadialGradientBrush()
    {
      Init();
      Attach();
    }

    public override void Dispose()
    {
      base.Dispose();
      Detach();
    }

    void Init()
    {
      _centerProperty = new SProperty(typeof(Vector2), new Vector2(0.5f, 0.5f));
      _gradientOriginProperty = new SProperty(typeof(Vector2), new Vector2(0.5f, 0.5f));
      _radiusXProperty = new SProperty(typeof(double), 0.5);
      _radiusYProperty = new SProperty(typeof(double), 0.5);
    }

    void Attach()
    {
      _centerProperty.Attach(OnPropertyChanged);
      _gradientOriginProperty.Attach(OnPropertyChanged);
      _radiusXProperty.Attach(OnPropertyChanged);
      _radiusYProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _centerProperty.Detach(OnPropertyChanged);
      _gradientOriginProperty.Detach(OnPropertyChanged);
      _radiusXProperty.Detach(OnPropertyChanged);
      _radiusYProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      RadialGradientBrush b = (RadialGradientBrush) source;
      Center = copyManager.GetCopy(b.Center);
      GradientOrigin = copyManager.GetCopy(b.GradientOrigin);
      RadiusX = b.RadiusX;
      RadiusY = b.RadiusY;
      Attach();
    }

    #endregion

    #region Protected methods

    protected override void OnPropertyChanged(AbstractProperty prop, object oldValue)
    {
      _refresh = true;
      FireChanged();
    }

    #endregion

    #region Public properties

    public AbstractProperty CenterProperty
    {
      get { return _centerProperty; }
    }

    public Vector2 Center
    {
      get { return (Vector2)_centerProperty.GetValue(); }
      set { _centerProperty.SetValue(value); }
    }

    public AbstractProperty GradientOriginProperty
    {
      get { return _gradientOriginProperty; }
    }

    public Vector2 GradientOrigin
    {
      get { return (Vector2) _gradientOriginProperty.GetValue(); }
      set { _gradientOriginProperty.SetValue(value); }
    }

    public AbstractProperty RadiusXProperty
    {
      get { return _radiusXProperty; }
    }

    public double RadiusX
    {
      get { return (double) _radiusXProperty.GetValue(); }
      set { _radiusXProperty.SetValue(value); }
    }

    public AbstractProperty RadiusYProperty
    {
      get { return _radiusYProperty; }
    }

    public double RadiusY
    {
      get { return (double) _radiusYProperty.GetValue(); }
      set { _radiusYProperty.SetValue(value); }
    }

    public override Texture Texture
    {
      get { return _gradientBrushTexture.Texture; }
    }

    #endregion

    protected void CheckSingleColor()
    {
      int color = -1;
      _singleColor = true;
      foreach (GradientStop stop in GradientStops)
        if (color == -1)
          color = stop.Color.ToArgb();
        else
          if (color != stop.Color.ToArgb())
          {
            _singleColor = false;
            return;
          }
    }

    #region Public methods

    public override void SetupBrush(ref PositionColored2Textured[] verts, float zOrder)
    {
      base.SetupBrush(ref verts, zOrder);

      if (_gradientBrushTexture == null)
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
      _refresh = true;
    }

    public override bool BeginRenderBrush(PrimitiveContext primitiveContext, RenderContext renderContext)
    {
      Matrix finalTransform = renderContext.Transform.Clone();
      if (Transform != null)
        finalTransform *= Transform.GetTransform();
      if (_gradientBrushTexture == null) return false;
      if (_refresh)
      {
        _refresh = false;
        CheckSingleColor();
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
        if (_singleColor)
        {
          _effect = ContentManager.GetEffect("solidbrush");
          _handleColor = _effect.GetParameterHandle("g_solidColor");
        }
        else
        {
          _effect = ContentManager.GetEffect("radialgradient");
          _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
          _handleFocus = _effect.GetParameterHandle("g_focus");
          _handleCenter = _effect.GetParameterHandle("g_center");
          _handleRadius = _effect.GetParameterHandle("g_radius");
          _handleOpacity = _effect.GetParameterHandle("g_opacity");
        }

        g_focus = new float[] { GradientOrigin.X, GradientOrigin.Y };
        g_center = new float[] { Center.X, Center.Y };
        g_radius = new float[] { (float) RadiusX, (float) RadiusY };

        if (MappingMode == BrushMappingMode.Absolute)
        {
          g_focus[0] /= _vertsBounds.Width;
          g_focus[1] /= _vertsBounds.Height;

          g_center[0] /= _vertsBounds.Width;
          g_center[1] /= _vertsBounds.Height;

          g_radius[0] /= _vertsBounds.Width;
          g_radius[1] /= _vertsBounds.Height;
        }
      }

      if (_singleColor)
      {
        Color4 v = ColorConverter.FromColor(GradientStops[0].Color);
        _handleColor.SetParameter(v);
        _effect.StartRender(finalTransform);
      }
      else
      {
// TODO: Why invert?
        Matrix m = Matrix.Invert(RelativeTransform.GetTransformRel());

        _handleRelativeTransform.SetParameter(m);
        _handleFocus.SetParameter(g_focus);
        _handleCenter.SetParameter(g_center);
        _handleRadius.SetParameter(g_radius);
        _handleOpacity.SetParameter((float) (Opacity * renderContext.Opacity));

        _effect.StartRender(_gradientBrushTexture.Texture, finalTransform);
      }
      return true;
    }

    public override void BeginRenderOpacityBrush(Texture tex, RenderContext renderContext)
    {
      if (tex == null)
        return;
      Matrix finalTransform = renderContext.Transform.Clone();
      if (Transform != null)
        finalTransform *= Transform.GetTransform();
      if (_refresh)
      {
        _refresh = false;
        CheckSingleColor();
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
        _effect = ContentManager.GetEffect("radialopacitygradient");
        _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
        _handleFocus = _effect.GetParameterHandle("g_focus");
        _handleCenter = _effect.GetParameterHandle("g_center");
        _handleRadius = _effect.GetParameterHandle("g_radius");
        _handleOpacity = _effect.GetParameterHandle("g_opacity");
        _handleAlphaTexture = _effect.GetParameterHandle("g_alphatex");

        g_focus = new float[] { GradientOrigin.X, GradientOrigin.Y };
        g_center = new float[] { Center.X, Center.Y };
        g_radius = new float[] { (float) RadiusX, (float) RadiusY };

        if (MappingMode == BrushMappingMode.Absolute)
        {
          g_focus[0] /= _vertsBounds.Width;
          g_focus[1] /= _vertsBounds.Height;

          g_center[0] /= _vertsBounds.Width;
          g_center[1] /= _vertsBounds.Height;

          g_radius[0] /= _vertsBounds.Width;
          g_radius[1] /= _vertsBounds.Height;
        }
      }

      if (_singleColor)
        _effect.StartRender(finalTransform);
      else
      {
// TODO: Why invert?
        Matrix m = Matrix.Invert(RelativeTransform.GetTransformRel());

        _handleRelativeTransform.SetParameter(m);
        _handleFocus.SetParameter(g_focus);
        _handleCenter.SetParameter(g_center);
        _handleRadius.SetParameter(g_radius);
        _handleOpacity.SetParameter((float) (Opacity * renderContext.Opacity));
        _handleAlphaTexture.SetParameter(_gradientBrushTexture.Texture);
        _effect.StartRender(tex, finalTransform);
      }
    }

    public override void EndRender()
    {
      if (_effect != null)
        _effect.EndRender();
    }

    #endregion

    public override void SetupPrimitive(PrimitiveContext primitiveContext, RenderContext renderContext)
    {
      primitiveContext.Parameters = new EffectParameters();
      CheckSingleColor();
      primitiveContext.Texture = BrushCache.Instance.GetGradientBrush(GradientStops);
      if (_singleColor)
      {

        Color4 v = ColorConverter.FromColor(GradientStops[0].Color);
        v.Alpha *= (float) renderContext.Opacity;
        primitiveContext.Effect = ContentManager.GetEffect("solidbrush");
        primitiveContext.Parameters.Add(primitiveContext.Effect.GetParameterHandle("g_solidColor"), v);
        return;
      }
      else
      {
        primitiveContext.Effect = ContentManager.GetEffect("radialgradient");
        _handleRelativeTransform = primitiveContext.Effect.GetParameterHandle("RelativeTransform");
        _handleFocus = primitiveContext.Effect.GetParameterHandle("g_focus");
        _handleCenter = primitiveContext.Effect.GetParameterHandle("g_center");
        _handleRadius = primitiveContext.Effect.GetParameterHandle("g_radius");
        _handleOpacity = primitiveContext.Effect.GetParameterHandle("g_opacity");

        g_focus = new float[] { GradientOrigin.X, GradientOrigin.Y };
        g_center = new float[] { Center.X, Center.Y };
        g_radius = new float[] { (float)RadiusX, (float)RadiusY };

        if (MappingMode == BrushMappingMode.Absolute)
        {
          g_focus[0] /= _vertsBounds.Width;
          g_focus[1] /= _vertsBounds.Height;

          g_center[0] /= _vertsBounds.Width;
          g_center[1] /= _vertsBounds.Height;

          g_radius[0] /= _vertsBounds.Width;
          g_radius[1] /= _vertsBounds.Height;
        }

// TODO: Why invert?
        Matrix m = Matrix.Invert(RelativeTransform.GetTransformRel());

        primitiveContext.Parameters.Add(_handleRelativeTransform, m);
        primitiveContext.Parameters.Add(_handleFocus, g_focus);
        primitiveContext.Parameters.Add(_handleCenter, g_center);
        primitiveContext.Parameters.Add(_handleRadius, g_radius);
        primitiveContext.Parameters.Add(_handleOpacity, (float) (Opacity * renderContext.Opacity));
      }
    }
  }
}
