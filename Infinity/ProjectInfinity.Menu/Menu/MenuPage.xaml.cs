using System.Windows;
using System.Windows.Controls;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Menu.View
{
  /// <summary>
  /// Interaction logic for MenuPage.xaml
  /// </summary>
  public partial class MenuPage : Page
  {
    public MenuPage()
    {
      InitializeComponent();
      IMenuManager mgr = ServiceScope.Get<IMenuManager>();
      trvMenu.ItemsSource = mgr.GetMenu();
    }

    private  void Clicked(object sender, RoutedEventArgs e)
    {
      TextBlock b = sender as TextBlock;
      if (b==null)
        return;
      ServiceScope.Get<IPluginManager>().Start(b.Text);
      
    }
  }
}