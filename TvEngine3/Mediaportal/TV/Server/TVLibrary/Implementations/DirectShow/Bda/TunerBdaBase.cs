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
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for
  /// tuners with BDA drivers.
  /// </summary>
  internal abstract class TunerBdaBase : TunerDirectShowBase, IESEvents
  {
    #region constants

    private static readonly Guid PBDA_PT_FILTER_CLSID = new Guid(0x89c2e132, 0xc29b, 0x11db, 0x96, 0xfa, 0x00, 0x50, 0x56, 0xc0, 0x00, 0x08);

    #endregion

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

    /// <summary>
    /// The [optional] PBDA PT filter.
    /// </summary>
    private IBaseFilter _filterPbdaPt = null;

    #region compatibility filters

    /// <summary>
    /// Enable or disable use of the MPEG 2 demultiplexer and BDA TIF. Compatibility with some tuners
    /// requires these filters to be connected into the graph.
    /// </summary>
    private bool _addCompatibilityFilters = true;

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

    #region signal status

    /// <summary>
    /// Tuner and demodulator signal statistic interfaces.
    /// </summary>
    private IList<IBDA_SignalStatistics> _signalStatisticsInterfaces = new List<IBDA_SignalStatistics>();

    /// <summary>
    /// The minimum signal strength reading reported by the tuner.
    /// </summary>
    private int _signalStrengthMin = 0;

    /// <summary>
    /// The maximum signal strength reading reported by the tuner.
    /// </summary>
    private int _signalStrengthMax = 100;

    /// <summary>
    /// The minimum signal quality reading reported by the tuner.
    /// </summary>
    private int _signalQualityMin = 0;

    /// <summary>
    /// The maximum signal quality reading reported by the tuner.
    /// </summary>
    private int _signalQualityMax = 100;

    #endregion

    /// <summary>
    /// A dictionary of PBDA event registrations.
    /// </summary>
    /// <remarks>
    /// event ID => [cookie, connection point]
    /// </remarks>
    private IDictionary<Guid, KeyValuePair<int, IConnectionPoint>> _eventRegistrations = new Dictionary<Guid, KeyValuePair<int, IConnectionPoint>>();

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaBase"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="externalId">The tuner's unique external identifier.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    protected TunerBdaBase(DsDevice device, string externalId, BroadcastStandard supportedBroadcastStandards)
      : base(device.Name, externalId, device.TunerInstanceIdentifier >= 0 ? device.TunerInstanceIdentifier.ToString() : null, device.ProductInstanceIdentifier, supportedBroadcastStandards)
    {
      _deviceMain = device;
    }

    #endregion

    #region tuning

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the sub-channel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the sub-channel associated with the tuned channel</returns>
    public override ISubChannel Tune(int subChannelId, IChannel channel)
    {
      ISubChannel subChannel = base.Tune(subChannelId, channel);
      if (_filterTransportInformation != null)
      {
        _filterTransportInformation.Stop();
      }
      return subChannel;
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
    /// Add the appropriate BDA network provider filter to the graph.
    /// </summary>
    private void AddNetworkProviderFilterToGraph()
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
        TvExceptionDirectShowError.Throw(hr, "Failed to get tuning space enumerator from tuning space container.");
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
              TvExceptionDirectShowError.Throw(hr, "Failed to get tuning space unique name.");
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
            catch
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
    private void AddAndConnectCaptureFilterIntoGraph(ref IBaseFilter lastFilter)
    {
      this.LogDebug("BDA base: add capture filter");

      IPin tunerOutputPin = DsFindPin.ByDirection(_filterMain, PinDirection.Output, 0);
      try
      {
        ICollection<RegPinMedium> mediums = FilterGraphTools.GetPinMediums(tunerOutputPin);
        if (mediums == null || mediums.Count == 0)
        {
          this.LogDebug("BDA base: capture filter not required");
          return;
        }
        if (!FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(_graph, tunerOutputPin, FilterCategory.BDAReceiverComponentsCategory, out _filterCapture, out _deviceCapture, ProductInstanceId))
        {
          this.LogWarn("BDA base: failed to add and connect capture filter, assuming extension required to complete graph");
          return;
        }
        lastFilter = _filterCapture;
      }
      catch (Exception ex)
      {
        throw new TvException(ex, "Failed to add and connect capture filter.");
      }
      finally
      {
        Release.ComObject("base BDA tuner output pin", ref tunerOutputPin);
      }
    }

    /// <summary>
    /// Add and connect a PBDA PT filter into the graph if required.
    /// </summary>
    /// <param name="lastFilter">The upstream filter to connect the PBDA PT filter to.</param>
    private void AddAndConnectPbdaPtFilterIntoGraph(ref IBaseFilter lastFilter)
    {
      this.LogDebug("BDA base: add PBDA PT filter");

      IPin lastFilterOutputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
      if (lastFilterOutputPin == null)
      {
        throw new TvException("Failed to add and connect PBDA PT filter. Last upstream filter does not have an output pin.");
      }

      bool isPtFilterRequired = false;
      try
      {
        IEnumMediaTypes mediaTypeEnum;
        int hr = lastFilterOutputPin.EnumMediaTypes(out mediaTypeEnum);
        TvExceptionDirectShowError.Throw(hr, "Failed to obtain media type enumerator for pin.");
        try
        {
          // For each pin media type...
          int mediaTypeCount;
          AMMediaType[] mediaTypes = new AMMediaType[2];
          while (mediaTypeEnum.Next(1, mediaTypes, out mediaTypeCount) == (int)NativeMethods.HResult.S_OK && mediaTypeCount == 1)
          {
            AMMediaType mediaType = mediaTypes[0];
            try
            {
              if (mediaType.majorType == DirectShowLib.MediaType.Mpeg2Sections)
              {
                isPtFilterRequired = true;
                break;
              }
            }
            finally
            {
              Release.AmMediaType(ref mediaType);
            }
          }
        }
        finally
        {
          Release.ComObject("base BDA tuner PBDA test pin media type enumerator", ref mediaTypeEnum);
        }
      }
      finally
      {
        Release.ComObject("base BDA tuner PBDA test pin", ref lastFilterOutputPin);
      }

      if (!isPtFilterRequired)
      {
        this.LogDebug("BDA base: PBDA PT filter not required");
        return;
      }

      _filterPbdaPt = FilterGraphTools.AddFilterFromRegisteredClsid(_graph, PBDA_PT_FILTER_CLSID, "PBDA PT Filter");
      FilterGraphTools.ConnectFilters(_graph, lastFilter, 0, _filterPbdaPt, 0);
      lastFilter = _filterPbdaPt;
    }

    /// <summary>
    /// Add and connect a BDA MPEG 2 transport information filter into the graph.
    /// </summary>
    private void AddAndConnectTransportInformationFilterIntoGraph()
    {
      this.LogDebug("BDA base: add BDA MPEG 2 transport information filter");
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
          _filterTransportInformation = FilterGraphTools.AddFilterFromDevice(_graph, device);
          FilterGraphTools.ConnectFilters(_graph, _filterMpeg2Demultiplexer, 0, _filterTransportInformation, 0);
          return;
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "BDA base: failed to add and connect BDA MPEG 2 transport information filter");
        return;
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
      TvExceptionDirectShowError.Throw(hr, "Failed to get topology node types.");

      IList<IBDA_SignalStatistics> statistics = new List<IBDA_SignalStatistics>(2);
      int interfaceCount;
      Guid[] interfaces = new Guid[33];
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        hr = topology.GetNodeInterfaces(nodeTypes[i], out interfaceCount, 32, interfaces);
        TvExceptionDirectShowError.Throw(hr, "Failed to get topology node interfaces for node type {0} ({1}).", nodeTypes[i], i);
        for (int j = 0; j < interfaceCount; j++)
        {
          if (interfaces[j] == typeof(IBDA_SignalStatistics).GUID)
          {
            object controlNode;
            hr = topology.GetControlNode(0, 1, nodeTypes[i], out controlNode);
            TvExceptionDirectShowError.Throw(hr, "Failed to get topology control node for node type {0} ({1}).", nodeTypes[i], i);
            statistics.Add(controlNode as IBDA_SignalStatistics);
          }
        }
      }
      if (statistics.Count == 0)
      {
        throw new TvException("Failed to find signal statistic interfaces.");
      }
      this.LogDebug("BDA base: found {0} interface(s)", statistics.Count);
      return statistics;
    }

    #region events

    /// <summary>
    /// Register to receive PBDA events from the graph.
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd758062%28v=vs.85%29.aspx
    /// </remarks>
    private void RegisterForEvents()
    {
      this.LogDebug("BDA base: register for events");

      DirectShowLib.IServiceProvider serviceProvider = _graph as DirectShowLib.IServiceProvider;
      if (serviceProvider == null)
      {
        this.LogWarn("BDA base: failed to register for events, graph is not a service provider");
        return;
      }

      int hr;
      object obj = null;
      #region debug/testing

      List<KeyValuePair<string, Guid>> servicesToTest = new List<KeyValuePair<string, Guid>>()
      {
        new KeyValuePair<string, Guid>("EAS", typeof(IBDA_EasMessage).GUID),
        new KeyValuePair<string, Guid>("conditional access", typeof(IBDA_ConditionalAccess).GUID),
        new KeyValuePair<string, Guid>("diagnostic properties", typeof(IBDA_DiagnosticProperties).GUID),
        new KeyValuePair<string, Guid>("DRM interface", typeof(IBDA_DRM).GUID),
        new KeyValuePair<string, Guid>("name-value", typeof(IBDA_NameValueService).GUID),
        new KeyValuePair<string, Guid>("extended conditional access", typeof(IBDA_ConditionalAccessEx).GUID),
        new KeyValuePair<string, Guid>("ISDB conditional access", typeof(IBDA_ISDBConditionalAccess).GUID),
        new KeyValuePair<string, Guid>("eventing", typeof(IBDA_EventingService).GUID),
        new KeyValuePair<string, Guid>("aux", typeof(IBDA_AUX).GUID),
        new KeyValuePair<string, Guid>("encoder", typeof(IBDA_Encoder).GUID),
        new KeyValuePair<string, Guid>("FDC", typeof(IBDA_FDC).GUID),
        new KeyValuePair<string, Guid>("guide data delivery", typeof(IBDA_GuideDataDeliveryService).GUID),
        new KeyValuePair<string, Guid>("DRM service", typeof(IBDA_DRMService).GUID),
        new KeyValuePair<string, Guid>("WMDRM session", typeof(IBDA_WMDRMSession).GUID),
        new KeyValuePair<string, Guid>("WMDRM tuner", typeof(IBDA_WMDRMTuner).GUID),
        new KeyValuePair<string, Guid>("DRI DRM", typeof(IBDA_DRIDRMService).GUID),
        new KeyValuePair<string, Guid>("DRI DRM session", typeof(IBDA_DRIWMDRMSession).GUID),
        new KeyValuePair<string, Guid>("mux", typeof(IBDA_MUX).GUID),
        new KeyValuePair<string, Guid>("transport stream selector", typeof(IBDA_TransportStreamSelector).GUID),
        new KeyValuePair<string, Guid>("user activity", typeof(IBDA_UserActivityService).GUID)
      };

      this.LogDebug("BDA base: enumerate services...");
      foreach (KeyValuePair<string, Guid> service in servicesToTest)
      {
        hr = serviceProvider.QueryService(service.Value, service.Value, out obj);
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("  {0}, is CP = {1}, is CP container = {2}", service.Key, obj is IConnectionPoint, obj is IConnectionPointContainer);
          Release.ComObject(string.Format("base BDA tuner {0} service", service.Key), ref obj);
        }
      }

      #endregion

      hr = serviceProvider.QueryService(typeof(ESEventService).GUID, typeof(IESEventService).GUID, out obj);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("BDA base: event service not available");
        return;
      }

      IConnectionPointContainer connectionPointContainer = obj as IConnectionPointContainer;
      if (connectionPointContainer == null)
      {
        this.LogWarn("BDA base: failed to register for events, service is not a connection point container");
        return;
      }

      this.LogDebug("BDA base: advise event service connection points...");
      try
      {
        IEnumConnectionPoints connectionPointEnum;
        connectionPointContainer.EnumConnectionPoints(out connectionPointEnum);
        IConnectionPoint[] connectionPoints = new IConnectionPoint[2];
        IntPtr fetchCount = Marshal.AllocHGlobal(sizeof(int));
        try
        {
          while (connectionPointEnum.Next(1, connectionPoints, fetchCount) == (int)NativeMethods.HResult.S_OK && Marshal.ReadInt32(fetchCount, 0) == 1)
          {
            IConnectionPoint connectionPoint = connectionPoints[0];
            Guid iid = Guid.Empty;
            int cookie;
            connectionPoint.GetConnectionInterface(out iid);
            if (
              iid == typeof(IESCloseMmiEvent).GUID ||
              iid == typeof(IESFileExpiryDateEvent).GUID ||
              iid == typeof(IESIsdbCasResponseEvent).GUID ||
              iid == typeof(IESLicenseRenewalResultEvent).GUID ||
              iid == typeof(IESOpenMmiEvent).GUID ||
              iid == typeof(IESRequestTunerEvent).GUID ||
              iid == typeof(IESValueUpdatedEvent).GUID
            )
            {
              this.LogDebug("  {0}", iid);
              connectionPoint.Advise((IESEvents)this, out cookie);
              _eventRegistrations.Add(iid, new KeyValuePair<int, IConnectionPoint>(cookie, connectionPoint));
            }
            else
            {
              // These events might be interesting but we can't register if we
              // don't implement the call back interface.
              this.LogDebug("  other, IID = {0}", iid);
              Release.ComObject(string.Format("base BDA tuner event service connection point {0}", iid), ref connectionPoint);
            }
          }
        }
        finally
        {
          Marshal.FreeHGlobal(fetchCount);
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "BDA base: failed to register for events");
      }
      finally
      {
        Release.ComObject("base BDA tuner event service", ref obj);
      }
    }

    private void UnregisterForEvents()
    {
      this.LogDebug("BDA base: unregister for events");

      foreach (KeyValuePair<int, IConnectionPoint> registration in _eventRegistrations.Values)
      {
        registration.Value.Unadvise(registration.Key);
        IConnectionPoint connectionPoint = registration.Value;
        Release.ComObject("base BDA tuner event registration connection point", ref connectionPoint);
      }
      _eventRegistrations.Clear();
    }

    #endregion

    #endregion

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(Tuner configuration)
    {
      base.ReloadConfiguration(configuration);

      this.LogDebug("BDA base: reload configuration");
      this.LogDebug("  network provider = {0}", (BdaNetworkProvider)configuration.BdaNetworkProvider);

      bool save = false;
      _networkProviderClsid = NetworkProviderClsid; // specific network provider
      if (configuration.BdaNetworkProvider == (int)BdaNetworkProvider.MediaPortal)
      {
        if (!File.Exists(PathManager.BuildAssemblyRelativePath("NetworkProvider.ax")))
        {
          this.LogWarn("BDA base: MediaPortal network provider is not available, try Microsoft generic network provider");
          configuration.BdaNetworkProvider = (int)BdaNetworkProvider.Generic;
          save = true;
        }
        else
        {
          _networkProviderClsid = typeof(MediaPortalNetworkProvider).GUID;
        }
      }
      if (configuration.BdaNetworkProvider == (int)BdaNetworkProvider.Generic)
      {
        if (!FilterGraphTools.IsThisComObjectInstalled(typeof(NetworkProvider).GUID))
        {
          this.LogWarn("BDA base: Microsoft generic network provider is not available, try Microsoft specific network provider");
          configuration.BdaNetworkProvider = (int)BdaNetworkProvider.Specific;
          save = true;
        }
        else
        {
          _networkProviderClsid = typeof(NetworkProvider).GUID;
        }
      }
      if (save)
      {
        TunerManagement.SaveTuner(configuration);
      }
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading()
    {
      this.LogDebug("BDA base: perform loading");
      InitialiseGraph();
      AddNetworkProviderFilterToGraph();

      int hr = (int)NativeMethods.HResult.S_OK;
      if (_networkProviderClsid != typeof(MediaPortalNetworkProvider).GUID)
      {
        // Initialise the tuning space for Microsoft network providers.
        _tuningSpace = GetTuningSpace();
        if (_tuningSpace == null)
        {
          _tuningSpace = CreateTuningSpace();
        }

        // Some specific Microsoft network providers won't connect to the tuner
        // filter unless you set a tuning space first. This is not required for
        // the generic network provider, which returns HRESULT 0x80070057
        // (E_INVALIDARG) if you try put_TuningSpace().
        if (_networkProviderClsid == NetworkProviderClsid)
        {
          DirectShowLib.BDA.ITuner tuner = _filterNetworkProvider as DirectShowLib.BDA.ITuner;
          if (tuner == null)
          {
            throw new TvException("Failed to find tuner interface on network provider.");
          }
          hr = tuner.put_TuningSpace(_tuningSpace);
          TvExceptionDirectShowError.Throw(hr, "Failed to apply tuning space on tuner.");
        }
      }

      AddMainComponentFilterToGraph();
      FilterGraphTools.ConnectFilters(_graph, _filterNetworkProvider, 0, _filterMain, 0);

      IBaseFilter lastFilter = _filterMain;
      AddAndConnectCaptureFilterIntoGraph(ref lastFilter);
      AddAndConnectPbdaPtFilterIntoGraph(ref lastFilter);

      // Check for and load extensions, adding any additional filters to the graph.
      IList<ITunerExtension> extensions = LoadExtensions(_filterMain, ref lastFilter);

      // If using a Microsoft network provider and configured to do so, add an
      // infinite tee, MPEG 2 demultiplexer and transport information filter in
      // addition to the required TS writer/analyser.
      if (_networkProviderClsid != typeof(MediaPortalNetworkProvider).GUID && _addCompatibilityFilters)
      {
        this.LogDebug("BDA base: add compatibility filters");
        _filterInfiniteTee = (IBaseFilter)new InfTee();
        FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterInfiniteTee, "Infinite Tee", lastFilter);
        AddAndConnectTsWriterIntoGraph(_filterInfiniteTee);
        _filterMpeg2Demultiplexer = (IBaseFilter)new MPEG2Demultiplexer();
        FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterMpeg2Demultiplexer, "MPEG 2 Demultiplexer", _filterInfiniteTee, 1);
        AddAndConnectTransportInformationFilterIntoGraph();
      }
      else
      {
        AddAndConnectTsWriterIntoGraph(lastFilter);
      }

      CompleteGraph();
      RegisterForEvents();
      _signalStatisticsInterfaces = GetTunerSignalStatisticsInterfaces();
      return extensions;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      this.LogDebug("BDA base: perform unloading");
      if (isFinalising)
      {
        CleanUpGraph(isFinalising);
        return;
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

      UnregisterForEvents();

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

      if (_filterPbdaPt != null)
      {
        if (_graph != null)
        {
          _graph.RemoveFilter(_filterPbdaPt);
        }
        Release.ComObject("base BDA tuner PBDA PT filter", ref _filterPbdaPt);
      }

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

      CleanUpGraph(isFinalising);
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      int hr = (int)NativeMethods.HResult.S_OK;
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
        hr = (_filterNetworkProvider as DirectShowLib.BDA.ITuner).put_TuneRequest(tuneRequest);
        this.LogDebug("BDA base: parameters applied, hr = 0x{0:x}", hr);
        Release.ComObject("base BDA tuner tune request", ref tuneRequest);
      }

      // TerraTec tuners return a positive HRESULT value when already tuned with the required
      // parameters. See mantis 3469 for more details.
      if (hr < (int)NativeMethods.HResult.S_OK)
      {
        TvExceptionDirectShowError.Throw(hr, "Failed to tune channel.");
      }
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public override void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      isLocked = false;
      isPresent = false;
      strength = 0;
      quality = 0;
      if (_signalStatisticsInterfaces == null || _signalStatisticsInterfaces.Count == 0)
      {
        return;
      }

      bool tempIsLocked = false;
      bool tempIsPresent = false;
      int tempStrength = 0;
      int tempQuality = 0;
      int strengthCount = 0;
      int qualityCount = 0;
      int hr = (int)NativeMethods.HResult.S_OK;
      for (int i = 0; i < _signalStatisticsInterfaces.Count; i++)
      {
        IBDA_SignalStatistics statisticsInterface = _signalStatisticsInterfaces[i];
        try
        {
          hr = statisticsInterface.get_SignalLocked(out tempIsLocked);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            isLocked |= tempIsLocked;
          }
          else
          {
            this.LogWarn("BDA base: failed to get signal lock from interface {0}, hr = 0x{1:x}", i, hr);
          }
          if (onlyGetLock)
          {
            continue;
          }

          hr = statisticsInterface.get_SignalPresent(out tempIsPresent);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            isPresent |= tempIsPresent;
          }
          else
          {
            this.LogWarn("BDA base: failed to get signal present from interface {0}, hr = 0x{1:x}", i, hr);
          }

          hr = statisticsInterface.get_SignalStrength(out tempStrength);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            if (tempStrength != 0)
            {
              strength += tempStrength;
              strengthCount++;
            }
          }
          else
          {
            this.LogWarn("BDA base: failed to get signal strength from interface {0}, hr = 0x{1:x}", i, hr);
          }

          hr = statisticsInterface.get_SignalQuality(out tempQuality);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            if (tempQuality != 0)
            {
              quality += tempQuality;
              qualityCount++;
            }
          }
          else
          {
            this.LogWarn("BDA base: failed to get signal quality from interface {0}, hr = 0x{1:x}", i, hr);
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "BDA base: exception getting signal statistics from interface {0}", i);
        }
      }

      if (!onlyGetLock)
      {
        if (strengthCount > 1)
        {
          strength /= strengthCount;
        }
        if (strength < _signalStrengthMin)
        {
          this.LogDebug("BDA base: adjusting minimum signal strength, current = {0}, new = {1}", _signalStrengthMin, strength);
          _signalStrengthMin = strength;
        }
        else if (strength > _signalStrengthMax)
        {
          this.LogDebug("BDA base: adjusting maximum signal strength, current = {0}, new = {1}", _signalStrengthMax, strength);
          _signalStrengthMax = strength;
        }
        strength = (strength - _signalStrengthMin) * 100 / (_signalStrengthMax - _signalStrengthMin);

        if (qualityCount > 1)
        {
          quality /= qualityCount;
        }
        if (quality < _signalQualityMin)
        {
          this.LogDebug("BDA base: adjusting minimum signal quality, current = {0}, new = {1}", _signalStrengthMin, quality);
          _signalQualityMin = quality;
        }
        else if (quality > _signalStrengthMax)
        {
          this.LogDebug("BDA base: adjusting maximum signal quality, current = {0}, new = {1}", _signalStrengthMax, quality);
          _signalQualityMax = quality;
        }
        quality = (quality - _signalQualityMin) * 100 / (_signalQualityMax - _signalQualityMin);
      }
    }

    #endregion

    #endregion

    #region IESEvents member

    /// <summary>
    /// Handler for PBDA events.
    /// </summary>
    public int OnESEventReceived(Guid eventType, IESEvent esEvent)
    {
      this.LogDebug("BDA base: received ES event, type = {0}", eventType);

      int id;
      int hr = esEvent.GetEventId(out id);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("  ID = {0}", id);
      }
      else
      {
        this.LogWarn("BDA base: failed to get event ID, hr = 0x{0:x}", hr);
      }

      byte[] data;
      hr = esEvent.GetData(out data);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("  data...");
        Dump.DumpBinary(data);
      }
      else
      {
        this.LogWarn("BDA base: failed to get event data, hr = 0x{0:x}", hr);
      }

      // The PBDA specification defines error codes in the core services
      // document. Zero is success.
      hr = esEvent.SetCompletionStatus(0);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogWarn("BDA base: failed to set event completion status, hr = 0x{0:x}", hr);
      }

      return (int)NativeMethods.HResult.S_OK;
    }

    #endregion
  }
}