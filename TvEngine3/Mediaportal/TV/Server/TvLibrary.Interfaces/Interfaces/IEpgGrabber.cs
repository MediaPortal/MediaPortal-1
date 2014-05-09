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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces
{
  /// <summary>
  /// Tuner in band electronic programme guide data grabber interface.
  /// </summary>
  public interface IEpgGrabber
  {
    /// <summary>
    /// Reload the grabber's configuration.
    /// </summary>
    void ReloadConfiguration();

    /// <summary>
    /// Start grabbing electronic programme guide data.
    /// </summary>
    /// <param name="tuningDetail">The current transponder/multiplex tuning details.</param>
    /// <param name="callBack">The delegate to notify when grabbing is complete or canceled.</param>
    void GrabEpg(IChannel tuningDetail, IEpgGrabberCallBack callBack);

    /// <summary>
    /// Get the grabber's current status.
    /// </summary>
    /// <value><c>true</c> if the grabber is grabbing, otherwise <c>false</c></value>
    bool IsEpgGrabbing
    {
      get;
    }

    /// <summary>
    /// Abort grabbing electronic programme guide data.
    /// </summary>
    void AbortGrabbing();
  }
}