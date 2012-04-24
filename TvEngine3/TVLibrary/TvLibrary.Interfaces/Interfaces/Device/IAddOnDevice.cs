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

using DirectShowLib;

namespace TvLibrary.Interfaces.Device
{
  /// <summary>
  /// An interface for devices that require one or more additional filters to be connected into the BDA tuner
  /// graph in order to enable custom or extended functions.
  /// </summary>
  public interface IAddOnDevice : ICustomDevice
  {
    /// <summary>
    /// Insert and connect the device's additional filter(s) into the BDA graph.
    /// [network provider]->[tuner]->[capture]->[...device filter(s)]->[infinite tee]->[MPEG 2 demultiplexer]->[transport information filter]->[transport stream writer]
    /// </summary>
    /// <remarks>
    /// It is expected that this function will update the lastFilter reference parameter as necessary to
    /// ensure that the caller can successfully finish building the graph. Usually that would mean pointing
    /// the reference at the [last] device filter that was successfully connected into the graph.
    /// </remarks>
    /// <param name="graphBuilder">The graph builder to use to insert the device filter(s).</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver filter) to
    ///   connect the [first] device filter to.</param>
    /// <returns><c>true</c> if the device was successfully added to the graph, otherwise <c>false</c></returns>
    bool AddToGraph(ICaptureGraphBuilder2 graphBuilder, ref IBaseFilter lastFilter);
  }
}
