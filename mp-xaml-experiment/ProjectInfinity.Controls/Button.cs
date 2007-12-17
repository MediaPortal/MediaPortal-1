using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectInfinity.Controls
{
  public class Button : System.Windows.Controls.Button
  {
    protected override void OnMouseEnter(MouseEventArgs e)
    {
      base.OnMouseEnter(e);
      Keyboard.Focus(this);
    }
  }
}
