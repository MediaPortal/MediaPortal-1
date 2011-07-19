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
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Channels;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles analog tv cards
  /// </summary>
  public class HDPVRChannel : TvDvbChannel, ITvSubChannel, IVideoAudioObserver, IPMTCallback
  {
    #region constants

    private readonly TvCardHDPVR _card;

    // The Hauppauge HD PVR and Colossus deliver a DVB stream with a single service on a fixed
    // service ID with a fixed PMT PID.
    private readonly String _deviceType;
    private readonly int SERVICE_ID = 1;
    private readonly int PMT_PID = 0x100;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="HDPVRChannel"/> class.
    /// </summary>
    public HDPVRChannel(TvCardHDPVR card, String deviceType, Int32 subchannelId, IBaseFilter filterTsWriter, IFilterGraph2 graphBuilder)
    {
      _eventPMT = new ManualResetEvent(false);
      _graphState = GraphState.Created;
      _graphBuilder = graphBuilder;
      _channelInfo = new ChannelInfo();
      _pmtPid = -1;
      _subChannelIndex = -1;

      // Keep a reference to the card for quality control.
      _card = card;
      _deviceType = deviceType;

      _tsFilterInterface = (ITsFilter)filterTsWriter;
      _tsFilterInterface.AddChannel(ref _subChannelIndex);
      _subChannelId = subchannelId;
      _timeshiftFileName = "";
      _recordingFileName = "";
      _pmtData = null;
      _pmtLength = 0;

      _grabTeletext = false;
      _mdplugs = null;
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~HDPVRChannel()
    {
      if (_eventPMT != null)
      {
        _eventPMT.Close();
      }
    }

    #region tuning and graph method overrides

    /// <summary>
    /// Start timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected override bool OnStartTimeShifting(string fileName)
    {
      if (_card.SupportsQualityControl && !IsRecording)
      {
        _card.Quality.StartPlayback();
      }
      return base.OnStartTimeShifting(fileName);
    }

    /// <summary>
    /// Stop timeshifting
    /// </summary>
    /// <returns></returns>
    protected override void OnStopTimeShifting()
    {
      base.OnStopTimeShifting();
      if (_card.SupportsQualityControl && IsRecording)
      {
        _card.Quality.StartRecord();
      }
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

    /// <summary>
    /// Wait for PMT to be found in the transport stream.
    /// </summary>
    /// <returns><c>true</c> if PMT was found, otherwise <c>false</c></returns>
    protected override bool WaitForPMT()
    {
      int pid = PMT_PID;
      bool pmtFound = false;
      TimeSpan waitLength = TimeSpan.MinValue;
      while (!pmtFound)
      {
        SetupPmtGrabber(pid, SERVICE_ID);
        _pmtRequested = true;
        DateTime dtStartWait = DateTime.Now;
        pmtFound = _eventPMT.WaitOne(_parameters.TimeOutPMT * 1000, true);
        waitLength = DateTime.Now - dtStartWait;
        if (!pmtFound)
        {
          Log.Log.Debug("HDPVR: timed out waiting for PMT after {0} seconds", waitLength.TotalSeconds);
          if (pid == 0)
          {
            Log.Log.Debug("Giving up waiting for PMT. You might need to increase the PMT timeout value.");
            return false;
          }
          else
          {
            Log.Log.Debug("Setting pid to 0 to search for new PMT.");
            pid = 0;
          }
        }
      }

      Log.Log.Debug("HDPVR: found PMT after {0} seconds", waitLength.TotalSeconds);
      return HandlePmt();
    }

    /// <summary>
    /// Stub override to allow inherritance from TvDvbChannel.
    /// </summary>
    protected override bool SendPmtToCam(out bool updatePids, out int waitInterval)
    {
      updatePids = false;
      waitInterval = 0;
      return true;
    }

    #endregion

    #region IPMTCallback Members

    /// <summary>
    /// Called when the PMT has been received.
    /// </summary>
    /// <returns></returns>
    public override int OnPMTReceived(int pmtPid)
    {
      Log.Log.WriteFile("HDPVR: OnPMTReceived() subch:{0} pid 0x{1:X}", _subChannelId, pmtPid);
      _pmtPid = pmtPid;
      if (_eventPMT != null)
      {
        _eventPMT.Set();
      }
      if (!_pmtRequested)
      {
        HandlePmt();
      }
      _pmtRequested = false;
      return 0;
    }

    private bool HandlePmt()
    {
      IntPtr pmtMem = Marshal.AllocCoTaskMem(4096); // max. size for pmt
      try
      {
        _pmtLength = _tsFilterInterface.PmtGetPMTData(_subChannelIndex, pmtMem);
        if (_pmtLength < 6)
        {
          return false;
        }

        // Check the program number.
        _pmtData = new byte[_pmtLength];
        Marshal.Copy(pmtMem, _pmtData, 0, _pmtLength);
        int version = ((_pmtData[5] >> 1) & 0x1F);
        int pmtProgramNumber = (_pmtData[3] << 8) + _pmtData[4];
        Log.Log.Info("HDPVR: PMT sid=0x{0:X} pid=0x{1:X} version=0x{2:X}", pmtProgramNumber, _pmtPid, version);
        if (pmtProgramNumber != SERVICE_ID)
        {
          throw new TvException("HDPVRChannel: PMT program number doesn't match expected service ID");
        }

        // Get the program PIDs.
        _pmtVersion = version;
        _channelInfo = new ChannelInfo();
        _channelInfo.DecodePmt(_pmtData);
        _channelInfo.serviceID = pmtProgramNumber;
        _channelInfo.network_pmt_PID = _pmtPid;
        SetMpegPidMapping(_channelInfo);
        return true;
      }
      finally
      {
        Marshal.FreeCoTaskMem(pmtMem);
      }
    }

    #endregion
  }
}