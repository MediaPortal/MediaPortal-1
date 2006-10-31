using System;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Indicates what icons to use for representing a plugin in the Configuration program.
  /// </summary>
  public class PluginIconsAttribute : Attribute
  {
    private string activatedResourceName;
    private string deactivatedResourceName;

    /// <summary>
    /// Indicate what icons to use for representing a plugin in the Configuration program.
    /// </summary>
    /// <param name="activatedResourceName">Indicates the resource to use when the plugin is active.</param>
    /// <param name="deactivatedResourceName">Indicates the resource to use when the plugin is deactivated.</param>
    public PluginIconsAttribute(string activatedResourceName, string deactivatedResourceName)
    {
      ActivatedResourceName = activatedResourceName;
      DeactivatedResourceName = deactivatedResourceName;
    }

    public string ActivatedResourceName
    {
      get { return activatedResourceName; }
      set
      {
        if (value==null)
          throw new ArgumentNullException("ActivatedResourceName");
        activatedResourceName = value;
      }
    }

    public string DeactivatedResourceName
    {
      get { return deactivatedResourceName; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("DeactivatedResourceName");
        deactivatedResourceName = value;
      }
    }
  }
}