using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Player
{
  public interface IPlayerFactory
  {
    IPlayer Create(string fileName);
  }
}
