using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Menu
{
  public interface IMenu : IMenuItem
  {
    List<IMenuItem> Items { get;}
    IMenuItem DefaultItem { get;}
  }
}
