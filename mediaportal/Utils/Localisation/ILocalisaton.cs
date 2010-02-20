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

using System.Globalization;

namespace MediaPortal.Localisation
{
  /// <summary>
  /// This interface for providing localised strings 
  /// </summary>
  public interface ILocalisation
  {
    /// <summary>
    /// Changes the language.
    /// </summary>
    /// <param name="cultureName">Name of the culture.</param>
    void ChangeLanguage(string cultureName);

    /// <summary>
    /// Get the translation for a given id and format the sting with
    /// the given parameters
    /// </summary>
    /// <param name="dwCode">id of text</param>
    /// <param name="parameters">parameters used in the formating</param>
    /// <returns>
    /// string containing the translated text
    /// </returns>
    string Get(string section, int dwCode, object[] parameters);

    /// <summary>
    /// Get the translation for a given id
    /// </summary>
    /// <param name="dwCode">id of text</param>
    /// <returns>
    /// string containing the translated text
    /// </returns>
    string Get(string section, int dwCode);

    //void LocaliseLabel(ref string strLabel);

    bool IsLocalSupported();

    CultureInfo[] AvailableLanguages();
  }
}