using System.Windows;

namespace ProjectInfinity.Themes
{
  public interface IThemeManager
  {
    /// <summary>
    /// Loads the default theme of the application
    /// </summary>
    void SetDefaultTheme();
    //JoeDalton: no longer necessary: resources can be embedded in the content control
    ///// <summary>
    ///// Loads the resource dictionary for the passed view
    ///// </summary>
    ///// <param name="view">An <see cref="object"/> representing the view to load the resources for.</param>
    ///// <returns>A <see cref="ResourceDictionary"/>, or <b>null</b> if the resource file could not be found.</returns>
    //ResourceDictionary LoadResources(object view);
    /// <summary>
    /// Loads the content for the passed view
    /// </summary>
    /// <param name="view">An <see cref="object"/> representing the view to load the content for.</param>
    /// <returns>A <see cref="object"/>, or <b>null</b> if the content file could not be found.</returns>
    /// <remarks>The returned content will typically contain an object deriving from 
    /// <see cref="UIElement"/> but can technically be any kind of object.  If the object
    /// that is returned cannot be rendered, WPF will call its <b>ToString</b> method and
    /// render the returned string instead.</remarks>
    object LoadContent(object view);
  }
}