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
using ProjectInfinity.Menu;
using ProjectInfinity.Messaging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Players;
using ProjectInfinity.Controls;
using System.IO;
using System.Windows.Markup;

namespace MediaModule
{
    class MediaHome : View
    {
        MediaViewModel _model;

        public MediaHome()
        {
            this.Unloaded += new RoutedEventHandler(MediaHome_Unloaded);
            this.Loaded += new RoutedEventHandler(MediaHome_Loaded);
        }

        void MediaHome_Loaded(object sender, RoutedEventArgs e)
        {
            _model = new MediaViewModel();
            DataContext = _model;
            this.InputBindings.Add(new KeyBinding(_model.Back, new KeyGesture(System.Windows.Input.Key.Back)));
            this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
        }

        void MediaHome_Unloaded(object sender, RoutedEventArgs e)
        {
            _model.Dispose();
            _model = null;
        }
    }
}
