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
using System.Xml;
using DirectShowLib;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB.Structures;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// A class for handling conditional access with softCAM
  /// plugins that support Agarwal's multi-decrypt plugin API.
  /// </summary>
  public class MdPlugin : IDisposable
  {
    #region COM interface imports

    [ComImport, Guid("72E6DB8F-9F33-4D1C-A37C-DE8148C0BE74")]
    private class MDAPIFilter {};

    [ComVisible(true), ComImport,
     Guid("C3F5AA0D-C475-401B-8FC9-E33FB749CD85"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel
    {
      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different channel using specific parameters.
      /// </summary>
      [PreserveSig]
      int ChangeChannel(int frequency, int bandwidth, int polarity, int videoPid, int audioPid, int ecmPid, int caId, int providerId);

      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different channel using a Program82 structure.
      /// </summary>
      int ChangeChannelTP82(IntPtr program82);

      ///<summary>
      /// Set the plugin directory.
      ///</summary>
      int SetPluginsDirectory([MarshalAs(UnmanagedType.LPWStr)] String directory);
    }

    /// <summary>
    /// IChangeChannel_Ex interface
    /// </summary>
    [ComVisible(true), ComImport,
     Guid("E98B70EE-F5A1-4f46-B8B8-A1324BA92F5F"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel_Ex
    {
      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different channel using Program82 and PidsToDecode structures.
      /// </summary>
      [PreserveSig]
      int ChangeChannelTP82_Ex(IntPtr program82, IntPtr pidsToDecode);
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    public struct CaSystem82
    {
      public UInt16 CaType;
      public UInt16 EcmPid;
      public UInt16 EmmPid;
      private UInt16 Padding;
      public UInt32 ProviderId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct PidFilter
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
      public String Name;
      public byte Id;
      public UInt16 Pid;
    }

    // Note: many of the struct members aren't documented or used.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct Program82
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
      public String Name;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
      public String Provider;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
      public String Country;
      private UInt16 Padding1;

      public UInt32 Frequency;  // unit = kHz
      public byte PType;
      public byte Voltage;      // polarisation ???
      public byte Afc;
      public byte Diseqc;
      public UInt16 SymbolRate; // unit = ksps
      public UInt16 Qam;        // modulation ???
      public UInt16 FecRate;
      public byte Norm;
      private byte Padding2;

      public UInt16 TransportStreamId;
      public UInt16 VideoPid;
      public UInt16 AudioPid;
      public UInt16 TeletextPid;
      public UInt16 PmtPid;
      public UInt16 PcrPid;
      public UInt16 EcmPid;
      public UInt16 ServiceId;
      public UInt16 Ac3AudioPid;

      public byte AnalogTvStandard; // 0x00 = PAL, 0x11 = NTSC

      public byte ServiceType;
      public byte CaId;
      private byte Padding3;

      public UInt16 TempAudioPid;

      public UInt16 FilterCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      public PidFilter[] Filters;

      public UInt16 CaSystemCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCaSystemCount)]
      public CaSystem82[] CaSystems;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
      public String CaCountry;

      public byte Marker;

      public UInt16 LinkTransponder;
      public UInt16 LinkServiceid;

      public byte Dynamic;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] ExternBuffer;
      private byte Padding4;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PidsToDecode  // TPids2Dec
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPidCount)]
      public UInt16[] Pids;
      public UInt16 PidCount;
    }

    private class DecodeSlot
    {
      public IBaseFilter Filter;
      public IChannel CurrentChannel;
    }

    #endregion

    #region constants

    private const int Program82Size = 804;
    private const int MaxCaSystemCount = 32;
    private const int PidsToDecodeSize = 128;
    private const int MaxPidCount = 63;

    #endregion

    #region variables

    private bool _isMdPlugin = false;
    private String _configurationFolderPrefix = String.Empty;
    private IFilterGraph2 _graph = null;
    private IBaseFilter _infTee = null;
    private List<DecodeSlot> _slots = null;
    private IntPtr _programmeBuffer = IntPtr.Zero;
    private IntPtr _pidBuffer = IntPtr.Zero;

    #endregion

    /// <summary>
    /// Initialises a new instance of the <see cref="MdPlug"/> class.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter.</param>
    /// <param name="tunerDevicePath">The tuner device path.</param>
    public MdPlugin(IBaseFilter tunerFilter, String tunerDevicePath)
    {
      if (tunerFilter == null || tunerDevicePath == null)
      {
        return;
      }

      // If there is no MD configuration folder then there is no softCAM plugin.
      if (Directory.Exists("MDPLUGINS") == false)
      {
        return;
      }

      try
      {
        // Look for a configuration file.
        String configFile = AppDomain.CurrentDomain.BaseDirectory + "MDPLUGINS\\MDAPICards.xml";
        int slotCount = 1;
        _configurationFolderPrefix = FilterGraphTools.GetFilterName(tunerFilter);
        XmlDocument doc = new XmlDocument();
        XmlNode rootNode = null;
        XmlNode tunerNode = null;
        if (File.Exists(configFile))
        {
          Log.Log.Debug("MD Plugin: searching for tuner configuration");
          doc.Load(configFile);
          rootNode = doc.SelectSingleNode("/cards");
          if (rootNode != null)
          {
            XmlNodeList tunerList = doc.SelectNodes("/cards/card");
            if (tunerList != null)
            {
              foreach (XmlNode node in tunerList)
              {
                // We found the configuration for the tuner.
                if (tunerNode.Attributes["DevicePath"].Value.Equals(tunerDevicePath))
                {
                  tunerNode = node;
                  break;
                }
              }
            }
          }
        }
        if (rootNode == null)
        {
          rootNode = doc.CreateElement("cards");
        }
        if (tunerNode == null)
        {
          Log.Log.Debug("MD Plugin: creating tuner configuration");
          tunerNode = doc.CreateElement("card");

          // The "name" attribute is used as the configuration folder prefix.
          XmlAttribute attr = doc.CreateAttribute("Name");
          attr.InnerText = _configurationFolderPrefix;
          tunerNode.Attributes.Append(attr);

          // Used to identify the tuner.
          attr = doc.CreateAttribute("DevicePath");
          attr.InnerText = tunerDevicePath;
          tunerNode.Attributes.Append(attr);

          // Default: enable one instance of the plugin.
          attr = doc.CreateAttribute("EnableMdapi");
          attr.InnerText = slotCount.ToString();
          tunerNode.Attributes.Append(attr);

          rootNode.AppendChild(tunerNode);
          doc.AppendChild(rootNode);
          doc.Save(configFile);
        }
        else
        {
          try
          {
            _configurationFolderPrefix = tunerNode.Attributes["Name"].Value;
            slotCount = Convert.ToInt32(tunerNode.Attributes["EnableMdapi"].Value);
          }
          catch (Exception ex)
          {
            // Assume that the plugin is enabled unless the parameter says "no".
            if (tunerNode.Attributes["EnableMdapi"].Value.Equals("no"))
            {
              slotCount = 0;
            }
            tunerNode.Attributes["EnableMdapi"].Value = slotCount.ToString();
            doc.Save(configFile);
          }
        }

        if (slotCount > 0)
        {
          Log.Log.Debug("MD Plugin: plugin is enabled for {0} decoding slot(s)", slotCount);
          _isMdPlugin = true;
          _slots = new List<DecodeSlot>(slotCount);
          _programmeBuffer = Marshal.AllocCoTaskMem(Program82Size);
          _pidBuffer = Marshal.AllocCoTaskMem(PidsToDecodeSize);
        }
        else
        {
          Log.Log.Debug("MD Plugin: plugin is not enabled");
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("MD Plugin: failed to create, load or read configuration\r\n{0}", ex.ToString());
      }
    }

    /// <summary>
    /// Insert and connect the add-on device into the graph.
    /// </summary>
    /// <param name="graphBuilder">The graph builder to use to insert the device.</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture filter) to connect the device to.</param>
    /// <returns><c>true</c> if the device was successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(ref ICaptureGraphBuilder2 graphBuilder, ref IBaseFilter lastFilter)
    {
      Log.Log.Debug("MD Plugin: add filter to graph");
      if (graphBuilder == null)
      {
        Log.Log.Debug("MD Plugin: graph builder is null");
        return false;
      }
      if (lastFilter == null)
      {
        Log.Log.Debug("MD Plugin: upstream filter is null");
        return false;
      }

      // We need a reference to the graph builder's graph.
      IGraphBuilder tmpGraph = null;
      int hr = graphBuilder.GetFiltergraph(out tmpGraph);
      if (hr != 0)
      {
        Log.Log.Debug("MD Plugin: failed to get graph reference, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      _graph = tmpGraph as IFilterGraph2;
      if (_graph == null)
      {
        Log.Log.Debug("MD Plugin: failed to get graph reference");
        return false;
      }

      // Add an inf tee after the tuner/capture filter.
      _infTee = (IBaseFilter)new InfTee();
      hr = _graph.AddFilter(_infTee, "Inf Tee (MD Plugin)");
      if (hr != 0)
      {
        Log.Log.Debug("MD Plugin: failed to add inf tee to graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      hr = graphBuilder.RenderStream(null, null, lastFilter, null, _infTee);
      if (hr != 0)
      {
        Log.Log.Debug("MD Plugin: failed to render stream through inf tee, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        _graph.RemoveFilter(_infTee);
        Release.ComObject(_infTee);
        _infTee = null;
        return false;
      }
      lastFilter = _infTee;

      // Add one filter for each decoding slot.
      IPin outputPin;
      IPin inputPin;
      for (int i = 0; i < _slots.Count; i++)
      {
        DecodeSlot slot = new DecodeSlot();
        slot.Filter = (IBaseFilter)new MDAPIFilter();
        slot.CurrentChannel = null;

        // Add the filter to the graph.
        hr = _graph.AddFilter(slot.Filter, "MDAPI Filter " + i);
        if (hr != 0)
        {
          Log.Log.Debug("MD Plugin: failed to add MD plugin filter {0} to graph, hr = 0x{1:x} ({2})", i + 1, hr, HResult.GetDXErrorString(hr));
          return false;
        }

        // Connect the filter into the graph.
        outputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
        inputPin = DsFindPin.ByDirection(slot.Filter, PinDirection.Input, 0);
        hr = _graph.Connect(outputPin, inputPin);
        Release.ComObject(outputPin);
        Release.ComObject(inputPin);
        if (hr != 0)
        {
          Log.Log.Debug("MD Plugin: failed to connect MD plugin filter {0} into the graph, hr = 0x{1:x} ({2})", i + 1, hr, HResult.GetDXErrorString(hr));
          return false;
        }
        lastFilter = slot.Filter;
        _slots[i] = slot;

        // Check whether the plugin supports extended capabilities.
        IChangeChannel temp = slot.Filter as IChangeChannel;
        try
        {
          temp.SetPluginsDirectory(_configurationFolderPrefix + i);
        }
        catch (Exception ex)
        {
          Log.Log.Debug("MD Plugin: failed to set plugin directory\r\n{0}", ex.ToString());
          return false;
        }
        IChangeChannel_Ex temp2 = slot.Filter as IChangeChannel_Ex;
        if (temp2 != null)
        {
          Log.Log.Debug("MD Plugin: extended capabilities supported");
        }
      }

      return true;
    }

    /// <summary>
    /// Send a command to to the conditional access interface. This function can be used to
    /// [for example] request that a service be decrypted, determine whether a service can be
    /// decrypted, or request that a service not be decrypted.
    /// </summary>
    /// <param name="service">The service information.</param>
    /// <param name="listAction">It is assumed that the interface may decrypt one or more services
    ///   simultaneously. This parameter specifies how this command should be applied to the service
    ///   in the context of the set of services that the interface is (or will be) managing.</param>
    /// <param name="commandType">The type of command.</param>
    /// <param name="pmt">The programme map table entry for the service.</param>
    /// <param name="cat">The conditional access table entry for the service.</param>
    /// <returns><c>true</c> if the service is successfully decrypted, otherwise <c>false</c></returns>
    public bool SendCommand(IChannel service, ListManagementType listAction, CommandIdType commandType, byte[] pmt, byte[] cat)
    {
      Log.Log.Debug("MD Plugin: send command, list action = {0}, command = {1}", listAction, commandType);
      if (!_isMdPlugin || _slots.Count == 0)
      {
        Log.Log.Debug("MD Plugin: no decrypt slots");
        return true;    // Don't retry.
      }
      if (commandType == CommandIdType.MMI || commandType == CommandIdType.Query)
      {
        Log.Log.Debug("MD Plugin: command type {0} is not supported", commandType);
        return false;
      }
      if (pmt == null || pmt.Length == 0)
      {
        Log.Log.Debug("MD Plugin: no PMT");
        return true;
      }
      if (cat == null || cat.Length == 0)
      {
        Log.Log.Debug("MD Plugin: no CAT");
        return true;
      }
      DVBBaseChannel dvbService = service as DVBBaseChannel;
      if (dvbService == null)
      {
        Log.Log.Debug("MD Plugin: service is not a DVB service");
        return true;
      }

      // Find the slot which is decoding the service (if there is one).
      DecodeSlot freeSlot = null;
      foreach (DecodeSlot slot in _slots)
      {
        DVBBaseChannel currentService = slot.CurrentChannel as DVBBaseChannel;
        if (currentService != null && currentService.ServiceId == dvbService.ServiceId)
        {
          // "Not selected" means stop decrypting the service.
          if (commandType == CommandIdType.NotSelected)
          {
            slot.CurrentChannel = null;
            return true;
          }
          // "Descrambling" means start decrypting the service,
          // but since we're already decrypting the service there
          // is nothing to do.
          else if (commandType == CommandIdType.Descrambling)
          {
            return true;
          }
        }
        else if (currentService == null && freeSlot == null)
        {
          freeSlot = slot;
        }
      }

      if (commandType == CommandIdType.NotSelected)
      {
        // If we get to here then we were asked to stop decrypting
        // a service that we were not decrypting. Strange...
        Log.Log.Debug("MD Plugin: received \"not selected\" request for channel that is not being decrypted");
        return true;
      }

      if (freeSlot == null)
      {
        // If we get to here then we were asked to start decrypting
        // a service, but we don't have any free slots to do it with.
        Log.Log.Debug("MD Plugin: no free decrypt slots");
        return false;
      }

      // If we get to here then we need to try to start decrypting
      // the service.
      Program82 programToDecode = new Program82();
      PidsToDecode pidsToDecode = new PidsToDecode();

      // Set the fields that we are able to set.
      if (dvbService.Name != null)
      {
        programToDecode.Name = dvbService.Name;
      }
      if (dvbService.Provider != null)
      {
        programToDecode.Provider = dvbService.Provider;
      }

      Log.Log.Debug("MD Plugin: registering PIDs");
      programToDecode.TransportStreamId = (UInt16)dvbService.TransportId;
      programToDecode.ServiceId = (UInt16)dvbService.ServiceId;
      programToDecode.PmtPid = (UInt16)dvbService.PmtPid;
      ChannelInfo channelInfo = new ChannelInfo();
      channelInfo.DecodePmt(pmt);
      channelInfo.DecodeCat(cat);
      programToDecode.PcrPid = channelInfo.pcrPid;
      foreach (PidInfo pid in channelInfo.pids)
      {
        // When using a plugin with extended support, we can just
        // specify the PIDs that need to be decoded.
        if (pidsToDecode.PidCount < MaxPidCount)
        {
          pidsToDecode.Pids[pidsToDecode.PidCount++] = pid.pid;
        }
        else
        {
          Log.Log.Debug("MD Plugin: unable to register all PIDs");
          break;
        }

        // Otherwise, we have to fill the specific PID fields in the
        // Program82 structure as best as we can. We'll keep the first
        // of each type of PID.
        if (pid.isVideo && programToDecode.VideoPid == 0)
        {
          programToDecode.VideoPid = pid.pid;
        }
        else if (pid.isAudio)
        {
          if (pid.isAC3Audio)
          {
            if (programToDecode.Ac3AudioPid == 0)
            {
              programToDecode.Ac3AudioPid = pid.pid;
            }
          }
          else if (!pid.isEAC3Audio && programToDecode.AudioPid == 0)
          {
            programToDecode.AudioPid = pid.pid;
          }
        }
        else if (pid.isTeletext && programToDecode.TeletextPid == 0)
        {
          programToDecode.TeletextPid = pid.pid;
        }
      }

      // We don't know what the actual service type is in this
      // context, but we can at least indicate whether this is
      // a TV or radio service.
      programToDecode.ServiceType = (byte)(dvbService.IsTv ? DvbServiceType.DigitalTelevision : DvbServiceType.DigitalRadio);

      Log.Log.Debug("MD Plugin: TSID = {0} (0x{1:x}), SID = {2} (0x{3:x}), PMT PID = {4} (0x{5:x}), PCR PID = {6} (0x{7:x}), service type = {8}, " +
                        "video PID = {9} (0x{10:x}), audio PID = {11} (0x{12:x}), AC3 PID = {13} (0x{14:x}), teletext PID = {15} (0x{16:x})",
          programToDecode.TransportStreamId, programToDecode.TransportStreamId,
          programToDecode.ServiceId, programToDecode.ServiceId,
          programToDecode.PmtPid, programToDecode.PmtPid,
          programToDecode.PcrPid, programToDecode.PcrPid,
          programToDecode.ServiceType,
          programToDecode.VideoPid, programToDecode.VideoPid,
          programToDecode.AudioPid, programToDecode.AudioPid,
          programToDecode.Ac3AudioPid, programToDecode.Ac3AudioPid,
          programToDecode.TeletextPid, programToDecode.TeletextPid);

      // Build a list of the ECMs and EMMs in the Program82
      // CA systems member.
      UInt16 count = 0;   // This variable will be used to count the number of distinct ECM/EMM PID combinations.
      programToDecode.CaSystems = new CaSystem82[MaxCaSystemCount];

      Log.Log.Debug("MD Plugin: registering ECM and EMM details");
      List<ECMEMM> ecmList = channelInfo.caPMT.GetECM();
      for (int i = 0; i < ecmList.Count && count < 32; i++)
      {
        Log.Log.Debug("MD Plugin: ECM #{0} CA ID = 0x{1:x}, PID = 0x{2:x}, provider = 0x{3:x}", i + 1, ecmList[i].CaId, ecmList[i].Pid, ecmList[i].ProviderId);

        programToDecode.CaSystems[i].CaType = (UInt16)ecmList[i].CaId;
        programToDecode.CaSystems[i].EcmPid = ecmList[i].Pid;
        programToDecode.CaSystems[i].ProviderId = (uint)ecmList[i].ProviderId;
        count++;
        if (i == 0)
        {
          programToDecode.EcmPid = ecmList[i].Pid;   // Default...
        }
      }

      List<ECMEMM> emmList = channelInfo.caPMT.GetEMM();
      for (int i = 0; i < emmList.Count; ++i)
      {
        Log.Log.Debug("MD Plugin: EMM #{0} CA ID = 0x{1:x}, PID = 0x{2:x}, provider = 0x{3:x}", i + 1, emmList[i].CaId, emmList[i].Pid, emmList[i].ProviderId);

        // Check if this EMM PID is linked to an ECM PID.
        bool found = false;
        for (int j = 0; j < ecmList.Count; j++)
        {
          if (emmList[i].CaId == ecmList[j].CaId && emmList[i].ProviderId == ecmList[j].ProviderId)
          {
            found = true;
            programToDecode.CaSystems[j].EmmPid = emmList[i].Pid;
            break;
          }
        }
        if (!found && count < 32)
        {
          programToDecode.CaSystems[count].CaType = (UInt16)emmList[i].CaId;
          programToDecode.CaSystems[count].EmmPid = emmList[i].Pid;
          programToDecode.CaSystems[count].ProviderId = (UInt32)emmList[i].ProviderId;
          count++;
        }
      }

      programToDecode.CaSystemCount = count;
      SetPreferredCaSystemIndex(ref programToDecode);

      Log.Log.Debug("MD Plugin: ECM PID = {0} (0x{1:x}, CA system count = {2}, CA index = {3}",
                    programToDecode.EcmPid,
                    programToDecode.EcmPid,
                    programToDecode.CaSystemCount,
                    programToDecode.CaId);
      for (byte i = 0; i < count; i++)
      {
        Log.Log.Debug("MD Plugin: #{0} CA type = {1} (0x{2:x}), ECM PID = {3} (0x{4:x}), EMM PID = {5} (0x{6:x}), provider = {7} (0x{8:x})",
                      i + 1,
                      programToDecode.CaSystems[i].CaType,
                      programToDecode.CaSystems[i].CaType,
                      programToDecode.CaSystems[i].EcmPid,
                      programToDecode.CaSystems[i].EcmPid,
                      programToDecode.CaSystems[i].EmmPid,
                      programToDecode.CaSystems[i].EmmPid,
                      programToDecode.CaSystems[i].ProviderId,
                      programToDecode.CaSystems[i].ProviderId);
      }

      // Instruct the MD filter to decrypt the service.
      Marshal.StructureToPtr(programToDecode, _programmeBuffer, true);
      Marshal.StructureToPtr(pidsToDecode, _pidBuffer, true);
      try
      {
        IChangeChannel_Ex changeEx = freeSlot.Filter as IChangeChannel_Ex;
        if (changeEx != null)
        {
          changeEx.ChangeChannelTP82_Ex(_programmeBuffer, _pidBuffer);
        }
        else
        {
          IChangeChannel change = freeSlot.Filter as IChangeChannel;
          if (change != null)
          {
            change.ChangeChannelTP82(_programmeBuffer);
          }
          else
          {
            throw new Exception("Failed to acquire interface on filter.");
          }
        }
        freeSlot.CurrentChannel = service;
      }
      catch (Exception ex)
      {
        Log.Log.Debug("MD Plugin: failed to change channel\r\n{0}", ex.ToString());
        return false;
      }

      return true;
    }

    private void SetPreferredCaSystemIndex(ref Program82 programToDecode)
    {
      try
      {
        Log.Log.Debug("MD Plugin: identifying primary ECM PID");

        // Load configuration (if we have any).
        String configFile = AppDomain.CurrentDomain.BaseDirectory + "MDPLUGINS\\MDAPIProvID.xml";
        XmlDocument doc = new XmlDocument();
        bool configFound = false;
        if (File.Exists(configFile))
        {
          doc.Load(configFile);

          // We're looking for the primary ECM PID. This can be configured [from
          // lowest to highest level] per-channel, per-provider, or per-CA type.
          // Search for channel-level configuration first.
          XmlNodeList channelList = doc.SelectNodes("/mdapi/channels/channel");
          if (channelList != null)
          {
            String transportStreamId = String.Format("{0:D}", programToDecode.TransportStreamId);
            String serviceId = String.Format("{0:D}", programToDecode.ServiceId);
            String pmtPid = String.Format("{0:D}", programToDecode.PmtPid);
            foreach (XmlNode channelNode in channelList)
            {
              if (channelNode.Attributes["tp_id"].Value.Equals(transportStreamId) &&
                  channelNode.Attributes["sid"].Value.Equals(serviceId) &&
                  channelNode.Attributes["pmt_pid"].Value.Equals(pmtPid))
              {
                Log.Log.Debug("MD Plugin: found channel configuration");
                for (byte i = 0; i < programToDecode.CaSystemCount; i++)
                {
                  String ecmPid = String.Format("{0:D}", programToDecode.CaSystems[i].EcmPid);
                  if (channelNode.Attributes["ecm_pid"].Value.Equals(ecmPid))
                  {
                    Log.Log.Debug("MD Plugin: found correct ECM PID");
                    programToDecode.CaId = i;
                    programToDecode.EcmPid = programToDecode.CaSystems[i].EcmPid;
                    configFound = true;

                    if (((XmlElement)channelNode).HasAttribute("emm_pid"))
                    {
                      programToDecode.CaSystems[i].EmmPid = UInt16.Parse(((XmlElement)channelNode).GetAttribute("emm_pid"));
                    }

                    break;
                  }
                }
                if (configFound)
                {
                  break;
                }
              }
            }
          }

          // No channel-level configuration? Try provider-level configuration.
          if (!configFound)
          {
            XmlNodeList providerList = doc.SelectNodes("/mdapi/providers/provider");
            if (providerList != null)
            {
              foreach (XmlNode providerNode in providerList)
              {
                for (byte i = 0; i < programToDecode.CaSystemCount; i++)
                {
                  if (providerNode.Attributes["ID"].Value.Equals(String.Format("{0:D}", programToDecode.CaSystems[i].ProviderId)))
                  {
                    Log.Log.Debug("MD Plugin: found provider configuration");
                    programToDecode.CaId = i;
                    programToDecode.EcmPid = programToDecode.CaSystems[i].EcmPid;
                    configFound = true;
                    break;
                  }
                }
                if (configFound)
                {
                  break;
                }
              }
            }
          }

          // Still no configuration found? Our final check is for CA type configuration.
          if (!configFound)
          {
            XmlNodeList caTypeList = doc.SelectNodes("/mdapi/CA_Types/CA_Type");
            if (caTypeList != null)
            {
              foreach (XmlNode caTypeNode in caTypeList)
              {
                for (byte i = 0; i < programToDecode.CaSystemCount; i++)
                {
                  if (caTypeNode.Attributes["ID"].Value.Equals(String.Format("{0:D}", programToDecode.CaSystems[i].CaType)))
                  {
                    Log.Log.Debug("MD Plugin: found CA type configuration");
                    programToDecode.CaId = i;
                    programToDecode.EcmPid = programToDecode.CaSystems[i].EcmPid;
                    configFound = true;
                    break;
                  }
                }
                if (configFound)
                {
                  break;
                }
              }
            }
          }
        }

        if (!configFound)
        {
          Log.Log.Debug("MD Plugin: no configuration found");

          // Now the question: do we add configuration?
          XmlElement mainNode = (XmlElement)doc.SelectSingleNode("/mdapi");
          bool fillOutConfig = false;
          if (mainNode == null || !mainNode.HasAttribute("fillout"))
          {
            if (mainNode == null)
            {
              mainNode = doc.CreateElement("mdapi");
            }
            XmlAttribute fillOutAttribute = doc.CreateAttribute("fillout");
            fillOutAttribute.Value = fillOutConfig.ToString();
            mainNode.Attributes.Append(fillOutAttribute);
            doc.Save(configFile);
          }
          else
          {
            Boolean.TryParse(mainNode.Attributes["fillout"].Value, out fillOutConfig);
          }

          if (fillOutConfig)
          {
            Log.Log.Info("MD Plugin: attempting to add entries to MDAPIProvID.xml");

            // Channel configuration stub.
            XmlNode node = doc.CreateElement("channel");
            XmlAttribute attribute = doc.CreateAttribute("tp_id");
            attribute.Value = programToDecode.TransportStreamId.ToString();
            node.Attributes.Append(attribute);
            attribute = doc.CreateAttribute("sid");
            attribute.Value = programToDecode.ServiceId.ToString();
            node.Attributes.Append(attribute);
            attribute = doc.CreateAttribute("pmt_pid");
            attribute.Value = programToDecode.PmtPid.ToString();
            node.Attributes.Append(attribute);
            attribute = doc.CreateAttribute("ecm_pid");
            attribute.Value = programToDecode.EcmPid.ToString();
            node.Attributes.Append(attribute);

            String comment = "Channel \"" + programToDecode.Name + "\", possible ECM PID values = {{";
            for (byte i = 0; i < programToDecode.CaSystemCount; i++)
            {
              if (programToDecode.CaSystems[i].EcmPid == 0)
              {
                continue;
              }
              if (i != 0)
              {
                comment += ", ";
              }
              comment += programToDecode.CaSystems[i].EcmPid;
            }
            node.AppendChild(doc.CreateComment(comment + "}}."));
            node.AppendChild(node);

            XmlNode listNode = doc.SelectSingleNode("/mdapi/channels");
            if (listNode == null)
            {
              listNode = doc.CreateElement("channels");
              listNode.AppendChild(node);
              mainNode.AppendChild(listNode);
            }
            else
            {
              listNode.AppendChild(node);
            }

            // Provider configuration stubs.
            listNode = doc.SelectSingleNode("/mdapi/providers");
            if (listNode == null)
            {
              listNode = doc.CreateElement("providers");
              mainNode.AppendChild(listNode);
            }
            // None of the provider IDs referenced in the CA system
            // array have configuration yet, however that set is not
            // guaranteed to be distinct.
            HashSet<uint> newProviders = new HashSet<uint>();
            for (byte i = 0; i < programToDecode.CaSystemCount; i++)
            {
              if (!newProviders.Contains(programToDecode.CaSystems[i].ProviderId))
              {
                node = doc.CreateElement("provider");
                attribute = doc.CreateAttribute("ID");
                attribute.Value = programToDecode.CaSystems[i].ProviderId.ToString();
                node.Attributes.Append(attribute);
                listNode.AppendChild(node);
                newProviders.Add(programToDecode.CaSystems[i].ProviderId);
              }
            }

            // CA type configuration stubs.
            listNode = doc.SelectSingleNode("/mdapi/CA_Types");
            if (listNode == null)
            {
              listNode = doc.CreateElement("CA_Types");
              mainNode.AppendChild(listNode);
            }
            // None of the CA types referenced in the CA system
            // array have configuration yet, however that set is not
            // guaranteed to be distinct.
            HashSet<uint> newCaTypes = new HashSet<uint>();
            for (byte i = 0; i < programToDecode.CaSystemCount; i++)
            {
              if (!newProviders.Contains(programToDecode.CaSystems[i].CaType))
              {
                node = doc.CreateElement("CA_Type");
                attribute = doc.CreateAttribute("ID");
                attribute.Value = programToDecode.CaSystems[i].CaType.ToString();
                node.Attributes.Append(attribute);
                listNode.AppendChild(node);
                newProviders.Add(programToDecode.CaSystems[i].CaType);
              }
            }
            
            doc.Save(configFile);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Debug("MD Plugin: failed to load or read configuration, or set preferred CA system index\r\n{0}", ex.ToString());
      }
    }

    #region IDisposable member

    /// <summary>
    /// Free unmanaged memory buffers and COM objects.
    /// </summary>
    public void Dispose()
    {
      if (!_isMdPlugin)
      {
        return;
      }

      if (_infTee != null)
      {
        _graph.RemoveFilter(_infTee);
        Release.ComObject(_infTee);
        _infTee = null;
      }

      foreach (DecodeSlot slot in _slots)
      {
        if (slot.Filter != null)
        {
          _graph.RemoveFilter(slot.Filter);
          slot.Filter = null;
        }
      }

      Marshal.FreeCoTaskMem(_programmeBuffer);
      _programmeBuffer = IntPtr.Zero;
      Marshal.FreeCoTaskMem(_pidBuffer);
      _pidBuffer = IntPtr.Zero;
    }

    #endregion
  }
}