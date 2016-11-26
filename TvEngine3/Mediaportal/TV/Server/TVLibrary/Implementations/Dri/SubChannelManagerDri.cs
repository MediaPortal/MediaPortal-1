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
using Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Service;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri
{
  internal class SubChannelManagerDri : SubChannelManagerBase
  {
    private readonly ServiceMux _muxService = null;
    private ISubChannelManager _subChannelManager = null;

    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelManagerDri"/> class.
    /// </summary>
    /// <param name="muxService">The tuner's DRI multiplex service.</param>
    /// <param name="subChannelManager">The wrapped stream tuner's sub-channel manager.</param>
    public SubChannelManagerDri(ServiceMux muxService, ISubChannelManager subChannelManager)
    {
      _muxService = muxService;
      _subChannelManager = subChannelManager;
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
      ChannelScte scteChannel = channel as ChannelScte;
      if (scteChannel != null && scteChannel.IsOutOfBandScanChannel())
      {
        // When scanning using the out-of-band tuner we don't require any
        // in band stream. Special handling is required. The regular
        // sub-channel manager throws an exception when PAT is not received.
        return new SubChannelDriOutOfBandScan(id);
      }

      IChannelMpeg2Ts mpeg2TsChannel = channel as IChannelMpeg2Ts;
      if (mpeg2TsChannel == null || mpeg2TsChannel.ProgramNumber != ChannelMpeg2TsBase.PROGRAM_NUMBER_NOT_KNOWN_SELECT_FIRST)
      {
        return _subChannelManager.Tune(id, channel) as ISubChannelInternal;
      }

      // When switched digital video (SDV) is active - ie. the tuner uses a
      // tuning adaptor/resolver (TA/TR) to ask the cable system which
      // frequency and program number to tune - we have to ask the tuner what
      // the correct program number is.
      // Note that SiliconDust and Hauppauge tuners actually deliver a TS
      // containing a single program. For them it would be enough to set the
      // program number to PROGRAM_NUMBER_NOT_KNOWN_SELECT_FIRST. However Ceton
      // tuners deliver the PAT and PMT for all the programs in the full
      // transport stream, only excluding extra video and audio streams.
      // Therefore we have to do this...
      int originalProgramNumber = mpeg2TsChannel.ProgramNumber;
      DateTime start = DateTime.Now;
      while (DateTime.Now - start < timeLimitReceiveStreamInfo)
      {
        ThrowExceptionIfTuneCancelled();
        mpeg2TsChannel.ProgramNumber = 0;
        try
        {
          mpeg2TsChannel.ProgramNumber = (int)_muxService.QueryStateVariable("ProgramNumber");
        }
        catch (Exception ex)
        {
          this.LogError(ex, "sub-channel manager DRI: failed to determine program number, ID = {0}", id);
        }

        ThrowExceptionIfTuneCancelled();
        if (mpeg2TsChannel.ProgramNumber != 0)
        {
          this.LogDebug("sub-channel manager DRI: determined program number, ID = {0}, program number = {1}", id, mpeg2TsChannel.ProgramNumber);
          ISubChannelInternal subChannel = _subChannelManager.Tune(id, channel) as ISubChannelInternal;
          if (subChannel != null)
          {
            mpeg2TsChannel.ProgramNumber = originalProgramNumber;
            subChannel.CurrentChannel = channel;
          }
          return subChannel;
        }

        System.Threading.Thread.Sleep(20);
      }
      this.LogError("sub-channel manager DRI: failed to determine program number, ID = {0}", id);
      throw new TvException("The program number could not be determined.");
    }

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    protected override void OnFreeSubChannel(int id)
    {
      _subChannelManager.FreeSubChannel(id);
    }

    /// <summary>
    /// Decompose the sub-channel manager.
    /// </summary>
    protected override void OnDecompose()
    {
      _subChannelManager.Decompose();
    }

    #endregion

    #region ISubChannelManager members

    /// <summary>
    /// Reload the manager's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(TVDatabase.Entities.Tuner configuration)
    {
      base.ReloadConfiguration(configuration);
      _subChannelManager.ReloadConfiguration(configuration);
    }

    /// <summary>
    /// Set the manager's extensions.
    /// </summary>
    /// <param name="extensions">A list of the tuner's extensions, in priority order.</param>
    public override void SetExtensions(IList<ITunerExtension> extensions)
    {
      _subChannelManager.SetExtensions(extensions);
    }

    #region tuning

    /// <summary>
    /// This function should be called before the tuner is tuned to a new
    /// transmitter.
    /// </summary>
    public override void OnBeforeTune()
    {
      base.OnBeforeTune();
      _subChannelManager.OnBeforeTune();
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    /// <param name="id">The identifier of the sub-channel associated with the tuning process that is being cancelled.</param>
    public new void CancelTune(int id)
    {
      base.CancelTune(id);
      _subChannelManager.CancelTune(id);
    }

    #endregion

    #region sub-channels

    /// <summary>
    /// Get the set of sub-channel identifiers for each channel the tuner is
    /// currently decrypting.
    /// </summary>
    /// <returns>a collection of sub-channel identifier lists</returns>
    public override ICollection<IList<int>> GetDecryptedSubChannelDetails()
    {
      return _subChannelManager.GetDecryptedSubChannelDetails();
    }

    /// <summary>
    /// Determine whether a sub-channel is being decrypted.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the sub-channel is being decrypted, otherwise <c>false</c></returns>
    public override bool IsDecrypting(IChannel channel)
    {
      return _subChannelManager.IsDecrypting(channel);
    }

    #endregion

    #endregion
  }
}