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
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// This interface defines extensions to the <see cref="ITuner"/> interface
  /// that we don't want to publicly expose.
  /// </summary>
  internal interface ITunerInternal : IDisposable
  {
    #region remove these

    // TODO I'd like to remove these methods because this interface is intended to describe the
    // functions that must be overriden/implemented in order to successfully reuse TunerBase via
    // inheritance... and in general these functions shouldn't or can't be overriden

    /// <summary>
    /// Set the tuner's group.
    /// </summary>
    ITunerGroup Group
    {
      set;
    }

    #endregion

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    void ReloadConfiguration(TVDatabase.Entities.Tuner configuration);

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default);

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    void PerformSetTunerState(TunerState state, bool isFinalising = false);

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    void PerformUnloading(bool isFinalising = false);

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    bool CanTune(IChannel channel);

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    void PerformTuning(IChannel channel);

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <remarks>
    /// The <paramref name="onlyGetLock"/> parameter exists as a speed
    /// optimisation. Getting strength and quality readings can be slow.
    /// </remarks>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock);

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's sub-channel manager.
    /// </summary>
    ISubChannelManager SubChannelManager
    {
      get;
    }

    /// <summary>
    /// Get the tuner's channel linkage scanning interface.
    /// </summary>
    IChannelLinkageScanner InternalChannelLinkageScanningInterface
    {
      get;
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    IChannelScannerInternal InternalChannelScanningInterface
    {
      get;
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    IEpgGrabberInternal InternalEpgGrabberInterface
    {
      get;
    }

    #endregion
  }
}