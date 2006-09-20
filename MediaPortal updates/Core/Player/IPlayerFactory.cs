using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Player
{
  public interface IPlayerFactory
  {
    IPlayer Create(string fileName);
    IPlayer Create(string fileName, g_Player.MediaType type);
  }
}
