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
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MdPlugin
{
  /// <summary>
  /// A class for handling conditional access with softCAM plugins that support Agarwal's multi-decrypt
  /// plugin API.
  /// </summary>
  public class MdPlugin : BaseCustomDevice, IDirectShowAddOnDevice, IConditionalAccessProvider
  {
    #region COM interface imports

    [ComImport, Guid("72e6dB8f-9f33-4d1c-a37c-de8148c0be74")]
    private class MDAPIFilter
    {
    }

    [Guid("c3f5aa0d-c475-401b-8fc9-e33fb749cd85"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel
    {
      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different program using specific parameters.
      /// </summary>
      [PreserveSig]
      int ChangeChannel(int frequency, int bandwidth, int polarity, int videoPid, int audioPid, int ecmPid, int caId, int providerId);

      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different program using a Program82 structure.
      /// </summary>
      [PreserveSig]
      int ChangeChannelTP82(IntPtr program82);

      ///<summary>
      /// Set the plugin directory.
      ///</summary>
      [PreserveSig]
      int SetPluginsDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);
    }

    [Guid("e98b70ee-f5a1-4f46-b8b8-a1324ba92f5f"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel_Ex
    {
      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different program using Program82 and PidsToDecode structures.
      /// </summary>
      [PreserveSig]
      int ChangeChannelTP82_Ex(IntPtr program82, IntPtr pidsToDecode);
    }

    [Guid("D0EACAB1-3211-414B-B58B-E1157AC4D93A"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel_Clear
    {
      /// <summary>
      /// Inform the softCAM plugin that it no longer needs to decrypt a program.
      /// </summary>
      [PreserveSig]
      int ClearChannel();
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CaSystem82
    {
      public ushort CaType;
      public ushort EcmPid;
      public ushort EmmPid;
      private ushort Padding;
      public uint ProviderId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct PidFilter
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
      public string Name;
      public byte Id;
      public ushort Pid;
    }

    // Note: many of the struct members aren't documented or used.
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct Program82
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
      public string Name;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
      public string Provider;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
      public string Country;
      private ushort Padding1;

      public uint Frequency;    // unit = kHz
      public byte PType;
      public byte Voltage;      // polarisation ???
      public byte Afc;
      public byte Diseqc;
      public ushort SymbolRate; // unit = ks/s
      public ushort Qam;        // modulation ???
      public ushort FecRate;
      public byte Norm;
      private byte Padding2;

      public ushort TransportStreamId;
      public ushort VideoPid;
      public ushort AudioPid;
      public ushort TeletextPid;
      public ushort PmtPid;
      public ushort PcrPid;
      public ushort EcmPid;
      public ushort ServiceId;
      public ushort Ac3AudioPid;

      public byte AnalogTvStandard; // 0x00 = PAL, 0x11 = NTSC

      public byte ServiceType;
      public byte CaId;
      private byte Padding3;

      public ushort TempAudioPid;

      public ushort FilterCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      public PidFilter[] Filters;

      public ushort CaSystemCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CA_SYSTEM_COUNT)]
      public CaSystem82[] CaSystems;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
      public string CaCountry;

      public byte Marker;

      public ushort LinkTransponder;
      public ushort LinkServiceid;

      public byte Dynamic;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] ExternBuffer;
      private byte Padding4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PidsToDecode  // TPids2Dec
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PID_COUNT)]
      public ushort[] Pids;
      public ushort PidCount;
    }

    private class DecodeSlot
    {
      public IBaseFilter Filter;
      public IChannel CurrentChannel;
    }

    #endregion

    #region constants

    private static readonly int PROGRAM82_SIZE = Marshal.SizeOf(typeof(Program82));           // 804
    private const int MAX_CA_SYSTEM_COUNT = 32;
    private static readonly int PIDS_TO_DECODE_SIZE = Marshal.SizeOf(typeof(PidsToDecode));   // 128
    private const int MAX_PID_COUNT = 63;

    #endregion

    #region variables

    private bool _isMdPlugin = false;
    private bool _isCaInterfaceOpen = false;
    private HashSet<string> _providers = new HashSet<string>();
    private string _pluginFolder = PathManager.BuildAssemblyRelativePath("MDPLUGINS");
    private string _configurationFolderPrefix = string.Empty;
    private IFilterGraph2 _graph = null;
    private IBaseFilter _infTee = null;
    private List<DecodeSlot> _slots = null;
    private IntPtr _programBuffer = IntPtr.Zero;
    private IntPtr _pidBuffer = IntPtr.Zero;

    #endregion

    /// <summary>
    /// Find the video and audio elementary stream PIDs in a program map and record them in MD API structures.
    /// </summary>
    /// <param name="pmt">The program map to read from.</param>
    /// <param name="programToDecode">The MD API program information structure.</param>
    /// <param name="pidsToDecode">The MD API PID list structure.</param>
    private void RegisterVideoAndAudioPids(Pmt pmt, out Program82 programToDecode, out PidsToDecode pidsToDecode)
    {
      this.LogDebug("MD plugin: registering video and audio PIDs");
      programToDecode = new Program82();
      pidsToDecode = new PidsToDecode();
      pidsToDecode.Pids = new ushort[MAX_PID_COUNT];

      programToDecode.ServiceId = pmt.ProgramNumber;
      programToDecode.PcrPid = pmt.PcrPid;

      foreach (PmtElementaryStream es in pmt.ElementaryStreams)
      {
        // When using a plugin with extended support, we can just
        // specify the PIDs that need to be decoded.
        if (pidsToDecode.PidCount < MAX_PID_COUNT)
        {
          // TODO: restrict to video, audio, sub-title and teletext PIDs???
          pidsToDecode.Pids[pidsToDecode.PidCount++] = es.Pid;
        }
        else
        {
          this.LogError("MD plugin: unable to register all PIDs");
          break;
        }

        // Otherwise, we have to fill the specific PID fields in the
        // Program82 structure as best as we can. We'll keep the first
        // of each type of PID.
        if (programToDecode.VideoPid == 0 && StreamTypeHelper.IsVideoStream(es.LogicalStreamType))
        {
          programToDecode.VideoPid = es.Pid;
        }
        else if (programToDecode.TeletextPid == 0 && es.LogicalStreamType == LogicalStreamType.Teletext)
        {
          programToDecode.TeletextPid = es.Pid;
        }
        else
        {
          if (es.LogicalStreamType == LogicalStreamType.AudioAc3 || es.LogicalStreamType == LogicalStreamType.AudioEnhancedAc3)
          {
            if (programToDecode.Ac3AudioPid == 0)
            {
              programToDecode.Ac3AudioPid = es.Pid;
            }
          }
          else
          {
            if (programToDecode.AudioPid == 0 && StreamTypeHelper.IsAudioStream(es.LogicalStreamType))
            {
              programToDecode.AudioPid = es.Pid;
            }
          }
        }
      }
    }

    /// <summary>
    /// Find the ECM and EMM PIDs in program map and conditional access table CA descriptors and record them
    /// in MD API structures.
    /// </summary>
    /// <param name="pmt">The program map to read from.</param>
    /// <param name="cat">The conditional access table to read from.</param>
    /// <param name="programToDecode">The MD API program information structure.</param>
    /// <returns>the number of ECM/EMM PID combinations registered</returns>
    private ushort RegisterEcmAndEmmPids(Pmt pmt, Cat cat, ref Program82 programToDecode)
    {
      this.LogDebug("MD plugin: registering ECM and EMM details");

      // Build a dictionary of CA system -> { ECM PID } -> { provider ID } from the PMT.
      Dictionary<ushort, Dictionary<ushort, HashSet<uint>>> seenEcmPids = new Dictionary<ushort, Dictionary<ushort, HashSet<uint>>>();

      // First get ECMs from the PMT program CA descriptors.
      this.LogDebug("MD plugin: reading PMT program CA descriptors...");
      IEnumerator<IDescriptor> descEn = pmt.ProgramCaDescriptors.GetEnumerator();
      while (descEn.MoveNext())
      {
        ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
        if (cad == null)
        {
          this.LogDebug("MD plugin: invalid descriptor");
          ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
          Dump.DumpBinary(rawDescriptor);
          continue;
        }
        Dictionary<ushort, HashSet<uint>> caSystemEcmPids;
        if (!seenEcmPids.TryGetValue(cad.CaSystemId, out caSystemEcmPids))
        {
          seenEcmPids.Add(cad.CaSystemId, cad.Pids);
        }
        else
        {
          foreach (KeyValuePair<ushort, HashSet<uint>> pid in cad.Pids)
          {
            HashSet<uint> ecmPidProviderIds;
            if (caSystemEcmPids.TryGetValue(pid.Key, out ecmPidProviderIds))
            {
              ecmPidProviderIds.UnionWith(pid.Value);
            }
            else
            {
              caSystemEcmPids.Add(pid.Key, pid.Value);
            }
          }
        }
      }

      // Add the ECMs from the PMT elementary stream CA descriptors.
      this.LogDebug("MD plugin: merging PMT elementary stream CA descriptors...");
      IEnumerator<PmtElementaryStream> esEn = pmt.ElementaryStreams.GetEnumerator();
      while (esEn.MoveNext())
      {
        descEn = esEn.Current.CaDescriptors.GetEnumerator();
        while (descEn.MoveNext())
        {
          ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
          if (cad == null)
          {
            this.LogError("MD plugin: invalid descriptor");
            ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
            Dump.DumpBinary(rawDescriptor);
            continue;
          }
          Dictionary<ushort, HashSet<uint>> caSystemEcmPids;
          if (!seenEcmPids.TryGetValue(cad.CaSystemId, out caSystemEcmPids))
          {
            seenEcmPids.Add(cad.CaSystemId, cad.Pids);
          }
          else
          {
            foreach (KeyValuePair<ushort, HashSet<uint>> pid in cad.Pids)
            {
              HashSet<uint> ecmPidProviderIds;
              if (caSystemEcmPids.TryGetValue(pid.Key, out ecmPidProviderIds))
              {
                ecmPidProviderIds.UnionWith(pid.Value);
              }
              else
              {
                caSystemEcmPids.Add(pid.Key, pid.Value);
              }
            }
          }
        }
      }

      // Build a dictionary of CA system -> { EMM PID } -> { provider ID } from the CAT.
      Dictionary<ushort, Dictionary<ushort, HashSet<uint>>> seenEmmPids = new Dictionary<ushort, Dictionary<ushort, HashSet<uint>>>();
      this.LogDebug("MD plugin: reading CAT CA descriptors...");
      descEn = cat.CaDescriptors.GetEnumerator();
      while (descEn.MoveNext())
      {
        ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
        if (cad == null)
        {
          this.LogError("MD plugin: invalid descriptor");
          ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
          Dump.DumpBinary(rawDescriptor);
          continue;
        }
        Dictionary<ushort, HashSet<uint>> caSystemEmmPids;
        if (!seenEmmPids.TryGetValue(cad.CaSystemId, out caSystemEmmPids))
        {
          seenEmmPids.Add(cad.CaSystemId, cad.Pids);
        }
        else
        {
          foreach (KeyValuePair<ushort, HashSet<uint>> pid in cad.Pids)
          {
            HashSet<uint> emmPidProviderIds;
            if (caSystemEmmPids.TryGetValue(pid.Key, out emmPidProviderIds))
            {
              emmPidProviderIds.UnionWith(pid.Value);
            }
            else
            {
              caSystemEmmPids.Add(pid.Key, pid.Value);
            }
          }
        }
      }

      // Merge the dictionaries and assemble the details to pass to the MD plugin.
      ushort count = 0;   // This variable will be used to count the number of distinct CA system-ECM-EMM combinations.
      programToDecode.CaSystems = new CaSystem82[MAX_CA_SYSTEM_COUNT];
      IEnumerator<uint> en;
      this.LogDebug("MD plugin: registering PIDs...");
      foreach (KeyValuePair<ushort, Dictionary<ushort, HashSet<uint>>> caSystemEcmPids in seenEcmPids)
      {
        // Register each ECM PID and attempt to link it with an EMM PID.
        foreach (KeyValuePair<ushort, HashSet<uint>> ecmPidProviderIds in caSystemEcmPids.Value)
        {
          CaSystem82 caSystem = programToDecode.CaSystems[count];
          caSystem.CaType = caSystemEcmPids.Key;
          caSystem.EcmPid = ecmPidProviderIds.Key;
          caSystem.EmmPid = 0;
          caSystem.ProviderId = 0;

          // Do we have EMM PIDs that we could link to the ECM PIDs for this CA system?
          uint providerId = 0;
          ushort emmPid = 0;
          Dictionary<ushort, HashSet<uint>> caSystemEmmPids;
          if (seenEmmPids.TryGetValue(caSystemEcmPids.Key, out caSystemEmmPids))
          {
            foreach (KeyValuePair<ushort, HashSet<uint>> emmPidProviderIds in caSystemEmmPids)
            {
              // Prepare to attempt to link. Take a "backup" of the ECM PID provider IDs - we might need to
              // restore them if the match with this EMM PID doesn't work out.
              uint[] ecmProviderIds = new uint[ecmPidProviderIds.Value.Count];
              ecmPidProviderIds.Value.CopyTo(ecmProviderIds);
              ecmPidProviderIds.Value.IntersectWith(emmPidProviderIds.Value);

              // The attempt to match here considers 3 situations:
              // - there is at least one common provider ID
              // - the ECM PID provider ID is not known
              // - the EMM PID provider ID is not known
              // In the first case, we choose one of the common provider IDs. In the last two cases we pick one
              // of the known provider IDs and assume it applies to both ECMs and EMMs.
              if (ecmPidProviderIds.Value.Count > 0 || ecmPidProviderIds.Value.Count == 0 || emmPidProviderIds.Value.Count == 0)
              {
                // We have a match! Pick the first common provider ID.
                if (ecmPidProviderIds.Value.Count == 0)
                {
                  en = emmPidProviderIds.Value.GetEnumerator();
                }
                else
                {
                  en = ecmPidProviderIds.Value.GetEnumerator();
                }
                if (en.MoveNext())
                {
                  providerId = en.Current;
                }
                else
                {
                  providerId = 0;
                }
                emmPid = emmPidProviderIds.Key;
                break;
              }
              else
              {
                // Restore the ECM PID provider IDs for the test with the next EMM PID...
                ecmPidProviderIds.Value.UnionWith(ecmProviderIds);
              }
            }
          }

          // Have we set the provider ID yet?
          if (emmPid == 0)
          {
            // No - we didn't find an EMM PID match, so just pick the first ECM PID provider ID.
            en = ecmPidProviderIds.Value.GetEnumerator();
            if (en.MoveNext())
            {
              providerId = en.Current;
            }
          }
          else
          {
            // We matched an EMM PID - that EMM PID shouldn't be reused again.
            seenEmmPids[caSystemEcmPids.Key].Remove(emmPid);
          }
          caSystem.EmmPid = emmPid;
          caSystem.ProviderId = providerId;

          if (count == 0)
          {
            programToDecode.EcmPid = ecmPidProviderIds.Key;
          }
          count++;
          if (count == MAX_CA_SYSTEM_COUNT)
          {
            this.LogError("MD plugin: unable to register all PIDs");
            return count;
          }
        }

        // Having gone through the ECM PIDs, any remaining EMM PIDs for this CA system should be registered
        // unconditionally.
        if (!seenEmmPids.ContainsKey(caSystemEcmPids.Key))
        {
          continue;
        }
        foreach (ushort emmPid in seenEmmPids[caSystemEcmPids.Key].Keys)
        {
          CaSystem82 caSystem = programToDecode.CaSystems[count];
          caSystem.CaType = caSystemEcmPids.Key;
          caSystem.EcmPid = 0;
          caSystem.EmmPid = emmPid;
          uint providerId = 0;
          en = seenEmmPids[caSystemEcmPids.Key][emmPid].GetEnumerator();
          if (en.MoveNext())
          {
            providerId = en.Current;
          }
          caSystem.ProviderId = providerId;
          count++;
          if (count == MAX_CA_SYSTEM_COUNT)
          {
            return count;
          }
        }
        seenEmmPids.Remove(caSystemEcmPids.Key);
      }

      // Having gone through the CA systems for ECM PIDs, any remaining EMM PIDs for other CA systems should
      // be registered unconditionally.
      foreach (KeyValuePair<ushort, Dictionary<ushort, HashSet<uint>>> caSystemEmmPids in seenEmmPids)
      {
        foreach (ushort emmPid in seenEmmPids[caSystemEmmPids.Key].Keys)
        {
          CaSystem82 caSystem = programToDecode.CaSystems[count];
          caSystem.CaType = caSystemEmmPids.Key;
          caSystem.EcmPid = 0;
          caSystem.EmmPid = emmPid;
          uint providerId = 0;
          en = caSystemEmmPids.Value[emmPid].GetEnumerator();
          if (en.MoveNext())
          {
            providerId = en.Current;
          }
          caSystem.ProviderId = providerId;
          count++;
          if (count == MAX_CA_SYSTEM_COUNT)
          {
            return count;
          }
        }
      }

      return count;
    }

    /// <summary>
    /// Search the MD plugin configuration to determine which CA system, provider, EMM PID and ECM PID should
    /// be preferred for use when decrypting.
    /// </summary>
    /// <param name="programToDecode">The MD API program information structure containing the options.</param>
    private void SetPreferredCaSystemIndex(ref Program82 programToDecode)
    {
      try
      {
        this.LogDebug("MD plugin: identifying primary ECM PID");

        // Load configuration (if we have any).
        string configFile = Path.Combine(_pluginFolder, "MDAPIProvID.xml");
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
            string transportStreamId = string.Format("{0:D}", programToDecode.TransportStreamId);
            string serviceId = string.Format("{0:D}", programToDecode.ServiceId);
            string pmtPid = string.Format("{0:D}", programToDecode.PmtPid);
            foreach (XmlNode channelNode in channelList)
            {
              if (channelNode.Attributes["tp_id"].Value.Equals(transportStreamId) &&
                  channelNode.Attributes["sid"].Value.Equals(serviceId) &&
                  channelNode.Attributes["pmt_pid"].Value.Equals(pmtPid))
              {
                this.LogDebug("MD plugin: found channel configuration");
                for (byte i = 0; i < programToDecode.CaSystemCount; i++)
                {
                  CaSystem82 caSystem = programToDecode.CaSystems[i];
                  string ecmPid = string.Format("{0:D}", caSystem.EcmPid);
                  if (channelNode.Attributes["ecm_pid"].Value.Equals(ecmPid))
                  {
                    this.LogDebug("MD plugin: found correct ECM PID");
                    programToDecode.CaId = i;
                    programToDecode.EcmPid = caSystem.EcmPid;
                    configFound = true;

                    if (((XmlElement)channelNode).HasAttribute("emm_pid"))
                    {
                      caSystem.EmmPid = ushort.Parse(((XmlElement)channelNode).GetAttribute("emm_pid"));
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
                  CaSystem82 caSystem = programToDecode.CaSystems[i];
                  if (providerNode.Attributes["ID"].Value.Equals(string.Format("{0:D}", caSystem.ProviderId)))
                  {
                    this.LogDebug("MD plugin: found provider configuration");
                    programToDecode.CaId = i;
                    programToDecode.EcmPid = caSystem.EcmPid;
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
                  CaSystem82 caSystem = programToDecode.CaSystems[i];
                  if (caTypeNode.Attributes["ID"].Value.Equals(string.Format("{0:D}", caSystem.CaType)))
                  {
                    this.LogDebug("MD plugin: found CA type configuration");
                    programToDecode.CaId = i;
                    programToDecode.EcmPid = caSystem.EcmPid;
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
          this.LogDebug("MD plugin: no configuration found");

          // Now the question: do we add configuration?
          XmlElement mainNode = (XmlElement)doc.SelectSingleNode("/mdapi");
          bool fillOutConfig = false;
          if (mainNode == null || !mainNode.HasAttribute("fillout"))
          {
            if (mainNode == null)
            {
              mainNode = doc.CreateElement("mdapi");
              doc.AppendChild(mainNode);
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
            this.LogDebug("MD plugin: attempting to add entries to MDAPIProvID.xml");

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

            string comment = "Channel \"" + programToDecode.Name + "\", possible ECM PID values = {{";
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
              uint providerId = programToDecode.CaSystems[i].ProviderId;
              if (!newProviders.Contains(providerId))
              {
                node = doc.CreateElement("provider");
                attribute = doc.CreateAttribute("ID");
                attribute.Value = providerId.ToString();
                node.Attributes.Append(attribute);
                listNode.AppendChild(node);
                newProviders.Add(providerId);
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
              uint caType = programToDecode.CaSystems[i].CaType;
              if (!newProviders.Contains(caType))
              {
                node = doc.CreateElement("CA_Type");
                attribute = doc.CreateAttribute("ID");
                attribute.Value = caType.ToString();
                node.Attributes.Append(attribute);
                listNode.AppendChild(node);
                newProviders.Add(caType);
              }
            }

            doc.Save(configFile);
          }
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MD plugin: failed to load or read configuration, or set preferred CA system index");
      }
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This extension can easily be disabled on a per-tuner basis, so we will give it higher
        // priority than hardware conditional access extensions.
        return 100;
      }
    }

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "MD Plugin";
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      this.LogDebug("MD plugin: initialising");

      if (_isMdPlugin)
      {
        this.LogWarn("MD plugin: extension already initialised");
        return true;
      }

      IBaseFilter tunerFilter = context as IBaseFilter;
      if (tunerFilter == null)
      {
        this.LogDebug("MD plugin: context is not a filter");
        return false;
      }
      if (string.IsNullOrEmpty(tunerExternalId))
      {
        this.LogDebug("MD plugin: tuner external identifier is not set");
        return false;
      }

      // If there is no MD configuration folder then there is no softCAM plugin.
      if (!Directory.Exists(_pluginFolder))
      {
        this.LogDebug("MD plugin: plugin not configured");
        return false;
      }

      // Get the tuner filter name. We use it as a prefix for the tuner configuration folder.
      FilterInfo tunerFilterInfo;
      int hr = tunerFilter.QueryFilterInfo(out tunerFilterInfo);
      _configurationFolderPrefix = tunerFilterInfo.achName;
      Release.FilterInfo(ref tunerFilterInfo);
      if (hr != (int)HResult.Severity.Success || _configurationFolderPrefix == null)
      {
        this.LogError("MD plugin: failed to get the tuner filter name, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }

      try
      {
        // Look for a configuration file.
        string configFile = Path.Combine(_pluginFolder, "MDAPICards.xml");
        int slotCount = 1;
        string providerList = "all";
        XmlDocument doc = new XmlDocument();
        XmlNode rootNode = null;
        XmlNode tunerNode = null;
        if (File.Exists(configFile))
        {
          this.LogDebug("MD plugin: searching for configuration, external identifier = {0}", tunerExternalId);
          doc.Load(configFile);
          rootNode = doc.SelectSingleNode("/cards");
          if (rootNode != null)
          {
            XmlNodeList tunerList = doc.SelectNodes("/cards/card");
            if (tunerList != null)
            {
              this.LogDebug("MD plugin: found {0} configuration(s)", tunerList.Count);
              foreach (XmlNode node in tunerList)
              {
                if (node.Attributes["DevicePath"].Value.Equals(tunerExternalId))
                {
                  // We found the configuration for the tuner.
                  this.LogDebug("MD plugin: found matching configuration");
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
          this.LogDebug("MD plugin: creating configuration");
          tunerNode = doc.CreateElement("card");

          // The "name" attribute is used as the configuration folder prefix.
          XmlAttribute attr = doc.CreateAttribute("Name");
          attr.InnerText = _configurationFolderPrefix;
          tunerNode.Attributes.Append(attr);

          // Used to identify the tuner.
          attr = doc.CreateAttribute("DevicePath");
          attr.InnerText = tunerExternalId;
          tunerNode.Attributes.Append(attr);

          // Default: enable one instance of the plugin.
          attr = doc.CreateAttribute("EnableMdapi");
          attr.InnerText = slotCount.ToString();
          tunerNode.Attributes.Append(attr);

          // Default: the plugin can decrypt any program from any provider.
          attr = doc.CreateAttribute("Provider");
          attr.InnerText = "all";
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
          catch (Exception)
          {
            // Assume that the plugin is enabled unless the parameter says "no".
            if (tunerNode.Attributes["EnableMdapi"].Value.Equals("no"))
            {
              slotCount = 0;
            }
            tunerNode.Attributes["EnableMdapi"].Value = slotCount.ToString();
            doc.Save(configFile);
          }
          try
          {
            providerList = tunerNode.Attributes["Provider"].Value.ToLowerInvariant();
            if (!providerList.Equals("all"))
            {
              _providers = new HashSet<string>(Regex.Split(providerList.Trim(), @"\s*,\s*"));
            }
          }
          catch (Exception)
          {
            // Assume that the plugin can decrypt any program.
            XmlAttribute providerListNode = tunerNode.Attributes["Provider"];
            if (providerListNode == null)
            {
              providerListNode = doc.CreateAttribute("Provider"); 
              tunerNode.Attributes.Append(providerListNode);
            }
            providerListNode.InnerText = "all";
            doc.Save(configFile);
          }
        }

        if (slotCount > 0)
        {
          this.LogInfo("MD plugin: plugin is enabled for {0} decoding slot(s)", slotCount);
          _isMdPlugin = true;
          _slots = new List<DecodeSlot>(slotCount);
          if (_providers.Count == 0)
          {
            this.LogDebug("MD plugin: plugin can decrypt programs from any provider");
          }
          else
          {
            this.LogDebug("MD plugin: plugin can decrypt programs from provider(s) \"{0}\"", providerList);
          }
          return true;
        }

        this.LogDebug("MD plugin: plugin is not enabled");
        return false;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MD plugin: failed to create, load or read configuration");
        return false;
      }
    }

    #endregion

    #region IDirectShowAddOnDevice member

    /// <summary>
    /// Insert and connect additional filter(s) into the graph.
    /// </summary>
    /// <param name="graph">The tuner filter graph.</param>
    /// <param name="lastFilter">The source filter (usually either a capture/receiver or
    ///   multiplexer filter) to connect the [first] additional filter to.</param>
    /// <returns><c>true</c> if one or more additional filters were successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(IFilterGraph2 graph, ref IBaseFilter lastFilter)
    {
      this.LogDebug("MD plugin: add to graph");

      if (!_isMdPlugin)
      {
        this.LogWarn("MD plugin: not initialised or interface not supported");
        return false;
      }
      if (graph == null)
      {
        this.LogError("MD plugin: failed to add the filter(s) to the graph, graph is null");
        return false;
      }
      if (lastFilter == null)
      {
        this.LogError("MD plugin: failed to add the filter(s) to the graph, last filter is null");
        return false;
      }
      if (_slots != null && _slots.Count > 0 && _slots[0].Filter != null)
      {
        this.LogWarn("MD plugin: {0} filter(s) already in graph", _slots.Count);
        return true;
      }

      // Add an infinite tee after the tuner/capture filter.
      this.LogDebug("MD plugin: adding infinite tee");
      _graph = graph;
      _infTee = (IBaseFilter)new InfTee();
      int hr = _graph.AddFilter(_infTee, "MD Plugin Infinite Tee");
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("MD plugin: failed to add the infinite tee to the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      IPin outputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
      IPin inputPin = DsFindPin.ByDirection(_infTee, PinDirection.Input, 0);
      hr = _graph.ConnectDirect(outputPin, inputPin, null);
      Release.ComObject("MD plugin upstream filter output pin", ref outputPin);
      Release.ComObject("MD plugin infinite tee input pin", ref inputPin);
      if (hr != (int)HResult.Severity.Success)
      {
        this.LogError("MD plugin: failed to connect the infinite tee into the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      lastFilter = _infTee;

      // Add one filter for each decoding slot. Note that we preset the capacity in Initialise().
      this.LogDebug("MD plugin: adding {0} decoding filter(s)", _slots.Capacity);
      for (int i = 0; i < _slots.Capacity; i++)
      {
        DecodeSlot slot = new DecodeSlot();
        slot.Filter = (IBaseFilter)new MDAPIFilter();
        slot.CurrentChannel = null;

        // Add the filter to the graph.
        hr = _graph.AddFilter(slot.Filter, "MDAPI Filter " + i);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("MD plugin: failed to add MD plugin filter {0} to the graph, hr = 0x{1:x} ({2})", i + 1, hr, HResult.GetDXErrorString(hr));
          return false;
        }

        // Connect the filter into the graph.
        outputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
        inputPin = DsFindPin.ByDirection(slot.Filter, PinDirection.Input, 0);
        hr = _graph.ConnectDirect(outputPin, inputPin, null);
        Release.ComObject("MD plugin source filter output pin", ref outputPin);
        Release.ComObject("MD plugin MDAPI filter input pin", ref inputPin);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("MD plugin: failed to connect MD plugin filter {0} into the graph, hr = 0x{1:x} ({2})", i + 1, hr, HResult.GetDXErrorString(hr));
          return false;
        }
        lastFilter = slot.Filter;
        _slots.Add(slot);

        // Check whether the plugin supports extended capabilities.
        IChangeChannel temp = slot.Filter as IChangeChannel;
        try
        {
          temp.SetPluginsDirectory(_configurationFolderPrefix + i);
          this.LogDebug("MD plugin: plugins directory is \"{0}{1}\"", _configurationFolderPrefix, i);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "MD plugin: failed to set plugin directory");
          return false;
        }
        IChangeChannel_Ex temp2 = slot.Filter as IChangeChannel_Ex;
        if (temp2 != null)
        {
          this.LogDebug("  filter {0}, extended capabilities supported", i + 1);
        }
        else
        {
          this.LogDebug("  filter {0}, extended capabilities not supported", i + 1);
        }
        IChangeChannel_Clear temp3 = slot.Filter as IChangeChannel_Clear;
        if (temp3 != null)
        {
          this.LogDebug("  filter {0}, channel clearing capabilities supported", i + 1);
        }
        else
        {
          this.LogDebug("  filter {0}, channel clearing capabilities not supported", i + 1);
        }
      }

      // Note all cleanup is done in Dispose(), which should be called immediately if we return false.
      return true;
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenConditionalAccessInterface()
    {
      this.LogDebug("MD plugin: open conditional access interface");

      if (!_isMdPlugin)
      {
        this.LogWarn("MD plugin: not initialised or interface not supported");
        return false;
      }
      if (_slots == null || _slots.Count == 0)
      {
        this.LogError("MD plugin: filter(s) not added to the BDA filter graph");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("MD plugin: conditional access interface is already open");
        return true;
      }

      _programBuffer = Marshal.AllocCoTaskMem(PROGRAM82_SIZE);
      _pidBuffer = Marshal.AllocCoTaskMem(PIDS_TO_DECODE_SIZE);
      _isCaInterfaceOpen = true;

      this.LogDebug("MD plugin: result = success");
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseConditionalAccessInterface()
    {
      this.LogDebug("MD plugin: close conditional access interface");

      if (_programBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_programBuffer);
        _programBuffer = IntPtr.Zero;
      }
      if (_pidBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_pidBuffer);
        _pidBuffer = IntPtr.Zero;
      }
      _isCaInterfaceOpen = false;

      this.LogDebug("MD plugin: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="resetTuner">This parameter will be set to <c>true</c> if the tuner must be reset
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    public bool ResetConditionalAccessInterface(out bool resetTuner)
    {
      this.LogDebug("MD plugin: reset conditional access interface");

      // We have to rebuild the graph to reset anything.
      resetTuner = true;
      return true;
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsConditionalAccessInterfaceReady()
    {
      this.LogDebug("MD plugin: is conditional access interface ready");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("MD plugin: not initialised or interface not supported");
        return false;
      }

      // As far as we know, the interface is always ready as long as it is open.
      this.LogDebug("MD plugin: result = {0}", _slots.Count > 0);
      return (_slots.Count > 0);
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendConditionalAccessCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      this.LogDebug("MD plugin: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("MD plugin: not initialised or interface not supported");
        return true;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        this.LogError("MD plugin: conditional access command type {0} is not supported", command);
        return true;
      }
      if (pmt == null)
      {
        this.LogError("MD plugin: failed to send conditional access command, PMT not supplied");
        return true;
      }
      if (cat == null)
      {
        this.LogError("MD plugin: failed to send conditional access command, CAT not supplied");
        return true;
      }
      DVBBaseChannel dvbChannel = channel as DVBBaseChannel;
      if (dvbChannel == null)
      {
        this.LogDebug("MD plugin: channel is not a DVB channel");
        return true;
      }
      string lowerProvider = dvbChannel.Provider == null ? string.Empty : dvbChannel.Provider.ToLowerInvariant();
      if (_providers.Count != 0 && !_providers.Contains(lowerProvider))
      {
        this.LogDebug("MD plugin: plugin not configured to decrypt programs for provider \"{0}\"", dvbChannel.Provider);
        return false;
      }

      // Find a free slot to decode this program. If this is the first or only program in the list then we
      // can reset our slots. This could be optimised to cache the new list until "last"/"only" action and
      // reuse slots where possible.
      DecodeSlot freeSlot = null;
      if (command == CaPmtCommand.OkDescrambling && (listAction == CaPmtListManagementAction.First || listAction == CaPmtListManagementAction.Only))
      {
        this.LogDebug("MD plugin: freeing all slots");
        foreach (DecodeSlot slot in _slots)
        {
          slot.CurrentChannel = null;
          IChangeChannel_Clear clear = slot.Filter as IChangeChannel_Clear;
          if (clear != null)
          {
            clear.ClearChannel();
          }
        }
        freeSlot = _slots[0];
      }
      else
      {
        foreach (DecodeSlot slot in _slots)
        {
          DVBBaseChannel currentProgram = slot.CurrentChannel as DVBBaseChannel;
          if (currentProgram != null && currentProgram.ServiceId == dvbChannel.ServiceId)
          {
            // "Not selected" means stop decrypting the program.
            if (command == CaPmtCommand.NotSelected)
            {
              this.LogDebug("MD plugin: found existing slot decrypting channel \"{0}\", freeing", currentProgram.Name);
              slot.CurrentChannel = null;
              IChangeChannel_Clear clear = slot.Filter as IChangeChannel_Clear;
              if (clear != null)
              {
                clear.ClearChannel();
              }
              return true;
            }
            // "Ok descrambling" means start or continue decrypting the program. If we're already decrypting
            // the program that is fine - this is an update.
            else if (command == CaPmtCommand.OkDescrambling)
            {
              this.LogDebug("MD plugin: found existing slot decrypting channel \"{0}\", sending update", currentProgram.Name);
              freeSlot = slot;
              // No need to continue looping - this is the optimal situation where we reuse the existing slot.
              break;
            }
          }
          else if (currentProgram == null)
          {
            this.LogDebug("  <free slot>...");
            if (freeSlot == null)
            {
              freeSlot = slot;
            }
          }
          else if (currentProgram != null)
          {
            this.LogDebug("  \"{0}\"...", currentProgram.Name);
          }
        }
      }

      if (command == CaPmtCommand.NotSelected)
      {
        // If we get to here then we were asked to stop decrypting
        // a program that we were not decrypting. Strange...
        this.LogWarn("MD plugin: received \"not selected\" command for program that is not being decrypted");
        return true;
      }

      if (freeSlot == null)
      {
        // If we get to here then we were asked to start decrypting
        // a program, but we don't have any free slots to do it with.
        this.LogError("MD plugin: failed to send conditional access command, no free decrypt slots");
        return false;
      }

      // If we get to here then we need to try to start decrypting the program.
      Program82 programToDecode;
      PidsToDecode pidsToDecode;
      RegisterVideoAndAudioPids(pmt, out programToDecode, out pidsToDecode);

      // Set the fields that we are able to set.
      if (dvbChannel.Name != null)
      {
        programToDecode.Name = dvbChannel.Name;
      }
      if (dvbChannel.Provider != null)
      {
        programToDecode.Provider = dvbChannel.Provider;
      }
      programToDecode.TransportStreamId = (ushort)dvbChannel.TransportId;
      programToDecode.PmtPid = (ushort)dvbChannel.PmtPid;

      // We don't know what the actual service type is in this
      // context, but we can at least indicate whether this is
      // a TV or radio service.
      // TODO: Sebastiii removed this, why?
      //programToDecode.ServiceType = (byte)(dvbChannel.IsTv ? DvbServiceType.DigitalTelevision : DvbServiceType.DigitalRadio); //TODO look if needed
      programToDecode.ServiceType = (byte)DvbServiceType.DigitalTelevision;

      this.LogDebug("MD plugin: TSID = {0}, service ID = {1}, PMT PID = {2}, PCR PID = {3}, service type = {4}, " +
                        "video PID = {5}, audio PID = {6}, AC3 PID = {7}, teletext PID = {8}",
          programToDecode.TransportStreamId, programToDecode.ServiceId, programToDecode.PmtPid, programToDecode.PcrPid,
          programToDecode.ServiceType, programToDecode.VideoPid, programToDecode.AudioPid, programToDecode.Ac3AudioPid, programToDecode.TeletextPid
      );

      programToDecode.CaSystemCount = RegisterEcmAndEmmPids(pmt, cat, ref programToDecode);
      SetPreferredCaSystemIndex(ref programToDecode);

      this.LogDebug("MD plugin: ECM PID = {0}, CA system count = {1}, CA index = {2}",
                    programToDecode.EcmPid, programToDecode.CaSystemCount, programToDecode.CaId
      );
      for (byte i = 0; i < programToDecode.CaSystemCount; i++)
      {
        CaSystem82 caSystem = programToDecode.CaSystems[i];
        this.LogDebug("MD plugin: #{0} CA type = 0x{1:x4}, ECM PID = {2}, EMM PID = {3}, provider = 0x{4:x4}",
                      i + 1, caSystem.CaType, caSystem.EcmPid, caSystem.EmmPid, caSystem.ProviderId
        );
      }

      // Instruct the MD filter to decrypt the program.
      Marshal.StructureToPtr(programToDecode, _programBuffer, false);
      //Dump.DumpBinary(_programBuffer, PROGRAM82_SIZE);
      Marshal.StructureToPtr(pidsToDecode, _pidBuffer, false);
      //Dump.DumpBinary(_pidBuffer, PIDS_TO_DECODE_SIZE);
      try
      {
        IChangeChannel_Ex changeEx = freeSlot.Filter as IChangeChannel_Ex;
        if (changeEx != null)
        {
          changeEx.ChangeChannelTP82_Ex(_programBuffer, _pidBuffer);
        }
        else
        {
          IChangeChannel change = freeSlot.Filter as IChangeChannel;
          if (change != null)
          {
            change.ChangeChannelTP82(_programBuffer);
          }
          else
          {
            throw new Exception("Failed to acquire interface on filter.");
          }
        }
        freeSlot.CurrentChannel = channel;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MD plugin: failed to change channel");
        return false;
      }

      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      if (_isMdPlugin)
      {
        CloseConditionalAccessInterface();
      }

      if (_graph != null)
      {
        _graph.RemoveFilter(_infTee);

        if (_slots != null)
        {
          foreach (DecodeSlot slot in _slots)
          {
            _graph.RemoveFilter(slot.Filter);
          }
        }

        Release.ComObject("MD plugin graph", ref _graph);
      }

      Release.ComObject("MD plugin infinite tee", ref _infTee);
      if (_slots != null)
      {
        foreach (DecodeSlot slot in _slots)
        {
          Release.ComObject("MD plugin MDAPI filter", ref slot.Filter);
        }
      }
      _slots = null;

      _isMdPlugin = false;
    }

    #endregion
  }
}