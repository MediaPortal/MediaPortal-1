using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Player
{
  public class DummyPlayerFactory : IPlayerFactory
  {
    public IPlayer Create(string fileName)
    {
      return new DummyPlayer(fileName);
    }
    public IPlayer Create(string fileName,g_Player.MediaType type)
    {
      return new DummyPlayer(fileName);
    }
  }
}
