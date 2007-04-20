using System.Windows.Input;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Menu
{
  internal class MenuView : View
  {
    public MenuView()
    {
      DataContext = new MenuViewModel();
      InputBindings.Add(new KeyBinding(new MenuViewModel().FullScreen, new KeyGesture(Key.Enter, ModifierKeys.Alt)));
      InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(Key.Escape)));
    }
  }
}