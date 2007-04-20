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
      IThemeManager themeMgr = ServiceScope.Get<IThemeManager>();
      Resources = themeMgr.LoadResources(this);
      Background = Application.Current.Resources["backGroundBrush"] as Brush;
      Content = themeMgr.LoadContent(this);
    }
  }
}