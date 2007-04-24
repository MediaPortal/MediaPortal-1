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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TvDatabase;
using TvControl;
using Dialogs;
using MCEControls;
using ProjectInfinity;
using ProjectInfinity.Players;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvNewSchedule.xaml
  /// </summary>

  public partial class TvSearch : View
  {
    static TvSearchViewModel.SearchType _searchType = TvSearchViewModel.SearchType.Title;
    public TvSearch()
    {
      TvSearchViewModel model = new TvSearchViewModel();
      DataContext = model;
      model.SearchMode = _searchType;
      this.InputBindings.Add(new KeyBinding(model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
    }
    static public TvSearchViewModel.SearchType SearchMode
    {
      get
      {
        return _searchType;
      }
      set
      {
        _searchType = value;
      }
    }
  }
}