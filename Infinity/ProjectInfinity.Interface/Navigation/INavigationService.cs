using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ProjectInfinity.Navigation
{
  public interface INavigationService
  {
    bool Navigate(Uri toPage);
    Window GetWindow();
  }
}
