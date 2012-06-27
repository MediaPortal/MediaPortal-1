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
    /// Unlike regular LNBs, bandstacked LNBs pass all transponders in a single cable at all times. This is
    /// as opposed with a regular LNB which only passes the transponders with a particular polarity, depending
    /// on the voltage supplied. Bandstacking greatly improves the ability to split the signal to more than
    /// one receiver without complicated or expensive switch equipement. The cost is that the LNB is not able
    /// to receive as wide a range of transponders as a regular LNB. This is due to the fact that the LNB
    /// shifts the frequencies of each transponder in such a way that they don't overlap... but this
    /// requires the use of bandwidth that would otherwise be used for receiving a wider frequency range.
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
    /// one satellite dish. The only side-effect is that the signal polarisation is inverted by the second reflector.
    /// For circularly polarised signals, this completely reverses the polarity; for linearly polarised signals it
    /// doesn't particularly matter.
    /// </remarks>
    bool IsToroidal { get; set; }
  }
}
