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
    #region variables
    string _logo, _label1, _label2, _label3;
    object _tag;
    #endregion
    #region ctors
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogMenuItem"/> class.
    /// </summary>
    public DialogMenuItem()
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogMenuItem"/> class.
    /// </summary>
    /// <param name="logo">The logo.</param>
    /// <param name="label1">The label1.</param>
    /// <param name="label2">The label2.</param>
    /// <param name="label3">The label3.</param>
    public DialogMenuItem(string logo, string label1, string label2, string label3)
    {
      Logo = logo;
      Label1 = label1;
      Label2 = label2;
      Label3 = label3;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogMenuItem"/> class.
    /// </summary>
    /// <param name="buttonName">Name of the button.</param>
    public DialogMenuItem(string buttonName)
    {
      Label1 = buttonName;
    }
    #endregion

    #region properties
    public string Logo
    {
      get
      {
        return _logo;
      }
      set
      {
        _logo = value;
      }
    }
    public string Label1
    {
      get
      {
        return _label1;
      }
      set
      {
        _label1 = value;
      }
    }
    public string Label2
    {
      get
      {
        return _label2;
      }
      set
      {
        _label2 = value;
      }
    }
    public string Label3
    {
      get
      {
        return _label3;
      }
      set
      {
        _label3 = value;
      }
    }
    public object Tag
    {
      get
      {
        return _tag;
      }
      set
      {
        _tag = value;
      }
    }
    #endregion
  }
}
