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
using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts
{
  internal class SubChannelManagerMpeg2Ts : SubChannelManagerBase, IObserver
  {
    #region enums & classes

    private enum DecryptUpdateAction
    {
      Add,
      Update,
      Remove
    }

    private class PidFilter
    {
      public IMpeg2PidFilter Filter = null;
      public bool IsEnabled = true;
      public HashSet<ushort> Pids = new HashSet<ushort>();
    }

    private class ProgramInformation
    {
      public int ProgramNumber = 0;
      public ushort PmtPid = 0;
      public bool? IsRunning = null;
      public TableProgramMap Pmt = null;
      public HashSet<ushort> Pids = new HashSet<ushort>();
      public bool HasVideo = false;
      public bool HasAudio = false;
      public HashSet<int> SubChannelIds = new HashSet<int>();
      public string Provider = string.Empty;
      public bool? IsEncryptedConfig = null;
      public bool IsEncryptedPids = false;

      public bool IsEncrypted
      {
        get
        {
          return IsEncryptedConfig.GetValueOrDefault(false) || IsEncryptedPids;
        }
      }
    }

    #endregion

    #region constants

    private const int PROGRAM_NUMBER_SI = int.MaxValue;
    private const int PROGRAM_NUMBER_EPG = int.MaxValue - 1;

    public const ushort PID_PAT = 0;
    private const ushort PID_CAT = 1;

    #endregion

    #region variables

    #region conditional access

    /// <summary>
    /// Enable or disable the use of conditional access interface(s).
    /// </summary>
    private bool _useConditionalAccessInterface = true;

    /// <summary>
    /// A list containing the available conditional access providers, ordered
    /// by descending priority.
    /// </summary>
    private IList<IConditionalAccessProvider> _caProviders = new List<IConditionalAccessProvider>();

    /// <summary>
    /// Is the conditional access table required in order for any or all of the
    /// conditional access providers to decrypt programs?
    /// </summary>
    private bool _isConditionalAccessTableRequired = false;

    /// <summary>
    /// The method that should be used to communicate the set of channels that
    /// the conditional access providers must manage.
    /// </summary>
    /// <remarks>
    /// Multi-channel decrypt is *not* the same as Digital Devices'
    /// multi-transponder decrypt (MTD). MCD is implmented using standard CA
    /// PMT commands; MTD is implemented in the Digital Devices drivers.
    /// Disabled = Always send Only. In most cases this will result in only one
    ///             channel being decrypted. If other methods are not working
    ///             reliably then this one should at least allow decrypting one
    ///             channel reliably.
    /// List = Send Only, First, More and Last. This is the most widely
    ///         supported set of commands.
    /// Changes = Send Add, Update and Remove. Some interfaces (CAMs) don't
    ///           support these commands.
    /// </remarks>
    private MultiChannelDecryptMode _multiChannelDecryptMode = MultiChannelDecryptMode.List;

    /// <summary>
    /// The type of conditional access module available to the conditional
    /// access providers.
    /// </summary>
    /// <remarks>
    /// Certain conditional access modules require specific handling to ensure
    /// compatibility.
    /// </remarks>
    private CamType _camType = CamType.Default;

    /// <summary>
    /// The number of times to re-attempt decrypting the current program set
    /// when one or more programs are not able to be decrypted for whatever
    /// reason.
    /// </summary>
    /// <remarks>
    /// Each conditional access provider will be tried in priority order. If
    /// decrypting is not started successfully, all providers are retried until
    /// each provider has been tried _decryptFailureRetryCount + 1 times, or
    /// until decrypting is successful.
    /// </remarks>
    private int _decryptFailureRetryCount = 2;

    /// <summary>
    /// Enable or disable waiting for the conditional providers to be ready
    /// before sending commands.
    /// </summary>
    private bool _waitUntilCaInterfaceReady = true;

    #endregion

    #region PID handling

    /// <summary>
    /// A dictionary containing the PIDs that are currently required, and the
    /// program numbers of the programs for which they are needed.
    /// </summary>
    private IDictionary<ushort, HashSet<int>> _pids = new Dictionary<ushort, HashSet<int>>(50);             // PID => [program numbers]

    /// <summary>
    /// The PIDs in the current transport stream which are encrypted.
    /// </summary>
    private HashSet<ushort> _encryptedPids = new HashSet<ushort>();

    /// <summary>
    /// The PIDs which are required to receive any program.
    /// </summary>
    private HashSet<ushort> _alwaysRequiredPids = new HashSet<ushort> { PID_PAT };

    /// <summary>
    /// A dictionary containing all the PIDs that carry program map tables, and
    /// the program numbers of their associated programs.
    /// </summary>
    private IDictionary<ushort, HashSet<ushort>> _pmtPids = new Dictionary<ushort, HashSet<ushort>>(20);    // PMT PID => [program numbers]

    #region PID filtering

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

    /// <summary>
    /// A list containing the tuner's PID filter interfaces.
    /// </summary>
    private IList<PidFilter> _pidFilters = new List<PidFilter>(3);

    #endregion

    #endregion

    // Used to synchronise access to _programs, _cat, _pids etc.
    private object _lock = new object();

    private ITsWriter _tsWriter = null;
    private bool _isTsWriterStopped = true;

    private IChannel _currentTuningDetail = null;
    private bool _isPatComplete = false;
    private IDictionary<int, ProgramInformation> _programs = new Dictionary<int, ProgramInformation>(20);   // program number => information
    private TableConditionalAccess _cat = null;
    private AutoResetEvent _programWaitEvent = new AutoResetEvent(false);

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelManagerMpeg2Ts"/> class.
    /// </summary>
    /// <param name="tsWriter">The TS writer instance used to perform/implement time-shifting and recording.</param>
    /// <param name="alwaysRequiredPids">The PIDs which are required for any and all sub-channels.</param>
    public SubChannelManagerMpeg2Ts(ITsWriter tsWriter, ICollection<ushort> alwaysRequiredPids = null)
    {
      _tsWriter = tsWriter;
      _tsWriter.SetObserver(this);
      if (alwaysRequiredPids != null)
      {
        _alwaysRequiredPids = new HashSet<ushort>(alwaysRequiredPids);
      }
    }

    #region private members

    /// <summary>
    /// Register a sub-channel to receive a program.
    /// </summary>
    /// <param name="subChannelId">The sub-channel's identifier.</param>
    /// <param name="programNumber">The program's number.</param>
    /// <param name="programProvider">The program's provider.</param>
    /// <param name="isProgramEncrypted"><c>True</c> if the program is expected to be encrypted.</param>
    /// <returns><c>true</c> if a decryption command needs to be sent in order to receive the program, otherwise <c>false</c></returns>
    private bool AddSubChannel(int subChannelId, int programNumber, string programProvider, bool isProgramEncrypted)
    {
      ProgramInformation program;
      if (
        IsEpgGrabbingEnabled &&
        programNumber != ChannelMpeg2TsBase.PROGRAM_NUMBER_SCANNING
      )
      {
        lock (_lock)
        {
          program = _programs[PROGRAM_NUMBER_EPG];
          if (program.SubChannelIds.Add(subChannelId) && program.SubChannelIds.Count == 1)
          {
            UpdateTsPids(PROGRAM_NUMBER_EPG, new HashSet<ushort>(), program.Pids);
          }
        }
      }

      if (
        programNumber == ChannelMpeg2TsBase.PROGRAM_NUMBER_SCANNING ||
        programNumber == ChannelMpeg2TsBase.PROGRAM_NUMBER_NOT_KNOWN_SELECT_FIRST
      )
      {
        // Scanning, single program transport stream with unknown program
        // number, or multi-program transport stream where the caller will
        // select the target program later.
        lock (_lock)
        {
          program = _programs[PROGRAM_NUMBER_SI];
          if (program.SubChannelIds.Add(subChannelId) && program.SubChannelIds.Count == 1)
          {
            UpdateTsPids(PROGRAM_NUMBER_SI, new HashSet<ushort>(), program.Pids);
          }
          return false;
        }
      }

      bool sendDecryptCommand = false;
      lock (_lock)
      {
        if (!_programs.TryGetValue(programNumber, out program))
        {
          program = new ProgramInformation();
          program.IsEncryptedConfig = isProgramEncrypted;
          program.Pids.UnionWith(_alwaysRequiredPids);
          if (_isConditionalAccessTableRequired)
          {
            program.Pids.Add(PID_CAT);
          }
          program.ProgramNumber = programNumber;
          program.Provider = programProvider;
          program.SubChannelIds.Add(subChannelId);
          _programs[programNumber] = program;
          UpdateTsPids(programNumber, new HashSet<ushort>(), program.Pids);
        }
        else if (program.SubChannelIds.Add(subChannelId))
        {
          if (program.SubChannelIds.Count == 1)
          {
            program.Provider = programProvider;
            UpdateTsPids(programNumber, new HashSet<ushort>(), program.Pids);
          }
          if (
            program.Pmt != null &&
            (!_isConditionalAccessTableRequired || _cat != null) &&
            (
              program.SubChannelIds.Count == 1 ||
              (isProgramEncrypted && !program.IsEncrypted)
            )
          )
          {
            sendDecryptCommand = true;
          }
          program.IsEncryptedConfig |= isProgramEncrypted;
        }
      }
      return sendDecryptCommand;
    }

    /// <summary>
    /// Update the list of programs that the conditional access providers are
    /// decrypting.
    /// </summary>
    /// <remarks>
    /// The strategy here is usually to only send commands to the CA providers
    /// when we need an *additional* program to be decrypted. The *only*
    /// exception is when we have to stop decrypting programs in "changes"
    /// mode. We don't send "not selected" commands for "list" or "only" mode
    /// because this can disrupt the other programs that still need to be
    /// decrypted. We also don't send "keep decrypting" commands (alternative
    /// to "not selected") because that will almost certainly cause glitches in
    /// streams.
    /// </remarks>
    /// <param name="program">The program triggering the update.</param>
    /// <param name="updateAction"><c>Add</c> if the program is being tuned.
    ///   <c>Update</c> if the PMT for the program has changed.
    ///   <c>Remove</c> if the program no longer needs to be decrypted.</param>
    private void UpdateDecryptList(ProgramInformation program, DecryptUpdateAction updateAction)
    {
      if (!_useConditionalAccessInterface)
      {
        this.LogWarn("MPEG 2: CA disabled");
        return;
      }
      if (_caProviders.Count == 0)
      {
        this.LogWarn("MPEG 2: no CA providers available");
        return;
      }
      this.LogDebug("MPEG 2: update decrypt list, program number = {0}, mode = {1}, update action = {2}", program.ProgramNumber, _multiChannelDecryptMode, updateAction);

      if (updateAction == DecryptUpdateAction.Remove && _multiChannelDecryptMode != MultiChannelDecryptMode.Changes)
      {
        this.LogDebug("MPEG 2: \"not selected\" command acknowledged, no action required");
        return;
      }

      IList<ProgramInformation> programs = new List<ProgramInformation>(_programs.Count);
      IDictionary<int, TableProgramMap> patchedPmts = new Dictionary<int, TableProgramMap>(_programs.Count);
      if (_multiChannelDecryptMode == MultiChannelDecryptMode.List)
      {
        foreach (var p in _programs.Values)
        {
          if (p.SubChannelIds.Count > 0 && p.IsEncrypted)
          {
            programs.Add(p);
            patchedPmts.Add(p.ProgramNumber, p.Pmt.PatchForCam(_camType));
          }
        }
        this.LogDebug("MPEG 2: list mode, program count = {0}", programs.Count);
      }
      else
      {
        programs.Add(program);
      }

      // Send the program list or changes to the CA providers.
      for (int attempt = 1; attempt <= _decryptFailureRetryCount + 1; attempt++)
      {
        if (attempt > 1)
        {
          this.LogDebug("MPEG 2: attempt #{0}...", attempt);
        }

        foreach (IConditionalAccessProvider caProvider in _caProviders)
        {
          if (!caProvider.IsOpen)
          {
            continue;
          }
          if (_caProviders.Count > 1)
          {
            this.LogDebug("MPEG 2: CA provider, name = {0}...", caProvider.Name);
          }

          if (_waitUntilCaInterfaceReady && !caProvider.IsReady())
          {
            this.LogDebug("MPEG 2: provider is not ready, waiting for up to 15 seconds", caProvider.Name);
            DateTime startWait = DateTime.Now;
            TimeSpan waitTime = TimeSpan.Zero;
            while (waitTime.TotalMilliseconds < 15000)
            {
              ThrowExceptionIfTuneCancelled();
              System.Threading.Thread.Sleep(200);
              waitTime = DateTime.Now - startWait;
              if (caProvider.IsReady())
              {
                this.LogDebug("MPEG 2: provider ready after {0} ms", waitTime.TotalMilliseconds);
                break;
              }
            }
          }

          // Ready or not, we send commands now.
          bool success = true;

          // The default action is "more" - this will be changed below if necessary.
          CaPmtListManagementAction action = CaPmtListManagementAction.More;

          // The command is "start/continue descrambling" unless we're removing programs.
          CaPmtCommand command = CaPmtCommand.OkDescrambling;
          if (updateAction == DecryptUpdateAction.Remove)
          {
            command = CaPmtCommand.NotSelected;
          }
          for (int i = 0; i < programs.Count; i++)
          {
            if (i == 0)
            {
              if (programs.Count == 1)
              {
                if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
                {
                  if (updateAction == DecryptUpdateAction.Remove)
                  {
                    action = CaPmtListManagementAction.Only;
                  }
                  else if (updateAction == DecryptUpdateAction.Add)
                  {
                    action = CaPmtListManagementAction.Add;
                  }
                  else
                  {
                    action = CaPmtListManagementAction.Update;
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
            else if (i == programs.Count - 1)
            {
              action = CaPmtListManagementAction.Last;
            }
            else
            {
              action = CaPmtListManagementAction.More;
            }

            ProgramInformation p = programs[i];
            this.LogDebug("  command = {0}, action = {1}, program = {2}", command, action, p.ProgramNumber);
            success &= caProvider.SendCommand(action, command, patchedPmts[p.ProgramNumber], _cat, p.Provider);
          }

          // Are we done?
          ThrowExceptionIfTuneCancelled();
          if (success)
          {
            return;
          }
        }
      }
    }

    #region PID handling

    /// <summary>
    /// Update the program map table PID for a program.
    /// </summary>
    /// <remarks>
    /// Changes are propagated to the PMT PIDs and SI program, and also to the
    /// PID filters if the call reflects a PAT change during scanning.
    /// </remarks>
    /// <param name="program"></param>
    /// <param name="newPmtPid"></param>
    private void UpdateProgramPmtPid(ProgramInformation program, ushort newPmtPid)
    {
      if (program.PmtPid == newPmtPid || newPmtPid == 0)
      {
        return;
      }

      // Careful. A single PID can carry PMT for multiple programs.
      HashSet<ushort> pidsToRemove = new HashSet<ushort>();
      HashSet<ushort> pidsToAdd = new HashSet<ushort>();
      HashSet<ushort> programNumbers;
      ushort programNumber = (ushort)program.ProgramNumber;
      if (program.PmtPid != 0)
      {
        programNumbers = _pmtPids[program.PmtPid];
        if (programNumbers.Remove(programNumber) && programNumbers.Count == 0)
        {
          _pmtPids.Remove(program.PmtPid);
          _programs[PROGRAM_NUMBER_SI].Pids.Remove(program.PmtPid);
          pidsToRemove.Add(program.PmtPid);
        }
      }

      if (_pmtPids.TryGetValue(newPmtPid, out programNumbers))
      {
        programNumbers.Add(programNumber);
      }
      else
      {
        _encryptedPids.Remove(newPmtPid);
        _pmtPids[newPmtPid] = new HashSet<ushort> { programNumber };
        _programs[PROGRAM_NUMBER_SI].Pids.Add(newPmtPid);
        pidsToAdd.Add(newPmtPid);
      }

      // Update the PID filters if this is a PAT change and we're scanning.
      if (
        _isPatComplete &&
        (pidsToRemove.Count != 0 || pidsToAdd.Count != 0) &&
        _programs[PROGRAM_NUMBER_SI].SubChannelIds.Count > 0
      )
      {
        UpdateTsPids(PROGRAM_NUMBER_SI, pidsToRemove, pidsToAdd);
      }

      program.PmtPid = newPmtPid;
    }

    /// <summary>
    /// Set the PIDs (and auxiliary program information) from the PMT for a
    /// given program.
    /// </summary>
    /// <remarks>
    /// Changes are propagated to the transport stream PIDs and PID filters.
    /// </remarks>
    /// <param name="program">The program.</param>
    private void SetProgramPids(ProgramInformation program)
    {
      HashSet<ushort> pids = new HashSet<ushort>(_alwaysRequiredPids);
      if (_isConditionalAccessTableRequired)
      {
        pids.Add(PID_CAT);
      }
      if (program.PmtPid > 0)
      {
        pids.Add(program.PmtPid);
      }
      if (program.Pmt != null)
      {
        pids.Add(program.Pmt.PcrPid);
        foreach (var es in program.Pmt.ElementaryStreams)
        {
          bool isVideoStream = StreamTypeHelper.IsVideoStream(es.LogicalStreamType);
          if (isVideoStream)
          {
            program.HasVideo = true;
          }
          bool isAudioStream = StreamTypeHelper.IsAudioStream(es.LogicalStreamType);
          if (isAudioStream)
          {
            program.HasAudio = true;
          }
          if (isVideoStream || isAudioStream || es.LogicalStreamType == LogicalStreamType.Subtitles || es.LogicalStreamType == LogicalStreamType.Teletext)
          {
            pids.Add(es.Pid);
            if (!program.IsEncryptedPids && _encryptedPids.Contains(es.Pid))
            {
              if (program.SubChannelIds.Count > 0 && !program.IsEncryptedConfig.Value)
              {
                this.LogWarn("MPEG 2: encryption override, program number = {0}, PID = {1}", program.ProgramNumber, es.Pid);
              }
              program.IsEncryptedPids = true;
            }
          }
        }
      }

      this.LogDebug("MPEG 2: set program PIDs, program number = {0}, PIDs = [{1}]", program.ProgramNumber, string.Join(", ", pids));
      HashSet<ushort> programPidsToAdd = new HashSet<ushort>(pids);
      programPidsToAdd.ExceptWith(program.Pids);
      HashSet<ushort> programPidsToRemove = new HashSet<ushort>(program.Pids);
      programPidsToRemove.ExceptWith(pids);
      program.Pids = new HashSet<ushort>(pids);
      if (program.SubChannelIds.Count > 0)
      {
        UpdateTsPids(program.ProgramNumber, programPidsToRemove, programPidsToAdd);
      }
    }

    /// <summary>
    /// Update the transport stream PIDs to reflect program PID changes.
    /// </summary>
    /// <remarks>
    /// Changes are propagated to the PID filters.
    /// </remarks>
    /// <param name="programNumber">The program number of the program that changed.</param>
    /// <param name="programPidsToRemove">The PIDs that are no longer required to receive the program.</param>
    /// <param name="programPidsToAdd">The new PIDs that are required to receive the program.</param>
    private void UpdateTsPids(int programNumber, HashSet<ushort> programPidsToRemove, HashSet<ushort> programPidsToAdd)
    {
      HashSet<ushort> pidsToRemove = new HashSet<ushort>();
      foreach (ushort pid in programPidsToRemove)
      {
        HashSet<int> programNumbersForPid;
        if (_pids.TryGetValue(pid, out programNumbersForPid))
        {
          if (programNumbersForPid.Remove(programNumber) && programNumbersForPid.Count == 0)
          {
            pidsToRemove.Add(pid);
            _pids.Remove(pid);
          }
        }
      }

      HashSet<ushort> pidsToAdd = new HashSet<ushort>();
      foreach (ushort pid in programPidsToAdd)
      {
        HashSet<int> currentProgramNumbers;
        if (_pids.TryGetValue(pid, out currentProgramNumbers))
        {
          currentProgramNumbers.Add(programNumber);
        }
        else
        {
          pidsToAdd.Add(pid);
          _pids.Add(pid, new HashSet<int> { programNumber });
        }
      }

      if (pidsToRemove.Count > 0 || pidsToAdd.Count > 0)
      {
        this.LogDebug("MPEG 2: PID changes, program number = {0}, remove = [{1}], add = [{2}]",
                      programNumber, string.Join(", ", pidsToRemove), string.Join(", ", pidsToAdd));
        UpdatePidFilters(pidsToRemove, pidsToAdd);
      }
    }

    /// <summary>
    /// Update the PID filters to reflect transport stream PID changes.
    /// </summary>
    /// <param name="pidsToRemove">The PIDs that are no longer required to receive the current programs.</param>
    /// <param name="pidsToAdd">The new PIDs that are required to receive the current programs.</param>
    private void UpdatePidFilters(HashSet<ushort> pidsToRemove, HashSet<ushort> pidsToAdd)
    {
      foreach (var filter in _pidFilters)
      {
        int maxPidCount = filter.Filter.MaximumPidCount;
        bool shouldEnableFilter = filter.Filter.ShouldEnable(_currentTuningDetail);
        if (
          _pidFilterMode == PidFilterMode.Disabled ||
          (
            _pidFilterMode == PidFilterMode.Automatic &&
            (
              !shouldEnableFilter ||
              (maxPidCount > 0 && _pids.Count > maxPidCount)
            )
          )
        )
        {
          if (filter.IsEnabled)
          {
            this.LogDebug("MPEG 2: disable PID filter, name = {0}, mode = {1}, should enable = {2}, PID count = {3} / {4}",
                          filter.Filter.Name, _pidFilterMode, shouldEnableFilter, _pids.Count, maxPidCount);
            if (filter.Filter.Disable())
            {
              filter.Pids.Clear();
            }
          }
          continue;
        }

        bool isFilterCapacitySufficient = maxPidCount < 0 || _pids.Count < maxPidCount;
        HashSet<ushort> pidsToAddToFilter = new HashSet<ushort>();
        if (!filter.IsEnabled || filter.Pids.Count == 0)
        {
          if (isFilterCapacitySufficient)
          {
            pidsToAddToFilter = new HashSet<ushort>(_pids.Keys);
          }
          else
          {
            foreach (ushort pid in _pids.Keys)
            {
              pidsToAddToFilter.Add(pid);
              if (pidsToAddToFilter.Count == maxPidCount)
              {
                break;
              }
            }
          }

          ThrowExceptionIfTuneCancelled();
          this.LogDebug("MPEG 2: enable PID filter, name = {0}, mode = {1}, PID count = {2} / {3}, PIDs = [{4}]",
                        filter.Filter.Name, _pidFilterMode, _pids.Count, maxPidCount, string.Join(", ", pidsToAddToFilter));
          filter.IsEnabled = true;
          filter.Filter.AllowStreams(pidsToAddToFilter);
          if (filter.Filter.ApplyConfiguration())
          {
            filter.Pids.UnionWith(pidsToAddToFilter);
          }
          ThrowExceptionIfTuneCancelled();
          continue;
        }

        HashSet<ushort> originalFilterPids = new HashSet<ushort>(filter.Pids);
        HashSet<ushort> pidsToRemoveFromFilter = new HashSet<ushort>();
        if (pidsToRemove.Count > 0)
        {
          if (isFilterCapacitySufficient)
          {
            pidsToRemoveFromFilter = pidsToRemove;
          }
          else
          {
            foreach (ushort pid in pidsToRemove)
            {
              if (filter.Pids.Contains(pid))
              {
                pidsToRemoveFromFilter.Add(pid);
              }
            }
          }

          if (pidsToRemoveFromFilter.Count > 0)
          {
            filter.Filter.BlockStreams(pidsToRemoveFromFilter);
            filter.Pids.ExceptWith(pidsToRemoveFromFilter);
          }
        }

        if (pidsToAdd.Count > 0)
        {
          if (isFilterCapacitySufficient)
          {
            pidsToAddToFilter = pidsToAdd;
          }
          else
          {
            foreach (ushort pid in _pids.Keys)
            {
              if (filter.Pids.Count + pidsToAddToFilter.Count >= maxPidCount)
              {
                break;
              }
              if (!filter.Pids.Contains(pid))
              {
                pidsToAddToFilter.Add(pid);
              }
            }
          }

          if (pidsToAddToFilter.Count > 0)
          {
            filter.Filter.AllowStreams(pidsToAddToFilter);
            filter.Pids.UnionWith(pidsToAddToFilter);
          }
        }

        if (pidsToRemoveFromFilter.Count > 0 || pidsToAddToFilter.Count > 0)
        {
          ThrowExceptionIfTuneCancelled();
          this.LogDebug("MPEG 2: modify PID filter, name = {0}, mode = {1}, PID count = {2} / {3}, remove = [{4}], add = [{5}]",
                        filter.Filter.Name, _pidFilterMode, shouldEnableFilter, _pids.Count, maxPidCount,
                        string.Join(", ", pidsToRemoveFromFilter), string.Join(", ", pidsToAddToFilter));
          if (!filter.Filter.ApplyConfiguration())
          {
            filter.Pids = originalFilterPids;
          }
          ThrowExceptionIfTuneCancelled();
        }
      }
    }

    #endregion

    #endregion

    protected virtual int GetTuningProgramNumber(IChannel channel)
    {
      IChannelMpeg2Ts mpeg2TsChannel = channel as IChannelMpeg2Ts;
      if (mpeg2TsChannel != null)
      {
        return mpeg2TsChannel.ProgramNumber;
      }
      return ChannelMpeg2TsBase.PROGRAM_NUMBER_NOT_KNOWN_SELECT_FIRST;
    }

    protected virtual HashSet<ushort> GetScanningPids()
    {
      return new HashSet<ushort>();
    }

    #region sub-channel manager base implementations/overrides

    /// <summary>
    /// Tune a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="timeLimitReceiveStreamInfo">The maximum time to wait for required implementation-dependent stream information during tuning.</param>
    /// <returns>the sub-channel</returns>
    protected override ISubChannelInternal OnTune(int id, IChannel channel, TimeSpan timeLimitReceiveStreamInfo)
    {
      DateTime tuneStartTime = DateTime.Now;

      // If switching to a different program within the same transport stream,
      // remove the previous program PIDs and tell the CA provider extensions
      // to stop decrypting if necessary.
      SubChannelMpeg2Ts subChannel = GetSubChannel(id) as SubChannelMpeg2Ts;
      if (!_isTsWriterStopped && subChannel != null)
      {
        OnFreeSubChannel(id);

        // Also set the wait event, because PAT etc. has probably already been
        // received, and TsWriter won't re-event it.
        _programWaitEvent.Set();
      }

      lock (_lock)
      {
        if (_isTsWriterStopped)
        {
          if (_tsWriter == null)
          {
            throw new TvException("TsWriter is null.");
          }
          _tsWriter.Start();
          _isTsWriterStopped = false;
        }
      }

      int programNumber = GetTuningProgramNumber(channel);
      bool sendDecryptCommand = AddSubChannel(id, programNumber, channel.Provider, channel.IsEncrypted);

      // Wait for PAT and PMT.
      ProgramInformation program = null;
      while (true)
      {
        ThrowExceptionIfTuneCancelled();
        if (!_programWaitEvent.WaitOne(timeLimitReceiveStreamInfo - (DateTime.Now - tuneStartTime)))
        {
          if (_isPatComplete)
          {
            this.LogError("MPEG 2: program not running (PMT not received), ID = {0}, program number = {1}", id, programNumber);
            throw new TvExceptionServiceNotRunning(channel);
          }

          // PAT not received - extremely unusual!
          this.LogError("MPEG 2: tuner not streaming (PAT not received), ID = {0}", id);
          throw new TvException("The tuner is not delivering any stream.");
        }
        ThrowExceptionIfTuneCancelled();

        if (!_isPatComplete)
        {
          continue;
        }

        if (programNumber == ChannelMpeg2TsBase.PROGRAM_NUMBER_SCANNING)
        {
          // Scanning - we only wait for the PAT to confirm we're receiving a stream.
          break;
        }

        if (programNumber == ChannelMpeg2TsBase.PROGRAM_NUMBER_NOT_KNOWN_SELECT_FIRST)
        {
          // Unknown program number. We assume the transport stream only
          // contains one program, or the caller wants us to select any running
          // program. Can we identify a running program yet?
          bool foundRunningProgram = false;
          lock (_lock)
          {
            foreach (ProgramInformation p in _programs.Values)
            {
              if (
                p.ProgramNumber != PROGRAM_NUMBER_SI &&
                p.ProgramNumber != PROGRAM_NUMBER_EPG &&
                p.IsRunning.GetValueOrDefault(false)
              )
              {
                foundRunningProgram = true;
                if (p.Pmt != null && (p.HasVideo || p.HasAudio))
                {
                  this.LogDebug("MPEG 2: select program, ID = {0}, program number = {1}", id, p.ProgramNumber);
                  programNumber = p.ProgramNumber;
                  OnFreeSubChannel(id);
                  sendDecryptCommand = AddSubChannel(id, programNumber, channel.Provider, channel.IsEncrypted);
                  break;
                }
              }
            }
          }
          if (!foundRunningProgram)
          {
            this.LogError("MPEG 2: no running programs available for selection, ID = {0}", id);
            throw new TvExceptionServiceNotRunning(channel);
          }
          if (programNumber == ChannelMpeg2TsBase.PROGRAM_NUMBER_NOT_KNOWN_SELECT_FIRST)
          {
            continue;   // still waiting for PMT
          }
        }

        lock (_lock)
        {
          program = _programs[programNumber];
          if (program.IsRunning.GetValueOrDefault(false))
          {
            if (program.IsRunning.HasValue)
            {
              this.LogError("MPEG 2: program not running (SDT), ID = {0}, program number = {1}", id, programNumber);
              throw new TvExceptionServiceNotRunning(channel);
            }
            else
            {
              this.LogError("MPEG 2: program not found in the PAT (not running, moved...???), ID = {0}, program number = {1}", id, programNumber);
              throw new TvExceptionServiceNotFound(channel);
            }
          }
          if (program.Pmt == null)
          {
            continue;
          }

          if (!program.HasVideo && !program.HasAudio)
          {
            this.LogError("MPEG 2: program has no video or audio streams, ID = {0}, program number = {1}", id, programNumber);
            throw new TvExceptionStreamNotReceived(channel, false, false);
          }

          if (sendDecryptCommand && program.IsEncrypted)
          {
            if (!program.IsEncryptedConfig.Value)
            {
              this.LogWarn("MPEG 2: encryption override, program number = {0}", program.ProgramNumber);
            }
            UpdateDecryptList(program, DecryptUpdateAction.Add);
          }
          break;
        }
      }

      // Tuning has been successful!
      if (subChannel == null)
      {
        subChannel = new SubChannelMpeg2Ts(id, _tsWriter);
      }
      subChannel.CurrentChannel = channel;
      if (programNumber == ChannelMpeg2TsBase.PROGRAM_NUMBER_SCANNING)
      {
        return subChannel;
      }

      // Set the sub-channel's PMT and wait for video and/or audio if already
      // time-shifting or recording.
      subChannel.OnPmtUpdate(program.Pmt, false, program.HasVideo, program.HasAudio);
      return subChannel;
    }

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    protected override void OnFreeSubChannel(int id)
    {
      lock (_lock)
      {
        foreach (ProgramInformation program in _programs.Values)
        {
          if (program.SubChannelIds.Remove(id) && program.SubChannelIds.Count == 0)
          {
            UpdateTsPids(program.ProgramNumber, program.Pids, new HashSet<ushort>());
            if (program.IsEncrypted)
            {
              UpdateDecryptList(program, DecryptUpdateAction.Remove);
            }
          }
        }
      }
    }

    /// <summary>
    /// Decompose the sub-channel manager.
    /// </summary>
    protected override void OnDecompose()
    {
      lock (_lock)
      {
        if (_tsWriter != null)
        {
          _tsWriter.SetObserver(null);
          _tsWriter = null;
        }
        if (_programWaitEvent != null)
        {
          _programWaitEvent.Set();
          _programWaitEvent.Close();
          _programWaitEvent.Dispose();
          _programWaitEvent = null;
        }
        _caProviders.Clear();
        _pidFilters.Clear();
      }
    }

    #endregion

    #region ISubChannelManager members

    /// <summary>
    /// Reload the manager's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(Tuner configuration)
    {
      if (configuration == null)
      {
        _pidFilterMode = PidFilterMode.Automatic;
        _useConditionalAccessInterface = true;
        _multiChannelDecryptMode = MultiChannelDecryptMode.List;
        _camType = CamType.Default;
        return;
      }

      _pidFilterMode = (PidFilterMode)configuration.PidFilterMode;
      _useConditionalAccessInterface = configuration.UseConditionalAccess;
      _multiChannelDecryptMode = (MultiChannelDecryptMode)configuration.MultiChannelDecryptMode;
      _camType = (CamType)configuration.CamType;
    }

    /// <summary>
    /// Set the manager's extensions.
    /// </summary>
    /// <param name="extensions">A list of the tuner's extensions, in priority order.</param>
    public override void SetExtensions(IList<ITunerExtension> extensions)
    {
      lock (_lock)
      {
        _isConditionalAccessTableRequired = false;
        _caProviders.Clear();
        _pidFilters.Clear();
        if (extensions != null)
        {
          foreach (ITunerExtension extension in extensions)
          {
            IConditionalAccessProvider caProvider = extension as IConditionalAccessProvider;
            if (caProvider != null)
            {
              _caProviders.Add(caProvider);
              _isConditionalAccessTableRequired |= caProvider.IsConditionalAccessTableRequiredForDecryption;
            }

            this.LogDebug("MPEG 2: found PID filter interface \"{0}\"", extension.Name);
            IMpeg2PidFilter pidFilter = extension as IMpeg2PidFilter;
            if (pidFilter != null)
            {
              _pidFilters.Add(new PidFilter { Filter = pidFilter });
            }
          }
        }
      }
      base.SetExtensions(extensions);
    }

    /// <summary>
    /// Enable or disable electronic programme guide data grabbing.
    /// </summary>
    public override bool IsEpgGrabbingEnabled
    {
      set
      {
        if (value != IsEpgGrabbingEnabled)
        {
          lock (_lock)
          {
            ProgramInformation epgProgram = _programs[PROGRAM_NUMBER_EPG];
            if (!value)
            {
              this.LogDebug("MPEG 2: disable EPG grabbing");
              epgProgram.SubChannelIds.Clear();
              UpdateTsPids(PROGRAM_NUMBER_EPG, epgProgram.Pids, new HashSet<ushort>());
            }
            else
            {
              this.LogDebug("MPEG 2: enable EPG grabbing");

              // Assemble a collection of all non-scanning sub-channel IDs.
              HashSet<int> subChannelIds = new HashSet<int>();
              foreach (ProgramInformation program in _programs.Values)
              {
                if (program.ProgramNumber != PROGRAM_NUMBER_SI)
                {
                  foreach (int subChannelId in program.SubChannelIds)
                  {
                    subChannelIds.Add(subChannelId);
                  }
                }
              }

              epgProgram.SubChannelIds = subChannelIds;
              if (subChannelIds.Count > 0)
              {
                UpdateTsPids(PROGRAM_NUMBER_EPG, new HashSet<ushort>(), epgProgram.Pids);
              }
            }
          }
        }
        base.IsEpgGrabbingEnabled = value;
      }
    }

    /// <summary>
    /// This function should be called before the tuner is tuned to a new
    /// transmitter.
    /// </summary>
    public override void OnBeforeTune()
    {
      this.LogDebug("MPEG 2: on before tune");
      base.OnBeforeTune();

      lock (_lock)
      {
        if (_tsWriter == null)
        {
          throw new TvException("TsWriter is null.");
        }
        _tsWriter.Stop();
        _isTsWriterStopped = true;

        // Stay in locked context. TsWriter is stopped so we won't receive
        // IObserver call-backs... but we may still receive calls to
        // GetDecryptedSubChannelDetails() etc.
        _isPatComplete = false;
        _pmtPids.Clear();
        _encryptedPids.Clear();

        // If necessary, tell the CA provider extensions to stop decrypting the
        // previous program.
        foreach (ProgramInformation program in _programs.Values)
        {
          if (program.SubChannelIds.Count > 0 && program.IsEncrypted)
          {
            UpdateDecryptList(program, DecryptUpdateAction.Remove);
            break;  // There can only be one active program when tuning.
          }
        }
        _programs.Clear();
        HashSet<ushort> siProgramPids = new HashSet<ushort>(_alwaysRequiredPids);
        siProgramPids.UnionWith(GetScanningPids());
        _programs[PROGRAM_NUMBER_SI] = new ProgramInformation { ProgramNumber = PROGRAM_NUMBER_SI, IsEncryptedConfig = false, IsRunning = true, Pids = siProgramPids };
        _programs[PROGRAM_NUMBER_EPG] = new ProgramInformation { ProgramNumber = PROGRAM_NUMBER_EPG, IsEncryptedConfig = false, IsRunning = true, Pids = new HashSet<ushort>(_alwaysRequiredPids) };

        // PID filter state after tuning is indeterminate. To be safe, explicitly
        // remove all old PIDs and trigger re-adding PIDs after tuning.
        if (_pids.Count > 0)
        {
          foreach (var filter in _pidFilters)
          {
            if (filter.IsEnabled)
            {
              filter.Filter.BlockStreams(filter.Pids);
              filter.Pids.Clear();
              // Don't apply the change here. Assume that will be handled after
              // tuning.
            }
          }
        }
        _pids.Clear();
      }
    }

    /// <summary>
    /// Get the set of sub-channel identifiers for each channel the tuner is currently decrypting.
    /// </summary>
    /// <returns>a collection of sub-channel identifier lists</returns>
    public override ICollection<IList<int>> GetDecryptedSubChannelDetails()
    {
      IList<IList<int>> decryptedPrograms = new List<IList<int>>(_programs.Count);
      lock (_lock)
      {
        foreach (ProgramInformation program in _programs.Values)
        {
          if (program.SubChannelIds.Count > 0 && program.IsEncrypted)
          {
            decryptedPrograms.Add(new List<int>(program.SubChannelIds));
          }
        }
      }
      return decryptedPrograms;
    }

    /// <summary>
    /// Determine whether a sub-channel is being decrypted.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the sub-channel is being decrypted, otherwise <c>false</c></returns>
    public override bool IsDecrypting(IChannel channel)
    {
      // If tuned, we could actually check the real-time encryption status for
      // the program. However, we don't expect this function to be called for
      // channels which are configured as not encrypted, so there's no point.
      if (channel == null || !channel.IsEncrypted)
      {
        return false;
      }

      IChannelMpeg2Ts mpeg2TsChannel = channel as IChannelMpeg2Ts;
      int programNumber = ChannelMpeg2TsBase.PROGRAM_NUMBER_NOT_KNOWN_SELECT_FIRST;   // assume single program TS
      if (mpeg2TsChannel != null)
      {
        if (mpeg2TsChannel.ProgramNumber == ChannelMpeg2TsBase.PROGRAM_NUMBER_SCANNING)
        {
          return false;
        }
        programNumber = mpeg2TsChannel.ProgramNumber;
      }

      if (programNumber != ChannelMpeg2TsBase.PROGRAM_NUMBER_NOT_KNOWN_SELECT_FIRST)
      {
        lock (_lock)
        {
          foreach (ProgramInformation program in _programs.Values)
          {
            if (program.ProgramNumber == programNumber)
            {
              programNumber = program.ProgramNumber;
              return program.SubChannelIds.Count > 0 || !program.IsEncryptedPids;
            }
          }
        }
        return false;
      }

      // Single program TS or program number not known. Match on the full
      // channel details in case the TS contains multiple programs.
      lock (_lock)
      {
        foreach (ProgramInformation program in _programs.Values)
        {
          if (_programs.Count == 1)
          {
            return program.SubChannelIds.Count > 0;
          }
          foreach (int subChannelId in program.SubChannelIds)
          {
            var subChannel = GetSubChannel(subChannelId);
            if (subChannel.CurrentChannel == channel)
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    #endregion

    #region IObserver members

    /// <summary>
    /// This function is invoked when an initial or updated program association table is received.
    /// </summary>
    /// <param name="transportStreamId">The transport stream's identifier.</param>
    /// <param name="networkPid">The PID which delivers the transport stream's network information.</param>
    /// <param name="programCount">The number of programs in the transport stream.</param>
    public void OnProgramAssociationTable(ushort transportStreamId, ushort networkPid, ushort programCount)
    {
      ThreadPool.QueueUserWorkItem(delegate
      {
        lock (_lock)
        {
          if (_isPatComplete)
          {
            // PAT changes are handled by OnProgramDetail().
            this.LogDebug("MPEG 2: PAT changed");
            return;
          }

          // Only add PMT PIDs to the PID filters on PAT completion. This has
          // the effect of replacing a burst of PID filter changes during PAT
          // processing with a single change.
          this.LogDebug("MPEG 2: PAT received");
          _isPatComplete = true;
          if (_programs[PROGRAM_NUMBER_SI].SubChannelIds.Count > 0)
          {
            UpdateTsPids(PROGRAM_NUMBER_SI, new HashSet<ushort>(), new HashSet<ushort>(_pmtPids.Keys));
          }
        }

        _programWaitEvent.Set();
      });
    }

    /// <summary>
    /// This function is invoked when an initial or updated conditional access table is received.
    /// </summary>
    /// <param name="cat">The new conditional access table. The callee must not change this array.</param>
    /// <param name="catBufferSize">The size of the <paramref name="cat">conditional access table</paramref>.</param>
    public void OnConditionalAccessTable(byte[] cat, ushort catBufferSize)
    {
      if (cat == null || cat.Length == 0)
      {
        return;
      }

      ThreadPool.QueueUserWorkItem(delegate
      {
        lock (_lock)
        {
          DecryptUpdateAction decryptUpdateAction;
          if (_cat == null)
          {
            this.LogDebug("MPEG 2: CAT received");
            decryptUpdateAction = DecryptUpdateAction.Add;
          }
          else
          {
            this.LogDebug("MPEG 2: CAT changed");
            decryptUpdateAction = DecryptUpdateAction.Update;
          }
          _cat = TableConditionalAccess.Decode(cat);

          if (!_isConditionalAccessTableRequired)
          {
            return;
          }

          IList<ProgramInformation> programsToDecrypt = new List<ProgramInformation>(_programs.Count);
          foreach (var program in _programs.Values)
          {
            if (program.ProgramNumber == PROGRAM_NUMBER_SI || program.ProgramNumber == PROGRAM_NUMBER_EPG)
            {
              continue;
            }

            if (
              program.SubChannelIds.Count > 0 &&
              program.Pmt != null &&
              program.IsEncrypted
            )
            {
              programsToDecrypt.Add(program);
            }
          }

          foreach (var program in programsToDecrypt)
          {
            UpdateDecryptList(program, decryptUpdateAction);
            if (_multiChannelDecryptMode == MultiChannelDecryptMode.List)
            {
              break;
            }
          }
        }

        _programWaitEvent.Set();
      });
    }

    /// <summary>
    /// This function is invoked when an initial or updated program details are received.
    /// </summary>
    /// <param name="programNumber">The program's identifier.</param>
    /// <param name="pmtPid">The PID which delivers the program's map table sections.</param>
    /// <param name="isRunning">An indication of whether the program is running.</param>
    /// <param name="pmt">The program's map table. The callee must not change this array.</param>
    /// <param name="pmtBufferSize">The size of the <paramref name="pmt">program map table</paramref>.</param>
    public void OnProgramDetail(ushort programNumber, ushort pmtPid, bool isRunning, byte[] pmt, ushort pmtBufferSize)
    {
      ThreadPool.QueueUserWorkItem(delegate
      {
        lock (_lock)
        {
          ProgramInformation program;
          if (!_programs.TryGetValue(programNumber, out program))
          {
            program = new ProgramInformation();
            program.ProgramNumber = programNumber;
            _programs[programNumber] = program;
          }

          // Update PMT and SI PID collections.
          UpdateProgramPmtPid(program, pmtPid);

          // Update program information.
          program.IsRunning = isRunning;
          DecryptUpdateAction decryptUpdateAction = DecryptUpdateAction.Add;
          if (pmt != null && pmt.Length > 0)
          {
            if (program.Pmt == null)
            {
              this.LogDebug("MPEG 2: PMT received, program number = {0}", program.ProgramNumber);
            }
            else
            {
              this.LogDebug("MPEG 2: PMT changed, program number = {0}", program.ProgramNumber);
              decryptUpdateAction = DecryptUpdateAction.Update;
            }
            program.Pmt = TableProgramMap.Decode(pmt);
            program.IsEncryptedPids = false;
          }

          // Update program PIDs.
          SetProgramPids(program);

          // Send a decrypt command if required.
          if (
            program.SubChannelIds.Count > 0 &&
            program.Pmt != null &&
            (!_isConditionalAccessTableRequired || _cat != null) &&
            program.IsEncrypted
          )
          {
            UpdateDecryptList(program, decryptUpdateAction);
          }

          if (program.Pmt != null)
          {
            // TODO Could we do this in a thread or parallelise the calls for each sub-channel?
            foreach (int subChannelId in program.SubChannelIds)
            {
              SubChannelMpeg2Ts subChannel = GetSubChannel(subChannelId) as SubChannelMpeg2Ts;
              if (subChannel != null)
              {
                try
                {
                  subChannel.OnPmtUpdate(program.Pmt, decryptUpdateAction == DecryptUpdateAction.Update, program.HasVideo, program.HasAudio);
                }
                catch
                {
                  // Video/audio encrypted or not received - we can't really do anything about that from here.
                }
              }
            }
          }
        }

        _programWaitEvent.Set();
      });
    }

    /// <summary>
    /// This function is invoked when an elementary stream is detected as encrypted.
    /// </summary>
    /// <param name="pid">The elementary stream's PID.</param>
    /// <param name="state">The elementary stream's encryption state.</param>
    public void OnPidEncryptionStateChange(ushort pid, EncryptionState state)
    {
      ThreadPool.QueueUserWorkItem(delegate
      {
        lock (_lock)
        {
          if (
            state != EncryptionState.Encrypted ||
            pid == PID_PAT ||
            pid == PID_CAT ||
            _programs[PROGRAM_NUMBER_SI].Pids.Contains(pid) ||
            _programs[PROGRAM_NUMBER_EPG].Pids.Contains(pid) ||
            _pmtPids.ContainsKey(pid)
          )
          {
            // False positive.
            return;
          }

          _encryptedPids.Add(pid);
          if (_isConditionalAccessTableRequired && _cat == null)
          {
            return;
          }

          // Send a decrypt command for the related channel(s) if they're
          // marked as not encrypted and we've been asked to time-shift or
          // record them.
          IList<ProgramInformation> programsToDecrypt = new List<ProgramInformation>(_programs.Count);
          foreach (var program in _programs.Values)
          {
            if (program.Pmt == null || program.IsEncryptedPids || !program.Pids.Contains(pid))
            {
              continue;
            }

            program.IsEncryptedPids = true;

            if (program.SubChannelIds.Count > 0 && !program.IsEncryptedConfig.Value)
            {
              this.LogWarn("MPEG 2: encryption override, program number = {0}, PID = {1}", program.ProgramNumber, pid);
              programsToDecrypt.Add(program);
            }
          }

          foreach (var program in programsToDecrypt)
          {
            UpdateDecryptList(program, DecryptUpdateAction.Add);
            if (_multiChannelDecryptMode == MultiChannelDecryptMode.List)
            {
              break;
            }
          }
        }
      });
    }

    /// <summary>
    /// This function is invoked when access to one or more PIDs is required.
    /// </summary>
    /// <param name="pids">The PIDs that are required.</param>
    /// <param name="pidCount">The number of PIDs in <paramref name="pids">the PID array</paramref>.</param>
    /// <param name="usage">The reason that access is required.</param>
    public void OnPidsRequired(ushort[] pids, byte pidCount, PidUsage usage)
    {
      if (pids == null || pids.Length == 0)
      {
        return;
      }

      ThreadPool.QueueUserWorkItem(delegate
      {
        this.LogDebug("MPEG 2: PIDs required, usage = {0}, PIDs = [{1}]", usage, string.Join(", ", pids));
        lock (_lock)
        {
          ProgramInformation program;
          if (usage == PidUsage.Si)
          {
            program = _programs[PROGRAM_NUMBER_SI];
          }
          else if (usage == PidUsage.Epg)
          {
            program = _programs[PROGRAM_NUMBER_EPG];
          }
          else
          {
            this.LogWarn("MPEG 2: unrecognised PID usage, usage = {0}, PIDs = [{1}]", usage, string.Join(", ", pids));
            return;
          }

          _encryptedPids.ExceptWith(pids);  // SI and EPG are assumed to never be encrypted
          program.Pids.UnionWith(pids);
          if (program.SubChannelIds.Count > 0)
          {
            UpdateTsPids(program.ProgramNumber, new HashSet<ushort>(), new HashSet<ushort>(pids));
          }
        }
      });
    }

    /// <summary>
    /// This function is invoked when access to one or more PIDs is no longer required.
    /// </summary>
    /// <param name="pids">The PIDs that are not required.</param>
    /// <param name="pidCount">The number of PIDs in <paramref name="pids">the PID array</paramref>.</param>
    /// <param name="usage">The reason that access was previously required.</param>
    public void OnPidsNotRequired(ushort[] pids, byte pidCount, PidUsage usage)
    {
      if (pids == null || pids.Length == 0)
      {
        return;
      }

      ThreadPool.QueueUserWorkItem(delegate
      {
        this.LogDebug("MPEG 2: PIDs not required, usage = {0}, PIDs = [{1}]", usage, string.Join(", ", pids));
        lock (_lock)
        {
          ProgramInformation program;
          if (usage == PidUsage.Si)
          {
            program = _programs[PROGRAM_NUMBER_SI];
          }
          else if (usage == PidUsage.Epg)
          {
            program = _programs[PROGRAM_NUMBER_EPG];
          }
          else
          {
            this.LogWarn("MPEG 2: unrecognised PID usage, usage = {0}, PIDs = [{1}]", usage, string.Join(", ", pids));
            return;
          }

          program.Pids.ExceptWith(pids);
          if (program.SubChannelIds.Count > 0)
          {
            UpdateTsPids(program.ProgramNumber, new HashSet<ushort>(pids), new HashSet<ushort>());
          }
        }
      });
    }

    #endregion
  }
}