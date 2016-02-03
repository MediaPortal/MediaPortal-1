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

namespace MediaPortal.Player
{
  /// <summary>
  /// This file contains an interface description for external audio players
  /// MP can use its internal player to play audio files or use an external audio player
  /// like winamp, foobar or,.... to play music
  /// By implementing this interface you can add support for your own external audio player
  /// </summary>
  public interface IExternalPlayer
  {
    bool Enabled { get; set; }
    /// <summary>
    /// This method returns the name of the external player
    /// </summary>
    /// <returns>string representing the name of the external player</returns>
    string PlayerName { get; }

    /// <summary>
    /// This method returns the version number of the plugin
    /// </summary>
    string VersionNumber { get; }

    /// <summary>
    /// This method returns the author of the external player
    /// </summary>
    /// <returns></returns>
    string AuthorName { get; }

    /// <summary>
    /// Returns all the extensions that the external player supports.  
    /// The return value is an array of extensions of the form: .wma, .mp3, etc...
    /// </summary>
    /// <returns>array of strings of extensions in the form: .wma, .mp3, etc..</returns>
    string[] GetAllSupportedExtensions();


    /// <summary>
    /// Returns true or false depending if the filename passed is supported or not.
    /// The filename could be just the filename or the complete path of a file.
    /// </summary>
    /// <param name="filename">a fully qualified path and filename or just the filename</param>
    /// <returns>true or false if the file is supported by the player</returns>
    bool SupportsFile(string filename);
  }
}