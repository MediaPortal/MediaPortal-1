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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using System;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts
{
  internal class SubChannelManagerMpeg2Ts
  {
    /// <summary>
    /// The number of times to re-attempt decrypting the current service set
    /// when one or more services are not able to be decrypted for whatever
    /// reason.
    /// </summary>
    /// <remarks>
    /// Each available CA interface will be tried in order of priority. If
    /// decrypting is not started successfully, all interfaces are retried
    /// until each interface has been tried _decryptFailureRetryCount + 1
    /// times, or until decrypting is successful.
    /// </remarks>
    private int _decryptFailureRetryCount = 2;

    /// <summary>
    /// Is the conditional access table required in order for any or all of the
    /// conditional access providers to decrypt programs.
    /// </summary>
    private bool _isConditionalAccessTableRequired = false;

    /// <summary>
    /// Enable or disable waiting for the conditional interface to be ready
    /// before sending commands.
    /// </summary>
    private bool _waitUntilCaInterfaceReady = true;

    /// <summary>
    /// Enable or disable the use of conditional access interface(s).
    /// </summary>
    private bool _useConditionalAccessInterface = true;

    // TODO load value of _useConditionalAccessInterface and _pidFilterMode



    private int _nextSubChannelId = 0;
    private IList<IConditionalAccessProvider> _caProviders = new List<IConditionalAccessProvider>();
    private IList<IMpeg2PidFilter> _pidFilters = new List<IMpeg2PidFilter>();
    private ITsFilter _tsFilter = null;
    private bool _supportsSubChannels = true;
    private Dictionary<int, ISubChannelInternal> _subChannels = new Dictionary<int, ISubChannelInternal>();
    private MultiChannelDecryptMode _multiChannelDecryptMode = MultiChannelDecryptMode.List;

    public SubChannelManagerMpeg2Ts(ICollection<ITunerExtension> extensions, ITsFilter tsFilter, bool supportsSubChannels = true)
    {
      _isConditionalAccessTableRequired = false;
      foreach (ITunerExtension extension in extensions)
      {
        IConditionalAccessProvider caProvider = extension as IConditionalAccessProvider;
        if (caProvider != null)
        {
          _caProviders.Add(caProvider);
          _isConditionalAccessTableRequired |= caProvider.IsConditionalAccessTableRequiredForDecryption();
        }
        IMpeg2PidFilter pidFilter = extension as IMpeg2PidFilter;
        if (pidFilter != null)
        {
          _pidFilters.Add(pidFilter);
        }
      }
      _tsFilter = tsFilter;
      _supportsSubChannels = supportsSubChannels;
    }

    /// <summary>
    /// Allocate a new sub-channel, or retrieve an existing sub-channel.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <param name="channel">The channel to associate with the sub-channel.</param>
    /// <returns>the sub-channel</returns>
    public ISubChannel CreateNewOrGetExistingSubChannel(int id, IChannel channel)
    {
      // Some tuners (for example: CableCARD tuners) are only able to
      // deliver one program... full stop.
      ISubChannelInternal subChannel = null;
      if (!_supportsSubChannels && _subChannels.Count > 0)
      {
        if (_subChannels.TryGetValue(id, out subChannel))
        {
          // Existing sub-channel.
          if (_subChannels.Count != 1)
          {
            // If this is not the only sub-channel then by definition this
            // must be an attempt to tune a new program. Not allowed.
            throw new TvException("Tuner is not able to receive more than one program.");
          }
        }
        else
        {
          // New sub-channel.
          Dictionary<int, ISubChannelInternal>.ValueCollection.Enumerator en = _subChannels.Values.GetEnumerator();
          en.MoveNext();
          if (en.Current.CurrentChannel != channel)
          {
            // The tuner is currently streaming a different program.
            throw new TvException("Tuner is not able to receive more than one program.");
          }
        }
      }

      // Get a sub-channel for the program.
      string description;
      if (subChannel == null && !_subChannels.TryGetValue(id, out subChannel))
      {
        description = "creating new sub-channel";
        id = _nextSubChannelId++;
        //subChannel = new NewSubChannelMpeg2Ts(this, id, _tsFilter, _isConditionalAccessTableRequired);
        _subChannels[id] = subChannel;
        // TODO FireNewSubChannelEvent(id);
      }
      else
      {
        description = "using existing sub-channel";

        // If reusing a sub-channel and our multi-channel decrypt mode is
        // "changes", tell the CA provider extensions to stop decrypting the
        // previous program before we lose access to the PMT and CAT.
        ushort programNumber;
        if (_subChannelProgramNumbers.TryGetValue(id, out programNumber))
        {
          PmtInfo pmtInfo = _pmt[programNumber];
          if (pmtInfo.SubChannelIds.Remove(id) && pmtInfo.SubChannelIds.Count == 0)
          {
            if (subChannel.CurrentChannel.IsEncrypted && _multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
            {
              foreach (IConditionalAccessProvider caProvider in _caProviders)
              {
                if (caProvider.SendCommand(subChannel.CurrentChannel, CaPmtListManagementAction.Only, CaPmtCommand.NotSelected, pmtInfo.Pmt, _cat))
                {
                  break;
                }
              }
            }
            _pmt.Remove(programNumber);
            _encryptedProgramNumbers.Remove(programNumber);
          }
          _subChannelProgramNumbers.Remove(id);
        }

        if (subChannel.CurrentChannel.IsDifferentTransmitter(channel))
        {
          _cat = null;
        }
      }

      this.LogInfo("sub-channel manager base: {0}, ID = {1}, count = {2}", description, id, _subChannels.Count);
      subChannel.CurrentChannel = channel;
      return subChannel;
    }

    #region x

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel identifier.</param>
    public void FreeSubChannel(int id)
    {
      this.LogDebug("tuner base: free sub-channel, ID = {0}, count = {1}", id, _subChannels.Count);
      ISubChannelInternal subChannel;
      if (_subChannels.TryGetValue(id, out subChannel))
      {
        if (subChannel.IsTimeShifting)
        {
          this.LogError("tuner base: asked to free sub-channel that is still timeshifting!");
          return;
        }
        if (subChannel.IsRecording)
        {
          this.LogError("tuner base: asked to free sub-channel that is still recording!");
          return;
        }

        subChannel.Decompose();
        _subChannels.Remove(id);
      }
      else
      {
        this.LogWarn("tuner base: sub-channel not found!");
      }
    }

    /// <summary>
    /// Free all sub-channels.
    /// </summary>
    public void FreeAllSubChannels()
    {
      this.LogInfo("tuner base: free all sub-channels, count = {0}", _subChannels.Count);
      Dictionary<int, ISubChannelInternal>.Enumerator en = _subChannels.GetEnumerator();
      while (en.MoveNext())
      {
        en.Current.Value.Decompose();
      }
      _subChannels.Clear();
      _nextSubChannelId = 0;
    }

    /// <summary>
    /// Get a specific sub-channel.
    /// </summary>
    /// <param name="id">The ID of the sub-channel.</param>
    /// <returns></returns>
    public ISubChannel GetSubChannel(int id)
    {
      ISubChannelInternal subChannel = null;
      if (_subChannels != null)
      {
        _subChannels.TryGetValue(id, out subChannel);
      }
      return subChannel;
    }

    /// <summary>
    /// Get the tuner's sub-channels.
    /// </summary>
    /// <value>An array containing the sub-channels.</value>
    public ISubChannel[] SubChannels
    {
      get
      {
        int i = 0;
        ISubChannel[] subChannels = new ISubChannel[_subChannels.Count];
        Dictionary<int, ISubChannelInternal>.Enumerator en = _subChannels.GetEnumerator();
        while (en.MoveNext())
        {
          subChannels[i++] = en.Current.Value;
        }
        return subChannels;
      }
    }

    #endregion

    #region pids

    /// <summary>
    /// The mode to use for controlling tuner PID filter(s).
    /// </summary>
    /// <remarks>
    /// This setting can be used to enable or disable the tuner's PID filter
    /// even when the tuning context (for example, DVB-S vs. DVB-S2) would
    /// usually result in different behaviour. Note that it is usually not
    /// ideal to have to manually enable or disable a PID filter as it can
    /// affect streaming reliability.
    /// </remarks>
    private PidFilterMode _pidFilterMode = PidFilterMode.Automatic;

    private IDictionary<ushort, HashSet<int>> _pids = new Dictionary<ushort, HashSet<int>>();               // PID -> sub-channel IDs
    private IDictionary<int, HashSet<ushort>> _subChannelPids = new Dictionary<int, HashSet<ushort>>();     // sub-channel ID -> PIDs
    private IDictionary<IMpeg2PidFilter, PidFilterMode> _pidFilterStates = new Dictionary<IMpeg2PidFilter, PidFilterMode>();

    private void UpdatePidsForSubChannel(int subChannelId, HashSet<ushort> pids)
    {
      lock (_pids)
      {
        this.LogDebug("MPEG 2 manager: current PIDs, count = {0}, PID(s) = [{1}]", _pids.Keys.Count, string.Join(", ", _pids.Keys));

        HashSet<ushort> subChannelPidsNew = new HashSet<ushort>(pids);
        HashSet<ushort> subChannelPidsOld;
        if (!_subChannelPids.TryGetValue(subChannelId, out subChannelPidsOld))
        {
          subChannelPidsOld = new HashSet<ushort>();
        }
        else
        {
          subChannelPidsNew.ExceptWith(subChannelPidsOld);
          subChannelPidsOld.ExceptWith(pids);
        }
        _subChannelPids[subChannelId] = pids;

        HashSet<int> subChannelIds;
        HashSet<ushort> pidsNew = new HashSet<ushort>();
        HashSet<ushort> pidsOld = new HashSet<ushort>();
        foreach (ushort pid in subChannelPidsNew)
        {
          if (_pids.TryGetValue(pid, out subChannelIds))
          {
            subChannelIds.Add(subChannelId);
          }
          else
          {
            pidsNew.Add(pid);
            _pids[pid] = new HashSet<int>() { subChannelId };
          }
        }

        foreach (ushort pid in subChannelPidsOld)
        {
          if (_pids.TryGetValue(pid, out subChannelIds) && subChannelIds.Remove(subChannelId) && subChannelIds.Count == 0)
          {
            pidsOld.Add(pid);
            _pids.Remove(pid);
          }
        }

        if (pidsNew.Count == 0 && pidsOld.Count == 0)
        {
          this.LogDebug("MPEG 2 manager: no PID filter change required");
          return;
        }

        this.LogDebug("MPEG 2 manager: add PIDs, count = {0}, PID(s) = [{1}]", pidsNew.Count, string.Join(", ", pidsNew));
        this.LogDebug("MPEG 2 manager: remove PIDs, count = {0}, PID(s) = [{1}]", pidsOld.Count, string.Join(", ", pidsOld));

        if (_pidFilterMode == PidFilterMode.Disabled)
        {
          this.LogDebug("MPEG 2 manager: PID filtering disabled");
          foreach (IMpeg2PidFilter pidFilter in _pidFilters)
          {
            PidFilterMode state;
            if (!_pidFilterStates.TryGetValue(pidFilter, out state) || state == PidFilterMode.Enabled)
            {
              pidFilter.Disable();
              _pidFilterStates[pidFilter] = PidFilterMode.Disabled;
            }
          }
          return;
        }

        var en = _subChannels.Values.GetEnumerator();
        en.MoveNext();
        IChannel tuningDetail = en.Current.CurrentChannel;
        foreach (IMpeg2PidFilter pidFilter in _pidFilters)
        {
          PidFilterMode state;
          bool tooManyPids = (pidFilter.MaximumPidCount > 0 && _pids.Count > pidFilter.MaximumPidCount);
          if (_pidFilterMode == PidFilterMode.Automatic && (!pidFilter.ShouldEnable(tuningDetail) || tooManyPids))
          {
            if (!_pidFilterStates.TryGetValue(pidFilter, out state) || state == PidFilterMode.Enabled)
            {
              if (tooManyPids)
              {
                this.LogDebug("MPEG 2 manager: disable {0} filter, PID count exceeds limit {1}", pidFilter.Name, pidFilter.MaximumPidCount);
              }
              else
              {
                this.LogDebug("MPEG 2 manager: disable {0} filter, unrequired", pidFilter.Name);
              }
              pidFilter.Disable();
              _pidFilterStates[pidFilter] = PidFilterMode.Disabled;
            }
            continue;
          }

          if (tooManyPids)
          {
            this.LogWarn("MPEG 2 manager: PID count exceeds {0} filter limit {1}", pidFilter.Name, pidFilter.MaximumPidCount);
          }
          if (!_pidFilterStates.TryGetValue(pidFilter, out state) || state == PidFilterMode.Disabled)
          {
            this.LogDebug("MPEG 2 manager: enable {0} filter", pidFilter.Name);
            pidFilter.AllowStreams(_pids.Keys);
            pidFilter.ApplyConfiguration();
            _pidFilterStates[pidFilter] = PidFilterMode.Enabled;
          }
          else
          {
            pidFilter.BlockStreams(pidsOld);
            pidFilter.AllowStreams(pidsNew);
            pidFilter.ApplyConfiguration();
          }
        }
      }
    }

    #endregion

    #region decrypt

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
    /// <param name="subChannelId">The ID of the sub-channel causing this update.</param>
    /// <param name="updateAction"><c>Add</c> if the sub-channel is being tuned, <c>update</c> if the PMT for the
    ///   sub-channel has changed, or <c>last</c> if the sub-channel is being disposed.</param>
    private void UpdateDecryptList(int subChannelId, CaPmtListManagementAction updateAction)
    {
      if (!_useConditionalAccessInterface)
      {
        this.LogWarn("Mpeg2TunerController: CA disabled");
        return;
      }
      if (_caProviders.Count == 0)
      {
        this.LogWarn("Mpeg2TunerController: no CA providers identified");
        return;
      }
      this.LogDebug("Mpeg2TunerController: sub-channel {0} update decrypt list, mode = {1}, update action = {2}", subChannelId, _multiChannelDecryptMode, updateAction);

      if (!_subChannels[subChannelId].CurrentChannel.IsEncrypted)
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
      List<ISubChannel> distinctServices = new List<ISubChannel>();
      Dictionary<int, ISubChannelInternal>.ValueCollection.Enumerator en = _subChannels.Values.GetEnumerator();
      ChannelMpeg2Base updatedMpeg2Service = _subChannels[subChannelId].CurrentChannel as ChannelMpeg2Base;
      ChannelAnalogTv updatedAnalogTvService = _subChannels[subChannelId].CurrentChannel as ChannelAnalogTv;
      ChannelFmRadio updatedFmRadioService = _subChannels[subChannelId].CurrentChannel as ChannelFmRadio;
      while (en.MoveNext())
      {
        IChannel service = en.Current.CurrentChannel;
        // We don't care about FTA services here.
        if (!service.IsEncrypted)
        {
          continue;
        }

        // Keep an eye out - if there is another sub-channel accessing the same service as the sub-channel that
        // is being updated then we always do *nothing* unless this is specifically an update request. In any other
        // situation, if we were to stop decrypting the service it would be wrong; if we were to start decrypting
        // the service it would be unnecessary and possibly cause stream interruptions.
        if (en.Current.SubChannelId != subChannelId && updateAction != CaPmtListManagementAction.Update)
        {
          if (updatedMpeg2Service != null)
          {
            ChannelMpeg2Base mpeg2Service = service as ChannelMpeg2Base;
            if (mpeg2Service != null && mpeg2Service.ProgramNumber == updatedMpeg2Service.ProgramNumber)
            {
              this.LogDebug("Mpeg2TunerController: the service for this sub-channel is a duplicate, no action required");
              return;
            }
          }
          else if (updatedAnalogTvService != null)
          {
            ChannelAnalogTv analogTvService = service as ChannelAnalogTv;
            if (analogTvService != null && analogTvService.PhysicalChannelNumber == updatedAnalogTvService.PhysicalChannelNumber)
            {
              this.LogDebug("Mpeg2TunerController: the service for this sub-channel is a duplicate, no action required");
              return;
            }
          }
          else if (updatedFmRadioService != null)
          {
            ChannelFmRadio fmRadioService = service as ChannelFmRadio;
            if (fmRadioService != null && fmRadioService.Frequency == updatedFmRadioService.Frequency)
            {
              this.LogDebug("Mpeg2TunerController: the service for this sub-channel is a duplicate, no action required");
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
          foreach (ISubChannel serviceToDecrypt in distinctServices)
          {
            ChannelMpeg2Base mpeg2Service = service as ChannelMpeg2Base;
            if (mpeg2Service != null)
            {
              if (mpeg2Service.ProgramNumber == ((ChannelMpeg2Base)serviceToDecrypt.CurrentChannel).ProgramNumber)
              {
                exists = true;
                break;
              }
            }
            else
            {
              ChannelAnalogTv analogTvService = service as ChannelAnalogTv;
              if (analogTvService != null)
              {
                if (analogTvService.PhysicalChannelNumber == ((ChannelAnalogTv)serviceToDecrypt.CurrentChannel).PhysicalChannelNumber)
                {
                  exists = true;
                  break;
                }
              }
              else
              {
                ChannelFmRadio fmRadioService = service as ChannelFmRadio;
                if (fmRadioService != null)
                {
                  if (fmRadioService.Frequency == ((ChannelFmRadio)serviceToDecrypt.CurrentChannel).Frequency)
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

      if (distinctServices.Count == 0)
      {
        this.LogDebug("Mpeg2TunerController: no services to update");
        return;
      }

      // Send the service list or changes to the CA providers.
      for (int attempt = 1; attempt <= _decryptFailureRetryCount + 1; attempt++)
      {
        // TODO ThrowExceptionIfTuneCancelled();
        if (attempt > 1)
        {
          this.LogDebug("Mpeg2TunerController: attempt {0}...", attempt);
        }

        foreach (IConditionalAccessProvider caProvider in _caProviders)
        {
          this.LogDebug("Mpeg2TunerController: CA provider {0}...", caProvider.Name);

          if (_waitUntilCaInterfaceReady && !caProvider.IsReady())
          {
            this.LogDebug("Mpeg2TunerController: provider is not ready, waiting for up to 15 seconds", caProvider.Name);
            DateTime startWait = DateTime.Now;
            TimeSpan waitTime = new TimeSpan(0);
            while (waitTime.TotalMilliseconds < 15000)
            {
              // TODO ThrowExceptionIfTuneCancelled();
              System.Threading.Thread.Sleep(200);
              waitTime = DateTime.Now - startWait;
              if (caProvider.IsReady())
              {
                this.LogDebug("Mpeg2TunerController: provider ready after {0} ms", waitTime.TotalMilliseconds);
                break;
              }
            }
          }

          // Ready or not, we send commands now.
          this.LogDebug("Mpeg2TunerController: sending command(s)");
          bool success = true;
          SubChannelMpeg2Ts digitalService;
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
            // TODO ThrowExceptionIfTuneCancelled();
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
            digitalService = distinctServices[i] as SubChannelMpeg2Ts;
            if (digitalService == null)
            {
              success &= caProvider.SendCommand(distinctServices[i].CurrentChannel, action, command, null, null);
            }
            else
            {
              // TODO need to PatchPmtForCam() sometime before now, and in such a way that the patched PMT is not propagated to TsWriter etc.
              success &= caProvider.SendCommand(distinctServices[i].CurrentChannel, action, command, digitalService.ProgramMapTable, digitalService.ConditionalAccessTable);
            }
          }

          // Are we done?
          if (success)
          {
            return;
          }
        }
      }
    }

    #endregion

    private struct PmtInfo
    {
      public TableProgramMap Pmt;
      public HashSet<int> SubChannelIds;
    }

    #region constants

    private const ushort PID_PAT = 0;
    private const ushort PID_CAT = 1;

    #endregion

    private TableConditionalAccess _cat = null;
    private IDictionary<ushort, PmtInfo> _pmt = new Dictionary<ushort, PmtInfo>();
    private IDictionary<int, ushort> _subChannelProgramNumbers = new Dictionary<int, ushort>();
    private HashSet<ushort> _encryptedProgramNumbers = new HashSet<ushort>();

    public void OnAfterTune(ISubChannel subChannel)
    {
      if (_pidFilters.Count == 0)
      {
        return;
      }

      HashSet<ushort> pids = new HashSet<ushort>();
      pids.Add(PID_PAT);         // PAT - for program lookup

      // Include the CAT PID when the program needs to be decrypted.
      if (_isConditionalAccessTableRequired && subChannel.CurrentChannel.IsEncrypted)
      {
        pids.Add(PID_CAT);
      }

      ChannelMpeg2Base mpeg2Channel = subChannel.CurrentChannel as ChannelMpeg2Base;
      if (mpeg2Channel != null)
      {
        // Include the PMT PID if we know it. We don't know what the PMT PID is
        // when scanning.
        if (mpeg2Channel.PmtPid > 0)
        {
          pids.Add((ushort)mpeg2Channel.PmtPid);
        }

        if (mpeg2Channel is ChannelAtsc || mpeg2Channel is ChannelScte)
        {
          pids.Add(0x1ffb);  // ATSC VCT - for terrestrial service info
          pids.Add(0x1ffc);  // SCTE VCT - for cable service info
        }
        else
        {
          pids.Add(0x10);    // DVB NIT - for network info
          pids.Add(0x11);    // DVB SDT, BAT - for service info
        }
      }
      UpdatePidsForSubChannel(subChannel.SubChannelId, pids);
    }

    public TableProgramMap GetPmtForProgram(ushort programNumber)
    {
      PmtInfo pmtInfo;
      if (_pmt.TryGetValue(programNumber, out pmtInfo))
      {
        return pmtInfo.Pmt;
      }
      return null;
    }

    public void OnPmtReceived(int subChannelId, TableProgramMap pmt)
    {
      if (!_subChannels[subChannelId].CurrentChannel.IsEncrypted)
      {
        return;
      }

      // TODO make sure you allow the dynamic FreeSat EIT PIDs through PID filters.
      TableProgramMap currentPmt;
      if (_pmt.ContainsKey(pmt.ProgramNumber))
      {
      }
      if (_isConditionalAccessTableRequired)
      {
        return;
      }
    }

    public void OnPmtChanged(int subChannelId, TableProgramMap pmt)
    {
      if (pmt == null)
      {
        if (_multiChannelDecryptMode == MultiChannelDecryptMode.List)
        {
          return;
        }
      }
    }

    public void OnCatReceived(int subChannelId, TableConditionalAccess cat)
    {
    }
  }
}