using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProjectInfinity.Themes;

namespace ProjectInfinity.Controls
{
  public abstract class View : Page
  {
    public View()
    {
      ShowsNavigationUI = false;
      //Load XAML code.  See remark below
      Loaded += View_Loaded;
    }

    /// <remarks>
    /// The Page content must be loaded from the <see cref="Page.Loaded"/> event to make sure
    /// the XAML is reloaded when we navigate back to the Page.  This is necessary to rerun any
    /// animations scheduled to run when entering/leaving the page.
    /// </remarks>
    void View_Loaded(object sender, RoutedEventArgs e)
    {
      IThemeManager themeMgr = ServiceScope.Get<IThemeManager>();
      Resources = themeMgr.LoadResources(this);
      Background = Application.Current.Resources["backGroundBrush"] as Brush;
      Content = themeMgr.LoadContent(this);
    }
  }
}