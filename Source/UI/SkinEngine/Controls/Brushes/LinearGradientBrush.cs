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

using System;
using System.Drawing;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  // TODO: Implement Freezable behaviour
  public class LinearGradientBrush : GradientBrush
  {
    #region Private fields

    EffectAsset _effect;

    AbstractProperty _startPointProperty;
    AbstractProperty _endPointProperty;
    bool _refresh = false;
    bool _singleColor = true;
    EffectHandleAsset _handleRelativeTransform;
    EffectHandleAsset _handleOpacity;
    EffectHandleAsset _handleStartPoint;
    EffectHandleAsset _handleEndPoint;
    EffectHandleAsset _handleSolidColor;
    EffectHandleAsset _handleAlphaTexture;
    GradientBrushTexture _gradientBrushTexture;

    #endregion

    #region Ctor

    public LinearGradientBrush()
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
      _startPointProperty = new SProperty(typeof(Vector2), new Vector2(0.0f, 0.0f));
      _endPointProperty = new SProperty(typeof(Vector2), new Vector2(1.0f, 1.0f));
    }

    void Attach()
    {
      _startPointProperty.Attach(OnPropertyChanged);
      _endPointProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _startPointProperty.Detach(OnPropertyChanged);
      _endPointProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      LinearGradientBrush b = (LinearGradientBrush) source;
      StartPoint = copyManager.GetCopy(b.StartPoint);
      EndPoint = copyManager.GetCopy(b.EndPoint);
      Attach();
    }

    #endregion

    protected override void OnPropertyChanged(AbstractProperty prop, object oldValue)
    {
      _refresh = true;
      FireChanged();
    }

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

    public AbstractProperty StartPointProperty
    {
      get { return _startPointProperty; }
    }

    public Vector2 StartPoint
    {
      get { return (Vector2)_startPointProperty.GetValue(); }
      set { _startPointProperty.SetValue(value); }
    }

    public AbstractProperty EndPointProperty
    {
      get { return _endPointProperty; }
    }

    public Vector2 EndPoint
    {
      get { return (Vector2)_endPointProperty.GetValue(); }
      set { _endPointProperty.SetValue(value); }
    }

    public override void SetupBrush(RectangleF bounds, ExtendedMatrix layoutTransform, float zOrder, PositionColored2Textured[] verts)
    {
      UpdateBounds(bounds, layoutTransform, verts);
      base.SetupBrush(bounds, layoutTransform, zOrder, verts);

      if (_gradientBrushTexture == null)
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
      _refresh = true;
    }

    public override bool BeginRender(PrimitiveContext primitiveContext)
    {
      if (Transform != null)
      {
        ExtendedMatrix mTrans;
        Transform.GetTransform(out mTrans);
        SkinContext.AddRenderTransform(mTrans);
      }
      if (_gradientBrushTexture == null)
        return false;
      if (_refresh)
      {
        _refresh = false;
        CheckSingleColor();
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
        if (_singleColor)
        {
          _effect = ContentManager.GetEffect("solidbrush");
          _handleSolidColor = _effect.GetParameterHandle("g_solidColor");
        }
        else
        {
          _effect = ContentManager.GetEffect("lineargradient");
          _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
          _handleOpacity = _effect.GetParameterHandle("g_opacity");
          _handleStartPoint = _effect.GetParameterHandle("g_StartPoint");
          _handleEndPoint = _effect.GetParameterHandle("g_EndPoint");
        }
      }

      float[] g_startpoint = new float[] { StartPoint.X, StartPoint.Y };
      float[] g_endpoint = new float[] { EndPoint.X, EndPoint.Y };
      if (MappingMode == BrushMappingMode.Absolute)
      {
        g_startpoint[0] = ((StartPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
        g_startpoint[1] = ((StartPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

        g_endpoint[0] = ((EndPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
        g_endpoint[1] = ((EndPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;
      }

      if (_singleColor)
      {
        Color4 v = ColorConverter.FromColor(GradientStops.OrderedGradientStopList[0].Color);
        _handleSolidColor.SetParameter(v);
        _effect.StartRender(null);
      }
      else
      {
        Matrix m;
        RelativeTransform.GetTransformRel(out m);
        m = Matrix.Invert(m);

        _handleRelativeTransform.SetParameter(m);
        _handleOpacity.SetParameter((float) (Opacity * SkinContext.Opacity));
        _handleStartPoint.SetParameter(g_startpoint);
        _handleEndPoint.SetParameter(g_endpoint);
        _effect.StartRender(_gradientBrushTexture.Texture);
      }
      return true;
    }

    public override void BeginRender(Texture tex)
    {
      if (Transform != null)
      {
        ExtendedMatrix mTrans;
        Transform.GetTransform(out mTrans);
        SkinContext.AddRenderTransform(mTrans);
      }
      if (tex == null)
        return;
      if (_refresh)
      {
        _refresh = false;
        CheckSingleColor();
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
        _effect = ContentManager.GetEffect("linearopacitygradient");
        _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
        _handleOpacity = _effect.GetParameterHandle("g_opacity");
        _handleStartPoint = _effect.GetParameterHandle("g_StartPoint");
        _handleEndPoint = _effect.GetParameterHandle("g_EndPoint");
        _handleAlphaTexture = _effect.GetParameterHandle("g_alphatex");
      }

      float[] g_startpoint = new float[] { StartPoint.X, StartPoint.Y };
      float[] g_endpoint = new float[] { EndPoint.X, EndPoint.Y };
      if (MappingMode == BrushMappingMode.Absolute)
      {
        g_startpoint[0] = ((StartPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
        g_startpoint[1] = ((StartPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

        g_endpoint[0] = ((EndPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
        g_endpoint[1] = ((EndPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;
      }

      if (_singleColor)
        _effect.StartRender(null);
      else
      {
        Matrix m;
        RelativeTransform.GetTransformRel(out m);
        m = Matrix.Invert(m);
        _handleRelativeTransform.SetParameter(m);
        _handleOpacity.SetParameter((float) (Opacity * SkinContext.Opacity));
        _handleStartPoint.SetParameter(g_startpoint);
        _handleEndPoint.SetParameter(g_endpoint);
        _handleAlphaTexture.SetParameter(_gradientBrushTexture.Texture);

        _effect.StartRender(tex);
      }
    }

    public override void EndRender()
    {
      if (_effect != null)
        _effect.EndRender();
      if (Transform != null)
        SkinContext.RemoveRenderTransform();
    }

    public override Texture Texture
    {
      get { return _gradientBrushTexture.Texture; }
    }

    public override void SetupPrimitive(PrimitiveContext context)
    {
      context.Parameters = new EffectParameters();
      CheckSingleColor();
      _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
      if (_singleColor)
      {
        Color4 v = ColorConverter.FromColor(GradientStops.OrderedGradientStopList[0].Color);
        v.Alpha *= (float) SkinContext.Opacity;
        context.Effect = ContentManager.GetEffect("solidbrush");
        context.Parameters.Add(context.Effect.GetParameterHandle("g_solidColor"), v);
        return;
      }
      else
      {
        context.Effect = ContentManager.GetEffect("lineargradient");
        _handleRelativeTransform = context.Effect.GetParameterHandle("RelativeTransform");
        _handleOpacity = context.Effect.GetParameterHandle("g_opacity");
        _handleStartPoint = context.Effect.GetParameterHandle("g_StartPoint");
        _handleEndPoint = context.Effect.GetParameterHandle("g_EndPoint");
      }

      float[] g_startpoint = new float[] { StartPoint.X, StartPoint.Y };
      float[] g_endpoint = new float[] { EndPoint.X, EndPoint.Y };
      if (MappingMode == BrushMappingMode.Absolute)
      {
        g_startpoint[0] = ((StartPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
        g_startpoint[1] = ((StartPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

        g_endpoint[0] = ((EndPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
        g_endpoint[1] = ((EndPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;
      }

      Matrix m;
      RelativeTransform.GetTransformRel(out m);
      m = Matrix.Invert(m);

      context.Parameters.Add(_handleRelativeTransform, m);
      context.Parameters.Add(_handleOpacity, (float) (Opacity * SkinContext.Opacity));
      context.Parameters.Add(_handleStartPoint, g_startpoint);
      context.Parameters.Add(_handleEndPoint, g_endpoint);
      context.Texture = _gradientBrushTexture;
    }
  }
}
