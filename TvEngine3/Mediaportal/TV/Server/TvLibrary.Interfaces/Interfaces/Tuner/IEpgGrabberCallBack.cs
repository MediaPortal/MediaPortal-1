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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner
{
  /// <summary>
  /// Call-back interface for EPG grabbing events.
  /// </summary>
  public interface IEpgGrabberCallBack
  {
    /// <summary>
    /// Called when grabbing is started because the tuner (re)tunes or
    /// configuration changes.
    /// </summary>
    void OnGrabbingStarted();

    /// <summary>
    /// Called when grabbing is complete and data is ready for processing.
    /// </summary>
    void OnEpgDataReady();

    /// <summary>
    /// Called when grabbing is stopped because the tuner is no longer tuned or
    /// configuration changes.
    /// </summary>
    void OnGrabbingStopped();
  }
}