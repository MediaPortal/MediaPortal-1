using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public interface IRenderLayer
  {
    bool ShouldRenderLayer();
    void RenderLayer(float timePassed);
  }
}
