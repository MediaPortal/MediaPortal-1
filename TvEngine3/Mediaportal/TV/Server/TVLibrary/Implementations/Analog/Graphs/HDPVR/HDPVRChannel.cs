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

using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog.Graphs.HDPVR
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles analog tv cards
  /// </summary>
  public class HDPVRChannel : TvDvbChannel
  {
    #region constants

    private readonly TvCardHDPVR _card;

    // The Hauppauge HD PVR and Colossus deliver a DVB stream with a single service on a fixed
    // service ID with a fixed PMT PID.
    private readonly int ServiceId = 1;
    private readonly int PmtPid = 0x100;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="HDPVRChannel"/> class.
    /// </summary>
    public HDPVRChannel(int subchannelId, TvCardHDPVR card, IBaseFilter filterTsWriter)
      : base(subchannelId, card, filterTsWriter, null)
    {
      // Keep a reference to the card for quality control.
      _card = card;

      _grabTeletext = false;
    }

    #region tuning and graph method overrides

    /// <summary>
    /// Should be called when the graph has been started.
    /// Sets up the PMT grabber to grab the PMT of the channel
    /// </summary>
    public override void OnGraphRunning()
    {
      Log.Debug("HDPVRChannel: subchannel {0} OnGraphRunning()", _subChannelId);

      if (!WaitForPmt(ServiceId, PmtPid))
      {
        throw new TvExceptionNoPMT("HDPVRChannel: PMT not received");
      }
    }

    /// <summary>
    /// Start timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override void OnStartTimeShifting(string fileName)
    {
      if (_card.SupportsQualityControl && !IsRecording)
      {
        _card.Quality.StartPlayback();
      }
      base.OnStartTimeShifting(fileName);
    }

    /// <summary>
    /// Start recording
    /// </summary>
    /// <param name="fileName">recording filename</param>
    protected override void OnStartRecording(string fileName)
    {
      if (_card.SupportsQualityControl)
      {
        _card.Quality.StartRecord();
      }
      base.OnStartRecording(fileName);
    }

    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected override void OnStopRecording()
    {
      base.OnStopRecording();
      if (_card.SupportsQualityControl && IsTimeShifting)
      {
        _card.Quality.StartPlayback();
      }
    }

    #endregion
  }
}