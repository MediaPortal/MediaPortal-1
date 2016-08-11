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
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
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
  /// A base implementation of <see cref="ITuner"/> for tuners with BDA
  /// drivers.
  /// </summary>
  internal abstract class TunerBdaBase : TunerDirectShowMpeg2TsBase, IESEvents
  {
    #region structs

    private struct EventRegistration
    {
      public Guid EventIid;
      public int RegistrationCookie;
      public IConnectionPoint ConnectionPoint;

      public EventRegistration(Guid eventIid, int registrationCookie, IConnectionPoint connectionPoint)
      {
        EventIid = eventIid;
        RegistrationCookie = registrationCookie;
        ConnectionPoint = connectionPoint;
      }
    }

    #endregion

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
    /// Enable or disable use of the MPEG 2 demultiplexer and BDA TIF.
    /// Compatibility with some tuners requires these filters to be connected
    /// into the graph.
    /// </summary>
    private bool _addCompatibilityFilters = true;

    /// <summary>
    /// An infinite tee filter, used to fork the stream to the demultiplexer
    /// and TS writer/analyser.
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
    private IList<EventRegistration> _eventRegistrations = new List<EventRegistration>();

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
        _filterNetworkProvider = FilterGraphTools.AddFilterFromFile(Graph, "NetworkProvider.ax", _networkProviderClsid, filterName);
        IDvbNetworkProvider internalNpInterface = _filterNetworkProvider as IDvbNetworkProvider;
        internalNpInterface.ConfigureLogging(MediaPortalNetworkProvider.GetFileName(ExternalId), MediaPortalNetworkProvider.GetHash(ExternalId), LogLevelOption.Debug);
      }
      else
      {
        _filterNetworkProvider = FilterGraphTools.AddFilterFromRegisteredClsid(Graph, _networkProviderClsid, filterName);
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
    /// <param name="container">The system's tuning space container.</param>
    /// <returns>the registed tuning space</returns>
    private ITuningSpace GetTuningSpace(ITuningSpaceContainer container)
    {
      this.LogDebug("BDA base: get tuning space");

      // We're going to enumerate the tuning spaces registered in the system to
      // see if we can find the correct MediaPortal tuning space.
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
          // Is this the one we're looking for?
          ITuningSpace tuningSpace = spaces[0];
          string name;
          hr = tuningSpace.get_UniqueName(out name);
          if (hr == (int)NativeMethods.HResult.S_OK && string.Equals(name, TuningSpaceName))
          {
            this.LogDebug("BDA base: found correct tuning space");
            return tuningSpace;
          }
          Release.ComObject("base BDA tuner tuning space", ref tuningSpace);
          TvExceptionDirectShowError.Throw(hr, "Failed to get tuning space unique name.");
        }
        return null;
      }
      finally
      {
        Release.ComObject("base BDA tuner tuning space enumerator", ref enumTuningSpaces);
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

      IPin tunerOutputPin = DsFindPin.ByDirection(MainFilter, PinDirection.Output, 0);
      try
      {
        ICollection<RegPinMedium> mediums = FilterGraphTools.GetPinMediums(tunerOutputPin);
        if (mediums == null || mediums.Count == 0)
        {
          this.LogDebug("BDA base: capture filter not required");
          return;
        }
        if (!FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(Graph, tunerOutputPin, FilterCategory.BDAReceiverComponentsCategory, out _filterCapture, out _deviceCapture, ProductInstanceId))
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
              // CableCARD tuners.
              if (mediaType.majorType == DirectShowLib.MediaType.Stream && mediaType.subType == TveGuid.MEDIA_SUB_TYPE_MPEG2_UDCR_TRANSPORT)
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

      _filterPbdaPt = FilterGraphTools.AddFilterFromRegisteredClsid(Graph, PBDA_PT_FILTER_CLSID, "PBDA PT Filter");
      FilterGraphTools.ConnectFilters(Graph, lastFilter, 0, _filterPbdaPt, 0);
      lastFilter = _filterPbdaPt;
    }

    /// <summary>
    /// Add and connect a BDA MPEG 2 transport information filter into the graph.
    /// </summary>
    private void AddAndConnectTransportInformationFilterIntoGraph()
    {
      this.LogDebug("BDA base: add BDA MPEG 2 transport information filter");
      try
      {
        _filterTransportInformation = FilterGraphTools.AddFilterFromCategory(Graph, FilterCategory.BDATransportInformationRenderersCategory, delegate(DsDevice device)
        {
          return device.Name == "BDA MPEG2 Transport Information Filter";
        });
        if (_filterTransportInformation == null)
        {
          this.LogWarn("BDA base: transport information filter not found");
          return;
        }
        FilterGraphTools.ConnectFilters(Graph, _filterMpeg2Demultiplexer, 0, _filterTransportInformation, 0);
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "BDA base: failed to add and connect BDA MPEG 2 transport information filter");
      }
    }

    /// <summary>
    /// Get the signal statistic interfaces from the tuner.
    /// </summary>
    /// <returns>the signal statistic interfaces</returns>
    private IList<IBDA_SignalStatistics> GetTunerSignalStatisticsInterfaces()
    {
      this.LogDebug("BDA base: get tuner signal statistics interfaces");

      IBDA_Topology topology = MainFilter as IBDA_Topology;
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

      DirectShowLib.IServiceProvider serviceProvider = Graph as DirectShowLib.IServiceProvider;
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
        new KeyValuePair<string, Guid>("user activity", typeof(IBDA_UserActivityService).GUID),
        new KeyValuePair<string, Guid>("DiSEqC", typeof(IBDA_DiseqCommand).GUID)
      };

      this.LogDebug("BDA base: enumerate services...");
      foreach (KeyValuePair<string, Guid> service in servicesToTest)
      {
        try
        {
          hr = serviceProvider.QueryService(service.Value, service.Value, out obj);
        }
        catch
        {
          // Invalid cast exception thrown when service not registered/available.
          hr = (int)NativeMethods.HResult.E_NOINTERFACE;
        }
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("  {0}, is CP = {1}, is CP container = {2}", service.Key, obj is IConnectionPoint, obj is IConnectionPointContainer);
          Release.ComObject(string.Format("base BDA tuner {0} service", service.Key), ref obj);
        }
      }

      #endregion

      try
      {
        hr = serviceProvider.QueryService(typeof(ESEventService).GUID, typeof(IESEventService).GUID, out obj);
      }
      catch
      {
        // Invalid cast exception thrown when service not registered/available.
        hr = (int)NativeMethods.HResult.E_NOINTERFACE;
      }
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("BDA base: event service not available, hr = 0x{0:x}", hr);
        return;
      }

      IConnectionPointContainer connectionPointContainer = obj as IConnectionPointContainer;
      if (connectionPointContainer == null)
      {
        this.LogWarn("BDA base: failed to register for events, service is not a connection point container");
        return;
      }

      this.LogDebug("BDA base: advise event service connection points...");
      IntPtr fetchCount = Marshal.AllocHGlobal(sizeof(int));
      IEnumConnectionPoints connectionPointEnum = null;
      try
      {
        connectionPointContainer.EnumConnectionPoints(out connectionPointEnum);
        IConnectionPoint[] connectionPoints = new IConnectionPoint[2];
        while (connectionPointEnum.Next(1, connectionPoints, fetchCount) == (int)NativeMethods.HResult.S_OK && Marshal.ReadInt32(fetchCount, 0) == 1)
        {
          IConnectionPoint connectionPoint = connectionPoints[0];
          if (connectionPoint == null)
          {
            this.LogWarn("BDA base: event service connection point is null");
            break;
          }

          Guid iid = Guid.Empty;
          int cookie;
          connectionPoint.GetConnectionInterface(out iid);
          if (
            // IESEvents is the only IID advertised for the Elgato EyeTV Sat on
            // W10 with driver 1.12.00.65. Other events not listed here might
            // be of interest but we can't register if we don't implement the
            // call back interface.
            iid == typeof(IESEvents).GUID ||
            iid == typeof(IESEvent).GUID ||
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
            try
            {
              connectionPoint.Advise((IESEvents)this, out cookie);
              _eventRegistrations.Add(new EventRegistration(iid, cookie, connectionPoint));
              continue;
            }
            catch (Exception ex)
            {
              this.LogWarn(ex, "BDA base: failed to register for connection interface, IID = {0}", iid);
            }
          }
          else
          {
            this.LogDebug("  other, IID = {0}", iid);
          }
          Release.ComObject(string.Format("base BDA tuner event service connection point {0}", iid), ref connectionPoint);
        }
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "BDA base: failed to register for events");
      }
      finally
      {
        Marshal.FreeHGlobal(fetchCount);
        Release.ComObject("base BDA tuner event service connection point enumerator", ref connectionPointEnum);
        Release.ComObject("base BDA tuner event service", ref obj);
      }
    }

    private void UnregisterForEvents()
    {
      this.LogDebug("BDA base: unregister for events");

      foreach (EventRegistration registration in _eventRegistrations)
      {
        registration.ConnectionPoint.Unadvise(registration.RegistrationCookie);
        IConnectionPoint connectionPoint = registration.ConnectionPoint;
        Release.ComObject("base BDA tuner event registration connection point", ref connectionPoint);
      }
      _eventRegistrations.Clear();
    }

    protected override string GetEventTypeName(Guid eventType)
    {
      if (eventType == typeof(IESCloseMmiEvent).GUID)
      {
        return "close MMI";
      }
      else if (eventType == typeof(IESFileExpiryDateEvent).GUID)
      {
        return "file expiry date";
      }
      else if (eventType == typeof(IESIsdbCasResponseEvent).GUID)
      {
        return "ISDB CAS response";
      }
      else if (eventType == typeof(IESLicenseRenewalResultEvent).GUID)
      {
        return "license renewal result";
      }
      else if (eventType == typeof(IESOpenMmiEvent).GUID)
      {
        return "open MMI";
      }
      else if (eventType == typeof(IESRequestTunerEvent).GUID)
      {
        return "request tuner";
      }
      else if (eventType == typeof(IESValueUpdatedEvent).GUID)
      {
        return "value updated";
      }

      if (eventType == BdaEventType.TUNING_CHANGING)
      {
        return "tuning changing";
      }
      else if (eventType == BdaEventType.TUNING_CHANGED)
      {
        return "tuning changed";
      }
      else if (eventType == BdaEventType.CANDIDATE_POST_TUNE_DATA)
      {
        return "candidate post tune data";
      }
      else if (eventType == BdaEventType.CA_DENIAL_COUNT_CHANGED)
      {
        return "CA denial count changed";
      }
      else if (eventType == BdaEventType.SIGNAL_STATUS_CHANGED)
      {
        return "signal status changed";
      }
      else if (eventType == BdaEventType.NEW_SIGNAL_ACQUIRED)
      {
        return "new signal acquired";
      }

      else if (eventType == BdaEventType.EAS_MESSAGE_RECEIVED)
      {
        return "EAS message received";
      }
      else if (eventType == BdaEventType.PSI_TABLE)
      {
        return "PSI table";
      }
      else if (eventType == BdaEventType.SERVICE_TERMINATED)
      {
        return "service terminated";
      }
      else if (eventType == BdaEventType.CARD_STATUS_CHANGED)
      {
        return "card status changed";
      }
      else if (eventType == BdaEventType.DRM_PAIRING_STATUS_CHANGED)
      {
        return "DRM pairing status changed";
      }
      else if (eventType == BdaEventType.DRM_PAIRING_STEP_COMPLETE)
      {
        return "DRM pairing step complete";
      }
      else if (eventType == BdaEventType.MMI_MESSAGE)
      {
        return "MMI message";
      }
      else if (eventType == BdaEventType.ENTITLEMENT_CHANGED)
      {
        return "entitlement changed";
      }
      else if (eventType == BdaEventType.STB_CHANNEL_NUMBER)
      {
        return "STB channel number";
      }

      else if (eventType == BdaEventType.BDA_EVENTING_SERVICE_PENDING_EVENT)
      {
        return "eventing service pending";
      }
      else if (eventType == BdaEventType.BDA_CONDITIONAL_ACCESS_TAG)
      {
        return "conditional access tag";
      }
      else if (eventType == BdaEventType.CAS_FAILURE_SPANNING_EVENT)
      {
        return "CAS failure [spanning]";
      }
      else if (eventType == BdaEventType.CHANNEL_CHANGE_SPANNING_EVENT)
      {
        return "channel change [spanning]";
      }
      else if (eventType == BdaEventType.CHANNEL_TYPE_SPANNING_EVENT)
      {
        return "channel type [spanning]";
      }
      else if (eventType == BdaEventType.CHANNEL_INFO_SPANNING_EVENT)
      {
        return "channel info [spanning]";
      }
      else if (eventType == BdaEventType.RRT_SPANNING_EVENT)
      {
        return "regional ratings table [spanning]";
      }
      else if (eventType == BdaEventType.CAPTION_SERVICE_DESCRIPTOR_SPANNING_EVENT)
      {
        return "caption service descriptor [spanning]";
      }
      else if (eventType == BdaEventType.CONTENT_ADVISORY_DESCRIPTOR_SPANNING_EVENT)
      {
        return "content advisory descriptor [spanning]";
      }
      else if (eventType == BdaEventType.DVB_SCRAMBLING_CONTROL_SPANNING_EVENT)
      {
        return "DVB scrambling control [spanning]";
      }
      else if (eventType == BdaEventType.SIGNAL_AND_SERVICE_STATUS_SPANNING_EVENT)
      {
        return "signal and service status [spanning]";
      }
      else if (eventType == BdaEventType.EMM_MESSAGE_SPANNING_EVENT)
      {
        return "EMM message [spanning]";
      }
      else if (eventType == BdaEventType.AUDIO_TYPE_SPANNING_EVENT)
      {
        return "audio type [spanning]";
      }
      else if (eventType == BdaEventType.STREAM_TYPE_SPANNING_EVENT)
      {
        return "stream type [spanning]";
      }
      else if (eventType == BdaEventType.ARIB_CONTENT_SPANNING_EVENT)
      {
        return "ARIB content [spanning]";
      }
      else if (eventType == BdaEventType.LANGUAGE_SPANNING_EVENT)
      {
        return "language [spanning]";
      }
      else if (eventType == BdaEventType.DUAL_MONO_SPANNING_EVENT)
      {
        return "dual mono [spanning]";
      }
      else if (eventType == BdaEventType.PID_LIST_SPANNING_EVENT)
      {
        return "PID list [spanning]";
      }
      else if (eventType == BdaEventType.AUDIO_DESCRIPTOR_SPANNING_EVENT)
      {
        return "audio descriptor [spanning]";
      }
      else if (eventType == BdaEventType.SUBTITLE_SPANNING_EVENT)
      {
        return "subtitle [spanning]";
      }
      else if (eventType == BdaEventType.TELETEXT_SPANNING_EVENT)
      {
        return "teletext [spanning]";
      }
      else if (eventType == BdaEventType.STREAM_ID_SPANNING_EVENT)
      {
        return "stream ID [spanning]";
      }
      else if (eventType == BdaEventType.PBDA_PARENTAL_CONTROL_EVENT)
      {
        return "PBDA parental control";
      }
      else if (eventType == BdaEventType.TUNE_FAILURE_EVENT)
      {
        return "tune failure";
      }
      else if (eventType == BdaEventType.TUNE_FAILURE_SPANNING_EVENT)
      {
        return "tune failure [spanning]";
      }
      else if (eventType == BdaEventType.DVB_PARENTAL_RATING_DESCRIPTOR)
      {
        return "DVB parental rating descriptor";
      }
      else if (eventType == BdaEventType.DFN_WITH_NO_ACTUAL_AV_DATA)
      {
        return "decrypt failure notification with no actual A/V data";
      }
      else if (eventType == BdaEventType.BDA_ISDB_CAS_RESPONSE)
      {
        return "ISDB CAS response";
      }
      else if (eventType == BdaEventType.BDA_CAS_REQUEST_TUNER)
      {
        return "CAS request tuner";
      }
      else if (eventType == BdaEventType.BDA_CAS_RELEASE_TUNER)
      {
        return "CAS release tuner";
      }
      else if (eventType == BdaEventType.BDA_CAS_OPEN_MMI)
      {
        return "CAS open MMI";
      }
      else if (eventType == BdaEventType.BDA_CAS_CLOSE_MMI)
      {
        return "CAS close MMI";
      }
      else if (eventType == BdaEventType.BDA_CAS_BROADCAST_MMI)
      {
        return "CAS broadcast MMI";
      }
      else if (eventType == BdaEventType.BDA_TUNER_SIGNAL_LOCK)
      {
        return "tuner signal lock";
      }
      else if (eventType == BdaEventType.BDA_TUNER_NO_SIGNAL)
      {
        return "tuner no signal";
      }
      else if (eventType == BdaEventType.BDA_GPNV_VALUE_UPDATE)
      {
        return "GPNV value update";
      }
      else if (eventType == BdaEventType.BDA_UPDATE_DRM_STATUS)
      {
        return "update DRM status";
      }
      else if (eventType == BdaEventType.BDA_UPDATE_SCAN_STATE)
      {
        return "update scan state";
      }
      else if (eventType == BdaEventType.BDA_GUIDE_DATA_AVAILABLE)
      {
        return "guide data available";
      }
      else if (eventType == BdaEventType.BDA_GUIDE_SERVICE_INFORMATION_UPDATED)
      {
        return "guide service information updated";
      }
      else if (eventType == BdaEventType.BDA_GUIDE_DATA_ERROR)
      {
        return "guide data error";
      }
      else if (eventType == BdaEventType.BDA_DISEQC_RESPONSE_AVAILABLE)
      {
        return "DiSEqC response available";
      }
      else if (eventType == BdaEventType.BDA_LBIGS_OPEN_CONNECTION)
      {
        return "low bit-rate internet gateway service open connection";
      }
      else if (eventType == BdaEventType.BDA_LBIGS_SEND_DATA)
      {
        return "low bit-rate internet gateway service send data";
      }
      else if (eventType == BdaEventType.BDA_LBIGS_CLOSE_CONNECTION_HANDLE)
      {
        return "low bit-rate internet gateway service close connection handle";
      }
      else if (eventType == BdaEventType.BDA_ENCODER_SIGNAL_LOCK)
      {
        return "encoder signal lock";
      }
      else if (eventType == BdaEventType.BDA_FDC_STATUS)
      {
        return "forward data channel status";
      }
      else if (eventType == BdaEventType.BDA_FDC_TABLE_SECTION)
      {
        return "forward data channel table section";
      }
      else if (eventType == BdaEventType.BDA_TRANSPORT_STREAM_SELECTOR_INFO)
      {
        return "transport stream selector info";
      }
      else if (eventType == BdaEventType.BDA_RATING_PIN_RESET)
      {
        return "rating PIN reset";
      }

      return base.GetEventTypeName(eventType);
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
      this.LogDebug("BDA base: reload configuration");
      base.ReloadConfiguration(configuration);

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
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      this.LogDebug("BDA base: perform loading");
      InitialiseGraph();
      AddNetworkProviderFilterToGraph();

      int hr = (int)NativeMethods.HResult.S_OK;
      if (_networkProviderClsid != typeof(MediaPortalNetworkProvider).GUID)
      {
        // Initialise the tuning space for Microsoft network providers.
        SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
        try
        {
          ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
          if (container == null)
          {
            throw new TvException("Failed to find tuning space container interface on system tuning spaces instance.");
          }

          _tuningSpace = GetTuningSpace(container);
          if (_tuningSpace == null)
          {
            _tuningSpace = CreateTuningSpace();
            object index;
            hr = container.Add(_tuningSpace, out index);
            TvExceptionDirectShowError.Throw(hr, "Failed to add new tuning space to tuning space container.");
          }
        }
        finally
        {
          Release.ComObject("base BDA tuner tuning space container", ref systemTuningSpaces);
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
      FilterGraphTools.ConnectFilters(Graph, _filterNetworkProvider, 0, MainFilter, 0);

      IBaseFilter lastFilter = MainFilter;
      AddAndConnectCaptureFilterIntoGraph(ref lastFilter);
      AddAndConnectPbdaPtFilterIntoGraph(ref lastFilter);

      // Check for and load extensions, adding any additional filters to the graph.
      IList<ITunerExtension> extensions = LoadExtensions(MainFilter, ref lastFilter);

      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Dvb;
      }

      // If using a Microsoft network provider and configured to do so, add an
      // infinite tee, MPEG 2 demultiplexer and transport information filter in
      // addition to the required TS writer/analyser.
      if (_networkProviderClsid != typeof(MediaPortalNetworkProvider).GUID && _addCompatibilityFilters)
      {
        this.LogDebug("BDA base: add compatibility filters");
        _filterInfiniteTee = (IBaseFilter)new InfTee();
        FilterGraphTools.AddAndConnectFilterIntoGraph(Graph, _filterInfiniteTee, "Infinite Tee", lastFilter);
        AddAndConnectTsWriterIntoGraph(_filterInfiniteTee, streamFormat);
        _filterMpeg2Demultiplexer = (IBaseFilter)new MPEG2Demultiplexer();
        FilterGraphTools.AddAndConnectFilterIntoGraph(Graph, _filterMpeg2Demultiplexer, "MPEG 2 Demultiplexer", _filterInfiniteTee, 1);
        AddAndConnectTransportInformationFilterIntoGraph();
      }
      else
      {
        AddAndConnectTsWriterIntoGraph(lastFilter, streamFormat);
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

      if (Graph != null)
      {
        Graph.RemoveFilter(_filterNetworkProvider);
        Graph.RemoveFilter(_filterPbdaPt);
        Graph.RemoveFilter(_filterInfiniteTee);
        Graph.RemoveFilter(_filterMpeg2Demultiplexer);
        Graph.RemoveFilter(_filterTransportInformation);
      }
      Release.ComObject("base BDA tuner network provider", ref _filterNetworkProvider);
      Release.ComObject("base BDA tuner PBDA PT filter", ref _filterPbdaPt);
      Release.ComObject("base BDA tuner infinite tee", ref _filterInfiniteTee);
      Release.ComObject("base BDA tuner MPEG 2 demultiplexer", ref _filterMpeg2Demultiplexer);
      Release.ComObject("base BDA tuner transport information filter", ref _filterTransportInformation);
      Release.ComObject("base BDA tuner tuning space", ref _tuningSpace);

      if (_filterCapture != null)
      {
        if (Graph != null)
        {
          Graph.RemoveFilter(_filterCapture);
        }
        Release.ComObject("base BDA tuner capture filter", ref _filterCapture);

        if (_deviceCapture != null)
        {
          DevicesInUse.Instance.Remove(_deviceCapture);
          _deviceCapture.Dispose();
          _deviceCapture = null;
        }
      }

      RemoveTsWriterFromGraph();
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
      this.LogDebug("BDA base: received ES event, type = {0}", GetEventTypeName(eventType));

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

      if (eventType == typeof(IESOpenMmiEvent).GUID)
      {
        IESOpenMmiEvent openMmiEvent = esEvent as IESOpenMmiEvent;
        if (openMmiEvent != null)
        {
          int dialogRequest;
          int dialogNumber;
          hr = openMmiEvent.GetDialogNumber(out dialogRequest, out dialogNumber);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  dialog request = {0}", dialogRequest);
            this.LogDebug("  dialog number  = {0}", dialogNumber);
          }
          else
          {
            this.LogWarn("BDA base: failed to get open MMI event dialog number, hr = 0x{0:x}", hr);
          }

          Guid dialogType;
          hr = openMmiEvent.GetDialogType(out dialogType);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  dialog type    = {0}", dialogType);
          }
          else
          {
            this.LogWarn("BDA base: failed to get open MMI event dialog type, hr = 0x{0:x}", hr);
          }

          string baseUrl;
          string stringData;
          hr = openMmiEvent.GetDialogStringData(out baseUrl, out stringData);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  base URL       = {0}", baseUrl);
            this.LogDebug("  string data    = {0}", stringData ?? "[null]");
          }
          else
          {
            this.LogWarn("BDA base: failed to get open MMI event dialog string data, hr = 0x{0:x}", hr);
          }

          byte[] dialogData;
          hr = openMmiEvent.GetDialogData(out dialogData);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  dialog data...");
            Dump.DumpBinary(dialogData);
          }
          else
          {
            this.LogWarn("BDA base: failed to get open MMI event dialog data, hr = 0x{0:x}", hr);
          }
        }
      }
      else if (eventType == typeof(IESCloseMmiEvent).GUID)
      {
        IESCloseMmiEvent closeMmiEvent = esEvent as IESCloseMmiEvent;
        if (closeMmiEvent != null)
        {
          int dialogNumber;
          hr = closeMmiEvent.GetDialogNumber(out dialogNumber);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  dialog number = {0}", dialogNumber);
          }
          else
          {
            this.LogWarn("BDA base: failed to get close MMI event dialog number, hr = 0x{0:x}", hr);
          }
        }
      }
      else if (eventType == typeof(IESValueUpdatedEvent).GUID)
      {
        IESValueUpdatedEvent valueUpdatedEvent = esEvent as IESValueUpdatedEvent;
        if (valueUpdatedEvent != null)
        {
          string[] names;
          hr = valueUpdatedEvent.GetValueNames(out names);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  value names = [{0}]", names == null ? "null" : string.Join(", ", names));
          }
          else
          {
            this.LogWarn("BDA base: failed to get value updated event value names, hr = 0x{0:x}", hr);
          }
        }
      }
      else if (eventType == typeof(IESRequestTunerEvent).GUID)
      {
        IESRequestTunerEvent requestTunerEvent = esEvent as IESRequestTunerEvent;
        if (requestTunerEvent != null)
        {
          byte priority;
          hr = requestTunerEvent.GetPriority(out priority);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            string priorityString;
            if (System.Enum.IsDefined(typeof(RequestTunerEventPriority), priority))
            {
              priorityString = ((RequestTunerEventPriority)priority).ToString();
            }
            else
            {
              priorityString = string.Format("reserved {0}", priority);
            }
            this.LogDebug("  priority     = {0}", priorityString);
          }
          else
          {
            this.LogWarn("BDA base: failed to get request tuner event priority, hr = 0x{0:x}", hr);
          }

          byte reason;
          hr = requestTunerEvent.GetReason(out reason);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            string reasonString;
            if (System.Enum.IsDefined(typeof(RequestTunerEventReason), reason))
            {
              reasonString = ((RequestTunerEventReason)reason).ToString();
            }
            else
            {
              reasonString = string.Format("reserved {0}", reason);
            }
            this.LogDebug("  reason       = {0}", reasonString);
          }
          else
          {
            this.LogWarn("BDA base: failed to get request tuner event reason, hr = 0x{0:x}", hr);
          }

          byte consequences;
          hr = requestTunerEvent.GetConsequences(out consequences);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            string consequencesString;
            if (System.Enum.IsDefined(typeof(RequestTunerEventConsequences), consequences))
            {
              consequencesString = ((RequestTunerEventConsequences)consequences).ToString();
            }
            else
            {
              consequencesString = string.Format("reserved {0}", consequences);
            }
            this.LogDebug("  consequences = {0}", consequencesString);
          }
          else
          {
            this.LogWarn("BDA base: failed to get request tuner event consequences, hr = 0x{0:x}", hr);
          }

          int estimatedTime;
          hr = requestTunerEvent.GetEstimatedTime(out estimatedTime);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  est. time    = {0} s", estimatedTime);
          }
          else
          {
            this.LogWarn("BDA base: failed to get request tuner event estimated time, hr = 0x{0:x}", hr);
          }
        }
      }
      else if (eventType == typeof(IESIsdbCasResponseEvent).GUID)
      {
        IESIsdbCasResponseEvent isdbCasResponseEvent = esEvent as IESIsdbCasResponseEvent;
        if (isdbCasResponseEvent != null)
        {
          int requestId;
          hr = isdbCasResponseEvent.GetRequestId(out requestId);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  request ID  = {0}", requestId);
          }
          else
          {
            this.LogWarn("BDA base: failed to get ISDB CAS response event request ID, hr = 0x{0:x}", hr);
          }

          int status;
          hr = isdbCasResponseEvent.GetStatus(out status);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            string statusString;
            if (System.Enum.IsDefined(typeof(IsdbCasResponseEventStatus), status))
            {
              statusString = ((IsdbCasResponseEventStatus)status).ToString();
            }
            else
            {
              statusString = string.Format("reserved {0}", status);
            }
            this.LogDebug("  status      = {0}", statusString);
          }
          else
          {
            this.LogWarn("BDA base: failed to get ISDB CAS response event status, hr = 0x{0:x}", hr);
          }

          int dataLength;
          hr = isdbCasResponseEvent.GetDataLength(out dataLength);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  data length = {0}", requestId);
          }
          else
          {
            this.LogWarn("BDA base: failed to get ISDB CAS response event data length, hr = 0x{0:x}", hr);
          }

          byte[] responseData;
          hr = isdbCasResponseEvent.GetResponseData(out responseData);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  response data...");
            Dump.DumpBinary(responseData);
          }
          else
          {
            this.LogWarn("BDA base: failed to get ISDB CAS response event response data, hr = 0x{0:x}", hr);
          }
        }
      }
      else if (eventType == typeof(IESLicenseRenewalResultEvent).GUID)
      {
        IESLicenseRenewalResultEvent licenseRenewalResultEvent = esEvent as IESLicenseRenewalResultEvent;
        if (licenseRenewalResultEvent != null)
        {
          int callersId;
          hr = licenseRenewalResultEvent.GetCallersId(out callersId);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  callers ID             = {0}", callersId);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event callers ID, hr = 0x{0:x}", hr);
          }

          string fileName;
          hr = licenseRenewalResultEvent.GetFileName(out fileName);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  file name              = {0}", fileName);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event file name, hr = 0x{0:x}", hr);
          }

          bool isRenewalSuccessful;
          hr = licenseRenewalResultEvent.IsRenewalSuccessful(out isRenewalSuccessful);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  is renewal successful? = {0}", isRenewalSuccessful);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event success indicator, hr = 0x{0:x}", hr);
          }

          bool isCheckEntitlementCallRequired;
          hr = licenseRenewalResultEvent.IsCheckEntitlementCallRequired(out isCheckEntitlementCallRequired);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  is CE call required?   = {0}", isCheckEntitlementCallRequired);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event check entitlement call required indicator, hr = 0x{0:x}", hr);
          }

          int descrambledStatus;
          hr = licenseRenewalResultEvent.GetDescrambledStatus(out descrambledStatus);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            string descrambledStatusString;
            if (System.Enum.IsDefined(typeof(DescrambleStatus), descrambledStatus))
            {
              descrambledStatusString = ((DescrambleStatus)descrambledStatus).ToString();
            }
            else
            {
              descrambledStatusString = string.Format("reserved {0}", descrambledStatus);
            }
            this.LogDebug("  descrambled status     = {0}", descrambledStatus);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event descrambled status, hr = 0x{0:x}", hr);
          }

          int renewalResultCode;
          hr = licenseRenewalResultEvent.GetRenewalResultCode(out renewalResultCode);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  renewal result code    = {0}", renewalResultCode);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event renewal result code, hr = 0x{0:x}", hr);
          }

          int casFailureCode;
          hr = licenseRenewalResultEvent.GetCASFailureCode(out casFailureCode);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  CAS failure code       = {0}", casFailureCode);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event CAS failure code, hr = 0x{0:x}", hr);
          }

          int renewalHresult;
          hr = licenseRenewalResultEvent.GetRenewalHResult(out renewalHresult);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  renewal HRESULT        = 0x{0:x}", renewalHresult);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event HRESULT, hr = 0x{0:x}", hr);
          }

          int entitlementTokenLength;
          hr = licenseRenewalResultEvent.GetEntitlementTokenLength(out entitlementTokenLength);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  token length           = {0}", entitlementTokenLength);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event entitlement token length, hr = 0x{0:x}", hr);
          }

          byte[] entitlementToken;
          hr = licenseRenewalResultEvent.GetEntitlementToken(out entitlementToken);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  entitlement token...");
            Dump.DumpBinary(entitlementToken);
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event entitlement token, hr = 0x{0:x}", hr);
          }

          long expiryDate;
          hr = licenseRenewalResultEvent.GetExpiryDate(out expiryDate);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  expiry date            = {0}", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDate));
          }
          else
          {
            this.LogWarn("BDA base: failed to get license renewal result event expiry date, hr = 0x{0:x}", hr);
          }
        }
      }
      else if (eventType == typeof(IESFileExpiryDateEvent).GUID)
      {
        IESFileExpiryDateEvent fileExpiryDateEvent = esEvent as IESFileExpiryDateEvent;
        if (fileExpiryDateEvent != null)
        {
          Guid tunerId;
          hr = fileExpiryDateEvent.GetTunerId(out tunerId);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  tuner ID          = {0}", tunerId);
          }
          else
          {
            this.LogWarn("BDA base: failed to get file expiry date event tuner ID, hr = 0x{0:x}", hr);
          }

          long expiryDate;
          hr = fileExpiryDateEvent.GetExpiryDate(out expiryDate);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  expiry date       = {0}", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDate));
          }
          else
          {
            this.LogWarn("BDA base: failed to get file expiry date event expiry date, hr = 0x{0:x}", hr);
          }

          hr = fileExpiryDateEvent.GetFinalExpiryDate(out expiryDate);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  final expiry date = {0}", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(expiryDate));
          }
          else
          {
            this.LogWarn("BDA base: failed to get file expiry date event final expiry date, hr = 0x{0:x}", hr);
          }

          int maxRenewalCount;
          hr = fileExpiryDateEvent.GetMaxRenewalCount(out maxRenewalCount);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  max renewal count = {0}", maxRenewalCount);
          }
          else
          {
            this.LogWarn("BDA base: failed to get file expiry date event maximum renewal count, hr = 0x{0:x}", hr);
          }

          bool isEntitlementTokenPresent;
          hr = fileExpiryDateEvent.IsEntitlementTokenPresent(out isEntitlementTokenPresent);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  is token present? = {0}", isEntitlementTokenPresent);
          }
          else
          {
            this.LogWarn("BDA base: failed to get file expiry date event entitlement token present indicator, hr = 0x{0:x}", hr);
          }

          bool doesTokenExpireAfterFirstUse;
          hr = fileExpiryDateEvent.DoesExpireAfterFirstUse(out doesTokenExpireAfterFirstUse);
          if (hr == (int)NativeMethods.HResult.S_OK)
          {
            this.LogDebug("  is one use token? = {0}", doesTokenExpireAfterFirstUse);
          }
          else
          {
            this.LogWarn("BDA base: failed to get file expiry date event expiry after first use indicator, hr = 0x{0:x}", hr);
          }
        }
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