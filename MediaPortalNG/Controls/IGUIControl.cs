using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace MediaPortal
{
  public interface IGUIControl
  {
    /// <summary>
    /// Gets the GUIControl-Label
    /// </summary>
    string Label { get;set;}
    /// <summary>
    /// Gets the GUIControl-ID
    /// </summary>
    int ID { get;}

    int OnUp { get;}
    int OnDown { get;}
    int OnLeft { get;}
    int OnRight { get;}
  }
}
