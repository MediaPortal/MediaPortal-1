using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using ProjectInfinity.Logging;
namespace ProjectInfinity.Themes
{
  public class ThemeManager : IThemeManager
  {
    private Dictionary<string, ResourceDictionary> themes = new Dictionary<string, ResourceDictionary>();

    public ThemeManager()
    {
      DirectoryInfo dir = new DirectoryInfo(@"Skin\default");// skin name=configuration. Needs to be changed later
      if (!dir.Exists)
        return;
      foreach (FileInfo file in dir.GetFiles("*.xaml"))
      {
        try
        {
          using (FileStream theme = file.OpenRead())
            themes.Add(file.Name.Replace(file.Extension, ""), (ResourceDictionary)XamlReader.Load(theme));
        }
        catch (Exception ex)
        {
          ServiceScope.Get<ILogger>().Error("error loading " + file);
          ServiceScope.Get<ILogger>().Error(ex);
        }
      }
    }

    public void SetDefaultTheme()
    {
      if (themes.ContainsKey("theme"))
      {
        Application.Current.Resources = themes["theme"];
      }
    }
  }
}
