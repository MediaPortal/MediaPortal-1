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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts
{
  /// <summary>
  /// A class that models the conditional access descriptor structure defined in ISO/IEC 13818-1.
  /// </summary>
  public class ConditionalAccessDescriptor : Descriptor
  {
    #region variables

    private ushort _caSystemId;
    private ushort _caPid;
    private byte[] _privateData;
    private Dictionary<ushort, HashSet<uint>> _pids;

    #endregion

    /// <summary>
    /// Constructor. Protected to ensure instances can only be created by derived classes or by calling
    /// Decode().
    /// </summary>
    /// <param name="descriptor">The base descriptor to use to instantiate this descriptor.</param>
    protected ConditionalAccessDescriptor(IDescriptor descriptor)
      : base(descriptor)
    {
    }

    #region properties

    /// <summary>
    /// The type of CA system applicable for the CA PID.
    /// </summary>
    public ushort CaSystemId
    {
      get
      {
        return _caSystemId;
      }
    }

    /// <summary>
    /// An encryption control or management message PID associated with the service to which this descriptor
    /// is attached.
    /// </summary>
    public ushort CaPid
    {
      get
      {
        return _caPid;
      }
    }

    /// <summary>
    /// The private conditional access system data.
    /// </summary>
    public ReadOnlyCollection<byte> PrivateData
    {
      get
      {
        return new ReadOnlyCollection<byte>(_privateData);
      }
    }

    /// <summary>
    /// A dictionary of ECM/EMM PIDs and their associated provider ID, interpretted from the private
    /// descriptor data.
    /// </summary>
    /// <remarks>
    /// This dictionary should be treated as read-only. Ideally we should enforce that, but there is currently no
    /// built in type for this.
    /// </remarks>
    public Dictionary<ushort, HashSet<uint>> Pids
    {
      get
      {
        return _pids;
      }
    }

    #endregion

    /// <summary>
    /// Attempt to decode an arbitrary descriptor as a conditional access descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor to decode.</param>
    /// <returns>a fully populated ConditionalAccessDescriptor instance if decoding is successful, otherwise <c>null</c></returns>
    public static ConditionalAccessDescriptor Decode(IDescriptor descriptor)
    {
      if (descriptor.Tag != DescriptorTag.ConditionalAccess || descriptor.Length < 4 ||
        descriptor.Data == null || descriptor.Data.Count < 4)
      {
        return null;
      }
      ConditionalAccessDescriptor d = new ConditionalAccessDescriptor(descriptor);

      // Standard fields.
      d._caSystemId = (ushort)((descriptor.Data[0] << 8) + descriptor.Data[1]);
      d._caPid = (ushort)(((descriptor.Data[2] & 0x1f) << 8) + descriptor.Data[3]);

      // Make our own copy of the private data.
      d._privateData = new byte[d._length - 4];
      Buffer.BlockCopy(d._data, 4, d._privateData, 0, d._length - 4);

      // Build a dictionary of PID info.
      uint providerId;
      d._pids = new Dictionary<ushort, HashSet<uint>>(5); // PID -> provider ID(s)
      d._pids.Add(d._caPid, new HashSet<uint>());

      // Canal Plus
      if ((d._caSystemId & 0xff00) == 0x100)
      {
        HandleCanalPlusDescriptor(d);
        return d;
      }
      // Nagra
      else if ((d._caSystemId & 0xff00) == 0x1800)
      {
        HandleNagraDescriptor(d);
        return d;
      }

      // Default - most other CA systems (eg. Irdeto) don't include private
      // data. Via Access (0x0500) does. We use this as the default handling.
      int offset = 0;
      while (offset + 1 < d._privateData.Length)
      {
        byte tagInd = d._privateData[offset++];
        byte tagLen = d._privateData[offset++];
        if (offset + tagLen <= d._privateData.Length)
        {
          if (tagInd == 0x14 && tagLen >= 2)  // Tag 0x14 is the Via Access provider ID.
          {
            providerId = (uint)((d._privateData[offset] << 16) + (d._privateData[offset + 1] << 8) +
                              d._privateData[offset + 2]);
            // Some providers (eg. Boxer) send wrong information in the lower 4
            // bits of the provider ID, so reset the lower 4 bits for Via Access.
            if (d._caSystemId == 0x500)
            {
              providerId = providerId & 0xfffffff0;
            }
            d._pids[d._caPid].Add(providerId);
          }
        }
        offset += tagLen;
      }

      return d;
    }

    #region proprietary descriptor format handling

    private static void HandleCanalPlusDescriptor(ConditionalAccessDescriptor d)
    {
      int offset = 0;
      ushort pid;
      uint providerId;

      // There are two formats...
      if (d._privateData.Length >= 3 && d._privateData[2] == 0xff)
      {
        // For this format, there is a loop of PID and provider ID info.
        #region examples
        //  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
        // 09 11 01 00 E6 43 00 6A FF 00 00 00 00 00 00 02 14 21 8C
        // 09 11 01 00 F6 BD 00 6D FF FF E0 00 00 00 00 00 00 2B 6D
        // 09 11 01 00 E6 1C 41 01 FF FF FF FF FF FF FF FF FF 21 8C
        // 09 2F 01 00 F6 A1 33 17 FF 40 20 0C 00 00 00 00 04 2C E3 F6 9F 33 11 FF 00 00 0C 00 08 00 00 02 2C E3 F6 A0 A8 21 FF 51 70 C1 03 00 00 01 4E 2C E3

        // 09       CA descriptor tag
        // 2F       total descriptor length (minus tag and length byte)
        // 01 00    CA system ID
        // f6 a1    PID
        // 33 17    provider ID
        // ff 40 20 0c 00 00 00 00 04 2c e3  (unknown)
        // f6 9f    PID
        // 33 11    provider ID
        // ff 00 00 0c 00 08 00 00 02 2c e3  (unknown)
        // f6 a0    PID
        // a8 21    provider ID
        // ff 51 70 c1 03 00 00 01 4e 2c e3  (unknown)
        #endregion

        // Handle the first provider ID "manually" - it is associated with the standardised PID.
        providerId = (uint)(((d._privateData[0] & 0x1f) << 8) + d._privateData[1]);
        d._pids[d._caPid].Add(providerId);

        offset = 13;
        while (offset + 3 < d._privateData.Length)
        {
          pid = (ushort)(((d._privateData[offset] & 0x1f) << 8) + d._privateData[offset + 1]);
          providerId = (uint)((d._privateData[offset + 2] << 8) + d._privateData[offset + 3]);

          HashSet<uint> pidProviders;
          if (d._pids.TryGetValue(pid, out pidProviders))
          {
            pidProviders.Add(providerId);
          }
          else
          {
            pidProviders = new HashSet<uint>() { providerId };
            d._pids.Add(pid, pidProviders);
          }
          offset += 15;
        }
      }
      else if (d._privateData.Length >= 5 && d._privateData[2] != 0xff)
      {
        // For this format, there are a variable number of PID and provider ID pairs.
        #region examples
        //  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18
        // 09 11 01 00 E0 C1 03 E0 92 41 01 E0 93 40 01 E0 C4 00 64
        // 09 0D 01 00 E0 B6 02 E0 B7 00 6A E0 B9 00 6C

        // 09       CA descriptor tag
        // 0d       total descriptor length (minus tag and length byte)
        // 01 00    CA system ID
        // e0 b6    PID
        // 02       "additional PID pair count" (not always accurate)
        //   e0 b7  PID
        //   00 6a  provider ID
        //   e0 b9  PID
        //   00 6c  provider ID
        #endregion

        int extraPidPairs = d._privateData[offset++];
        while (offset + 3 < d._privateData.Length)
        {
          pid = (ushort)(((d._privateData[offset] & 0x1f) << 8) + d._privateData[offset + 1]);
          providerId = (uint)((d._privateData[offset + 2] << 8) + d._privateData[offset + 3]);
          offset += 4;

          HashSet<uint> pidProviders;
          if (d._pids.TryGetValue(pid, out pidProviders))
          {
            pidProviders.Add(providerId);
          }
          else
          {
            pidProviders = new HashSet<uint> { providerId };
            d._pids.Add(pid, pidProviders);
          }
        }
      }
    }

    private static void HandleNagraDescriptor(ConditionalAccessDescriptor d)
    {
      if (d._privateData.Length == 0)
      {
        return;
      }

      // There are two formats...
      if (d._privateData[0] == 0x8c)
      {
        // For this format, the private data appears to be structured. I don't
        // know how to extract the provider ID(s).
        #region examples
        // http://forum.team-mediaportal.com/threads/multiseat-recording-stopped-when-recording-watching-other-channel.129335/#post-1130546
        // 09 6F 18 30 FC CC 8C 69 00 00 3D A3 0D 01 80 11 80 00 02 18 30 01 0F 9F 20 FF AA 15 02 80 11 00 00 00 64 00 03 03 21 00 00 00 02 18 30 00 00 00 64 AA 15 02 80 11 00 00 0B EA 00 03 03 21 00 00 00 02 18 30 00 00 0B EA 01 00 26 A3 0D 01 80 11 80 00 02 18 30 01 0D AB 20 FF AA 15 02 80 11 00 00 0B B8 00 03 03 21 00 00 00 02 18 43 00 00 0B B8
        // 09 7B 18 60 FE CC 8C 75 00 00 43 8C 13 00 80 11 08 02 18 60 20 0A 09 38 59 00 0F 9F 59 00 0F 9F AA 15 03 80 11 5D 00 00 68 00 03 03 21 00 00 02 18 60 5D 00 00 68 00 AA 15 03 80 11 00 00 0B EA 00 03 03 21 00 00 02 18 60 5D 00 0B EA 00 01 00 2C 8C 13 00 80 11 08 02 18 60 20 0A 09 38 59 00 0D 7A 59 00 0D 7A AA 15 03 80 11 5D 00 0B B8 00 03 03 21 00 00 02 18 60 5D 00 0B B8 00
        // 09 7B 18 6A FF CC 8C 75 00 00 43 8C 13 00 80 11 08 02 18 6A 20 0A 09 38 59 00 0F 9F 59 00 0F 9F AA 15 03 80 11 5D 00 00 6A 00 03 03 21 00 00 02 18 6A 5D 00 00 6A 00 AA 15 03 80 11 00 00 0B EA 00 03 03 21 00 00 02 18 6A 5D 00 0B EA 00 01 00 2C 8C 13 00 80 11 08 02 18 6A 20 0A 09 38 59 00 0D 7A 59 00 0D 7A AA 15 03 80 11 5D 00 0B B8 00 03 03 21 00 00 02 18 6A 5D 00 0B B8 00

        // 09       CA descriptor tag
        // 7b       total descriptor length (minus tag and length byte)
        // 18 6a    CA system ID
        // ff cc    PID
        // 8c       tag???
        // 75       length
        //   00 00  ???
        //     43   length
        //     8c   tag???
        //       13 length
        //       00 80 11 08 02 18 6a 20 0a 09 38 59 00 0f 9f 59 00 0f 9f
        //     aa
        //       15
        //       03 80 11 5d 00 00 6a 00 03 03 21 00 00 02 18 6a 5d 00 00 6a 00
        //     aa
        //       15
        //       03 80 11 00 00 0b ea 00 03 03 21 00 00 02 18 6a 5d 00 0b ea 00
        //   01 00
        //     2c
        //     8c
        //       13
        //       00 80 11 08 02 18 6a 20 0a 09 38 59 00 0d 7a 59 00 0d 7a
        //     aa
        //       15
        //       03 80 11 5d 00 0b b8 00 03 03 21 00 00 02 18 6a 5d 00 0b b8 00
        #endregion
      }
      else
      {
        #region examples
        //  0  1  2  3  4  5  6  7  8  9 10 11 12
        // 09 07 18 11 E2 BD 02 33 11
        // 09 09 18 63 E2 C7 04 33 42 33 43
        // 09 0B 18 63 E2 C8 06 33 41 33 42 33 43

        // 09       CA descriptor tag
        // 0B       total descriptor length (minus tag and length byte)
        // 18 63    CA system ID
        // e2 c8    PID
        // 06       number of bytes containing provider IDs
        //   33 41  provider ID
        //   33 42  provider ID
        //   33 43  provider ID
        #endregion

        int offset = 1;
        while (offset + 1 < d._privateData.Length)
        {
          uint providerId = (uint)((d._privateData[offset] << 8) + d._privateData[offset + 1]);
          d._pids[d._caPid].Add(providerId);
          offset += 2;
        }
      }
    }

    #endregion

    /// <summary>
    /// For debug use.
    /// </summary>
    public override void Dump()
    {
      this.LogDebug("CA Descriptor: dump...");
      this.LogDebug("  tag          = {0}", _tag);
      this.LogDebug("  length       = {0}", _length);
      this.LogDebug("  CA system ID = {0}", _caSystemId);
      this.LogDebug("  CA PID       = {0}", _caPid);
      foreach (KeyValuePair<ushort, HashSet<uint>> pid in _pids)
      {
        this.LogDebug("  PID = {0}, provider IDs = {1}", pid.Key, string.Join(", ", pid.Value));
      }
      Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(_data, _data.Length);
    }
  }
}