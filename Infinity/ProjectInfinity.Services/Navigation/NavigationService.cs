using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Navigation;

namespace ProjectInfinity.Navigation
{
  public class NavigationService : NavigationWindow, INavigationService
  {
    #region INavigationService Members

    public Window GetWindow()
    {
      return this;
    }

    #endregion
  }
}
