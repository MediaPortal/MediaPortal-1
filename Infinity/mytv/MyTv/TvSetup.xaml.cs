using System;
using System.Xml;
using System.Collections;
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
using TvControl;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Navigation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvSetup.xaml
  /// </summary>
  /// 
  public partial class TvSetup : System.Windows.Controls.Page
  {
    TvSetupModel _model;
    /// <summary>
    /// Initializes a new instance of the <see cref="TvSetup"/> class.
    /// </summary>
    public TvSetup()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Called when page gets loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      gridMain.Children.Clear();
      try
      {
        using (FileStream steam = new FileStream(@"skin\default\mytv\TvSetup.xaml", FileMode.Open, FileAccess.Read))
        {
          UIElement documentRoot = (UIElement)XamlReader.Load(steam);
          gridMain.Children.Add(documentRoot);
        }
      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Error("error loading TvSetup.xaml");
        ServiceScope.Get<ILogger>().Error(ex);
      }
      _model = new TvSetupModel();
      gridMain.DataContext = _model;
      ServiceScope.Get<ILogger>().Info("mytv:setuptv");

    }
  }
}