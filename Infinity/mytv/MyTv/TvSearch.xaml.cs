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

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvNewSchedule.xaml
  /// </summary>

  public partial class TvSearch : System.Windows.Controls.Page
  {
    TvSearchViewModel _model;
    static TvSearchViewModel.SearchType _searchType = TvSearchViewModel.SearchType.Title;
    public TvSearch()
    {
      InitializeComponent();
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
    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      gridMain.Children.Clear();
      using (FileStream steam = new FileStream(@"skin\default\mytv\tvsearch.xaml", FileMode.Open, FileAccess.Read))
      {
        UIElement documentRoot = (UIElement)XamlReader.Load(steam);
        gridMain.Children.Add(documentRoot);
      }
      _model = new TvSearchViewModel(this);
      gridMain.DataContext = _model;
      _model.SearchMode = _searchType;
      this.InputBindings.Add(new KeyBinding(_model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onPreviewKeyDown));
    }
    /// <summary>
    /// Event handler for OnKeyDown
    /// Handles some basic navigation
    /// Guess this should be done via command binding?
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    protected void onPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.X)
      {
        ICommand command = _model.FullScreenTv;
        if (command.CanExecute(this))
        {
          command.Execute(this);
          e.Handled = true;
        }
        return;
      }
    }


  }
}