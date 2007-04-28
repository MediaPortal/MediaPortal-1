using System;
using System.ComponentModel;
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
    bool FullScreen { get; set; }

    /// <summary>
    /// Gets the current window scaling.
    /// </summary>
    /// <value>The current window scaling.</value>
    Size CurrentScaling { get;}

    void Close();
  }
}