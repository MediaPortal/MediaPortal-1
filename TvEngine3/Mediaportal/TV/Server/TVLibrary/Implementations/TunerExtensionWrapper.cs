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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  internal class TunerExtensionWrapper : ITunerExtension
  {
    private ITunerExtension _extension = null;
    private ITuner _tuner = null;
    private IChannel _currentChannel = null;

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerExtensionWrapper"/> class.
    /// </summary>
    /// <param name="extension">The <see cref="ITunerExtension"/> instance to wrap.</param>
    /// <param name="tuner">The <see cref="ITuner"/> instance that the extension is associated with.</param>
    public TunerExtensionWrapper(ITunerExtension extension, ITuner tuner)
    {
      _extension = extension;
      _tuner = tuner;
    }

    ~TunerExtensionWrapper()
    {
      Dispose(false);
    }

    public IChannel CurrentChannel
    {
      set
      {
        _currentChannel = value;
      }
    }

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public byte Priority
    {
      get
      {
        return _extension.Priority;
      }
    }

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public string Name
    {
      get
      {
        return _extension.Name;
      }
    }

    /// <summary>
    /// Does the extension control some part of the tuner hardware?
    /// </summary>
    public bool ControlsTunerHardware
    {
      get
      {
        return _extension.ControlsTunerHardware;
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      return _extension.Initialise(tunerExternalId, tunerSupportedBroadcastStandards, context);
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public void OnLoaded(ITuner tuner, out TunerAction action)
    {
      _extension.OnLoaded(_tuner, out action);
    }

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      // Do not pass this call on. The extension should not have to handle tune
      // requests for channel types that don't match the tuner type.
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before
    /// the tuner is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public void OnAfterTune(ITuner tuner, IChannel currentChannel)
    {
      // As with OnBeforeTune().
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the
    /// tuner is started but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public void OnStarted(ITuner tuner, IChannel currentChannel)
    {
      _extension.OnStarted(_tuner, _currentChannel);
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    public void OnStop(ITuner tuner, ref TunerAction action)
    {
      _extension.OnStop(_tuner, ref action);
    }

    #endregion

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        IDisposable d = _extension as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
      }
    }

    #endregion
  }
}