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
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Controls.Visuals;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  public class VisualBrush : TileBrush
  {
    #region Private fields

    AbstractProperty _visualProperty;
    EffectAsset _effect;
    Texture _textureVisual;

    #endregion

    #region Ctor

    public VisualBrush()
    {
      Init();
    }

    void Init()
    {
      _visualProperty = new SProperty(typeof(FrameworkElement), null);
      _effect = ContentManager.GetEffect("normal");
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      VisualBrush b = (VisualBrush) source;
      Visual = b.Visual; // Use the original Visual. Why should we use a copy?
    }

    #endregion

    #region Public properties

    public AbstractProperty VisualProperty
    {
      get { return _visualProperty; }
    }

    public FrameworkElement Visual
    {
      get { return (FrameworkElement)_visualProperty.GetValue(); }
      set { _visualProperty.SetValue(value); }
    }

    #endregion

    public override void SetupBrush(RectangleF bounds, float zOrder, PositionColored2Textured[] verts)
    {
      base.SetupBrush(bounds, zOrder, verts);
      _textureVisual = new Texture(GraphicsDevice.Device, (int) _vertsBounds.Width, (int ) _vertsBounds.Height, 1,
          Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
    }

    public override bool BeginRenderBrush(PrimitiveContext primitiveContext, RenderContext renderContext)
    {
      if (Visual == null) return false;

      // TODO: Is this correct?
      RenderContext tempRenderContext = renderContext.Derive(_vertsBounds, null, RelativeTransform.GetTransform(), new Vector2(0.5f, 0.5f), Opacity);

      Visual.RenderToTexture(_textureVisual, tempRenderContext);

      // Now render our texture
      _effect.StartRender(_textureVisual, Transform == null ? Matrix.Identity : Transform.GetTransform());

      return true;
    }

    public override void EndRender()
    {
      if (Visual != null)
        _effect.EndRender();
    }
  }
}
