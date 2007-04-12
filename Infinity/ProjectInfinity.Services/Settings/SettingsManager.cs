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
    /// Retrieves an object's public properties from a given Xml file 
    /// </summary>
    /// <param name="settingsObject">Object's instance</param>
    /// <param name="filename">Xml file wich contains stored datas</param>
    public void Load(object settingsObject, string filename)
    {
      ObjectParser.Deserialize(settingsObject, filename);
    }

    /// <summary>
    /// Stores an object's public properties to a given Xml file 
    /// </summary>
    /// <param name="settingsObject">Object's instance</param>
    /// <param name="filename">Xml file where we wanna store datas</param>
    public void Save(object settingsObject, string filename)
    {
      ObjectParser.Serialize(settingsObject, filename);
    }
  }
}
