#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.IO;
using System.Collections.Generic;
using MediaPortal.Configuration;

namespace MediaPortal.Profile
{
  /// <summary>
  /// MPSettings allows to read and write MediaPortal.xml configuration file
  /// (wrapper class to unify path handling)
  /// </summary>
  public class MPSettings : Settings
  {
    private static string _configPathName;

    public static string ConfigPathName
    {
      get
      {
        if (string.IsNullOrEmpty(_configPathName))
        {
          _configPathName = Config.GetFile(Config.Dir.Config, "MediaPortal.xml");
        }
        return _configPathName;
      }
      set
      {
        if (string.IsNullOrEmpty(_configPathName) || AlternateConfig)
        {
          _configPathName = value;
          if (!Path.IsPathRooted(_configPathName))
          {
            _configPathName = Config.GetFile(Config.Dir.Config, _configPathName);
          }
        }
        else
        {
          throw new InvalidOperationException("ConfigPathName already has a value.");
        }
      }
    }

    public static bool AlternateConfig { get; set; }

    private static MPSettings _instance;

    public static MPSettings Instance
    {
      get { return _instance ?? (_instance = new MPSettings()); }
    }

    // public constructor should be made/private protected, we should encourage the usage of Instance

    public MPSettings()
      : base(ConfigPathName) {}
  }

  /// <summary>
  /// SKSettings allows to read and write SkinSetting.xml configuration file.  Each skin can have its own file (same filename different path).
  /// (wrapper class to unify path handling)
  /// </summary>
  public class SKSettings : Settings
  {
    private static string _configPathName;

    public static string ConfigPathName
    {
      get
      {
        // Always form the path since switching between skins will cause different files to be returned.
        _configPathName = Config.GetFile(Config.Dir.Skin, Config.SkinName, "SkinSettings.xml");
        return _configPathName;
      }
      set
      {
        _configPathName = value;
        if (!Path.IsPathRooted(_configPathName))
        {
          _configPathName = Config.GetFile(Config.Dir.Skin, Config.SkinName, _configPathName);
        }
      }
    }

    private static SKSettings _instance;

    public static SKSettings Instance
    {
      get { return _instance ?? (_instance = new SKSettings()); }
    }

    // public constructor should be made/private protected, we should encourage the usage of Instance

    public SKSettings()
      : base(ConfigPathName) { }
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
      // Each skin may have its own SkinSettings.xml file so we need to use the entire path to detect a cache hit.
      // Adding a check when plugin send only the filename for mediaportal.xml instead of the full path and filename.
      _xmlFileName = fileName.ToLowerInvariant() == "mediaportal.xml"
        ? Config.GetFile(Config.Dir.Config, "MediaPortal.xml").ToLowerInvariant()
        : Path.GetFullPath(fileName).ToLowerInvariant();

      _isCached = isCached;

      if (_isCached)
        XMLCache.TryGetValue(_xmlFileName, out _xmlDoc);

      if (_xmlDoc == null)
      {
        _xmlDoc = new CacheSettingsProvider(new XmlSettingsProvider(fileName));

        if (_isCached)
          XMLCache.Add(_xmlFileName, _xmlDoc);
      }
    }

    public bool HasSection<T>(string section)
    {
      return _xmlDoc.HasSection<T>(section);
    }

    public IDictionary<string, T> GetSection<T>(string section)
    {
      return _xmlDoc.GetSection<T>(section);
    }

    public string GetValue(string section, string entry)
    {
      object value = _xmlDoc.GetValue(section, entry);
      return value == null ? string.Empty : value.ToString();
    }

    private T GetValueOrDefault<T>(string section, string entry, Func<string, T> conv, T defaultValue)
    {
      string strValue = GetValue(section, entry);
      return string.IsNullOrEmpty(strValue) ? defaultValue : conv(strValue);
    }

    public string GetValueAsString(string section, string entry, string strDefault)
    {
      return GetValueOrDefault(section, entry, val => val, strDefault);
    }

    public bool GetValueAsBool(string section, string entry, bool bDefault)
    {
      return GetValueOrDefault(section, entry,
                               val => val.Equals("yes", StringComparison.InvariantCultureIgnoreCase),
                               bDefault);
    }

    public int GetValueAsInt(string section, string entry, int iDefault)
    {
      return GetValueOrDefault(section, entry,
                               val =>
                                 {
                                   int iVal;
                                   return Int32.TryParse(val, out iVal) ? iVal : iDefault;
                                 }, iDefault);
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
      _xmlDoc.SetValue(section, entry, objValue);
    }

    public void SetValueAsBool(string section, string entry, bool bValue)
    {
      SetValue(section, entry, bValue ? "yes" : "no");
    }

    public void RemoveEntry(string section, string entry)
    {
      _xmlDoc.RemoveEntry(section, entry);
    }

    public static void ClearCache()
    {
      XMLCache.Clear();
    }

    #region IDisposable Members

    public void Dispose()
    {
      if (!_isCached)
      {
        _xmlDoc.Save();
      }
    }

    public void Clear() {}

    public static void SaveCache()
    {
      lock (ThisLock)
      {
        foreach (var doc in XMLCache)
        {
          doc.Value.Save();
        }
      }

    }

    #endregion

    #region Fields

    private readonly bool _isCached;
    private static readonly Dictionary<string, ISettingsProvider> XMLCache = new Dictionary<string, ISettingsProvider>();
    private readonly string _xmlFileName;
    private readonly ISettingsProvider _xmlDoc;
    private static readonly Object ThisLock = new Object();

    #endregion Fields
  }
}