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
    private readonly Dictionary<string, ResourceDictionary> themes = new Dictionary<string, ResourceDictionary>();
    private string _currentTheme = "Default";

    public ThemeManager()
    {
      //TODO: read current skin name from configuration
      DirectoryInfo dir = new DirectoryInfo(@"Skin\"+_currentTheme);
      if (!dir.Exists)
        return;
      foreach (FileInfo file in dir.GetFiles("*.xaml"))
      {
        try
        {
            if (!file.Name.ToLower().Equals("app.xaml"))
            {
                using (FileStream theme = file.OpenRead())
                    themes.Add(file.Name.Replace(file.Extension, ""), (ResourceDictionary)XamlReader.Load(theme));
            }
            else ServiceScope.Get<ILogger>().Warn("ThemeManager: App.xaml was found, but we shouldn't use that one - App.xaml was prevented from loading");
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

    //JoeDalton: no longer necessary: resources can be embedded in the content control
    ///// <summary>
    ///// Loads the resource dictionary for the passed view
    ///// </summary>
    ///// <param name="view">An <see cref="object"/> representing the view to load the resources for.</param>
    ///// <returns>A <see cref="ResourceDictionary"/>, or <b>null</b> if the resource file could not be found.</returns>
    //public ResourceDictionary LoadResources(object view)
    //{
    //  string resourceName = GetResourceName(view);
    //  if (File.Exists(resourceName))
    //  {
    //    using (FileStream fileStream = new FileStream(resourceName, FileMode.Open, FileAccess.Read))
    //    {
    //      try
    //      {
    //        if (fileStream.CanRead)
    //        {
    //          return (ResourceDictionary) XamlReader.Load(fileStream);
    //        }
    //        else
    //          ServiceScope.Get<ILogger>().Error("Resource file {0} could not be read", resourceName);
    //      }
    //      catch(Exception ex)
    //      {
    //        ServiceScope.Get<ILogger>().Error("Error while reading resource file {0}", ex,resourceName);
    //      }
    //    }
    //  }
    //  ServiceScope.Get<ILogger>().Warn("Resource file {0} could not be found",resourceName);
    //  return null;
    //}

    /// <summary>
    /// Loads the content for the passed view
    /// </summary>
    /// <param name="view">An <see cref="object"/> representing the view to load the content for.</param>
    /// <returns>A <see cref="object"/>, or <b>null</b> if the content file could not be found.</returns>
    /// <remarks>The returned content will typically contain an object deriving from 
    /// <see cref="UIElement"/> but can technically be any kind of object.  If the object
    /// that is returned cannot be rendered, WPF will call its <b>ToString</b> method and
    /// render the returned string instead.</remarks>
    public object LoadContent(object view)
    {
      string viewName = GetViewName(view);
      if (File.Exists(viewName))
      {
        using (FileStream fileStream = new FileStream(viewName, FileMode.Open, FileAccess.Read))
        {
          try
          {
            if (fileStream.CanRead)
            {
              return XamlReader.Load(fileStream);
            }
            else
              ServiceScope.Get<ILogger>().Error("View file {0} could not be read", viewName);
          }
          catch (Exception ex)
          {
            ServiceScope.Get<ILogger>().Error("Error while reading view file {0}", ex, viewName);
          }
        }
      }
      ServiceScope.Get<ILogger>().Error("View file {0} could not be found", viewName);
      return null;
    }

    //JoeDalton: no longer necessary
    //private string GetResourceName(object view)
    //{
    //  return GetName(@"skin\{0}\{1}_resources.xaml", view);
    //}

    private string GetViewName(object view)
    {
      return GetName(@"skin\{0}\{1}.xaml", view);
    }

    private string GetName(string format, object view)
    {
      string name = view.GetType().FullName;
      if (name.ToUpper().StartsWith("PROJECTINFINITY."))
      {
        name = name.Substring(16);
      }
      name = name.Replace('.', '\\');
      return string.Format(format, _currentTheme, name);
    }
  }
}
