using System.Windows.Input;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Menu
{
  internal class MenuView : View
  {
    public MenuView(string id)
    {
      DataContext = new MenuViewModel(id);
      InputBindings.Add(new KeyBinding(new MenuViewModel(id).FullScreen, new KeyGesture(Key.Enter, ModifierKeys.Alt)));
      InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(Key.Escape)));
    }
  }
}