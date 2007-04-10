using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace ProjectInfinity.Navigation
{
  public interface INavigationService
  {
    bool Navigate(Uri toPage);
    bool Navigate(object root);
    void GoBack();
    Window GetWindow();
    event CancelEventHandler Closing;
  }
}
