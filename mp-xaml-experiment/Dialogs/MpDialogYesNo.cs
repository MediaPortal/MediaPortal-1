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
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;

namespace Dialogs
{
  /// <summary>
  /// Interaction logic for MpDialogYesNo.xaml
  /// </summary>

  public partial class MpDialogYesNo : ViewWindow
  {
    DialogViewModel _model;
    DialogResult _result = DialogResult.No;

    public MpDialogYesNo()
    {
      this.Visibility = Visibility.Visible;
      this.BorderThickness = new Thickness(0);
      this.Width = 530;
      this.Height = 370;

      _model = new DialogViewModel(this);
      Size scaling = ServiceScope.Get<INavigationService>().CurrentScaling;
      this.Width *= scaling.Width;
      this.Height *= scaling.Height;
      DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.Close, new KeyGesture(System.Windows.Input.Key.Escape)));
    }
    /// <summary>
    /// Shows this instance.
    /// </summary>


    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      if (base.Content != null)
      {
        Size scaling = ServiceScope.Get<INavigationService>().CurrentScaling;
        ((FrameworkElement)base.Content).LayoutTransform = new ScaleTransform(scaling.Width, scaling.Height);
      }
    }
    protected override void OnContentChanged(object oldContent, object newContent)
    {
      base.OnContentChanged(oldContent, newContent);
      Size scaling = ServiceScope.Get<INavigationService>().CurrentScaling;
      ((FrameworkElement)base.Content).LayoutTransform = new ScaleTransform(scaling.Width, scaling.Height);
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