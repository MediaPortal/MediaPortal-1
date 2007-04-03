namespace ProjectInfinity.Menu.View
{
  /// <summary>
  /// Interaction logic for MenuPage.xaml
  /// </summary>
  public partial class MenuPage
  {
    public MenuPage()
    {
      InitializeComponent();
      DataContext = new MenuItemViewModel(ServiceScope.Get<IMenuManager>().GetMenu());
    }
  }
}