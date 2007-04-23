using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Collections;
using System.Xml;
using ProjectInfinity.Settings.Xml;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Settings
{
  /// <summary>
  /// Static Class used to store or retrieve settings classes
  /// </summary>
  class ObjectParser
  {
    static ObjectParser() 
    { 
    }

    /// <summary>
    /// Serialize public properties of a Settings object to a given xml file
    /// </summary>
    /// <param name="obj">Setting Object to serialize</param>
    /// <param name="fileName">Xml file name</param>
    public static void Serialize(object obj)
    {
      string fileName = obj.ToString()+".xml";
      ILogger log = ServiceScope.Get<ILogger>();
      log.Debug("Serialize({0},{1})", obj.ToString(), fileName);
      Dictionary<string, string> globalSettingsList = new Dictionary<string, string>();
      Dictionary<string, string> userSettingsList = new Dictionary<string, string>();
      XmlSettingsProvider xmlWriter = new XmlSettingsProvider(fileName);
      bool isFirstSave = (!File.Exists(fileName));
      foreach (PropertyInfo property in obj.GetType().GetProperties())
      {
        Type thisType = property.PropertyType;
        string defaultval = "";
        log.Debug("Got property name: {0}, isCLR: {1}", property.Name, isCLRType(thisType));

        #region CLR Typed property
        if (isCLRType(thisType))
        {
          object[] attributes = property.GetCustomAttributes(typeof(SettingAttribute), false);
          SettingScope scope = SettingScope.Global;
          if (attributes.Length != 0)
          {
            SettingAttribute attribute = (SettingAttribute)attributes[0];
            scope = attribute.SettingScope;
            defaultval = attribute.DefaultValue;
          }
          else
          {
            scope = SettingScope.Global;
            defaultval = "";
          }
          string value = defaultval;

          if (!isFirstSave) //else default value will be used if it exists
          {
            if (obj.GetType().GetProperty(property.Name).GetValue(obj, null) != null)
            {
              value = obj.GetType().GetProperty(property.Name).GetValue(obj, null).ToString();
            }
            if (scope == SettingScope.User)
            {
              log.Debug("added property: {0}, value= {1} to user list", property.Name, obj.GetType().GetProperty(property.Name).GetValue(obj, null));
              userSettingsList.Add(property.Name, value);
            }
            else
            {
              log.Debug("added property: {0}, value= {1} to global list", property.Name, obj.GetType().GetProperty(property.Name).GetValue(obj, null));
              globalSettingsList.Add(property.Name, value);
            }
          }
          else
          {
            if (scope == SettingScope.Global) globalSettingsList.Add(property.Name, value);
            if (scope == SettingScope.User) userSettingsList.Add(property.Name, value);
          }
            
        }
        #endregion

        #region not CLR Typed property
        else
        {
          XmlSerializer xmlSerial = new XmlSerializer(thisType);
          StringBuilder sb = new StringBuilder();
          StringWriter strWriter = new StringWriter(sb);
          XmlTextWriter writer = new XmlNoNamespaceWriter(strWriter);
          writer.Formatting = System.Xml.Formatting.Indented;
          object propertyValue = obj.GetType().GetProperty(property.Name).GetValue(obj, null);
          xmlSerial.Serialize(writer, propertyValue);
          strWriter.Close();
          strWriter.Dispose();
          // remove unneeded encoding tag
          sb.Remove(0, 41);
          object[] attributes = property.GetCustomAttributes(typeof(SettingAttribute), false);
          SettingScope scope = SettingScope.Global;
          if (attributes.Length != 0)
          {
            SettingAttribute attribute = (SettingAttribute)attributes[0];
            scope = attribute.SettingScope;
            defaultval = attribute.DefaultValue;
          }
          else
          {
            scope = SettingScope.Global;
            defaultval = "";
          }
          string value = defaultval;
          /// a changer
          if (!isFirstSave || defaultval == "") value = sb.ToString();
          if (scope == SettingScope.User)
          {
            userSettingsList.Add(property.Name, value);
          }
          else
          {
            globalSettingsList.Add(property.Name, value);
          }
        }
        #endregion

      }

      #region write Settings
      // write settings to xml
      foreach (KeyValuePair<string, string> pair in globalSettingsList)
      {
        log.Debug("Writing Xml setting: {0}, value= {1}", pair.Key, pair.Value);
        xmlWriter.SetValue(obj.ToString(), pair.Key, pair.Value, SettingScope.Global);
      }
      foreach (KeyValuePair<string, string> pair in userSettingsList)
      {
        log.Debug("Writing Xml setting: {0}, value= {1}", pair.Key, pair.Value);
        xmlWriter.SetValue(obj.ToString(), pair.Key, pair.Value, SettingScope.User);
      }
      log.Debug("Save");
      xmlWriter.Save();
      #endregion

    }

    /// <summary>
    /// De-serialize public properties of a Settings object from a given xml file
    /// </summary>
    /// <param name="obj">Setting Object to retrieve</param>
    /// <param name="fileName">Xml file name</param>
    public static void Deserialize(object obj)
    {
      string fileName = obj.ToString() + ".xml";
      XmlSettingsProvider xmlreader = new XmlSettingsProvider(fileName);
      ILogger log = ServiceScope.Get<ILogger>();
      log.Debug("Deserialize({0},{1})", obj.ToString(), fileName);
      // if xml file doesn't exist yet then create it
      if (!File.Exists(fileName)) Serialize(obj);

      foreach (PropertyInfo property in obj.GetType().GetProperties())
      {
        Type thisType = property.PropertyType;

        #region get scope
        SettingScope scope = SettingScope.Global;
        object[] attributes = property.GetCustomAttributes(typeof(SettingAttribute), false);
        string defaultval = "";
        if (attributes.Length != 0)
        {
          SettingAttribute attribute = (SettingAttribute)attributes[0];
          scope = attribute.SettingScope;
          defaultval = attribute.DefaultValue;
        }
        else
        {
          scope = SettingScope.Global;
          defaultval = "";
        }
        #endregion

        if (isCLRType(thisType))
        #region CLR Typed property
        {
          try
          {
            string value = xmlreader.GetValue(obj.ToString(), property.Name, scope);
            if (value == null || value == string.Empty) value = defaultval;
            if (thisType == typeof(string)) property.SetValue(obj, value, null);
            if (thisType == typeof(bool)) property.SetValue(obj, bool.Parse(value), null);
            if (thisType == typeof(Int16)) property.SetValue(obj, Int16.Parse(value), null);
            if (thisType == typeof(Int32)) property.SetValue(obj, Int32.Parse(value), null);
            if (thisType == typeof(Int64)) property.SetValue(obj, Int64.Parse(value), null);
            if (thisType == typeof(UInt16)) property.SetValue(obj, UInt16.Parse(value), null);
            if (thisType == typeof(UInt32)) property.SetValue(obj, UInt32.Parse(value), null);
            if (thisType == typeof(UInt64)) property.SetValue(obj, UInt64.Parse(value), null);
            if (thisType == typeof(float)) property.SetValue(obj, float.Parse(value), null);
            if (thisType == typeof(double)) property.SetValue(obj, double.Parse(value), null);
            if (thisType == typeof(Int16)) property.SetValue(obj, Int16.Parse(value), null);
            if (thisType == typeof(Int32)) property.SetValue(obj, Int32.Parse(value), null);
            if (thisType == typeof(DateTime)) property.SetValue(obj, DateTime.Parse(value), null);
            if (thisType == typeof(bool?)) property.SetValue(obj, bool.Parse(value), null);
            if (thisType == typeof(Int16?)) property.SetValue(obj, Int16.Parse(value), null);
            if (thisType == typeof(Int32?)) property.SetValue(obj, Int32.Parse(value), null);
            if (thisType == typeof(Int64?)) property.SetValue(obj, Int64.Parse(value), null);
            if (thisType == typeof(UInt16?)) property.SetValue(obj, UInt16.Parse(value), null);
            if (thisType == typeof(UInt32?)) property.SetValue(obj, UInt32.Parse(value), null);
            if (thisType == typeof(UInt64?)) property.SetValue(obj, UInt64.Parse(value), null);
            if (thisType == typeof(float?)) property.SetValue(obj, float.Parse(value), null);
            if (thisType == typeof(double?)) property.SetValue(obj, double.Parse(value), null);
            if (thisType == typeof(Int16?)) property.SetValue(obj, Int16.Parse(value), null);
            if (thisType == typeof(Int32?)) property.SetValue(obj, Int32.Parse(value), null);
          }
          catch (Exception ex)
          {
            int foo = 0;
          }
        }
        #endregion

        else
        #region not CLR Typed property
        {
          XmlSerializer xmlSerial = new XmlSerializer(thisType);

          string value = xmlreader.GetValue(obj.ToString(), property.Name, scope);
          if (value != null)
          {
            TextReader reader = new StringReader(value);
            try
            {
              property.SetValue(obj, xmlSerial.Deserialize(reader), null);
            }
            catch (Exception ex)
            {
            }
          }

        }
        #endregion

      }
    }

    /// <summary>
    /// Detects if the current property type is or not a CLR type
    /// </summary>
    /// <param name="aType">property type</param>
    /// <returns>true: CLR Type , false: guess what</returns>
    public static bool isCLRType(Type aType)
    {
      if ((aType == typeof(int))
           || (aType == typeof(string))
           || (aType == typeof(bool))
           || (aType == typeof(float))
           || (aType == typeof(double))
           || (aType == typeof(UInt32))
           || (aType == typeof(UInt64))
           || (aType == typeof(UInt16))
           || (aType == typeof(System.DateTime))
           //|| (aType == typeof(string?))
           || (aType == typeof(bool?))
           || (aType == typeof(float?))
           || (aType == typeof(double?))
           || (aType == typeof(UInt32?))
           || (aType == typeof(UInt64?))
           || (aType == typeof(UInt16?))
           || (aType == typeof(Int32?))
           || (aType == typeof(Int64?))
           || (aType == typeof(Int16?)))
      {
        return true;
      }
      return false;
    }

  }
}
