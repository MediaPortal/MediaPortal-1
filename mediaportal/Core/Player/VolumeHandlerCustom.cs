#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using MediaPortal.Configuration;
using MediaPortal.Profile;

namespace MediaPortal.Player
{
  internal class VolumeHandlerCustom : VolumeHandler
  {
    #region Constructors

    public VolumeHandlerCustom()
    {
      using (Settings reader = new MPSettings())
      {
        string text = reader.GetValueAsString("volume", "table",
                                              "0, 4095, 8191, 1638, 12287, 16383, 20479, 24575, 28671, 32767, 36863, 40959, 45055, 49151, 53247, 57343, 61439, 65535");

        if (text == string.Empty)
        {
          return;
        }

        ArrayList array = new ArrayList();

        try
        {
          foreach (string volume in text.Split(new char[] {',', ';'}))
          {
            if (volume == string.Empty)
            {
              continue;
            }

            array.Add(Math.Max(this.Minimum, Math.Min(this.Maximum, int.Parse(volume))));
          }

          array.Sort();

          this.Table = (int[]) array.ToArray(typeof (int));
        }
        catch
        {
          // heh, its undocumented remember, no fancy logging going on here
        }
      }
    }

    #endregion Constructors
  }
}