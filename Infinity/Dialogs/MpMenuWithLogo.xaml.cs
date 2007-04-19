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

namespace Dialogs
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>

  public partial class MpMenuWithLogo : System.Windows.Window
  {
    DialogViewModel _model;
    DialogMenuItemCollection _menuItems;
    /// <summary>
    /// Initializes a new instance of the <see cref="MpImageMenu"/> class.
    /// </summary>
    public MpMenuWithLogo()
    {
      this.WindowStyle = WindowStyle.None;
      this.ShowInTaskbar = false;
      this.ResizeMode = ResizeMode.NoResize;
      this.AllowsTransparency = true;//we need it so we can alphablend the dialog with the gui. However this causes s/w rendering in wpf
      InitializeComponent();
      _menuItems = new DialogMenuItemCollection();
      _model = new DialogViewModel(this);
    }
    public MpMenuWithLogo(DialogMenuItemCollection items)
    {
      _menuItems = items;
      this.WindowStyle = WindowStyle.None;
      this.ShowInTaskbar = false;
      this.ResizeMode = ResizeMode.NoResize;
      this.AllowsTransparency = true;
      InitializeComponent();
      _model = new DialogViewModel(this);
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      LoadSkin();
      _model.SetItems(_menuItems);
      gridMain.DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.Close, new KeyGesture(System.Windows.Input.Key.Escape)));
      this.Visibility = Visibility.Visible;
    }
    protected virtual void LoadSkin()
    {
      gridMain.Children.Clear();
      try
      {
        using (FileStream steam = new FileStream(@"skin\default\Dialogs\DialogMenuLogo.xaml", FileMode.Open, FileAccess.Read))
        {
          UIElement documentRoot = (UIElement)XamlReader.Load(steam);
          gridMain.Children.Add(documentRoot);
        }
      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Error("error loading DialogMenuLogo.xaml");
        ServiceScope.Get<ILogger>().Error(ex);
      }
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