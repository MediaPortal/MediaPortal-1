using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace ProjectInfinity.Settings
{
  /// <summary>
  /// Enumerator for a setting's scope
  /// </summary>
  public enum SettingScope
  {
    Global = 1, // global setting, doesn't allow per user/per plugin override
    User = 2    // per user setting : allows per user storage
  }

  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
  public class SettingAttribute : Attribute
  {
    private SettingScope _settingScope;
    private string _DefaultValue;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="settingScope">Setting's scope</param>
    /// <param name="defaultValue">Default value</param>
    public SettingAttribute(SettingScope settingScope, string defaultValue)
    {
      this._settingScope = settingScope;
      this.DefaultValue = defaultValue;
    }

    /// <summary>
    /// Get/Set setting's scope (User/Global)
    /// </summary>
    public SettingScope SettingScope
    {
      get { return _settingScope; }
      set { _settingScope = value; }
    }

    /// <summary>
    /// Get/Set the default value
    /// </summary>
    public string DefaultValue
    {
      get { return _DefaultValue; }
      set { _DefaultValue = value; }
    }
  }
}
