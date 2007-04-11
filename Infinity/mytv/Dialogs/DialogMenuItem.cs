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
    string _logo = "", _label1 = "", _label2 = "", _label3 = "";
    string _recordingLogo = "";
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

    public override bool Equals(object obj)
    {
      DialogMenuItem item = obj as DialogMenuItem;
      if (item == null)
      {
        if (obj as ListBoxItem != null)
        {
          ListBoxItem box = (ListBoxItem)obj;
          item = (DialogMenuItem)box.Content as DialogMenuItem;
        }
        else return false;
      }
      if (item == null) return false;
      if (_logo != item._logo) return false;
      if (_label1 != item._label1) return false;
      if (_label2 != item._label2) return false;
      if (_label3 != item._label3) return false;
      if (_recordingLogo != item._recordingLogo) return false;
      return true;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _logo.GetHashCode() ^ _label1.GetHashCode() ^ _label2.GetHashCode() ^ _label3.GetHashCode() & _recordingLogo.GetHashCode();
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
    public string RecordingLogo
    {
      get
      {
        return _recordingLogo;
      }
      set
      {
        _recordingLogo = value;
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
