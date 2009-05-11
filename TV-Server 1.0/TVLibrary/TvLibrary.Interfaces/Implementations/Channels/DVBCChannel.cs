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
using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLib.BDA;
using TvLibrary.Interfaces;

namespace TvLibrary.Channels
{
  /// <summary>
  /// class holding all tuning details for DVBC
  /// </summary>
  [Serializable]
  public class DVBCChannel : DVBBaseChannel
  {
    #region variables
    ModulationType _modulation;
    int _symbolRate;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="T:DVBCChannel"/> class.
    /// </summary>
    public DVBCChannel()
    {
      ModulationType = ModulationType.Mod64Qam;
      SymbolRate = 6875;
    }

    #region properties
    /// <summary>
    /// gets/sets the symbolrate for this channel
    /// </summary>
    public int SymbolRate
    {
      get
      {
        return _symbolRate;
      }
      set
      {
        _symbolRate = value;
      }
    }
    /// <summary>
    /// gets/sets the ModulationType for this channel
    /// </summary>
    public ModulationType ModulationType
    {
      get
      {
        return _modulation;
      }
      set
      {
        _modulation = value;
      }
    }
    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      string line = String.Format("DVBC:{0} SymbolRate:{1} Modulation:{2}",
          base.ToString(), SymbolRate, ModulationType);
      return line;
    }


    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(object obj)
    {
      if ((obj as DVBCChannel) == null) return false;
      if (!base.Equals(obj)) return false;
      DVBCChannel ch = obj as DVBCChannel;
      if (ch.ModulationType != ModulationType) return false;
      if (ch.SymbolRate != SymbolRate) return false;

      return true;
    }
    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return base.GetHashCode() ^ _modulation.GetHashCode() ^ _symbolRate.GetHashCode() ;
    }
  }
}

