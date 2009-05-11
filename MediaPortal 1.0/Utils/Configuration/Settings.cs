#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Reflection;
//using MediaPortal.GUI.Library;
using System.Globalization;

namespace MediaPortal.Profile
{

  public class Settings : IDisposable
  {
    public Settings(string fileName)
      : this(fileName, true)
    {
    }

    public Settings(string fileName, bool isCached)
    {
      xmlFileName = System.IO.Path.GetFileName(fileName);

      _isCached = isCached;

      if (_isCached)
      {
        foreach (ISettingsProvider doc in xmlCache)
        {
          string xmlName = System.IO.Path.GetFileName(doc.FileName);
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
          xmlCache.Add(xmlDoc);
      }
    }

    public string GetValue(string section, string entry)
    {
      object value = xmlDoc.GetValue(section, entry);
      if (value == null) return string.Empty;

      string strValue = value.ToString();
      if (strValue.Length == 0) return string.Empty;
      return strValue;
    }

    public string GetValueAsString(string section, string entry, string strDefault)
    {
      object obj = xmlDoc.GetValue(section, entry);
      if (obj == null) return strDefault;
      string strValue = obj.ToString();
      if (strValue == null) return strDefault;
      if (strValue.Length == 0) return strDefault;
      return strValue;
    }

    public bool GetValueAsBool(string section, string entry, bool bDefault)
    {
      string strValue = (string)xmlDoc.GetValue(section, entry);
      if (strValue == null) return bDefault;
      if (strValue.Length == 0) return bDefault;
      if (strValue.ToLower() == "yes") return true;
      return false;
    }
    public int GetValueAsInt(string section, string entry, int iDefault)
    {
      object obj = xmlDoc.GetValue(section, entry);
      if (obj == null) return iDefault;
      string strValue = obj.ToString();
      if (strValue == null) return iDefault;
      if (strValue.Length == 0) return iDefault;
      try
      {
        int iRet = System.Int32.Parse(strValue);
        return iRet;
      }
      catch (Exception)
      {
      }
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
      if (!bValue) strValue = "no";
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
        xmlDoc.Save();
    }

    public void Clear()
    {
    }

    public static void SaveCache()
    {
      foreach (ISettingsProvider doc in xmlCache)
      {
        doc.Save();
      }
    }
    #endregion

    #region Fields

    bool _isCached;
    static ArrayList xmlCache = new ArrayList();
    string xmlFileName;
    ISettingsProvider xmlDoc;

    #endregion Fields
  }
}
