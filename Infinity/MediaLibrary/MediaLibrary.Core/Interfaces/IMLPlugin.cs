using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLibrary
{
    public interface IMLPlugin
    {
        #region bool GetProperties(IMLPluginProperties PropertiesCollection)
        /// <summary>
        /// Defing config entries
        /// </summary>
        /// <PARAM name="Properties">Collection of IMLPluginProperty</PARAM>
        /// <returns>Returns true on succes, false on error.</returns>
        bool GetProperties(IMLPluginProperties PropertiesCollection);
        #endregion

        #region bool SetProperties(IMLHashItem Properties, out string ErrorText)
        /// <summary>
        /// Retrieving config settings from configuration.exe and at startup 
        /// </summary>
        /// <PARAM name="Properties">Containing the list of settings</PARAM>
        /// <PARAM name="ErrorText">Returns the error text</PARAM>
        /// <returns>Returns true on succes, false on error.</returns>
        bool SetProperties(IMLHashItem Properties, out string ErrorText);
        #endregion

        #region bool ValidateProperties(IMLPluginProperties PropertiesCollection, IMLHashItem Properties)
        /// <summary>
        /// Validating or using controls dependent on another controls input
        /// </summary>
        /// <param name="Properties"></param>
        /// <returns></returns>
        bool ValidateProperties(IMLPluginProperties PropertiesCollection, IMLHashItem Properties);
        #endregion

        #region bool EditCustomProperty(IntPtr Window, string PropertyName, string Value)
        /// <summary>
        /// Retrieving custom config setting
        /// </summary>
        /// <PARAM name="Window">The handle of Media config window</PARAM>
        /// <PARAM name="PropertyName">The name of the the setting to edit</PARAM>
        /// <PARAM name="Value">A string that may contain the setting</PARAM>
        /// <returns>Returns true on succes, false on error.</returns>
        bool EditCustomProperty(IntPtr Window, string PropertyName, ref string Value);
        #endregion
    }
}
