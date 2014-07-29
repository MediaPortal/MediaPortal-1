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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  internal class TunerExtensionWrapper : ICustomDevice
  {
    private ICustomDevice _extension = null;
    private ITVCard _tuner = null;
    private IChannel _currentChannel = null;

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerExtensionWrapper"/> class.
    /// </summary>
    /// <param name="extension">The <see cref="ICustomDevice"/> instance to wrap.</param>
    /// <param name="tuner">The <see cref="ITVCard"/> instance that the extension is associated with.</param>
    public TunerExtensionWrapper(ICustomDevice extension, ITVCard tuner)
    {
      _extension = extension;
      _tuner = tuner;
    }

    public IChannel CurrentChannel
    {
      set
      {
        _currentChannel = value;
      }
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public byte Priority
    {
      get
      {
        return _extension.Priority;
      }
    }

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public string Name
    {
      get
      {
        return _extension.Name;
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// the ICustomDevice instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      return _extension.Initialise(tunerExternalId, tunerType, context);
    }

    #region device state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public void OnLoaded(ITVCard tuner, out TunerAction action)
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
    public void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      // Do not pass this call on. The extension should not have to handle tune
      // requests for channel types that don't match the tuner type.
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before the device is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
      // As with OnBeforeTune().
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the tuner is started but
    /// before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public void OnStarted(ITVCard tuner, IChannel currentChannel)
    {
      _extension.OnStarted(_tuner, _currentChannel);
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    public void OnStop(ITVCard tuner, ref TunerAction action)
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
      _extension.Dispose();
    }

    #endregion
  }
}