using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProjectInfinity;
using ProjectInfinity.Logging;

namespace Dialogs
{
  /// <summary>
  /// Interaction logic for MpDialogYesNo.xaml
  /// </summary>

  public partial class MpDialogYesNo : System.Windows.Window
  {
    DialogViewModel _model;
    DialogResult _result = DialogResult.No;

    public MpDialogYesNo()
    {
      this.WindowStyle = WindowStyle.None;
      this.ShowInTaskbar = false;
      this.ResizeMode = ResizeMode.NoResize;
      this.AllowsTransparency = true;//we need it so we can alphablend the dialog with the gui. However this causes s/w rendering in wpf
      InitializeComponent();
      _model = new DialogViewModel(this);
    }
    /// <summary>
    /// Shows this instance.
    /// </summary>

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      gridMain.Children.Clear();
      try
      {
        using (FileStream steam = new FileStream(@"skin\default\Dialogs\DialogYesNo.xaml", FileMode.Open, FileAccess.Read))
        {
          UIElement documentRoot = (UIElement)XamlReader.Load(steam);
          gridMain.Children.Add(documentRoot);
        }
      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Error("error loading DialogYesNo.xaml");
        ServiceScope.Get<ILogger>().Error(ex);
      }
      gridMain.DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.Close, new KeyGesture(System.Windows.Input.Key.Escape)));


    }

    public DialogResult DialogResult
    {
      get
      {
        return _model.DialogResult;
      }
    }
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title
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
    /// <summary>
    /// Gets or sets the header.
    /// </summary>
    /// <value>The header.</value>
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
    /// Gets or sets the content.
    /// </summary>
    /// <value>The content.</value>
    public string Content
    {
      get
      {
        return _model.Content;
      }
      set
      {
        _model.Content = value;
      }
    }
  }
}