using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ProjectInfinity.Controls
{
  public class MenuItem
  {
    FrameworkElement _content;
    MenuCollection _subMenus;
    string _label;
    string _image;

    public MenuItem()
    {
    }
    public MenuItem(string label)
    {
      Label = label;
    }

    public MenuCollection SubMenus
    {
      get
      {
        return _subMenus;
      }
      set
      {
        _subMenus = value;
      }
    }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
    public FrameworkElement Content
    {
      get
      {
        return _content;
      }
      set
      {
        _content = value;
      }
    }

    public string Label
    {
      get
      {
        return _label;
      }
      set
      {
        _label = value;
      }
    }
    public string Image
    {
      get
      {
        return _image;
      }
      set
      {
        _image = value;
      }
    }

  }
}
