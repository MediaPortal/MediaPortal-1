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

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// interface which describes a MediaPortal genre
  /// </summary>
  public interface IMpGenre
  {
    /// <summary>
    /// Gets the MediaPortal genre id.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Gets or sets the MediaPortal genre name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets the whether or not the MediaPortal genre is a movie genre.
    /// </summary>
    bool IsMovie { get; set; }

    /// <summary>
    /// Gets or sets the whether or not the MediaPortal genre is enabled.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Gets a list of program genres mapped to thisMediaPortal genre.
    /// </summary>
    List<string> MappedProgramGenres { get; }

    /// <summary>
    /// Maps the specified program genre to this MediaPortal genre.
    /// </summary>
    /// <param name="programGenre">The program genre to map</param>
    void MapToProgramGenre(string programGenre);

    /// <summary>
    /// Unmaps the specified program genre from this MediaPortal genre.
    /// </summary>
    /// <param name="programGenre">The program genre to unmap</param>
    void UnmapProgramGenre(string programGenre);
  }
}