using System;
using System.Collections.Generic;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;

namespace ProjectInfinity.Settings
{
  /// <summary>
  /// Main Config Service
  /// </summary>
  public class SettingsManager : ISettingsManager
  {
    /// <summary>
    /// Retrieves an object's public properties from an Xml file 
    /// </summary>
    /// <param name="settingsObject">Object's instance</param>
    public void Load(object settingsObject)
    {
      ObjectParser.Deserialize(settingsObject);
    }

    /// <summary>
    /// Stores an object's public properties to an Xml file 
    /// </summary>
    /// <param name="settingsObject">Object's instance</param>
    public void Save(object settingsObject)
    {
      ObjectParser.Serialize(settingsObject);
    }
  }
}
