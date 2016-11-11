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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Upnp;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using MediaPortal.Common.Utils;
using UPnP.Infrastructure;
using UPnP.Infrastructure.CP;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.SSDP;
using DbTunerGroup = Mediaportal.TV.Server.TVDatabase.Entities.TunerGroup;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// This class is responsible for detecting tuners.
  /// </summary>
  public class TunerDetector : IDisposable
  {
    #region private classes

    private class DeviceInfo
    {
      public string Id;
      public string HardwareId;
      public string Name;
      public string Description;
      public string Manufacturer;
      public string Location;
      public string PhysicalDeviceObjectName;
      public NativeMethods.SP_DRVINFO_DATA DriverInfo;
    }

    #endregion

    #region constants

    private Guid SYSTEM_DEVICE_CLASS_MEDIA = new Guid(0x4d36e96c, 0xe325, 0x11ce, 0xbf, 0xc1, 0x08, 0x00, 0x2b, 0xe1, 0x03, 0x18);
    private static readonly Regex REGEX_DRIVER_DATE = new Regex(@"^(\d{4})(\d{2})(\d{2})");

    #endregion

    #region variables

    private bool _isStarted = false;

    // Used for detecting UPnP devices.
    private CPData _upnpControlPointData = null;
    private UPnPNetworkTracker _upnpAgent = null;
    private UPnPControlPoint _upnpControlPoint = null;
    private List<ITunerDetectorUpnp> _upnpDetectors = new List<ITunerDetectorUpnp>();

    // Used for detecting devices that are directly connected to the system.
    private SystemChangeNotifier _systemChangeNotifier = null;
    private IList<DeviceInfo> _systemDeviceDriverInfo = null;
    private IList<ITunerDetectorSystem> _systemDetectors = new List<ITunerDetectorSystem>();

    private HashSet<string> _firstDetectionTuners = new HashSet<string>();  // external IDs
    private HashSet<string> _knownUpnpRootDevices = new HashSet<string>();  // UUIDs
    private HashSet<string> _knownSystemTuners = new HashSet<string>();     // external IDs

    // tuner external ID => tuner
    private IDictionary<string, ITuner> _tuners = new Dictionary<string, ITuner>();
    // UPnP device UUID => [tuner external IDs] (used to implement UPnP tuner removal)
    private IDictionary<string, HashSet<string>> _upnpDeviceTuners = new Dictionary<string, HashSet<string>>();
    // product instance ID => tuner instance ID => tuner external IDs
    private IDictionary<string, Dictionary<string, HashSet<string>>> _naturalTunerGroups = new Dictionary<string, Dictionary<string, HashSet<string>>>();
    // database group ID => group
    private IDictionary<int, TunerGroup> _configuredTunerGroups = new Dictionary<int, TunerGroup>();

    // The listener that we notify when tuner changes are detected.
    private ITunerDetectionEventListener _eventListener = null;

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="listener">A listener that wishes to be notified about tuner detection events.</param>
    public TunerDetector(ITunerDetectionEventListener listener)
    {
      _eventListener = listener;

      // Setup power event handling.
      _systemChangeNotifier = new SystemChangeNotifier();
      _systemChangeNotifier.OnPowerBroadcast += OnSystemPowerManagementEvent;

      // Setup UPnP tuner detection.
      UPnPConfiguration.LOGGER = new Logger();
      _upnpControlPointData = new CPData();
      _upnpAgent = new UPnPNetworkTracker(_upnpControlPointData);
      _upnpAgent.RootDeviceAdded += OnUpnpRootDeviceAdded;
      _upnpAgent.RootDeviceRemoved += OnUpnpRootDeviceRemoved;
      _upnpControlPoint = new UPnPControlPoint(_upnpAgent);

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

    ~TunerDetector()
    {
      Dispose(false);
    }

    #endregion

    #region detector control

    /// <summary>
    /// Start tuner detection.
    /// </summary>
    public void Start()
    {
      lock (_tuners)
      {
        if (_isStarted)
        {
          return;
        }
        _isStarted = true;

        this.LogInfo("detector: starting tuner detection");

        // Start detecting tuners connected directly to the system.
        ThreadPool.QueueUserWorkItem(
          delegate
          {
            DetectSystemTuners();
          }
        );
        _systemChangeNotifier.OnDeviceInterfaceChange += OnSystemDeviceChangeEvent;

        // Start detecting UPnP tuners.
        // IMPORTANT: this parameter must be set to allow devices with many
        // sub-devices and/or services to be detected. The timer interval
        // specifies how long the SSDP controller has from first detection
        // of the root device SSDP packet until descriptions for all devices
        // and services have been requested, received and processed. DRI
        // tuners normally take about 5 seconds.
        SSDPClientController.EXPIRATION_TIMER_INTERVAL = 60000;
        // IMPORTANT: you should start the control point before the network
        // tracker.
        _upnpControlPoint.Start();
        _upnpAgent.Start();
        SearchForSupportedUpnpDevices();
      }
    }

    /// <summary>
    /// Reload the detector's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      lock (_tuners)
      {
        if (!_isStarted)
        {
          return;
        }
        this.LogDebug("detector: reload configuration");

        // Order of operations is important.

        // 1. Force-detect tuner changes.
        DetectSystemTuners();
        SearchForSupportedUpnpDevices();

        // 2. Remove old groups.
        HashSet<int> currentGroupIds = new HashSet<int>(_configuredTunerGroups.Keys);
        HashSet<int> unchangedGroupIds = new HashSet<int>();
        List<DbTunerGroup> newGroups = new List<DbTunerGroup>();
        IList<DbTunerGroup> dbGroups = TunerGroupManagement.ListAllTunerGroups();
        foreach (DbTunerGroup dbGroup in dbGroups)
        {
          unchangedGroupIds.Add(dbGroup.IdTunerGroup);
          if (!_configuredTunerGroups.ContainsKey(dbGroup.IdTunerGroup))
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
            foreach (ITuner tuner in group.Tuners)
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
        foreach (DbTunerGroup dbGroup in dbGroups)
        {
          TunerGroup group;
          if (_configuredTunerGroups.TryGetValue(dbGroup.IdTunerGroup, out group))
          {
            // Collect the external IDs for current members.
            HashSet<string> currentGroupMembers = new HashSet<string>();
            foreach (ITuner tuner in group.Tuners)
            {
              currentGroupMembers.Add(tuner.ExternalId);
            }

            // Record new and unchanged members.
            HashSet<string> unchangedGroupMembers = new HashSet<string>();
            foreach (Tuner dbTuner in dbGroup.Tuners)
            {
              string externalId = dbTuner.ExternalId;
              ITuner tuner;
              if (_tuners.TryGetValue(externalId, out tuner))
              {
                if (currentGroupMembers.Contains(externalId))
                {
                  unchangedGroupMembers.Add(externalId);
                }
                else
                {
                  changedGroupIds.Add(dbGroup.IdTunerGroup);
                  newGroupMembers.Add(externalId, dbGroup.IdTunerGroup);
                }
              }
            }

            // 3a. Remove old members.
            currentGroupMembers.ExceptWith(unchangedGroupMembers);
            foreach (string externalId in currentGroupMembers)
            {
              ITuner tuner;
              if (_tuners.TryGetValue(externalId, out tuner))
              {
                this.LogInfo("detector: removing tuner from group...");
                this.LogInfo("  group, name = {0}, product instance ID = {1}, tuner instance ID = {2}", group.Name, group.ProductInstanceId ?? "[null]", group.TunerInstanceId ?? "[null]");
                this.LogInfo("  tuner, name = {0}, product instance ID = {1}, tuner instance ID = {2}", tuner.Name, tuner.ProductInstanceId ?? "[null]", tuner.TunerInstanceId ?? "[null]");
                changedGroupIds.Add(dbGroup.IdTunerGroup);
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
          ITuner tuner;
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
            foreach (ITuner tuner in group.Tuners)
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
        foreach (DbTunerGroup dbGroup in newGroups)
        {
          TunerGroup group = new TunerGroup(dbGroup);
          bool isFirst = true;
          string commonProductInstanceId = null;
          string commonTunerInstanceId = null;
          foreach (Tuner dbTuner in dbGroup.Tuners)
          {
            ITuner tuner;
            if (_tuners.TryGetValue(dbTuner.ExternalId, out tuner))
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
            foreach (ITuner tuner in group.Tuners)
            {
              this.LogInfo("  tuner, name = {0}, product instance ID = {1}, tuner instance ID = {2}", tuner.Name, tuner.ProductInstanceId ?? "[null]", tuner.TunerInstanceId ?? "[null]");
            }
            _configuredTunerGroups.Add(dbGroup.IdTunerGroup, group);
          }
        }
      }
    }

    /// <summary>
    /// Reset and restart tuner detection.
    /// </summary>
    public void Reset()
    {
      lock (_tuners)
      {
        Stop();
        RemoveAllTuners();
        Start();
      }
    }

    /// <summary>
    /// Stop tuner detection.
    /// </summary>
    public void Stop()
    {
      lock (_tuners)
      {
        if (!_isStarted)
        {
          return;
        }
        this.LogInfo("detector: stopping tuner detection...");
        _upnpAgent.Close();
        _upnpControlPoint.Close();
        _systemChangeNotifier.OnDeviceInterfaceChange -= OnSystemDeviceChangeEvent;
        _isStarted = false;
      }
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (!isDisposing)
      {
        return;
      }

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

      if (_systemChangeNotifier != null)
      {
        _systemChangeNotifier.OnPowerBroadcast -= OnSystemPowerManagementEvent;
        _systemChangeNotifier.Dispose();
        _systemChangeNotifier = null;
      }
    }

    #endregion

    #region power event handling

    private void OnSystemPowerManagementEvent(NativeMethods.PBT_MANAGEMENT_EVENT eventType)
    {
      if (eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMSUSPEND)
      {
        this.LogInfo("detector: suspending...");
        Stop();
        RemoveAllTuners();
      }
      else if (
        !_isStarted &&
        (
          eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMEAUTOMATIC ||
          eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMECRITICAL ||
          eventType == NativeMethods.PBT_MANAGEMENT_EVENT.PBT_APMRESUMESUSPEND
        )
      )
      {
        ThreadPool.QueueUserWorkItem(
          delegate
          {
            this.LogInfo("detector: resuming...");
            Start();
            Thread.Sleep(10000);
            SearchForSupportedUpnpDevices();
          }
        );
      }
    }

    #endregion

    #region system device detection

    private void OnSystemDeviceChangeEvent(NativeMethods.DBT_MANAGEMENT_EVENT eventType, Guid classGuid, string devicePath)
    {
      if (!_isStarted || string.IsNullOrEmpty(devicePath))
      {
        return;
      }

      // https://msdn.microsoft.com/en-us/library/windows/desktop/aa363431%28v=vs.85%29.aspx
      // Be sure to handle Plug and Play device events as quickly as possible.
      // Otherwise, the system may become unresponsive. If your event handler
      // is to perform an operation that may block execution (such as I/O), it
      // is best to start another thread to perform the operation
      // asynchronously.
      //
      // We don't want to cause problems like this:
      // http://www.codeproject.com/Articles/35212/WM-DEVICECHANGE-problem
      if (eventType == NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEARRIVAL)
      {
        ThreadPool.QueueUserWorkItem(
          delegate
          {
            foreach (ITunerDetectorSystem detector in _systemDetectors)
            {
              ICollection<ITuner> tuners = detector.DetectTuners(classGuid, devicePath);
              if (tuners == null || tuners.Count == 0)
              {
                continue;
              }
              lock (_tuners)
              {
                UpdateSystemDeviceInfo();
                foreach (ITuner tuner in tuners)
                {
                  if (_tuners.ContainsKey(tuner.ExternalId))
                  {
                    // Not a new tuner.
                    continue;
                  }
                  OnTunerDetected(tuner);
                  _knownSystemTuners.Add(tuner.ExternalId);
                }
              }
            }
          }
        );
      }
      else if (eventType == NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEREMOVECOMPLETE)
      {
        ThreadPool.QueueUserWorkItem(
          delegate
          {
            lock (_tuners)
            {
              ITuner tuner;
              HashSet<string> removedTunerExternalIds = new HashSet<string>();
              foreach (string tunerExternalId in _knownSystemTuners)
              {
                if (tunerExternalId.ToLowerInvariant().Contains(devicePath.ToLowerInvariant()) && _tuners.TryGetValue(tunerExternalId, out tuner))
                {
                  OnTunerRemoved(tuner);
                  removedTunerExternalIds.Add(tunerExternalId);
                }
              }
              _knownSystemTuners.ExceptWith(removedTunerExternalIds);
            }
          }
        );
      }
    }

    private void DetectSystemTuners()
    {
      lock (_tuners)
      {
        if (!_isStarted)
        {
          return;
        }
        this.LogDebug("detector: detecting system tuners...");
        UpdateSystemDeviceInfo();

        HashSet<string> currentSystemTuners = new HashSet<string>();
        foreach (ITunerDetectorSystem detector in _systemDetectors)
        {
          ICollection<ITuner> tuners = detector.DetectTuners();
          if (tuners != null && tuners.Count > 0)
          {
            foreach (ITuner tuner in tuners)
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
        }

        // Remove the tuners that are no longer available.
        _knownSystemTuners.ExceptWith(currentSystemTuners);
        foreach (string removedTunerExternalId in _knownSystemTuners)
        {
          ITuner tuner;
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

    private void SearchForSupportedUpnpDevices()
    {
      _upnpAgent.SharedControlPointData.SSDPController.SearchDeviceByDeviceTypeVersion("schemas-opencable-com:service:Tuner", "1", null);
      _upnpAgent.SharedControlPointData.SSDPController.SearchDeviceByDeviceTypeVersion("urn:ses-com:device:SatIPServer", "1", null);
    }

    private void OnUpnpRootDeviceAdded(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null || rootDescriptor.SSDPRootEntry == null || rootDescriptor.SSDPRootEntry.RootDeviceUUID == null || rootDescriptor.State != RootDescriptorState.Ready)
      {
        return;
      }
      lock (_tuners)
      {
        if (!_isStarted)
        {
          return;
        }
        if (!_knownUpnpRootDevices.Add(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
        {
          this.LogWarn("detector: re-detecting known root device {0}", rootDescriptor.SSDPRootEntry.RootDeviceUUID);
        }

        DeviceDescriptor rootDeviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
        if (rootDeviceDescriptor != null)
        {
          foreach (ITunerDetectorUpnp detector in _upnpDetectors)
          {
            ICollection<ITuner> tuners = detector.DetectTuners(rootDeviceDescriptor, _upnpControlPoint);
            if (tuners != null && tuners.Count > 0)
            {
              LogUpnpDeviceInfo(rootDeviceDescriptor);
              HashSet<string> tunerExternalIds = new HashSet<string>();
              foreach (ITuner tuner in tuners)
              {
                OnTunerDetected(tuner);
                tunerExternalIds.Add(tuner.ExternalId);
              }
              _upnpDeviceTuners.Add(rootDeviceDescriptor.DeviceUUID, tunerExternalIds);
            }
          }
        }
      }
    }

    private void OnUpnpRootDeviceRemoved(RootDescriptor rootDescriptor)
    {
      if (rootDescriptor == null || rootDescriptor.SSDPRootEntry == null || rootDescriptor.SSDPRootEntry.RootDeviceUUID == null)
      {
        return;
      }

      lock (_tuners)
      {
        if (!_isStarted)
        {
          return;
        }
        if (!_knownUpnpRootDevices.Remove(rootDescriptor.SSDPRootEntry.RootDeviceUUID))
        {
          this.LogWarn("detector: detecting removal of unknown root device {0}", rootDescriptor.SSDPRootEntry.RootDeviceUUID);
        }

        DeviceDescriptor rootDeviceDescriptor = DeviceDescriptor.CreateRootDeviceDescriptor(rootDescriptor);
        HashSet<string> tunerExternalIds;
        if (rootDeviceDescriptor != null && _upnpDeviceTuners.TryGetValue(rootDeviceDescriptor.DeviceUUID, out tunerExternalIds))
        {
          this.LogInfo("detector: UPnP device removed");
          LogUpnpDeviceInfo(rootDeviceDescriptor);
          foreach (string t in tunerExternalIds)
          {
            ITuner tuner;
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

    #region system device information

    private void UpdateSystemDeviceInfo()
    {
      // This information can be retrieved with a ManagementObjectSearcher
      // and query:
      // SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'MEDIA'
      // However that method seems to be significantly slower (5+ seconds
      // sometimes) particularly on XP.
      _systemDeviceDriverInfo = new List<DeviceInfo>(20);

      Guid mediaClass = NativeMethods.GUID_DEVCLASS_MEDIA;
      IntPtr devInfoSet = NativeMethods.SetupDiGetClassDevs(ref mediaClass, null, IntPtr.Zero, NativeMethods.DiGetClassFlags.DIGCF_PRESENT);
      if (devInfoSet == IntPtr.Zero || devInfoSet == NativeMethods.INVALID_HANDLE_VALUE)
      {
        this.LogError("detector: failed to get system device information set, error = {0}", Marshal.GetLastWin32Error());
        return;
      }

      try
      {
        int error;
        uint index = 0;
        NativeMethods.SP_DEVINFO_DATA devInfo = new NativeMethods.SP_DEVINFO_DATA();
        devInfo.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DEVINFO_DATA));
        while (NativeMethods.SetupDiEnumDeviceInfo(devInfoSet, index++, ref devInfo))
        {
          // Read device info.
          StringBuilder deviceId = new StringBuilder((int)NativeMethods.MAX_DEVICE_ID_LEN);
          uint requiredSize;
          if (!NativeMethods.SetupDiGetDeviceInstanceId(devInfoSet, ref devInfo, deviceId, NativeMethods.MAX_DEVICE_ID_LEN, out requiredSize))
          {
            this.LogWarn("detector: failed to get system device instance ID, error = {0}, dev inst = {1}", Marshal.GetLastWin32Error(), devInfo.DevInst);
            continue;
          }

          DeviceInfo deviceInfo = new DeviceInfo();
          deviceInfo.Id = deviceId.ToString();
          deviceInfo.HardwareId = GetSystemDeviceProperty(devInfoSet, devInfo, NativeMethods.DeviceProperty.SPDRP_HARDWAREID);
          deviceInfo.Name = GetSystemDeviceProperty(devInfoSet, devInfo, NativeMethods.DeviceProperty.SPDRP_FRIENDLYNAME);
          deviceInfo.Description = GetSystemDeviceProperty(devInfoSet, devInfo, NativeMethods.DeviceProperty.SPDRP_DEVICEDESC);
          deviceInfo.Manufacturer = GetSystemDeviceProperty(devInfoSet, devInfo, NativeMethods.DeviceProperty.SPDRP_MFG);
          deviceInfo.Location = GetSystemDeviceProperty(devInfoSet, devInfo, NativeMethods.DeviceProperty.SPDRP_LOCATION_INFORMATION);
          deviceInfo.PhysicalDeviceObjectName = GetSystemDeviceProperty(devInfoSet, devInfo, NativeMethods.DeviceProperty.SPDRP_PHYSICAL_DEVICE_OBJECT_NAME);
          _systemDeviceDriverInfo.Add(deviceInfo);

          // Read driver info.
          NativeMethods.SP_DEVINSTALL_PARAMS installParams = new NativeMethods.SP_DEVINSTALL_PARAMS();
          installParams.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DEVINSTALL_PARAMS));
          installParams.Flags = 0;
          installParams.FlagsEx = NativeMethods.DeviceInstallFlagsEx.DI_FLAGSEX_INSTALLEDDRIVER;
          installParams.hwndParent = IntPtr.Zero;
          installParams.InstallMsgHandler = IntPtr.Zero;
          installParams.InstallMsgHandlerContext = IntPtr.Zero;
          installParams.FileQueue = IntPtr.Zero;
          installParams.ClassInstallReserved = UIntPtr.Zero;
          installParams.Reserved = 0;
          installParams.DriverPath = null;
          if (!NativeMethods.SetupDiSetDeviceInstallParams(devInfoSet, ref devInfo, ref installParams))
          {
            this.LogError("detector: failed to set system device install parameters, error = {0}, device ID = {1}, device name = {2}", Marshal.GetLastWin32Error(), deviceInfo.Id, deviceInfo.Name ?? deviceInfo.Description ?? "[null]");
            continue;
          }

          if (!NativeMethods.SetupDiBuildDriverInfoList(devInfoSet, ref devInfo, NativeMethods.DriverType.SPDIT_COMPATDRIVER))
          {
            this.LogError("detector: failed to get system device driver information set, error = {0}, device ID = {1}, device name = {2}", Marshal.GetLastWin32Error(), deviceInfo.Id, deviceInfo.Name ?? deviceInfo.Description ?? "[null]");
            continue;
          }

          try
          {
            NativeMethods.SP_DRVINFO_DATA driverInfo = new NativeMethods.SP_DRVINFO_DATA();
            driverInfo.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DRVINFO_DATA));
            if (!NativeMethods.SetupDiEnumDriverInfo(devInfoSet, ref devInfo, NativeMethods.DriverType.SPDIT_COMPATDRIVER, 0, ref driverInfo))
            {
              this.LogError("detector: failed to get system device driver information, error = {0}, device ID = {1}, device name = {2}", Marshal.GetLastWin32Error(), deviceInfo.Id, deviceInfo.Name ?? deviceInfo.Description ?? "[null]");
              driverInfo.cbSize = 0;
              continue;
            }
            deviceInfo.DriverInfo = driverInfo;
          }
          finally
          {
            NativeMethods.SetupDiDestroyDriverInfoList(devInfoSet, ref devInfo, NativeMethods.DriverType.SPDIT_COMPATDRIVER);
          }
        }

        error = Marshal.GetLastWin32Error();
        if (error != (int)NativeMethods.SystemErrorCode.ERROR_NO_MORE_ITEMS)
        {
          this.LogError("detector: failed to get next system device information, error = {0}", error);
        }
      }
      finally
      {
        NativeMethods.SetupDiDestroyDeviceInfoList(devInfoSet);
      }
    }

    private static string GetSystemDeviceProperty(IntPtr devInfoSet, NativeMethods.SP_DEVINFO_DATA devInfo, NativeMethods.DeviceProperty property)
    {
      uint dataType;
      uint requiredSize;
      if (NativeMethods.SetupDiGetDeviceRegistryProperty(devInfoSet, ref devInfo, property, out dataType, IntPtr.Zero, 0, out requiredSize))
      {
        Log.Error("detector: failed to get system device property data length, error = {0}, property = {1}, required size = {2}, dev inst = {3}", Marshal.GetLastWin32Error(), property, requiredSize, devInfo.DevInst);
        return null;
      }

      int error = Marshal.GetLastWin32Error();
      if (error == (int)NativeMethods.SystemErrorCode.ERROR_INVALID_DATA)
      {
        // property does not exist
        return null;
      }
      else if (error != (int)NativeMethods.SystemErrorCode.ERROR_INSUFFICIENT_BUFFER || requiredSize == 0)
      {
        // ERROR_INSUFFICIENT_BUFFER is the ***SUCCESS*** result when
        // retrieving the property data length.
        Log.Error("detector: failed to get system device property data length, error = {0}, property = {1}, required size = {2}, dev inst = {3}", Marshal.GetLastWin32Error(), property, requiredSize, devInfo.DevInst);
        return null;
      }

      IntPtr buffer = Marshal.AllocHGlobal((int)requiredSize);
      try
      {
        uint dummy;
        if (!NativeMethods.SetupDiGetDeviceRegistryProperty(devInfoSet, ref devInfo, property, out dataType, buffer, requiredSize, out dummy))
        {
          Log.Error("detector: failed to get system device property value, error = {0}, property = {1}, required size = {2}, dev inst = {3}", Marshal.GetLastWin32Error(), property, requiredSize, devInfo.DevInst);
          return null;
        }
        return Marshal.PtrToStringUni(buffer);
      }
      finally
      {
        Marshal.FreeHGlobal(buffer);
      }
    }

    #endregion

    #region common tuner add/remove logic

    private void OnTunerDetected(ITuner tuner)
    {
      this.LogInfo("  add...");
      this.LogInfo("    name        = {0}", tuner.Name);
      this.LogInfo("    external ID = {0}", tuner.ExternalId);
      this.LogInfo("    standards   = {0}", tuner.SupportedBroadcastStandards);
      this.LogInfo("    product ID  = {0}", tuner.ProductInstanceId ?? "[null]");
      this.LogInfo("    tuner ID    = {0}", tuner.TunerInstanceId ?? "[null]");
      string productName = string.Empty;
      if (tuner.ProductInstanceId != null)
      {
        productName = string.Format("Product Instance {0}", tuner.ProductInstanceId);

        IList<DeviceInfo> possibleInfoMatches = new List<DeviceInfo>();
        foreach (DeviceInfo info in _systemDeviceDriverInfo)
        {
          if (info.Id.ToLowerInvariant().Contains(tuner.ProductInstanceId))
          {
            possibleInfoMatches.Add(info);
          }
        }
        if (possibleInfoMatches.Count == 1)
        {
          DeviceInfo info = possibleInfoMatches[0];
          if (!string.IsNullOrEmpty(info.Name))
          {
            productName = info.Name;
          }
          else if (!string.IsNullOrEmpty(info.Description))
          {
            productName = info.Description;
          }
          this.LogInfo("    device...");
          this.LogInfo("      ID           = {0}", info.Id);
          this.LogInfo("      hardware ID  = {0}", info.HardwareId ?? "[null]");
          this.LogInfo("      name         = {0}", info.Name ?? "[null]");
          this.LogInfo("      description  = {0}", info.Description ?? "[null]");
          this.LogInfo("      manufacturer = {0}", info.Manufacturer ?? "[null]");
          this.LogInfo("      location     = {0}", info.Location ?? "[null]");
          this.LogInfo("      PDO name     = {0}", info.PhysicalDeviceObjectName ?? "[null]");

          if (info.DriverInfo.cbSize > 0)
          {
            NativeMethods.SP_DRVINFO_DATA driverInfo = info.DriverInfo;
            this.LogInfo("    driver...");
            this.LogInfo("      description  = {0}", driverInfo.Description ?? "[null]");
            this.LogInfo("      manufacturer = {0}", driverInfo.MfgName ?? "[null]");
            this.LogInfo("      provider     = {0}", driverInfo.ProviderName ?? "[null]");
            this.LogInfo("      version      = {0}.{1}.{2}.{3}", ((driverInfo.DriverVersion >> 48) & 0xffff), ((driverInfo.DriverVersion >> 32) & 0xffff), ((driverInfo.DriverVersion >> 16) & 0xffff), (driverInfo.DriverVersion & 0xffff));
            NativeMethods.SYSTEMTIME systemTime;
            if (NativeMethods.FileTimeToSystemTime(ref driverInfo.DriverDate, out systemTime))
            {
              this.LogInfo("      date         = {0}-{1:d2}-{2:d2}", systemTime.wYear, systemTime.wMonth, systemTime.wDay);
            }
          }
        }
      }
      _tuners.Add(tuner.ExternalId, tuner);

      // Detect the naturally related tuners. These are tuners with the same product and tuner
      // instance ID.
      HashSet<string> relatedTuners = FindRelatedTuners(tuner);

      // Find or create a group for the tuner if appropriate.
      TunerGroup group = null;
      Tuner tunerDbSettings = TunerManagement.GetTunerByExternalId(tuner.ExternalId, TunerRelation.None);
      if (tunerDbSettings == null)
      {
        // First ever detection. Create the tuner settings.
        this.LogInfo("    new tuner...");
        _firstDetectionTuners.Add(tuner.ExternalId);
        tunerDbSettings = new Tuner
        {
          ExternalId = tuner.ExternalId,
          Name = tuner.Name,
          IsEnabled = true,
          Priority = 1,
          UseForEpgGrabbing = (tuner.SupportedBroadcastStandards & (BroadcastStandard.Atsc | BroadcastStandard.MaskDvb | BroadcastStandard.MaskIsdb)) != 0,
          Preload = false,
          AlwaysSendDiseqcCommands = false,
          UseConditionalAccess = (tuner.SupportedBroadcastStandards & BroadcastStandard.MaskDigital) != 0,
          ConditionalAccessProviders = string.Empty,    // any
          CamType = (int)CamType.Default,
          DecryptLimit = 1,
          MultiChannelDecryptMode = (int)MultiChannelDecryptMode.List,
          IdleMode = (int)TunerIdleMode.Stop,
          BdaNetworkProvider = (int)(Environment.OSVersion.Version.Major >= 6 ? BdaNetworkProvider.Generic : BdaNetworkProvider.Specific),
          PidFilterMode = (int)PidFilterMode.Automatic,
          UseCustomTuning = false,
          SupportedBroadcastStandards = (int)tuner.SupportedBroadcastStandards,
          TsMuxerInputDumpMask = 0,
          TsWriterInputDumpMask = 0,
          DisableTsWriterCrcChecking = false
        };

        // If we have product/tuner instance information and detected the tuner is a member of a
        // natural group...
        if (tuner.ProductInstanceId != null && tuner.TunerInstanceId != null && relatedTuners != null && relatedTuners.Count > 1)
        {
          // Automatically add the tuner to an existing tuner group if the group product and tuner
          // instance information matches.
          group = FindExistingNaturalGroup(tuner.ProductInstanceId, tuner.TunerInstanceId);

          // ...or if that doesn't apply, automatically add the tuner to an existing tuner group if
          // the group contains *all* the naturally related tuners. The assumption in this case is
          // that our group detection failed to automatically detect all the tuners in the group.
          bool foundAnyRelatedTunerInAnyGroup = false;
          if (group == null)
          {
            group = FindExistingExtendedNaturalGroup(tuner.ExternalId, relatedTuners, out foundAnyRelatedTunerInAnyGroup);
          }

          // Finally, if all of the tuners in the natural group are new and not in any other group
          // then create a new group.
          if (group == null && !foundAnyRelatedTunerInAnyGroup)
          {
            group = CreateNewGroupOnFirstDetection(tuner.ExternalId, relatedTuners, productName);
          }

          if (group != null)
          {
            group.Add(tuner);
            tunerDbSettings.IdTunerGroup = group.TunerGroupId;
          }
        }

        tunerDbSettings = TunerManagement.SaveTuner(tunerDbSettings);
      }
      else
      {
        // This tuner has been detected previously. Check if it is a member of an existing tuner
        // group.
        this.LogInfo("    existing tuner, ID = {0}...", tunerDbSettings.IdTuner);
        group = AddExistingTunerToConfiguredGroup(tuner);
      }

      ITunerInternal internalTuner = tuner as ITunerInternal;
      if (internalTuner != null)
      {
        internalTuner.Group = group;
      }

      // Load the tuner's configuration. This also triggers preloading.
      tuner.ReloadConfiguration();

      _eventListener.OnTunerAdded(tuner);
    }

    #region tuner group handling

    private HashSet<string> FindRelatedTuners(ITuner tuner)
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
              ITuner t = _tuners[id];
              this.LogInfo("      name = {0}, standards = [{1}], external ID = {2}", t.Name, t.SupportedBroadcastStandards, id);
            }
          }
          relatedTuners.Add(tuner.ExternalId);
        }
      }
      return relatedTuners;
    }

    private TunerGroup FindExistingNaturalGroup(string productInstanceId, string tunerInstanceId)
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
        if (string.Equals(group.ProductInstanceId, productInstanceId) && string.Equals(group.TunerInstanceId, tunerInstanceId))
        {
          this.LogInfo("    detected existing tuner group with matching information, name = {0}...", group.Name);
          return group;
        }
      }
      return null;
    }

    private TunerGroup FindExistingExtendedNaturalGroup(string externalId, HashSet<string> relatedTuners, out bool foundAnyRelatedTunerInAnyGroup)
    {
      foundAnyRelatedTunerInAnyGroup = false;
      foreach (TunerGroup group in _configuredTunerGroups.Values)
      {
        // Only look at mixed product/tuner instance groups. FindExistingNaturalGroup() already
        // checked and failed to find a non-extended/pure group.
        if (group.ProductInstanceId == null && group.TunerInstanceId == null)
        {
          // For each naturally related tuner that is not this tuner...
          foreach (string relatedTunerExternalId1 in relatedTuners)
          {
            if (!relatedTunerExternalId1.Equals(externalId))
            {
              if (group.Tuners.Any(x => relatedTunerExternalId1.Equals(x.ExternalId)))
              {
                // Okay, we found a group containing one of the related tuners. If all of the
                // related tuners are in this group then we'll add the tuner to this group.
                // Otherwise this tuner won't get a group.
                foundAnyRelatedTunerInAnyGroup = true;
                foreach (string relatedTunerExternalId2 in relatedTuners)
                {
                  if (!relatedTunerExternalId2.Equals(externalId) && !relatedTunerExternalId2.Equals(relatedTunerExternalId1))
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
                return group;
              }
            }
          }
        }
      }
      return null;
    }

    private TunerGroup CreateNewGroupOnFirstDetection(string externalId, HashSet<string> relatedTuners, string productName)
    {
      // Check that this is the first time all the related tuners have been
      // detected.
      string anyRelatedTunerExternalId = null;
      foreach (string relatedTunerExternalId in relatedTuners)
      {
        anyRelatedTunerExternalId = relatedTunerExternalId;
        if (!_firstDetectionTuners.Contains(relatedTunerExternalId))
        {
          return null;
        }
      }
      ITuner anyRelatedTuner = _tuners[anyRelatedTunerExternalId];

      // Create the new group.
      DbTunerGroup dbGroup = new DbTunerGroup();
      dbGroup.Name = string.Format("{0} Tuner {1}", productName, anyRelatedTuner.TunerInstanceId);
      dbGroup = TunerGroupManagement.SaveTunerGroup(dbGroup);
      TunerGroup group = new TunerGroup(dbGroup);
      group.ProductInstanceId = anyRelatedTuner.ProductInstanceId;
      group.TunerInstanceId = anyRelatedTuner.TunerInstanceId;
      _configuredTunerGroups.Add(group.TunerGroupId, group);

      // Link all the related tuners except this tuner to the new group.
      foreach (string relatedTunerExternalId in relatedTuners)
      {
        if (!string.Equals(externalId, relatedTunerExternalId))
        {
          Tuner relatedTuner = TunerManagement.GetTunerByExternalId(relatedTunerExternalId, TunerRelation.None);
          if (relatedTuner != null)
          {
            relatedTuner.IdTunerGroup = dbGroup.IdTunerGroup;
            TunerManagement.SaveTuner(relatedTuner);
          }
          group.Add(_tuners[relatedTunerExternalId]);
        }
      }
      this.LogInfo("    creating new tuner group, name = {0}...", group.Name);
      return group;
    }

    private TunerGroup AddExistingTunerToConfiguredGroup(ITuner tuner)
    {
      foreach (DbTunerGroup dbGroup in TunerGroupManagement.ListAllTunerGroups())
      {
        foreach (Tuner dbTuner in dbGroup.Tuners)
        {
          // Does the tuner belong to this group?
          if (string.Equals(dbTuner.ExternalId, tuner.ExternalId))
          {
            // Find and update our local group info.
            TunerGroup group;
            if (!_configuredTunerGroups.TryGetValue(dbGroup.IdTunerGroup, out group))
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
              foreach (ITuner t in group.Tuners)
              {
                this.LogInfo("      name = {0}, standards = [{1}], external ID = {2}", t.Name, t.SupportedBroadcastStandards, t.ExternalId);
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

    private void OnTunerRemoved(ITuner tuner)
    {
      this.LogInfo("  remove...");
      this.LogInfo("    name        = {0}", tuner.Name);
      this.LogInfo("    external ID = {0}", tuner.ExternalId);
      this.LogInfo("    standards   = {0}", tuner.SupportedBroadcastStandards);
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

    private void RemoveAllTuners()
    {
      lock (_tuners)
      {
        _firstDetectionTuners.Clear();
        _knownSystemTuners.Clear();
        _knownUpnpRootDevices.Clear();
        _upnpDeviceTuners.Clear();
        _naturalTunerGroups.Clear();
        _configuredTunerGroups.Clear();

        List<ITuner> tuners = new List<ITuner>(_tuners.Values);
        foreach (ITuner tuner in tuners)
        {
          OnTunerRemoved(tuner);
        }
        _tuners.Clear();
      }
    }

    #endregion
  }
}