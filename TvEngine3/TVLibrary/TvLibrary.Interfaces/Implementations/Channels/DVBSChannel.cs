/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
  public enum DisEqcType
  {
    None,
    SimpleA,
    SimpleB,
    Level1AA,
    Level1BA,
    Level1AB,
    Level1BB,
  };

  /// <summary>
  /// class holding all tuning details for DVBS
  /// </summary>
  [Serializable]
  public class DVBSChannel : DVBBaseChannel
  {
    #region variables
    Polarisation _polarisation;
    int _symbolRate;
    int _switchingFrequency;
    DisEqcType _disEqc;
    #endregion

    public DVBSChannel()
    {
      SwitchingFrequency = 0;
      DisEqc = DisEqcType.SimpleA;
    }

    #region properties
    /// <summary>
    /// gets/sets the Polarisation for this channel
    /// </summary>
    public Polarisation Polarisation
    {
      get
      {
        return _polarisation;
      }
      set
      {
        _polarisation = value;
      }
    }
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
    /// gets/sets the lnb switch frequency for this channel
    /// </summary>
    public int SwitchingFrequency
    {
      get
      {
        return _switchingFrequency;
      }
      set
      {
        _switchingFrequency = value;
      }
    }
    /// <summary>
    /// gets/sets the diseqc setting for this channel
    /// </summary>
    public DisEqcType DisEqc
    {
      get
      {
        return _disEqc;
      }
      set
      {
        _disEqc = value;
      }
    }
    #endregion

    public override string ToString()
    {
      string line = String.Format("DVBS:{0} SymbolRate:{1} Polarisation:{2} DisEqc:{3} Switch Freq:{4}",
          base.ToString(), SymbolRate, Polarisation, DisEqc,SwitchingFrequency);
      return line;
    }

    public override bool Equals(object obj)
    {
      if ((obj as DVBSChannel)==null) return false;
      if (!base.Equals(obj)) return false;
      DVBSChannel ch = obj as DVBSChannel;
      if (ch.Polarisation != Polarisation) return false;
      if (ch.SymbolRate != SymbolRate) return false;
      if (ch.SwitchingFrequency != SwitchingFrequency) return false;
      if (ch.DisEqc != DisEqc) return false;

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
      return base.GetHashCode() ^ _polarisation.GetHashCode() ^ _symbolRate.GetHashCode() ^
             _switchingFrequency.GetHashCode() ^ _disEqc.GetHashCode() ;
    }
  }
}

