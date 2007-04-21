using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ProjectInfinity.Navigation
{
  public class NavigationService : NavigationWindow, INavigationService, INotifyPropertyChanged
  {
    private bool _fullScreen = false;
    private Size originalSize = Size.Empty;
    double _scaleX = 1.0;
    double _scaleY = 1.0;

    public NavigationService()
    {
      this.Width = 800;
      this.Height = 600;
      
    }



    #region INavigationService Members

    /// <summary>
    /// Gets the current window scaling.
    /// </summary>
    /// <value>The current window scaling.</value>
    public Size CurrentScaling 
    {
      get
      {
        return new Size(_scaleX, _scaleY);
      }
    }

    public bool FullScreen
    {
      get { return _fullScreen; }
      set
      {
        if (_fullScreen == value)
        {
          return;
        }
        _fullScreen = value;
        WindowStyle = _fullScreen ? WindowStyle.None : WindowStyle.ThreeDBorderWindow;
        WindowState = _fullScreen ? WindowState.Maximized : WindowState.Normal;
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs("FullScreen"));
        }
      }
    }

    public Window GetWindow()
    {
      return this;
    }

    #endregion

    #region INotifyPropertyChanged Members

    ///<summary>
    ///Occurs when a property value changes.
    ///</summary>
    ///
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
     base.OnRenderSizeChanged(sizeInfo);
     if (originalSize.Equals(Size.Empty))
     {
       originalSize = sizeInfo.NewSize;
       return;
     }
      _scaleX= sizeInfo.NewSize.Width/originalSize.Width;
      _scaleY= sizeInfo.NewSize.Height / originalSize.Height;
     ((FrameworkElement) this.Content).LayoutTransform  = new ScaleTransform(_scaleX, _scaleY);
    }
    protected override void OnContentChanged(object oldContent, object newContent)
    {
      base.OnContentChanged(oldContent, newContent);
      ((FrameworkElement)this.Content).LayoutTransform = new ScaleTransform(_scaleX, _scaleY);
    }

  }
}