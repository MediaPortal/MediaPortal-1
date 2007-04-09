using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectInfinity.Controls
{
  public class RadioButton : System.Windows.Controls.CheckBox
  {
    protected override void OnMouseEnter(MouseEventArgs e)
    {
      base.OnMouseEnter(e);
      Keyboard.Focus(this);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        IsChecked = !IsChecked;
        e.Handled = true;
        return;
      }
      base.OnKeyDown(e);
    }
  }
}
