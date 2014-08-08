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
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using UPnP.Infrastructure;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.SSDP;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// This class is responsible for detecting tuners.
  /// </summary>
  public class TunerDetector : IDisposable
  {
    #region constants

    private static readonly Regex REGEX_DRIVER_DATE = new Regex(@"^(\d{4})(\d{2})(\d{2})");

    #endregion

    #region variables

    private bool _isStarted = false;

    // Used for detecting and communicating with UPnP devices.
    private CPData _upnpControlPointData = null;
    private UPnPNetworkTracker _upnpAgent = null;
    private UPnPControlPoint _upnpControlPoint = null;
    private List<ITunerDetectorUpnp> _upnpDetectors = new List<ITunerDetectorUpnp>();

    // Used for detecting and communicating with devices that are directly
    // connected to the system.
    private ManagementEventWatcher _systemDeviceChangeEventWatcher = null;
    private ManagementObjectSearcher _systemDeviceInfoSearcher = null;
    private ManagementObjectCollection _systemDeviceDriverInfo = null;
    private List<KeyValuePair<string, string>> _systemDeviceInterestingProperties = new List<KeyValuePair<string, string>>();
    private DateTime _previousSystemDeviceChange = DateTime.MinValue;
    private List<ITunerDetectorSystem> _systemDetectors = new List<ITunerDetectorSystem>();

    private HashSet<string> _firstDetectionTuners = new HashSet<string>();  // external IDs
    private HashSet<string> _knownUpnpRootDevices = new HashSet<string>();  // UUIDs
    private HashSet<string> _knownSystemTuners = new HashSet<string>();     // external IDs

    // tuner external ID => tuner
    private Dictionary<string, ITVCard> _tuners = new Dictionary<string, ITVCard>();
    // UPnP device UUID => [tuner external IDs] (used to implement UPnP tuner removal)
    private Dictionary<string, HashSet<string>> _upnpDeviceTuners = new Dictionary<string, HashSet<string>>();
    // product instance ID => tuner instance ID => tuner external IDs
    private Dictionary<string, Dictionary<string, HashSet<string>>> _naturalTunerGroups = new Dictionary<string, Dictionary<string, HashSet<string>>>();
    // database group ID => group
    private Dictionary<int, TunerGroup> _configuredTunerGroups = new Dictionary<int, TunerGroup>();

    // The listener that we notify when tuner changes are detected.
    private ITunerDetectionEventListener _eventListener = null;

    #endregion

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="listener">A listener that wishes to be notified about tuner detection events.</param>
    public TunerDetector(ITunerDetectionEventListener listener)
    {
      _eventListener = listener;

      // Setup UPnP tuner detection.
      UPnPConfiguration.LOGGER = new Logger();
      _upnpControlPointData = new CPData();
      _upnpAgent = new UPnPNetworkTracker(_upnpControlPointData);
      _upnpAgent.RootDeviceAdded += OnUpnpRootDeviceAdded;
      _upnpAgent.RootDeviceRemoved += OnUpnpRootDeviceRemoved;
      _upnpControlPoint = new UPnPControlPoint(_upnpAgent);

      // Setup system tuner detection.
      _systemDeviceInterestingProperties.Add(new KeyValuePair<string, string>("DeviceName", "name    "));
      _systemDeviceInterestingProperties.Add(new KeyValuePair<string, string>("DriverProviderName", "provider"));
      _systemDeviceInterestingProperties.Add(new KeyValuePair<string, string>("DriverVersion", "version "));
      _systemDeviceInterestingProperties.Add(new KeyValuePair<string, string>("DriverDate", "date    "));
      _systemDeviceInterestingProperties.Add(new KeyValuePair<string, string>("Location", "location"));
      _systemDeviceInterestingProperties.Add(new KeyValuePair<string, string>("PDO", "PDO     "));
      string[] queryProperties = new string[_systemDeviceInterestingProperties.Count];
      int i = 0;
      foreach (KeyValuePair<string, string> p in _systemDeviceInterestingProperties)
      {
        queryProperties[i++] = p.Key;
      }
      _systemDeviceInfoSearcher = new ManagementObjectSearcher(string.Format("SELECT DeviceID, {0} FROM Win32_PnPSignedDriver WHERE DeviceClass = 'MEDIA'", string.Join(", ", queryProperties)));

      // EventType 2 and 3 are device arrival and removal. See:
      // http://msdn.microsoft.com/en-us/library/windows/desktop/aa394124%28v=vs.85%29.aspx
      // You'd think checking for arrival and removal would be enough, but in
      // practice it seems we need to be looking at configuration change events
      // (EventType 1) more than anything else. Configuration changes are
      // triggered [for example] on disabling or enabling a device in device
      // manager, and replugging a tuner for which a driver is already
      // installed.
      _systemDeviceChangeEventWatcher = new ManagementEventWatcher();
      _systemDeviceChangeEventWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
      _systemDeviceChangeEventWatcher.EventArrived += OnSystemDeviceConnectedOrDisconnected;

      this.LogInfo("detector: loading detectors");
      Assembly a = Assembly.GetExecutingAssembly();
      foreach (Type t in a.GetTypes())
      {
        if (t.IsClass && !t.IsAbstract)
        {
          Type detectorInterface = t.GetInterface(typeof(ITunerDetectorUpnp).Name);
          if (detectorInterface != null)
          {
            ITunerDetectorUpnp upnpDetector = (ITunerDetectorUpnp)Activator.CreateInstance(t);
            this.LogInfo("detector: adding UPnP tuner detector {0} ({1})", upnpDetector.Name, t.Name);
            _upnpDetectors.Add(upnpDetector);
          }
          detectorInterface = t.GetInterface(typeof(ITunerDetectorSystem).Name);
          if (detectorInterface != null)
          {
            ITunerDetectorSystem systemDetector = (ITunerDetectorSystem)Activator.CreateInstance(t);
            this.LogInfo("detector: adding system tuner detector {0} ({1})", systemDetector.Name, t.Name);
            _systemDetectors.Add(systemDetector);
          }
        }
      }
    }

    #region detector control

    /// <summary>
    /// Start tuner detection.
    /// </summary>
    public void Start()
    {
      if (_isStarted)
      {
        return;
      }
      this.LogInfo("detector: starting tuner detection");
      // Start detecting tuners connected directly to the system.
      DetectSystemTuners();

      try
      {
        _systemDeviceChangeEventWatcher.Start();
      }
      catch
      {
        // Fails on Windows Media Center 2005 (ManagementException "unsupported", despite MS documentation).
        this.LogWarn("detector: failed to start device change event watcher, you'll have to restart the TV Engine to detect new tuners");
      }

      // Start detecting UPnP tuners.
      // IMPORTANT: this parameter must be set to allow devices with many sub-devices
      // and/or services to be detected. The timer interval specifies how long the
      // SSDP controller has from first detection of the root device SSDP packet
      // until descriptions for all devices and services have been requested, received
      // and processed. DRI tuners normally take about 5 seconds.
      SSDPClientController.EXPIRATION_TIMER_INTERVAL = 60000;
      // IMPORTANT: you should start the control point before the network tracker.
      _upnpControlPoint.Start();
      _upnpAgent.Start();
      _upnpAgent.SharedControlPointData.SSDPController.SearchDeviceByDeviceTypeVersion("schemas-opencable-com:service:Tuner", "1", null);
      _upnpAgent.SharedControlPointData.SSDPController.SearchDeviceByDeviceTypeVersion("urn:ses-com:device:SatIPServer", "1", null);
      _isStarted = true;
    }

    /// <summary>
    /// Reload the detector's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("detector: reload configuration");

      // Order of operations is important.
      lock (_tuners)
      {
        // 1. Force-detect tuner changes.
        DetectSystemTuners();

        // 2. Remove old groups.
        HashSet<int> currentGroupIds = new HashSet<int>(_configuredTunerGroups.Keys);
        HashSet<int> unchangedGroupIds = new HashSet<int>();
        List<CardGroup> newGroups = new List<CardGroup>();
        IList<CardGroup> dbGroups = CardManagement.ListAllCardGroups();
        foreach (CardGroup dbGroup in dbGroups)
        {
          unchangedGroupIds.Add(dbGroup.IdCardGroup);
          if (!_configuredTunerGroups.ContainsKey(dbGroup.IdCardGroup))
          {
            newGroups.Add(dbGroup);
          }
        }
        currentGroupIds.ExceptWith(unchangedGroupIds);
        foreach (int groupId in currentGroupIds)
        {
          TunerGroup group;
          if (_configuredTunerGroups.TryGetValue(groupId, out group))
          {
            this.LogInfo("detector: removing tuner group, name = {0}, product instance ID = {1}, tuner instance ID = {2}...", group.Name, group.ProductInstanceId ?? "[null]", group.TunerInstanceId ?? "[null]");
            foreach (ITVCard tuner in group.Tuners)
            {
              this.LogInfo("  tuner, name = {0}, product instance ID = {1}, tuner instance ID = {2}", tuner.Name, tuner.ProductInstanceId ?? "[null]", tuner.TunerInstanceId ?? "[null]");
              ITunerInternal internalTuner = tuner as ITunerInternal;
              if (internalTuner != null)
              {
                internalTuner.Group = null;
              }
            }
            _configuredTunerGroups.Remove(groupId);
          }
        }

        // 3. Update groups that we already knew about but have changed.
        HashSet<int> changedGroupIds = new HashSet<int>();
        Dictionary<string, int> newGroupMembers = new Dictionary<string, int>();
        foreach (CardGroup dbGroup in dbGroups)
        {
          TunerGroup group;
          if (_configuredTunerGroups.TryGetValue(dbGroup.IdCardGroup, out group))
          {
            // Collect the external IDs for current members.
            HashSet<string> currentGroupMembers = new HashSet<string>();
            foreach (ITVCard tuner in group.Tuners)
            {
              currentGroupMembers.Add(tuner.ExternalId);
            }

            // Record new and unchanged members.
            HashSet<string> unchangedGroupMembers = new HashSet<string>();
            IList<CardGroupMap> groupMaps = dbGroup.CardGroupMaps;
            foreach (CardGroupMap map in groupMaps)
            {
              string externalId = map.Card.DevicePath;
              ITVCard tuner;
              if (_tuners.TryGetValue(externalId, out tuner))
              {
                if (currentGroupMembers.Contains(externalId))
                {
                  unchangedGroupMembers.Add(externalId);
                }
                else
                {
                  changedGroupIds.Add(dbGroup.IdCardGroup);
                  newGroupMembers.Add(externalId, dbGroup.IdCardGroup);
                }
              }
            }

            // 3a. Remove old members.
            currentGroupMembers.ExceptWith(unchangedGroupMembers);
            foreach (string externalId in currentGroupMembers)
            {
              ITVCard tuner;
              if (_tuners.TryGetValue(externalId, out tuner))
              {
                this.LogInfo("detector: removing tuner from group...");
                this.LogInfo("  group, name = {0}, product instance ID = {1}, tuner instance ID = {2}", group.Name, group.ProductInstanceId ?? "[null]", group.TunerInstanceId ?? "[null]");
                this.LogInfo("  tuner, name = {0}, product instance ID = {1}, tuner instance ID = {2}", tuner.Name, tuner.ProductInstanceId ?? "[null]", tuner.TunerInstanceId ?? "[null]");
                changedGroupIds.Add(dbGroup.IdCardGroup);
                group.Remove(tuner);
                ITunerInternal internalTuner = tuner as ITunerInternal;
                if (internalTuner != null)
                {
                  internalTuner.Group = null;
                }
              }
            }
          }
        }

        // 3b. Add new members.
        foreach (KeyValuePair<string, int> newGroupMember in newGroupMembers)
        {
          TunerGroup group;
          ITVCard tuner;
          if (_tuners.TryGetValue(newGroupMember.Key, out tuner) && _configuredTunerGroups.TryGetValue(newGroupMember.Value, out group))
          {
            this.LogInfo("detector: adding tuner to group...");
            this.LogInfo("  group, name = {0}, product instance ID = {1}, tuner instance ID = {2}", group.Name, group.ProductInstanceId ?? "[null]", group.TunerInstanceId ?? "[null]");
            this.LogInfo("  tuner, name = {0}, product instance ID = {1}, tuner instance ID = {2}", tuner.Name, tuner.ProductInstanceId ?? "[null]", tuner.TunerInstanceId ?? "[null]");
            group.Add(tuner);
            ITunerInternal internalTuner = tuner as ITunerInternal;
            if (internalTuner != null)
            {
              internalTuner.Group = group;
            }
          }
        }

        // 3c. Update the group details.
        foreach (int groupId in changedGroupIds)
        {
          TunerGroup group;
          if (_configuredTunerGroups.TryGetValue(groupId, out group))
          {
            bool isFirst = true;
            string commonProductInstanceId = null;
            string commonTunerInstanceId = null;
            foreach (ITVCard tuner in group.Tuners)
            {
              if (isFirst)
              {
                isFirst = false;
                if (tuner.ProductInstanceId == null || tuner.TunerInstanceId == null)
                {
                  break;
                }
                commonProductInstanceId = tuner.ProductInstanceId;
                commonTunerInstanceId = tuner.TunerInstanceId;
              }
              if (!string.Equals(commonProductInstanceId, tuner.ProductInstanceId) || !string.Equals(commonTunerInstanceId, tuner.TunerInstanceId))
              {
                commonProductInstanceId = null;
                commonTunerInstanceId = null;
                break;
              }
            }
            if (!string.Equals(commonProductInstanceId, group.ProductInstanceId) || !string.Equals(commonTunerInstanceId, group.TunerInstanceId))
            {
              this.LogInfo("detector: updating identifiers for tuner group {0}...", group.Name);
              this.LogInfo("  previous, product instance ID = {0}, tuner instance ID = {1}", group.ProductInstanceId ?? "[null]", group.TunerInstanceId ?? "[null]");
              this.LogInfo("  new, product instance ID = {0}, tuner instance ID = {1}", commonProductInstanceId ?? "[null]", commonTunerInstanceId ?? "[null]");
              group.ProductInstanceId = commonProductInstanceId;
              group.TunerInstanceId = commonTunerInstanceId;
            }
          }
        }

        // 4. Construct new groups for the groups we didn't already know about.
        foreach (CardGroup dbGroup in newGroups)
        {
          TunerGroup group = new TunerGroup(dbGroup);
          IList<CardGroupMap> groupMaps = dbGroup.CardGroupMaps;
          bool isFirst = true;
          string commonProductInstanceId = null;
          string commonTunerInstanceId = null;
          foreach (CardGroupMap map in groupMaps)
          {
            ITVCard tuner;
            if (_tuners.TryGetValue(map.Card.DevicePath, out tuner))
            {
              if (isFirst)
              {
                isFirst = false;
                if (tuner.ProductInstanceId != null && tuner.TunerInstanceId != null)
                {
                  commonProductInstanceId = tuner.ProductInstanceId;
                  commonTunerInstanceId = tuner.TunerInstanceId;
                }
              }
              else if (!string.Equals(commonProductInstanceId, tuner.ProductInstanceId) || !string.Equals(commonTunerInstanceId, tuner.TunerInstanceId))
              {
                commonProductInstanceId = null;
                commonTunerInstanceId = null;
              }
              group.Add(tuner);
              ITunerInternal internalTuner = tuner as ITunerInternal;
              if (internalTuner != null)
              {
                internalTuner.Group = group;
              }
            }
          }

          if (group.Tuners.Count > 0)
          {
            this.LogInfo("detector: adding tuner group, name = {0}, product instance ID = {1}, tuner instance ID = {2}...", group.Name, group.ProductInstanceId ?? "[null]", group.TunerInstanceId ?? "[null]");
            foreach (ITVCard tuner in group.Tuners)
            {
              this.LogInfo("  tuner, name = {0}, product instance ID = {1}, tuner instance ID = {2}", tuner.Name, tuner.ProductInstanceId ?? "[null]", tuner.TunerInstanceId ?? "[null]");
            }
            _configuredTunerGroups.Add(dbGroup.IdCardGroup, group);
          }
        }
      }
    }

    /// <summary>
    /// Reset and restart tuner detection.
    /// </summary>
    public void Reset()
    {
      Stop();
      _firstDetectionTuners.Clear();
      _knownUpnpRootDevices.Clear();
      _knownSystemTuners.Clear();
      _upnpDeviceTuners.Clear();

      // TODO should these be disposed?
      _tuners.Clear();

      _naturalTunerGroups.Clear();
      _configuredTunerGroups.Clear();
      Start();
    }

    /// <summary>
    /// Stop tuner detection.
    /// </summary>
    public void Stop()
    {
      if (!_isStarted)
      {
        return;
      }
      this.LogInfo("detector: stopping tuner detection...");
      _upnpAgent.Close();
      _upnpControlPoint.Close();
      _systemDeviceChangeEventWatcher.Stop();
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Stop();

      foreach (ITunerDetectorUpnp detector in _upnpDetectors)
      {
        IDisposable d = detector as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
      }
      foreach (ITunerDetectorSystem detector in _systemDetectors)
      {
        IDisposable d = detector as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
      }

      if (_upnpControlPoint != null)
      {
        _upnpControlPoint.DisconnectAll();
        _upnpControlPoint.Dispose();
        _upnpControlPoint = null;
      }
      if (_upnpAgent != null)
      {
        _upnpAgent.Dispose();
        _upnpAgent = null;
      }
      if (_systemDeviceChangeEventWatcher != null)
      {
        _systemDeviceChangeEventWatcher.Dispose();
        _systemDeviceChangeEventWatcher = null;
      }
      if (_systemDeviceInfoSearcher != null)
      {
        _systemDeviceInfoSearcher.Dispose();
        _systemDeviceInfoSearcher = null;
      }
    }

    #endregion

    #region system device detection

    private void OnSystemDeviceConnectedOrDisconnected(object sender, EventArrivedEventArgs e)
    {
      // Often several events will be triggered within a very short period of
      // time when a device is added/removed. We only want to check for new
      // tuners once per burst.
      if ((DateTime.Now - _previousSystemDeviceChange).TotalMilliseconds < 10000)
      {
        return;
      }
      _previousSystemDeviceChange = DateTime.Now;

      // Do our processing in a different thread to ensure we don't cause
      // problems like this:
      // http://www.codeproject.com/Articles/35212/WM-DEVICECHANGE-problem
      Thread t = new Thread(DoDelayedSystemTunerDetection);
      t.Start();
    }

    private void DoDelayedSystemTunerDetection()
    {
      // Give the tuner time to load. Hopefully 10 seconds will be enough.
      Thread.Sleep(10000);

      // Configurable extra delay...
      int delayDetect = SettingsManagement.GetValue("delayCardDetect", 0);
      if (delayDetect >= 1)
      {
        Thread.Sleep(delayDetect * 1000);
      }
      DetectSystemTuners();
    }

    private void DetectSystemTuners()
    {
      lock (_tuners)
      {
        this.LogDebug("detector: detecting system tuners...");
        _systemDeviceDriverInfo = _systemDeviceInfoSearcher.Get();

        HashSet<string> currentSystemTuners = new HashSet<string>();
        foreach (ITunerDetectorSystem detector in _systemDetectors)
        {
          ICollection<ITVCard> tuners = detector.DetectTuners();
          foreach (ITVCard tuner in tuners)
          {
            currentSystemTuners.Add(tuner.ExternalId);
            if (_tuners.ContainsKey(tuner.ExternalId))
            {
              // Not a new tuner.
              continue;
            }
            OnTunerDetected(tuner);
          }
        }

        // Remove the tuners that are no longer available.
        _knownSystemTuners.ExceptWith(currentSystemTuners);
        foreach (string removedTunerExternalId in _knownSystemTuners)
        {
          ITVCard tuner;
          if (_tuners.TryGetValue(removedTunerExternalId, out tuner))
          {
            OnTunerRemoved(tuner);
          }
        }

        _knownSystemTuners = currentSystemTuners;
      }
    }

    #endregion

    #region UPnP device detection

    private void OnUpnpRootDeviceAdded(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null || rootDescriptor.State != RootDescriptorState.Ready)
      {
        return;
      }
      lock (_tuners)
      {
        if (!_knownUpnpRootDevices.Add(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
        {
          this.LogWarn("detector: re-detecting known root device {0}", rootDescriptor.SSDPRootEntry.RootDeviceUUID);
        }

        DeviceDescriptor rootDeviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
        foreach (ITunerDetectorUpnp detector in _upnpDetectors)
        {
          ICollection<ITVCard> tuners = detector.DetectTuners(rootDeviceDescriptor, _upnpControlPoint);
          if (tuners.Count > 0)
          {
            LogUpnpDeviceInfo(rootDeviceDescriptor);
            HashSet<string> tunerExternalIds = new HashSet<string>();
            foreach (ITVCard tuner in tuners)
            {
              OnTunerDetected(tuner);
              tunerExternalIds.Add(tuner.ExternalId);
            }
            _upnpDeviceTuners.Add(rootDeviceDescriptor.DeviceUUID, tunerExternalIds);
          }
        }
      }
    }

    private void OnUpnpRootDeviceRemoved(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null)
      {
        return;
      }

      lock (_tuners)
      {
        if (!_knownUpnpRootDevices.Remove(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
        {
          this.LogWarn("detector: detecting removal of unknown root device {0}", rootDescriptor.SSDPRootEntry.RootDeviceUUID);
        }

        DeviceDescriptor rootDeviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
        HashSet<string> tunerExternalIds;
        if (_upnpDeviceTuners.TryGetValue(rootDeviceDescriptor.DeviceUUID, out tunerExternalIds))
        {
          this.LogInfo("detector: UPnP device removed");
          LogUpnpDeviceInfo(rootDeviceDescriptor);
          foreach (string t in tunerExternalIds)
          {
            ITVCard tuner;
            if (_tuners.TryGetValue(t, out tuner))
            {
              OnTunerRemoved(tuner);
            }
          }
          _upnpDeviceTuners.Remove(rootDeviceDescriptor.DeviceUUID);
        }
      }
    }

    private void LogUpnpDeviceInfo(DeviceDescriptor rootDeviceDescriptor)
    {
      this.LogInfo("  friendly name = {0}", rootDeviceDescriptor.FriendlyName);
      this.LogInfo("  UUID          = {0}", rootDeviceDescriptor.DeviceUUID);

      XmlNamespaceManager nm = new XmlNamespaceManager(rootDeviceDescriptor.DeviceNavigator.NameTable);
      nm.AddNamespace("d", UPnPConsts.NS_DEVICE_DESCRIPTION);
      XPathNavigator nav = rootDeviceDescriptor.DeviceNavigator;
      this.LogDebug("  manufacturer");
      this.LogDebug("    name        = {0}", nav.SelectSingleNode("d:manufacturer/text()", nm).Value);
      XPathNodeIterator it = nav.Select("d:manufacturerURL/text()", nm);
      if (it.MoveNext())
      {
        this.LogDebug("    URL         = {0}", it.Current.Value);
      }

      this.LogDebug("  model");
      string[] propertyDescriptions = new string[] { "name", "number", "description", "URL" };
      string[] propertyNames = new string[] { "modelName", "modelNumber", "modelDescription", "modelURL" };
      for (int i = 0; i < propertyDescriptions.Length; i++)
      {
        it = nav.Select("d:" + propertyNames[i] + "/text()", nm);
        if (it.MoveNext())
        {
          this.LogDebug("    {0, -11} = {1}", propertyDescriptions[i], it.Current.Value);
        }
      }

      it = nav.Select("d:serialNumber/text()", nm);
      if (it.MoveNext())
      {
        this.LogDebug("  serial number = {0}", it.Current.Value);
      }
      it = nav.Select("d:UPC/text()", nm);
      if (it.MoveNext())
      {
        this.LogDebug("  UPC           = {0}", it.Current.Value);
      }
    }

    #endregion

    #region common tuner add/remove logic

    private void OnTunerDetected(ITVCard tuner)
    {
      this.LogInfo("  add...");
      this.LogInfo("    name        = {0}", tuner.Name);
      this.LogInfo("    external ID = {0}", tuner.ExternalId);
      this.LogInfo("    type        = {0}", tuner.TunerType);
      this.LogInfo("    product ID  = {0}", tuner.ProductInstanceId ?? "[null]");
      this.LogInfo("    tuner ID    = {0}", tuner.TunerInstanceId ?? "[null]");
      string productName = string.Empty;
      if (tuner.ProductInstanceId != null)
      {
        productName = string.Format("Product Instance {0}", tuner.ProductInstanceId);

        List<ManagementObject> objects = new List<ManagementObject>();
        foreach (ManagementObject o in _systemDeviceDriverInfo)
        {
          if (o.Properties["DeviceID"].Value.ToString().ToLowerInvariant().Contains(tuner.ProductInstanceId))
          {
            objects.Add(o);
          }
        }
        if (objects.Count == 1)
        {
          this.LogInfo("    driver...");
          ManagementObject o = objects[0];
          foreach (KeyValuePair<string, string> p in _systemDeviceInterestingProperties)
          {
            object pVal = o.Properties[p.Key].Value;
            if (pVal != null)
            {
              if (p.Key.Equals("DriverDate"))
              {
                Match m = REGEX_DRIVER_DATE.Match(pVal.ToString());
                if (m.Success)
                {
                  this.LogInfo("      {0} = {1}-{2}-{3}", p.Value, m.Groups[1].Captures[0], m.Groups[2].Captures[0], m.Groups[3].Captures[0]);
                }
                else
                {
                  this.LogInfo("      {0} = {1}", p.Value, pVal.ToString());
                }
              }
              else
              {
                this.LogInfo("      {0} = {1}", p.Value, pVal.ToString());
              }
              if (p.Key.Equals("DeviceName"))
              {
                productName = pVal.ToString();
              }
            }
          }
        }
      }
      _tuners.Add(tuner.ExternalId, tuner);

      // Detect the naturally related tuners. These are tuners with the same product and tuner
      // instance ID.
      HashSet<string> relatedTuners = FindRelatedTuners(tuner);

      // Find or create a group for the tuner if appropriate.
      bool isNewTuner = false;
      TunerGroup group = null;
      Card tunerDbSettings = CardManagement.GetCardByDevicePath(tuner.ExternalId, CardIncludeRelationEnum.None);
      if (tunerDbSettings == null)
      {
        // First ever detection. Create the tuner settings.
        this.LogInfo("    new tuner...");
        isNewTuner = true;
        _firstDetectionTuners.Add(tuner.ExternalId);
        tunerDbSettings = new Card
        {
          TimeshiftingFolder = string.Empty,
          RecordingFolder = string.Empty,
          DevicePath = tuner.ExternalId,
          Name = tuner.Name,
          Priority = 1,
          GrabEPG = (tuner.TunerType != CardType.Analog && tuner.TunerType != CardType.Atsc),   // analog signals don't carry EPG, ATSC EPG not supported
          Enabled = true,
          PreloadCard = false,
          AlwaysSendDiseqcCommands = false,
          DiseqcCommandRepeatCount = 0,
          UseConditionalAccess = true,
          CamType = (int)CamType.Default,
          DecryptLimit = 0,
          MultiChannelDecryptMode = (int)MultiChannelDecryptMode.List,
          NetProvider = (int)DbNetworkProvider.Generic,
          PidFilterMode = (int)PidFilterMode.Auto,
          IdleMode = (int)IdleMode.Stop
        };
        tunerDbSettings = CardManagement.SaveCard(tunerDbSettings);
      }
      else
      {
        this.LogInfo("    existing tuner...");
      }

      // Configuration loading is necessary before group detection. This also
      // triggers preloading.
      tuner.ReloadConfiguration();

      if (isNewTuner)
      {
        // If we have product/tuner instance information and detected the tuner is a member of a
        // natural group...
        if (tuner.ProductInstanceId != null && tuner.TunerInstanceId != null && relatedTuners != null && relatedTuners.Count > 1)
        {
          // Automatically add the tuner to an existing tuner group if the group product and tuner
          // instance information matches.
          group = AddNewTunerToExistingNaturalGroup(tuner);

          // ...or if that doesn't apply, automatically add the tuner to an existing tuner group if
          // the group contains *all* the naturally related tuners. The assumption in this case is
          // that our group detection failed to automatically detect all the tuners in the group.
          bool foundAnyRelatedTunerInAnyGroup = false;
          if (group == null)
          {
            group = AddNewTunerToExistingExtendedNaturalGroup(tuner, relatedTuners, out foundAnyRelatedTunerInAnyGroup);
          }

          // Finally, if all of the tuners in the natural group are new and not in any other group
          // then create a new group.
          if (group == null && !foundAnyRelatedTunerInAnyGroup)
          {
            group = CreateNewGroupOnFirstDetection(tuner, relatedTuners, productName);
          }
        }
      }
      else
      {
        // This tuner has been detected previously. Check if it is a member of an existing tuner
        // group.
        group = AddExistingTunerToConfiguredGroup(tuner);
      }

      ITunerInternal internalTuner = tuner as ITunerInternal;
      if (internalTuner != null)
      {
        internalTuner.Group = group;
      }

      _eventListener.OnTunerAdded(tuner);
    }

    #region tuner group handling

    private HashSet<string> FindRelatedTuners(ITVCard tuner)
    {
      HashSet<string> relatedTuners = null;
      if (tuner.ProductInstanceId != null && tuner.TunerInstanceId != null)
      {
        Dictionary<string, HashSet<string>> productTuners;
        if (!_naturalTunerGroups.TryGetValue(tuner.ProductInstanceId, out productTuners))
        {
          productTuners = new Dictionary<string, HashSet<string>>();
          relatedTuners = new HashSet<string>();
          relatedTuners.Add(tuner.ExternalId);
          productTuners.Add(tuner.TunerInstanceId, relatedTuners);
          _naturalTunerGroups.Add(tuner.ProductInstanceId, productTuners);
        }
        else
        {
          if (!productTuners.TryGetValue(tuner.TunerInstanceId, out relatedTuners))
          {
            relatedTuners = new HashSet<string>();
            productTuners.Add(tuner.TunerInstanceId, relatedTuners);
          }
          else if (relatedTuners.Count > 0)
          {
            this.LogInfo("    detected naturally related tuners...");
            foreach (string id in relatedTuners)
            {
              ITVCard t = _tuners[id];
              this.LogInfo("      name = {0}, type = {1}, external ID = {2}", t.Name, t.TunerType, id);
            }
          }
          relatedTuners.Add(tuner.ExternalId);
        }
      }
      return relatedTuners;
    }

    private TunerGroup AddNewTunerToExistingNaturalGroup(ITVCard tuner)
    {
      // This could incorrectly match or miss an existing group depending on
      // the order of tuner detection.
      // MISS = If no tuners in the group have been detected then the group
      // won't be in the dictionary.
      // MATCH = If some of the tuners in the group with matching product and
      // tuner instance IDs have been detected, but other tuners with
      // non-matching instance IDs haven't yet been detected, the instance
      // IDs may be set when they shouldn't be.
      foreach (TunerGroup group in _configuredTunerGroups.Values)
      {
        if (string.Equals(group.ProductInstanceId, tuner.ProductInstanceId) && string.Equals(group.TunerInstanceId, tuner.TunerInstanceId))
        {
          this.LogInfo("    detected existing tuner group with matching information, name = {0}...", group.Name);
          group.Add(tuner);
          CardGroupMap map = new CardGroupMap();
          map.IdCard = tuner.TunerId;
          map.IdCardGroup = group.TunerGroupId;
          CardManagement.SaveCardGroupMap(map);
          return group;
        }
      }
      return null;
    }

    private TunerGroup AddNewTunerToExistingExtendedNaturalGroup(ITVCard tuner, HashSet<string> relatedTuners, out bool foundAnyRelatedTunerInAnyGroup)
    {
      foundAnyRelatedTunerInAnyGroup = false;
      foreach (TunerGroup group in _configuredTunerGroups.Values)
      {
        // Only look at mixed product/tuner instance groups. AddNewTunerToExistingNaturalGroup() already
        // checked and failed to find a non-extended/pure group.
        if (group.ProductInstanceId == null && group.TunerInstanceId == null)
        {
          // For each naturally related tuner that is not this tuner...
          foreach (string relatedTunerExternalId1 in relatedTuners)
          {
            if (!relatedTunerExternalId1.Equals(tuner.ExternalId))
            {
              if (group.Tuners.Any(x => relatedTunerExternalId1.Equals(x.ExternalId)))
              {
                // Okay, we found a group containing one of the related tuners. If all of the
                // related tuners are in this group then we'll add the tuner to this group.
                // Otherwise this tuner won't get a group.
                foundAnyRelatedTunerInAnyGroup = true;
                foreach (string relatedTunerExternalId2 in relatedTuners)
                {
                  if (!relatedTunerExternalId2.Equals(tuner.ExternalId) && !relatedTunerExternalId2.Equals(relatedTunerExternalId1))
                  {
                    if (!group.Tuners.Any(x => relatedTunerExternalId2.Equals(x.ExternalId)))
                    {
                      // One of the related tuners is not in this group => its all over.
                      return null;
                    }
                  }
                }
                // All tuners are in the group!
                this.LogInfo("    detected existing tuner group containing all related tuners, name = {0}...", group.Name);
                group.Add(tuner);
                CardGroupMap map = new CardGroupMap();
                map.IdCard = tuner.TunerId;
                map.IdCardGroup = group.TunerGroupId;
                CardManagement.SaveCardGroupMap(map);
                return group;
              }
            }
          }
        }
      }
      return null;
    }

    private TunerGroup CreateNewGroupOnFirstDetection(ITVCard tuner, HashSet<string> relatedTuners, string productName)
    {
      foreach (string relatedTunerExternalId in relatedTuners)
      {
        if (!_firstDetectionTuners.Contains(relatedTunerExternalId))
        {
          return null;
        }
      }

      CardGroup dbGroup = new CardGroup();
      dbGroup.Name = string.Format("{0} Tuner {1}", productName, tuner.TunerInstanceId);
      dbGroup = CardManagement.SaveCardGroup(dbGroup);
      TunerGroup group = new TunerGroup(dbGroup);
      group.ProductInstanceId = tuner.ProductInstanceId;
      group.TunerInstanceId = tuner.TunerInstanceId;
      _configuredTunerGroups.Add(group.TunerGroupId, group);
      foreach (string relatedTunerExternalId in relatedTuners)
      {
        ITVCard relatedTuner = _tuners[relatedTunerExternalId];
        CardGroupMap map = new CardGroupMap();
        map.IdCard = relatedTuner.TunerId;
        map.IdCardGroup = group.TunerGroupId;
        CardManagement.SaveCardGroupMap(map);
        group.Add(relatedTuner);
      }
      this.LogInfo("    creating new tuner group, name = {0}...", group.Name);
      return group;
    }

    private TunerGroup AddExistingTunerToConfiguredGroup(ITVCard tuner)
    {
      IList<CardGroup> dbGroups = CardManagement.ListAllCardGroups();
      foreach (CardGroup dbGroup in dbGroups)
      {
        IList<CardGroupMap> groupMaps = dbGroup.CardGroupMaps;
        foreach (CardGroupMap map in groupMaps)
        {
          // Does the tuner belong to this group?
          if (map.IdCard == tuner.TunerId)
          {
            // Find and update our local group info.
            TunerGroup group;
            if (!_configuredTunerGroups.TryGetValue(dbGroup.IdCardGroup, out group))
            {
              group = new TunerGroup(dbGroup);
              if (tuner.ProductInstanceId != null && tuner.TunerInstanceId != null)
              {
                group.ProductInstanceId = tuner.ProductInstanceId;
                group.TunerInstanceId = tuner.TunerInstanceId;
              }
              _configuredTunerGroups.Add(group.TunerGroupId, group);
              this.LogInfo("    detected existing tuner group, name = {0}, product instance ID = {1}, tuner instance ID = {2}...", group.Name, group.ProductInstanceId ?? "[null]", group.TunerInstanceId ?? "[null]");
            }
            else
            {
              this.LogInfo("    detected existing tuner group, name = {0}, product instance ID = {1}, tuner instance ID = {2}...", group.Name, group.ProductInstanceId ?? "[null]", group.TunerInstanceId ?? "[null]");
              // Does the product/tuner instance info indicate that this is a natural group?
              if (group.ProductInstanceId != null && group.TunerInstanceId != null &&
                (!string.Equals(group.ProductInstanceId, tuner.ProductInstanceId) || !string.Equals(group.TunerInstanceId, tuner.TunerInstanceId)))
              {
                this.LogInfo("      group is no longer natural");
                group.ProductInstanceId = null;
                group.TunerInstanceId = null;
              }
            }
            if (group.Tuners.Count > 0)
            {
              foreach (ITVCard t in group.Tuners)
              {
                this.LogInfo("      name = {0}, type = {1}, external ID = {2}", t.Name, t.TunerType, t.ExternalId);
              }
            }
            group.Add(tuner);
            // Tuners can't be a member of more than one group.
            return group;
          }
        }
      }
      return null;
    }

    #endregion

    private void OnTunerRemoved(ITVCard tuner)
    {
      this.LogInfo("  remove...");
      this.LogInfo("    name        = {0}", tuner.Name);
      this.LogInfo("    external ID = {0}", tuner.ExternalId);
      this.LogInfo("    type        = {0}", tuner.TunerType);
      this.LogInfo("    product ID  = {0}", tuner.ProductInstanceId ?? "[null]");
      this.LogInfo("    tuner ID    = {0}", tuner.TunerInstanceId ?? "[null]");
      _eventListener.OnTunerRemoved(tuner);

      foreach (Dictionary<string, HashSet<string>> productTunerInstance in _naturalTunerGroups.Values)
      {
        foreach (HashSet<string> tunerInstance in productTunerInstance.Values)
        {
          tunerInstance.Remove(tuner.ExternalId);
        }
      }

      foreach (TunerGroup group in _configuredTunerGroups.Values)
      {
        group.Remove(tuner);
      }

      _firstDetectionTuners.Remove(tuner.ExternalId);
      _tuners.Remove(tuner.ExternalId);
      tuner.Dispose();
    }

    #endregion
  }
}