using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public class GUILayerRenderer : IRenderLayer    
  {

    public GUILayerRenderer()
    {
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Gui);
    }

    public bool ShouldRenderLayer()
    {
      if (GUIGraphicsContext.IsFullScreenVideo) return false;
      return true;
    }
    public void RenderLayer(float timePassed)
    {
      GUIWindowManager.Render(timePassed);
    }
  }
}
