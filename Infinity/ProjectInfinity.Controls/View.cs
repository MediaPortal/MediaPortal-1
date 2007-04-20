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
      Loaded += new RoutedEventHandler(View_Loaded);
    }

    void View_Loaded(object sender, RoutedEventArgs e)
    {
      IThemeManager themeMgr = ServiceScope.Get<IThemeManager>();
      Resources = themeMgr.LoadResources(this);
      Background = Application.Current.Resources["backGroundBrush"] as Brush;
      Content = themeMgr.LoadContent(this);
    }
  }
}