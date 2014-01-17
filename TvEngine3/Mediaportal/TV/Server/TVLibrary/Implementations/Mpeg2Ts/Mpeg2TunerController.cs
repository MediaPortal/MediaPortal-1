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
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  public class Mpeg2TunerController
  {
    /// <summary>
    /// The mode to use for controlling tuner PID filter(s).
    /// </summary>
    /// <remarks>
    /// This setting can be used to enable or disable the tuner's PID filter even when the tuning context
    /// (for example, DVB-S vs. DVB-S2) would usually result in different behaviour. Note that it is usually
    /// not ideal to have to manually enable or disable a PID filter as it can affect tuning reliability.
    /// </remarks>
    protected PidFilterMode _pidFilterMode = PidFilterMode.Auto;

    /// <summary>
    /// Configure the tuner's PID filter(s) to enable receiving the PIDs for each of the current subchannels.
    /// </summary>
    protected void ConfigurePidFilter()
    {
      this.LogDebug("Mpeg2TunerController: configure PID filter, mode = {0}", _pidFilterMode);

      if (_mapSubChannels == null || _mapSubChannels.Count == 0)
      {
        this.LogDebug("Mpeg2TunerController: no subchannels");
        return;
      }

      HashSet<ushort> pidSet = null;
      foreach (ICustomDevice d in _customDeviceInterfaces)
      {
        IMpeg2PidFilter filter = d as IMpeg2PidFilter;
        if (filter != null)
        {
          this.LogDebug("Mpeg2TunerController: found PID filter controller interface");

          if (_pidFilterMode == PidFilterMode.Disabled)
          {
            filter.SetFilterPids(null, modulation, false);
            continue;
          }

          if (pidSet == null)
          {
            this.LogDebug("Mpeg2TunerController: assembling PID list");
            pidSet = new HashSet<ushort>();
            int count = 1;
            foreach (ITvSubChannel subchannel in _mapSubChannels.Values)
            {
              TvDvbChannel dvbChannel = subchannel as TvDvbChannel;
              if (dvbChannel != null && dvbChannel.Pids != null)
              {
                // Build a distinct super-set of PIDs used by the subchannels.
                foreach (ushort pid in dvbChannel.Pids)
                {
                  if (!pidSet.Contains(pid))
                  {
                    this.LogDebug("  {0, -2} = {1} (0x{1:x})", count++, pid);
                    pidSet.Add(pid);
                  }
                }
              }
            }
          }
          filter.SetFilterState(pidSet, _pidFilterMode == PidFilterMode.Enabled);
        }
      }
    }

    /// <summary>
    /// Update the list of services being decrypted by the device's conditional access interfaces(s).
    /// </summary>
    /// <remarks>
    /// The strategy here is usually to only send commands to the CAM when we need an *additional* service
    /// to be decrypted. The *only* exception is when we have to stop decrypting services in "changes" mode.
    /// We don't send "not selected" commands for "list" or "only" mode because this can disrupt the other
    /// services that still need to be decrypted. We also don't send "keep decrypting" commands (alternative
    /// to "not selected") because that will almost certainly cause glitches in streams.
    /// </remarks>
    /// <param name="subChannelId">The ID of the subchannel causing this update.</param>
    /// <param name="updateAction"><c>Add</c> if the subchannel is being tuned, <c>update</c> if the PMT for the
    ///   subchannel has changed, or <c>last</c> if the subchannel is being disposed.</param>
    protected void UpdateDecryptList(int subChannelId, CaPmtListManagementAction updateAction)
    {
      this.LogDebug("Mpeg2TunerController: subchannel {0} update decrypt list, mode = {1}, update action = {2}", subChannelId, _multiChannelDecryptMode, updateAction);

      if (_mapSubChannels == null || _mapSubChannels.Count == 0 || !_mapSubChannels.ContainsKey(subChannelId))
      {
        this.LogDebug("Mpeg2TunerController: subchannel not found");
        return;
      }
      if (_mapSubChannels[subChannelId].CurrentChannel.FreeToAir)
      {
        this.LogDebug("Mpeg2TunerController: service is not encrypted");
        return;
      }
      if (updateAction == CaPmtListManagementAction.Last && _multiChannelDecryptMode != MultiChannelDecryptMode.Changes)
      {
        this.LogDebug("Mpeg2TunerController: \"not selected\" command acknowledged, no action required");
        return;
      }

      // First build a distinct list of the services that we need to handle.
      this.LogDebug("Mpeg2TunerController: assembling service list");
      List<ITvSubChannel> distinctServices = new List<ITvSubChannel>();
      Dictionary<int, ITvSubChannel>.ValueCollection.Enumerator en = _mapSubChannels.Values.GetEnumerator();
      DVBBaseChannel updatedDigitalService = _mapSubChannels[subChannelId].CurrentChannel as DVBBaseChannel;
      AnalogChannel updatedAnalogService = _mapSubChannels[subChannelId].CurrentChannel as AnalogChannel;
      while (en.MoveNext())
      {
        IChannel service = en.Current.CurrentChannel;
        // We don't care about FTA services here.
        if (service.FreeToAir)
        {
          continue;
        }

        // Keep an eye out - if there is another subchannel accessing the same service as the subchannel that
        // is being updated then we always do *nothing* unless this is specifically an update request. In any other
        // situation, if we were to stop decrypting the service it would be wrong; if we were to start decrypting
        // the service it would be unnecessary and possibly cause stream interruptions.
        if (en.Current.SubChannelId != subChannelId && updateAction != CaPmtListManagementAction.Update)
        {
          if (updatedDigitalService != null)
          {
            DVBBaseChannel digitalService = service as DVBBaseChannel;
            if (digitalService != null && digitalService.ServiceId == updatedDigitalService.ServiceId)
            {
              this.LogDebug("Mpeg2TunerController: the service for this subchannel is a duplicate, no action required");
              return;
            }
          }
          else if (updatedAnalogService != null)
          {
            AnalogChannel analogService = service as AnalogChannel;
            if (analogService != null && analogService.Frequency == updatedAnalogService.Frequency && analogService.ChannelNumber == updatedAnalogService.ChannelNumber)
            {
              this.LogDebug("Mpeg2TunerController: the service for this subchannel is a duplicate, no action required");
              return;
            }
          }
          else
          {
            throw new TvException("Mpeg2TunerController: service type not recognised, unable to assemble decrypt service list\r\n" + service.ToString());
          }
        }

        if (_multiChannelDecryptMode == MultiChannelDecryptMode.List)
        {
          // Check for "list" mode: have we already go this service in our distinct list? If so, don't add it
          // again...
          bool exists = false;
          foreach (ITvSubChannel serviceToDecrypt in distinctServices)
          {
            DVBBaseChannel digitalService = service as DVBBaseChannel;
            if (digitalService != null)
            {
              if (digitalService.ServiceId == ((DVBBaseChannel)serviceToDecrypt.CurrentChannel).ServiceId)
              {
                exists = true;
                break;
              }
            }
            else
            {
              AnalogChannel analogService = service as AnalogChannel;
              if (analogService != null)
              {
                if (analogService.Frequency == ((AnalogChannel)serviceToDecrypt.CurrentChannel).Frequency &&
                  analogService.ChannelNumber == ((AnalogChannel)serviceToDecrypt.CurrentChannel).ChannelNumber)
                {
                  exists = true;
                  break;
                }
              }
              else
              {
                throw new TvException("Mpeg2TunerController: service type not recognised, unable to assemble decrypt service list\r\n" + service.ToString());
              }
            }
          }
          if (!exists)
          {
            distinctServices.Add(en.Current);
          }
        }
        else if (en.Current.SubChannelId == subChannelId)
        {
          // For "changes" and "only" modes: we only send one command and that relates to the service being updated.
          distinctServices.Add(en.Current);
        }
      }

      // This should never happen, regardless of the action that is being performed. Note that this is just a
      // sanity check. It is expected that the service will manage decrypt limit logic. This check does not work
      // for "changes" mode.
      if (_decryptLimit > 0 && distinctServices.Count > _decryptLimit)
      {
        this.LogDebug("Mpeg2TunerController: decrypt limit exceeded");
        return;
      }
      if (distinctServices.Count == 0)
      {
        this.LogDebug("Mpeg2TunerController: no services to update");
        return;
      }

      // Identify the conditional access interface(s) and send the service list.
      bool foundCaProvider = false;
      for (int attempt = 1; attempt <= _decryptFailureRetryCount + 1; attempt++)
      {
        ThrowExceptionIfTuneCancelled();
        if (attempt > 1)
        {
          this.LogDebug("Mpeg2TunerController: attempt {0}...", attempt);
        }

        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          IConditionalAccessProvider caProvider = deviceInterface as IConditionalAccessProvider;
          if (caProvider == null)
          {
            continue;
          }

          this.LogDebug("Mpeg2TunerController: CA provider {0}...", caProvider.Name);
          foundCaProvider = true;

          if (_waitUntilCaInterfaceReady && !caProvider.IsInterfaceReady())
          {
            this.LogDebug("Mpeg2TunerController: provider is not ready, waiting for up to 15 seconds", caProvider.Name);
            DateTime startWait = DateTime.Now;
            TimeSpan waitTime = new TimeSpan(0);
            while (waitTime.TotalMilliseconds < 15000)
            {
              ThrowExceptionIfTuneCancelled();
              System.Threading.Thread.Sleep(200);
              waitTime = DateTime.Now - startWait;
              if (caProvider.IsInterfaceReady())
              {
                this.LogDebug("Mpeg2TunerController: provider ready after {0} ms", waitTime.TotalMilliseconds);
                break;
              }
            }
          }

          // Ready or not, we send commands now.
          this.LogDebug("Mpeg2TunerController: sending command(s)");
          bool success = true;
          TvDvbChannel digitalService;
          // The default action is "more" - this will be changed below if necessary.
          CaPmtListManagementAction action = CaPmtListManagementAction.More;

          // The command is "start/continue descrambling" unless we're removing services.
          CaPmtCommand command = CaPmtCommand.OkDescrambling;
          if (updateAction == CaPmtListManagementAction.Last)
          {
            command = CaPmtCommand.NotSelected;
          }
          for (int i = 0; i < distinctServices.Count; i++)
          {
            ThrowExceptionIfTuneCancelled();
            if (i == 0)
            {
              if (distinctServices.Count == 1)
              {
                if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
                {
                  // Remove a service...
                  if (updateAction == CaPmtListManagementAction.Last)
                  {
                    action = CaPmtListManagementAction.Only;
                  }
                  // Add or update a service...
                  else
                  {
                    action = updateAction;
                  }
                }
                else
                {
                  action = CaPmtListManagementAction.Only;
                }
              }
              else
              {
                action = CaPmtListManagementAction.First;
              }
            }
            else if (i == distinctServices.Count - 1)
            {
              action = CaPmtListManagementAction.Last;
            }
            else
            {
              action = CaPmtListManagementAction.More;
            }

            this.LogDebug("  command = {0}, action = {1}, service = {2}", command, action, distinctServices[i].CurrentChannel.Name);
            digitalService = distinctServices[i] as TvDvbChannel;
            if (digitalService == null)
            {
              success &= caProvider.SendCommand(distinctServices[i].CurrentChannel, action, command, null, null);
            }
            else
            {
              success &= caProvider.SendCommand(distinctServices[i].CurrentChannel, action, command, digitalService.Pmt, digitalService.Cat);
            }
          }

          // Are we done?
          if (success)
          {
            return;
          }
        }

        if (!foundCaProvider)
        {
          this.LogDebug("Mpeg2TunerController: no CA providers identified");
          return;
        }
      }
    }
  }
}
