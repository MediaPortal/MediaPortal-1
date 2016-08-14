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
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension
{
  /// <summary>
  /// A base interface for tuners that support extended features and functions.
  /// </summary>
  public interface ITunerExtension
  {
    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    /// <remarks>
    /// Tuner extension loading/detection is done in order of descending
    /// priority. This approach allows certain driver interface conflicts to be
    /// avoided. Priority ranges from 100 (highest priority) to 1 (lowest
    /// priority).
    /// </remarks>
    byte Priority
    {
      get;
    }

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    /// <remarks>
    /// This could be a manufacturer or reseller name, or even a model name
    /// and/or number.
    /// </remarks>
    string Name
    {
      get;
    }

    /// <summary>
    /// Does the extension control some part of the tuner hardware?
    /// </summary>
    /// <remarks>
    /// This property is used to determine whether it is possible to load
    /// additional extensions which implement interfaces that are also
    /// implemented by extensions that have already been loaded.
    /// </remarks>
    bool ControlsTunerHardware
    {
      get;
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <remarks>
    /// If initialisation fails, the <see cref="ITunerExtension"/> instance
    /// should be disposed immediately.
    /// </remarks>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context);

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <remarks>
    /// Example usage: start or reconfigure the tuner.
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    void OnLoaded(ITuner tuner, out TunerAction action);

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <remarks>
    /// Example usage: tweak tuning parameters or force the tuner into a
    /// particular state before the tune request is submitted.
    /// Process: [this call back] -> assemble and submit tune request -> OnAfterTune() -> start tuner -> OnRunning() -> lock check
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action);

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before
    /// the tuner is started.
    /// </summary>
    /// <remarks>
    /// Example usage: call a function that must be called at this specific
    /// stage in the tuning process.
    /// Process: OnBeforeTune() -> assemble and submit tune request -> [this call back] -> start tuner -> OnRunning() -> lock check
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    void OnAfterTune(ITuner tuner, IChannel currentChannel);

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the
    /// tuner is started but before signal lock is checked.
    /// </summary>
    /// <remarks>
    /// Example usage: call a function that must be called at this specific stage in the tuning process.
    /// Process: OnBeforeTune() -> assemble and submit tune request -> OnAfterTune() -> start tuner -> [this call back] -> lock check
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    void OnStarted(ITuner tuner, IChannel currentChannel);

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <remarks>
    /// Example usage: don't allow the tuner to be stopped to ensure optimal
    /// operation or compatibility.
    /// </remarks>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    void OnStop(ITuner tuner, ref TunerAction action);

    #endregion
  }

  /// <summary>
  /// A base implementation of <see cref="ITunerExtension"/> to minimise
  /// implementation effort.
  /// </summary>
  public abstract class BaseTunerExtension : ITunerExtension
  {
    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public virtual byte Priority
    {
      get
      {
        // The following comments should be taken into account when considering
        // how to set extension priority...
        //
        // CONEXANT
        // - TeVii, Hauppauge, Geniatech, Turbosight, DVBSky, Prof and possibly
        //   others all use or implement the same Conexant property set for
        //   DiSEqC support.
        // - Some TeVii drivers implement an additional unique property set and
        //   do not implement the Conexant property set.
        // - TeVii hardware can be identified using their SDK DLL.
        // - DVBSky drivers implement an additional unique property set.
        // - Hauppauge drivers implement an additional property set (also
        //   used by DVBSky, but not the same as the unique DVBSky set).
        // - TBS drivers implement additional unique properties... sometimes.
        // - The Prof SDK says to test for the USB interface before testing the
        //   PCIe/PCI interface.
        // - Geniatech drivers implement a unique property.
        //
        // The following priority hierarchy is used to avoid problems:
        // DVBSky, TeVii SDK/API [75] > Hauppauge, TeVii BDA, Turbosight [70] > Prof (USB) [65] > Prof (PCI, PCIe) [60] > Geniatech [50] > Conexant [40]
        //
        // OTHER
        // - KNC and Omicom implement the same property set for DiSEqC support.
        //
        // The following priority hierarchy is used to avoid problems:
        // KNC [60] > Omicom [50]
        return 50;
      }
    }

    /// <summary>
    /// A human-readable name for the extension.
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
    /// Does the extension control some part of the tuner hardware?
    /// </summary>
    public virtual bool ControlsTunerHardware
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public virtual bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      // This base class is not intended to be instantiated.
      return false;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked when the tuner has been successfully loaded.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">The action to take, if any.</param>
    public virtual void OnLoaded(ITuner tuner, out TunerAction action)
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
    public virtual void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      action = TunerAction.Default;
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted but before
    /// the tuner is started.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner has been tuned to.</param>
    public virtual void OnAfterTune(ITuner tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the
    /// tuner is started but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public virtual void OnStarted(ITuner tuner, IChannel currentChannel)
    {
    }

    /// <summary>
    /// This call back is invoked before the tuner is stopped.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="action">As an input, the action that the TV Engine wants to take; as an output, the action to take.</param>
    public virtual void OnStop(ITuner tuner, ref TunerAction action)
    {
    }

    #endregion
  }
}
