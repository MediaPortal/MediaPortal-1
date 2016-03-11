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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvLibrary.Epg
{
  [Serializable]
  public class MpGenre
  {

    private string _name = "";
    private int _id = -1;
    private bool _isMovie = false;
    private bool _enabled = true;
    private List<string> _mappedProgramGenres = new List<string>();

    /// <summary>
    /// Creates a new MediaPortal genre object.
    /// </summary>
    public MpGenre(string name, int id)
    {
      _name = name;
      _id = id;
    }

    /// <summary>
    /// Gets or sets the MediaPortal genre id.
    /// </summary>
    public int Id
    {
      get { return this._id; }
      set { this._id = value; }
    }

    /// <summary>
    /// Gets or sets the MediaPortal genre name.
    /// </summary>
    public string Name
    {
      set { this._name = value; }
      get { return this._name; }
    }

    /// <summary>
    /// Gets or sets the whether or not the MediaPortal genre is a movie genre.
    /// </summary>
    public bool IsMovie
    {
      set { this._isMovie = value; }
      get { return this._isMovie; }
    }

    /// <summary>
    /// Gets or sets the whether or not the MediaPortal genre is enabled.
    /// </summary>
    public bool Enabled
    {
      set { this._enabled = value; }
      get { return this._enabled; }
    }

    /// <summary>
    /// Gets a list of program genres mapped to thisMediaPortal genre.
    /// </summary>
    public List<string> MappedProgramGenres
    {
      get { return this._mappedProgramGenres; }
    }

    /// <summary>
    /// Maps the specified program genre to this MediaPortal genre.
    /// </summary>
    /// <param name="programGenre">The program genre to map</param>
    public void MapToProgramGenre(string programGenre)
    {
      // Don't allow duplicate entries.
      if (!_mappedProgramGenres.Contains(programGenre))
      {
        _mappedProgramGenres.Add(programGenre);
      }
    }

    /// <summary>
    /// Unmaps the specified program genre from this MediaPortal genre.
    /// </summary>
    /// <param name="programGenre">The program genre to map</param>
    public void UnmapProgramGenre(string programGenre)
    {
      _mappedProgramGenres.Remove(programGenre);
    }

  }
}
