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
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.Localisation;
using MediaPortal.Profile;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// This class will hold all text used in the application
  /// The text is loaded for the current language from
  /// the file language/[language]/strings.xml
  /// </summary>
  public class GUILocalizeStrings
  {
    #region Variables

    private static ILocalizationProvider _stringProvider;
    private static Dictionary<string, string> _cultures;
    private static string[] _languages;

    #endregion

    #region Constructors/Destructors

    // singleton. Dont allow any instance of this class
    private GUILocalizeStrings() {}

    public static void Dispose()
    {
      if (_stringProvider != null)
      {
        _stringProvider.SafeDispose();
      }
    }

    #endregion

    #region Properties

    public static bool UseRTL
    {
      get
      {
        if (_stringProvider == null)
        {
          Load(null);
        }

        if (_stringProvider != null)
          return _stringProvider.UseRTL;
        else
          return false;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets localization provider. Uses for tests
    /// </summary>
    /// <param name="provider">Localization provider</param>
    public static void SetLocalisationProvider(ILocalizationProvider provider)
    {
      _stringProvider = provider;
    }

    /// <summary>
    /// Public method to load the text from a strings/xml file into memory
    /// </summary>
    /// <param name="strFileName">Contains the filename+path for the string.xml file</param>
    /// <returns>
    /// true when text is loaded
    /// false when it was unable to load the text
    /// </returns>
    //[Obsolete("This method has changed", true)]
    public static bool Load(string language)
    {
      bool isPrefixEnabled = true;
      using (Settings reader = new MPSettings())
      {
        isPrefixEnabled = reader.GetValueAsBool("gui", "myprefix", true);
      }

      string directory = Config.GetFolder(Config.Dir.Language);
      string cultureName = null;
      if (language != null)
      {
        cultureName = GetCultureName(language);
      }

      Log.Info("Loading localized Strings - Path: {0} Culture: {1}  Language: {2} Prefix: {3}", directory, cultureName,
               language, isPrefixEnabled);

      // http://msdn.microsoft.com/en-us/library/system.globalization.cultureinfo.aspx
      _stringProvider = new LocalisationProvider(directory, cultureName, isPrefixEnabled);

      if (_stringProvider.Characters > GUIGraphicsContext.CharsInCharacterSet)
      {
        GUIGraphicsContext.CharsInCharacterSet = _stringProvider.Characters;
      }
      return true;
    }

    public static string CurrentLanguage()
    {
      if (_stringProvider == null)
      {
        Load(null);
      }

      return _stringProvider.CurrentLanguage.EnglishName;
    }

    public static void ChangeLanguage(string language)
    {
      GUIFontManager.ClearFontCache();
      if (_stringProvider == null)
      {
        Load(language);
      }
      else
      {
        _stringProvider.ChangeLanguage(GetCultureName(language));
        if (_stringProvider.Characters > GUIGraphicsContext.CharsInCharacterSet)
        {
          GUIGraphicsContext.CharsInCharacterSet = _stringProvider.Characters;
        }
      }
    }

    /// <summary>
    /// Get the translation for a given id and format the sting with
    /// the given parameters
    /// </summary>
    /// <param name="dwCode">id of text</param>
    /// <param name="parameters">parameters used in the formating</param>
    /// <returns>
    /// string containing the translated text
    /// </returns>
    public static string Get(int dwCode, object[] parameters)
    {
      if (_stringProvider == null)
      {
        Load(null);
      }

      string translation = _stringProvider.GetString("unmapped", dwCode);
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
      catch (FormatException e)
      {
        Log.Error("Error formatting translation with id {0}", dwCode);
        Log.Error("Unformatted translation: {0}", translation);
        Log.Error(e);
        return translation;
      }
    }

    /// <summary>
    /// Get the translation for a given id
    /// </summary>
    /// <param name="dwCode">id of text</param>
    /// <returns>
    /// string containing the translated text
    /// </returns>
    public static string Get(int dwCode)
    {
      if (_stringProvider == null)
      {
        Load(null);
      }

      string translation = _stringProvider.GetString("unmapped", dwCode);

      if (translation == null)
      {
        Log.Error("No translation found for id {0}", dwCode);
        return string.Empty;
      }

      return translation;
    }

    public static void LocalizeLabel(ref string strLabel)
    {
      if (_stringProvider == null)
      {
        Load(null);
      }

      if (strLabel == null)
      {
        strLabel = string.Empty;
      }
      if (strLabel == "-")
      {
        strLabel = "";
      }
      if (strLabel == "")
      {
        return;
      }

      // This can't be a valid string code if the first character isn't a number.
      // This check will save us from catching unnecessary exceptions.
      if (!char.IsNumber(strLabel, 0))
      {
        return;
      }

      // Attempt to parse an int from the string.  On failure just return leaving the input string unchanged.
      int dwLabelID;
      if (!Int32.TryParse(strLabel, out dwLabelID))
      {
        return;
      }

      strLabel = _stringProvider.GetString("unmapped", dwLabelID);
      if (strLabel == null)
      {
        Log.Error("No translation found for id {0}", dwLabelID);
        strLabel = string.Empty;
      }
    }

    public static string LocalSupported()
    {
      if (_stringProvider == null)
      {
        Load(null);
      }

      CultureInfo culture = _stringProvider.GetBestLanguage();

      return culture.EnglishName;
    }

    public static string[] SupportedLanguages()
    {
      if (_languages == null)
      {
        if (_stringProvider == null)
        {
          Load(null);
        }

        CultureInfo[] cultures = _stringProvider.AvailableLanguages();

        SortedList sortedLanguages = new SortedList();
        foreach (CultureInfo culture in cultures)
        {
          sortedLanguages.Add(culture.EnglishName, culture.EnglishName);
        }

        _languages = new string[sortedLanguages.Count];

        for (int i = 0; i < sortedLanguages.Count; i++)
        {
          _languages[i] = (string)sortedLanguages.GetByIndex(i);
        }
      }

      return _languages;
    }

    public static string GetCultureName(string language)
    {
      if (_cultures == null)
      {
        _cultures = new Dictionary<string, string>();

        CultureInfo[] cultureList = CultureInfo.GetCultures(CultureTypes.AllCultures);

        for (int i = 0; i < cultureList.Length; i++)
        {
          _cultures[cultureList[i].EnglishName] = cultureList[i].Name;
        }
      }
      string cultures = null;
      if (_cultures.TryGetValue(language, out cultures))
      {
        return cultures;
      }

      return null;
    }

    #endregion
  }
}