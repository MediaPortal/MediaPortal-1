using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;

namespace ProjectInfinity.Themes
{
  public class ThemeManager : IThemeManager
  {
    private Dictionary<string, ResourceDictionary> themes = new Dictionary<string, ResourceDictionary>();

    public ThemeManager()
    {
      DirectoryInfo dir = new DirectoryInfo(@"..\..\Themes");
      if (!dir.Exists)
        return;
      foreach(FileInfo file in dir.GetFiles("*.xaml"))
      {
        using(FileStream theme = file.OpenRead())
        themes.Add(file.Name.Replace(file.Extension,""), (ResourceDictionary) XamlReader.Load(theme));
      }
    }

    public void SetDefaultTheme()
    {
      Application.Current.Resources = themes["Test"];
    }
  }
}
