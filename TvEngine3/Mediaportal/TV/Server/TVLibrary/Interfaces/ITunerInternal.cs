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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// This interface defines extensions to the <see cref="ITVCard"/> interface
  /// that we don't want to publicly expose.
  /// </summary>
  internal interface ITunerInternal : IDisposable
  {
    #region remove these

    // TODO I'd like to remove these methods because this interface is intended to describe
    // the functions that must be overriden/implemented in order to successfully reuse TvCardBase
    // via inherritence... and in general these functions shouldn't or can't be overriden

    /// <summary>
    /// Set the tuner's group.
    /// </summary>
    ITunerGroup Group
    {
      set;
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    IEpgGrabber EpgGrabberInterface
    {
      get;
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    ITVScanning ScanningInterface
    {
      get;
    }

    #endregion

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    void ReloadConfiguration();

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    bool CanTune(IChannel channel);

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    void PerformLoading();

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    void PerformUnloading();

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    void PerformSignalStatusUpdate(bool onlyUpdateLock);

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="id">The identifier for the subchannel.</param>
    /// <returns>the new subchannel instance</returns>
    ITvSubChannel CreateNewSubChannel(int id);

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    void PerformTuning(IChannel channel);

    /// <summary>
    /// Set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    void SetTunerState(TunerState state);
  }
}