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
  }
}
