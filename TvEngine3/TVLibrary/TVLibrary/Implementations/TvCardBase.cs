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

namespace TvLibrary.Implementations
{
  /// <summary>
  /// Base class for all tv cards
  /// </summary>
  public abstract class TvCardBase
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
    protected void OnAfterTuneEvent()
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
      _device = device;
      _tunerDevice = device;
      _name = device.Name;
      _devicePath = device.DevicePath;

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
    /// Instance of the conditional access
    /// </summary>
    protected ConditionalAccess _conditionalAccess;

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
    /// Minimum analog channel number
    /// </summary>
    protected int _minChannel;

    /// <summary>
    /// Maximum analog channel number
    /// </summary>
    protected int _maxChannel;

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
    /// The numer of channels that the device is capable of or allowed to decrypt simultaneously. Zero means
    /// there is no limit.
    /// </summary>
    protected int _decryptLimit = 0;

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
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MinChannel
    {
      get { return _minChannel; }
    }

    /// <summary>
    /// Gets the max channel.
    /// </summary>
    /// <value>The max channel.</value>
    public int MaxChannel
    {
      get { return _maxChannel; }
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
        if (_conditionalAccess == null)
          return 0;
        return _conditionalAccess.NumberOfChannelsDecrypting;
      }
    }

    /// <summary>
    /// Gets the interface for controlling the DiSEqC satellite dish motor.
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
    public bool IsTunerLocked
    {
      get
      {
        UpdateSignalQuality(true);
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
        UpdateSignalQuality();
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
        UpdateSignalQuality();
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
        if (_isScanning)
        {
          OnScanning();
        }
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
      Log.Log.Debug("TvCardBase: loading custom device plugins");

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
            IConditionalAccessProvider caProvider = d as IConditionalAccessProvider;
            if (caProvider != null)
            {
              caProvider.OpenInterface();
            }
          }
        }
      }
      if (_customDeviceInterfaces.Count == 0)
      {
        Log.Log.Debug("TvCardBase: no plugins supported");
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

    ///<summary>
    /// Checks if the tuner is locked in and a sginal is present
    ///</summary>
    ///<returns>true, when the tuner is locked and a signal is present</returns>
    public virtual bool LockedInOnSignal()
    {
      return false;
    }

    #endregion

    #region abstract methods

    /// <summary>
    /// A derrived class should update the signal informations of the tv cards
    /// </summary>
    protected abstract void UpdateSignalQuality();

    /// <summary>
    /// A derrived class should update the signal informations of the tv cards
    /// </summary>
    protected abstract void UpdateSignalQuality(bool force);

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
    /// Starts the graph
    /// </summary>
    public abstract void RunGraph(int subChannel);

    /// <summary>
    /// A derrived class should activate / deactivate the epg grabber
    /// </summary>
    /// <param name="value">Mode</param>
    protected abstract void UpdateEpgGrabber(bool value);

    /// <summary>
    /// A derrived class should activate / deactivate the scanning
    /// </summary>
    protected abstract void OnScanning();

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
      if (_mapSubChannels.ContainsKey(id))
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