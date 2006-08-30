/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Epg
{
  [Serializable]
  public class EpgLanguageText
  {
    #region variables
    string _language;
    string _title;
    string _description;
    string _genre;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="T:EpgLanguageText"/> class.
    /// </summary>
    /// <param name="language">The language.</param>
    /// <param name="title">The title.</param>
    /// <param name="description">The description.</param>
    /// <param name="genre">The genre.</param>
    public EpgLanguageText(string language, string title, string description, string genre)
    {
      Language = language;
      Title = title;
      Description = description;
      Genre = genre;
    }
    #endregion

    #region properties
    public string Language
    {
      get
      {
        return _language;
      }
      set
      {
        _language = value;
        if (_language == null) _language = "";
      }
    }
    public string Title
    {
      get
      {
        return _title;
      }
      set
      {
        _title = value;
        if (_title == null) _title = "";
      }
    }
    public string Description
    {
      get
      {
        return _description;
      }
      set
      {
        _description = value;
        if (_description == null) _description = "";
      }
    }
    public string Genre
    {
      get
      {
        return _genre;
      }
      set
      {
        _genre = value;
        if (_genre == null) _genre = "";
      }
    }
    #endregion
  }
}
