using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dialogs;
using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;
using ProjectInfinity.Plugins;
namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvGuide.xaml
  /// </summary>

  public partial class TvGuide : View, IMenuCommand, IDisposable
  {

    #region IMenuCommand Members

    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new TvGuide());
    }

    #region IDisposable Members
    public void Dispose()
    {
    }
    #endregion

    #endregion
    #region ctor
    public TvGuide()
    {
      TvGuideViewModel model;
      if (ServiceScope.IsRegistered<TvGuideViewModel>())
      {
        model = ServiceScope.Get<TvGuideViewModel>();
      }
      else
      {
        model = new TvGuideViewModel();
        ServiceScope.Add<TvGuideViewModel>(model);
      }
      model.Reload();
      DataContext = model;
      this.InputBindings.Add(new KeyBinding(model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      this.Loaded += new RoutedEventHandler(TvGuide_Loaded);
    }

    void TvGuide_Loaded(object sender, RoutedEventArgs e)
    {
      TvGuideViewModel model;
      model = ServiceScope.Get<TvGuideViewModel>();
      model.Reload();
    }
    
    #endregion


  }
}