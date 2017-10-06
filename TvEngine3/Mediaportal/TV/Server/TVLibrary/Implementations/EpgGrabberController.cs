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

using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// An implementation of <see cref="IEpgGrabberController"/> for a tuner's
  /// electronic programme guide data grabber.
  /// </summary>
  internal class EpgGrabberController : IEpgGrabberController
  {
    private ISubChannelManager _subChannelManager = null;

    /// <summary>
    /// Initialise a new instance of the <see cref="EpgGrabberController"/> class.
    /// </summary>
    /// <param name="subChannelManager">The tuner's sub-channel manager.</param>
    public EpgGrabberController(ISubChannelManager subChannelManager)
    {
      _subChannelManager = subChannelManager;
    }

    #region IBackgroundEpgGrabberController members

    /// <summary>
    /// Enable the electronic programme guide data grabber.
    /// </summary>
    public void Enable()
    {
      _subChannelManager.IsEpgGrabbingEnabled = true;
    }

    /// <summary>
    /// Disable the electronic programme guide data grabber.
    /// </summary>
    public void Disable()
    {
      _subChannelManager.IsEpgGrabbingEnabled = false;
    }

    /// <summary>
    /// Indicates whether the electronic programme guide data grabber is enabled.
    /// </summary>
    /// <returns><c>true</c> if the electronic programme guide data grabber is enabled, otherwise <c>false</c></returns>
    public bool IsEnabled()
    {
      return _subChannelManager.IsEpgGrabbingEnabled;
    }

    #endregion
  }
}