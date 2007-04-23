using System;
using System.Collections.Generic;
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
  public class FolderDialog : ViewWindow
  {
    FolderViewModel _model;
    public FolderDialog()
    {
      this.Visibility = Visibility.Visible;
      this.BorderThickness = new Thickness(0);
      this.Width = 530;
      this.Height = 370;
      _model = new FolderViewModel(this);
      Size scaling = ServiceScope.Get<INavigationService>().CurrentScaling;
      this.Width *= scaling.Width;
      this.Height *= scaling.Height;
      WindowStartupLocation = WindowStartupLocation.CenterOwner;
      DataContext = _model;
      this.InputBindings.Add(new KeyBinding(_model.Close, new KeyGesture(System.Windows.Input.Key.Escape)));
    }
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
    /// Gets the folders.
    /// </summary>
    /// <value>The folders.</value>
    public List<Folder> Folders
    {
      get
      {
        return _model.SelectedFolders;
      }
    }
  }
}
