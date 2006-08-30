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

namespace TvLibrary.Channels
{
  /// <summary>
  /// class holding all tuning details for ATSC
  /// </summary>
  [Serializable]
  public class ATSCChannel : DVBBaseChannel
  {
    #region variables
    int _physicalChannel;
    int _majorChannel;
    int _minorChannel;
    int _symbolRate;
    ModulationType _modulation;
    #endregion

    public ATSCChannel()
    {
      _majorChannel = -1;
      _minorChannel = -1;
      _symbolRate = -1;
      _physicalChannel = -1;
    }

    #region properties
    /// <summary>
    /// gets/sets the modulation type
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
    /// <summary>
    /// gets/sets the physical channel
    /// </summary>
    public int PhysicalChannel
    {
      get
      {
        return _physicalChannel;
      }
      set
      {
        _physicalChannel = value;
      }
    }
    /// <summary>
    /// gets/sets the major channel
    /// </summary>
    public int MajorChannel
    {
      get
      {
        return _majorChannel;
      }
      set
      {
        _majorChannel = value;
      }
    }
    /// <summary>
    /// gets/sets the minor channel
    /// </summary>
    public int MinorChannel
    {
      get
      {
        return _minorChannel;
      }
      set
      {
        _minorChannel = value;
      }
    }
    /// <summary>
    /// gets/sets the symbolrate
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
    #endregion

    public override string ToString()
    {
      return String.Format("{0} phys:{1} maj:{2} min:{3} SR:{4} mod:{5}",
        base.ToString(), _physicalChannel, _majorChannel, _minorChannel, _symbolRate, _modulation);
    }

    public override bool Equals(object obj)
    {
      if ((obj as ATSCChannel) == null) return false;
      if (!base.Equals(obj)) return false;
      ATSCChannel ch = obj as ATSCChannel;
      if (ch.MajorChannel != MajorChannel) return false;
      if (ch.MinorChannel != MinorChannel) return false;
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
      return base.GetHashCode() ^ _physicalChannel.GetHashCode() ^ _majorChannel.GetHashCode()
             ^ _minorChannel.GetHashCode() ^ _symbolRate.GetHashCode() ^ _modulation.GetHashCode();
    }
  }
}
