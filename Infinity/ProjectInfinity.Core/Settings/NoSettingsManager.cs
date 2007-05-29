namespace ProjectInfinity.Settings
{
  /// <summary>
  /// Default <see cref="ISettingsManager"/> implementation that does absolutely nothing
  /// </summary>
  /// <remarks>
  /// </remarks>
  internal class NoSettingsManager : ISettingsManager
  {
    #region ISettingsManager Members

    /// <summary>
    /// Retrieves an object's public properties from a given Xml file 
    /// </summary>
    /// <param name="settingsObject">Object's instance</param>
    public void Load(object settingsObject)
    {}

    /// <summary>
    /// Stores an object's public properties to a given Xml file 
    /// </summary>
    /// <param name="settingsObject">Object's instance</param>
    public void Save(object settingsObject)
    {}

    #endregion
  }
}