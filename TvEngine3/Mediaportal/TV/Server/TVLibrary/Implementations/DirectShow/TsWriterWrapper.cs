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
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using BroadcastStandard = Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer.BroadcastStandard;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  internal delegate int TsWriterSubChannelMethodDelegate(string methodName, ref object[] parameters);
  internal delegate int TsWriterScanMethodDelegate(string methodName, ref object[] parameters);

  /// <summary>
  /// A wrapper class for TsWriter. This class makes it easy to proxy all interaction with TsWriter
  /// through an extra layer of logic or translation. In particular, this wrapper is useful when
  /// TsWriter is hosted in a single threaded COM apartment.
  /// </summary>
  internal class TsWriterWrapper : ITsFilter, ITsChannelScan, IEncryptionStateChangeCallBack, IPmtCallBack, IVideoAudioObserver, ICaCallBack, IChannelScanCallBack
  {
    private class CallBackJob
    {
      public Type TargetType;
      public string MethodName;
      public object TargetInstance;
      public object[] Parameters;
    }

    #region variables

    private TsWriterSubChannelMethodDelegate _delegateSubChannel = null;
    private TsWriterScanMethodDelegate _delegateScan = null;

    private IEncryptionStateChangeCallBack _callBackEncryptionStateChange = null;
    private IPmtCallBack _callBackPmt = null;
    private IVideoAudioObserver _callBackVideoAudioObserverRecorder = null;
    private IVideoAudioObserver _callBackVideoAudioObserverTimeShifter = null;
    private ICaCallBack _callBackCat = null;
    private IChannelScanCallBack _callBackChannelScan = null;

    #endregion

    public TsWriterWrapper(TsWriterSubChannelMethodDelegate delegateSubChannel, TsWriterScanMethodDelegate delegateScan)
    {
      _delegateSubChannel = delegateSubChannel;
      _delegateScan = delegateScan;
    }

    #region sub-channel delegation

    public int AddChannel(ref int handle)
    {
      object[] parameters = new object[1] { handle };
      int hr = _delegateSubChannel("AddChannel", ref parameters);
      handle = (int)parameters[0];
      return hr;
    }

    public int DeleteChannel(int handle)
    {
      object[] parameters = new object[1] { handle };
      return _delegateSubChannel("DeleteChannel", ref parameters);
    }

    public int DeleteAllChannels()
    {
      object[] parameters = null;
      return _delegateSubChannel("DeleteAllChannels", ref parameters);
    }

    public int AnalyserAddPid(int handle, int pid)
    {
      object[] parameters = new object[2] { handle, pid };
      return _delegateSubChannel("AnalyserAddPid", ref parameters);
    }

    public int AnalyserRemovePid(int handle, int pid)
    {
      object[] parameters = new object[2] { handle, pid };
      return _delegateSubChannel("AnalyserRemovePid", ref parameters);
    }

    public int AnalyserGetPidCount(int handle, out int pidCount)
    {
      pidCount = 0;
      object[] parameters = new object[2] { handle, pidCount };
      int hr = _delegateSubChannel("AnalyserGetPidCount", ref parameters);
      pidCount = (int)parameters[1];
      return hr;
    }

    public int AnalyserGetPid(int handle, int pidIndex, out int pid, out EncryptionState encryptionState)
    {
      pid = 0;
      encryptionState = EncryptionState.NotSet;
      object[] parameters = new object[4] { handle, pidIndex, pid, encryptionState };
      int hr = _delegateSubChannel("AnalyserGetPid", ref parameters);
      pid = (int)parameters[2];
      encryptionState = (EncryptionState)parameters[3];
      return hr;
    }

    public int AnalyserSetCallBack(int handle, IEncryptionStateChangeCallBack callBack)
    {
      _callBackEncryptionStateChange = callBack;
      if (callBack != null)
      {
        callBack = this;
      }
      object[] parameters = new object[2] { handle, callBack };
      return _delegateSubChannel("AnalyserSetCallBack", ref parameters);
    }

    public int AnalyserReset(int handle)
    {
      object[] parameters = new object[1] { handle };
      return _delegateSubChannel("AnalyserReset", ref parameters);
    }

    public int PmtSetPmtPid(int handle, int pmtPid, int serviceId)
    {
      object[] parameters = new object[3] { handle, pmtPid, serviceId };
      return _delegateSubChannel("PmtSetPmtPid", ref parameters);
    }

    public int PmtSetCallBack(int handle, IPmtCallBack callBack)
    {
      _callBackPmt = callBack;
      if (callBack != null)
      {
        callBack = this;
      }
      object[] parameters = new object[2] { handle, callBack };
      return _delegateSubChannel("PmtSetCallBack", ref parameters);
    }

    public int PmtGetPmtData(int handle, IntPtr pmtData)
    {
      object[] parameters = new object[2] { handle, pmtData };
      return _delegateSubChannel("PmtGetPmtData", ref parameters);
    }

    public int RecordSetRecordingFileNameW(int handle, string fileName)
    {
      object[] parameters = new object[2] { handle, fileName };
      return _delegateSubChannel("RecordSetRecordingFileNameW", ref parameters);
    }

    public int RecordStartRecord(int handle)
    {
      object[] parameters = new object[1] { handle };
      return _delegateSubChannel("RecordStartRecord", ref parameters);
    }

    public int RecordStopRecord(int handle)
    {
      object[] parameters = new object[1] { handle };
      return _delegateSubChannel("RecordStopRecord", ref parameters);
    }

    public int RecordSetPmtPid(int handle, int pmtPid, int serviceId, byte[] pmtData, int pmtLength)
    {
      object[] parameters = new object[5] { handle, pmtPid, serviceId, pmtData, pmtLength };
      return _delegateSubChannel("RecordSetPmtPid", ref parameters);
    }

    public int RecorderSetVideoAudioObserver(int handle, IVideoAudioObserver observer)
    {
      _callBackVideoAudioObserverRecorder = observer;
      if (observer != null)
      {
        observer = this;
      }
      object[] parameters = new object[2] { handle, observer };
      return _delegateSubChannel("RecorderSetVideoAudioObserver", ref parameters);
    }

    public int TimeShiftSetTimeShiftingFileNameW(int handle, string fileName)
    {
      object[] parameters = new object[2] { handle, fileName };
      return _delegateSubChannel("TimeShiftSetTimeShiftingFileNameW", ref parameters);
    }

    public int TimeShiftStart(int handle)
    {
      object[] parameters = new object[1] { handle };
      return _delegateSubChannel("TimeShiftStart", ref parameters);
    }

    public int TimeShiftStop(int handle)
    {
      object[] parameters = new object[1] { handle };
      return _delegateSubChannel("TimeShiftStop", ref parameters);
    }

    public int TimeShiftReset(int handle)
    {
      object[] parameters = new object[1] { handle };
      return _delegateSubChannel("TimeShiftReset", ref parameters);
    }

    public int TimeShiftGetBufferSize(int handle, out long size)
    {
      size = 0;
      object[] parameters = new object[2] { handle, size };
      int hr = _delegateSubChannel("TimeShiftGetBufferSize", ref parameters);
      size = (long)parameters[1];
      return hr;
    }

    public int TimeShiftSetPmtPid(int handle, int pmtPid, int serviceId, byte[] pmtData, int pmtLength)
    {
      object[] parameters = new object[5] { handle, pmtPid, serviceId, pmtData, pmtLength };
      return _delegateSubChannel("TimeShiftSetPmtPid", ref parameters);
    }

    public int TimeShiftPause(int handle, byte onOff)
    {
      object[] parameters = new object[2] { handle, onOff };
      return _delegateSubChannel("TimeShiftPause", ref parameters);
    }

    public int TimeShiftSetParams(int handle, int minFiles, int maxFiles, uint chunkSize)
    {
      object[] parameters = new object[4] { handle, minFiles, maxFiles, chunkSize };
      return _delegateSubChannel("TimeShiftSetParams", ref parameters);
    }

    public int TimeShiftGetCurrentFilePosition(int handle, out long position, out long bufferId)
    {
      position = 0;
      bufferId = 0;
      object[] parameters = new object[3] { handle, position, bufferId };
      int hr = _delegateSubChannel("TimeShiftGetCurrentFilePosition", ref parameters);
      position = (long)parameters[1];
      bufferId = (long)parameters[2];
      return hr;
    }

    public int SetVideoAudioObserver(int handle, IVideoAudioObserver observer)
    {
      _callBackVideoAudioObserverTimeShifter = observer;
      if (observer != null)
      {
        observer = this;
      }
      object[] parameters = new object[2] { handle, observer };
      return _delegateSubChannel("SetVideoAudioObserver", ref parameters);
    }

    public int CaReset(int handle)
    {
      object[] parameters = new object[1] { handle };
      return _delegateSubChannel("CaReset", ref parameters);
    }

    public int CaSetCallBack(int handle, ICaCallBack callBack)
    {
      _callBackCat = callBack;
      if (callBack != null)
      {
        callBack = this;
      }
      object[] parameters = new object[2] { handle, callBack };
      return _delegateSubChannel("CaSetCallBack", ref parameters);
    }

    public int CaGetCaData(int handle, IntPtr caData)
    {
      object[] parameters = new object[2] { handle, caData };
      return _delegateSubChannel("CaGetCaData", ref parameters);
    }

    public int GetStreamQualityCounters(int handle, out int timeShiftByteCount, out int recordByteCount, out int timeShiftDiscontinuityCount, out int recordDiscontinuityCount)
    {
      timeShiftByteCount = 0;
      recordByteCount = 0;
      timeShiftDiscontinuityCount = 0;
      recordDiscontinuityCount = 0;
      object[] parameters = new object[5] { handle, timeShiftByteCount, recordByteCount, timeShiftDiscontinuityCount, recordDiscontinuityCount };
      int hr = _delegateSubChannel("GetStreamQualityCounters", ref parameters);
      timeShiftByteCount = (int)parameters[1];
      recordByteCount = (int)parameters[2];
      timeShiftDiscontinuityCount = (int)parameters[3];
      recordDiscontinuityCount = (int)parameters[4];
      return hr;
    }

    public int TimeShiftSetChannelType(int handle, int channelType)
    {
      object[] parameters = new object[2] { handle, channelType };
      return _delegateSubChannel("TimeShiftSetChannelType", ref parameters);
    }

    #endregion

    #region scan delegation

    public int SetCallBack(IChannelScanCallBack callBack)
    {
      _callBackChannelScan = callBack;
      if (callBack != null)
      {
        callBack = this;
      }
      object[] parameters = new object[1] { callBack };
      return _delegateScan("SetCallBack", ref parameters);
    }

    public int ScanStream(BroadcastStandard broadcastStandard)
    {
      object[] parameters = new object[1] { broadcastStandard };
      return _delegateScan("ScanStream", ref parameters);
    }

    public int StopStreamScan()
    {
      object[] parameters = null;
      return _delegateScan("StopStreamScan", ref parameters);
    }

    public int GetServiceCount(out int serviceCount)
    {
      serviceCount = 0;
      object[] parameters = new object[1] { serviceCount };
      int hr = _delegateScan("GetServiceCount", ref parameters);
      serviceCount = (int)parameters[0];
      return hr;
    }

    public int GetServiceDetail(int index,
                        out int originalNetworkId,
                        out int transportStreamId,
                        out int serviceId,
                        out IntPtr serviceName,
                        out IntPtr providerName,
                        out IntPtr logicalChannelNumber,
                        out int serviceType,
                        out int videoStreamCount,
                        out int audioStreamCount,
                        out bool isHighDefinition,
                        out bool isEncrypted,
                        out bool isRunning,
                        out int pmtPid,
                        out int previousOriginalNetworkId,
                        out int previousTransportStreamId,
                        out int previousServiceId,
                        ref int networkIdCount,
                        ref ushort[] networkIds,
                        ref int bouquetIdCount,
                        ref ushort[] bouquetIds,
                        ref int languageCount,
                        ref Iso639Code[] languages,
                        ref int availableInCellCount,
                        ref uint[] availableInCells,
                        ref int unavailableInCellCount,
                        ref uint[] unavailableInCells,
                        ref int targetRegionCount,
                        ref long[] targetRegions,
                        ref int availableInCountryCount,
                        ref Iso639Code[] availableInCountries,
                        ref int unavailableInCountryCount,
                        ref Iso639Code[] unavailableInCountries)
    {
      originalNetworkId = 0;
      transportStreamId = 0;
      serviceId = 0;
      serviceName = IntPtr.Zero;
      providerName = IntPtr.Zero;
      logicalChannelNumber = IntPtr.Zero;
      serviceType = 0;
      videoStreamCount = 0;
      audioStreamCount = 0;
      isHighDefinition = false;
      isEncrypted = false;
      isRunning = false;
      pmtPid = 0;
      previousOriginalNetworkId = 0;
      previousTransportStreamId = 0;
      previousServiceId = 0;
      object[] parameters = new object[33] { index, originalNetworkId, transportStreamId,
                                              serviceId, serviceName, providerName,
                                              logicalChannelNumber, serviceType, videoStreamCount,
                                              audioStreamCount, isHighDefinition, isEncrypted,
                                              isRunning, pmtPid, previousOriginalNetworkId,
                                              previousTransportStreamId, previousServiceId,
                                              networkIdCount, networkIds, bouquetIdCount,
                                              bouquetIds, languageCount, languages,
                                              availableInCellCount, availableInCells,
                                              unavailableInCellCount, unavailableInCells,
                                              targetRegionCount, targetRegions,
                                              availableInCountryCount, availableInCountries,
                                              unavailableInCountryCount, unavailableInCountries };
      int hr = _delegateScan("GetServiceDetail", ref parameters);
      originalNetworkId = (int)parameters[1];
      transportStreamId = (int)parameters[2];
      serviceId = (int)parameters[3];
      serviceName = (IntPtr)parameters[4];
      providerName = (IntPtr)parameters[5];
      logicalChannelNumber = (IntPtr)parameters[6];
      serviceType = (int)parameters[7];
      videoStreamCount = (int)parameters[8];
      audioStreamCount = (int)parameters[9];
      isHighDefinition = (bool)parameters[10];
      isEncrypted = (bool)parameters[11];
      isRunning = (bool)parameters[12];
      pmtPid = (int)parameters[13];
      previousOriginalNetworkId = (int)parameters[14];
      previousTransportStreamId = (int)parameters[15];
      previousServiceId = (int)parameters[16];
      networkIdCount = (int)parameters[17];
      networkIds = (ushort[])parameters[18];
      bouquetIdCount = (int)parameters[19];
      bouquetIds = (ushort[])parameters[20];
      languageCount = (int)parameters[21];
      languages = (Iso639Code[])parameters[22];
      availableInCellCount = (int)parameters[23];
      availableInCells = (uint[])parameters[24];
      unavailableInCellCount = (int)parameters[25];
      unavailableInCells = (uint[])parameters[26];
      targetRegionCount = (int)parameters[27];
      targetRegions = (long[])parameters[28];
      availableInCountryCount = (int)parameters[29];
      availableInCountries = (Iso639Code[])parameters[30];
      unavailableInCountryCount = (int)parameters[31];
      unavailableInCountries = (Iso639Code[])parameters[32];
      return hr;
    }

    public int ScanNetwork()
    {
      object[] parameters = null;
      return _delegateScan("ScanNetwork", ref parameters);
    }

    public int StopNetworkScan(out bool isOtherMuxServiceInfoAvailable)
    {
      isOtherMuxServiceInfoAvailable = false;
      object[] parameters = new object[1] { isOtherMuxServiceInfoAvailable };
      int hr = _delegateScan("StopNetworkScan", ref parameters);
      isOtherMuxServiceInfoAvailable = (bool)parameters[0];
      return hr;
    }

    public int GetMultiplexCount(out int multiplexCount)
    {
      multiplexCount = 0;
      object[] parameters = new object[1] { multiplexCount };
      int hr = _delegateScan("GetMultiplexCount", ref parameters);
      multiplexCount = (int)parameters[0];
      return hr;
    }

    public int GetMultiplexDetail(int index,
                            out int originalNetworkId,
                            out int transportStreamId,
                            out int type,
                            out int frequency,
                            out int polarisation,
                            out int modulation,
                            out int symbolRate,
                            out int bandwidth,
                            out int innerFecRate,
                            out int rollOff,
                            out int longitude,
                            out int cellId,
                            out int cellIdExtension,
                            out int plpId)
    {
      originalNetworkId = 0;
      transportStreamId = 0;
      type = 0;
      frequency = 0;
      polarisation = 0;
      modulation = 0;
      symbolRate = 0;
      bandwidth = 0;
      innerFecRate = 0;
      rollOff = 0;
      longitude = 0;
      cellId = 0;
      cellIdExtension = 0;
      plpId = 0;
      object[] parameters = new object[15] { index, originalNetworkId, transportStreamId, type,
                                              frequency, polarisation, modulation, symbolRate,
                                              bandwidth, innerFecRate, rollOff, longitude, cellId,
                                              cellIdExtension, plpId };
      int hr = _delegateScan("GetMultiplexDetail", ref parameters);
      originalNetworkId = (int)parameters[1];
      transportStreamId = (int)parameters[2];
      type = (int)parameters[3];
      frequency = (int)parameters[4];
      polarisation = (int)parameters[5];
      modulation = (int)parameters[6];
      symbolRate = (int)parameters[7];
      bandwidth = (int)parameters[8];
      innerFecRate = (int)parameters[9];
      rollOff = (int)parameters[10];
      longitude = (int)parameters[11];
      cellId = (int)parameters[12];
      cellIdExtension = (int)parameters[13];
      plpId = (int)parameters[14];
      return hr;
    }

    public int GetTargetRegionName(long targetRegionId, out IntPtr name)
    {
      name = IntPtr.Zero;
      object[] parameters = new object[2] { targetRegionId, name };
      int hr = _delegateScan("GetTargetRegionName", ref parameters);
      name = (IntPtr)parameters[1];
      return hr;
    }

    public int GetBouquetName(int bouquetId, out IntPtr name)
    {
      name = IntPtr.Zero;
      object[] parameters = new object[2] { bouquetId, name };
      int hr = _delegateScan("GetBouquetName", ref parameters);
      name = (IntPtr)parameters[1];
      return hr;
    }

    public int GetNetworkName(int networkId, out IntPtr name)
    {
      name = IntPtr.Zero;
      object[] parameters = new object[2] { networkId, name };
      int hr = _delegateScan("GetNetworkName", ref parameters);
      name = (IntPtr)parameters[1];
      return hr;
    }

    #endregion

    #region call back proxying

    private void StartCallBackThread(CallBackJob job)
    {
      if (job.TargetInstance == null)
      {
        this.LogError("TsWriter wrapper: failed to invoke {0} {1} call back, target is null", job.TargetType.Name, job.MethodName);
        return;
      }
      Thread t = new Thread(InvokeCallBack);
      t.Name = "TsWriter wrapper call back";
      t.Start(job);
    }

    private void InvokeCallBack(object param)
    {
      try
      {
        CallBackJob job = (CallBackJob)param;
        job.TargetType.GetMethod(job.MethodName).Invoke(job.TargetInstance, job.Parameters);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TsWriter wrapper: call back thread exception");
      }
    }

    #region IEncryptionStateChangeCallBack member

    public int OnEncryptionStateChange(int pid, EncryptionState encryptionState)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IEncryptionStateChangeCallBack);
      job.MethodName = "OnEncryptionStateChange";
      job.TargetInstance = _callBackEncryptionStateChange;
      job.Parameters = new object[2] { pid, encryptionState };
      StartCallBackThread(job);
      return 0;
    }

    #endregion

    #region IPmtCallBack member

    public int OnPmtReceived(int pmtPid, int serviceId, bool isServiceRunning)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IPmtCallBack);
      job.MethodName = "OnPmtReceived";
      job.TargetInstance = _callBackPmt;
      job.Parameters = new object[3] { pmtPid, serviceId, isServiceRunning };
      StartCallBackThread(job);
      return 0;
    }

    #endregion

    #region IVideoAudioObserver member

    public int OnNotify(PidType pidType)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IVideoAudioObserver);
      job.MethodName = "OnNotify";
      job.Parameters = new object[1] { pidType };
      if (_callBackVideoAudioObserverTimeShifter == _callBackVideoAudioObserverRecorder)
      {
        job.TargetInstance = _callBackVideoAudioObserverTimeShifter;
      }
      else if (_callBackVideoAudioObserverTimeShifter != null && _callBackVideoAudioObserverRecorder != null)
      {
        this.LogWarn("TsWriter wrapper: target for IVideoAudioObserver OnNotify call back is ambiguous, selecting recorder");
        job.TargetInstance = _callBackVideoAudioObserverRecorder;
      }
      else
      {
        job.TargetInstance = _callBackVideoAudioObserverRecorder ?? _callBackVideoAudioObserverTimeShifter;
      }
      StartCallBackThread(job);
      return 0;
    }

    #endregion

    #region ICaCallBack member

    public int OnCaReceived()
    {
      if (_callBackCat != null)
      {
        CallBackJob job = new CallBackJob();
        job.TargetType = typeof(ICaCallBack);
        job.MethodName = "OnCaReceived";
        job.TargetInstance = _callBackCat;
        StartCallBackThread(job);
      }
      return 0;
    }

    #endregion

    #region IChannelScanCallBack member

    public int OnScannerDone()
    {
      if (_callBackChannelScan != null)
      {
        CallBackJob job = new CallBackJob();
        job.TargetType = typeof(IChannelScanCallBack);
        job.MethodName = "OnScannerDone";
        job.TargetInstance = _callBackChannelScan;
        StartCallBackThread(job);
      }
      return 0;
    }

    #endregion

    #endregion
  }
}