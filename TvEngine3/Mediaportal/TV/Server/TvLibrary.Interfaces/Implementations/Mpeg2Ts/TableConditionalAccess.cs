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
  /// A class that models the transport stream conditional access table section
  /// defined in ISO/IEC 13818-1.
  /// </summary>
  public class TableConditionalAccess
  {
    #region constants

    /// <summary>
    /// The maximum size of an ISO/IEC 13818-1 CAT section, in bytes.
    /// </summary>
    public const int MAX_SIZE = 4096;

    #endregion

    #region variables

    private byte _tableId;
    private bool _sectionSyntaxIndicator;
    private ushort _sectionLength;
    private byte _version;
    private bool _currentNextIndicator;
    private byte _sectionNumber;
    private byte _lastSectionNumber;
    private List<IDescriptor> _descriptors;
    private List<IDescriptor> _caDescriptors;
    private byte[] _crc;

    private byte[] _rawCat;

    #endregion

    // This class has a specific purpose - decoding CAT data. Although it may
    // be tempting, we want to prevent it being used for holding various other
    // info. Therefore the only way you can get an instance is by calling
    // Decode() with a valid CAT section.
    private TableConditionalAccess()
    {
    }

    #region properties

    /// <summary>
    /// The conditional access table identifier.
    /// </summary>
    /// <remarks>
    /// Expected to be <c>0x01</c>.
    /// </remarks>
    public byte TableId
    {
      get
      {
        return _tableId;
      }
    }

    /// <summary>
    /// The conditional access section syntax indicator.
    /// </summary>
    /// <remarks>
    /// Expected to be <c>true</c>.
    /// </remarks>
    public bool SectionSyntaxIndicator
    {
      get
      {
        return _sectionSyntaxIndicator;
      }
    }

    /// <summary>
    /// The length of the conditional access section.
    /// </summary>
    /// <remarks>
    /// Includes the CRC but not the table ID, section syntax indicator or
    /// section length bytes.
    /// </remarks>
    public ushort SectionLength
    {
      get
      {
        return _sectionLength;
      }
    }

    /// <summary>
    /// The version number of the conditional access table.
    /// </summary>
    public byte Version
    {
      get
      {
        return _version;
      }
    }

    /// <summary>
    /// When <c>true</c>, indicates that the condtional access table
    /// information is current. Otherwise, indicates that the information will
    /// apply in the future.
    /// </summary>
    public bool CurrentNextIndicator
    {
      get
      {
        return _currentNextIndicator;
      }
    }

    /// <summary>
    /// The index corresponding with this section of the conditional access
    /// table.
    /// </summary>
    public byte SectionNumber
    {
      get
      {
        return _sectionNumber;
      }
    }

    /// <summary>
    /// The total number of sections (minus one) that comprise the complete
    /// conditional access table.
    /// </summary>
    public byte LastSectionNumber
    {
      get
      {
        return _lastSectionNumber;
      }
    }

    /// <summary>
    /// The descriptors for the transport stream described by the conditional
    /// access table.
    /// </summary>
    /// <remarks>
    /// Conditional access descriptors are not included.
    /// </remarks>
    public ReadOnlyCollection<IDescriptor> Descriptors
    {
      get
      {
        return new ReadOnlyCollection<IDescriptor>(_descriptors);
      }
    }

    /// <summary>
    /// The conditional access descriptors for the transport stream described
    /// by the conditional access table.
    /// </summary>
    public ReadOnlyCollection<IDescriptor> CaDescriptors
    {
      get
      {
        return new ReadOnlyCollection<IDescriptor>(_caDescriptors);
      }
    }

    /// <summary>
    /// Cyclic redundancy check bytes for confirming the integrity of the
    /// conditional access section data.
    /// </summary>
    public ReadOnlyCollection<byte> Crc
    {
      get
      {
        return new ReadOnlyCollection<byte>(_crc);
      }
    }

    #endregion

    /// <summary>
    /// Decode and check the validity of raw conditional access section data.
    /// </summary>
    /// <param name="data">The raw conditional access section data.</param>
    /// <returns>a fully populated cat instance if the section is valid, otherwise <c>null</c></returns>
    public static TableConditionalAccess Decode(byte[] data)
    {
      Log.Debug("CAT: decode");
      if (data == null || data.Length < 12)
      {
        Log.Error("CAT: CAT not supplied or too short");
        return null;
      }

      try
      {
        if (data[0] != 0x01)
        {
          Log.Error("CAT: invalid table ID {0}", data[0]);
          throw new System.Exception();
        }
        if ((data[1] & 0x80) != 0x80)
        {
          Log.Error("CAT: section syntax indicator is 0, should be 1");
          throw new System.Exception();
        }
        if ((data[1] & 0x40) != 0)
        {
          Log.Error("CAT: corruption detected at header zero bit");
          throw new System.Exception();
        }

        TableConditionalAccess cat = new TableConditionalAccess();
        cat._tableId = data[0];
        cat._sectionSyntaxIndicator = (data[1] & 0x80) != 0;
        cat._sectionLength = (ushort)(((data[1] & 0x0f) << 8) | data[2]);
        if (3 + cat._sectionLength > data.Length)
        {
          Log.Error("CAT: section length {0} is invalid, data length = {1}", cat._sectionLength, data.Length);
          throw new System.Exception();
        }

        cat._version = (byte)((data[5] & 0x3e) >> 1);
        cat._currentNextIndicator = (data[5] & 0x01) != 0;
        cat._sectionNumber = data[6];
        cat._lastSectionNumber = data[7];

        // Descriptors.
        int offset = 8;
        int endDescriptors = data.Length - 4;
        cat._descriptors = new List<IDescriptor>(10);
        cat._caDescriptors = new List<IDescriptor>(5);
        while (offset + 1 < endDescriptors)
        {
          IDescriptor d = Descriptor.Decode(data, offset);
          if (d == null)
          {
            Log.Error("CAT: descriptor {0} is invalid", cat._descriptors.Count + cat._caDescriptors.Count + 1);
            throw new System.Exception();
          }
          offset += d.Length + 2;
          if (d.Tag == DescriptorTag.ConditionalAccess)
          {
            cat._caDescriptors.Add(d);
          }
          else
          {
            cat._descriptors.Add(d);
          }
        }
        if (offset != endDescriptors)
        {
          Log.Error("CAT: corruption detected at end of descriptors, offset = {0}, descriptors end = {1}", offset, endDescriptors);
          throw new System.Exception();
        }

        cat._crc = new byte[4];
        Buffer.BlockCopy(data, offset, cat._crc, 0, 4);

        // Make a copy of the CAT so that changes made by the caller on the
        // original array have no effect on our reference/copy.
        cat._rawCat = new byte[data.Length];
        Buffer.BlockCopy(data, 0, cat._rawCat, 0, data.Length);

        //cat.Dump();

        return cat;
      }
      catch
      {
        Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(data, data.Length);
        return null;
      }
    }

    /// <summary>
    /// Retrieve a read-only copy of the original conditional access section
    /// data that was decoded to create this Cat instance.
    /// </summary>
    /// <returns>a copy of the raw conditional access section data</returns>
    public ReadOnlyCollection<byte> GetRawCat()
    {
      return new ReadOnlyCollection<byte>(_rawCat);
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public void Dump()
    {
      this.LogDebug("CAT: dump...");
      Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(_rawCat, _rawCat.Length);
      this.LogDebug("  table ID                 = {0}", _tableId);
      this.LogDebug("  section syntax indicator = {0}", _sectionSyntaxIndicator);
      this.LogDebug("  section length           = {0}", _sectionLength);
      this.LogDebug("  version                  = {0}", _version);
      this.LogDebug("  current next indicator   = {0}", _currentNextIndicator);
      this.LogDebug("  section number           = {0}", _sectionNumber);
      this.LogDebug("  last section number      = {0}", _lastSectionNumber);
      this.LogDebug("  CRC                      = 0x{0:x2}{1:x2}{2:x2}{3:x2}", _crc[0], _crc[1], _crc[2], _crc[3]);
      this.LogDebug("  {0} descriptor(s)...", _descriptors.Count + _caDescriptors.Count);
      foreach (IDescriptor d in _descriptors)
      {
        d.Dump();
      }
      foreach (IDescriptor cad in _caDescriptors)
      {
        cad.Dump();
      }
    }
  }
}