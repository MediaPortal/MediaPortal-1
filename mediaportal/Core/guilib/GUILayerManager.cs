using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public class GUILayerManager
  {
    // layers:
    // when in GUI
    //      [GUI] - [PREVIEW] - [TOPBAR] - [DIALOG] 
    // when in fullscreen tv
    //      [VIDEO] - [OSD] - - [TOPBAR] - [DIALOG]

    const int MAX_LAYERS = 10;

    static IRenderLayer[] _layers = new IRenderLayer[MAX_LAYERS];

    static public void RegisterLayer(IRenderLayer renderer, int zOrder)
    {
      _layers[zOrder] = renderer;
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

      for (int i = 0; i < MAX_LAYERS; ++i)
      {
        if (_layers[i] !=null)
        {
          _layers[i].RenderLayer(timePassed);
        }
      }
    }
  }
}
