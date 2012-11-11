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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.CustomDevices.MdPlugin
{
  /// <summary>
  /// A class for handling conditional access with softCAM plugins that support Agarwal's multi-decrypt
  /// plugin API.
  /// </summary>
  public class MdPlugin : BaseCustomDevice, IAddOnDevice, IConditionalAccessProvider
  {


    #region COM interface imports

    [ComImport, Guid("72e6dB8f-9f33-4d1c-a37c-de8148c0be74")]
    private class MDAPIFilter { };

    [ComVisible(true), ComImport,
      Guid("c3f5aa0d-c475-401b-8fc9-e33fb749cd85"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel
    {
      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different service using specific parameters.
      /// </summary>
      [PreserveSig]
      int ChangeChannel(int frequency, int bandwidth, int polarity, int videoPid, int audioPid, int ecmPid, int caId, int providerId);

      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different service using a Program82 structure.
      /// </summary>
      int ChangeChannelTP82(IntPtr program82);

      ///<summary>
      /// Set the plugin directory.
      ///</summary>
      int SetPluginsDirectory([MarshalAs(UnmanagedType.LPWStr)] String directory);
    }

    [ComVisible(true), ComImport,
      Guid("e98b70ee-f5a1-4f46-b8b8-a1324ba92f5f"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel_Ex
    {
      /// <summary>
      /// Instruct the softCAM plugin filter to decode a different service using Program82 and PidsToDecode structures.
      /// </summary>
      [PreserveSig]
      int ChangeChannelTP82_Ex(IntPtr program82, IntPtr pidsToDecode);
    }

    [ComVisible(true), ComImport,
      Guid("D0EACAB1-3211-414B-B58B-E1157AC4D93A"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel_Clear
    {
      /// <summary>
      /// Inform the softCAM plugin that it no longer needs to decrypt a service.
      /// </summary>
      [PreserveSig]
      int ClearChannel();
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
      public UInt16 SymbolRate; // unit = ks/s
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
    private HashSet<String> _providers = new HashSet<String>();
    private String _configurationFolderPrefix = String.Empty;
    private IFilterGraph2 _graph = null;
    private IBaseFilter _infTee = null;
    private List<DecodeSlot> _slots = null;
    private IntPtr _programmeBuffer = IntPtr.Zero;
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
      this.LogDebug("MD Plugin: registering video and audio PIDs");
      programToDecode = new Program82();
      pidsToDecode = new PidsToDecode();
      pidsToDecode.Pids = new UInt16[MaxPidCount];

      programToDecode.ServiceId = pmt.ProgramNumber;
      programToDecode.PcrPid = pmt.PcrPid;

      foreach (PmtElementaryStream es in pmt.ElementaryStreams)
      {
        // When using a plugin with extended support, we can just
        // specify the PIDs that need to be decoded.
        if (pidsToDecode.PidCount < MaxPidCount)
        {
          // TODO: restrict to video, audio, subtitle and teletext PIDs???
          pidsToDecode.Pids[pidsToDecode.PidCount++] = es.Pid;
        }
        else
        {
          this.LogDebug("MD Plugin: unable to register all PIDs");
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
          if (es.LogicalStreamType == LogicalStreamType.Ac3 || es.LogicalStreamType == LogicalStreamType.EnhancedAc3)
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
    private UInt16 RegisterEcmAndEmmPids(Pmt pmt, Cat cat, ref Program82 programToDecode)
    {
      this.LogDebug("MD Plugin: registering ECM and EMM details");

      // Build a dictionary of CA system -> { ECM PID } -> { provider ID } from the PMT.
      Dictionary<UInt16, Dictionary<UInt16, HashSet<UInt32>>> seenEcmPids = new Dictionary<UInt16, Dictionary<UInt16, HashSet<UInt32>>>();

      // First get ECMs from the PMT program CA descriptors.
      this.LogDebug("MD Plugin: reading PMT program CA descriptors...");
      IEnumerator<IDescriptor> descEn = pmt.ProgramCaDescriptors.GetEnumerator();
      while (descEn.MoveNext())
      {
        ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
        if (cad == null)
        {
          this.LogDebug("MD Plugin: invalid descriptor");
          ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
          DVB_MMI.DumpBinary(rawDescriptor, 0, rawDescriptor.Count);
          continue;
        }
        if (!seenEcmPids.ContainsKey(cad.CaSystemId))
        {
          seenEcmPids.Add(cad.CaSystemId, cad.Pids);
        }
        else
        {
          foreach (UInt16 pid in cad.Pids.Keys)
          {
            if (seenEcmPids[cad.CaSystemId].ContainsKey(pid))
            {
              seenEcmPids[cad.CaSystemId][pid].UnionWith(cad.Pids[pid]);
            }
            else
            {
              seenEcmPids[cad.CaSystemId].Add(pid, cad.Pids[pid]);
            }
          }
        }
      }

      // Add the ECMs from the PMT elementary stream CA descriptors.
      this.LogDebug("MD Plugin: merging PMT elementary stream CA descriptors...");
      IEnumerator<PmtElementaryStream> esEn = pmt.ElementaryStreams.GetEnumerator();
      while (esEn.MoveNext())
      {
        descEn = esEn.Current.CaDescriptors.GetEnumerator();
        while (descEn.MoveNext())
        {
          ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
          if (cad == null)
          {
            this.LogDebug("MD Plugin: invalid descriptor");
            ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
            DVB_MMI.DumpBinary(rawDescriptor, 0, rawDescriptor.Count);
            continue;
          }
          if (!seenEcmPids.ContainsKey(cad.CaSystemId))
          {
            seenEcmPids.Add(cad.CaSystemId, cad.Pids);
          }
          else
          {
            foreach (UInt16 pid in cad.Pids.Keys)
            {
              if (seenEcmPids[cad.CaSystemId].ContainsKey(pid))
              {
                seenEcmPids[cad.CaSystemId][pid].UnionWith(cad.Pids[pid]);
              }
              else
              {
                seenEcmPids[cad.CaSystemId].Add(pid, cad.Pids[pid]);
              }
            }
          }
        }
      }

      // Build a dictionary of CA system -> { EMM PID } -> { provider ID } from the CAT.
      Dictionary<UInt16, Dictionary<UInt16, HashSet<UInt32>>> seenEmmPids = new Dictionary<UInt16, Dictionary<UInt16, HashSet<UInt32>>>();
      this.LogDebug("MD Plugin: reading CAT CA descriptors...");
      descEn = cat.CaDescriptors.GetEnumerator();
      while (descEn.MoveNext())
      {
        ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
        if (cad == null)
        {
          this.LogDebug("MD Plugin: invalid descriptor");
          ReadOnlyCollection<byte> rawDescriptor = descEn.Current.GetRawData();
          DVB_MMI.DumpBinary(rawDescriptor, 0, rawDescriptor.Count);
          continue;
        }
        if (!seenEmmPids.ContainsKey(cad.CaSystemId))
        {
          seenEmmPids.Add(cad.CaSystemId, cad.Pids);
        }
        else
        {
          foreach (UInt16 pid in cad.Pids.Keys)
          {
            if (seenEmmPids[cad.CaSystemId].ContainsKey(pid))
            {
              seenEmmPids[cad.CaSystemId][pid].UnionWith(cad.Pids[pid]);
            }
            else
            {
              seenEmmPids[cad.CaSystemId].Add(pid, cad.Pids[pid]);
            }
          }
        }
      }

      // Merge the dictionaries and assemble the details to pass to the MD plugin.
      UInt16 count = 0;   // This variable will be used to count the number of distinct CA system-ECM-EMM combinations.
      programToDecode.CaSystems = new CaSystem82[MaxCaSystemCount];
      IEnumerator<UInt32> en;
      this.LogDebug("MD Plugin: registering PIDs...");
      foreach (UInt16 caSystemId in seenEcmPids.Keys)
      {
        // Register each ECM PID and attempt to link it with an EMM PID.
        foreach (UInt16 ecmPid in seenEcmPids[caSystemId].Keys)
        {
          programToDecode.CaSystems[count].CaType = caSystemId;
          programToDecode.CaSystems[count].EcmPid = ecmPid;
          programToDecode.CaSystems[count].EmmPid = 0;
          programToDecode.CaSystems[count].ProviderId = 0;

          // Do we have EMM PIDs that we could link to the ECM PIDs for this CA system?
          UInt32 providerId = 0;
          UInt16 emmPid = 0;
          if (seenEmmPids.ContainsKey(caSystemId))
          {
            foreach (UInt16 emm in seenEmmPids[caSystemId].Keys)
            {
              // Prepare to attempt to link. Take a "backup" of the ECM PID provider IDs - we might need to
              // restore them if the match with this EMM PID doesn't work out.
              UInt32[] ecmProviderIds = new UInt32[seenEcmPids[caSystemId][ecmPid].Count];
              seenEcmPids[caSystemId][ecmPid].CopyTo(ecmProviderIds);
              seenEcmPids[caSystemId][ecmPid].IntersectWith(seenEmmPids[caSystemId][emm]);

              // The attempt to match here considers 3 situations:
              // - there is at least one common provider ID
              // - the ECM PID provider ID is not known
              // - the EMM PID provider ID is not known
              // In the first case, we choose one of the common provider IDs. In the last two cases we pick one
              // of the known provider IDs and assume it applies to both ECMs and EMMs.
              if (seenEcmPids[caSystemId][ecmPid].Count > 0 || seenEcmPids[caSystemId][ecmPid].Count == 0 || seenEmmPids[caSystemId][emm].Count == 0)
              {
                // We have a match! Pick the first common provider ID.
                if (seenEcmPids[caSystemId][ecmPid].Count == 0)
                {
                  en = seenEmmPids[caSystemId][emm].GetEnumerator();
                }
                else
                {
                  en = seenEcmPids[caSystemId][ecmPid].GetEnumerator();
                }
                if (en.MoveNext())
                {
                  providerId = en.Current;
                }
                else
                {
                  providerId = 0;
                }
                emmPid = emm;
                break;
              }
              else
              {
                // Restore the ECM PID provider IDs for the test with the next EMM PID...
                seenEcmPids[caSystemId][ecmPid].UnionWith(ecmProviderIds);
              }
            }
          }

          // Have we set the provider ID yet?
          if (emmPid == 0)
          {
            // No - we didn't find an EMM PID match, so just pick the first ECM PID provider ID.
            en = seenEcmPids[caSystemId][ecmPid].GetEnumerator();
            if (en.MoveNext())
            {
              providerId = en.Current;
            }
          }
          else
          {
            // We matched an EMM PID - that EMM PID shouldn't be reused again.
            seenEmmPids[caSystemId].Remove(emmPid);
          }
          programToDecode.CaSystems[count].EmmPid = emmPid;
          programToDecode.CaSystems[count].ProviderId = providerId;

          if (count == 0)
          {
            programToDecode.EcmPid = ecmPid;
          }
          count++;
          if (count == 32)
          {
            this.LogDebug("MD Plugin: unable to register all PIDs");
            return count;
          }
        }

        // Having gone through the ECM PIDs, any remaining EMM PIDs for this CA system should be registered
        // unconditionally.
        if (!seenEmmPids.ContainsKey(caSystemId))
        {
          continue;
        }
        foreach (UInt16 emmPid in seenEmmPids[caSystemId].Keys)
        {
          programToDecode.CaSystems[count].CaType = caSystemId;
          programToDecode.CaSystems[count].EcmPid = 0;
          programToDecode.CaSystems[count].EmmPid = emmPid;
          UInt32 providerId = 0;
          en = seenEmmPids[caSystemId][emmPid].GetEnumerator();
          if (en.MoveNext())
          {
            providerId = en.Current;
          }
          programToDecode.CaSystems[count].ProviderId = providerId;
          count++;
          if (count == 32)
          {
            return count;
          }
        }
        seenEmmPids.Remove(caSystemId);
      }

      // Having gone through the CA systems for ECM PIDs, any remaining EMM PIDs for other CA systems should
      // be registered unconditionally.
      foreach (UInt16 caSystemId in seenEmmPids.Keys)
      {
        foreach (UInt16 emmPid in seenEmmPids[caSystemId].Keys)
        {
          programToDecode.CaSystems[count].CaType = caSystemId;
          programToDecode.CaSystems[count].EcmPid = 0;
          programToDecode.CaSystems[count].EmmPid = emmPid;
          UInt32 providerId = 0;
          en = seenEmmPids[caSystemId][emmPid].GetEnumerator();
          if (en.MoveNext())
          {
            providerId = en.Current;
          }
          programToDecode.CaSystems[count].ProviderId = providerId;
          count++;
          if (count == 32)
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
        this.LogDebug("MD Plugin: identifying primary ECM PID");

        // Load configuration (if we have any).
        String configFile = PathManager.BuildAssemblyRelativePath("MDPLUGINS\\MDAPIProvID.xml");
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
                this.LogDebug("MD Plugin: found channel configuration");
                for (byte i = 0; i < programToDecode.CaSystemCount; i++)
                {
                  String ecmPid = String.Format("{0:D}", programToDecode.CaSystems[i].EcmPid);
                  if (channelNode.Attributes["ecm_pid"].Value.Equals(ecmPid))
                  {
                    this.LogDebug("MD Plugin: found correct ECM PID");
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
                    this.LogDebug("MD Plugin: found provider configuration");
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
                    this.LogDebug("MD Plugin: found CA type configuration");
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
          this.LogDebug("MD Plugin: no configuration found");

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
            this.LogDebug("MD Plugin: attempting to add entries to MDAPIProvID.xml");

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
        this.LogError(ex, "MD Plugin: failed to load or read configuration, or set preferred CA system index");
      }
    }

    #region ICustomDevice members

    /// <summary>
    /// The loading priority for this device type.
    /// </summary>
    public override byte Priority
    {
      get
      {
        // This plugin can easily be disabled on a per-tuner basis, so we will give it higher priority than
        // hardware conditional access interfaces.
        return 100;
      }
    }

    /// <summary>
    /// A human-readable name for the device. This could be a manufacturer or reseller name, or even a model
    /// name/number.
    /// </summary>
    public override String Name
    {
      get
      {
        return "MD Plugin";
      }
    }

    /// <summary>
    /// Attempt to initialise the device-specific interfaces supported by the class. If initialisation fails,
    /// the ICustomDevice instance should be disposed immediately.
    /// </summary>
    /// <param name="tunerFilter">The tuner filter in the BDA graph.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="tunerDevicePath">The device path of the DsDevice associated with the tuner filter.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(IBaseFilter tunerFilter, CardType tunerType, String tunerDevicePath)
    {
      this.LogDebug("MD Plugin: initialising device");

      if (tunerFilter == null)
      {
        this.LogDebug("MD Plugin: tuner filter is null");
        return false;
      }
      if (String.IsNullOrEmpty(tunerDevicePath))
      {
        this.LogDebug("MD Plugin: tuner device path is not set");
        return false;
      }
      if (_isMdPlugin)
      {
        this.LogDebug("MD Plugin: device is already initialised");
        return true;
      }

      // If there is no MD configuration folder then there is no softCAM plugin.
      if (!Directory.Exists("MDPLUGINS"))
      {
        this.LogDebug("MD Plugin: plugin not configured");
        return false;
      }

      // Get the tuner filter name. We use it as a prefix for the device configuration folder.
      FilterInfo tunerFilterInfo;
      int hr = tunerFilter.QueryFilterInfo(out tunerFilterInfo);
      if (tunerFilterInfo.pGraph != null)
      {
        DsUtils.ReleaseComObject(tunerFilterInfo.pGraph);
        tunerFilterInfo.pGraph = null;
      }
      if (hr != 0 || tunerFilterInfo.achName == null)
      {
        this.LogDebug("MD Plugin: failed to get the tuner filter name, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
      }
      _configurationFolderPrefix = tunerFilterInfo.achName;

      try
      {
        // Look for a configuration file.
        String configFile= PathManager.BuildAssemblyRelativePath("MDPLUGINS\\MDAPICards.xml");
        int slotCount = 1;
        String providerList = "all";
        XmlDocument doc = new XmlDocument();
        XmlNode rootNode = null;
        XmlNode tunerNode = null;
        if (File.Exists(configFile))
        {
          this.LogDebug("MD Plugin: searching for device configuration, device path = {0}", tunerDevicePath);
          doc.Load(configFile);
          rootNode = doc.SelectSingleNode("/cards");
          if (rootNode != null)
          {
            XmlNodeList tunerList = doc.SelectNodes("/cards/card");
            if (tunerList != null)
            {
              this.LogDebug("MD Plugin: found {0} device configuration(s)", tunerList.Count);
              foreach (XmlNode node in tunerList)
              {
                if (node.Attributes["DevicePath"].Value.Equals(tunerDevicePath))
                {
                  // We found the configuration for the tuner.
                  this.LogDebug("MD Plugin: found configuration");
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
          this.LogDebug("MD Plugin: creating device configuration");
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

          // Default: the plugin can decrypt any service from any provider.
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
              _providers = new HashSet<String>(Regex.Split(providerList.Trim(), @"\s*,\s*"));
            }
          }
          catch (Exception)
          {
            // Assume that the plugin can decrypt any service.
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
          this.LogDebug("MD Plugin: plugin is enabled for {0} decoding slot(s)", slotCount);
          _isMdPlugin = true;
          _slots = new List<DecodeSlot>(slotCount);
          if (_providers.Count == 0)
          {
            this.LogDebug("MD Plugin: plugin can decrypt services from any provider");
          }
          else
          {
            this.LogDebug("MD Plugin: plugin can decrypt services from provider(s) \"{0}\"", providerList);
          }
          return true;
        }

        this.LogDebug("MD Plugin: plugin is not enabled");
        return false;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MD Plugin: failed to create, load or read configuration");
        return false;
      }
    }

    #endregion

    #region IAddOnDevice member

    /// <summary>
    /// Insert and connect the device's additional filter(s) into the BDA graph.
    /// [network provider]->[tuner]->[capture]->[...device filter(s)]->[infinite tee]->[MPEG 2 demultiplexer]->[transport information filter]->[transport stream writer]
    /// </summary>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver filter) to
    ///   connect the [first] device filter to.</param>
    /// <returns><c>true</c> if the device was successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(ref IBaseFilter lastFilter)
    {
      this.LogDebug("MD Plugin: add to graph");

      if (!_isMdPlugin)
      {
        this.LogDebug("MD Plugin: device not initialised or interface not supported");
        return false;
      }
      if (lastFilter == null)
      {
        this.LogDebug("MD Plugin: upstream filter is null");
        return false;
      }
      if (_slots != null && _slots.Count > 0 && _slots[0].Filter != null)
      {
        this.LogDebug("MD Plugin: {0} device filter(s) already in graph", _slots.Count);
        return true;
      }

      // We need a reference to the graph.
      FilterInfo filterInfo;
      int hr = lastFilter.QueryFilterInfo(out filterInfo);
      if (hr != 0)
      {
        this.LogDebug("MD Plugin: failed to get filter info, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      _graph = filterInfo.pGraph as IFilterGraph2;
      if (_graph == null)
      {
        this.LogDebug("MD Plugin: failed to get graph reference");
        return false;
      }

      // Add an infinite tee after the tuner/capture filter.
      this.LogDebug("MD Plugin: adding infinite tee");
      _infTee = (IBaseFilter)new InfTee();
      hr = _graph.AddFilter(_infTee, "MD Plugin Infinite Tee");
      if (hr != 0)
      {
        this.LogDebug("MD Plugin: failed to add the inf tee to the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      IPin outputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
      IPin inputPin = DsFindPin.ByDirection(_infTee, PinDirection.Input, 0);
      hr = _graph.Connect(outputPin, inputPin);
      DsUtils.ReleaseComObject(outputPin);
      outputPin = null;
      DsUtils.ReleaseComObject(inputPin);
      inputPin = null;
      if (hr != 0)
      {
        this.LogDebug("MD Plugin: failed to connect the inf tee into the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
        return false;
      }
      lastFilter = _infTee;

      // Add one filter for each decoding slot. Note that we preset the capacity in Initialise().
      this.LogDebug("MD Plugin: adding {0} decoding filter(s)", _slots.Capacity);
      for (int i = 0; i < _slots.Capacity; i++)
      {
        DecodeSlot slot = new DecodeSlot();
        slot.Filter = (IBaseFilter)new MDAPIFilter();
        slot.CurrentChannel = null;

        // Add the filter to the graph.
        hr = _graph.AddFilter(slot.Filter, "MDAPI Filter " + i);
        if (hr != 0)
        {
          this.LogDebug("MD Plugin: failed to add MD plugin filter {0} to the graph, hr = 0x{1:x} ({2})", i + 1, hr, HResult.GetDXErrorString(hr));
          return false;
        }

        // Connect the filter into the graph.
        outputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
        inputPin = DsFindPin.ByDirection(slot.Filter, PinDirection.Input, 0);
        hr = _graph.Connect(outputPin, inputPin);
        DsUtils.ReleaseComObject(outputPin);
        outputPin = null;
        DsUtils.ReleaseComObject(inputPin);
        inputPin = null;
        if (hr != 0)
        {
          this.LogDebug("MD Plugin: failed to connect MD plugin filter {0} into the graph, hr = 0x{1:x} ({2})", i + 1, hr, HResult.GetDXErrorString(hr));
          return false;
        }
        lastFilter = slot.Filter;
        _slots.Add(slot);

        // Check whether the plugin supports extended capabilities.
        IChangeChannel temp = slot.Filter as IChangeChannel;
        try
        {
          temp.SetPluginsDirectory(_configurationFolderPrefix + i);
          this.LogDebug("MD Plugin: plugins directory is \"{0}{1}\"", _configurationFolderPrefix + i);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "MD Plugin: failed to set plugin directory");
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
    public bool OpenInterface()
    {
      this.LogDebug("MD Plugin: open conditional access interface");

      if (!_isMdPlugin)
      {
        this.LogDebug("MD Plugin: device not initialised or interface not supported");
        return false;
      }
      if (_slots == null || _slots.Count == 0)
      {
        this.LogDebug("MD Plugin: device filter(s) not added to the BDA filter graph");
        return false;
      }
      if (_programmeBuffer != IntPtr.Zero)
      {
        this.LogDebug("MD Plugin: interface is already open");
        return false;
      }

      _programmeBuffer = Marshal.AllocCoTaskMem(Program82Size);
      _pidBuffer = Marshal.AllocCoTaskMem(PidsToDecodeSize);

      this.LogDebug("MD Plugin: result = success");
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseInterface()
    {
      this.LogDebug("MD Plugin: close conditional access interface");

      if (_programmeBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_programmeBuffer);
        _programmeBuffer = IntPtr.Zero;
      }
      if (_pidBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_pidBuffer);
        _pidBuffer = IntPtr.Zero;
      }

      this.LogDebug("MD Plugin: result = true");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    public bool ResetInterface(out bool rebuildGraph)
    {
      this.LogDebug("MD Plugin: reset conditional access interface");

      // We have to rebuild the graph to reset anything.
      rebuildGraph = true;
      return true;
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    public bool IsInterfaceReady()
    {
      this.LogDebug("MD Plugin: is conditional access interface ready");

      if (!_isMdPlugin || _slots == null)
      {
        this.LogDebug("MD Plugin: device not initialised or interface not supported");
        return false;
      }
      if (_programmeBuffer == IntPtr.Zero)
      {
        this.LogDebug("MD Plugin: interface not opened");
        return false;
      }

      // As far as we know, the interface is always ready as long as it is open.
      this.LogDebug("MD Plugin: result = {0}", _slots.Count > 0);
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
    /// <param name="pmt">The programme map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    public bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat)
    {
      this.LogDebug("MD Plugin: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isMdPlugin || _slots == null)
      {
        this.LogDebug("MD Plugin: device not initialised or interface not supported");
        return true;  // Don't retry...
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        this.LogDebug("MD Plugin: command type {0} is not supported", command);
        return false;
      }
      if (pmt == null)
      {
        this.LogDebug("MD Plugin: PMT not supplied");
        return true;
      }
      if (cat == null)
      {
        this.LogDebug("MD Plugin: CAT not supplied");
        return true;
      }
      DVBBaseChannel dvbChannel = channel as DVBBaseChannel;
      if (dvbChannel == null)
      {
        this.LogDebug("MD Plugin: channel is not a DVB channel");
        return true;
      }
      if (_providers.Count != 0 && !_providers.Contains(dvbChannel.Provider))
      {
        this.LogDebug("MD Plugin: plugin not configured to decrypt services for provider \"{0}\"", dvbChannel.Provider);
        return false;
      }

      // Find a free slot to decode this service. If this is the first or only service in the list then we
      // can reset our slots. This could be optimised to cache the new list until "last"/"only" action and
      // reuse slots where possible.
      DecodeSlot freeSlot = null;
      if (command == CaPmtCommand.OkDescrambling && (listAction == CaPmtListManagementAction.First || listAction == CaPmtListManagementAction.Only))
      {
        this.LogDebug("MD Plugin: freeing all slots");
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
          DVBBaseChannel currentService = slot.CurrentChannel as DVBBaseChannel;
          if (currentService != null && currentService.ServiceId == dvbChannel.ServiceId)
          {
            // "Not selected" means stop decrypting the service.
            if (command == CaPmtCommand.NotSelected)
            {
              this.LogDebug("MD Plugin: found existing slot decrypting channel \"{0}\", freeing", currentService.Name);
              slot.CurrentChannel = null;
              IChangeChannel_Clear clear = slot.Filter as IChangeChannel_Clear;
              if (clear != null)
              {
                clear.ClearChannel();
              }
              return true;
            }
            // "Ok descrambling" means start or continue decrypting the service. If we're already decrypting
            // the service that is fine - this is an update.
            else if (command == CaPmtCommand.OkDescrambling)
            {
              this.LogDebug("MD Plugin: found existing slot decrypting channel \"{0}\", sending update", currentService.Name);
              freeSlot = slot;
              // No need to continue looping - this is the optimal situation where we reuse the existing slot.
              break;
            }
          }
          else if (currentService == null)
          {
            this.LogDebug("  <free slot>...");
            if (freeSlot == null)
            {
              freeSlot = slot;
            }
          }
          else if (currentService != null)
          {
            this.LogDebug("  \"{0}\"...", currentService.Name);
          }
        }
      }

      if (command == CaPmtCommand.NotSelected)
      {
        // If we get to here then we were asked to stop decrypting
        // a service that we were not decrypting. Strange...
        this.LogDebug("MD Plugin: received \"not selected\" request for channel that is not being decrypted");
        return true;
      }

      if (freeSlot == null)
      {
        // If we get to here then we were asked to start decrypting
        // a service, but we don't have any free slots to do it with.
        this.LogDebug("MD Plugin: no free decrypt slots");
        return false;
      }

      // If we get to here then we need to try to start decrypting the service.
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
      programToDecode.TransportStreamId = (UInt16)dvbChannel.TransportId;
      programToDecode.PmtPid = (UInt16)dvbChannel.PmtPid;

      // We don't know what the actual service type is in this
      // context, but we can at least indicate whether this is
      // a TV or radio service.
      //programToDecode.ServiceType = (byte)(dvbChannel.IsTv ? DvbServiceType.DigitalTelevision : DvbServiceType.DigitalRadio); //TODO look if needed
      programToDecode.ServiceType = (byte)DvbServiceType.DigitalTelevision;

      this.LogDebug("MD Plugin: TSID = {0} (0x{0:x}), SID = {1} (0x{1:x}), PMT PID = {2} (0x{2:x}), PCR PID = {3} (0x{3:x}), service type = {4}, " +
                        "video PID = {5} (0x{5:x}), audio PID = {6} (0x{6:x}), AC3 PID = {7} (0x{7:x}), teletext PID = {8} (0x{8:x})",
          programToDecode.TransportStreamId, programToDecode.ServiceId, programToDecode.PmtPid, programToDecode.PcrPid,
          programToDecode.ServiceType, programToDecode.VideoPid, programToDecode.AudioPid, programToDecode.Ac3AudioPid, programToDecode.TeletextPid
      );

      programToDecode.CaSystemCount = RegisterEcmAndEmmPids(pmt, cat, ref programToDecode);
      SetPreferredCaSystemIndex(ref programToDecode);

      this.LogDebug("MD Plugin: ECM PID = {0} (0x{0:x}), CA system count = {1}, CA index = {2}",
                    programToDecode.EcmPid, programToDecode.CaSystemCount, programToDecode.CaId
      );
      for (byte i = 0; i < programToDecode.CaSystemCount; i++)
      {
        this.LogDebug("MD Plugin: #{0} CA type = {1} (0x{1:x}), ECM PID = {2} (0x{2:x}), EMM PID = {3} (0x{3:x}), provider = {4} (0x{4:x})",
                      i + 1, programToDecode.CaSystems[i].CaType, programToDecode.CaSystems[i].EcmPid, programToDecode.CaSystems[i].EmmPid, programToDecode.CaSystems[i].ProviderId
        );
      }

      // Instruct the MD filter to decrypt the service.
      Marshal.StructureToPtr(programToDecode, _programmeBuffer, true);
      //DVB_MMI.DumpBinary(_programmeBuffer, 0, Program82Size);
      Marshal.StructureToPtr(pidsToDecode, _pidBuffer, true);
      //DVB_MMI.DumpBinary(_pidBuffer, 0, PidsToDecodeSize);
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
        freeSlot.CurrentChannel = channel;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MD Plugin: failed to change channel");
        return false;
      }

      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Close interfaces, free memory and release COM object references.
    /// </summary>
    public override void Dispose()
    {
      CloseInterface();

      if (_graph != null)
      {
        if (_infTee != null)
        {
          _graph.RemoveFilter(_infTee);
          DsUtils.ReleaseComObject(_infTee);
          _infTee = null;
        }

        foreach (DecodeSlot slot in _slots)
        {
          if (slot.Filter != null)
          {
            _graph.RemoveFilter(slot.Filter);
            DsUtils.ReleaseComObject(slot.Filter);
            slot.Filter = null;
          }
        }
        _slots = null;
        _graph = null;
      }

      _isMdPlugin = false;
    }

    #endregion
  }
}