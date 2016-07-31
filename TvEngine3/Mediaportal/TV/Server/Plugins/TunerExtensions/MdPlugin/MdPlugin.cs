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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MdPlugin
{
  /// <summary>
  /// A class for handling conditional access with softCAM plugins that support Agarwal's multi-decrypt
  /// plugin API.
  /// </summary>
  public class MdPlugin : BaseTunerExtension, IConditionalAccessProvider, IDirectShowAddOnDevice, IDisposable
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
      /// Instruct the softCAM plugin filter to decrypt a different program using specific parameters.
      /// </summary>
      [PreserveSig]
      int ChangeChannel(int frequency, int bandwidth, int polarity, int videoPid, int audioPid, int ecmPid, int caId, int providerId);

      /// <summary>
      /// Instruct the softCAM plugin filter to decrypt a different program using a Program82 structure.
      /// </summary>
      [PreserveSig]
      int ChangeChannelTP82([In] ref Program82 program82);

      /// <summary>
      /// Set the plugin directory.
      /// </summary>
      [PreserveSig]
      int SetPluginsDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);
    }

    [Guid("e98b70ee-f5a1-4f46-b8b8-a1324ba92f5f"),
      InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IChangeChannel_Ex
    {
      /// <summary>
      /// Instruct the softCAM plugin filter to decrypt a different program using Program82 and PidsToDecrypt structures.
      /// </summary>
      [PreserveSig]
      int ChangeChannelTP82_Ex([In] ref Program82 program82, [In] ref PidsToDecrypt pidsToDecrypt);
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
      public byte Polarisation;
      public byte Voltage;
      public byte Afc;          // automatic frequency control
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

      public byte ServiceType;  // DVB encoding
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
    private struct PidsToDecrypt  // TPids2Dec
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_PID_COUNT)]
      public ushort[] Pids;
      public ushort PidCount;
    }

    private class DecryptSlot
    {
      public IBaseFilter Filter;
      public ushort ProgramNumber;
      public PendingChannel PendingChannel;
    }

    private class PendingChannel
    {
      public TableProgramMap Pmt;
      public TableConditionalAccess Cat;

      public PendingChannel(TableProgramMap pmt, TableConditionalAccess cat)
      {
        Pmt = pmt;
        Cat = cat;
      }
    }

    #endregion

    #region constants

    private static readonly int PROGRAM82_SIZE = Marshal.SizeOf(typeof(Program82));           // 804
    private const int MAX_CA_SYSTEM_COUNT = 32;
    private static readonly int PIDS_TO_DECRYPT_SIZE = Marshal.SizeOf(typeof(PidsToDecrypt));   // 128
    private const int MAX_PID_COUNT = 63;

    private static readonly string CONFIG_FOLDER = PathManager.BuildAssemblyRelativePath("MDPLUGINS");

    #endregion

    #region variables

    private bool _isMdPlugin = false;
    private string _tunerExternalId = null;
    private string _tunerName = null;
    private bool _isCaInterfaceOpen = false;

    private int _slotCount = 0;
    private List<DecryptSlot> _slots = null;
    private List<PendingChannel> _pendingNewChannels = null;
    private HashSet<string> _providers = new HashSet<string>();
    private string _pluginFolderPrefix = string.Empty;

    private IFilterGraph2 _graph = null;
    private IBaseFilter _upstreamFilter = null;

    #endregion

    /// <summary>
    /// Load the configuration for a tuner.
    /// </summary>
    /// <param name="tunerExternalId">The tuner's external identifier.</param>
    /// <param name="tunerName">The tuner's name.</param>
    /// <param name="slotCount">The number of decrypt slots that the tuner is configured to have.</param>
    /// <param name="providers">The set of provider names that the tuner is configured to be able to decrypt.</param>
    /// <param name="pluginFolderPrefix">The prefix for the tuner's decrypt plugin folders.</param>
    private void GetTunerConfig(string tunerExternalId, string tunerName, out int slotCount, out HashSet<string> providers, out string pluginFolderPrefix)
    {
      slotCount = 1;
      providers = new HashSet<string>();    // all
      pluginFolderPrefix = tunerName;

      try
      {
        string configFile = Path.Combine(CONFIG_FOLDER, "MDAPICards.xml");
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

          // The "name" attribute is used as the plugin folder prefix.
          XmlAttribute attr = doc.CreateAttribute("Name");
          attr.InnerText = tunerName;
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
          return;
        }

        try
        {
          pluginFolderPrefix = tunerNode.Attributes["Name"].Value;
          slotCount = Convert.ToInt32(tunerNode.Attributes["EnableMdapi"].Value);
        }
        catch
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
          string providerList = tunerNode.Attributes["Provider"].Value.ToLowerInvariant();
          if (!providerList.Equals("all"))
          {
            providers = new HashSet<string>(Regex.Split(providerList.Trim(), @"\s*,\s*"));
          }
        }
        catch
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
      catch (Exception ex)
      {
        this.LogWarn(ex, "MD plugin: failed to create, load or read tuner configuration");
        slotCount = 1;
        providers = new HashSet<string>();
        pluginFolderPrefix = tunerName;
      }
    }

    /// <summary>
    /// Send a decrypt command.
    /// </summary>
    /// <param name="slotFilter">The slot to use.</param>
    /// <param name="pmt">The program's PMT.</param>
    /// <param name="cat">The program's CAT.</param>
    /// <returns><c>true</c> if the decrypt command is sent successfully, otherwise <c>false</c></returns>
    private bool DecryptProgram(IBaseFilter slotFilter, TableProgramMap pmt, TableConditionalAccess cat)
    {
      Program82 programToDecrypt;
      PidsToDecrypt pidsToDecrypt;
      RegisterVideoAndAudioPids(pmt, out programToDecrypt, out pidsToDecrypt);

      // Set the fields that we are able to set.
      programToDecrypt.Name = string.Empty;
      programToDecrypt.Provider = string.Empty;

      // We don't know what the actual service type is in this
      // context, but we can at least indicate whether this is
      // a TV or radio service.
      if (programToDecrypt.VideoPid != 0)
      {
        programToDecrypt.ServiceType = 1;   // digital television
      }
      else
      {
        programToDecrypt.ServiceType = 2;   // digital radio
      }

      this.LogDebug("MD plugin: service ID = {0}, PCR PID = {1}, service type = {2}, video PID = {3}, audio PID = {4}, AC3 PID = {5}, teletext PID = {6}",
                    programToDecrypt.ServiceId, programToDecrypt.PcrPid, programToDecrypt.ServiceType,
                    programToDecrypt.VideoPid, programToDecrypt.AudioPid, programToDecrypt.Ac3AudioPid,
                    programToDecrypt.TeletextPid);

      programToDecrypt.CaSystemCount = RegisterEcmAndEmmPids(pmt, cat, ref programToDecrypt);
      SetPreferredCaSystemIndex(ref programToDecrypt);

      this.LogDebug("MD plugin: ECM PID = {0}, CA system count = {1}, CA index = {2}",
                    programToDecrypt.EcmPid, programToDecrypt.CaSystemCount, programToDecrypt.CaId);
      for (byte i = 0; i < programToDecrypt.CaSystemCount; i++)
      {
        CaSystem82 caSystem = programToDecrypt.CaSystems[i];
        this.LogDebug("MD plugin: #{0} CA type = 0x{1:x4}, ECM PID = {2}, EMM PID = {3}, provider = 0x{4:x4}",
                      i + 1, caSystem.CaType, caSystem.EcmPid, caSystem.EmmPid, caSystem.ProviderId);
      }

      // Instruct the MD filter to decrypt the program.
      try
      {
        IChangeChannel_Ex changeEx = slotFilter as IChangeChannel_Ex;
        if (changeEx != null)
        {
          changeEx.ChangeChannelTP82_Ex(ref programToDecrypt, ref pidsToDecrypt);
        }
        else
        {
          IChangeChannel change = slotFilter as IChangeChannel;
          if (change != null)
          {
            change.ChangeChannelTP82(ref programToDecrypt);
          }
          else
          {
            throw new Exception("Failed to acquire interface on filter.");
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "MD plugin: failed to change channel");
        return false;
      }
    }

    /// <summary>
    /// Find the video and audio elementary stream PIDs in a program map and record them in MD API structures.
    /// </summary>
    /// <param name="pmt">The program map to read from.</param>
    /// <param name="programToDecrypt">The MD API program information structure.</param>
    /// <param name="pidsToDecrypt">The MD API PID list structure.</param>
    private void RegisterVideoAndAudioPids(TableProgramMap pmt, out Program82 programToDecrypt, out PidsToDecrypt pidsToDecrypt)
    {
      this.LogDebug("MD plugin: registering video and audio PIDs");
      programToDecrypt = new Program82();
      pidsToDecrypt = new PidsToDecrypt();
      pidsToDecrypt.Pids = new ushort[MAX_PID_COUNT];

      programToDecrypt.ServiceId = pmt.ProgramNumber;
      programToDecrypt.PcrPid = pmt.PcrPid;

      foreach (PmtElementaryStream es in pmt.ElementaryStreams)
      {
        bool isVideoStream = StreamTypeHelper.IsVideoStream(es.LogicalStreamType);

        // When using a plugin with extended support, we can just
        // specify the PIDs that need to be decrypted.
        if (pidsToDecrypt.PidCount < MAX_PID_COUNT)
        {
          if (
            isVideoStream ||
            StreamTypeHelper.IsAudioStream(es.LogicalStreamType) ||
            es.LogicalStreamType == LogicalStreamType.Subtitles ||
            es.LogicalStreamType == LogicalStreamType.Teletext
          )
          {
            pidsToDecrypt.Pids[pidsToDecrypt.PidCount++] = es.Pid;
          }
        }
        else
        {
          this.LogError("MD plugin: unable to register all PIDs");
          break;
        }

        // Otherwise, we have to fill the specific PID fields in the
        // Program82 structure as best as we can. We'll keep the first
        // of each type of PID.
        if (programToDecrypt.VideoPid == 0 && isVideoStream)
        {
          programToDecrypt.VideoPid = es.Pid;
        }
        else if (programToDecrypt.TeletextPid == 0 && es.LogicalStreamType == LogicalStreamType.Teletext)
        {
          programToDecrypt.TeletextPid = es.Pid;
        }
        else
        {
          if (es.LogicalStreamType == LogicalStreamType.AudioAc3 || es.LogicalStreamType == LogicalStreamType.AudioEnhancedAc3)
          {
            if (programToDecrypt.Ac3AudioPid == 0)
            {
              programToDecrypt.Ac3AudioPid = es.Pid;
            }
          }
          else
          {
            if (programToDecrypt.AudioPid == 0 && StreamTypeHelper.IsAudioStream(es.LogicalStreamType))
            {
              programToDecrypt.AudioPid = es.Pid;
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
    /// <param name="programToDecrypt">The MD API program information structure.</param>
    /// <returns>the number of ECM/EMM PID combinations registered</returns>
    private ushort RegisterEcmAndEmmPids(TableProgramMap pmt, TableConditionalAccess cat, ref Program82 programToDecrypt)
    {
      this.LogDebug("MD plugin: registering ECM and EMM details");

      // Build a dictionary of CA system -> { ECM PID } -> { provider ID } from the PMT.
      Dictionary<ushort, Dictionary<ushort, HashSet<uint>>> seenEcmPids = new Dictionary<ushort, Dictionary<ushort, HashSet<uint>>>(10);

      // First get ECMs from the PMT program CA descriptors.
      this.LogDebug("MD plugin: reading PMT program CA descriptors...");
      IEnumerator<IDescriptor> descEn = pmt.ProgramCaDescriptors.GetEnumerator();
      while (descEn.MoveNext())
      {
        ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
        if (cad == null)
        {
          this.LogError("MD plugin: invalid PMT program CA descriptor");
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
            this.LogError("MD plugin: invalid PMT ES CA descriptor");
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
      Dictionary<ushort, Dictionary<ushort, HashSet<uint>>> seenEmmPids = new Dictionary<ushort, Dictionary<ushort, HashSet<uint>>>(10);
      this.LogDebug("MD plugin: reading CAT CA descriptors...");
      descEn = cat.CaDescriptors.GetEnumerator();
      while (descEn.MoveNext())
      {
        ConditionalAccessDescriptor cad = ConditionalAccessDescriptor.Decode(descEn.Current);
        if (cad == null)
        {
          this.LogError("MD plugin: invalid CAT CA descriptor");
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
      programToDecrypt.CaSystems = new CaSystem82[MAX_CA_SYSTEM_COUNT];
      IEnumerator<uint> en;
      this.LogDebug("MD plugin: registering PIDs...");
      foreach (KeyValuePair<ushort, Dictionary<ushort, HashSet<uint>>> caSystemEcmPids in seenEcmPids)
      {
        // Register each ECM PID and attempt to link it with an EMM PID.
        foreach (KeyValuePair<ushort, HashSet<uint>> ecmPidProviderIds in caSystemEcmPids.Value)
        {
          CaSystem82 caSystem = programToDecrypt.CaSystems[count];
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
            programToDecrypt.EcmPid = ecmPidProviderIds.Key;
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
          CaSystem82 caSystem = programToDecrypt.CaSystems[count];
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
          CaSystem82 caSystem = programToDecrypt.CaSystems[count];
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
    /// <param name="programToDecrypt">The MD API program information structure containing the options.</param>
    private void SetPreferredCaSystemIndex(ref Program82 programToDecrypt)
    {
      try
      {
        this.LogDebug("MD plugin: identifying primary ECM PID");

        // Load configuration (if we have any).
        string configFile = Path.Combine(CONFIG_FOLDER, "MDAPIProvID.xml");
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
            string transportStreamId = string.Format("{0:D}", programToDecrypt.TransportStreamId);
            string serviceId = string.Format("{0:D}", programToDecrypt.ServiceId);
            string pmtPid = string.Format("{0:D}", programToDecrypt.PmtPid);
            foreach (XmlNode channelNode in channelList)
            {
              if (channelNode.Attributes["tp_id"].Value.Equals(transportStreamId) &&
                  channelNode.Attributes["sid"].Value.Equals(serviceId) &&
                  channelNode.Attributes["pmt_pid"].Value.Equals(pmtPid))
              {
                this.LogDebug("MD plugin: found channel configuration");
                for (byte i = 0; i < programToDecrypt.CaSystemCount; i++)
                {
                  CaSystem82 caSystem = programToDecrypt.CaSystems[i];
                  string ecmPid = string.Format("{0:D}", caSystem.EcmPid);
                  if (channelNode.Attributes["ecm_pid"].Value.Equals(ecmPid))
                  {
                    this.LogDebug("MD plugin: found correct ECM PID");
                    programToDecrypt.CaId = i;
                    programToDecrypt.EcmPid = caSystem.EcmPid;
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
                for (byte i = 0; i < programToDecrypt.CaSystemCount; i++)
                {
                  CaSystem82 caSystem = programToDecrypt.CaSystems[i];
                  if (providerNode.Attributes["ID"].Value.Equals(string.Format("{0:D}", caSystem.ProviderId)))
                  {
                    this.LogDebug("MD plugin: found provider configuration");
                    programToDecrypt.CaId = i;
                    programToDecrypt.EcmPid = caSystem.EcmPid;
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
                for (byte i = 0; i < programToDecrypt.CaSystemCount; i++)
                {
                  CaSystem82 caSystem = programToDecrypt.CaSystems[i];
                  if (caTypeNode.Attributes["ID"].Value.Equals(string.Format("{0:D}", caSystem.CaType)))
                  {
                    this.LogDebug("MD plugin: found CA type configuration");
                    programToDecrypt.CaId = i;
                    programToDecrypt.EcmPid = caSystem.EcmPid;
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
            bool.TryParse(mainNode.Attributes["fillout"].Value, out fillOutConfig);
          }

          if (fillOutConfig)
          {
            this.LogDebug("MD plugin: attempting to add entries to MDAPIProvID.xml");

            // Channel configuration stub.
            XmlNode node = doc.CreateElement("channel");
            XmlAttribute attribute = doc.CreateAttribute("tp_id");
            attribute.Value = programToDecrypt.TransportStreamId.ToString();
            node.Attributes.Append(attribute);
            attribute = doc.CreateAttribute("sid");
            attribute.Value = programToDecrypt.ServiceId.ToString();
            node.Attributes.Append(attribute);
            attribute = doc.CreateAttribute("pmt_pid");
            attribute.Value = programToDecrypt.PmtPid.ToString();
            node.Attributes.Append(attribute);
            attribute = doc.CreateAttribute("ecm_pid");
            attribute.Value = programToDecrypt.EcmPid.ToString();
            node.Attributes.Append(attribute);

            string comment = "Channel \"" + programToDecrypt.Name + "\", possible ECM PID values = {{";
            for (byte i = 0; i < programToDecrypt.CaSystemCount; i++)
            {
              if (programToDecrypt.CaSystems[i].EcmPid == 0)
              {
                continue;
              }
              if (i != 0)
              {
                comment += ", ";
              }
              comment += programToDecrypt.CaSystems[i].EcmPid;
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
            for (byte i = 0; i < programToDecrypt.CaSystemCount; i++)
            {
              uint providerId = programToDecrypt.CaSystems[i].ProviderId;
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
            for (byte i = 0; i < programToDecrypt.CaSystemCount; i++)
            {
              uint caType = programToDecrypt.CaSystems[i].CaType;
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

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
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
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "MD plugin";
      }
    }

    /// <summary>
    /// Does the extension control some part of the tuner hardware?
    /// </summary>
    public override bool ControlsTunerHardware
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
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

      if (!Directory.Exists(CONFIG_FOLDER))
      {
        this.LogDebug("MD plugin: plugin is not configured");
        return false;
      }

      try
      {
        MDAPIFilter mdFilter = new MDAPIFilter();
        if (mdFilter == null)
        {
          throw new NullReferenceException();
        }
        Release.ComObject("MD plugin test filter", ref mdFilter);
      }
      catch
      {
        this.LogDebug("MD plugin: filter class is not registered");
        return false;
      }

      // Get the tuner filter name. We use it as a prefix for the tuner's plugin folder(s).
      FilterInfo tunerFilterInfo;
      int hr = tunerFilter.QueryFilterInfo(out tunerFilterInfo);
      _tunerName = tunerFilterInfo.achName;
      Release.FilterInfo(ref tunerFilterInfo);
      if (hr != (int)NativeMethods.HResult.S_OK || string.IsNullOrEmpty(_tunerName))
      {
        this.LogError("MD plugin: failed to get the tuner filter name, hr = 0x{0:x}", hr);
        return false;
      }

      this.LogInfo("MD plugin: extension supported");
      _tunerExternalId = tunerExternalId;
      GetTunerConfig(_tunerExternalId, _tunerName, out _slotCount, out _providers, out _pluginFolderPrefix);
      _isMdPlugin = true;
      _slots = new List<DecryptSlot>(_slotCount);
      this.LogDebug("  slot count           = {0}", _slotCount);
      this.LogDebug("  provider(s)          = [{0}]", string.Join(", ", _providers));
      this.LogDebug("  plugin folder prefix = {0}", _pluginFolderPrefix);
      return true;
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

      // Add one filter for each decrypt slot.
      this.LogDebug("MD plugin: adding {0} decrypting filter(s)", _slotCount);
      _graph = graph;
      _upstreamFilter = lastFilter;
      int hr;
      IPin inputPin;
      IPin outputPin;
      for (int i = 0; i < _slotCount; i++)
      {
        DecryptSlot slot = new DecryptSlot();
        try
        {
          slot.Filter = (IBaseFilter)new MDAPIFilter();
        }
        catch
        {
          this.LogError("MD plugin: failed to instance MP plugin filter {0}", i + 1);
          return false;
        }
        slot.ProgramNumber = 0;
        slot.PendingChannel = null;

        // Add the filter to the graph.
        hr = _graph.AddFilter(slot.Filter, "MDAPI Filter " + i);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("MD plugin: failed to add MD plugin filter {0} to the graph, hr = 0x{1:x}", i + 1, hr);
          return false;
        }

        // Connect the filter into the graph.
        outputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
        inputPin = DsFindPin.ByDirection(slot.Filter, PinDirection.Input, 0);
        hr = _graph.ConnectDirect(outputPin, inputPin, null);
        Release.ComObject("MD plugin source filter output pin", ref outputPin);
        Release.ComObject("MD plugin MDAPI filter input pin", ref inputPin);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("MD plugin: failed to connect MD plugin filter {0} into the graph, hr = 0x{1:x}", i + 1, hr);
          return false;
        }
        lastFilter = slot.Filter;
        _slots.Add(slot);

        // Check whether the plugin supports extended capabilities.
        IChangeChannel temp = slot.Filter as IChangeChannel;
        try
        {
          temp.SetPluginsDirectory(_pluginFolderPrefix + i);
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
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      this.LogDebug("MD plugin: open conditional access interface");

      if (!_isMdPlugin)
      {
        this.LogWarn("MD plugin: not initialised or interface not supported");
        return false;
      }
      if (_slots == null || (_slots.Count == 0 && _slotCount != 0))
      {
        this.LogError("MD plugin: failed to open conditional access interface, filter(s) not added to the BDA filter graph");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("MD plugin: conditional access interface is already open");
        return true;
      }

      _isCaInterfaceOpen = true;

      this.LogDebug("MD plugin: result = success");
      return true;
    }

    /// <summary>
    /// Determine if the conditional access interface is open.
    /// </summary>
    /// <value><c>true</c> if the conditional access interface is open, otherwise <c>false</c></value>
    bool IConditionalAccessProvider.IsOpen
    {
      get
      {
        return _isCaInterfaceOpen;
      }
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Close()
    {
      return CloseConditionalAccessInterface(true);
    }

    private bool CloseConditionalAccessInterface(bool isDisposing)
    {
      this.LogDebug("MD plugin: close conditional access interface");

      if (isDisposing && _slots != null)
      {
        foreach (DecryptSlot slot in _slots)
        {
          if (slot.ProgramNumber != 0)
          {
            slot.ProgramNumber = 0;
            IChangeChannel_Clear clear = slot.Filter as IChangeChannel_Clear;
            if (clear != null)
            {
              clear.ClearChannel();
            }
          }
          slot.PendingChannel = null;
        }
      }
      _isCaInterfaceOpen = false;

      this.LogDebug("MD plugin: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      this.LogDebug("MD plugin: reset conditional access interface");

      if (!_isMdPlugin)
      {
        this.LogWarn("MD plugin: not initialised or interface not supported");
        return false;
      }
      if (!_isCaInterfaceOpen)
      {
        this.LogError("MD plugin: failed to reset conditional access interface, interface is not open");
        return false;
      }

      // Get a reference to the upstream filter that the first MD filter is
      // connected to.
      IBaseFilter upstreamFilter = _upstreamFilter;
      int hr;
      IPin connectedPin = null;
      if (_slots.Count != 0)
      {
        this.LogDebug("MD plugin: get upstream connection");
        IPin firstFilterInputPin = DsFindPin.ByDirection(_slots[0].Filter, PinDirection.Input, 0);
        if (firstFilterInputPin == null)
        {
          this.LogError("MD plugin: failed to get the first MD filter input pin");
          return false;
        }

        try
        {
          hr = firstFilterInputPin.ConnectedTo(out connectedPin);
          if (hr != (int)NativeMethods.HResult.S_OK || connectedPin == null)
          {
            this.LogError("MD plugin: failed to get the pin connected to the first MD filter input, hr = 0x{0:x}", hr);
            return false;
          }
        }
        finally
        {
          Release.ComObject("MD plugin first filter input pin", ref firstFilterInputPin);
        }

        try
        {
          PinInfo pinInfo;
          hr = connectedPin.QueryPinInfo(out pinInfo);
          if (hr != (int)NativeMethods.HResult.S_OK || pinInfo.filter == null)
          {
            this.LogError("MD plugin: failed to get the filter connected to the first MD filter input, hr = 0x{0:x}", hr);
            return false;
          }
          upstreamFilter = pinInfo.filter;
        }
        finally
        {
          Release.ComObject("MD plugin first filter input connected pin", ref connectedPin);
        }
      }

      // Get a reference to the input pin that the last MD filter is connected
      // to.
      this.LogDebug("MD plugin: get downstream connection");
      try
      {
        IBaseFilter lastMdFilter = _upstreamFilter;
        if (_slotCount != 0)
        {
          lastMdFilter = _slots[_slots.Count - 1].Filter;
        }

        IPin lastFilterOutputPin = DsFindPin.ByDirection(lastMdFilter, PinDirection.Output, 0);
        if (lastFilterOutputPin == null)
        {
          this.LogError("MD plugin: failed to get the last MD filter output pin");
          return false;
        }

        try
        {
          hr = lastFilterOutputPin.ConnectedTo(out connectedPin);
          if (hr != (int)NativeMethods.HResult.S_OK || connectedPin == null)
          {
            this.LogError("MD plugin: failed to get the pin connected to the last MD filter output, hr = 0x{0:x}", hr);
            return false;
          }
        }
        finally
        {
          Release.ComObject("MD plugin last filter output pin", ref lastFilterOutputPin);
        }

        try
        {
          if (_slots.Count > 0)
          {
            // Close the interface.
            (this as IConditionalAccessProvider).Close();
            this.LogDebug("MD plugin: release filter(s)");
            foreach (DecryptSlot slot in _slots)
            {
              _graph.RemoveFilter(slot.Filter);
              Release.ComObject("MD plugin filter", ref slot.Filter);
            }
            _slots.Clear();
          }

          // Reload configuration.
          int previousSlotCount = _slotCount;
          GetTunerConfig(_tunerExternalId, _tunerName, out _slotCount, out _providers, out _pluginFolderPrefix);

          // Re-add filters.
          if (previousSlotCount == 0)
          {
            if (_slotCount == 0)
            {
              this.LogDebug("MD plugin: nothing to do");
              return true;
            }
            _graph.Disconnect(connectedPin);
          }
          IBaseFilter lastFilter = upstreamFilter;
          if (!AddToGraph(_graph, ref lastFilter))
          {
            return false;
          }

          // Reconnect the downstream chain.
          this.LogDebug("MD plugin: reconnect downstream filter chain");
          lastFilterOutputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
          if (lastFilterOutputPin == null)
          {
            this.LogError("MD plugin: failed to get the new last filter output pin");
            return false;
          }

          try
          {
            hr = _graph.ConnectDirect(lastFilterOutputPin, connectedPin, null);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              this.LogError("MD plugin: failed to reconnect downstream filter chain, hr = 0x{0:x}", hr);
              return false;
            }
            return (this as IConditionalAccessProvider).Open();
          }
          finally
          {
            Release.ComObject("MD plugin new last filter output pin", ref lastFilterOutputPin);
          }
        }
        finally
        {
          Release.ComObject("MD plugin last filter output connected pin", ref connectedPin);
        }
      }
      finally
      {
        if (upstreamFilter != _upstreamFilter)
        {
          Release.ComObject("MD plugin upstream filter", ref upstreamFilter);
        }
      }
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
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
    /// Determine whether the conditional access interface requires access to
    /// the MPEG 2 conditional access table in order to successfully decrypt
    /// programs.
    /// </summary>
    /// <value><c>true</c> if access to the MPEG 2 conditional access table is required in order to successfully decrypt programs, otherwise <c>false</c></value>
    bool IConditionalAccessProvider.IsConditionalAccessTableRequiredForDecryption
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more programs
    ///   simultaneously. This parameter gives the interface an indication of the number of programs that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program's map table.</param>
    /// <param name="cat">The conditional access table for the program's transport stream.</param>
    /// <param name="programProvider">The program's provider.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.SendCommand(CaPmtListManagementAction listAction, CaPmtCommand command, TableProgramMap pmt, TableConditionalAccess cat, string programProvider)
    {
      this.LogDebug("MD plugin: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("MD plugin: not initialised or interface not supported");
        return true;
      }
      if (command == CaPmtCommand.OkMmi)
      {
        this.LogError("MD plugin: conditional access command type {0} is not supported", command);
        return true;
      }
      if (command != CaPmtCommand.Query)
      {
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
      }

      string lowerProvider = programProvider == null ? string.Empty : programProvider.ToLowerInvariant();
      if (_providers.Count != 0 && !_providers.Contains(lowerProvider))
      {
        this.LogWarn("MD plugin: plugin not configured to decrypt programs for provider \"{0}\"", programProvider ?? "[null]");
        return false;
      }

      // If we get to here then the plugin is configured to decrypt the program
      // provider.
      if (command == CaPmtCommand.Query)
      {
        this.LogDebug("MD plugin: result = success");
        return true;
      }

      // Start of a program list...
      if (command == CaPmtCommand.OkDescrambling && (listAction == CaPmtListManagementAction.First || listAction == CaPmtListManagementAction.Only))
      {
        this.LogDebug("MD plugin: reset pending channel for all slots");
        foreach (DecryptSlot slot in _slots)
        {
          slot.PendingChannel = null;
        }
        _pendingNewChannels = new List<PendingChannel>(10);
      }

      // This loop handles add, update and remove actions immediately. List
      // actions are deferred until the last action is received to minimise
      // glitches.
      foreach (DecryptSlot slot in _slots)
      {
        if (slot.ProgramNumber == 0)
        {
          if (listAction == CaPmtListManagementAction.Add)
          {
            this.LogDebug("MD plugin: found free slot to decrypt program {0}", pmt.ProgramNumber);
            slot.ProgramNumber = pmt.ProgramNumber;
            if (DecryptProgram(slot.Filter, pmt, cat))
            {
              this.LogDebug("MD plugin: result = success");
              return true;
            }
            return false;
          }
        }
        else if (slot.ProgramNumber == pmt.ProgramNumber)
        {
          // "Not selected" means stop decrypting the program.
          if (command == CaPmtCommand.NotSelected)
          {
            this.LogDebug("MD plugin: found existing slot decrypting program {0}, freeing", slot.ProgramNumber);
            slot.ProgramNumber = 0;
            IChangeChannel_Clear clear = slot.Filter as IChangeChannel_Clear;
            if (clear != null)
            {
              clear.ClearChannel();
            }
            this.LogDebug("MD plugin: result = success");
            return true;
          }
          // "Ok descrambling" means start or continue decrypting the program.
          // If we're already decrypting the program that is fine - this is an
          // update.
          else if (command == CaPmtCommand.OkDescrambling)
          {
            if (listAction == CaPmtListManagementAction.Update)
            {
              this.LogDebug("MD plugin: found existing slot decrypting program {0}, updating", slot.ProgramNumber);
              if (DecryptProgram(slot.Filter, pmt, cat))
              {
                this.LogDebug("MD plugin: result = success");
                return true;
              }
              return false;
            }
            slot.PendingChannel = new PendingChannel(pmt, cat);

            // No need to continue looping - this is the optimal situation
            // where we reuse the existing slot.
            if (listAction == CaPmtListManagementAction.First || listAction == CaPmtListManagementAction.More)
            {
              this.LogDebug("MD plugin: result = success");
              return true;
            }
            else
            {
              break;
            }
          }
        }
      }

      if (command == CaPmtCommand.NotSelected)
      {
        this.LogError("MD plugin: failed to send conditional access unselect command, not currently decrypting program {0}", pmt.ProgramNumber);
        return true;
      }
      else if (listAction == CaPmtListManagementAction.Add)
      {
        this.LogError("MD plugin: failed to send conditional access add command, no free slots to decrypt program {0}", pmt.ProgramNumber);
        return false;
      }
      else if (listAction == CaPmtListManagementAction.Update)
      {
        this.LogError("MD plugin: failed to send conditional access update command, not currently decrypting program {0}", pmt.ProgramNumber);
        return false;
      }

      if (listAction == CaPmtListManagementAction.First || listAction == CaPmtListManagementAction.More)
      {
        _pendingNewChannels.Add(new PendingChannel(pmt, cat));
        this.LogDebug("MD plugin: result = success");
        return true;
      }

      // This loop handles list actions when the last action is received.
      bool updateAll = false;
      if (_pendingNewChannels.Count > 0)
      {
        this.LogDebug("MD plugin: update all slots...");
        updateAll = true;
      }
      foreach (DecryptSlot slot in _slots)
      {
        if (slot.PendingChannel == null)
        {
          // Free slot. Do we have any use for it?
          if (_pendingNewChannels.Count > 0)
          {
            PendingChannel pendingChannel = _pendingNewChannels[0];
            this.LogDebug("MD plugin: found free slot to decrypt program {0}", pendingChannel.Pmt.ProgramNumber);
            if (!DecryptProgram(slot.Filter, pendingChannel.Pmt, pendingChannel.Cat))
            {
              return false;
            }
            slot.ProgramNumber = pendingChannel.Pmt.ProgramNumber;
            _pendingNewChannels.RemoveAt(0);
          }
          else if (slot.ProgramNumber != 0)
          {
            this.LogDebug("MD plugin: freeing unrequired slot decrypting program {0}", slot.ProgramNumber);
            slot.ProgramNumber = 0;
            IChangeChannel_Clear clear = slot.Filter as IChangeChannel_Clear;
            if (clear != null)
            {
              clear.ClearChannel();
            }
          }
        }
        else if (updateAll && !DecryptProgram(slot.Filter, slot.PendingChannel.Pmt, slot.PendingChannel.Cat))
        {
          return false;
        }
      }

      if (_pendingNewChannels.Count > 0)
      {
        this.LogError("MD plugin: failed to send conditional access command, no free slots to decrypt {0} program(s)", _pendingNewChannels.Count);
        return false;
      }
      this.LogDebug("MD plugin: result = success");
      return true;
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

    ~MdPlugin()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isMdPlugin)
      {
        CloseConditionalAccessInterface(isDisposing);
      }

      if (isDisposing)
      {
        if (_graph != null)
        {
          if (_slots != null)
          {
            foreach (DecryptSlot slot in _slots)
            {
              _graph.RemoveFilter(slot.Filter);
            }
          }

          _graph = null;
        }

        if (_slots != null)
        {
          foreach (DecryptSlot slot in _slots)
          {
            Release.ComObject("MD plugin MDAPI filter", ref slot.Filter);
          }
        }
        _slots = null;
      }

      _upstreamFilter = null;
      _isMdPlugin = false;
    }

    #endregion
  }
}