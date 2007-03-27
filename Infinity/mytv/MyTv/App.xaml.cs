using System;
using System.IO;
using System.Windows;
using System.Data;
using System.Xml;
using System.Configuration;
using System.Windows.Markup;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>

  public partial class App : System.Windows.Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      using (FileStream theme = new FileStream("MpStylesAndTemplates.xaml",FileMode.Open))
      {
        Application.Current.Resources = (ResourceDictionary)XamlReader.Load(theme);
      }
      base.OnStartup(e);
    }
  }
}