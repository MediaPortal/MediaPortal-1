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

namespace ProjectInfinity.Localisation
{
  /// <summary>
  /// This interface for providing localised strings 
  /// </summary>
  public interface ILocalisation
  {
    #region Properties
    CultureInfo CurrentCulture
    {
      get;
    }

    int Characters
    {
      get;
    }
    #endregion

    #region Methods
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
    string ToString(string section, int id, object[] parameters);

    /// <summary>
    /// Get the translation for a given id
    /// </summary>
    /// <param name="dwCode">id of text</param>
    /// <returns>
    /// string containing the translated text
    /// </returns>
    string ToString(string section, int id);

    string ToString(StringId id);

    bool IsLocalSupported();

    CultureInfo[] AvailableLanguages();

    CultureInfo GetBestLanguage();
    #endregion
  }
}
