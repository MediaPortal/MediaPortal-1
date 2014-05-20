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

using System.Collections.Generic;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Rtl283x
{
  internal class EncoderRtl283xFm : Encoder
  {
    /// <summary>
    /// Load the encoder component.
    /// </summary>
    /// <remarks>
    /// This override is required because the standard encoder component
    /// incorrectly detects the source filter RDS output pin as a capture pin,
    /// which then results in graph building failure.
    /// </remarks>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    /// <param name="productInstanceId">A common identifier shared by the tuner's components.</param>
    /// <param name="capture">The capture component.</param>
    public override void PerformLoading(IFilterGraph2 graph, string productInstanceId, Capture capture)
    {
      AddAndConnectSoftwareFilters(graph, capture, null);
      IPin pin = DsFindPin.ByDirection(_filterEncoderAudio, PinDirection.Output, 0);
      try
      {
        if (!AddAndConnectTsMultiplexer(graph, new List<IPin> { pin }))
        {
          throw new TvException("Failed to add and connect TS multiplexer.");
        }
      }
      finally
      {
        Release.ComObject("RTL283x FM encoder output pin", ref pin);
      }
    }
  }
}