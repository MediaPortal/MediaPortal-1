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
namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Types of cards
  /// </summary>
  public enum CardType
  {
    /// <summary>
    /// analog card
    /// </summary>
    Analog,
    /// <summary>
    /// DVB-S card
    /// </summary>
    DvbS,
    /// <summary>
    /// DVB-T card
    /// </summary>
    DvbT,
    /// <summary>
    /// DVB-C card
    /// </summary>
    DvbC,
    /// <summary>
    /// ATSC card
    /// </summary>
    Atsc,
    /// <summary>
    /// RadioWebStream card
    /// </summary>
    RadioWebStream,
    /// <summary>
    /// Unknown card
    /// </summary>
    Unknown
  }

}
