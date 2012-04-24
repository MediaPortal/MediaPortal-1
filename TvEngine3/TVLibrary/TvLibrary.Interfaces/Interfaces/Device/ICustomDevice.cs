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

namespace TvLibrary.Interfaces.Device
{
  /// <summary>
  /// A base interface for devices that support extended functions.
  /// </summary>
  public interface ICustomDevice : IDisposable
  {
    /// <summary>
    /// The loading priority for this device type. Custom device loading/detection is done in order of
    /// ascending priority. This approach allows certain driver interface conflicts to be avoided. Priority
    /// ranges from 1 (highest priority) to 100 (lowest priority).
    /// </summary>
    byte Priority { get; }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    String Name { get; }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath);

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces, or that need
    /// to be tweaked in order for the standard BDA tuning process to succeed.
    /// </summary>
    /// <param name="channel">The channel that will be tuned.</param>
    void SetTuningParameters(ref IChannel channel);

    #region graph state change callbacks

    /// <summary>
    /// This callback is invoked after a tune request is submitted and the BDA graph is started, but before
    /// signal lock is checked.
    /// Process: submit tune request -> (graph not running) -> start graph -> callback -> lock check
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    void OnGraphStarted(ITVCard tuner, IChannel currentChannel);

    /// <summary>
    /// This callback is invoked after a tune request is submitted but before signal lock is checked when
    /// the BDA graph is already running.
    /// Process: submit tune request -> (graph already running) -> callback -> lock check
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    void OnGraphStart(ITVCard tuner, IChannel currentChannel);

    /// <summary>
    /// This callback is invoked after the BDA graph is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    void OnGraphStop(ITVCard tuner);

    /// <summary>
    /// This callback is invoked after the BDA graph is paused.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    void OnGraphPause(ITVCard tuner);

    #endregion
  }

  /// <summary>
  /// A base implementation of ICustomDevice to minimise implementation effort for other device classes.
  /// </summary>
  public abstract class BaseCustomDevice : ICustomDevice
  {
    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    /// <remarks>
    /// Custom device loading/detection is done in order of ascending priority. This approach allows
    /// certain driver interface conflicts to be avoided. Priority ranges from 1 (highest priority) to
    /// 100 (lowest priority).
    /// </remarks>
    public virtual byte Priority
    {
      get
      {
        return 50;
      }
    }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public virtual String Name
    {
      get
      {
        return this.GetType().ToString();
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public virtual bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      // This base class is not intended to be instantiated.
      return false;
    }

    /// <summary>
    /// Set tuning parameters that can or could not previously be set through BDA interfaces, or that need
    /// to be tweaked in order for the standard BDA tuning process to succeed.
    /// </summary>
    /// <param name="channel">The channel that will be tuned.</param>
    public virtual void SetTuningParameters(ref IChannel channel)
    {
    }

    #region graph state change callbacks

    /// <summary>
    /// This callback is invoked after a tune request is submitted and the BDA graph is started, but before
    /// signal lock is checked.
    /// Process: submit tune request -> (graph not running) -> start graph -> callback -> lock check
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnGraphStarted(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This callback is invoked after a tune request is submitted but before signal lock is checked when
    /// the BDA graph is already running.
    /// Process: submit tune request -> (graph already running) -> callback -> lock check
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnGraphStart(ITVCard tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This callback is invoked after the BDA graph is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    public virtual void OnGraphStop(ITVCard tuner)
    {
    }

    /// <summary>
    /// This callback is invoked after the BDA graph is paused.
    /// </summary>
    /// <param name="tuner">The tuner instance that this device instance is associated with.</param>
    public virtual void OnGraphPause(ITVCard tuner)
    {
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public virtual void Dispose()
    {
    }

    #endregion
  }
}
