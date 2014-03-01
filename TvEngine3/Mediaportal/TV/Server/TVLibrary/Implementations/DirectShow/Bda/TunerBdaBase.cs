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
using System.IO;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for tuners with BDA
  /// drivers.
  /// </summary>
  public abstract class TunerBdaBase : TunerDirectShowBase
  {
    #region variables

    /// <summary>
    /// The network provider filter.
    /// </summary>
    private IBaseFilter _filterNetworkProvider = null;

    /// <summary>
    /// The [optional] BDA capture/receiver filter.
    /// </summary>
    private IBaseFilter _filterCapture = null;

    /// <summary>
    /// The [optional] BDA capture/receiver device.
    /// </summary>
    private DsDevice _deviceCapture = null;

    #region compatibility filters

    /// <summary>
    /// Enable or disable use of the MPEG 2 demultiplexer and BDA TIF. Compatibility with some tuners
    /// requires these filters to be connected into the graph.
    /// </summary>
    private bool _addCompatibilityFilters = false;

    /// <summary>
    /// An infinite tee filter, used to fork the stream to the demultiplexer and TS writer/analyser.
    /// </summary>
    private IBaseFilter _filterInfiniteTee = null;

    /// <summary>
    /// The Microsoft MPEG 2 demultiplexer filter.
    /// </summary>
    private IBaseFilter _filterMpeg2Demultiplexer = null;

    /// <summary>
    /// The Microsoft transport information filter.
    /// </summary>
    private IBaseFilter _filterTransportInformation = null;

    #endregion

    /// <summary>
    /// The configured network provider class identifier.
    /// </summary>
    private Guid _networkProviderClsid = typeof(NetworkProvider).GUID;

    /// <summary>
    /// A pre-configured tuning space, used to speed up the tuning process. 
    /// </summary>
    private ITuningSpace _tuningSpace = null;

    /// <summary>
    /// Tuner and demodulator signal statistic interfaces.
    /// </summary>
    private IList<IBDA_SignalStatistics> _signalStatisticsInterfaces = new List<IBDA_SignalStatistics>();

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaBase"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="externalId">The external identifier for the tuner.</param>
    protected TunerBdaBase(DsDevice device, string externalId)
      : base(device.Name, externalId)
    {
      _deviceMain = device;
    }

    #endregion

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();

      bool save = false;
      Card tuner = CardManagement.GetCard(_cardId, CardIncludeRelationEnum.None);
      _networkProviderClsid = Guid.Empty; // specific network provider
      if (tuner.NetProvider == (int)DbNetworkProvider.MediaPortal)
      {
        if (!File.Exists(PathManager.BuildAssemblyRelativePath("NetworkProvider.ax")))
        {
          this.LogWarn("BDA base: MediaPortal network provider is not available, try Microsoft generic network provider");
          tuner.NetProvider = (int)DbNetworkProvider.Generic;
          save = true;
        }
        else
        {
          _networkProviderClsid = typeof(MediaPortalNetworkProvider).GUID;
        }
      }
      if (tuner.NetProvider == (int)DbNetworkProvider.Generic)
      {
        if (!FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID))
        {
          this.LogWarn("BDA base: MediaPortal network provider is not available, try Microsoft specific network provider");
          tuner.NetProvider = (int)DbNetworkProvider.Specific;
          save = true;
        }
        else
        {
          _networkProviderClsid = typeof(NetworkProvider).GUID;
        }
      }
      if (save)
      {
        CardManagement.SaveCard(tuner);
      }
    }

    #region tuning & scanning

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the subchannel associated with the tuned channel</returns>
    public override ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      ITvSubChannel subChannel = base.Tune(subChannelId, channel);
      if (_filterTransportInformation != null)
      {
        _filterTransportInformation.Stop();
      }
      return subChannel;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      int hr = (int)HResult.Severity.Success;
      if (_networkProviderClsid == typeof(MediaPortalNetworkProvider).GUID)
      {
        this.LogDebug("BDA base: perform tuning, MediaPortal network provider tuning");
        IDvbNetworkProvider networkProviderInterface = _filterNetworkProvider as IDvbNetworkProvider;
        if (networkProviderInterface == null)
        {
          throw new TvException("Failed to find MediaPortal network provider interface on network provider.");
        }
        hr = PerformMediaPortalNetworkProviderTuning(networkProviderInterface, channel);
      }
      else
      {
        this.LogDebug("BDA base: perform tuning, standard BDA tuning");
        ITuneRequest tuneRequest = AssembleTuneRequest(_tuningSpace, channel);
        this.LogDebug("BDA base: apply tuning parameters");
        hr = (_filterNetworkProvider as ITuner).put_TuneRequest(tuneRequest);
        this.LogDebug("BDA base: parameters applied, hr = 0x{0:x}", hr);
        Release.ComObject("base BDA tuner tune request", ref tuneRequest);
      }

      // TerraTec tuners return a positive HRESULT value when already tuned with the required
      // parameters. See mantis 3469 for more details.
      if (hr < (int)HResult.Severity.Success)
      {
        HResult.ThrowException(hr, "Failed to tune channel.");
      }
    }

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="tuningSpace">The tuner's tuning space.</param>
    /// <param name="channel">The channel to translate into a tune request.</param>
    /// <returns>a tune request instance</returns>
    protected abstract ITuneRequest AssembleTuneRequest(ITuningSpace tuningSpace, IChannel channel);

    /// <summary>
    /// Tune using the MediaPortal network provider.
    /// </summary>
    /// <param name="networkProvider">The network provider interface to use for tuning.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>an HRESULT indicating whether the tuning parameters were applied successfully</returns>
    protected abstract int PerformMediaPortalNetworkProviderTuning(IDvbNetworkProvider networkProvider, IChannel channel);

    #endregion

    #region graph building

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("BDA base: perform loading");
      InitialiseGraph();
      AddNetworkProviderFilterToGraph();

      int hr = (int)HResult.Severity.Success;
      if (_networkProviderClsid != typeof(MediaPortalNetworkProvider).GUID)
      {
        // Some Microsoft network providers won't connect to the tuner filter
        // unless you set a tuning space.
        _tuningSpace = GetTuningSpace();
        if (_tuningSpace == null)
        {
          _tuningSpace = CreateTuningSpace();
        }
        ITuner tuner = _filterNetworkProvider as ITuner;
        if (tuner == null)
        {
          throw new TvException("Failed to find tuner interface on network provider.");
        }
        hr = tuner.put_TuningSpace(_tuningSpace);
        HResult.ThrowException(hr, "Failed to apply tuning space on tuner.");
      }

      AddMainComponentFilterToGraph();
      hr = _captureGraphBuilder.RenderStream(null, null, _filterNetworkProvider, null, _filterMain);
      HResult.ThrowException(hr, "Failed to render from the network provider into the tuner filter.");

      IBaseFilter lastFilter = _filterMain;
      AddAndConnectCaptureFilterIntoGraph(ref lastFilter);

      // Check for and load extensions, adding any additional filters to the graph.
      LoadPlugins(_filterMain, _graph, ref lastFilter);

      // If using a Microsoft network provider and configured to do so, add an infinite
      // tee, MPEG 2 demultiplexer and transport information filter.
      if (_networkProviderClsid != typeof(MediaPortalNetworkProvider).GUID && _addCompatibilityFilters)
      {
        _filterInfiniteTee = (IBaseFilter)new InfTee();
        FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterInfiniteTee, "Infinite Tee", lastFilter, _captureGraphBuilder);
        lastFilter = _filterInfiniteTee;
        _filterMpeg2Demultiplexer = (IBaseFilter)new MPEG2Demultiplexer();
        FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterMpeg2Demultiplexer, "MPEG 2 Demultiplexer", _filterInfiniteTee, _captureGraphBuilder);
        AddAndConnectTransportInformationFilterIntoGraph();
      }

      // Complete the graph.
      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();

      _signalStatisticsInterfaces = GetTunerSignalStatisticsInterfaces();
    }

    /// <summary>
    /// Add the appropriate BDA network provider filter to the graph.
    /// </summary>
    protected void AddNetworkProviderFilterToGraph()
    {
      this.LogDebug("BDA base: add network provider filter");

      string filterName = string.Empty;
      if (_networkProviderClsid == typeof(MediaPortalNetworkProvider).GUID)
      {
        filterName = "MediaPortal Network Provider";
      }
      else if (_networkProviderClsid == typeof(NetworkProvider).GUID)
      {
        filterName = "Generic Network Provider";
      }
      else
      {
        filterName = "Specific Network Provider";
        _networkProviderClsid = NetworkProviderClsid;
      }
      this.LogDebug("BDA base: using {0}", filterName);
      if (_networkProviderClsid == typeof(MediaPortalNetworkProvider).GUID)
      {
        _filterNetworkProvider = FilterGraphTools.AddFilterFromFile(_graph, "NetworkProvider.ax", _networkProviderClsid, filterName);
        IDvbNetworkProvider internalNpInterface = _filterNetworkProvider as IDvbNetworkProvider;
        internalNpInterface.ConfigureLogging(MediaPortalNetworkProvider.GetFileName(ExternalId), MediaPortalNetworkProvider.GetHash(ExternalId), LogLevelOption.Debug);
      }
      else
      {
        _filterNetworkProvider = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, _networkProviderClsid, filterName);
      }
    }

    /// <summary>
    /// Get the class ID of the network provider for the tuner type.
    /// </summary>
    protected abstract Guid NetworkProviderClsid
    {
      get;
    }

    /// <summary>
    /// Get the registered name of the BDA tuning space for the tuner type.
    /// </summary>
    protected abstract string TuningSpaceName
    {
      get;
    }

    /// <summary>
    /// Get the registered BDA tuning space for the tuner type if it exists.
    /// </summary>
    /// <returns>the registed tuning space</returns>
    private ITuningSpace GetTuningSpace()
    {
      this.LogDebug("BDA base: get tuning space");

      // We're going to enumerate the tuning spaces registered in the system to
      // see if we can find the correct MediaPortal tuning space.
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      try
      {
        ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
        if (container == null)
        {
          throw new TvException("Failed to find tuning space container interface on system tuning spaces instance.");
        }

        IEnumTuningSpaces enumTuningSpaces;
        int hr = container.get_EnumTuningSpaces(out enumTuningSpaces);
        HResult.ThrowException(hr, "Failed to get tuning space enumerator from tuning space container.");
        try
        {
          // Enumerate...
          ITuningSpace[] spaces = new ITuningSpace[2];
          int fetched;
          while (enumTuningSpaces.Next(1, spaces, out fetched) == 0 && fetched == 1)
          {
            try
            {
              // Is this the one we're looking for?
              string name;
              hr = spaces[0].get_UniqueName(out name);
              HResult.ThrowException(hr, "Failed to get tuning space unique name.");
              if (name.Equals(TuningSpaceName))
              {
                this.LogDebug("BDA base: found correct tuning space");
                return spaces[0];
              }
              else
              {
                Release.ComObject("base BDA tuner tuning space", ref spaces[0]);
              }
            }
            catch (Exception)
            {
              Release.ComObject("base BDA tuner tuning space", ref spaces[0]);
              throw;
            }
          }
          return null;
        }
        finally
        {
          Release.ComObject("base BDA tuner tuning space enumerator", ref enumTuningSpaces);
        }
      }
      finally
      {
        Release.ComObject("base BDA tuner tuning space container", ref systemTuningSpaces);
      }
    }

    /// <summary>
    /// Create and register a BDA tuning space for the tuner type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected abstract ITuningSpace CreateTuningSpace();

    /// <summary>
    /// Add and connect the appropriate BDA capture/receiver filter into the graph.
    /// </summary>
    /// <param name="lastFilter">The upstream filter to connect the capture filter to.</param>
    protected virtual void AddAndConnectCaptureFilterIntoGraph(ref IBaseFilter lastFilter)
    {
      this.LogDebug("BDA base: add capture filter");

      bool isCaptureFilterRequired = false;
      IPin pin = DsFindPin.ByDirection(_filterMain, PinDirection.Output, 0);
      try
      {
        IKsPin ksPin = pin as IKsPin;
        if (ksPin != null)
        {
          IntPtr ksMultiple = IntPtr.Zero;
          int hr = ksPin.KsQueryMediums(out ksMultiple);
          try
          {
            if (hr == (int)HResult.Severity.Success)
            {
              int mediumCount = Marshal.ReadInt32(ksMultiple, sizeof(int));
              IntPtr mediumPtr = IntPtr.Add(ksMultiple, 8);
              int regPinMediumSize = Marshal.SizeOf(typeof(RegPinMedium));
              for (int i = 0; i < mediumCount; i++)
              {
                RegPinMedium rpm = (RegPinMedium)Marshal.PtrToStructure(mediumPtr, typeof(RegPinMedium));
                if (rpm.clsMedium != Guid.Empty && rpm.clsMedium != MediaPortalGuid.KS_MEDIUM_SET_ID_STANDARD)
                {
                  this.LogDebug("BDA base: capture filter required");
                  isCaptureFilterRequired = true;
                  break;
                }
                mediumPtr = IntPtr.Add(mediumPtr, regPinMediumSize);
              }
            }
          }
          finally
          {
            if (ksMultiple != IntPtr.Zero)
            {
              Marshal.FreeCoTaskMem(ksMultiple);
            }
          }
        }
      }
      finally
      {
        Release.ComObject("base BDA tuner output pin", ref pin);
      }
      if (!isCaptureFilterRequired)
      {
        this.LogDebug("BDA base: capture filter not required");
        return;
      }

      bool matchProductInstanceId = true;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
      try
      {
        while (true)
        {
          for (int i = 0; i < devices.Length; i++)
          {
            DsDevice device = devices[i];
            if (!DevicesInUse.Instance.Add(device))
            {
              continue;
            }
            string devicePath = device.DevicePath;
            string deviceName = device.Name;
            if (devicePath.Contains("root#system#") ||
              (matchProductInstanceId && _productInstanceId != null && _productInstanceId.Equals(device.ProductInstanceIdentifier))
            )
            {
              continue;
            }
            this.LogDebug("BDA base: try {0} {1}", deviceName, devicePath);
            try
            {
              // The filter needs a unique name.
              if (deviceName.Equals(_deviceMain.Name))
              {
                deviceName += " Capture";
              }
              _filterCapture = FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, device, _filterMain, _captureGraphBuilder, deviceName);
              this.LogDebug("BDA base: connected!");
              _deviceCapture = device;
              lastFilter = _filterCapture;
              return;
            }
            catch
            {
              DevicesInUse.Instance.Remove(device);
            }
          }
          if (!matchProductInstanceId)
          {
            this.LogWarn("BDA base: failed to add and connect capture filter, assuming extension required to complete graph");
            break;
          }
          matchProductInstanceId = false;
          this.LogDebug("BDA base: allow non-matching capture components");
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          if (d != _deviceCapture)
          {
            d.Dispose();
          }
        }
      }
    }

    /// <summary>
    /// Add and connect a transport information filter into the graph.
    /// </summary>
    private void AddAndConnectTransportInformationFilterIntoGraph()
    {
      this.LogDebug("BDA base: add BDA MPEG 2 TIF filter");
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
      try
      {
        for (int i = 0; i < devices.Length; i++)
        {
          DsDevice device = devices[i];
          if (device.Name == null || !device.Name.Equals("BDA MPEG2 Transport Information Filter"))
          {
            continue;
          }
          _filterTransportInformation = FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, device, _filterMpeg2Demultiplexer, _captureGraphBuilder);
          return;
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          d.Dispose();
        }
      }
      this.LogWarn("BDA base: transport information filter not found");
    }

    /// <summary>
    /// Get the signal statistic interfaces from the tuner.
    /// </summary>
    /// <returns>the signal statistic interfaces</returns>
    private IList<IBDA_SignalStatistics> GetTunerSignalStatisticsInterfaces()
    {
      this.LogDebug("BDA base: get tuner signal statistics interfaces");

      IBDA_Topology topology = _filterMain as IBDA_Topology;
      if (topology == null)
      {
        throw new TvException("Failed to find BDA topology interface on main filter.");
      }

      int nodeTypeCount;
      int[] nodeTypes = new int[33];
      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      HResult.ThrowException(hr, "Failed to get topology node types.");

      IList<IBDA_SignalStatistics> statistics = new List<IBDA_SignalStatistics>();
      Guid[] guidInterfaces = new Guid[33];
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        object controlNode;
        hr = topology.GetControlNode(0, 1, nodeTypes[i], out controlNode);
        HResult.ThrowException(hr, "Failed to get topology control node.");

        IBDA_SignalStatistics s = controlNode as IBDA_SignalStatistics;
        if (s != null)
        {
          statistics.Add(s);
        }
      }
      if (statistics.Count == 0)
      {
        throw new TvException("Failed to find signal statistic interfaces.");
      }
      this.LogDebug("BDA base: found {0} interfaces", statistics.Count);
      return statistics;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    protected override void PerformUnloading()
    {
      this.LogDebug("BDA base: perform unloading");
      if (_graph != null)
      {
        _graph.RemoveFilter(_filterNetworkProvider);
        _graph.RemoveFilter(_filterInfiniteTee);
        _graph.RemoveFilter(_filterMpeg2Demultiplexer);
        _graph.RemoveFilter(_filterTransportInformation);
      }
      Release.ComObject("base BDA tuner network provider", ref _filterNetworkProvider);
      Release.ComObject("base BDA tuner infinite tee", ref _filterInfiniteTee);
      Release.ComObject("base BDA tuner MPEG 2 demultiplexer", ref _filterMpeg2Demultiplexer);
      Release.ComObject("base BDA tuner transport information filter", ref _filterTransportInformation);
      Release.ComObject("base BDA tuner tuning space", ref _tuningSpace);

      if (_filterCapture != null)
      {
        if (_graph != null)
        {
          _graph.RemoveFilter(_filterCapture);
        }
        Release.ComObject("base BDA tuner capture filter", ref _filterCapture);

        DevicesInUse.Instance.Remove(_deviceCapture);
        _deviceCapture.Dispose();
        _deviceCapture = null;
      }
      if (_signalStatisticsInterfaces != null)
      {
        for (int i = 0; i < _signalStatisticsInterfaces.Count; i++)
        {
          IBDA_SignalStatistics s = _signalStatisticsInterfaces[i];
          Release.ComObject(string.Format("base BDA tuner signal statistics interface {0}", i), ref s);
        }
        _signalStatisticsInterfaces.Clear();
      }

      CleanUpGraph();
    }

    #endregion

    #region signal quality, level etc

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected override void PerformSignalStatusUpdate(bool onlyUpdateLock)
    {
      if (_signalStatisticsInterfaces == null || _signalStatisticsInterfaces.Count == 0)
      {
        _isSignalPresent = false;
        _isSignalLocked = false;
        _signalLevel = 0;
        _signalQuality = 0;
        return;
      }

      bool finalIsPresent = false;
      bool finalIsLocked = false;
      int finalStrength = 0;
      int strengthCount = 0;
      int finalQuality = 0;
      int qualityCount = 0;
      for (int i = 0; i < _signalStatisticsInterfaces.Count; i++)
      {
        int hr = (int)HResult.Severity.Success;
        IBDA_SignalStatistics statisticsInterface = _signalStatisticsInterfaces[i];
        try
        {
          bool isLocked = false;
          hr = statisticsInterface.get_SignalLocked(out isLocked);
          if (hr == (int)HResult.Severity.Success)
          {
            finalIsLocked |= isLocked;
          }
          if (onlyUpdateLock)
          {
            continue;
          }

          bool isPresent = false;
          hr = statisticsInterface.get_SignalPresent(out isPresent);
          if (hr == (int)HResult.Severity.Success)
          {
            finalIsPresent |= isPresent;
          }

          int quality = 0;
          hr = statisticsInterface.get_SignalQuality(out quality);
          if (hr == (int)HResult.Severity.Success && quality != 0)
          {
            finalQuality += quality;
            qualityCount++;
          }

          int strength = 0;
          hr = statisticsInterface.get_SignalStrength(out strength);
          if (hr == (int)HResult.Severity.Success && strength != 0)
          {
            finalStrength += strength;
            strengthCount++;
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "BDA base: exception updating signal status with interface {0}", i);
        }
        finally
        {
          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("BDA base: potential error updating signal status with interface {0}, hr = 0x{1:x}", i, hr);
          }
        }
      }

      _isSignalLocked = finalIsLocked;
      if (!onlyUpdateLock)
      {
        _isSignalPresent = finalIsPresent;
        _signalLevel = finalStrength;
        if (strengthCount > 1)
        {
          _signalLevel /= strengthCount;
        }
        _signalQuality = finalQuality;
        if (qualityCount > 1)
        {
          _signalQuality /= qualityCount;
        }
      }
    }

    #endregion

    #region Channel linkage handling

    private static bool SameAsPortalChannel(PortalChannel pChannel, LinkedChannel lChannel)
    {
      return ((pChannel.NetworkId == lChannel.NetworkId) && (pChannel.TransportId == lChannel.TransportId) &&
              (pChannel.ServiceId == lChannel.ServiceId));
    }

    private static bool IsNewLinkedChannel(PortalChannel pChannel, LinkedChannel lChannel)
    {
      bool bRet = true;
      foreach (LinkedChannel lchan in pChannel.LinkedChannels)
      {
        if ((lchan.NetworkId == lChannel.NetworkId) && (lchan.TransportId == lChannel.TransportId) &&
            (lchan.ServiceId == lChannel.ServiceId))
        {
          bRet = false;
          break;
        }
      }
      return bRet;
    }

    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    public override void StartLinkageScanner(BaseChannelLinkageScanner callBack)
    {
      ITsChannelLinkageScanner linkageScanner = _filterTsWriter as ITsChannelLinkageScanner;
      linkageScanner.SetCallBack(callBack);
      linkageScanner.Start();
    }

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public override void ResetLinkageScanner()
    {
      (_filterTsWriter as ITsChannelLinkageScanner).Reset();
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public override List<PortalChannel> ChannelLinkages
    {
      get
      {
        ITsChannelLinkageScanner linkageScanner = _filterTsWriter as ITsChannelLinkageScanner;
        try
        {
          uint channelCount;
          List<PortalChannel> portalChannels = new List<PortalChannel>();
          linkageScanner.GetChannelCount(out channelCount);
          if (channelCount == 0)
            return portalChannels;
          for (uint i = 0; i < channelCount; i++)
          {
            ushort network_id = 0;
            ushort transport_id = 0;
            ushort service_id = 0;
            linkageScanner.GetChannel(i, ref network_id, ref transport_id, ref service_id);
            PortalChannel pChannel = new PortalChannel();
            pChannel.NetworkId = network_id;
            pChannel.TransportId = transport_id;
            pChannel.ServiceId = service_id;
            uint linkCount;
            linkageScanner.GetLinkedChannelsCount(i, out linkCount);
            if (linkCount > 0)
            {
              for (uint j = 0; j < linkCount; j++)
              {
                ushort nid = 0;
                ushort tid = 0;
                ushort sid = 0;
                IntPtr ptrName;
                linkageScanner.GetLinkedChannel(i, j, ref nid, ref tid, ref sid, out ptrName);
                LinkedChannel lChannel = new LinkedChannel();
                lChannel.NetworkId = nid;
                lChannel.TransportId = tid;
                lChannel.ServiceId = sid;
                lChannel.Name = DvbTextConverter.Convert(ptrName);
                if ((!SameAsPortalChannel(pChannel, lChannel)) && (IsNewLinkedChannel(pChannel, lChannel)))
                  pChannel.LinkedChannels.Add(lChannel);
              }
            }
            if (pChannel.LinkedChannels.Count > 0)
              portalChannels.Add(pChannel);
          }
          linkageScanner.Reset();
          return portalChannels;
        }
        catch (Exception ex)
        {
          this.LogError(ex);
          return new List<PortalChannel>();
        }
      }
    }

    #endregion

    #region EPG

    /// <summary>
    /// Start grabbing the epg
    /// </summary>
    public override void GrabEpg(BaseEpgGrabber callBack)
    {
      _epgGrabberCallBack = callBack;
      this.LogDebug("dvb:grab epg...");
      ITsEpgScanner epgGrabber = _filterTsWriter as ITsEpgScanner;
      if (epgGrabber == null)
      {
        return;
      }
      epgGrabber.SetCallBack(callBack);
      epgGrabber.GrabEPG();
      epgGrabber.GrabMHW();
      _epgGrabbing = true;
    }

    /// <summary>
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public override void GrabEpg()
    {
      if (_timeshiftingEpgGrabber.StartGrab())
        GrabEpg(_timeshiftingEpgGrabber);
    }

    /// <summary>
    /// Activates / deactivates the epg grabber
    /// </summary>
    /// <param name="value">Mode</param>
    protected override void UpdateEpgGrabber(bool value)
    {
      if (_epgGrabbing && value == false)
      {
        if (_epgGrabberCallBack != null)
        {
          _epgGrabberCallBack.OnEpgCancelled();
        }
        (_filterTsWriter as ITsEpgScanner).Reset();
      }
      _epgGrabbing = value;
    }

    /// <summary>
    /// Gets the UTC.
    /// </summary>
    /// <param name="val">The val.</param>
    /// <returns></returns>
    private static int getUTC(int val)
    {
      if ((val & 0xF0) >= 0xA0)
        return 0;
      if ((val & 0xF) >= 0xA)
        return 0;
      return ((val & 0xF0) >> 4) * 10 + (val & 0xF);
    }

    /// <summary>
    /// Aborts grabbing the epg
    /// </summary>
    public override void AbortGrabbing()
    {
      this.LogDebug("dvb:abort grabbing epg");
      ITsEpgScanner epgGrabber = _filterTsWriter as ITsEpgScanner;
      if (epgGrabber != null)
      {
        epgGrabber.AbortGrabbing();
      }
      if (_timeshiftingEpgGrabber != null)
      {
        _timeshiftingEpgGrabber.OnEpgCancelled();
      }
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public override List<EpgChannel> Epg
    {
      get
      {
        try
        {
          ITsEpgScanner epgGrabber = _filterTsWriter as ITsEpgScanner;
          bool dvbReady, mhwReady;
          epgGrabber.IsEPGReady(out dvbReady);
          epgGrabber.IsMHWReady(out mhwReady);
          if (dvbReady == false || mhwReady == false)
            return null;
          uint titleCount;
          uint channelCount;
          epgGrabber.GetMHWTitleCount(out titleCount);
          mhwReady = titleCount > 10;
          epgGrabber.GetEPGChannelCount(out channelCount);
          dvbReady = channelCount > 0;
          List<EpgChannel> epgChannels = new List<EpgChannel>();
          this.LogInfo("dvb:mhw ready MHW {0} titles found", titleCount);
          this.LogInfo("dvb:dvb ready.EPG {0} channels", channelCount);
          if (mhwReady)
          {
            epgGrabber.GetMHWTitleCount(out titleCount);
            for (int i = 0; i < titleCount; ++i)
            {
              uint id = 0;
              uint programid = 0;
              uint transportid = 0, networkid = 0, channelnr = 0, channelid = 0, themeid = 0, PPV = 0, duration = 0;
              byte summaries = 0;
              uint datestart = 0, timestart = 0;
              uint tmp1 = 0, tmp2 = 0;
              IntPtr ptrTitle, ptrProgramName;
              IntPtr ptrChannelName, ptrSummary, ptrTheme;
              epgGrabber.GetMHWTitle((ushort)i, ref id, ref tmp1, ref tmp2, ref channelnr, ref programid,
                                               ref themeid, ref PPV, ref summaries, ref duration, ref datestart,
                                               ref timestart, out ptrTitle, out ptrProgramName);
              epgGrabber.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid,
                                                 out ptrChannelName);
              epgGrabber.GetMHWSummary(programid, out ptrSummary);
              epgGrabber.GetMHWTheme(themeid, out ptrTheme);
              string channelName = DvbTextConverter.Convert(ptrChannelName);
              string title = DvbTextConverter.Convert(ptrTitle);
              string summary = DvbTextConverter.Convert(ptrSummary);
              string theme = DvbTextConverter.Convert(ptrTheme);
              if (channelName == null)
                channelName = string.Empty;
              if (title == null)
                title = string.Empty;
              if (summary == null)
                summary = string.Empty;
              if (theme == null)
                theme = string.Empty;
              channelName = channelName.Trim();
              title = title.Trim();
              summary = summary.Trim();
              theme = theme.Trim();
              EpgChannel epgChannel = null;
              foreach (EpgChannel chan in epgChannels)
              {
                DVBBaseChannel dvbChan = (DVBBaseChannel)chan.Channel;
                if (dvbChan.NetworkId == networkid && dvbChan.TransportId == transportid &&
                    dvbChan.ServiceId == channelid)
                {
                  epgChannel = chan;
                  break;
                }
              }
              if (epgChannel == null)
              {
                // We need to use a matching channel type per card, because tuning details will be looked up with cardtype as filter.
                DVBBaseChannel channel = (DVBBaseChannel)_currentTuningDetail.Clone();
                channel.NetworkId = (int)networkid;
                channel.TransportId = (int)transportid;
                channel.ServiceId = (int)channelid;
                epgChannel = new EpgChannel { Channel = channel };
                //this.LogInfo("dvb: start filtering channel NID {0} TID {1} SID{2}", dvbChan.NetworkId, dvbChan.TransportId, dvbChan.ServiceId);
                if (FilterOutEPGChannel((ushort)networkid, (ushort)transportid, (ushort)channelid) == false)
                {
                  //this.LogInfo("dvb: Not Filtered channel NID {0} TID {1} SID{2}", dvbChan.NetworkId, dvbChan.TransportId, dvbChan.ServiceId);
                  epgChannels.Add(epgChannel);
                }
              }
              uint d1 = datestart;
              uint m = timestart & 0xff;
              uint h1 = (timestart >> 16) & 0xff;
              DateTime dayStart = DateTime.Now;
              dayStart =
                dayStart.Subtract(new TimeSpan(1, dayStart.Hour, dayStart.Minute, dayStart.Second, dayStart.Millisecond));
              int day = (int)dayStart.DayOfWeek;
              DateTime programStartTime = dayStart;
              int minVal = (int)((d1 - day) * 86400 + h1 * 3600 + m * 60);
              if (minVal < 21600)
                minVal += 604800;
              programStartTime = programStartTime.AddSeconds(minVal);
              EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
              EpgLanguageText epgLang = new EpgLanguageText("ALL", title, summary, theme, 0, string.Empty, -1);
              program.Text.Add(epgLang);
              epgChannel.Programs.Add(program);
            }
            for (int i = 0; i < epgChannels.Count; ++i)
            {
              epgChannels[i].Sort();
            }
            // free the epg infos in TsWriter so that the mem used gets released 
            epgGrabber.Reset();
            return epgChannels;
          }

          if (dvbReady)
          {
            ushort networkid = 0;
            ushort transportid = 0;
            ushort serviceid = 0;
            for (uint x = 0; x < channelCount; ++x)
            {
              epgGrabber.GetEPGChannel(x, ref networkid, ref transportid, ref serviceid);
              // We need to use a matching channel type per card, because tuning details will be looked up with cardtype as filter.
              DVBBaseChannel channel = (DVBBaseChannel)_currentTuningDetail.Clone();
              channel.NetworkId = networkid;
              channel.TransportId = transportid;
              channel.ServiceId = serviceid;
              EpgChannel epgChannel = new EpgChannel { Channel = channel };
              uint eventCount;
              epgGrabber.GetEPGEventCount(x, out eventCount);
              for (uint i = 0; i < eventCount; ++i)
              {
                uint start_time_MJD, start_time_UTC, duration, languageCount;
                string title, description;
                IntPtr ptrGenre;
                int starRating;
                IntPtr ptrClassification;

                epgGrabber.GetEPGEvent(x, i, out languageCount, out start_time_MJD, out start_time_UTC,
                                                 out duration, out ptrGenre, out starRating, out ptrClassification);
                string genre = DvbTextConverter.Convert(ptrGenre);
                string classification = DvbTextConverter.Convert(ptrClassification);

                if (starRating < 1 || starRating > 7)
                  starRating = 0;

                int duration_hh = getUTC((int)((duration >> 16)) & 255);
                int duration_mm = getUTC((int)((duration >> 8)) & 255);
                int duration_ss = 0; //getUTC((int) (duration )& 255);
                int starttime_hh = getUTC((int)((start_time_UTC >> 16)) & 255);
                int starttime_mm = getUTC((int)((start_time_UTC >> 8)) & 255);
                int starttime_ss = 0; //getUTC((int) (start_time_UTC )& 255);

                if (starttime_hh > 23)
                  starttime_hh = 23;
                if (starttime_mm > 59)
                  starttime_mm = 59;
                if (starttime_ss > 59)
                  starttime_ss = 59;

                // DON'T ENABLE THIS. Some entries can be indeed >23 Hours !!!
                //if (duration_hh > 23) duration_hh = 23;
                if (duration_mm > 59)
                  duration_mm = 59;
                if (duration_ss > 59)
                  duration_ss = 59;

                // convert the julian date
                int year = (int)((start_time_MJD - 15078.2) / 365.25);
                int month = (int)((start_time_MJD - 14956.1 - (int)(year * 365.25)) / 30.6001);
                int day = (int)(start_time_MJD - 14956 - (int)(year * 365.25) - (int)(month * 30.6001));
                int k = (month == 14 || month == 15) ? 1 : 0;
                year += 1900 + k; // start from year 1900, so add that here
                month = month - 1 - k * 12;
                int starttime_y = year;
                int starttime_m = month;
                int starttime_d = day;
                if (year < 2000)
                  continue;

                try
                {
                  DateTime dtUTC = new DateTime(starttime_y, starttime_m, starttime_d, starttime_hh, starttime_mm,
                                                starttime_ss, 0);
                  DateTime dtStart = dtUTC.ToLocalTime();
                  if (dtStart < DateTime.Now.AddDays(-1) || dtStart > DateTime.Now.AddMonths(2))
                    continue;
                  DateTime dtEnd = dtStart.AddHours(duration_hh);
                  dtEnd = dtEnd.AddMinutes(duration_mm);
                  dtEnd = dtEnd.AddSeconds(duration_ss);
                  EpgProgram epgProgram = new EpgProgram(dtStart, dtEnd);
                  //EPGEvent newEvent = new EPGEvent(genre, dtStart, dtEnd);
                  for (int z = 0; z < languageCount; ++z)
                  {
                    uint languageId;
                    IntPtr ptrTitle;
                    IntPtr ptrDesc;
                    int parentalRating;
                    epgGrabber.GetEPGLanguage(x, i, (uint)z, out languageId, out ptrTitle, out ptrDesc,
                                                        out parentalRating);
                    string language = string.Empty;
                    language += (char)((languageId >> 16) & 0xff);
                    language += (char)((languageId >> 8) & 0xff);
                    language += (char)((languageId) & 0xff);
                    title = DvbTextConverter.Convert(ptrTitle);
                    description = DvbTextConverter.Convert(ptrDesc);
                    if (title == null)
                      title = string.Empty;
                    if (description == null)
                      description = string.Empty;
                    if (string.IsNullOrEmpty(language))
                      language = string.Empty;
                    if (genre == null)
                      genre = string.Empty;
                    if (classification == null)
                      classification = string.Empty;
                    title = title.Trim();
                    description = description.Trim();
                    language = language.Trim();
                    genre = genre.Trim();
                    EpgLanguageText epgLangague = new EpgLanguageText(language, title, description, genre, starRating,
                                                                      classification, parentalRating);
                    epgProgram.Text.Add(epgLangague);
                  }
                  epgChannel.Programs.Add(epgProgram);
                }
                catch (Exception ex)
                {
                  this.LogError(ex);
                }
              } //for (uint i = 0; i < eventCount; ++i)
              if (epgChannel.Programs.Count > 0)
              {
                epgChannel.Sort();
                //this.LogInfo("dvb: start filtering channel NID {0} TID {1} SID{2}", chan.NetworkId, chan.TransportId, chan.ServiceId);
                if (FilterOutEPGChannel(networkid, transportid, serviceid) == false)
                {
                  //this.LogInfo("dvb: Not Filtered channel NID {0} TID {1} SID{2}", chan.NetworkId, chan.TransportId, chan.ServiceId);
                  epgChannels.Add(epgChannel);
                }
              }
            } //for (uint x = 0; x < channelCount; ++x)
          }
          // free the epg infos in TsWriter so that the mem used gets released 
          epgGrabber.Reset();
          return epgChannels;
        }
        catch (Exception ex)
        {
          this.LogError(ex);
          return new List<EpgChannel>();
        }
      }
    }

    /// <summary>
    /// Check if the EPG data found in a scan should not be kept.
    /// </summary>
    /// <remarks>
    /// This function implements the logic to filter out data for services that are not on the same transponder.
    /// </remarks>
    /// <value><c>false</c> if the data should be kept, otherwise <c>true</c></value>
    protected virtual bool FilterOutEPGChannel(ushort networkId, ushort transportStreamId, ushort serviceId)
    {
      bool setting = SettingsManagement.GetValue("generalGrapOnlyForSameTransponder", true);

      if (!setting)
      {
        return false;
      }

      // The following code attempts to find a tuning detail for the tuner type (eg. a DVB-T tuning detail for
      // a DVB-T tuner), and check if that tuning detail corresponds with the same transponder that the EPG was
      // collected from (ie. the transponder that the tuner is currently tuned to). This logic will potentially
      // fail for people that merge HD and SD tuning details that happen to be for the same tuner type.
      Channel dbchannel = ChannelManagement.GetChannelByTuningDetail(networkId, transportStreamId, serviceId);
      if (dbchannel == null)
      {
        return false;
      }
      foreach (TuningDetail detail in dbchannel.TuningDetails)
      {
        IChannel channel = ChannelManagement.GetTuningChannel(detail);
        if (CanTune(channel))
        {
          return _currentTuningDetail.IsDifferentTransponder(channel);
        }
      }
      return false;
    }

    #endregion
  }
}