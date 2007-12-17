using System.Windows.Input;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Menu
{
  internal class MenuView : View
  {
    public MenuView(string id)
    {
      MenuViewModel model = new MenuViewModel(id);
      DataContext = model;
      InputBindings.Add(new KeyBinding(model.FullScreen, new KeyGesture(Key.Enter, ModifierKeys.Alt)));
      InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(Key.Escape)));
    }
  }
}