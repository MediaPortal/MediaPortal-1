using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dialogs
{
  public class DialogMenuItem
  {
    List<UIElement> _subItems = new List<UIElement>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogMenuItem"/> class.
    /// </summary>
    /// <param name="buttonName">Name of the button.</param>
    public DialogMenuItem(string buttonName)
    {
      Button b = new Button();
      b.Content = buttonName;
      b.Template = (ControlTemplate)Application.Current.Resources["MpButton"];
      _subItems.Add(b);
    }

    /// <summary>
    /// Gets or sets the sub items.
    /// </summary>
    /// <value>The sub items.</value>
    public List<UIElement> SubItems
    {
      get
      {
        return _subItems;
      }
      set
      {
        _subItems = value;
      }
    }
  }
}
