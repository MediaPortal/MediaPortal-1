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

using System.Collections.ObjectModel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts
{
  /// <summary>
  /// An interface that models the descriptor structure defined in ISO/IEC 13818-1.
  /// </summary>
  public interface IDescriptor
  {
    /// <summary>
    /// The descriptor's tag.
    /// </summary>
    DescriptorTag Tag { get; }

    /// <summary>
    /// The descriptor data length.
    /// </summary>
    byte Length { get; }

    /// <summary>
    /// The descriptor data.
    /// </summary>
    ReadOnlyCollection<byte> Data { get; }

    /// <summary>
    /// Retrieve a read-only copy of the original data that was decoded to
    /// create a descriptor instance.
    /// </summary>
    /// <remarks>
    /// The copy includes tag and length bytes.
    /// </remarks>
    /// <returns>a copy of the raw descriptor data</returns>
    ReadOnlyCollection<byte> GetRawData();

    /// <summary>
    /// Write the descriptor fields to the log file. Useful for debugging.
    /// </summary>
    void Dump();
  }
}