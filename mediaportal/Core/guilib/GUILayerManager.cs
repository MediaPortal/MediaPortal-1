using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public class GUILayerManager
  {
    public enum LayerType:int
    {
      Gui=0,
      MusicOverlay,
      VideoOverlay,
      TvOverlay,
      Video,
      TopOverlay,
      Osd,
      Topbar1,
      Topbar2,
      Dialog
    }
    // layers:
    //      [GUI] - [PREVIEW] - [VIDEO] - [OSD] - [TOPBAR1] - [TOPBAR2] - [DIALOG]

    const int MAX_LAYERS = 15;

    static IRenderLayer[] _layers = new IRenderLayer[MAX_LAYERS];

    static public void RegisterLayer(IRenderLayer renderer, LayerType zOrder)
    {
      _layers[(int)zOrder] = renderer;
    }

    static public void UnRegisterLayer(IRenderLayer renderer)
    {
      for (int i = 0; i < MAX_LAYERS; ++i)
      {
        if (_layers[i] == renderer)
        {
          _layers[i] = null;
        }
      }
    }
    static public void Render(float timePassed)
    {
      if (GUIGraphicsContext.BlankScreen) return;

      for (int i = 0; i < MAX_LAYERS; ++i)
      {
        if (_layers[i] !=null)
        {
          if (_layers[i].ShouldRenderLayer())
          {
            _layers[i].RenderLayer(timePassed);
            GUIFontManager.Present();
          }
        }
      }
    }
  }
}
