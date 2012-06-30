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

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// This interface describes the properties of a satellite dish low noise block down-converter.
  /// </summary>
  public interface ILnbType
  {
    /// <summary>
    /// Get/set the LNB type's name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Get/set the LNB type's low band local oscillator frequency.
    /// </summary>
    int LowBandFrequency { get; set; }

    /// <summary>
    /// Get/set the LNB type's high band local oscillator frequency.
    /// </summary>
    int HighBandFrequency { get; set; }

    /// <summary>
    /// Get/set the LNB type's switch frequency.
    /// </summary>
    int SwitchFrequency { get; set; }

    /// <summary>
    /// Get/set whether the LNB type is a band-stacked LNB type.
    /// </summary>
    /// <remarks>
    /// Unlike regular LNBs, bandstacked LNBs pass all transponder streams in a single cable at all times. This
    /// is a distinct advantage over regular LNBs which only pass the transponder stream with a particular
    /// polarity, depending on the voltage supplied. Bandstacking greatly improves the ability to split the
    /// signal to multiple receivers without complicated or expensive switch equipement. The trade-off is that
    /// bandstacked LNBs are not able to receive as wide a frequency range as a regular LNB. This is due to the
    /// fact that the LNB shifts the frequencies of each transponder in such a way that they don't overlap...
    /// but this requires the use of bandwidth that would otherwise be used for receiving other transponders.
    /// The net result is that all the transponders of a particular polarity (usually vertical or circular
    /// right) end up with a lower frequency than the other transponders.
    /// </remarks>
    bool IsBandStacked { get; set; }

    /// <summary>
    /// Get/set whether the LNB type is a toroidal LNB type.
    /// </summary>
    /// <remarks>
    /// Toroidal satellite dishes have two reflectors:
    /// - the main reflector, which is just like any other standard satellite dish
    /// - a sub-reflector which catches the signals from the main reflector and bounces them back towards the main
    ///   dish, spreading them in the process
    /// The spreading makes it significantly easier to receive signals from a large number of satellites with just
    /// one satellite dish. The signal is slightly weakened by the second reflector, and the signal polarisation
    /// is also inverted. For circularly polarised signals, the inversion completely reverses the polarity; for
    /// linearly polarised signals it doesn't particularly matter.
    /// </remarks>
    bool IsToroidal { get; set; }
  }
}
