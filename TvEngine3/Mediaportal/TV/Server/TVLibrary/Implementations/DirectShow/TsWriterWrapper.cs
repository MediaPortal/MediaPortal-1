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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  internal delegate object TsWriterMethodDelegateITsWriter(string methodName, ref object[] parameters);
  internal delegate object TsWriterMethodDelegateIGrabberSiDvb(string methodName, ref object[] parameters);
  internal delegate object TsWriterMethodDelegateIGrabberSiMpeg(string methodName, ref object[] parameters);

  /// <summary>
  /// A wrapper class for TsWriter. This class makes it easy to proxy all interaction with TsWriter
  /// through an extra layer of logic or translation. In particular, this wrapper is useful when
  /// TsWriter is hosted in a single threaded COM apartment.
  /// </summary>
  internal class TsWriterWrapper : ITsWriter, IGrabberSiDvb, IGrabberSiMpeg, ICallBackGrabber, IObserver
  {
    private class CallBackJob
    {
      public Type TargetType;
      public string MethodName;
      public object TargetInstance;
      public object[] Parameters;
    }

    private class ChannelObserverWrapper : IChannelObserver
    {
      public IChannelObserver Observer = null;

      public void OnSeen(ushort pid, PidType pidType)
      {
        if (Observer == null)
        {
          this.LogError("TsWriter wrapper: failed to invoke IChannelObserver OnSeen() call back, target is null");
          return;
        }

        CallBackJob job = new CallBackJob();
        job.TargetType = typeof(IEncryptionStateChangeCallBack);
        job.MethodName = "OnEncryptionStateChange";
        job.TargetInstance = Observer;
        job.Parameters = new object[2] { pid, pidType };

        Thread t = new Thread(InvokeCallBack);
        t.Name = "TsWriter channel observer wrapper call back";
        t.Start(job);

        Observer.OnSeen(pid, pidType);
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
          this.LogError(ex, "TsWriter channel observer wrapper: call back thread exception");
        }
      }
    }

    #region variables

    private TsWriterMethodDelegateITsWriter _delegateTsWriter = null;
    private TsWriterMethodDelegateIGrabberSiDvb _delegateGrabberSiDvb = null;
    private TsWriterMethodDelegateIGrabberSiMpeg _delegateGrabberSiMpeg = null;

    private ICallBackGrabber _callBackGrabber = null;
    private IObserver _observer = null;
    private IDictionary<int, ChannelObserverWrapper> _channelObservers = new Dictionary<int, ChannelObserverWrapper>();

    #endregion

    public TsWriterWrapper(TsWriterMethodDelegateITsWriter delegateTsWriter, TsWriterMethodDelegateIGrabberSiDvb delegateGrabberSiDvb, TsWriterMethodDelegateIGrabberSiMpeg delegateGrabberSiMpeg)
    {
      _delegateTsWriter = delegateTsWriter;
      _delegateGrabberSiDvb = delegateGrabberSiDvb;
      _delegateGrabberSiMpeg = delegateGrabberSiMpeg;
    }

    #region ITsWriter delegation

    public int ConfigureLogging(string path)
    {
      object[] parameters = new object[1] { path };
      return (int)_delegateTsWriter("ConfigureLogging", ref parameters);
    }

    public void DumpInput(bool enableTs, bool enableOobSi)
    {
      object[] parameters = new object[2] { enableTs, enableOobSi };
      _delegateTsWriter("DumpInput", ref parameters);
    }

    public void CheckSectionCrcs(bool enable)
    {
      object[] parameters = new object[1] { enable };
      _delegateTsWriter("CheckSectionCrcs", ref parameters);
    }

    public void SetObserver(IObserver observer)
    {
      _observer = observer;
      if (observer != null)
      {
        observer = this;
      }
      object[] parameters = new object[1] { observer };
      _delegateTsWriter("SetObserver", ref parameters);
    }

    public void Start()
    {
      object[] parameters = null;
      _delegateTsWriter("Start", ref parameters);
    }

    public void Stop()
    {
      object[] parameters = null;
      _delegateTsWriter("Stop", ref parameters);
    }

    public int AddChannel(IChannelObserver observer, out int handle)
    {
      ChannelObserverWrapper observerWrapper = new ChannelObserverWrapper();
      observerWrapper.Observer = observer;
      handle = -1;
      object[] parameters = new object[2] { observerWrapper, handle };
      int hr = (int)_delegateTsWriter("AddChannel", ref parameters);
      handle = (int)parameters[1];
      if (hr == (int)MediaPortal.Common.Utils.NativeMethods.HResult.S_OK)
      {
        _channelObservers[handle] = observerWrapper;
      }
      return hr;
    }

    public void DeleteChannel(int handle)
    {
      object[] parameters = new object[1] { handle };
      _delegateTsWriter("DeleteChannel", ref parameters);
      _channelObservers.Remove(handle);
    }

    public void DeleteAllChannels()
    {
      object[] parameters = null;
      _delegateTsWriter("DeleteAllChannels", ref parameters);
      _channelObservers.Clear();
    }

    public int RecorderSetFileName(int handle, string fileName)
    {
      object[] parameters = new object[2] { handle, fileName };
      return (int)_delegateTsWriter("RecorderSetFileName", ref parameters);
    }

    public int RecorderSetPmt(int handle, byte[] pmt, ushort pmtSize, bool isDynamicPmtChange)
    {
      object[] parameters = new object[4] { handle, pmt, pmtSize, isDynamicPmtChange };
      return (int)_delegateTsWriter("RecorderSetPmt", ref parameters);
    }

    public int RecorderStart(int handle)
    {
      object[] parameters = new object[1] { handle };
      return (int)_delegateTsWriter("RecorderStart", ref parameters);
    }

    public int RecorderGetStreamQuality(int handle,
                                        out ulong countTsPackets,
                                        out ulong countDiscontinuities,
                                        out ulong countDroppedBytes)
    {
      countTsPackets = 0;
      countDiscontinuities = 0;
      countDroppedBytes = 0;
      object[] parameters = new object[4] { handle, countTsPackets, countDiscontinuities, countDroppedBytes };
      int hr = (int)_delegateTsWriter("RecorderGetStreamQuality", ref parameters);
      countTsPackets = (ulong)parameters[1];
      countDiscontinuities = (ulong)parameters[2];
      countDroppedBytes = (ulong)parameters[3];
      return hr;
    }

    public int RecorderStop(int handle)
    {
      object[] parameters = new object[1] { handle };
      return (int)_delegateTsWriter("RecorderStop", ref parameters);
    }

    public int TimeShifterSetFileName(int handle, string fileName)
    {
      object[] parameters = new object[2] { handle, fileName };
      return (int)_delegateTsWriter("TimeShifterSetFileName", ref parameters);
    }

    public int TimeShifterSetParameters(int handle,
                                        uint fileCountMinimum,
                                        uint fileCountMaximum,
                                        ulong fileSizeBytes)
    {
      object[] parameters = new object[4] { handle, fileCountMinimum, fileCountMaximum, fileSizeBytes };
      return (int)_delegateTsWriter("TimeShifterSetParameters", ref parameters);
    }

    public int TimeShifterSetPmt(int handle, byte[] pmt, ushort pmtSize, bool isDynamicPmtChange)
    {
      object[] parameters = new object[4] { handle, pmt, pmtSize, isDynamicPmtChange };
      return (int)_delegateTsWriter("TimeShifterSetPmt", ref parameters);
    }

    public int TimeShifterStart(int handle)
    {
      object[] parameters = new object[1] { handle };
      return (int)_delegateTsWriter("TimeShifterStart", ref parameters);
    }

    public int TimeShifterGetStreamQuality(int handle,
                                            out ulong countTsPackets,
                                            out ulong countDiscontinuities,
                                            out ulong countDroppedBytes)
    {
      countTsPackets = 0;
      countDiscontinuities = 0;
      countDroppedBytes = 0;
      object[] parameters = new object[4] { handle, countTsPackets, countDiscontinuities, countDroppedBytes };
      int hr = (int)_delegateTsWriter("TimeShifterGetStreamQuality", ref parameters);
      countTsPackets = (ulong)parameters[1];
      countDiscontinuities = (ulong)parameters[2];
      countDroppedBytes = (ulong)parameters[3];
      return hr;
    }

    public int TimeShifterGetCurrentFilePosition(int handle, out ulong position, out uint bufferId)
    {
      position = 0;
      bufferId = 0;
      object[] parameters = new object[3] { handle, position, bufferId };
      int hr = (int)_delegateTsWriter("TimeShifterGetCurrentFilePosition", ref parameters);
      position = (ulong)parameters[1];
      bufferId = (uint)parameters[2];
      return hr;
    }

    public int TimeShifterStop(int handle)
    {
      object[] parameters = new object[1] { handle };
      return (int)_delegateTsWriter("TimeShifterStop", ref parameters);
    }

    #endregion

    /// <remarks>
    /// The same signature is used for both the DVB and MPEG SI grabbers. So, for us calls to
    /// SetCallBack() for those 2 interfaces are indistinguishable. This means that the same
    /// delegate must handle the call backs for both interfaces.
    /// </remarks>
    public void SetCallBack(ICallBackGrabber callBack)
    {
      _callBackGrabber = callBack;
      if (callBack != null)
      {
        callBack = this;
      }
      object[] parameters = new object[1] { callBack };
      _delegateGrabberSiDvb("SetCallBack", ref parameters);
      _delegateGrabberSiMpeg("SetCallBack", ref parameters);
    }

    #region IGrabberSiDvb delegation

    public bool IsSeenBat()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsSeenBat", ref parameters);
    }

    public bool IsSeenNitActual()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsSeenNitActual", ref parameters);
    }

    public bool IsSeenNitOther()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsSeenNitOther", ref parameters);
    }

    public bool IsSeenSdtActual()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsSeenSdtActual", ref parameters);
    }

    public bool IsSeenSdtOther()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsSeenSdtOther", ref parameters);
    }

    public bool IsReadyBat()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsReadyBat", ref parameters);
    }

    public bool IsReadyNitActual()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsReadyNitActual", ref parameters);
    }

    public bool IsReadyNitOther()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsReadyNitOther", ref parameters);
    }

    public bool IsReadySdtActual()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsReadySdtActual", ref parameters);
    }

    public bool IsReadySdtOther()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiDvb("IsReadySdtOther", ref parameters);
    }

    public ushort GetServiceCount()
    {
      object[] parameters = null;
      return (ushort)_delegateGrabberSiDvb("GetServiceCount", ref parameters);
    }

    public bool GetService(ushort index,
                            ushort preferredLogicalChannelNumberBouquetId,
                            ushort preferredLogicalChannelNumberRegionId,
                            out byte tableId,
                            out ushort originalNetworkId,
                            out ushort transportStreamId,
                            out ushort serviceId,
                            out ushort referenceServiceId,
                            out ushort freesatChannelId,
                            out ushort openTvChannelId,
                            out ushort logicalChannelNumber,
                            out byte dishSubChannelNumber,
                            out bool eitScheduleFlag,
                            out bool eitPresentFollowingFlag,
                            out byte runningStatus,
                            out bool freeCaMode,
                            out byte serviceType,
                            out byte serviceNameCount,
                            out bool visibleInGuide,
                            out ushort streamCountVideo,
                            out ushort streamCountAudio,
                            out bool isHighDefinition,
                            out bool isStandardDefinition,
                            out bool isThreeDimensional,
                            Iso639Code[] audioLanguages,
                            ref byte audioLanguageCount,
                            Iso639Code[] subtitlesLanguages,
                            ref byte subtitlesLanguageCount,
                            ushort[] networkIds,
                            ref byte networkIdCount,
                            ushort[] bouquetIds,
                            ref byte bouquetIdCount,
                            Iso639Code[] availableInCountries,
                            ref byte availableInCountryCount,
                            Iso639Code[] unavailableInCountries,
                            ref byte unavailableInCountryCount,
                            uint[] availableInCells,
                            ref byte availableInCellCount,
                            uint[] unavailableInCells,
                            ref byte unavailableInCellCount,
                            ulong[] targetRegionIds,
                            ref byte targetRegionIdCount,
                            ushort[] freesatRegionIds,
                            ref byte freesatRegionIdCount,
                            ushort[] openTvRegionIds,
                            ref byte openTvRegionIdCount,
                            ushort[] freesatChannelCategoryIds,
                            ref byte freesatChannelCategoryIdCount,
                            out byte virginMediaChannelCategoryId,
                            out ushort dishMarketId,
                            byte[] norDigChannelListIds,
                            ref byte norDigChannelListIdCount,
                            out ushort previousOriginalNetworkId,
                            out ushort previousTransportStreamId,
                            out ushort previousServiceId,
                            out ushort epgOriginalNetworkId,
                            out ushort epgTransportStreamId,
                            out ushort epgServiceId)
    {
      tableId = 0;
      originalNetworkId = 0;
      transportStreamId = 0;
      serviceId = 0;
      referenceServiceId = 0;
      freesatChannelId = 0;
      openTvChannelId = 0;
      logicalChannelNumber = 0;
      dishSubChannelNumber = 0;
      eitScheduleFlag = false;
      eitPresentFollowingFlag = false;
      runningStatus = 0;
      freeCaMode = false;
      serviceType = 0;
      serviceNameCount = 0;
      visibleInGuide = true;
      streamCountVideo = 0;
      streamCountAudio = 0;
      isHighDefinition = false;
      isStandardDefinition = false;
      isThreeDimensional = false;
      virginMediaChannelCategoryId = 0;
      dishMarketId = 0;
      previousOriginalNetworkId = 0;
      previousTransportStreamId = 0;
      previousServiceId = 0;
      epgOriginalNetworkId = 0;
      epgTransportStreamId = 0;
      epgServiceId = 0;
      object[] parameters = new object[58]
      {
        index,
        preferredLogicalChannelNumberBouquetId,
        preferredLogicalChannelNumberRegionId,
        tableId,
        originalNetworkId,
        transportStreamId,
        serviceId,
        referenceServiceId,
        freesatChannelId,
        openTvChannelId,
        logicalChannelNumber,
        dishSubChannelNumber,
        eitScheduleFlag,
        eitPresentFollowingFlag,
        runningStatus,
        freeCaMode,
        serviceType,
        serviceNameCount,
        visibleInGuide,
        streamCountVideo,
        streamCountAudio,
        isHighDefinition,
        isStandardDefinition,
        isThreeDimensional,
        audioLanguages,
        audioLanguageCount,
        subtitlesLanguages,
        subtitlesLanguageCount,
        networkIds,
        networkIdCount,
        bouquetIds,
        bouquetIdCount,
        availableInCountries,
        availableInCountryCount,
        unavailableInCountries,
        unavailableInCountryCount,
        availableInCells,
        availableInCellCount,
        unavailableInCells,
        unavailableInCellCount,
        targetRegionIds,
        targetRegionIdCount,
        freesatRegionIds,
        freesatRegionIdCount,
        openTvRegionIds,
        openTvRegionIdCount,
        freesatChannelCategoryIds,
        freesatChannelCategoryIdCount,
        virginMediaChannelCategoryId,
        dishMarketId,
        norDigChannelListIds,
        norDigChannelListIdCount,
        previousOriginalNetworkId,
        previousTransportStreamId,
        previousServiceId,
        epgOriginalNetworkId,
        epgTransportStreamId,
        epgServiceId
      };
      bool result = (bool)_delegateGrabberSiDvb("GetService", ref parameters);
      tableId = (byte)parameters[3];
      originalNetworkId = (ushort)parameters[4];
      transportStreamId = (ushort)parameters[5];
      serviceId = (ushort)parameters[6];
      referenceServiceId = (ushort)parameters[7];
      freesatChannelId = (ushort)parameters[8];
      openTvChannelId = (ushort)parameters[9];
      logicalChannelNumber = (ushort)parameters[10];
      dishSubChannelNumber = (byte)parameters[11];
      eitScheduleFlag = (bool)parameters[12];
      eitPresentFollowingFlag = (bool)parameters[13];
      runningStatus = (byte)parameters[14];
      freeCaMode = (bool)parameters[15];
      serviceType = (byte)parameters[16];
      serviceNameCount = (byte)parameters[17];
      visibleInGuide = (bool)parameters[18];
      streamCountVideo = (ushort)parameters[19];
      streamCountAudio = (ushort)parameters[20];
      isHighDefinition = (bool)parameters[21];
      isStandardDefinition = (bool)parameters[22];
      isThreeDimensional = (bool)parameters[23];
      audioLanguageCount = (byte)parameters[25];
      subtitlesLanguageCount = (byte)parameters[27];
      networkIdCount = (byte)parameters[29];
      bouquetIdCount = (byte)parameters[31];
      availableInCountryCount = (byte)parameters[33];
      unavailableInCountryCount = (byte)parameters[35];
      availableInCellCount = (byte)parameters[37];
      unavailableInCellCount = (byte)parameters[39];
      targetRegionIdCount = (byte)parameters[41];
      freesatRegionIdCount = (byte)parameters[43];
      openTvRegionIdCount = (byte)parameters[45];
      freesatChannelCategoryIdCount = (byte)parameters[47];
      virginMediaChannelCategoryId = (byte)parameters[48];
      dishMarketId = (ushort)parameters[49];
      norDigChannelListIdCount = (byte)parameters[51];
      previousOriginalNetworkId = (ushort)parameters[52];
      previousTransportStreamId = (ushort)parameters[53];
      previousServiceId = (ushort)parameters[54];
      epgOriginalNetworkId = (ushort)parameters[55];
      epgTransportStreamId = (ushort)parameters[56];
      epgServiceId = (ushort)parameters[57];
      return result;
    }

    public bool GetServiceNameByIndex(ushort serviceIndex,
                                      byte nameIndex,
                                      out Iso639Code language,
                                      IntPtr providerName,
                                      ref ushort providerNameBufferSize,
                                      IntPtr serviceName,
                                      ref ushort serviceNameBufferSize)
    {
      language = new Iso639Code();
      object[] parameters = new object[7]
      {
        serviceIndex,
        nameIndex,
        language,
        providerName,
        providerNameBufferSize,
        serviceName,
        serviceNameBufferSize
      };
      bool result = (bool)_delegateGrabberSiDvb("GetServiceNameByIndex", ref parameters);
      language = (Iso639Code)parameters[2];
      providerNameBufferSize = (ushort)parameters[4];
      serviceNameBufferSize = (ushort)parameters[6];
      return result;
    }

    public bool GetServiceNameByLanguage(ushort serviceIndex,
                                          Iso639Code language,
                                          IntPtr providerName,
                                          ref ushort providerNameBufferSize,
                                          IntPtr serviceName,
                                          ref ushort serviceNameBufferSize)
    {
      object[] parameters = new object[6]
      {
        serviceIndex,
        language,
        providerName,
        providerNameBufferSize,
        serviceName,
        serviceNameBufferSize
      };
      bool result = (bool)_delegateGrabberSiDvb("GetServiceNameByLanguage", ref parameters);
      providerNameBufferSize = (ushort)parameters[3];
      serviceNameBufferSize = (ushort)parameters[5];
      return result;
    }

    public byte GetNetworkNameCount(ushort networkId)
    {
      object[] parameters = new object[1] { networkId };
      return (byte)_delegateGrabberSiDvb("GetNetworkNameCount", ref parameters);
    }

    public bool GetNetworkNameByIndex(ushort networkId,
                                      byte index,
                                      out Iso639Code language,
                                      IntPtr name,
                                      ref ushort nameBufferSize)
    {
      language = new Iso639Code();
      object[] parameters = new object[5] { networkId, index, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetNetworkNameByIndex", ref parameters);
      language = (Iso639Code)parameters[2];
      nameBufferSize = (ushort)parameters[4];
      return result;
    }

    public bool GetNetworkNameByLanguage(ushort networkId,
                                          Iso639Code language,
                                          IntPtr name,
                                          ref ushort nameBufferSize)
    {
      object[] parameters = new object[4] { networkId, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetNetworkNameByLanguage", ref parameters);
      nameBufferSize = (ushort)parameters[3];
      return result;
    }

    public byte GetBouquetNameCount(ushort bouquetId)
    {
      object[] parameters = new object[1] { bouquetId };
      return (byte)_delegateGrabberSiDvb("GetBouquetNameCount", ref parameters);
    }

    public bool GetBouquetNameByIndex(ushort bouquetId,
                                      byte index,
                                      out Iso639Code language,
                                      IntPtr name,
                                      ref ushort nameBufferSize)
    {
      language = new Iso639Code();
      object[] parameters = new object[5] { bouquetId, index, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetBouquetNameByIndex", ref parameters);
      language = (Iso639Code)parameters[2];
      nameBufferSize = (ushort)parameters[4];
      return result;
    }

    public bool GetBouquetNameByLanguage(ushort bouquetId,
                                          Iso639Code language,
                                          IntPtr name,
                                          ref ushort nameBufferSize)
    {
      object[] parameters = new object[4] { bouquetId, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetBouquetNameByLanguage", ref parameters);
      nameBufferSize = (ushort)parameters[3];
      return result;
    }

    public byte GetTargetRegionNameCount(ulong regionId)
    {
      object[] parameters = new object[1] { regionId };
      return (byte)_delegateGrabberSiDvb("GetTargetRegionNameCount", ref parameters);
    }

    public bool GetTargetRegionNameByIndex(ulong regionId,
                                            byte index,
                                            out Iso639Code language,
                                            IntPtr name,
                                            ref ushort nameBufferSize)
    {
      language = new Iso639Code();
      object[] parameters = new object[5] { regionId, index, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetTargetRegionNameByIndex", ref parameters);
      language = (Iso639Code)parameters[2];
      nameBufferSize = (ushort)parameters[4];
      return result;
    }

    public bool GetTargetRegionNameByLanguage(ulong regionId,
                                              Iso639Code language,
                                              IntPtr name,
                                              ref ushort nameBufferSize)
    {
      object[] parameters = new object[4] { regionId, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetTargetRegionNameByLanguage", ref parameters);
      nameBufferSize = (ushort)parameters[3];
      return result;
    }

    public byte GetFreesatRegionNameCount(ushort regionId)
    {
      object[] parameters = new object[1] { regionId };
      return (byte)_delegateGrabberSiDvb("GetFreesatRegionNameCount", ref parameters);
    }

    public bool GetFreesatRegionNameByIndex(ushort regionId,
                                            byte index,
                                            out Iso639Code language,
                                            IntPtr name,
                                            ref ushort nameBufferSize)
    {
      language = new Iso639Code();
      object[] parameters = new object[5] { regionId, index, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetFreesatRegionNameByIndex", ref parameters);
      language = (Iso639Code)parameters[2];
      nameBufferSize = (ushort)parameters[4];
      return result;
    }

    public bool GetFreesatRegionNameByLanguage(ushort regionId,
                                                Iso639Code language,
                                                IntPtr name,
                                                ref ushort nameBufferSize)
    {
      object[] parameters = new object[4] { regionId, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetFreesatRegionNameByLanguage", ref parameters);
      nameBufferSize = (ushort)parameters[3];
      return result;
    }

    public byte GetFreesatChannelCategoryNameCount(ushort categoryId)
    {
      object[] parameters = new object[1] { categoryId };
      return (byte)_delegateGrabberSiDvb("GetFreesatChannelCategoryNameCount", ref parameters);
    }

    public bool GetFreesatChannelCategoryNameByIndex(ushort categoryId,
                                                      byte index,
                                                      out Iso639Code language,
                                                      IntPtr name,
                                                      ref ushort nameBufferSize)
    {
      language = new Iso639Code();
      object[] parameters = new object[5] { categoryId, index, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetFreesatChannelCategoryNameByIndex", ref parameters);
      language = (Iso639Code)parameters[2];
      nameBufferSize = (ushort)parameters[4];
      return result;
    }

    public bool GetFreesatChannelCategoryNameByLanguage(ushort categoryId,
                                                        Iso639Code language,
                                                        IntPtr name,
                                                        ref ushort nameBufferSize)
    {
      object[] parameters = new object[4] { categoryId, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetFreesatChannelCategoryNameByLanguage", ref parameters);
      nameBufferSize = (ushort)parameters[3];
      return result;
    }

    public byte GetNorDigChannelListNameCount(byte channelListId)
    {
      object[] parameters = new object[1] { channelListId };
      return (byte)_delegateGrabberSiDvb("GetNorDigChannelListNameCount", ref parameters);
    }

    public bool GetNorDigChannelListNameByIndex(byte channelListId,
                                                byte index,
                                                out Iso639Code language,
                                                IntPtr name,
                                                ref ushort nameBufferSize)
    {
      language = new Iso639Code();
      object[] parameters = new object[5] { channelListId, index, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetNorDigChannelListNameByIndex", ref parameters);
      language = (Iso639Code)parameters[2];
      nameBufferSize = (ushort)parameters[4];
      return result;
    }

    public bool GetNorDigChannelListNameByLanguage(byte channelListId,
                                                    Iso639Code language,
                                                    IntPtr name,
                                                    ref ushort nameBufferSize)
    {
      object[] parameters = new object[4] { channelListId, language, name, nameBufferSize };
      bool result = (bool)_delegateGrabberSiDvb("GetNorDigChannelListNameByLanguage", ref parameters);
      nameBufferSize = (ushort)parameters[3];
      return result;
    }

    public ushort GetTransmitterCount()
    {
      object[] parameters = null;
      return (ushort)_delegateGrabberSiDvb("GetTransmitterCount", ref parameters);
    }

    public bool GetTransmitter(ushort index,
                                out ushort tableId,
                                out ushort networkId,
                                out ushort originalNetworkId,
                                out ushort transportStreamId,
                                out bool isHomeTransmitter,
                                out BroadcastStandard broadcastStandard,
                                uint[] frequencies,
                                ref byte frequencyCount,
                                out byte polarisation,
                                out byte modulation,
                                out uint symbolRate,
                                out ushort bandwidth,
                                out byte innerFecRate,
                                out byte rollOffFactor,
                                out short longitude,
                                out ushort cellId,
                                out byte cellIdExtension,
                                out byte plpId)
    {
      tableId = 0;
      networkId = 0;
      originalNetworkId = 0;
      transportStreamId = 0;
      isHomeTransmitter = false;
      broadcastStandard = BroadcastStandard.Unknown;
      polarisation = 0;
      modulation = 0;
      symbolRate = 0;
      bandwidth = 0;
      innerFecRate = 0;
      rollOffFactor = 0;
      longitude = 0;
      cellId = 0;
      cellIdExtension = 0;
      plpId = 0;
      object[] parameters = new object[19]
      {
        index,
        tableId,
        networkId,
        originalNetworkId,
        transportStreamId,
        isHomeTransmitter,
        broadcastStandard,
        frequencies,
        frequencyCount,
        polarisation,
        modulation,
        symbolRate,
        bandwidth,
        innerFecRate,
        rollOffFactor,
        longitude,
        cellId,
        cellIdExtension,
        plpId
      };
      bool result = (bool)_delegateGrabberSiDvb("GetTransmitter", ref parameters);
      tableId = (byte)parameters[1];
      networkId = (ushort)parameters[2];
      originalNetworkId = (ushort)parameters[3];
      transportStreamId = (ushort)parameters[4];
      isHomeTransmitter = (bool)parameters[5];
      broadcastStandard = (BroadcastStandard)parameters[6];
      frequencyCount = (byte)parameters[8];
      polarisation = (byte)parameters[9];
      modulation = (byte)parameters[10];
      symbolRate = (uint)parameters[11];
      bandwidth = (ushort)parameters[12];
      innerFecRate = (byte)parameters[13];
      rollOffFactor = (byte)parameters[14];
      longitude = (short)parameters[15];
      cellId = (ushort)parameters[16];
      cellIdExtension = (byte)parameters[17];
      plpId = (byte)parameters[18];
      return result;
    }

    #endregion

    #region IGrabberSiMpeg delegation

    public bool IsReadyPat()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiMpeg("IsReadyPat", ref parameters);
    }

    public bool IsReadyCat()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiMpeg("IsReadyCat", ref parameters);
    }

    public bool IsReadyPmt()
    {
      object[] parameters = null;
      return (bool)_delegateGrabberSiMpeg("IsReadyPmt", ref parameters);
    }

    public void GetTransportStreamDetail(out ushort transportStreamId,
                                          out ushort networkPid,
                                          out ushort programCount)
    {
      transportStreamId = 0;
      networkPid = 0;
      programCount = 0;
      object[] parameters = new object[3] { transportStreamId, networkPid, programCount };
      _delegateGrabberSiMpeg("GetTransportStreamDetail", ref parameters);
      transportStreamId = (ushort)parameters[0];
      networkPid = (ushort)parameters[1];
      programCount = (ushort)parameters[2];
    }

    public bool GetProgramByIndex(uint index,
                                  out ushort programNumber,
                                  out ushort pmtPid,
                                  out bool isPmtReceived,
                                  out ushort streamCountVideo,
                                  out ushort streamCountAudio,
                                  out bool isEncrypted,
                                  out bool isEncryptionDetectionAccurate,
                                  out bool isThreeDimensional,
                                  Iso639Code[] audioLanguages,
                                  ref byte audioLanguageCount,
                                  Iso639Code[] subtitlesLanguages,
                                  ref byte subtitlesLanguageCount)
    {
      programNumber = 0;
      pmtPid = 0;
      isPmtReceived = false;
      streamCountVideo = 0;
      streamCountAudio = 0;
      isEncrypted = false;
      isEncryptionDetectionAccurate = false;
      isThreeDimensional = false;
      object[] parameters = new object[13]
      {
        index,
        programNumber,
        pmtPid,
        isPmtReceived,
        streamCountVideo,
        streamCountAudio,
        isEncrypted,
        isEncryptionDetectionAccurate,
        isThreeDimensional,
        audioLanguages,
        audioLanguageCount,
        subtitlesLanguages,
        subtitlesLanguageCount
      };
      bool result = (bool)_delegateGrabberSiMpeg("GetProgramByIndex", ref parameters);
      programNumber = (ushort)parameters[1];
      pmtPid = (ushort)parameters[2];
      isPmtReceived = (bool)parameters[3];
      streamCountVideo = (ushort)parameters[4];
      streamCountAudio = (ushort)parameters[5];
      isEncrypted = (bool)parameters[6];
      isEncryptionDetectionAccurate = (bool)parameters[7];
      isThreeDimensional = (bool)parameters[8];
      audioLanguageCount = (byte)parameters[10];
      subtitlesLanguageCount = (byte)parameters[12];
      return result;
    }

    public bool GetProgramByNumber(ushort programNumber,
                                    out ushort pmtPid,
                                    out bool isPmtReceived,
                                    out ushort streamCountVideo,
                                    out ushort streamCountAudio,
                                    out bool isEncrypted,
                                    out bool isEncryptionDetectionAccurate,
                                    out bool isThreeDimensional,
                                    Iso639Code[] audioLanguages,
                                    ref byte audioLanguageCount,
                                    Iso639Code[] subtitlesLanguages,
                                    ref byte subtitlesLanguageCount)
    {
      pmtPid = 0;
      isPmtReceived = false;
      streamCountVideo = 0;
      streamCountAudio = 0;
      isEncrypted = false;
      isEncryptionDetectionAccurate = false;
      isThreeDimensional = false;
      object[] parameters = new object[12]
      {
        programNumber,
        pmtPid,
        isPmtReceived,
        streamCountVideo,
        streamCountAudio,
        isEncrypted,
        isEncryptionDetectionAccurate,
        isThreeDimensional,
        audioLanguages,
        audioLanguageCount,
        subtitlesLanguages,
        subtitlesLanguageCount
      };
      bool result = (bool)_delegateGrabberSiMpeg("GetProgramByNumber", ref parameters);
      pmtPid = (ushort)parameters[1];
      isPmtReceived = (bool)parameters[2];
      streamCountVideo = (ushort)parameters[3];
      streamCountAudio = (ushort)parameters[4];
      isEncrypted = (bool)parameters[5];
      isEncryptionDetectionAccurate = (bool)parameters[6];
      isThreeDimensional = (bool)parameters[7];
      audioLanguageCount = (byte)parameters[9];
      subtitlesLanguageCount = (byte)parameters[11];
      return result;
    }

    public bool GetCat(byte[] table, ref ushort tableBufferSize)
    {
      object[] parameters = new object[2] { table, tableBufferSize };
      bool result = (bool)_delegateGrabberSiMpeg("GetCat", ref parameters);
      tableBufferSize = (ushort)parameters[1];
      return result;
    }

    public bool GetPmt(ushort programNumber, byte[] table, ref ushort tableBufferSize)
    {
      object[] parameters = new object[3] { programNumber, table, tableBufferSize };
      bool result = (bool)_delegateGrabberSiMpeg("GetPmt", ref parameters);
      tableBufferSize = (ushort)parameters[2];
      return result;
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

    #region ICallBackGrabber members

    public void OnTableSeen(ushort pid, byte tableId)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(ICallBackGrabber);
      job.MethodName = "OnTableSeen";
      job.TargetInstance = _callBackGrabber;
      job.Parameters = new object[2] { pid, tableId };
      StartCallBackThread(job);
    }

    public void OnTableComplete(ushort pid, byte tableId)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(ICallBackGrabber);
      job.MethodName = "OnTableComplete";
      job.TargetInstance = _callBackGrabber;
      job.Parameters = new object[2] { pid, tableId };
      StartCallBackThread(job);
    }

    public void OnTableChange(ushort pid, byte tableId)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(ICallBackGrabber);
      job.MethodName = "OnTableChange";
      job.TargetInstance = _callBackGrabber;
      job.Parameters = new object[2] { pid, tableId };
      StartCallBackThread(job);
    }

    #endregion

    #region IObserver members

    public void OnProgramAssociationTable(ushort transportStreamId, ushort networkPid, ushort programCount)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IObserver);
      job.MethodName = "OnProgramAssociationTable";
      job.TargetInstance = _observer;
      job.Parameters = new object[3] { transportStreamId, networkPid, programCount };
      StartCallBackThread(job);
    }

    public void OnConditionalAccessTable(byte[] cat, ushort catBufferSize)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IObserver);
      job.MethodName = "OnConditionalAccessTable";
      job.TargetInstance = _observer;
      job.Parameters = new object[2] { cat, catBufferSize };
      StartCallBackThread(job);
    }

    public void OnProgramDetail(ushort programNumber,
                                ushort pmtPid,
                                bool isRunning,
                                byte[] pmt,
                                ushort pmtBufferSize)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IObserver);
      job.MethodName = "OnProgramDetail";
      job.TargetInstance = _observer;
      job.Parameters = new object[5] { programNumber, pmtPid, isRunning, pmt, pmtBufferSize };
      StartCallBackThread(job);
    }

    public void OnPidEncryptionStateChange(ushort pid, EncryptionState state)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IObserver);
      job.MethodName = "OnPidEncryptionStateChange";
      job.TargetInstance = _observer;
      job.Parameters = new object[2] { pid, state };
      StartCallBackThread(job);
    }

    public void OnPidsRequired(ushort[] pids, byte pidCount, PidUsage usage)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IObserver);
      job.MethodName = "OnPidsRequired";
      job.TargetInstance = _observer;
      job.Parameters = new object[3] { pids, pidCount, usage };
      StartCallBackThread(job);
    }

    public void OnPidsNotRequired(ushort[] pids, byte pidCount, PidUsage usage)
    {
      CallBackJob job = new CallBackJob();
      job.TargetType = typeof(IObserver);
      job.MethodName = "OnPidsNotRequired";
      job.TargetInstance = _observer;
      job.Parameters = new object[3] { pids, pidCount, usage };
      StartCallBackThread(job);
    }

    #endregion

    #endregion
  }
}