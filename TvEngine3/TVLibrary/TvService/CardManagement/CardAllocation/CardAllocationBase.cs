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

using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvControl;

namespace TvService
{
  public class CardAllocationBase
  {
    #region protected members

    protected static bool isFTA(ITvCardHandler tvcard, User user)
    {
      IChannel unknownChannel = tvcard.CurrentChannel(ref user);

      bool fta = true;

      if (unknownChannel != null)
      {
        if (unknownChannel is DVBCChannel)
        {
          fta = ((DVBCChannel)unknownChannel).FreeToAir;
        }
        else if (unknownChannel is DVBSChannel)
        {
          fta = ((DVBSChannel)unknownChannel).FreeToAir;
        }
        else if (unknownChannel is DVBTChannel)
        {
          fta = ((DVBTChannel)unknownChannel).FreeToAir;
        }
        else if (unknownChannel is ATSCChannel)
        {
          fta = ((ATSCChannel)unknownChannel).FreeToAir;
        }
      }
      return fta;
    }

    #endregion

  }
}
