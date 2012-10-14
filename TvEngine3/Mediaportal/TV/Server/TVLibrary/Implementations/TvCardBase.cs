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
using System.Linq;
using System.Reflection;
using DirectShowLib;
using DirectShowLib.BDA;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// Base class for all devices.
  /// </summary>
  public abstract class TvCardBase : ITVCard
  {    

    #region events

    /// <summary>
    /// New subchannel observer event, fired when a new subchannel is created.
    /// </summary>
    public event OnNewSubChannelDelegate NewSubChannelEvent;

    /// <summary>
    /// Set the device's new subchannel event handler.
    /// </summary>
    /// <value>the delegate</value>
    public OnNewSubChannelDelegate OnNewSubChannelEvent
    {
      set
      {
        NewSubChannelEvent += value;
      }
    }

    /// <summary>
    /// Fire the new subchannel observer event.
    /// </summary>
    /// <param name="subChannelId">The ID of the new subchannel.</param>
    protected void FireNewSubChannelEvent(int subChannelId)
    {
      if (NewSubChannelEvent != null)
      {
        NewSubChannelEvent(subChannelId);
      }
    }

    /// <summary>
    /// After tune observer event, fired after tuning is complete.
    /// </summary>    
    public event OnAfterTuneDelegate AfterTuneEvent;

    /// <summary>
    /// Set the device's after tune event handler.
    /// </summary>
    /// <value>the delegate</value>
    public OnAfterTuneDelegate OnAfterTuneEvent
    {
      set
      {
        AfterTuneEvent -= value;
        AfterTuneEvent += value;
      }
    }

    /// <summary>
    /// Fire the after tune observer event.
    /// </summary>
    private void FireAfterTuneEvent()
    {
      if (AfterTuneEvent != null)
      {
        AfterTuneEvent();
      }
    }

    #endregion

    #region variables

    /// <summary>
    /// Indicates if the card should be preloaded.
    /// </summary>
    protected bool _preloadCard;

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
    /// The graph builder
    /// </summary>
    protected IFilterGraph2 _graphBuilder;

    /// <summary>
    /// Indicates, if the card sub channels
    /// </summary>
    protected bool _supportsSubChannels;

    /// <summary>
    /// The tuner type (eg. DVB-S, DVB-T... etc.).
    /// </summary>
    protected CardType _tunerType;

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
    /// The action that will be taken when the device is no longer being actively used.
    /// </summary>
    protected DeviceIdleMode _idleMode = DeviceIdleMode.Stop;

    /// <summary>
    /// An indicator: has the device been initialised? For most devices this indicates whether the DirectShow/BDA
    /// filter graph has been built.
    /// </summary>
    protected bool _isDeviceInitialised = false;

    /// <summary>
    /// A list containing the custom device interfaces supported by this device. The list is ordered by
    /// interface priority.
    /// </summary>
    protected List<ICustomDevice> _customDeviceInterfaces = null;

    /// <summary>
    /// Enable or disable the use of conditional access interface(s).
    /// </summary>
    protected bool _useConditionalAccessInterace = true;

    /// <summary>
    /// The type of conditional access module available to the conditional access interface.
    /// </summary>
    /// <remarks>
    /// Certain conditional access modules require specific handling to ensure compatibility.
    /// </remarks>
    protected CamType _camType = CamType.Default;

    /// <summary>
    /// The number of channels that the device is capable of or permitted to decrypt simultaneously. Zero means
    /// there is no limit.
    /// </summary>
    protected int _decryptLimit = 0;

    /// <summary>
    /// The method that should be used to communicate the set of channels that the device's conditional access
    /// interface needs to manage.
    /// </summary>
    /// <remarks>
    /// Multi-channel decrypt is *not* the same as Digital Devices' multi-transponder decrypt (MTD). MCD is a
    /// implmented using standard CA PMT commands; MTD is implemented in the Digital Devices drivers.
    /// Disabled = Always send Only. In most cases this will result in only one channel being decrypted. If other
    ///         methods are not working reliably then this one should at least allow decrypting one channel
    ///         reliably.
    /// List = Explicit management using Only, First, More and Last. This is the most widely supported set
    ///         of commands, however they are not suitable for some interfaces (such as the Digital Devices
    ///         interface).
    /// Changes = Use Add, Update and Remove to pass changes to the interface. The full channel list is never
    ///         passed. Most interfaces don't support these commands.
    /// </remarks>
    protected MultiChannelDecryptMode _multiChannelDecryptMode = MultiChannelDecryptMode.List;

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
    /// The mode to use for controlling device PID filter(s).
    /// </summary>
    /// <remarks>
    /// This setting can be used to enable or disable the device's PID filter even when the tuning context
    /// (for example, DVB-S vs. DVB-S2) would usually result in different behaviour. Note that it is usually
    /// not ideal to have to manually enable or disable a PID filter as it can affect tuning reliability.
    /// </remarks>
    protected PidFilterMode _pidFilterMode = PidFilterMode.Auto;

    /// <summary>
    /// The previous channel that the device was tuned to. This variable is reset each time the device
    /// is stopped, paused or reset.
    /// </summary>
    protected IChannel _previousChannel = null;

    /// <summary>
    /// Enable or disable the use of custom device interfaces for tuning.
    /// </summary>
    /// <remarks>
    /// Custom/direct tuning may be faster or more reliable than regular tuning methods. Equally, it can
    /// also be slower (eg. TeVii) or more limiting (eg. Digital Everywhere) than regular tuning methods.
    /// </remarks>
    protected bool _useCustomTuning = false;

    /// <summary>
    /// A flag used by the TV service as a signal to abort the tuning process before it is completed.
    /// </summary>
    protected bool _cancelTune = false;

    #endregion

    #region ctor

    ///<summary>
    /// Base constructor
    ///</summary>
    ///<param name="device">Base DS device</param>
    protected TvCardBase(DsDevice device)
    {
      _isDeviceInitialised = false;
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
        Card c = CardManagement.GetCardByDevicePath(_devicePath, CardIncludeRelationEnum.None);
        if (c != null)
        {
          _cardId = c.IdCard;
          _name = c.Name;   // We prefer to use the name that can be set in TV Server configuration for more readable logs...
          _preloadCard = c.PreloadCard;
          _idleMode = (DeviceIdleMode)c.IdleMode;
          _pidFilterMode = (PidFilterMode)c.PidFilterMode;
          _useCustomTuning = c.UseCustomTuning;

          // Conditional access...
          _useConditionalAccessInterace = c.UseConditionalAccess;
          _camType = (CamType)c.CamType;
          _decryptLimit = c.DecryptLimit;
          _multiChannelDecryptMode = (MultiChannelDecryptMode)c.MultiChannelDecryptMode;
        }
      }
    }

    #endregion

    #region properties

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
    /// <value>A set of timeout parameters used for tuning and scanning.</value>
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
      get { return _tunerType; }
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
    /// that the interface is able to decrypt simultaneously. A value of zero indicates that the limit is
    /// to be ignored.
    /// </summary>
    public int DecryptLimit
    {
      get
      {
        return _decryptLimit;
      }
    }

    /// <summary>
    /// Does the device support conditional access?
    /// </summary>
    /// <value><c>true</c> if the device supports conditional access, otherwise <c>false</c></value>
    public bool IsConditionalAccessSupported
    {
      get
      {
        if (!_useConditionalAccessInterace)
        {
          return false;
        }
        // Return true if any interface implements IConditionalAccessProvider.
        foreach (ICustomDevice d in _customDeviceInterfaces)
        {
          if (d is IConditionalAccessProvider)
          {
            return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Get the device's conditional access menu interaction interface. This interface is only applicable if
    /// conditional access is supported.
    /// </summary>
    /// <value><c>null</c> if the device does not support conditional access</value>
    public ICiMenuActions CaMenuInterface
    {
      get
      {
        if (!_useConditionalAccessInterace)
        {
          return null;
        }
        // Return the first interface that implements ICiMenuActions.
        foreach (ICustomDevice d in _customDeviceInterfaces)
        {
          ICiMenuActions caMenuInterface = d as ICiMenuActions;
          if (caMenuInterface != null)
          {
            return caMenuInterface;
          }
        }
        return null;
      }
    }

    /// <summary>
    /// Get a count of the number of services that the device is currently decrypting.
    /// </summary>
    /// <value>The number of services currently being decrypted.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        // If not decrypting any channels or the limit is diabled then return zero.
        if (_mapSubChannels == null || _mapSubChannels.Count == 0 || _decryptLimit == 0)
        {
          return 0;
        }

        HashSet<long> decryptedServices = new HashSet<long>();
        Dictionary<int, BaseSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          IChannel service = en.Current.Value.CurrentChannel;
          DVBBaseChannel digitalService = service as DVBBaseChannel;
          if (digitalService != null)
          {
            if (!decryptedServices.Contains(digitalService.ServiceId))
            {
              decryptedServices.Add(digitalService.ServiceId);
            }
          }
          else
          {
            AnalogChannel analogService = service as AnalogChannel;
            if (analogService != null)
            {
              if (!decryptedServices.Contains(analogService.Frequency))
              {
                decryptedServices.Add(analogService.Frequency);
              }
            }
            else
            {
              throw new TvException("TvCardBase: service type not recognised, unable to count number of services being decrypted\r\n" + service.ToString());
            }
          }
        }

        return decryptedServices.Count;
      }
    }

    #endregion

    /// <summary>
    /// Get the device's DiSEqC control interface. This interface is only applicable for satellite tuners.
    /// It is used for controlling switch, positioner and LNB settings.
    /// </summary>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner does not support sending/receiving
    /// DiSEqC commands</value>
    public virtual IDiseqcController DiseqcController
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
    /// Check if the BDA filter graph is running.
    /// </summary>
    /// <returns><c>true</c> if the graph is running, otherwise <c>false</c></returns>
    protected bool GraphRunning()
    {
      bool graphRunning = false;

      if (_graphBuilder != null)
      {
        FilterState state;
        ((IMediaControl)_graphBuilder).GetState(10, out state);
        graphRunning = (state == FilterState.Running);
      }
      return graphRunning;
    }

    /// <summary>
    /// Load the <see cref="T:TvLibrary.Interfaces.ICustomDevice"/> plugins for this device.
    /// </summary>
    /// <remarks>
    /// It is expected that this function will be called at some stage during the DirectShow graph building process.
    /// This function may update the lastFilter reference parameter to insert filters for IAddOnDevice
    /// plugins.
    /// </remarks>
    /// <param name="mainFilter">The main device source filter. Usually a tuner filter.</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver filter) to
    ///   connect the [first] device filter to.</param>
    protected void LoadPlugins(IBaseFilter mainFilter, ref IBaseFilter lastFilter)
    {
      Log.Debug("TvCardBase: load custom device plugins");

      if (mainFilter == null)
      {
        Log.Debug("TvCardBase: the main filter is null");
        return;
      }
      if (!Directory.Exists("plugins") || !Directory.Exists("plugins\\CustomDevices"))
      {
        Log.Debug("TvCardBase: plugin directory doesn't exist or is not accessible");
        return;
      }

      // Load all available and compatible plugins.
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
              if (CompatibilityManager.IsPluginCompatible(type))
              {
                ICustomDevice plugin = (ICustomDevice)Activator.CreateInstance(type);
                plugins.Add(plugin);
              }
              else
              {
                Log.Debug("TvCardBase: skipping incompatible plugin \"{0}\" ({1})", type.Name, dllName);
              }
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
        Log.Debug("  {0} [{1} - {2}]: {3}", d.Name, d.Priority, d.GetType().Name, String.Join(", ", interfaceNames));
      }

      Log.Debug("TvCardBase: checking for supported plugins");
      _customDeviceInterfaces = new List<ICustomDevice>();
      foreach (ICustomDevice d in plugins)
      {
        if (!d.Initialise(mainFilter, _tunerType, _devicePath))
        {
          d.Dispose();
          continue;
        }

        // The plugin is supported. If the plugin is an add on plugin, we attempt to add it to the graph.
        bool isAddOn = false;
        if (lastFilter != null)
        {
          IAddOnDevice addOn = d as IAddOnDevice;
          if (addOn != null)
          {
            Log.Debug("TvCardBase: add-on plugin found");
            if (!addOn.AddToGraph(ref lastFilter))
            {
              Log.Debug("TvCardBase: failed to add device filters to graph");
              addOn.Dispose();
              continue;
            }
            isAddOn = true;
          }
        }

        try
        {
          // When we find the main plugin, then we stop searching...
          if (!isAddOn)
          {
            Log.Debug("TvCardBase: primary plugin found");
            break;
          }
        }
        finally
        {
          _customDeviceInterfaces.Add(d);
        }
      }
      if (_customDeviceInterfaces.Count == 0)
      {
        Log.Debug("TvCardBase: no plugins supported");
      }
    }

    /// <summary>
    /// Open any <see cref="T:TvLibrary.Interfaces.ICustomDevice"/> plugins loaded for this device by LoadPlugins().
    /// </summary>
    /// <remarks>
    /// We separate this from the loading because some plugins (for example, the NetUP plugin) can't be opened
    /// until the graph has finished being built.
    /// </remarks>
    protected void OpenPlugins()
    {
      Log.Debug("TvCardBase: open custom device plugins");
      if (_useConditionalAccessInterace)
      {
        foreach (ICustomDevice plugin in _customDeviceInterfaces)
        {
          IConditionalAccessProvider caProvider = plugin as IConditionalAccessProvider;
          if (caProvider != null)
          {
            caProvider.OpenInterface();
          }
        }
      }
    }

    /// <summary>
    /// Configure the device's PID filter(s) to enable receiving the PIDs for each of the current subchannels.
    /// </summary>
    protected void ConfigurePidFilter()
    {
      Log.Debug("TvCardBase: configure PID filter, mode = {0}", _pidFilterMode);

      if (_tunerType == CardType.Analog || _tunerType == CardType.RadioWebStream || _tunerType == CardType.Unknown)
      {
        Log.Debug("TvCardBase: unsupported device type {0}", _tunerType);
        return;
      }
      if (_mapSubChannels == null || _mapSubChannels.Count == 0)
      {
        Log.Debug("TvCardBase: no subchannels");
        return;
      }

      HashSet<UInt16> pidSet = null;
      ModulationType modulation = ModulationType.ModNotDefined;
      bool checkedModulation = false;
      foreach (ICustomDevice d in _customDeviceInterfaces)
      {
        IPidFilterController filter = d as IPidFilterController;
        if (filter != null)
        {
          Log.Debug("TvCardBase: found PID filter controller interface");

          if (_pidFilterMode == PidFilterMode.Disabled)
          {
            filter.SetFilterPids(null, modulation, false);
            continue;
          }

          if (pidSet == null)
          {
            Log.Debug("TvCardBase: assembling PID list");
            pidSet = new HashSet<UInt16>();
            int count = 1;
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
                    if (dvbsChannel != null)
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
                    Log.Debug("  {0,-2} = {1} (0x{1:x})", count++, pid);
                    pidSet.Add(pid);
                  }
                }
              }
            }
          }
          filter.SetFilterPids(pidSet, modulation, _pidFilterMode == PidFilterMode.Enabled);
        }
      }
    }

    /// <summary>
    /// Update the list of services being decrypted by the device's conditional access interfaces(s).
    /// </summary>
    /// <remarks>
    /// The strategy here is usually to only send commands to the CAM when we need an *additional* service
    /// to be decrypted. The *only* exception is when we have to stop decrypting services in "changes" mode.
    /// We don't send "not selected" commands for "list" or "only" mode because this can disrupt the other
    /// services that still need to be decrypted. We also don't send "keep decrypting" commands (alternative
    /// to "not selected") because that will almost certainly cause glitches in streams.
    /// </remarks>
    /// <param name="subChannelId">The ID of the subchannel causing this update.</param>
    /// <param name="updateAction"><c>Add</c> if the subchannel is being tuned, <c>update</c> if the PMT for the
    ///   subchannel has changed, or <c>last</c> if the subchannel is being disposed.</param>
    protected void UpdateDecryptList(int subChannelId, CaPmtListManagementAction updateAction)
    {
      Log.Debug("TvCardBase: subchannel {0} update decrypt list, mode = {1}, update action = {2}", subChannelId, _multiChannelDecryptMode, updateAction);

      if (_mapSubChannels == null || _mapSubChannels.Count == 0 || !_mapSubChannels.ContainsKey(subChannelId))
      {
        Log.Debug("TvCardBase: subchannel not found");
        return;
      }
      if (_mapSubChannels[subChannelId].CurrentChannel.FreeToAir)
      {
        Log.Debug("TvCardBase: service is not encrypted");
        return;
      }
      if (updateAction == CaPmtListManagementAction.Last && _multiChannelDecryptMode != MultiChannelDecryptMode.Changes)
      {
        Log.Debug("TvCardBase: \"not selected\" command acknowledged, no action required");
        return;
      }

      // First build a distinct list of the services that we need to handle.
      Log.Debug("TvCardBase: assembling service list");
      List<BaseSubChannel> distinctServices = new List<BaseSubChannel>();
      Dictionary<int, BaseSubChannel>.ValueCollection.Enumerator en = _mapSubChannels.Values.GetEnumerator();
      DVBBaseChannel updatedDigitalService = _mapSubChannels[subChannelId].CurrentChannel as DVBBaseChannel;
      AnalogChannel updatedAnalogService = _mapSubChannels[subChannelId].CurrentChannel as AnalogChannel;
      while (en.MoveNext())
      {
        IChannel service = en.Current.CurrentChannel;
        // We don't care about FTA services here.
        if (service.FreeToAir)
        {
          continue;
        }

        // Keep an eye out - if there is another subchannel accessing the same service as the subchannel that
        // is being updated then we always do *nothing* unless this is specifically an update request. In any other
        // situation, if we were to stop decrypting the service it would be wrong; if we were to start decrypting
        // the service it would be unnecessary and possibly cause stream interruptions.
        if (en.Current.SubChannelId != subChannelId && updateAction != CaPmtListManagementAction.Update)
        {
          if (updatedDigitalService != null)
          {
            DVBBaseChannel digitalService = service as DVBBaseChannel;
            if (digitalService != null && digitalService.ServiceId == updatedDigitalService.ServiceId)
            {
              Log.Debug("TvCardBase: the service for this subchannel is a duplicate, no action required");
              return;
            }
          }
          else if (updatedAnalogService != null)
          {
            AnalogChannel analogService = service as AnalogChannel;
            if (analogService != null && analogService.Frequency == updatedAnalogService.Frequency && analogService.ChannelNumber == updatedAnalogService.ChannelNumber)
            {
              Log.Debug("TvCardBase: the service for this subchannel is a duplicate, no action required");
              return;
            }
          }
          else
          {
            throw new TvException("TvCardBase: service type not recognised, unable to assemble decrypt service list\r\n" + service.ToString());
          }
        }

        if (_multiChannelDecryptMode == MultiChannelDecryptMode.List)
        {
          // Check for "list" mode: have we already go this service in our distinct list? If so, don't add it
          // again...
          bool exists = false;
          foreach (BaseSubChannel serviceToDecrypt in distinctServices)
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
            else
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
                throw new TvException("TvCardBase: service type not recognised, unable to assemble decrypt service list\r\n" + service.ToString());
              }
            }
          }
          if (!exists)
          {
            distinctServices.Add(en.Current);
          }
        }
        else if (en.Current.SubChannelId == subChannelId)
        {
          // For "changes" and "only" modes: we only send one command and that relates to the service being updated.
          distinctServices.Add(en.Current);
        }
      }

      // This should never happen, regardless of the action that is being performed. Note that this is just a
      // sanity check. It is expected that the service will manage decrypt limit logic. This check does not work
      // for "changes" mode.
      if (_decryptLimit > 0 && distinctServices.Count > _decryptLimit)
      {
        Log.Debug("TvCardBase: decrypt limit exceeded");
        return;
      }
      if (distinctServices.Count == 0)
      {
        Log.Debug("TvCardBase: no services to update");
        return;
      }

      // Identify the conditional access interface(s) and send the service list.
      bool foundCaProvider = false;
      for (int attempt = 1; attempt <= _decryptFailureRetryCount + 1; attempt++)
      {
        ThrowExceptionIfTuneCancelled();
        if (attempt > 1)
        {
          Log.Debug("TvCardBase: attempt {0}...", attempt);
        }

        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          IConditionalAccessProvider caProvider = deviceInterface as IConditionalAccessProvider;
          if (caProvider == null)
          {
            continue;
          }

          Log.Debug("TvCardBase: CA provider {0}...", caProvider.Name);
          foundCaProvider = true;

          if (_waitUntilCaInterfaceReady && !caProvider.IsInterfaceReady())
          {
            Log.Debug("TvCardBase: provider is not ready, waiting for up to 15 seconds", caProvider.Name);
            DateTime startWait = DateTime.Now;
            TimeSpan waitTime = new TimeSpan(0);
            while (waitTime.TotalMilliseconds < 15000)
            {
              ThrowExceptionIfTuneCancelled();
              System.Threading.Thread.Sleep(200);
              waitTime = DateTime.Now - startWait;
              if (caProvider.IsInterfaceReady())
              {
                Log.Debug("TvCardBase: provider ready after {0} ms", waitTime.TotalMilliseconds);
                break;
              }
            }
          }

          // Ready or not, we send commands now.
          Log.Debug("TvCardBase: sending command(s)");
          bool success = false;
          TvDvbChannel digitalService;
          // The default action is "more" - this will be changed below if necessary.
          CaPmtListManagementAction action = CaPmtListManagementAction.More;

          // The command is "start/continue descrambling" unless we're removing services.
          CaPmtCommand command = CaPmtCommand.OkDescrambling;
          if (updateAction == CaPmtListManagementAction.Last)
          {
            command = CaPmtCommand.NotSelected;
          }
          for (int i = 0; i < distinctServices.Count; i++)
          {
            ThrowExceptionIfTuneCancelled();
            if (i == 0)
            {
              if (distinctServices.Count == 1)
              {
                if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
                {
                  // Remove a service...
                  if (updateAction == CaPmtListManagementAction.Last)
                  {
                    action = CaPmtListManagementAction.Only;
                  }
                  // Add or update a service...
                  else
                  {
                    action = updateAction;
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

            Log.Debug("  command = {0}, action = {1}, service = {2}", command, action, distinctServices[i].CurrentChannel.Name);
            digitalService = distinctServices[i] as TvDvbChannel;
            if (digitalService == null)
            {
              success = caProvider.SendCommand(distinctServices[i].CurrentChannel, action, command, null, null);
            }
            else
            {
              success = caProvider.SendCommand(distinctServices[i].CurrentChannel, action, command, digitalService.Pmt, digitalService.Cat);
            }
            // Are we done?
            if (success)
            {
              return;
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
          Log.Debug("TvCardBase: no CA providers identified");
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
    /// <remarks>
    /// This property should *never* be used or thought of at a service level. Rather, it is only useful for
    /// transponder/multiplex tuning details such as frequency and modulation that are common to all subchannels.
    /// </remarks>
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

    #region abstract and virtual methods

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public virtual void BuildGraph() { }

    /// <summary>
    /// Wait for the tuner to acquire signal lock.
    /// </summary>
    public void LockInOnSignal()
    {
      Log.Debug("TvCardBase: lock in on signal");
      _tunerLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!_tunerLocked && ts.TotalSeconds < _parameters.TimeOutTune)
      {
        ThrowExceptionIfTuneCancelled();
        UpdateSignalStatus(true);
        if (!_tunerLocked)
        {
          ts = DateTime.Now - timeStart;
          Log.Debug("  waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
      }

      if (!_tunerLocked)
      {
        throw new TvExceptionNoSignal("TvCardBase: failed to lock signal");
      }

      Log.Debug("TvCardBase: locked");
    }

    /// <summary>
    /// Reload the device configuration.
    /// </summary>
    public virtual void ReloadCardConfiguration()
    {
    }

    /// <summary>
    /// Get the device's channel scanning interface.
    /// </summary>
    public virtual ITVScanning ScanningInterface
    {
      get
      {
        return null;
      }
    }

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
    /// Stop the device. The actual result of this function depends on device configuration.
    /// </summary>
    public virtual void Stop()
    {
      Log.Debug("TvCardBase: stop, idle mode = {0}", _idleMode);
      try
      {
        UpdateEpgGrabber(false);  // Stop grabbing EPG.
        _isScanning = false;
        FreeAllSubChannels();

        DeviceAction action = DeviceAction.Stop;
        switch (_idleMode)
        {
          case DeviceIdleMode.Pause:
            action = DeviceAction.Pause;
            break;
          case DeviceIdleMode.Unload:
            action = DeviceAction.Unload;
            break;
          case DeviceIdleMode.AlwaysOn:
            action = DeviceAction.Start;
            break;
        }

        // Plugins may want to prevent or direct actions to ensure compatibility and smooth device operation.
        DeviceAction pluginAction = action;
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          deviceInterface.OnStop(this, ref pluginAction);
          if (pluginAction > action)
          {
            Log.Debug("TvCardBase: plugin \"{0}\" overrides action {1} with {2}", deviceInterface.Name, action, pluginAction);
            action = pluginAction;
          }
          else if (action != pluginAction)
          {
            Log.Debug("TvCardBase: plugin \"{0}\" wants to perform action {1}, overriden", deviceInterface.Name, pluginAction);
          }
        }

        PerformDeviceAction(action);

        // Turn off the device power.
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          IPowerDevice powerDevice = deviceInterface as IPowerDevice;
          if (powerDevice != null)
          {
            powerDevice.SetPowerState(false);
          }
        }
      }
      finally
      {
        // One potential reason for getting here is that signal could not be locked, and the reason for
        // that may be that tuning failed. We always want to force a full retune on the next tune request
        // in this situation.
        _previousChannel = null;
      }
    }

    /// <summary>
    /// Perform a specific device action. For example, stop the device.
    /// </summary>
    /// <param name="action">The action to perform with the device.</param>
    protected virtual void PerformDeviceAction(DeviceAction action)
    {
      Log.Debug("TvCardBase: perform device action, action = {0}", action);
      try
      {
        if (action == DeviceAction.Reset)
        {
          // TODO: this should work, but it would be better to have Dispose() as final and Decompose() or
          // some other alternative for resetting.
          Dispose();
          BuildGraph();
        }
        else if (action == DeviceAction.Unload)
        {
          Dispose();
        }
        else
        {
          if (_graphBuilder == null)
          {
            Log.Debug("TvCardBase: graphbuilder is null");
            return;
          }

          if (action == DeviceAction.Pause)
          {
            SetGraphState(FilterState.Paused);
          }
          else if (action == DeviceAction.Stop)
          {
            SetGraphState(FilterState.Stopped);
          }
          else if (action == DeviceAction.Start)
          {
            SetGraphState(FilterState.Running);
          }
          else if (action == DeviceAction.Restart)
          {
            SetGraphState(FilterState.Stopped);
            SetGraphState(FilterState.Running);
          }
          else
          {
            Log.Debug("TvCardBase: unhandled action");
            return;
          }
        }

        Log.Debug("TvCardBase: action succeeded");
      }
      catch (Exception ex)
      {
        Log.Debug("TvCardBase: action failed\r\n" + ex.ToString());
      }
    }

    /// <summary>
    /// Set the state of the DirectShow/BDA filter graph.
    /// </summary>
    /// <param name="state">The state to put the filter graph in.</param>
    protected virtual void SetGraphState(FilterState state)
    {
      Log.Debug("TvCardBase: set graph state, state = {0}", state);

      // Get current state.
      FilterState currentState;
      ((IMediaControl)_graphBuilder).GetState(10, out currentState);
      Log.Debug("  current state = {0}", currentState);
      if (state == currentState)
      {
        Log.Debug("TvCardBase: graph already in required state");
        return;
      }
      int hr = 0;
      if (state == FilterState.Stopped)
      {
        hr = ((IMediaControl)_graphBuilder).Stop();
      }
      else if (state == FilterState.Paused)
      {
        hr = ((IMediaControl)_graphBuilder).Pause();
      }
      else
      {
        hr = ((IMediaControl)_graphBuilder).Run();
      }
      if (hr < 0 || hr > 1)
      {
        Log.Error("TvCardBase: failed to perform action, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        throw new TvException("TvCardBase: failed to set graph state");
      }
    }

    #endregion

    #region scan/tune

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public abstract bool CanTune(IChannel channel);

    /// <summary>
    /// Scan a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being scanned.</param>
    /// <param name="channel">The channel to scan.</param>
    /// <returns>the subchannel associated with the scanned channel</returns>
    public virtual ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      return Tune(subChannelId, channel);
    }

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the subchannel associated with the tuned channel</returns>
    public virtual ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Debug("TvCardBase: tune channel, {0}", channel);
      try
      {
        // The DirectShow/BDA graph needs to be assembled before the channel can be tuned.
        if (!_isDeviceInitialised)
        {
          BuildGraph();
          ThrowExceptionIfTuneCancelled();
        }

        // Get a subchannel for the service.
        if (!_mapSubChannels.ContainsKey(subChannelId))
        {
          Log.Debug("TvCardBase: creating new subchannel");
          subChannelId = CreateNewSubChannel(channel);
        }
        else
        {
          Log.Debug("TvCardBase: using existing subchannel");
          // If reusing a subchannel and our multi-channel decrypt mode is "changes", tell the plugin to stop
          // decrypting the previous service before we lose access to the PMT and CAT.
          if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
          {
            UpdateDecryptList(subChannelId, CaPmtListManagementAction.Last);
          }
        }
        Log.Info("TvCardBase: subchannel ID = {0}, subchannel count = {1}", subChannelId, _mapSubChannels.Count);
        _mapSubChannels[subChannelId].CurrentChannel = channel;

        // Subchannel OnBeforeTune().
        _mapSubChannels[subChannelId].OnBeforeTune();

        // Do we need to tune?
        if (_previousChannel == null || _previousChannel.IsDifferentTransponder(channel))
        {
          // Stop the EPG grabber. We're going to move to a different channel so any EPG data that has been
          // grabbed for the previous channel should be stored.
          UpdateEpgGrabber(false);

          // When we call ICustomDevice.OnBeforeTune(), the ICustomDevice may modify the tuning parameters.
          // However, the original channel object *must not* be modified otherwise IsDifferentTransponder()
          // will sometimes returns true when it shouldn't. See mantis 0002979.
          IChannel tuneChannel = channel.GetTuningChannel();

          // Plugin OnBeforeTune().
          DeviceAction action = DeviceAction.Default;
          foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
          {
            DeviceAction pluginAction;
            deviceInterface.OnBeforeTune(this, _previousChannel, ref tuneChannel, out pluginAction);
            if (pluginAction != DeviceAction.Unload && pluginAction != DeviceAction.Default)
            {
              // Valid action requested...
              if (pluginAction > action)
              {
                Log.Debug("TvCardBase: plugin \"{0}\" overrides action {1} with {2}", deviceInterface.Name, action, pluginAction);
                action = pluginAction;
              }
              else if (pluginAction != action)
              {
                Log.Debug("TvCardBase: plugin \"{0}\" wants to perform action {1}, overriden", deviceInterface.Name, pluginAction);
              }
            }

            // Turn on device power. This usually needs to happen before tuning.
            IPowerDevice powerDevice = deviceInterface as IPowerDevice;
            if (powerDevice != null)
            {
              powerDevice.SetPowerState(true);
            }
          }
          ThrowExceptionIfTuneCancelled();
          if (action != DeviceAction.Default)
          {
            PerformDeviceAction(action);
          }

          // Apply tuning parameters to the device.
          ThrowExceptionIfTuneCancelled();
          PerformTuning(tuneChannel);
          ThrowExceptionIfTuneCancelled();

          // Plugin OnAfterTune().
          foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
          {
            deviceInterface.OnAfterTune(this, channel);
          }
        }

        // Subchannel OnAfterTune().
        _lastSignalUpdate = DateTime.MinValue;
        _mapSubChannels[subChannelId].OnAfterTune();

        _previousChannel = channel;

        // Start the DirectShow/BDA graph if it is not already running.
        ThrowExceptionIfTuneCancelled();
        SetGraphState(FilterState.Running);
        ThrowExceptionIfTuneCancelled();

        // Ensure that data/streams which are required to detect the service will pass through the device's
        // PID filter.
        ConfigurePidFilter();
        ThrowExceptionIfTuneCancelled();

        // Plugin OnRunning().
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          deviceInterface.OnRunning(this, _mapSubChannels[subChannelId].CurrentChannel);
        }

        LockInOnSignal();

        // Subchannel OnGraphRunning().
        _mapSubChannels[subChannelId].AfterTuneEvent -= new BaseSubChannel.OnAfterTuneDelegate(FireAfterTuneEvent);
        _mapSubChannels[subChannelId].AfterTuneEvent += new BaseSubChannel.OnAfterTuneDelegate(FireAfterTuneEvent);
        _mapSubChannels[subChannelId].OnGraphRunning();

        // At this point we should know which data/streams form the service(s) that are being accessed. We need to
        // ensure those streams will pass through the device's PID filter.
        ThrowExceptionIfTuneCancelled();
        ConfigurePidFilter();
        ThrowExceptionIfTuneCancelled();

        // If the service is encrypted, start decrypting it.
        UpdateDecryptList(subChannelId, CaPmtListManagementAction.Add);
      }
      catch (Exception ex)
      {
        if (!(ex is TvException))
        {
          Log.Write(ex);
        }

        // One potential reason for getting here is that signal could not be locked, and the reason for
        // that may be that tuning failed. We always want to force a retune on the next tune request in
        // this situation.
        _previousChannel = null;
        if (_mapSubChannels[subChannelId] != null)
        {
          FreeSubChannel(subChannelId);
        }
        throw;
      }
      finally
      {
        _cancelTune = false;
      }

      return _mapSubChannels[subChannelId];
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being cancelled.</param>
    public void CancelTune(int subChannelId)
    {
      Log.Debug("TvCardBase: subchannel {0} cancel tune", subChannelId);
      _cancelTune = true;
      if (_mapSubChannels.ContainsKey(subChannelId))
      {
        _mapSubChannels[subChannelId].CancelTune();
      }
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw and exception if it has.
    /// </summary>
    protected void ThrowExceptionIfTuneCancelled()
    {
      if (_cancelTune)
      {
        throw new TvExceptionTuneCancelled();
      }
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected abstract void PerformTuning(IChannel channel);

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
    /// Get the device's ITVEPG interface, used for grabbing electronic programme guide data.
    /// </summary>
    public virtual ITVEPG EpgInterface
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Abort grabbing electronic programme guide data.
    /// </summary>
    public virtual void AbortGrabbing()
    {
    }

    /// <summary>
    /// Get the electronic programme guide data found in a grab session.
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
    /// Start grabbing electronic programme guide data (idle EPG grabber).
    /// </summary>
    /// <param name="callback">The delegate to call when grabbing is complete or canceled.</param>
    public virtual void GrabEpg(BaseEpgGrabber callback)
    {
    }

    /// <summary>
    /// Start grabbing electronic programme guide data (timeshifting/recording EPG grabber).
    /// </summary>
    public virtual void GrabEpg()
    {
    }

    /// <summary>
    /// Activate or deactivate the EPG grabber.
    /// </summary>
    /// <param name="value"><c>True</c> to enable EPG grabbing.</param>
    protected virtual void UpdateEpgGrabber(bool value)
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
      // Dispose plugins.
      if (_customDeviceInterfaces != null)
      {
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          // Avoid recursive loop for ITVCard implementations that also implement ICustomDevice... like TvCardDvbSs2.
          if (!(deviceInterface is TvCardBase))
          {
            deviceInterface.Dispose();
          }
        }
      }
      _customDeviceInterfaces = new List<ICustomDevice>();
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="channel">The service or channel to associate with the subchannel.</param>
    /// <returns>the ID of the new subchannel</returns>
    protected abstract int CreateNewSubChannel(IChannel channel);

    /// <summary>
    /// Free a subchannel.
    /// </summary>
    /// <param name="id">The ID of the subchannel.</param>
    public virtual void FreeSubChannel(int id)
    {
      Log.Debug("TvCardBase: free subchannel, ID = {0}, subchannel count = {1}", id, _mapSubChannels.Count);
      if (_mapSubChannels.ContainsKey(id))
      {
        if (_mapSubChannels[id].IsTimeShifting)
        {
          Log.Debug("TvCardBase: subchannel is still timeshifting!");
          return;
        }
        if (_mapSubChannels[id].IsRecording)
        {
          Log.Debug("TvCardBase: subchannel is still recording!");
          return;
        }

        try
        {
          UpdateDecryptList(id, CaPmtListManagementAction.Last);
          _mapSubChannels[id].Decompose();
        }
        finally
        {
          _mapSubChannels.Remove(id);
        }
        // PID filters are configured according to the PIDs that need to be passed, so reconfigure the PID
        // filter *after* we removed the subchannel (otherwise PIDs for the subchannel that we're freeing
        // won't be removed).
        ConfigurePidFilter();
      }
      else
      {
        Log.Debug("TvCardBase: subchannel not found!");
      }
      if (_mapSubChannels.Count == 0)
      {
        _subChannelId = 0;
        Log.Debug("TvCardBase: no subchannels present, stopping device");
        Stop();
      }
      else
      {
        Log.Debug("TvCardBase: subchannels still present, leave device running");
      }
    }

    /// <summary>
    /// Free all subchannels.
    /// </summary>
    protected void FreeAllSubChannels()
    {
      Log.Info("tvcard:FreeAllSubChannels");
      Dictionary<int, BaseSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
      while (en.MoveNext())
      {
        en.Current.Value.Decompose();
      }
      _mapSubChannels.Clear();
      _subChannelId = 0;
    }

    /// <summary>
    /// Get a specific subchannel.
    /// </summary>
    /// <param name="id">The ID of the subchannel.</param>
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
    /// Gets the first sub channel.
    /// </summary>
    /// <returns></returns>
    public ITvSubChannel GetFirstSubChannel()
    {
      ITvSubChannel subChannel = null;
      if (_mapSubChannels.Count > 0)
      {
        subChannel = _mapSubChannels.FirstOrDefault().Value;
      }
      return subChannel;
    }

    /// <summary>
    /// Get the devices' subchannels.
    /// </summary>
    /// <value>An array containing the subchannels.</value>
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