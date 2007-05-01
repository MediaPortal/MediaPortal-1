using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dialogs;
using TvDatabase;
using TvControl;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvProgramInfo.xaml
  /// </summary>

  public partial class TvProgramInfo : View
  {
    TvScheduledViewModel model;
    public TvProgramInfo()
    {
      this.Loaded += new RoutedEventHandler(TvProgramInfo_Loaded);
      this.Unloaded += new RoutedEventHandler(TvProgramInfo_Unloaded);
    }

    void TvProgramInfo_Unloaded(object sender, RoutedEventArgs e)
    {
      model.Dispose();
      model = null;
    }

    void TvProgramInfo_Loaded(object sender, RoutedEventArgs e)
    {
      model = ServiceScope.Get<TvScheduledViewModel>();
      DataContext = model;

      // Sets keyboard focus on the first Button in the sample.
      //this.InputBindings.Add(new KeyBinding(model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      this.InputBindings.Add(new KeyBinding(model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
    }

  }
}