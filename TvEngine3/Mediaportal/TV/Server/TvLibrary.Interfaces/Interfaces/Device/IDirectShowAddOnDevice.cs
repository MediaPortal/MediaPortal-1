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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension
{
  /// <summary>
  /// An interface for DirectShow tuners. This interface can be used to support non-standard graph
  /// structures, additional filter requirements, or add-on devices which provide extended
  /// functions.
  /// </summary>
  public interface IDirectShowAddOnDevice : ICustomDevice
  {
    /// <summary>
    /// Insert and connect additional filter(s) into the graph.
    /// </summary>
    /// <remarks>
    /// It is expected that this function will update the lastFilter reference parameter as
    /// necessary to ensure that the caller can successfully finish building the graph. Usually
    /// that would mean setting lastFilter to the [last] additional filter that was successfully
    /// connected into the graph. However, the graph may also be restructured.
    /// 
    /// BDA:
    /// [network provider]->[tuner]->[capture]->[...additional filter(s)...]->[infinite tee]->[MPEG 2 demultiplexer]->[transport information filter]
    ///                                                                     ->[transport stream writer]
    ///
    /// WDM:
    /// [tuner]------------->[crossbar]->[capture]->[encoder]->[multiplexer]->[...additional filter(s)...]->[transport stream writer]
    ///        ->[TV audio]->
    /// </remarks>
    /// <param name="graph">The tuner filter graph.</param>
    /// <param name="lastFilter">The source filter (usually either a capture/receiver or
    ///   multiplexer filter) to connect the [first] additional filter to.</param>
    /// <returns><c>true</c> if one or more additional filters were successfully added to the graph, otherwise <c>false</c></returns>
    bool AddToGraph(IFilterGraph2 graph, ref IBaseFilter lastFilter);
  }
}
