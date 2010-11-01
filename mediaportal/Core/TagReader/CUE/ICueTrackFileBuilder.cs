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

using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.TagReader
{
  /// <summary>
  /// Interface for building objects like GUIListItem
  /// from a Cue track
  /// Additionally makes some kind of adaptation via getFileName
  /// </summary>
  public interface ICueTrackFileBuilder<T>
  {
    /// <summary>
    /// get file name
    /// </summary>
    /// <param name="fobj">Object compatible to this builder/adaptor</param>
    /// <returns>file name for fobj</returns>
    string getFileName(T fobj);

    /// <summary>
    /// builds object T from Cue sheet and track
    /// </summary>
    /// <param name="cueFileName">Cue sheet file name</param>
    /// <param name="cueSheet">CueSheet</param>
    /// <param name="track">Track</param>
    /// <returns>object T constructed from cueSheet and Track</returns>
    T build(string cueFileName, CueSheet cueSheet, Track track);
  }
}