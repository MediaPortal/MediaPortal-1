using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Controls;

using Dialogs;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvConflictDialog.xaml
  /// </summary>

  public partial class TvConflictDialog : ViewWindow
  {
    TvConflictDialogViewModel _model;
    DialogMenuItemCollection _menuItems;
    /// <summary>
    /// Initializes a new instance of the <see cref="MpImageMenu"/> class.
    /// </summary>
    public TvConflictDialog()
    {
      //this.AllowsTransparency = true;//we need it so we can alphablend the dialog with the gui. However this causes s/w rendering in wpf
      _menuItems = new DialogMenuItemCollection();
      _model = new TvConflictDialogViewModel(this);
      _model.SetItems(_menuItems);
      DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.Close, new KeyGesture(System.Windows.Input.Key.Escape)));
      this.Visibility = Visibility.Visible;
      this.BorderThickness = new Thickness(0);
      this.Width = 530;
      this.Height = 370;
    }
    public TvConflictDialog(DialogMenuItemCollection items)
    {
      _menuItems = items;
      //this.AllowsTransparency = true;
      _model = new TvConflictDialogViewModel(this);
      _model.SetItems(_menuItems);
      DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.Close, new KeyGesture(System.Windows.Input.Key.Escape)));
      this.Visibility = Visibility.Visible;
      this.BorderThickness = new Thickness(0);
      this.Width = 530;
      this.Height = 370;
    }


    public string SubTitle
    {
      get
      {
        return _model.Title;
      }
      set
      {
        _model.Title = value;
      }
    }

    public string Header
    {
      get
      {
        return _model.Header;
      }
      set
      {
        _model.Header = value;
      }
    }
    /// <summary>
    /// Shows this instance.
    /// </summary>

    /// <summary>
    /// Gets or sets the index of the selected.
    /// </summary>
    /// <value>The index of the selected.</value>
    public int SelectedIndex
    {
      get
      {
        return _model.SelectedIndex;
      }
      set
      {
        _model.SetSelectedIndex(value);
      }
    }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    /// <value>The selected item.</value>
    public DialogMenuItem SelectedItem
    {
      get
      {
        if (SelectedIndex < 0) return null;
        return _menuItems[SelectedIndex];
      }
    }

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <value>The items.</value>
    public DialogMenuItemCollection Items
    {
      get
      {
        return _menuItems;
      }
      set
      {
        _menuItems = value;
      }
    }

  }
}