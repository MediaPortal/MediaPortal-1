using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.MenuManager
{
  public interface ICommandItem : IMenuItem
  {
    void Execute();
  }
}
