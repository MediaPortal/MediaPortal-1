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

using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// This class is a wrapper for tuner instances that are used as components
  /// by other tuner implementations. It eases issues with extension handling.
  /// </summary>
  internal class TunerInternalWrapper : ITunerInternal
  {
    private ITunerInternal _tuner = null;
    private IChannel _currentTuningDetail = null;
    private IList<ICustomDevice> _extensions = new List<ICustomDevice>();
    private IList<TunerExtensionWrapper> _wrappedExtensions = new List<TunerExtensionWrapper>();

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerInternalWrapper"/> class.
    /// </summary>
    /// <param name="tuner">The <see cref="ITunerInternal"/> instance to wrap.</param>
    public TunerInternalWrapper(ITunerInternal tuner)
    {
      _tuner = tuner;
    }

    #region ITunerInternal members

    /// <summary>
    /// Set the tuner's group.
    /// </summary>
    public ITunerGroup Group
    {
      set
      {
        _tuner.Group = value;
      }
    }

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      _tuner.ReloadConfiguration();
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public IList<ICustomDevice> PerformLoading()
    {
      _extensions = _tuner.PerformLoading();
      _wrappedExtensions = new List<TunerExtensionWrapper>(_extensions.Count);
      IList<ICustomDevice> extensions = new List<ICustomDevice>(_extensions.Count);
      foreach (ICustomDevice e in _extensions)
      {
        TunerExtensionWrapper we = new TunerExtensionWrapper(e, (ITVCard)_tuner);
        _wrappedExtensions.Add(we);
        extensions.Add(we);
      }
      return extensions;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    public void PerformSetTunerState(TunerState state)
    {
      _tuner.PerformSetTunerState(state);

      // We assume that the wrapped tuner is configured to stop when idle.
      if (state == TunerState.Stopped)
      {
        _currentTuningDetail = null;
        foreach (TunerExtensionWrapper we in _wrappedExtensions)
        {
          we.CurrentChannel = null;
        }
      }
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public void PerformUnloading()
    {
      _tuner.PerformUnloading();
    }

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public bool CanTune(IChannel channel)
    {
      return _tuner.CanTune(channel);
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public void PerformTuning(IChannel channel)
    {
      if (_currentTuningDetail != null && !_currentTuningDetail.IsDifferentTransponder(channel))
      {
        return;
      }

      IChannel tuneChannel = channel.GetTuningChannel();

      // Extension OnBeforeTune().
      foreach (ICustomDevice extension in _extensions)
      {
        // Notify the extension about the channel change for its tuner so it
        // has a real chance to modify the tuning parameters. Right now we
        // don't support actions... but that could be added in future if really
        // necessary.
        TunerAction extensionAction;
        extension.OnBeforeTune(_tuner as ITVCard, _currentTuningDetail, ref tuneChannel, out extensionAction);
        if (extensionAction != TunerAction.Default)
        {
          this.LogWarn("tuner internal wrapper: extension \"{0}\" wants to perform action {1}, not implemented", extension.Name, extensionAction);
        }
      }

      // If the extension implements custom tuning, assume it should be used.
      bool tuned = false;
      foreach (ICustomDevice extension in _extensions)
      {
        ICustomTuner customTuner = extension as ICustomTuner;
        if (customTuner != null && customTuner.CanTuneChannel(channel))
        {
          this.LogDebug("tuner internal wrapper: using custom tuning");
          tuned = true;
          if (!customTuner.Tune(tuneChannel))
          {
            this.LogWarn("tuner internal wrapper: custom tuning failed, falling back to default tuning");
            _tuner.PerformTuning(tuneChannel);
          }
          break;
        }
      }
      if (!tuned)
      {
        _tuner.PerformTuning(tuneChannel);
      }

      // Extension OnAfterTune().
      foreach (ICustomDevice extension in _extensions)
      {
        extension.OnAfterTune(_tuner as ITVCard, channel);
      }

      foreach (TunerExtensionWrapper we in _wrappedExtensions)
      {
        we.CurrentChannel = channel;
      }
      _currentTuningDetail = channel;
    }

    /// <summary>
    /// Allocate a new sub-channel instance.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <returns>the new sub-channel instance</returns>
    public ITvSubChannel CreateNewSubChannel(int id)
    {
      return _tuner.CreateNewSubChannel(id);
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    public void GetSignalStatus(bool onlyGetLock, out bool isLocked, out bool isPresent, out int strength, out int quality)
    {
      _tuner.GetSignalStatus(onlyGetLock, out isLocked, out isPresent, out strength, out quality);
    }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public IChannelScannerInternal InternalChannelScanningInterface
    {
      get
      {
        return _tuner.InternalChannelScanningInterface;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public IEpgGrabber InternalEpgGrabberInterface
    {
      get
      {
        return _tuner.InternalEpgGrabberInterface;
      }
    }

    #endregion

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      _tuner.Dispose();
    }

    #endregion
  }
}