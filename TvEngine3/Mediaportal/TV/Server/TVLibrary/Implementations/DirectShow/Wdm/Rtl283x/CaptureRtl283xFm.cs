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

using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Components;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Rtl283x
{
  /// <summary>
  /// An override of the analog capture component class. This enables us to
  /// easily buid an RTL283x FM radio capture graph.
  /// </summary>
  internal class CaptureRtl283xFm : Capture
  {
    [ComImport, Guid("6b368f8c-f383-44d3-b8c2-3a150b70b1c9")]
    private class Rtl283xFmSource
    {
    }

    /// <summary>
    /// Load the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    /// <param name="captureGraphBuilder">The capture graph builder instance associated with the graph.</param>
    /// <param name="productInstanceId">A common identifier shared by the tuner's components.</param>
    /// <param name="crossbar">The crossbar component.</param>
    public override void PerformLoading(IFilterGraph2 graph, ICaptureGraphBuilder2 captureGraphBuilder, string productInstanceId, Crossbar crossbar)
    {
      this.LogDebug("RTL283x FM capture: perform loading");
      _filterAudio = FilterGraphTools.AddFilterFromRegisteredClsid(graph, typeof(Rtl283xFmSource).GUID, "RTL283x FM Source");
    }
  }
}