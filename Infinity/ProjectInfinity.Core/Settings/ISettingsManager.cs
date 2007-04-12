using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Settings
{
  public interface ISettingsManager
  {
    /// <summary>
    /// Retrieves an object's public properties from a given Xml file 
    /// </summary>
    /// <param name="settingsObject">Object's instance</param>
    /// <param name="filename">Xml file wich contains stored datas</param>
    void Load(object settingsObject, string filename);

    /// <summary>
    /// Stores an object's public properties to a given Xml file 
    /// </summary>
    /// <param name="settingsObject">Object's instance</param>
    /// <param name="filename">Xml file where we wanna store datas</param>
    void Save(object settingsObject, string filename);

  }
}
