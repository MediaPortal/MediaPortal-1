#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;

namespace MediaPortal.Configuration
{
  public class RadioStation
  {
    public bool Scrambled = false;
    public int ID = -1;
    public string Type = "";
    public string Name = "";
    public string Genre = "";
    public int Bitrate = 0;
    public string URL = "";
    public Frequency Frequency;

    public RadioStation()
    {
    }

    public RadioStation(int channel, long frequency)
    {
      this.Frequency = frequency;
    }

    public override string ToString()
    {
      return String.Format("Frequency: {0}", Frequency.ToString(Frequency.Format.Hertz));
    }
  }
}
