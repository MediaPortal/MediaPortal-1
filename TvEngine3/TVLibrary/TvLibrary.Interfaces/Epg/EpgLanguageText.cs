/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace TvLibrary.Epg
{
  /// <summary>
  /// class which holds the title, description and genre for all epg languages received
  /// </summary>
  [Serializable]
  public class EpgLanguageText
  {
    #region variables
    string _language;
    string _title;
    string _description;
    string _genre;
    int _starRating;
    string _classification;
    int _parentalRating;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="EpgLanguageText"/> class.
    /// </summary>
    /// <param name="language">The language.</param>
    /// <param name="title">The title.</param>
    /// <param name="description">The description.</param>
    /// <param name="genre">The genre.</param>
    /// <param name="starRating">The star rating</param>
    /// <param name="classification">The classification</param>
    /// <param name="parentalRating">The parental rating.</param>
    public EpgLanguageText(string language, string title, string description, string genre, int starRating, string classification, int parentalRating)
    {
      Language = language;
      Title = title;
      Description = description;
      Genre = genre;
      StarRating = starRating;
      Classification = classification;
      ParentalRating = parentalRating;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    /// <value>The language.</value>
    public string Language
    {
      get
      {
        return _language;
      }
      set
      {
        _language = value ?? "";
      }
    }
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title
    {
      get
      {
        return _title;
      }
      set
      {
        _title = value ?? "";
      }
    }
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description
    {
      get
      {
        return _description;
      }
      set
      {
        _description = value ?? "";
      }
    }
    /// <summary>
    /// Gets or sets the genre.
    /// </summary>
    /// <value>The genre.</value>
    public string Genre
    {
      get
      {
        return _genre;
      }
      set
      {
        _genre = value ?? "";
      }
    }
    /// <summary>
    /// Gets or sets the star rating.
    /// </summary>
    /// <value>The star rating.</value>
    public int StarRating
    {
      get
      {
        return _starRating;
      }
      set
      {
        _starRating = value;
      }
    }
    /// <summary>
    /// Gets or sets the classification.
    /// </summary>
    /// <value>The classification.</value>
    public string Classification
    {
      get
      {
        return _classification;
      }
      set
      {
        _classification = value ?? "";
      }
    }
    /// <summary>
    /// Gets or sets the parental rating.
    /// </summary>
    /// <value>The parental rating.</value>
    public int ParentalRating
    {
      get
      {
        return _parentalRating;
      }
      set
      {
        _parentalRating = value;
      }
    }
    #endregion
  }
}
