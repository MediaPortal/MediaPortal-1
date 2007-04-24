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
using ProjectInfinity.Controls;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvSetup.xaml
  /// </summary>
  /// 
  public partial class TvSetup : View
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="TvSetup"/> class.
    /// </summary>
    public TvSetup()
    {
      TvSetupModel model = new TvSetupModel();
      DataContext = model;
    }
  }
}