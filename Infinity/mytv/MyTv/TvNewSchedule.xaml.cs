using System;
using System.Collections;
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
using TvDatabase;
using TvControl;
using Dialogs;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvNewSchedule.xaml
  /// </summary>

  public partial class TvNewSchedule : System.Windows.Controls.Page
  {
    TvScheduledViewModel _model;
    public TvNewSchedule()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _model = new TvScheduledViewModel(this);
      gridMain.DataContext = _model;
    }
  }
}