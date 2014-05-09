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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension
{
  /// <summary>
  /// A base interface for devices that support extended functions.
  /// </summary>
  public interface ICustomDevice : IDisposable
  {
    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    /// <remarks>
    /// Custom device loading/detection is done in order of descending priority, and custom devices that
    /// implement the <see cref="IDirectShowAddOnDevice"/> interface are loaded before other devices. This
    /// approach allows certain driver interface conflicts to be avoided. Priority ranges from 100
    /// (highest priority) to 1 (lowest priority).
    /// </remarks>
    byte Priority { get; }

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    bool Initialise(string tunerExternalId, CardType tunerType, object context);

    #region device state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <remarks>
    /// Example usage: start or reconfigure the device.
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    void OnLoaded(ITVCard tuner, out TunerAction action);

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <remarks>
    /// Example usage: tweak tuning parameters or force the device's BDA graph into a particular state before
    /// the tune request is submitted.
    /// Process: [this call back] -> assemble and submit tune request -> OnAfterTune() -> start device -> OnRunning() -> lock check
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action);

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before the device is started.
    /// </summary>
    /// <remarks>
    /// Example usage: call a function that must be called at this specific stage in the tuning process.
    /// Process: OnBeforeTune() -> assemble and submit tune request -> [this call back] -> start device -> OnRunning() -> lock check
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    void OnAfterTune(ITVCard tuner, IChannel currentChannel);

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the tuner is started but
    /// before signal lock is checked.
    /// </summary>
    /// <remarks>
    /// Example usage: call a function that must be called at this specific stage in the tuning process.
    /// Process: OnBeforeTune() -> assemble and submit tune request -> OnAfterTune() -> start device -> [this call back] -> lock check
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    void OnStarted(ITVCard tuner, IChannel currentChannel);

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <remarks>
    /// Example usage: don't allow the device to be stopped to ensure optimal operation or compatibility.
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    void OnStop(ITVCard tuner, ref TunerAction action);

    #endregion
  }

  /// <summary>
  /// A base implementation of ICustomDevice to minimise implementation effort for other device classes.
  /// </summary>
  public abstract class BaseCustomDevice : ICustomDevice
  {
    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public virtual byte Priority
    {
      get
      {
        // The following comments should be taken into account when considering how to set device priority...
        // - TeVii, Hauppauge, Geniatech, Turbosight, DVBSky, Prof and possibly others all use or implement the
        //   same Conexant property set for DiSEqC support, often adding custom extensions.
        // - TeVii differentiates itself with a DLL that identifies its devices.
        // - DVBSky drivers implement a unique property set, but also implement the Hauppauge property set.
        // - Hauppauge drivers implement a unique property set.
        // - The Turbosight plugin limits support by matching the tuner name.
        // - The Prof SDK says to test for the USB interface before testing the PCIe/PCI interface.
        // - The Geniatech plugin checks for support of a unique property.
        //
        // The following priority hierarchy is used:
        // DVBSky, TeVii [75] > Hauppauge, Turbosight [70] > Prof (USB) [65] > Prof (PCI, PCIe) [60] > Geniatech [50] > Conexant [40]
        return 50;
      }
    }

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public virtual string Name
    {
      get
      {
        return GetType().Name;
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
    public virtual bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      // This base class is not intended to be instantiated.
      return false;
    }

    #region device state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnLoaded(ITVCard tuner, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnBeforeTune(ITVCard tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before the device is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the tuner is started but
    /// before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnStarted(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITVCard tuner, ref TunerAction action)
    {
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public virtual void Dispose()
    {
    }

    #endregion
  }
}
