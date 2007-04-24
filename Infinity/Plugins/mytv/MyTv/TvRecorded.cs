using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Threading;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using Dialogs;
using TvControl;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;
using ProjectInfinity.Plugins;
using ProjectInfinity.Navigation;
using ProjectInfinity.Controls;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvRecorded.xaml
  /// </summary>

  public partial class TvRecorded : View, IMenuCommand, IDisposable
  {

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvRecorded"/> class.
    /// </summary>
    public TvRecorded()
    {
      TvRecordedViewModel model = new TvRecordedViewModel();
      DataContext = model;
      this.InputBindings.Add(new KeyBinding(model.FullScreen, new KeyGesture(System.Windows.Input.Key.Enter, ModifierKeys.Alt)));
      this.InputBindings.Add(new KeyBinding(NavigationCommands.BrowseBack, new KeyGesture(System.Windows.Input.Key.Escape)));
    }
    #endregion

    #region IMenuCommand Members

    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new TvRecorded());
    }

    #region IDisposable Members
    public void Dispose()
    {
    }
    #endregion

    #endregion

  }
}