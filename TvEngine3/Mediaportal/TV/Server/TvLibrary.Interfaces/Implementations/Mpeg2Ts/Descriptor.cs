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
using System.Collections.ObjectModel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts
{
  /// <summary>
  /// A base class that implements the <see cref="IDescriptor"/> interface,
  /// modelling the basic descriptor structure defined in ISO/IEC 13818-1.
  /// </summary>
  public class Descriptor : IDescriptor
  {
    #region variables

    /// <summary>
    /// The descriptor's tag.
    /// </summary>
    protected DescriptorTag _tag;
    /// <summary>
    /// The descriptor data length.
    /// </summary>
    protected byte _length;
    /// <summary>
    /// The descriptor data.
    /// </summary>
    protected byte[] _data;

    /// <summary>
    /// The raw descriptor data (ie. including tag and length).
    /// </summary>
    protected byte[] _rawData;

    #endregion

    #region constructor

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// Protected to ensure instances can only be created by derived classes or
    /// by calling Decode(). This should ensure the safety of the parameters,
    /// which is why we don't check them.
    /// </remarks>
    /// <param name="tag">The descriptor's tag.</param>
    /// <param name="length">The descriptor data length.</param>
    /// <param name="data">The descriptor data.</param>
    protected Descriptor(DescriptorTag tag, byte length, byte[] data)
    {
      _tag = tag;
      _length = length;
      // Make a copy of the data array so that changes in the caller's array
      // don't affect our data.
      _data = new byte[data.Length];
      Buffer.BlockCopy(data, 0, _data, 0, data.Length);
    }

    /// <summary>
    /// Copy-constructor.
    /// </summary>
    /// <remarks>
    /// Protected to ensure instances can only be created by derived classes or
    /// by calling Decode(). This should ensure the safety of the parameters,
    /// which is why we don't check them.
    /// </remarks>
    /// <param name="descriptor">The descriptor to copy.</param>
    protected Descriptor(IDescriptor descriptor)
    {
      _tag = descriptor.Tag;
      _length = descriptor.Length;
      // Make a copy of the data array so that changes in the original
      // descriptor data don't affect our data.
      _data = new byte[descriptor.Data.Count];
      descriptor.Data.CopyTo(_data, 0);
    }

    #endregion

    #region properties

    /// <summary>
    /// The descriptor's tag.
    /// </summary>
    public DescriptorTag Tag
    {
      get
      {
        return _tag;
      }
    }

    /// <summary>
    /// The descriptor data length.
    /// </summary>
    public byte Length
    {
      get
      {
        return _length;
      }
    }

    /// <summary>
    /// The descriptor data.
    /// </summary>
    public ReadOnlyCollection<byte> Data
    {
      get
      {
        return new ReadOnlyCollection<byte>(_data);
      }
    }

    #endregion

    /// <summary>
    /// Decode raw descriptor data.
    /// </summary>
    /// <param name="data">The raw descriptor data.</param>
    /// <param name="offset">The offset in the data array at which the descriptor starts.</param>
    /// <returns>an IDescriptor instance</returns>
    public static IDescriptor Decode(byte[] data, int offset)
    {
      // Parse the base descriptor fields. Return null if they're not valid for
      // any reason.
      if (offset + 1 >= data.Length)
      {
        return null;
      }
      DescriptorTag tag = (DescriptorTag)data[offset];
      byte length = data[offset + 1];
      if (offset + 2 + length > data.Length)
      {
        return null;
      }

      // If we get to here, the descriptor data seems to be valid. Instantiate
      // a descriptor.
      byte[] descData = new byte[length];
      Buffer.BlockCopy(data, offset + 2, descData, 0, length);
      Descriptor d = new Descriptor(tag, length, descData);

      // Make a copy of the entire descriptor so that changes made by the
      // caller on the original array have no effect on our reference/copy.
      d._rawData = new byte[2 + d._length];
      Buffer.BlockCopy(data, offset, d._rawData, 0, 2 + length);

      return d;
    }

    /// <summary>
    /// Retrieve a read-only copy of the original data that was decoded to
    /// create this Descriptor instance.
    /// </summary>
    /// <returns>a copy of the raw descriptor data</returns>
    public ReadOnlyCollection<byte> GetRawData()
    {
      return new ReadOnlyCollection<byte>(_rawData);
    }

    /// <summary>
    /// For debug use.
    /// </summary>
    public virtual void Dump()
    {
      this.LogDebug("Descriptor: dump...");
      this.LogDebug("  tag    = {0}", _tag);
      this.LogDebug("  length = {0}", _length);
      Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper.Dump.DumpBinary(_data, _data.Length);
    }
  }
}