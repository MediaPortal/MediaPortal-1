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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations
{
  [Serializable]
  public class SetTopBoxProfile
  {
    public string Name;

    #region commands
    // All commands are Pronto-formatted strings.
    // http://www.remotecentral.com/features/irdisp1.htm

    public string Digit0;
    public string Digit1;
    public string Digit2;
    public string Digit3;
    public string Digit4;
    public string Digit5;
    public string Digit6;
    public string Digit7;
    public string Digit8;
    public string Digit9;

    /// <summary>
    /// The separator command for two-part channel numbers (eg. XXX.YY) used
    /// for ATSC and some Dish Network channels.
    /// </summary>
    /// <remarks>
    /// If available, the corresponding remote control button may be labelled
    /// as '.', '-' or '*'.
    /// </remarks>
    public string Separator;
    /// <summary>
    /// The enter/select/OK command.
    /// </summary>
    public string Enter;
    /// <summary>
    /// The [optional] command to send before the first channel number digit.
    /// </summary>
    /// <remarks>
    /// If available, the corresponding remote control button may be labelled
    /// as '-/--'.
    /// </remarks>
    public string PreChange;

    /// <summary>
    /// The command to turn the set top box on.
    /// </summary>
    public string PowerOn;
    /// <summary>
    /// The command to turn the set top box off.
    /// </summary>
    public string PowerOff;
    /// <summary>
    /// The command to toggle the set top box's power state.
    /// </summary>
    /// <remarks>
    /// This command will turn the set top box on if it is currently off, or
    /// off if it is currently on.
    /// </remarks>
    public string PowerToggle;

    #endregion

    /// <summary>
    /// The time to wait between sending pre-change/digit/separator/enter
    /// commands. The unit is milli-seconds (ms).
    /// </summary>
    public int CommandDelay;

    /// <summary>
    /// The time to wait after sending a power on/off/toggle command and before
    /// sending any other command. The unit is milli-seconds (ms).
    /// </summary>
    public int PowerChangeDelay;

    /// <summary>
    /// How many digits must be sent to the set top box in order to select a
    /// channel?
    /// </summary>
    /// <remarks>
    /// Channel numbers will be zero-padded to supply the correct number of
    /// digits.
    /// Zero means don't apply padding.
    /// </remarks>
    public int DigitCount;
  }
}