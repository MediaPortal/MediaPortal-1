using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.MenuManager
{
  public interface IMenuManager
  {
    IList<IMenuItem> GetMenu(string menuPath);
  }
}
