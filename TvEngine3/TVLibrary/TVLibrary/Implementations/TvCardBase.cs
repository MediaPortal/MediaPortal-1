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
using System.Reflection;
using DirectShowLib;
using TvDatabase;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;
using TvLibrary.Epg;
using TvLibrary.ChannelLinkage;
using DirectShowLib.BDA;
using TvLibrary.Channels;

namespace TvLibrary.Implementations
{
  /// <summary>
  /// Base class for all tv cards
  /// </summary>
  public abstract class TvCardBase : ITVCard
  {
    #region events

    /// <summary>
    /// Delegate for the after tune event.
    /// </summary>
    public delegate void OnAfterTuneDelegate();

    /// <summary>
    /// After tune observer event.
    /// </summary>
    public event OnAfterTuneDelegate AfterTuneEvent;

    /// <summary>
    /// Handles the after tune observer event.
    /// </summary>
    protected void TvCardBase_OnAfterTuneEvent()
    {
      if (AfterTuneEvent != null)
      {
        AfterTuneEvent();
      }
    }

    #endregion

    #region ctor

    ///<summary>
    /// Base constructor
    ///</summary>
    ///<param name="device">Base DS device</param>
    protected TvCardBase(DsDevice device)
    {
      _graphState = GraphState.Idle;
      _mapSubChannels = new Dictionary<int, BaseSubChannel>();
      _lastSignalUpdate = DateTime.MinValue;
      _parameters = new ScanParameters();
      _epgGrabbing = false;   // EPG grabbing not supported by default.
      _customDeviceInterfaces = new List<ICustomDevice>();

      _device = device;
      _tunerDevice = device;
      if (device != null)
      {
        _name = device.Name;
        _devicePath = device.DevicePath;
      }

      if (_devicePath != null)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card c = layer.GetCardByDevicePath(_devicePath);
        if (c != null)
        {
          _cardId = c.IdCard;
          _preloadCard = c.PreloadCard;
          _stopGraph = c.StopGraph;
          _useConditionalAccessInterace = c.CAM;
          _camType = (CamType)c.CamType;
          _decryptLimit = c.DecryptLimit;
        }
      }
    }

    #endregion

    /// <summary>
    /// Checks if the graph is running
    /// </summary>
    /// <returns>true, if the graph is running; false otherwise</returns>
    protected bool GraphRunning()
    {
      bool graphRunning = false;

      if (_graphBuilder != null)
      {
        FilterState state;
        ((IMediaControl)_graphBuilder).GetState(10, out state);
        graphRunning = (state == FilterState.Running);
      }
      //Log.Log.WriteFile("subch:{0} GraphRunning: {1}", _subChannelId, graphRunning);
      return graphRunning;
    }

    #region variables

    /// <summary>
    /// Indicates, if the card should be preloaded
    /// </summary>
    protected bool _preloadCard;

    /// <summary>
    /// Indicates, if the card supports pausegraph, otherwise stopgraph will be used when card is idle.
    /// </summary>
    protected bool _stopGraph;

    /// <summary>
    /// Scanning Paramters
    /// </summary>
    protected ScanParameters _parameters;

    /// <summary>
    /// Dictionary of the corresponding sub channels
    /// </summary>
    protected Dictionary<int, BaseSubChannel> _mapSubChannels;

    /// <summary>
    /// Indicates, if the card is a hybrid one
    /// </summary>
    protected bool _isHybrid;

    /// <summary>
    /// Context reference
    /// </summary>
    protected object m_context;

    /// <summary>
    /// Indicates, if the tuner is locked
    /// </summary>
    protected bool _tunerLocked;

    /// <summary>
    /// Value of the signal level
    /// </summary>
    protected int _signalLevel;

    /// <summary>
    /// Value of the signal quality
    /// </summary>
    protected int _signalQuality;

    /// <summary>
    /// Device Path of the tv card
    /// </summary>
    protected String _devicePath;

    /// <summary>
    /// Indicates, if the card is grabbing epg
    /// </summary>
    protected bool _epgGrabbing;

    /// <summary>
    /// Name of the tv card
    /// </summary>
    protected String _name;

    /// <summary>
    /// Indicates, if the card is scanning
    /// </summary>
    protected bool _isScanning;

    /// <summary>
    /// State of the graph
    /// </summary>
    protected GraphState _graphState = GraphState.Idle;

    /// <summary>
    /// The graph builder
    /// </summary>
    protected IFilterGraph2 _graphBuilder;

    /// <summary>
    /// Indicates, if the card sub channels
    /// </summary>
    protected bool _supportsSubChannels;

    /// <summary>
    /// Type of the card
    /// </summary>
    protected CardType _cardType;

    /// <summary>
    /// Date and time of the last signal update
    /// </summary>
    protected DateTime _lastSignalUpdate;

    /// <summary>
    /// Last subchannel id
    /// </summary>
    protected int _subChannelId;

    /// <summary>
    /// Indicates, if the signal is present
    /// </summary>
    protected bool _signalPresent;

    /// <summary>
    /// Indicates, if the card is present
    /// </summary>
    protected bool _cardPresent = true;

    /// <summary>
    /// The tuner device
    /// </summary>
    protected DsDevice _tunerDevice;

    /// <summary>
    /// Main device of the card
    /// </summary>
    protected DsDevice _device;

    /// <summary>
    /// The db card id
    /// </summary>
    protected int _cardId;

    /// <summary>
    /// A list containing the custom device interfaces supported by this device. The list is ordered by
    /// interface priority.
    /// </summary>
    protected List<ICustomDevice> _customDeviceInterfaces;

    /// <summary>
    /// The device's conditional access interface.
    /// </summary>
    protected IConditionalAccessProvider _conditionalAccessInterface = null;

    /// <summary>
    /// Enable or disable the use of the conditional access interface (assuming an interface is available).
    /// </summary>
    protected bool _useConditionalAccessInterace = true;

    /// <summary>
    /// The type of conditional access module available to the conditional access interface.
    /// </summary>
    protected CamType _camType = CamType.Default;

    /// <summary>
    /// The numer of channels that the device is capable of or permitted to decrypt simultaneously. Zero means
    /// there is no limit.
    /// </summary>
    protected int _decryptLimit = 0;

    /// <summary>
    /// The method that should be used to communicate the set of channels that the device's conditional access
    /// interface needs to manage.
    /// </summary>
    /// <remarks>
    /// First = Explicit management using Only, First, More and Last. This is the most widely supported set
    ///         of commands, however they are not suitable for some interfaces (such as the Digital Devices
    ///         interface).
    /// Only = Always send Only. In most cases this will result in only one channel being decrypted. If other
    ///         methods are not working reliably then this one should at least allow decrypting one channel
    ///         reliably.
    /// Add = Use Add, Update and Remove to pass changes to the interface. The full channel list is never
    ///         passed. Most interfaces don't support these commands.
    /// </remarks>
    protected CaPmtListManagementAction _multiDecryptMode = CaPmtListManagementAction.First;

    /// <summary>
    /// Enable or disable waiting for the conditional interface to be ready before sending commands.
    /// </summary>
    protected bool _waitUntilCaInterfaceReady = true;

    /// <summary>
    /// The number of times to re-attempt decrypting the current service set when one or more services are
    /// not able to be decrypted for whatever reason.
    /// </summary>
    /// <remarks>
    /// Each available CA interface will be tried in order of priority. If decrypting is not started
    /// successfully, all interfaces are retried until each interface has been tried
    /// _decryptFailureRetryCount + 1 times, or until decrypting is successful.
    /// </remarks>
    protected int _decryptFailureRetryCount = 2;

    /// <summary>
    /// Force the device PID filter(s) to be enabled even when the tuning context (DVB-S vs. DVB-S2 etc.)
    /// would not normally require them to be enabled. Enabling this setting also forces the filter to
    /// remain active when the filter controller is not capable of passing all of the required PIDs. It is
    /// recommended to disable this setting.
    /// </summary>
    protected bool _forceEnablePidFilter = false;

    /// <summary>
    /// Force the device PID filter(s) to be disabled even when the tuning context (DVB-S vs. DVB-S2 etc.)
    /// would normally require them to be enabled. It is recommended to disable this setting.
    /// </summary>
    protected bool _forceDisablePidFilter = false;

    #endregion

    #region properties

    /// <summary>
    /// Gets wether or not card supports pausing the graph.
    /// </summary>
    public virtual bool SupportsPauseGraph
    {
      get { return !_stopGraph; }
    }

    /// <summary>
    /// Gets or sets the unique id of this card
    /// </summary>
    public virtual int CardId
    {
      get { return _cardId; }
      set { _cardId = value; }
    }


    /// <summary>
    /// returns true if card should be preloaded
    /// </summary>
    public bool PreloadCard
    {
      get { return _preloadCard; }
    }

    /// <summary>
    /// Gets a value indicating whether card supports subchannels
    /// </summary>
    /// <value><c>true</c> if card supports sub channels; otherwise, <c>false</c>.</value>
    public bool SupportsSubChannels
    {
      get { return _supportsSubChannels; }
    }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    /// <value>The parameters.</value>
    public ScanParameters Parameters
    {
      get { return _parameters; }
      set
      {
        _parameters = value;
        Dictionary<int, BaseSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          en.Current.Value.Parameters = value;
        }
      }
    }

    /// <summary>
    /// Gets/sets the card name
    /// </summary>
    /// <value></value>
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>
    /// returns true if card is currently present
    /// </summary>
    public bool CardPresent
    {
      get { return _cardPresent; }
      set { _cardPresent = value; }
    }

    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    public virtual string DevicePath
    {
      get { return _devicePath; }
    }

    /// <summary>
    /// Gets the device tuner type.
    /// </summary>
    public virtual CardType CardType
    {
      get { return _cardType; }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }

    #region tuning range properties

    /// <summary>
    /// Get the minimum channel number that the device is capable of tuning (only applicable for analog
    /// tuners - should be removed if possible).
    /// </summary>
    /// <value>
    /// <c>-1</c> if the property is not applicable, otherwise the minimum channel number that the device
    /// is capable of tuning
    /// </value>
    public int MinChannel
    {
      get
      {
        return -1;
      }
    }

    /// <summary>
    /// Get the maximum channel number that the device is capable of tuning (only applicable for analog
    /// tuners - should be removed if possible).
    /// </summary>
    /// <value>
    /// <c>-1</c> if the property is not applicable, otherwise the maximum channel number that the device
    /// is capable of tuning
    /// </value>
    public int MaxChannel
    {
      get
      {
        return -1;
      }
    }

    #endregion

    #region conditional access properties

    /// <summary>
    /// Does the device have a conditional access interface?
    /// </summary>
    public bool HasCA
    {
      get
      {
        return _conditionalAccessInterface != null && _useConditionalAccessInterace;
      }
    }

    /// <summary>
    /// Get the device's conditional access interface.
    /// </summary>
    public IConditionalAccessProvider CaInterface
    {
      get
      {
        return _conditionalAccessInterface;
      }
    }

    /// <summary>
    /// Get/set the type of conditional access module available to the conditional access interface.
    /// </summary>
    /// <value>The type of the cam.</value>
    public CamType CamType
    {
      get
      {
        return _camType;
      }
      set
      {
        _camType = value;
      }
    }

    /// <summary>
    /// Get the device's conditional access interface decrypt limit. This is usually the number of channels
    /// that the interface is able to decrypt simultaneously. A value of zero indicates that there is no
    /// limit.
    /// </summary>
    public int DecryptLimit
    {
      get
      {
        return _decryptLimit;
      }
    }

    /// <summary>
    /// Gets the number of channels the card is currently decrypting.
    /// </summary>
    /// <value>The number of channels decrypting.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        //TODO
/*        if (_mapSubChannels == null)
          return 0;
        if (_mapSubChannels.Count == 0)
          return 0;
        //if (_decryptLimit == 0)
          return 0; //CA disabled, so no channels are decrypting.

        List<ConditionalAccessContext> filteredChannels = new List<ConditionalAccessContext>();

        Dictionary<int, ConditionalAccessContext>.Enumerator en = _mapSubChannels.GetEnumerator();

        while (en.MoveNext())
        {
          bool exists = false;
          ConditionalAccessContext context = en.Current.Value;
          if (context != null)
          {
            foreach (ConditionalAccessContext dvbService in filteredChannels)
            {
              if (dvbService.Channel != null && context.Channel != null)
              {
                if (dvbService.Channel.Equals(context.Channel))
                {
                  exists = true;
                  break;
                }
              }
            }
            if (!exists)
            {
              if (context.Channel != null && !context.Channel.FreeToAir)
              {
                filteredChannels.Add(context);
              }
            }
          }
        }
        return filteredChannels.Count;*/
        return 0;
      }
    }

    #endregion

    /// <summary>
    /// Gets the interface for controlling a DiSEqC satellite dish motor (only applicable for satellite
    /// tuners - should be removed if possible).
    /// </summary>
    /// <value>
    /// <c>null</c> if the tuner is not a satellite tuner or the tuner doesn't support controlling a motor,
    /// otherwise the interface for controlling the motor
    /// </value>
    public virtual IDiSEqCMotor DiSEqCMotor
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is hybrid.
    /// </summary>
    /// <value><c>true</c> if this instance is hybrid; otherwise, <c>false</c>.</value>
    public bool IsHybrid
    {
      get { return _isHybrid; }
      set { _isHybrid = value; }
    }

    /// <summary>
    /// boolean indicating if tuner is locked to a signal
    /// </summary>
    public virtual bool IsTunerLocked
    {
      get
      {
        UpdateSignalStatus(true);
        return _tunerLocked;
      }
    }

    /// <summary>
    /// returns the signal quality
    /// </summary>
    public int SignalQuality
    {
      get
      {
        UpdateSignalStatus();
        if (_signalQuality < 0)
          _signalQuality = 0;
        if (_signalQuality > 100)
          _signalQuality = 100;
        return _signalQuality;
      }
    }

    /// <summary>
    /// returns the signal level
    /// </summary>
    public int SignalLevel
    {
      get
      {
        UpdateSignalStatus();
        if (_signalLevel < 0)
          _signalLevel = 0;
        if (_signalLevel > 100)
          _signalLevel = 100;
        return _signalLevel;
      }
    }

    /// <summary>
    /// Resets the signal update.
    /// </summary>
    public void ResetSignalUpdate()
    {
      _lastSignalUpdate = DateTime.MinValue;
    }

    /// <summary>
    /// Gets or sets the context.
    /// </summary>
    /// <value>The context.</value>
    public object Context
    {
      get { return m_context; }
      set { m_context = value; }
    }

    /// <summary>
    /// returns true if card is currently grabbing the epg
    /// </summary>
    public bool IsEpgGrabbing
    {
      get { return _epgGrabbing; }
      set
      {
        UpdateEpgGrabber(value);
        _epgGrabbing = value;
      }
    }

    /// <summary>
    /// returns true if card is currently scanning
    /// </summary>
    public bool IsScanning
    {
      get { return _isScanning; }
      set
      {
        _isScanning = value;
      }
    }

    #endregion

    /// <summary>
    /// Load the <see cref="T:TvLibrary.Interfaces.ICustomDevice"/> plugins for this device.
    /// </summary>
    /// <remarks>
    /// It is expected that this function will be called at some stage during the BDA graph building process.
    /// This function may update the lastFilter reference parameter to insert filters for IAddOnDevice
    /// plugins.
    /// </remarks>
    /// <param name="mainFilter">The main device source filter. Usually a tuner filter.</param>
    /// <param name="graphBuilder">The graph builder to use to insert any additional device filter(s).</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver filter) to
    ///   connect the [first] device filter to.</param>
    protected void LoadPlugins(IBaseFilter mainFilter, ICaptureGraphBuilder2 graphBuilder, ref IBaseFilter lastFilter)
    {
      Log.Log.Debug("TvCardBase: load custom device plugins");

      if (mainFilter == null)
      {
        Log.Log.Debug("TvCardBase: the main filter is null");
        return;
      }
      if (!Directory.Exists("plugins") || !Directory.Exists("plugins\\CustomDevices"))
      {
        Log.Log.Debug("TvCardBase: plugin directory doesn't exist or is not accessible");
        return;
      }

      // Load all available plugins.
      List<ICustomDevice> plugins = new List<ICustomDevice>();
      String[] dllNames = Directory.GetFiles("plugins\\CustomDevices", "*.dll");
      foreach (String dllName in dllNames)
      {
        Assembly dll = Assembly.LoadFrom(dllName);
        Type[] pluginTypes = dll.GetExportedTypes();
        foreach (Type type in pluginTypes)
        {
          if (type.IsClass && !type.IsAbstract)
          {
            Type cdInterface = type.GetInterface("ICustomDevice");
            if (cdInterface != null)
            {
              ICustomDevice plugin = (ICustomDevice)Activator.CreateInstance(type);
              plugins.Add(plugin);
            }
          }
        }
      }

      // There is a well defined loading/checking order for plugins: add-ons, priority, name.
      plugins.Sort(
        delegate(ICustomDevice cd1, ICustomDevice cd2)
        {
          bool cd1IsAddOn = cd1 is IAddOnDevice;
          bool cd2IsAddOn = cd2 is IAddOnDevice;
          if (cd1IsAddOn && !cd2IsAddOn)
          {
            return -1;
          }
          if (cd2IsAddOn && !cd1IsAddOn)
          {
            return 1;
          }
          int priorityCompare = cd2.Priority.CompareTo(cd1.Priority);
          if (priorityCompare != 0)
          {
            return priorityCompare;
          }
          return cd1.Name.CompareTo(cd2.Name);
        }
      );

      // Log the name, priority and capabilities for each plugin, in priority order.
      foreach (ICustomDevice d in plugins)
      {
        Type[] interfaces = d.GetType().GetInterfaces();
        String[] interfaceNames = new String[interfaces.Length];
        for (int i = 0; i < interfaces.Length; i++)
        {
          interfaceNames[i] = interfaces[i].Name;
        }
        Array.Sort(interfaceNames);
        Log.Log.Debug("  {0} [{1} - {2}]: {3}", d.Name, d.Priority, d.GetType().Name, String.Join(", ", interfaceNames));
      }

      Log.Log.Debug("TvCardBase: checking for supported plugins");
      _customDeviceInterfaces = new List<ICustomDevice>();
      foreach (ICustomDevice d in plugins)
      {
        if (!d.Initialise(mainFilter, _cardType, _devicePath))
        {
          d.Dispose();
          continue;
        }

        // The plugin is supported. If the plugin is an add on plugin, we attempt to add it to the graph.
        bool isAddOn = false;
        if (graphBuilder != null && lastFilter != null)
        {
          IAddOnDevice addOn = d as IAddOnDevice;
          if (addOn != null)
          {
            Log.Log.Debug("TvCardBase: add-on plugin found");
            if (!addOn.AddToGraph(graphBuilder, ref lastFilter))
            {
              Log.Log.Debug("TvCardBase: failed to add device filters to graph");
              addOn.Dispose();
              continue;
            }
            isAddOn = true;
          }
        }

        try
        {
          // When we find the main plugin, then we stop searching... but we still need to open the
          // conditional access interface for the add-on *and* the main plugin.
          if (!isAddOn)
          {
            Log.Log.Debug("TvCardBase: primary plugin found");
            break;
          }
        }
        finally
        {
          _customDeviceInterfaces.Add(d);
          if (_useConditionalAccessInterace)
          {
            _conditionalAccessInterface = d as IConditionalAccessProvider;
            if (_conditionalAccessInterface != null)
            {
              _conditionalAccessInterface.OpenInterface();
            }
          }
        }
      }
      if (_customDeviceInterfaces.Count == 0)
      {
        Log.Log.Debug("TvCardBase: no plugins supported");
      }
    }

    /// <summary>
    /// Configure the device's PID filter(s) to enable receiving the PIDs for each of the current subchannels.
    /// </summary>
    protected void ConfigurePidFilter()
    {
      Log.Log.Debug("TvCardBase: configure PID filter");

      if (_cardType == CardType.Analog || _cardType == CardType.RadioWebStream || _cardType == CardType.Unknown)
      {
        Log.Log.Debug("TvCardBase: unsupported device type {0}", _cardType);
        return;
      }
      if (_mapSubChannels == null || _mapSubChannels.Count == 0)
      {
        Log.Log.Debug("TvCardBase: no subchannels");
        return;
      }

      if (_forceDisablePidFilter)
      {
        Log.Log.Debug("TvCardBase: filtering is force-disabled");
      }
      else if (_forceEnablePidFilter)
      {
        Log.Log.Debug("TvCardBase: filtering is force-enabled");
      }

      List<UInt16> pidList = null;
      ModulationType modulation = ModulationType.ModNotDefined;
      bool checkedModulation = false;
      foreach (ICustomDevice d in _customDeviceInterfaces)
      {
        IPidFilterController filter = d as IPidFilterController;
        if (filter != null)
        {
          Log.Log.Debug("TvCardBase: found PID filter controller interface");

          if (_forceDisablePidFilter)
          {
            filter.SetFilterPids(null, modulation, false);
            continue;
          }

          if (pidList == null)
          {
            Log.Log.Debug("TvCardBase: assembling PID list");
            HashSet<UInt16> pidSet = new HashSet<UInt16>();
            pidList = new List<UInt16>();
            foreach (ITvSubChannel subchannel in _mapSubChannels.Values)
            {
              TvDvbChannel dvbChannel = subchannel as TvDvbChannel;
              if (dvbChannel != null && dvbChannel.Pids != null)
              {
                // Figure out the multiplex modulation scheme.
                if (!checkedModulation)
                {
                  checkedModulation = true;
                  ATSCChannel atscChannel = dvbChannel.CurrentChannel as ATSCChannel;
                  if (atscChannel != null)
                  {
                    modulation = atscChannel.ModulationType;
                  }
                  else
                  {
                    DVBSChannel dvbsChannel = dvbChannel.CurrentChannel as DVBSChannel;
                    if (dvbChannel != null)
                    {
                      modulation = dvbsChannel.ModulationType;
                    }
                    else
                    {
                      DVBCChannel dvbcChannel = dvbChannel.CurrentChannel as DVBCChannel;
                      if (dvbcChannel != null)
                      {
                        modulation = dvbcChannel.ModulationType;
                      }
                    }
                  }
                }

                // Build a distinct super-set of PIDs used by the subchannels.
                foreach (UInt16 pid in dvbChannel.Pids)
                {
                  if (!pidSet.Contains(pid))
                  {
                    Log.Log.Debug("TvCardBase:   {0} (0x{1:x})", pid, pid);
                    pidSet.Add(pid);
                    pidList.Add(pid);
                  }
                }
              }
            }
          }
          filter.SetFilterPids(pidList, modulation, _forceEnablePidFilter);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel being tuned or for which updated PMT has been found.</param>
    /// <param name="isUpdate"><c>True</c> if the channel is already being decrypted.</param>
    protected void StartDecrypting(int subChannelId, bool isUpdate)
    {
      Log.Log.Debug("TvCardBase: subchannel {0} start decrypting, mode = {1}, update = {2}", subChannelId, _multiDecryptMode, isUpdate);

      if (_mapSubChannels == null || _mapSubChannels.Count == 0 || !_mapSubChannels.ContainsKey(subChannelId))
      {
        Log.Log.Debug("TvCardBase: subchannel not found");
        return;
      }
      if (_mapSubChannels[subChannelId].CurrentChannel.FreeToAir)
      {
        Log.Log.Debug("TvCardBase: current service is not encrypted");
        return;
      }

      // First build a distinct list of the services that we need to handle.
      Log.Log.Debug("TvCardBase: assembling service list");
      List<BaseSubChannel> distinctServices = new List<BaseSubChannel>();
      if (_multiDecryptMode == CaPmtListManagementAction.Only || _multiDecryptMode == CaPmtListManagementAction.Add)
      {
        // We only manage the service associated with the subchannel.
        distinctServices.Add(_mapSubChannels[subChannelId]);
        Log.Log.Debug("TvCardBase:   {0}", _mapSubChannels[subChannelId].CurrentChannel.Name);
        return;
      }
      else
      {
        Dictionary<int, BaseSubChannel>.ValueCollection.Enumerator en = _mapSubChannels.Values.GetEnumerator();
        while (en.MoveNext())
        {
          IChannel service = en.Current.CurrentChannel;
          // We don't need to decrypt free-to-air channels.
          if (service.FreeToAir)
          {
            continue;
          }

          bool exists = false;
          foreach (BaseSubChannel serviceToDecrypt in distinctServices)
          {
            AnalogChannel analogService = service as AnalogChannel;
            if (analogService != null)
            {
              if (analogService.Frequency == ((AnalogChannel)serviceToDecrypt.CurrentChannel).Frequency &&
                analogService.ChannelNumber == ((AnalogChannel)serviceToDecrypt.CurrentChannel).ChannelNumber)
              {
                exists = true;
                break;
              }
            }
            else
            {
              DVBBaseChannel digitalService = service as DVBBaseChannel;
              if (digitalService != null)
              {
                if (digitalService.ServiceId == ((DVBBaseChannel)serviceToDecrypt.CurrentChannel).ServiceId)
                {
                  exists = true;
                  break;
                }
              }
            }
          }
          if (!exists)
          {
            distinctServices.Add(en.Current);
            Log.Log.Debug("TvCardBase:   {0}", service.Name);
          }
        }
      }

      if (_decryptLimit > 0 && distinctServices.Count > _decryptLimit)
      {
        Log.Log.Debug("TvCardBase: decrypt limit exceeded");
        return;
      }
      if (distinctServices.Count == 0)
      {
        Log.Log.Debug("TvCardBase: no services to decrypt");
        return;
      }

      // Identify the conditional access interface(s) and send the service list.
      bool foundCaProvider = false;
      for (int attempt = 1; attempt <= 3; attempt++)
      {
        if (attempt > 1)
        {
          Log.Log.Debug("TvCardBase: attempt {0}...", attempt);
        }

        foreach (ICustomDevice d in _customDeviceInterfaces)
        {
          IConditionalAccessProvider caProvider = d as IConditionalAccessProvider;
          if (caProvider == null)
          {
            continue;
          }

          Log.Log.Debug("TvCardBase: CA provider {0}...", caProvider.Name);
          foundCaProvider = true;

          if (_waitUntilCaInterfaceReady && !caProvider.IsInterfaceReady())
          {
            Log.Log.Debug("TvCardBase: provider is not ready, waiting for up to 15 seconds", caProvider.Name);
            DateTime startWait = DateTime.Now;
            TimeSpan waitTime = new TimeSpan(0);
            while (waitTime.TotalMilliseconds < 15000)
            {
              System.Threading.Thread.Sleep(200);
              waitTime = DateTime.Now - startWait;
              if (caProvider.IsInterfaceReady())
              {
                Log.Log.Debug("TvCardBase: provider ready after {0} ms", waitTime.TotalMilliseconds);
                break;
              }
            }
          }

          // Ready or not, we send commands now.
          Log.Log.Debug("TvCardBase: sending command(s)");
          bool success = true;
          TvDvbChannel digitalService;
          CaPmtListManagementAction action = CaPmtListManagementAction.More;
          for (int i = 0; i < distinctServices.Count; i++)
          {
            if (i == 0)
            {
              if (distinctServices.Count == 1)
              {
                if (_multiDecryptMode == CaPmtListManagementAction.Add)
                {
                  if (isUpdate)
                  {
                    action = CaPmtListManagementAction.Update;
                  }
                  else
                  {
                    action = CaPmtListManagementAction.Add;
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

            digitalService = distinctServices[i] as TvDvbChannel;
            if (digitalService == null)
            {
              success &= caProvider.SendCommand(distinctServices[i].CurrentChannel, action,
                                                        CaPmtCommand.OkDescrambling, null, null);
            }
            else
            {
              success &= caProvider.SendCommand(distinctServices[i].CurrentChannel, action,
                                                        CaPmtCommand.OkDescrambling, digitalService.Pmt, digitalService.Cat);
            }
          }

          // Are we done?
          if (success)
          {
            return;
          }
        }

        if (!foundCaProvider)
        {
          Log.Log.Debug("TvCardBase: no CA providers identified");
          return;
        }
      }
    }

    #region HelperMethods

    /// <summary>
    /// Gets the first subchannel being used.
    /// </summary>
    /// <value>The current channel.</value>
    private int firstSubchannel
    {
      get
      {
        foreach (int i in _mapSubChannels.Keys)
        {
          if (_mapSubChannels.ContainsKey(i))
          {
            return i;
          }
        }
        return 0;
      }
    }

    /// <summary>
    /// Gets or sets the current channel.
    /// </summary>
    /// <value>The current channel.</value>
    public IChannel CurrentChannel
    {
      get
      {
        if (_mapSubChannels.Count > 0)
        {
          return _mapSubChannels[firstSubchannel].CurrentChannel;
        }
        return null;
      }
      set
      {
        if (_mapSubChannels.Count > 0)
        {
          _mapSubChannels[firstSubchannel].CurrentChannel = value;
        }
      }
    }

    #endregion

    #region virtual methods

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public virtual void BuildGraph() {}

    /// <summary>
    /// Check if the tuner has acquired signal lock.
    /// </summary>
    /// <returns><c>true</c> if the tuner has locked in on signal, otherwise <c>false</c></returns>
    public virtual bool LockedInOnSignal()
    {
      Log.Log.Debug("TvCardBase: check for signal lock");
      _tunerLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!_tunerLocked && ts.TotalSeconds < _parameters.TimeOutTune)
      {
        UpdateSignalStatus(true);
        if (!_tunerLocked)
        {
          ts = DateTime.Now - timeStart;
          Log.Log.Debug("TvCardBase:   waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
      }

      if (!_tunerLocked)
      {
        Log.Log.Debug("TvCardBase: failed to lock signal");
      }
      else
      {
        Log.Log.Debug("TvCardBase: locked");
      }
      return _tunerLocked;
    }

    /// <summary>
    /// Reload the device configuration.
    /// </summary>
    public virtual void ReloadCardConfiguration()
    {
    }

    /// <summary>
    /// Get the device's ITVScanning interface, used for finding channels.
    /// </summary>
    public virtual ITVScanning ScanningInterface
    {
      get
      {
        return null;
      }
    }

    #endregion

    #region abstract methods

    /// <summary>
    /// Update the tuner signal status statistics.
    /// </summary>
    protected virtual void UpdateSignalStatus()
    {
      UpdateSignalStatus(false);
    }

    /// <summary>
    /// Update the tuner signal status statistics.
    /// </summary>
    /// <param name="force"><c>True</c> to force the status to be updated (status information may be cached).</param>
    protected abstract void UpdateSignalStatus(bool force);

    /// <summary>
    /// Stops the current graph
    /// </summary>
    ///     
    public abstract void StopGraph();

    /// <summary>
    /// Pauses the current graph
    /// </summary>
    ///     
    public abstract void PauseGraph();

    /// <summary>
    /// Start the BDA filter graph (subject to a few conditions).
    /// </summary>
    /// <param name="subChannelId">The subchannel ID for the channel that is being started.</param>
    public virtual void RunGraph(int subChannelId)
    {
      Log.Log.Debug("TvCardBase: RunGraph()");

      if (GraphRunning())
      {
        Log.Log.Debug("TvCardBase: graph already running");
      }
      else
      {
        Log.Log.Debug("TvCardBase: start graph");
        int hr = ((IMediaControl)_graphBuilder).Run();
        if (hr < 0 || hr > 1)
        {
          Log.Log.Debug("TvCardBase: failed to start graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
          throw new TvException("TvCardBase: failed to start graph");
        }
      }

      _epgGrabbing = false;
      if (_mapSubChannels.ContainsKey(subChannelId))
      {
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          deviceInterface.OnGraphRunning(this, _mapSubChannels[subChannelId].CurrentChannel);
          IPowerDevice powerDevice = deviceInterface as IPowerDevice;
          if (powerDevice != null)
          {
            powerDevice.SetPowerState(true);
          }
        }
        if (!LockedInOnSignal())
        {
          throw new TvExceptionNoSignal("TvCardBase: failed to lock in on signal");
        }
        _mapSubChannels[subChannelId].AfterTuneEvent -= new BaseSubChannel.OnAfterTuneDelegate(TvCardBase_OnAfterTuneEvent);
        _mapSubChannels[subChannelId].AfterTuneEvent += new BaseSubChannel.OnAfterTuneDelegate(TvCardBase_OnAfterTuneEvent);
        _mapSubChannels[subChannelId].OnGraphRunning();
      }
    }

    /// <summary>
    /// A derrived class may activate / deactivate the epg grabber
    /// </summary>
    /// <param name="value">Mode</param>
    protected virtual void UpdateEpgGrabber(bool value)
    {
    }

    #endregion


    #region scan/tune

    /// <summary>
    /// Check if the tuner can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public abstract bool CanTune(IChannel channel);

    /// <summary>
    /// Scan a specific channel.
    /// </summary>
    /// <param name="subChannelId">The subchannel ID for the channel that is being scanned.</param>
    /// <param name="channel">The channel to scan.</param>
    /// <returns>the subchannel associated with the scanned channel</returns>
    public abstract ITvSubChannel Scan(int subChannelId, IChannel channel);

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The subchannel ID for the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the subchannel associated with the tuned channel</returns>
    public abstract ITvSubChannel Tune(int subChannelId, IChannel channel);

    #endregion

    #region quality control

    /// <summary>
    /// Check if the device supports stream quality control.
    /// </summary>
    /// <value></value>
    public virtual bool SupportsQualityControl
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Get the device's quality control interface.
    /// </summary>
    public virtual IQuality Quality
    {
      get
      {
        return null;
      }
    }

    #endregion

    #region EPG

    /// <summary>
    /// Get the device's ITVEPG interface, used for grabbing electronic program guide data.
    /// </summary>
    public virtual ITVEPG EpgInterface
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Abort grabbing electronic program guide data.
    /// </summary>
    public virtual void AbortGrabbing()
    {
    }

    /// <summary>
    /// Get the electronic program guide data found in a grab session.
    /// </summary>
    /// <value>EPG data if the device supports EPG grabbing and grabbing is complete, otherwise <c>null</c></value>
    public virtual List<EpgChannel> Epg
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Start grabbing electronic program guide data (idle EPG grabber).
    /// </summary>
    /// <param name="callback">The delegate to call when grabbing is complete or canceled.</param>
    public virtual void GrabEpg(BaseEpgGrabber callback)
    {
    }

    /// <summary>
    /// Start grabbing electronic program guide data (timeshifting/recording EPG grabber).
    /// </summary>
    public virtual void GrabEpg()
    {
    }

    #endregion

    #region channel linkages

    /// <summary>
    /// Starts scanning for linkages.
    /// </summary>
    /// <param name="callback">The delegate to call when scanning is complete or canceled.</param>
    public virtual void StartLinkageScanner(BaseChannelLinkageScanner callback)
    {
    }

    /// <summary>
    /// Stop/reset the linkage scanner.
    /// </summary>
    public virtual void ResetLinkageScanner()
    {
    }

    /// <summary>
    /// Get the portal channels found by the linkage scanner.
    /// </summary>
    public virtual List<PortalChannel> ChannelLinkages
    {
      get
      {
        return null;
      }
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public virtual void Dispose()
    {
    }

    #endregion


    #region subchannel management

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="subchannelBusy">is the subcannel busy with other users.</param>
    public virtual void FreeSubChannelContinueGraph(int id, bool subchannelBusy)
    {
      FreeSubChannel(id, true);
    }

    /// <summary>
    /// Frees the sub channel. but keeps the graph running.
    /// </summary>
    /// <param name="id">Handle to the subchannel.</param>
    public virtual void FreeSubChannelContinueGraph(int id)
    {
      // this method is overriden in tvcardbase      
      FreeSubChannel(id, true);
    }

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">Handle to the subchannel.</param>
    public virtual void FreeSubChannel(int id)
    {
      FreeSubChannel(id, false);
    }

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">Handle to the subchannel.</param>
    /// <param name="continueGraph">Indicates, if the graph should be continued or stopped</param>
    private void FreeSubChannel(int id, bool continueGraph)
    {
      Log.Log.Info("tvcard:FreeSubChannel: subchannels count {0} subch#{1} keep graph={2}", _mapSubChannels.Count, id,
                   continueGraph);
      if (_mapSubChannels.ContainsKey(id))
      {
        if (_mapSubChannels[id].IsTimeShifting)
        {
          Log.Log.Info("tvcard:FreeSubChannel :{0} - is timeshifting (skipped)", id);
          return;
        }

        if (_mapSubChannels[id].IsRecording)
        {
          Log.Log.Info("tvcard:FreeSubChannel :{0} - is recording (skipped)", id);
          return;
        }

        try
        {
          _mapSubChannels[id].Decompose();
        }
        finally
        {
          _mapSubChannels.Remove(id);
        }


        /*if (_conditionalAccess != null)
        {         
          Log.Log.Info("tvcard:FreeSubChannel CA:{0}", id);
          _conditionalAccess.FreeSubChannel(id);
        }*/
      }
      else
      {
        Log.Log.Info("tvcard:FreeSubChannel :{0} - sub channel not found", id);
      }
      if (_mapSubChannels.Count == 0)
      {
        _subChannelId = 0;
        if (!continueGraph)
        {
          Log.Log.Info("tvcard:FreeSubChannel : no subchannels present, pausing graph");
          if (SupportsPauseGraph)
          {
            PauseGraph();
          }
          else
          {
            StopGraph();
          }
        }
        else
        {
          Log.Log.Info("tvcard:FreeSubChannel : no subchannels present, continueing graph");
        }
      }
      else
      {
        Log.Log.Info("tvcard:FreeSubChannel : subchannels STILL present {}, continueing graph", _mapSubChannels.Count);
      }
    }

    /// <summary>
    /// Frees all sub channels.
    /// </summary>
    protected void FreeAllSubChannels()
    {
      Log.Log.Info("tvcard:FreeAllSubChannels");
      Dictionary<int, BaseSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
      while (en.MoveNext())
      {
        en.Current.Value.Decompose();
      }
      _mapSubChannels.Clear();
      _subChannelId = 0;
    }

    /// <summary>
    /// Gets the sub channel.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns></returns>
    public ITvSubChannel GetSubChannel(int id)
    {
      if (_mapSubChannels != null && _mapSubChannels.ContainsKey(id))
      {
        return _mapSubChannels[id];
      }
      return null;
    }

    /// <summary>
    /// Gets the sub channels.
    /// </summary>
    /// <value>The sub channels.</value>
    public ITvSubChannel[] SubChannels
    {
      get
      {
        int count = 0;
        ITvSubChannel[] channels = new ITvSubChannel[_mapSubChannels.Count];
        Dictionary<int, BaseSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          channels[count++] = en.Current.Value;
        }
        return channels;
      }
    }

    #endregion
  }
}