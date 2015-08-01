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

    public string Separator;
    public string Enter;
    public string PreChange;

    public int CommandDelay;        // unit = ms

    public string PowerOn;
    public string PowerOff;
    public string PowerToggle;

    public int PowerChangeDelay;    // unit = ms
  }
}