using System;
using System.IO;
using System.Management;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using TVCapture;
using MediaPortal.GUI.Library;

namespace MediaPortal.TV.Recording
{
  public class GraphHelper
  {
    #region variables
    private bool _definitionLoaded = false;
    private CaptureCardDefinition _captureCardDefinition = new CaptureCardDefinition();
    private string _deviceId;
    //private string _captureName;
    private string _commercialName;
    #endregion


    #region properties

    public ArrayList TvFilterDefinitions
    {
      get
      {
        if (_captureCardDefinition == null) return null;
        if (_captureCardDefinition.Tv == null) return null;
        return _captureCardDefinition.Tv.FilterDefinitions;
      }
    }

    /// <summary>
    /// #MW#
    /// </summary>
    public ArrayList TvConnectionDefinitions
    {
      get
      {
        if (_captureCardDefinition == null) return null;
        if (_captureCardDefinition.Tv == null) return null;
        return _captureCardDefinition.Tv.ConnectionDefinitions;
      }
    }
    /// <summary>
    /// #MW#
    /// </summary>
    public InterfaceDefinition TvInterfaceDefinition
    {
      get
      {
        if (_captureCardDefinition.Tv == null) return null;
        return _captureCardDefinition.Tv.InterfaceDefinition;
      }
    }

    /// <summary>
    /// #MW#
    /// </summary>
    public ArrayList RadioFilterDefinitions
    {
      get
      {
        if (_captureCardDefinition.Radio == null) return null;
        return _captureCardDefinition.Radio.FilterDefinitions;
      }
    }

    /// <summary>
    /// #MW#
    /// </summary>
    public ArrayList RadioConnectionDefinitions
    {
      get
      {
        if (_captureCardDefinition.Radio == null) return null;
        return _captureCardDefinition.Radio.ConnectionDefinitions;
      }
    }

    /// <summary>
    /// #MW#
    /// </summary>
    public InterfaceDefinition RadioInterfaceDefinition
    {
      get
      {
        if (_captureCardDefinition.Radio == null) return null;
        return _captureCardDefinition.Radio.InterfaceDefinition;
      }
    }


    /// <summary>
    /// #MW#
    /// </summary>
    public string DeviceId
    {
      get
      {
        return _deviceId;
      }
      set
      {
        if (_deviceId != value)
        {
          _deviceId = value;
          _definitionLoaded = false;
        }
      }
    }/*

    /// <summary>
    /// #MW#
    /// </summary>
    public string CaptureName
    {
      get { return _captureName; }
      set {
        if (_captureName != value)
        {
          _definitionLoaded = false;
          _captureName = value;
        }
      }
    }*/
    /// <summary>
    /// #MW#
    /// </summary>
    public string CommercialName
    {
      get
      {
        return _commercialName;
      }
      set
      {
        if (_commercialName != value)
        {
          _commercialName = value;
          _definitionLoaded = false;
        }
      }
    }
    /// <summary>
    /// #MW#
    /// </summary>
    public CapabilityDefinition Capabilities
    {
      get { return _captureCardDefinition.Capabilities; }
    }
    #endregion

    #region public members
    /// <summary>
    /// This method:
    /// 1. finds the device entry  in the registry key : SYSTEM\CurrentControlSet\Enum\[DEVICE moniker]
    /// 2. looks at the 'Service' subkey which points to the service for this device.
    /// 3. then looks at SYSTEM\CurrentControlSet\Services\[ServiceName]\Enum
    /// 4. reads the 'Count' subkey which indicates how many instances there are of this device
    /// 5. checks each instance in SYSTEM\CurrentControlSet\Services\[ServiceName]\Enum\
    ///    to find the correct instance...
    /// </summary>
    /// <param name="monikerName">device moniker</param>
    /// <example>ven_4444&dev_0016&subsys_40090070&rev_01#4&2e98101c&0&68f0</example>
    /// <returns>instance for this device moniker (0-count)</returns>
    /// 
    /// Registry layout:
    /// SYSTEM\CurrentControlSet\Enum\[DEVICE moniker]
    ///     Service=[ServiceName]
    ///     
    /// SYSTEM\CurrentControlSet\Services\[ServiceName]\Enum\
    ///     Count=[number of instances]
    ///     0=[moniker of instance 0]
    ///     1=[moniker of instance 1]
    ///     ...
    public int FindInstanceForDevice(string monikerName)
    {
      Log.Write("    FindInstance:{0}", monikerName);

      // find the device entry in SYSTEM\CurrentControlSet\Enum\[device moniker]
      int pos1 = monikerName.IndexOf("#");
      int pos2 = monikerName.LastIndexOf("#");
      string left = monikerName.Substring(0, pos1);
      string mid = monikerName.Substring(pos1 + 1, (pos2 - pos1) - 1);
      mid = mid.Replace("#", "/");
      string right = monikerName.Substring(pos2 + 1);
      string registryKeyName = left + @"\" + mid + @"\" + right;

      if (registryKeyName.StartsWith(@"@device:pnp:\\?\"))
        registryKeyName = registryKeyName.Substring(@"@device:pnp:\\?\".Length);
      registryKeyName = @"SYSTEM\CurrentControlSet\Enum\" + registryKeyName;
      Log.Write("      key:{0}", registryKeyName);
      RegistryKey hklm = Registry.LocalMachine;
      RegistryKey subkey = hklm.OpenSubKey(registryKeyName, false);
      if (subkey != null)
      {
        //Get the name of the service which handles this device
        string serviceName = (string)subkey.GetValue("Service");

        //next open the service entry in SYSTEM\CurrentControlSet\Services\[Service name\enum
        Log.Write("        serviceName:{0}", serviceName);
        registryKeyName = @"SYSTEM\CurrentControlSet\Services\" + serviceName + @"\Enum";
        Log.Write("        key:{0}", registryKeyName);
        subkey = hklm.OpenSubKey(registryKeyName, false);
        if (subkey != null)
        {
          // get the number of instances for the device
          Int32 count = (Int32)subkey.GetValue("Count");

          Log.Write("        Number of cards:{0}", count);
          for (int i = 0; i < count; i++)
          {
            string moniker = (string)subkey.GetValue(i.ToString());
            moniker = moniker.Replace(@"\", "#");
            moniker = moniker.Replace(@"/", "#");
            Log.Write("          card#{0}={1}", i, moniker);
          }

          // for each instance
          for (int i = 0; i < count; i++)
          {
            //get the moniker
            string moniker = (string)subkey.GetValue(i.ToString());
            moniker = moniker.Replace(@"\", "#");
            moniker = moniker.Replace(@"/", "#");

            // and check if its the same as the device moniker
            if (monikerName.ToLower().IndexOf(moniker.ToLower()) >= 0)
            {
              //yes then return this instance
              Log.Write("        using card:#{0}", i);
              subkey.Close();
              hklm.Close();
              return i;
            }
          }
        }
        else
        {
          Log.Write("        using default card:0 (subkey not found)");
          hklm.Close();
          return 0;
        }
        subkey.Close();
      }
      hklm.Close();
      return -1;
    }
    /// <summary>
    /// This method:
    /// 1. finds the device entry  in the registry key : SYSTEM\CurrentControlSet\Enum\[DEVICE moniker]
    /// 2. looks at the 'Service' subkey which points to the service for this device.
    /// 3. then looks at SYSTEM\CurrentControlSet\Services\[ServiceName]\Enum
    /// 4. reads the 'Count' subkey which indicates how many instances there are of this device
    /// 5. returns the moniker for the instance requested.
    /// </summary>
    /// <param name="monikerName">device moniker</param>
    /// <example>ven_4444&dev_0016&subsys_40090070&rev_01#4&2e98101c&0&68f0</example>
    /// <param name="monikerName">instance instance for this device moniker (0-count)</param>
    /// <returns>moniker for this instance</returns>
    /// 
    /// Registry layout:
    /// SYSTEM\CurrentControlSet\Enum\[DEVICE moniker]
    ///     Service=[ServiceName]
    ///     
    /// SYSTEM\CurrentControlSet\Services\[ServiceName]\Enum\
    ///     Count=[number of instances]
    ///     0=[moniker of instance 0]
    ///     1=[moniker of instance 1]
    ///     ...
    public string FindUniqueFilter(string monikerName, int instance)
    {
      Log.Write("    FindUniqueFilter:card#{0} filter:{1}", instance, monikerName);

      int pos1 = monikerName.IndexOf("#");
      int pos2 = monikerName.LastIndexOf("#");
      string left = monikerName.Substring(0, pos1);
      string mid = monikerName.Substring(pos1 + 1, (pos2 - pos1) - 1);
      mid = mid.Replace("#", "/");
      string right = monikerName.Substring(pos2 + 1);
      string registryKeyName = left + @"\" + mid + @"\" + right;

      if (registryKeyName.StartsWith(@"@device:pnp:\\?\"))
        registryKeyName = registryKeyName.Substring(@"@device:pnp:\\?\".Length);

      registryKeyName = @"SYSTEM\CurrentControlSet\Enum\" + registryKeyName;
      Log.Write("        key:{0}", registryKeyName);
      RegistryKey hklm = Registry.LocalMachine;
      RegistryKey subkey = hklm.OpenSubKey(registryKeyName, false);
      if (subkey != null)
      {
        string serviceName = (string)subkey.GetValue("Service");
        Log.Write("        serviceName:{0}", serviceName);
        registryKeyName = @"SYSTEM\CurrentControlSet\Services\" + serviceName + @"\Enum";
        Log.Write("        key:{0}", registryKeyName);
        subkey = hklm.OpenSubKey(registryKeyName, false);
        if (subkey != null)
        {
          Int32 count = (Int32)subkey.GetValue("Count");
          Log.Write("        filters available:{0}", count);
          for (int i = 0; i < count; ++i)
          {
            string moniker = (string)subkey.GetValue(i.ToString());
            moniker = moniker.Replace(@"\", "#");
            moniker = moniker.Replace(@"/", "#");
            Log.Write("          filter#:{0}={1}", i, moniker);
          }
          string monikerToUse = (string)subkey.GetValue(instance.ToString());
          monikerToUse = monikerToUse.Replace(@"\", "#");
          monikerToUse = monikerToUse.Replace(@"/", "#");
          Log.Write("        using filter #:{0}={1}", instance, monikerToUse);
          subkey.Close();
          hklm.Close();
          return monikerToUse;
        }
        else
        {
          hklm.Close();
          return String.Empty;
        }
      }
      hklm.Close();
      return String.Empty;
    }

    /// <summary>
    /// Retrieves the hardware location information associated with a moniker.
    /// Can be used to help decided which filters go together.
    /// </summary>
    /// <returns>
    /// A string of format "PCI bus x, device y" where x and y are numbers
    /// </returns>
      public string GetHardwareLocation(string monikerName)
      {
          //Log.WriteFile(Log.LogType.Capture, "    GetHardwareLocation: filter:{1}", monikerName);

          int pos1 = monikerName.IndexOf("#");
          int pos2 = monikerName.LastIndexOf("#");
          string left = monikerName.Substring(0, pos1);
          string mid = monikerName.Substring(pos1 + 1, (pos2 - pos1) - 1);
          mid = mid.Replace("#", "\\");
          string right = monikerName.Substring(pos2 + 1);
          string registryKeyName = mid;

          registryKeyName = @"SYSTEM\CurrentControlSet\Enum\PCI\" + mid;
          Log.Write("        key:{0}", registryKeyName);

          RegistryKey hklm = Registry.LocalMachine;
          RegistryKey subkey = hklm.OpenSubKey(registryKeyName, false);
          if (subkey != null)
          {
              string locInfo = (string)subkey.GetValue("LocationInformation");
              if (locInfo == null) locInfo = string.Empty;
              //Log.Write("        LocationInformation:{0}", locInfo);
              int fPos = locInfo.LastIndexOf(",");
              locInfo = locInfo.Substring(0, fPos);
              return locInfo;
          }
          hklm.Close();
          return String.Empty;
      }


    /// <summary>
    /// #MW#
    /// </summary>
    /// <returns></returns>
    public bool LoadDefinitions( string videoDevice,string videoDeviceMoniker)
    {
      if (_definitionLoaded) return true;
      _definitionLoaded = true;
      _captureCardDefinition = null;
      try
      {
         Log.WriteFile(Log.LogType.Capture, "LoadDefs for device at {0}",GetHardwareLocation(videoDeviceMoniker));
        //Log.WriteFile(Log.LogType.Capture, "LoadDefinitions() card:{0} {1}", ID, this.FriendlyName);
        CaptureCardDefinitions captureCardDefinitions = CaptureCardDefinitions.Instance;
        if (CaptureCardDefinitions.CaptureCards.Count == 0)
        {
          // Load failed!!!
          Log.WriteFile(Log.LogType.Capture, " No capturecards defined, or load failed");
          return (false);
        }

        if (videoDeviceMoniker == null)
        {
          Log.WriteFile(Log.LogType.Capture, " No video device moniker specified");
          return true;
        }

        // Determine the deviceid "hidden" in the moniker of the capture device and use that to load
        // the definitions of the card... The id is between the first and second "#" character
        // example:
        //                     <------------------ ID ---------------->
        // @device:pnp:\\?\pci#ven_4444&dev_0016&subsys_40090070&rev_01#4&2e98101c&0&68f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\hauppauge wintv pvr pci ii capture
        string deviceId = videoDeviceMoniker.ToLower();
        string[] tmp1 = videoDeviceMoniker.Split((char[])"#".ToCharArray());
        if (tmp1.Length >= 2)
          deviceId = tmp1[1].ToLower();

        CaptureCardDefinition ccd = null;
        foreach (CaptureCardDefinition cd in CaptureCardDefinitions.CaptureCards)
        {
          if (cd.DeviceId.ToLower().IndexOf(deviceId) == 0 && cd.CaptureName.ToLower() == videoDevice.ToLower() && cd.CommercialName.ToLower() == CommercialName.ToLower())
          {
            ccd = cd;
            break;
          }
        }
        //
        // If card is unsupported, simply return
        if (_captureCardDefinition == null)
          _captureCardDefinition = new CaptureCardDefinition();
        if (ccd == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, " CaptureCard {0} NOT supported, no definitions found", videoDevice);
          return (false);
        }
        _captureCardDefinition.CaptureName = ccd.CaptureName;
        _captureCardDefinition.CommercialName = ccd.CommercialName;
        _captureCardDefinition.DeviceId = ccd.DeviceId.ToLower();

        _captureCardDefinition.Capabilities = ccd.Capabilities;
        _captureCardDefinition.Capabilities = ccd.Capabilities;

        _captureCardDefinition.Tv = new DeviceDefinition();
        _captureCardDefinition.Tv.FilterDefinitions = new ArrayList();
        foreach (FilterDefinition fd in ccd.Tv.FilterDefinitions)
        {
            fd.DSFilter = null;
            fd.MonikerDisplayName = String.Empty;
            FilterDefinition fd_copy = new FilterDefinition();
            fd_copy.Category = fd.Category;
            fd_copy.CheckDevice = fd.CheckDevice;
            fd_copy.DSFilter = fd.DSFilter;
            fd_copy.FriendlyName = fd.FriendlyName;
            fd_copy.MonikerDisplayName = fd.MonikerDisplayName;
            _captureCardDefinition.Tv.FilterDefinitions.Add(fd_copy);
        }
        _captureCardDefinition.Tv.ConnectionDefinitions = ccd.Tv.ConnectionDefinitions;
        _captureCardDefinition.Tv.InterfaceDefinition = ccd.Tv.InterfaceDefinition;
        int Instance = -1;

        AvailableFilters af = AvailableFilters.Instance;

        // Determine what PnP device the capture device is. This is done very, very simple by extracting
        // the first part of the moniker display name, which contains device specific information
        // <-------------GET THIS PART-------------------------------------------------->        
        // @device:pnp:\\?\pci#ven_4444&dev_0016&subsys_40090070&rev_01#4&2e98101c&0&68f0#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\hauppauge wintv pvr pci ii capture
        string captureDeviceDeviceName = videoDeviceMoniker;
        int pos = captureDeviceDeviceName.LastIndexOf("#");
        if (pos >= 0) captureDeviceDeviceName = captureDeviceDeviceName.Substring(0, pos);
        Log.WriteFile(Log.LogType.Capture, " video device moniker   :{0}", videoDeviceMoniker);
        Log.WriteFile(Log.LogType.Capture, " captureDeviceDeviceName:{0}", captureDeviceDeviceName);



        Instance = FindInstanceForDevice(captureDeviceDeviceName);
        Log.WriteFile(Log.LogType.Capture, " Using card#{0}", Instance);
        //for each tv filter we need for building the graph
        foreach (FilterDefinition fd in _captureCardDefinition.Tv.FilterDefinitions)
        {
          //Hauppauge WinTV PVR PCI II TvTuner
          //pvr 150:  ven_4444&amp;dev_0016&amp;subsys_88010070&amp;rev_01
          //pvr 350:  ven_4444&amp;dev_0803&amp;subsys_40000070&amp;rev_01
          //pvr 250:  ven_4444&amp;dev_0803&amp;subsys_40010070&amp;rev_01

          string friendlyName = fd.Category;
          bool filterFound = false;
          Log.WriteFile(Log.LogType.Capture, "  filter {0}='{1}' check:{2}", friendlyName, fd.FriendlyName, fd.CheckDevice);

          //for each directshow filter
          foreach (string key in AvailableFilters.Filters.Keys)
          {
            // check if this filter has the correct friendly name
            Filter tmpFilter;
            ArrayList al = AvailableFilters.Filters[key] as System.Collections.ArrayList;
            tmpFilter = (Filter)al[0];
            if (String.Compare(tmpFilter.Name, fd.FriendlyName, true) != 0) continue;
            if (fd.CheckDevice)
            {
              //yes, then check all instances of this filter
              foreach (Filter directShowFilter in al)
              {
                if (String.Compare(directShowFilter.Name, fd.FriendlyName, true) != 0) continue;

                //next check if the moniker is the same as the capturedevice moniker 
                //we got a direct show filter with the correct name.
                string filterMoniker = directShowFilter.MonikerString;
                int posTmp = filterMoniker.LastIndexOf("#");
                if (posTmp >= 0) filterMoniker = filterMoniker.Substring(0, posTmp);
                if (captureDeviceDeviceName.ToLower().IndexOf(filterMoniker.ToLower()) >= 0)
                {
                  //yes, filter found
                  filterFound = true;
                  fd.MonikerDisplayName = directShowFilter.MonikerString;
                  break;
                }
              }//foreach (Filter directShowFilter in al)
            }
            else
            {
              //yes, filter found
              filterFound = true;
              fd.MonikerDisplayName = tmpFilter.MonikerString;
            }
            if (filterFound) break;
          }//foreach (string key in AvailableFilters.Filters.Keys)

          if (filterFound) continue;

          //filter not found
          //could be that filter has completly different moniker then the
          //moniker of the capture device filter
          //for each directshow filter available under windows
          foreach (string key in AvailableFilters.Filters.Keys)
          {
            Filter filter;
            ArrayList al = AvailableFilters.Filters[key] as System.Collections.ArrayList;
            filter = (Filter)al[0];

            // if directshow filter name == video filter name
            if (filter.Name.ToLower()==fd.FriendlyName.ToLower())
            {
              // FriendlyName found. Now check if this name should be checked against a (PnP) device
              // to make sure that we found the right filter...z
              if (fd.CheckDevice)
              {
                filter = al[0] as Filter;
                string filterMoniker = filter.MonikerString;
                int posTmp = filterMoniker.LastIndexOf("#");
                if (posTmp >= 0) filterMoniker = filterMoniker.Substring(0, posTmp);

                Log.WriteFile(Log.LogType.Capture, "  CheckDevice:{0}", filterMoniker);
                if (!filterFound)
                {
                  string moniker = FindUniqueFilter(filterMoniker, Instance);
                  for (int filterInst = 0; filterInst < al.Count; ++filterInst)
                  {
                    filter = al[filterInst] as Filter;
                    string tmpMoniker = filter.MonikerString.Replace(@"\", "#");
                    tmpMoniker = tmpMoniker.Replace(@"/", "#");
                    if (tmpMoniker.ToLower().IndexOf(moniker.ToLower()) >= 0)
                    {
                      Log.Write("use unique filter moniker:{0}", filter.MonikerString);
                      filterFound = true;
                      break;
                    }
                  }
                }

                // Match the filter based on the hardware location, that is find the filter
                // associated with the same piece of hardware.
                if (!filterFound)
                {
                    foreach (Filter f in al) {
                        string locf = GetHardwareLocation(f.MonikerString);
                        string locv = GetHardwareLocation(videoDeviceMoniker);
                        if (locf.Equals(locv) && !locf.Equals(string.Empty) && !locv.Equals(string.Empty) ) {
                            filter = f;
                            filterFound = true;
                            Log.WriteFile(Log.LogType.Capture, "Filter matched on hardware location: {1} -> {0}", f.MonikerString, GetHardwareLocation(f.MonikerString));
                            break;
                        }
                    }
                    if (!filterFound) Log.WriteFile(Log.LogType.Capture, "No filters matched on hardware location");
                }

                if (!filterFound)
                {
                  if (al.Count > 0)
                  {
                    Log.Write("use global filter moniker (if you have two identical tuner cards and see this, you might experience problems using both)");
                    filter = al[0] as Filter;
                    filterFound = true;
                  }
                }
                if (!filterFound)
                {
                  Log.WriteFile(Log.LogType.Capture, true, "  ERROR Cannot find unique filter for filter:{0}", filter.Name);
                }
                else
                {
                  Log.WriteFile(Log.LogType.Capture, "    Found {0}={1}", filter.Name, filter.MonikerString);
                }
              }
              else filterFound = true;

              // For found filter, get the unique name, the moniker display name which contains not only
              // things like the type of device, but also a reference (in case of PnP hardware devices)
              // to the actual device number which makes it possible to distinqiush two identical cards!
              if (filterFound)
              {
                fd.MonikerDisplayName = filter.MonikerString;
                break;
              }
            }//if (filter.Name.Equals(fd.FriendlyName))
          }//foreach (string key in AvailableFilters.Filters.Keys)
          // If no filter found thats in the definitions file, we obviously made a mistake defining it
          // Log the error and return false...
          if (!filterFound)
          {
            if (fd.FriendlyName.StartsWith("%") == false || fd.FriendlyName.EndsWith("%") == false )
            {
              Log.WriteFile(Log.LogType.Capture, true, "  Filter {0} not found in definitions file", friendlyName);
              return (false);
            }
          }
        }//foreach (string friendlyName in _captureCardDefinition.Tv.FilterDefinitions.Keys)
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return (false);
      }
      return (true);
    }
    #endregion
  }
}
