using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Menu
{
  public interface IMenuManager
  {
    IList<IMenuItem> GetMenu(string menuPath);
  }
}
