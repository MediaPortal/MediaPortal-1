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

using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Stream
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVScanning"/> for stream tuners.
  /// </summary>
  public class ScannerStream : ScannerMpeg2TsBase
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="ScannerStream"/> class.
    /// </summary>
    /// <param name="tuner">The tuner associated with this scanner.</param>
    /// <param name="analyser">The stream analyser instance to use for scanning.</param>
    public ScannerStream(TunerStream tuner, ITsChannelScan analyser)
      : base(tuner, analyser)
    {
    }

    /// <summary>
    /// Set the name for services which do not supply a name.
    /// </summary>
    /// <param name="channel">The service details.</param>
    protected override void SetMissingServiceName(IChannel channel)
    {
      DVBIPChannel dvbipChannel = channel as DVBIPChannel;
      if (dvbipChannel == null)
      {
        return;
      }

      // Streams often don't have meaningful PSI. Just use the URL in those cases.
      dvbipChannel.Name = dvbipChannel.Url;
    }
  }
}