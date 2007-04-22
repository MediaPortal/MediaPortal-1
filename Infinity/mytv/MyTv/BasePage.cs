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
  public class BasePage : System.Windows.Controls.Page, IMenuCommand, IDisposable
  {
    #region IMenuCommand Members
    public BasePage()
    {
      this.Loaded+=new RoutedEventHandler(BasePage_Loaded);
    }

    public void Run()
    {
      ServiceScope.Get<INavigationService>().Navigate(new Uri(XamlFileName, UriKind.Relative));
    }

    #region IDisposable Members
    public void Dispose()
    {
    }
    #endregion
    #endregion

    #region virtual properties
    public virtual string XamlFileName
    {
      get
      {
        return "";
      }
    }
    public virtual Grid SkinContainerElement
    {
      get
      {
        return null;
      }
    }
    #endregion

    void  BasePage_Loaded(object sender, RoutedEventArgs e)
    {
      LoadSkin();
    }

    void LoadSkin()
    {
      SkinContainerElement.Children.Clear();
      using (FileStream steam = new FileStream(@String.Format(@"skin\default\mytv\{0}",XamlFileName), FileMode.Open, FileAccess.Read))
      {
        UIElement documentRoot = (UIElement)XamlReader.Load(steam);
        SkinContainerElement.Children.Add(documentRoot);
      }
    }
  }
}
