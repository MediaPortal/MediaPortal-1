#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.IO;

//using MediaPortal.GUI.Library;

namespace MediaPortal.Profile
{
  /// <summary>
  /// MPSettings allows to read and write MediaPortal.xml configuration file
  /// (wrapper class to unify path handling)
  /// </summary>
  public class MPSettings : Settings
  {
    public MPSettings()
      : base(Configuration.Config.GetFile(Configuration.Config.Dir.Config, "MediaPortal.xml")) {}
  }

  /// <summary>
  /// Settings allows to read and write any xml configuration file
  /// </summary>
  public class Settings : IDisposable
  {
    public Settings(string fileName)
      : this(fileName, true) {}

    public Settings(string fileName, bool isCached)
    {
      xmlFileName = Path.GetFileName(fileName);

      _isCached = isCached;

      if (_isCached)
      {
        foreach (ISettingsProvider doc in xmlCache)
        {
          string xmlName = Path.GetFileName(doc.FileName);
          if (String.Compare(xmlName, xmlFileName, true) == 0)
          {
            xmlDoc = doc;
            break;
          }
        }
      }

      if (xmlDoc == null)
      {
        xmlDoc = new CacheSettingsProvider(new XmlSettingsProvider(fileName));

        if (_isCached)
        {
          xmlCache.Add(xmlDoc);
        }
      }
    }

    public string GetValue(string section, string entry)
    {
      object value = xmlDoc.GetValue(section, entry);
      if (value == null)
      {
        return string.Empty;
      }

      string strValue = value.ToString();
      if (strValue.Length == 0)
      {
        return string.Empty;
      }
      return strValue;
    }

    public string GetValueAsString(string section, string entry, string strDefault)
    {
      object obj = xmlDoc.GetValue(section, entry);
      if (obj == null)
      {
        return strDefault;
      }
      string strValue = obj.ToString();
      if (strValue == null)
      {
        return strDefault;
      }
      if (strValue.Length == 0)
      {
        return strDefault;
      }
      return strValue;
    }

    public bool GetValueAsBool(string section, string entry, bool bDefault)
    {
      string strValue = (string)xmlDoc.GetValue(section, entry);
      if (strValue == null)
      {
        return bDefault;
      }
      if (strValue.Length == 0)
      {
        return bDefault;
      }
      if (strValue.ToLower() == "yes")
      {
        return true;
      }
      return false;
    }

    public int GetValueAsInt(string section, string entry, int iDefault)
    {
      object obj = xmlDoc.GetValue(section, entry);
      if (obj == null)
      {
        return iDefault;
      }
      string strValue = obj.ToString();
      if (strValue == null)
      {
        return iDefault;
      }
      if (strValue.Length == 0)
      {
        return iDefault;
      }
      try
      {
        int iRet = Int32.Parse(strValue);
        return iRet;
      }
      catch (Exception) {}
      return iDefault;
    }

    //public float GetValueAsFloat(string section, string entry, float fDefault)
    //{
    //  object obj = xmlDoc.GetValue(section, entry);
    //  if (obj == null) return fDefault;
    //  string strValue = obj.ToString();
    //  if (strValue == null) return fDefault;
    //  if (strValue.Length == 0) return fDefault;
    //  try
    //  {
    //    float test=123.456f;
    //    string tmp=test.ToString();
    //    bool useCommas = (tmp.IndexOf(",") >= 0);
    //    if (useCommas==false) 
    //      strValue = strValue.Replace(',', '.');
    //    else
    //      strValue = strValue.Replace('.', ',');

    //    float fRet = (float)System.Double.Parse(strValue, NumberFormatInfo.InvariantInfo);
    //    return fRet;
    //  }
    //  catch (Exception)
    //  {
    //  }
    //  return fDefault;
    //}

    public void SetValue(string section, string entry, object objValue)
    {
      xmlDoc.SetValue(section, entry, objValue);
    }

    public void SetValueAsBool(string section, string entry, bool bValue)
    {
      string strValue = "yes";
      if (!bValue)
      {
        strValue = "no";
      }
      SetValue(section, entry, strValue);
    }

    public void RemoveEntry(string section, string entry)
    {
      xmlDoc.RemoveEntry(section, entry);
    }

    public static void ClearCache()
    {
      xmlCache = new ArrayList();
    }

    #region IDisposable Members

    public void Dispose()
    {
      if (_isCached == false)
      {
        xmlDoc.Save();
      }
    }

    public void Clear() {}

    public static void SaveCache()
    {
      foreach (ISettingsProvider doc in xmlCache)
      {
        doc.Save();
      }
    }

    #endregion

    #region Fields

    private bool _isCached;
    private static ArrayList xmlCache = new ArrayList();
    private string xmlFileName;
    private ISettingsProvider xmlDoc;

    #endregion Fields
  }
}