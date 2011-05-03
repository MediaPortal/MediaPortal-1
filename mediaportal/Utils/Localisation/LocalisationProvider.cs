#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using MediaPortal.CoreServices;
using MediaPortal.Localisation.LanguageStrings;
using MediaPortal.Services;
using MediaPortal.Profile;
using MediaPortal.Configuration;

namespace MediaPortal.Localisation
{
  public class LocalisationProvider
  {
    #region Variables

    private Dictionary<string, Dictionary<int, StringLocalised>> _languageStrings;
    private Dictionary<string, CultureInfo> _availableLanguages;
    private List<string> _languageDirectories;
    private CultureInfo _currentLanguage;
    private string _systemDirectory;
    private string _userDirectory;
    private int _characters;
    private bool _prefix;
    private bool _userLanguage;
    private bool _useRTL = false;

    #endregion

    #region Constructors/Destructors

    public LocalisationProvider(string systemDirectory, string userDirectory, string cultureName, bool prefix)
    {
      // Base strings directory
      _systemDirectory = systemDirectory;
      // User strings directory
      _userDirectory = userDirectory;

      _prefix = prefix;

      _languageDirectories = new List<string>();
      _languageDirectories.Add(_systemDirectory);

      GetAvailableLangauges();

      // If the language cannot be found default to Local language or English
      CultureInfo cultureInfo = null;
      if (cultureName != null && _availableLanguages.TryGetValue(cultureName, out cultureInfo))
      {
        _currentLanguage = cultureInfo;
      }
      else
      {
        _currentLanguage = GetBestLanguage();
      }

      if (_currentLanguage == null)
      {
        throw (new ArgumentException("No available language found"));
      }

      _languageStrings = new Dictionary<string, Dictionary<int, StringLocalised>>();

      CheckUserStrings();
      ReloadAll();
    }

    public LocalisationProvider(string directory, string cultureName, bool prefix)
      : this(directory, directory, cultureName, prefix) {}

    public LocalisationProvider(string directory, string cultureName)
      : this(directory, cultureName, true) {}

    public void Dispose()
    {
      Clear();
    }

    #endregion

    #region Properties

    public CultureInfo CurrentLanguage
    {
      get { return _currentLanguage; }
    }

    public int Characters
    {
      get { return _characters; }
    }

    public bool UseRTL
    {
      get { return _useRTL; }
    }

    #endregion

    #region Public Methods

    public void AddDirection(string directory)
    {
      // Add directory to list, to enable reloading/changing language
      _languageDirectories.Add(directory);

      LoadStrings(directory);
    }

    public void ChangeLanguage(string cultureName)
    {
      CultureInfo currentLanguageFound = null;
      if (!_availableLanguages.TryGetValue(cultureName, out currentLanguageFound))
      {
        throw new ArgumentException("Language not available");
      }

      _currentLanguage = currentLanguageFound;

      ReloadAll();
    }

    public StringLocalised Get(string section, int id)
    {
      Dictionary<int, StringLocalised> localList = null;
      if (_languageStrings.TryGetValue(section.ToLower(), out localList))
      {
        StringLocalised stringLocalised = null;
        if (localList.TryGetValue(id, out stringLocalised))
        {
          return stringLocalised;
        }
      }

      return null;
    }

    public string GetString(string section, int id)
    {
      Dictionary<int, StringLocalised> localList = null;
      if (_languageStrings.TryGetValue(section.ToLower(), out localList))
      {
        StringLocalised stringLocalised = null;
        if (localList.TryGetValue(id, out stringLocalised))
        {
          string prefix = string.Empty;
          if (_prefix)
          {
            prefix = stringLocalised.prefix;
          }
          return prefix + stringLocalised.text;
        }
      }

      return null;
    }

    public string GetString(string section, int id, object[] parameters)
    {
      string translation = GetString(section, id);
      // if parameters or the translation is null, return the translation.
      if ((translation == null) || (parameters == null))
      {
        return translation;
      }
      // return the formatted string. If formatting fails, log the error
      // and return the unformatted string.
      try
      {
        return String.Format(translation, parameters);
      }
      catch (FormatException)
      {
        //Log.Error("Error formatting translation with id {0}", dwCode);
        //Log.Error("Unformatted translation: {0}", translation);
        //Log.Error(e);  
        // Throw exception??
        return translation;
      }
    }

    public CultureInfo[] AvailableLanguages()
    {
      CultureInfo[] available = new CultureInfo[_availableLanguages.Count];

      IDictionaryEnumerator languageEnumerator = _availableLanguages.GetEnumerator();

      for (int i = 0; i < _availableLanguages.Count; i++)
      {
        languageEnumerator.MoveNext();
        available[i] = (CultureInfo)languageEnumerator.Value;
      }

      return available;
    }

    public bool IsLocalSupported()
    {
      if (_availableLanguages.ContainsKey(CultureInfo.CurrentCulture.Name))
      {
        return true;
      }

      return false;
    }

    public CultureInfo GetBestLanguage()
    {
      // Try current local language
      if (_availableLanguages.ContainsKey(CultureInfo.CurrentCulture.Name))
      {
        return CultureInfo.CurrentCulture;
      }

      // Try Language Parent if it has one
      if (!CultureInfo.CurrentCulture.IsNeutralCulture &&
          _availableLanguages.ContainsKey(CultureInfo.CurrentCulture.Parent.Name))
      {
        return CultureInfo.CurrentCulture.Parent;
      }

      // default to English
      CultureInfo english = null;
      if (_availableLanguages.TryGetValue("en", out english))
      {
        return english;
      }

      return null;
    }

    #endregion

    #region Private Methods

    private void LoadUserStrings()
    {
      // Load User Custom strings
      if (_userLanguage)
      {
        LoadStrings(_userDirectory, "user", false);
      }
    }

    private void LoadStrings(string directory)
    {
      LoadStrings(directory, false);
    }

    private void LoadStrings(string directory, bool loadRTL)
    {
      // Local Language
      LoadStrings(directory, _currentLanguage.Name, false, loadRTL);

      // Parent Language
      if (!_currentLanguage.IsNeutralCulture)
      {
        LoadStrings(directory, _currentLanguage.Parent.Name, false, loadRTL);
      }

      // Default to English
      if (_currentLanguage.Name != "en")
      {
        LoadStrings(directory, "en", true);
      }
    }

    private void ReloadAll()
    {
      Clear();

      LoadUserStrings();

      if (_languageDirectories != null)
      {
        for (int i = 0; i < _languageDirectories.Count; i++)
        {
          LoadStrings(_languageDirectories[i], i == 0); // we want to load RTL flag only from main directory, first in list (index 0)
        }
      }
    }

    private void Clear()
    {
      if (_languageStrings != null)
      {
        _languageStrings.Clear();
      }

      _characters = 255;
      _useRTL = false;
    }

    private void CheckUserStrings()
    {
      _userLanguage = false;

      string path = Path.Combine(_userDirectory, "strings_user.xml");

      if (File.Exists(path))
      {
        _userLanguage = true;
      }
    }

    private void GetAvailableLangauges()
    {
      _availableLanguages = new Dictionary<string, CultureInfo>();

      DirectoryInfo dir = new DirectoryInfo(_systemDirectory);
      foreach (FileInfo file in dir.GetFiles("strings_*.xml"))
      {
        int pos = file.Name.IndexOf('_') + 1;
        string cultName = file.Name.Substring(pos, file.Name.Length - file.Extension.Length - pos);

        try
        {
          CultureInfo cultInfo = new CultureInfo(cultName);
          _availableLanguages.Add(cultName, cultInfo);
        }
        catch (ArgumentException)
        {
          // Log file error?
        }
      }
    }

    private void LoadStrings(string directory, string language, bool log)
    {
      LoadStrings(directory, language, log, false);
    }

    private void LoadStrings(string directory, string language, bool log, bool loadRTL)
    {
      string filename = "strings_" + language + ".xml";
      GlobalServiceProvider.Instance.Get<ILogger>().Info("    Loading strings file: {0}", filename);

      string path = Path.Combine(directory, filename);
      if (File.Exists(path))
      {
        StringFile strings;
        try
        {
          XmlSerializer s = new XmlSerializer(typeof (StringFile));
          using (TextReader r = new StreamReader(path))
          {
            strings = (StringFile)s.Deserialize(r);
          }
        }
        catch (Exception)
        {
          return;
        }

        if (_characters < strings.characters)
        {
          _characters = strings.characters;
        }

        // Some chinese might prefer to use an english OS but still have all chars for media, etc
        bool useChineseHack = false;
        int useChineseHackNum = 0;
        using (Settings reader = new MPSettings())
        {
          useChineseHack = reader.GetValueAsBool("debug", "useExtendedCharsWithStandardCulture", false);
          if (useChineseHack)
          {
            _characters = 1536;
          }

          useChineseHackNum = reader.GetValueAsInt("debug", "useExtendedCharsWithStandardCulture", 0);
          if (useChineseHackNum >= 128 && useChineseHackNum <= 1536)
          {
            useChineseHack = true;
            _characters = useChineseHackNum;
          }
          else
          {
            if (useChineseHack)
              useChineseHackNum = 1536;
          }
        }
        GlobalServiceProvider.Instance.Get<ILogger>().Debug("    ExtendedChars = {0}:{0}, StringChars = {1}", useChineseHack,
                                                useChineseHackNum, strings.characters);

        if (loadRTL)
          _useRTL = strings.rtl;

        foreach (StringSection section in strings.sections)
        {
          // convert section name tolower -> no case matching.
          section.name = section.name.ToLower();

          Dictionary<int, StringLocalised> newSection;
          if (_languageStrings.TryGetValue(section.name, out newSection))
          {
            _languageStrings.Remove(section.name);
          }
          else
          {
            newSection = new Dictionary<int, StringLocalised>();
          }

          foreach (StringLocalised languageString in section.localisedStrings)
          {
            if (!newSection.ContainsKey(languageString.id))
            {
              languageString.language = language;
              newSection.Add(languageString.id, languageString);
              if (log)
              {
                GlobalServiceProvider.Instance.Get<ILogger>().Info("    String not found, using English: {0}",
                                                       languageString.ToString());
              }
            }
          }

          if (newSection.Count > 0)
          {
            _languageStrings.Add(section.name, newSection);
          }
        }
      }
    }

    #endregion
  }
}