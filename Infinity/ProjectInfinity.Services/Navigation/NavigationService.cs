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

    #region INavigationService Members

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
      //base.OnRenderSizeChanged(sizeInfo);
      //if (originalSize.Equals(Size.Empty))
      //{
      //  originalSize = sizeInfo.NewSize;
      //  return;
      //}
      //double scaleX = sizeInfo.NewSize.Width/originalSize.Width;
      //double scaleY = sizeInfo.NewSize.Height / originalSize.Height;
      //((UIElement) this.Content).RenderTransform = new ScaleTransform(scaleX, scaleY);
    }

  }
}