using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Menu
{
  public interface ICommandItem : IMenuItem
  {
    void Execute();
  }
}
