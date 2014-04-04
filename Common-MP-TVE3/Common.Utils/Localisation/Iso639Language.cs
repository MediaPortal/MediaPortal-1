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

namespace MediaPortal.Common.Utils.Localisation
{
  /// <summary>
  /// A class to hold language attributes standardised by ISO 639.
  /// </summary>
  public class Iso639Language
  {
    private string _name = string.Empty;
    private string _twoLetterCode = string.Empty;       // ISO 639-1
    private string _bibliographicCode = string.Empty;   // ISO 639-2/B
    private string _terminologicCode = string.Empty;    // ISO 639-2/T
    private bool _isDeprecated = false;

    /// <summary>
    /// Initialise a new instance of the <see cref="Iso639Language"/> class.
    /// </summary>
    /// <param name="name">The language's English name.</param>
    /// <param name="twoLetterCode">The language's ISO 639-1 two letter code.</param>
    /// <param name="bibliographicCode">The language's ISO 639-2/B three letter code.</param>
    /// <param name="terminologicCode">The language's ISO 639-2/T three letter code.</param>
    /// <param name="isDeprecated"><c>True</c> if one or more of the attributes are deprecated by ISO 639.</param>
    public Iso639Language(string name, string twoLetterCode, string bibliographicCode, string terminologicCode = null, bool isDeprecated = false)
    {
      _name = name;
      _twoLetterCode = twoLetterCode;
      _bibliographicCode = bibliographicCode;
      _terminologicCode = terminologicCode;
      _isDeprecated = isDeprecated;
    }

    /// <summary>
    /// Get the English name for the language.
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
    }

    /// <summary>
    /// Get the ISO 639-1 code for the language.
    /// </summary>
    /// <remarks>
    /// May be null.
    /// </remarks>
    public string TwoLetterCode
    {
      get
      {
        return _twoLetterCode;
      }
    }

    /// <summary>
    /// Get the ISO 639-2/B code for the language.
    /// </summary>
    public string BibliographicCode
    {
      get
      {
        return _bibliographicCode;
      }
    }

    /// <summary>
    /// Get the ISO 639-2/T code for the language.
    /// </summary>
    public string TerminologicCode
    {
      get
      {
        return _terminologicCode;
      }
    }

    /// <summary>
    /// Get an indication of whether any of the language attributes are
    /// deprecated by ISO 639.
    /// </summary>
    /// <remarks>
    /// Example: "scc" is a deprecated ISO 639-2 code for the Serbian language.
    /// </remarks>
    public bool IsDeprecated
    {
      get
      {
        return _isDeprecated;
      }
    }
  }
}