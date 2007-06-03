using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.MenuManager
{
  public interface IPluginItem : IMenuItem
  {
    void Execute();

    string Description { get;}

  }
}
