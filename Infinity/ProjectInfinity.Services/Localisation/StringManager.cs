#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Utilities.Localisation;

namespace ProjectInfinity.Localisation
{
  /// <summary>
  /// This class manages localisation strings
  /// </summary>
  public class StringManager : ILocalisation
  {
    LocalisationProvider _stringProvider;

    public StringManager(string directory, string cultureName)
    {
      _stringProvider = new LocalisationProvider(directory, cultureName);
    }

    public CultureInfo CurrentCulture
    {
      get { return _stringProvider.CurrentCulture; }
    }

    public int Characters
    {
      get { return _stringProvider.Characters; }
    }

    /// <summary>
    /// Changes the language.
    /// </summary>
    /// <param name="cultureName">Name of the culture.</param>
    public void ChangeLanguage(string cultureName)
    {
      _stringProvider.ChangeLanguage(cultureName);
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
    public string ToString(string section, int id, object[] parameters)
    {
      return _stringProvider.ToString(section, id, parameters);
    }

    /// <summary>
    /// Get the translation for a given id
    /// </summary>
    /// <param name="dwCode">id of text</param>
    /// <returns>
    /// string containing the translated text
    /// </returns>
    public string ToString(string section, int id)
    {
      return _stringProvider.ToString(section, id);
    }

    public string ToString(StringId id)
    {
      return _stringProvider.ToString(id.Section, id.Id);
    }

    public bool IsLocalSupported()
    {
      return _stringProvider.IsLocalSupported();
    }

    public CultureInfo[] AvailableLanguages()
    {
      return _stringProvider.AvailableLanguages();
    }

    public CultureInfo GetBestLanguage()
    {
      return _stringProvider.GetBestLanguage();
    }
  }
}
