using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Menu
{
  public interface IPluginItem : IMenuItem
  {
    void Execute();

    string Description { get;}

    string ImagePath { get; }
  }
}
