using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ProjectInfinity.Localisation
{
  internal class NoLocalisation : ILocalisation
  {
    public CultureInfo CurrentCulture
    {
      get { return CultureInfo.CurrentUICulture; }
    }

    public int Characters
    {
      get { return 1; }
    }

    /// <summary>
    /// Changes the language.
    /// </summary>
    /// <param name="cultureName">Name of the culture.</param>
    public void ChangeLanguage(string cultureName)
    {
    }

    /// <summary>
    /// Get the translation for a given id and format the sting with
    /// the given parameters
    /// </summary>
    /// <param name="id">id of text</param>
    /// <param name="parameters">parameters used in the formating</param>
    /// <returns>
    /// string containing the translated text
    /// </returns>
    public string ToString(string section, int id, object[] parameters)
    {
      return string.Format("{0}:{1}", section, id);
    }

    /// <summary>
    /// Get the translation for a given id
    /// </summary>
    /// <param name="id">id of text</param>
    /// <returns>
    /// string containing the translated text
    /// </returns>
    public string ToString(string section, int id)
    {
      return string.Format("{0}:{1}", section, id);
    }

    public string ToString(StringId id)
    {
      return id.ToString();
    }

    public bool IsLocalSupported()
    {
      return false;
    }

    public CultureInfo[] AvailableLanguages()
    {
      return new CultureInfo[] {CultureInfo.CurrentUICulture};
    }

    public CultureInfo GetBestLanguage()
    {
      return CultureInfo.CurrentUICulture;
    }
  }
}
