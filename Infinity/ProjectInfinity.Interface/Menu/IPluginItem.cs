using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Menu
{
  public interface IPluginItem : IMenuItem
  {
    string Description { get;}
  }
}
