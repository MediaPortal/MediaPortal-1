using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.GUI.Library
{
  public interface IPlayerService
  {
    void RegisterService(int iWindowID);

    PlayerServiceResultEnum GetPlaybackLink(string strLink, out object result);
  }
}
