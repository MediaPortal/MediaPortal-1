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

namespace MediaPortal.GUI.Library
{
  public class GUILayerManager
  {
    public enum LayerType : int
    {
      Gui = 0,
      MusicOverlay,
      VideoOverlay,
      TvOverlay,
      Video,
      WeatherOverlay,
      TopOverlay,
      Osd,
      Topbar1,
      Topbar2,
      Dialog,
      MiniEPG
    }

    // layers:
    //      [GUI] - [PREVIEW] - [VIDEO] - [OSD] - [TOPBAR1] - [TOPBAR2] - [DIALOG]

    private const int MAX_LAYERS = 15;

    private static IRenderLayer[] _layers = new IRenderLayer[MAX_LAYERS];

    public static void RegisterLayer(IRenderLayer renderer, LayerType zOrder)
    {
      _layers[(int)zOrder] = renderer;
    }

    public static void UnRegisterLayer(IRenderLayer renderer)
    {
      for (int i = 0; i < MAX_LAYERS; ++i)
      {
        if (_layers[i] == renderer)
        {
          _layers[i] = null;
        }
      }
    }

    public static IRenderLayer GetLayer(LayerType zOrder)
    {
      return _layers[(int)zOrder];
    }

    public static void Render(float timePassed)
    {
      if (GUIGraphicsContext.BlankScreen)
      {
        return;
      }
      int videoLayer = (int)LayerType.Video;
      if (GUIGraphicsContext.ShowBackground == false)
      {
        if (_layers[videoLayer] != null)
        {
          if (_layers[videoLayer].ShouldRenderLayer())
          {
            _layers[videoLayer].RenderLayer(timePassed);
            GUIFontManager.Present();
          }
        }
      }
      for (int i = 0; i < MAX_LAYERS; ++i)
      {
        if (_layers[i] != null)
        {
          if (_layers[i].ShouldRenderLayer())
          {
            if (GUIGraphicsContext.ShowBackground == false && i == videoLayer)
            {
              continue;
            }
            _layers[i].RenderLayer(timePassed);
            GUIFontManager.Present();
          }
        }
      }
    }
  }
}