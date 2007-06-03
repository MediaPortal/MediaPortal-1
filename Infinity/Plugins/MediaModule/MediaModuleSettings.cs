using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjectInfinity;
using ProjectInfinity.Navigation;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Controls;
using ProjectInfinity.Plugins;
using ProjectInfinity.MenuManager;

namespace MediaModule
{
    class MediaModuleSettings : View, IMenuCommand, IDisposable
    {
        public MediaModuleSettings()
        {
            SettingsViewModel _model = new SettingsViewModel();
            DataContext = _model;
            this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
        }
        public void Run()
        {
            ServiceScope.Get<INavigationService>().Navigate(new MediaModuleSettings());
        }

        public void Dispose()
        {
        }
    }
}
