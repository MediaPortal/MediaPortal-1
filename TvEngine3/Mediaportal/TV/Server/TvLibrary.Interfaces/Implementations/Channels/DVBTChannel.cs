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
using System.Runtime.Serialization;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels
{
  /// <summary>
  /// Tuning part of DVB-T required for scanning
  /// </summary>
  [Serializable]
  public struct DVBTTuning
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="DVBTTuning"/> class.
    /// </summary>
    /// <param name="frequency">The transmission frequency, in kHz.</param>
    /// <param name="bandwidth">The transmission bandwidth, in kHz.</param>
    /// <param name="offset">The transmission frequency offset, in kHz.</param>
    /// <param name="cellId">The transmitter's cell identifier.</param>
    /// <param name="cellIdExtension">The transmitter's cell identifier extension.</param>
    /// <param name="isDvbT2"><c>True</c> if the transmission complies with the DVB-T2 standard, otherwise <c>false</c>.</param>
    public DVBTTuning(int frequency, int bandwidth, int offset, int cellId, int cellIdExtension, bool isDvbT2)
    {
      Frequency = frequency;
      Bandwidth = bandwidth;
      Offset = offset;
      CellId = cellId;
      CellIdExtension = cellIdExtension;
      IsDvbT2 = isDvbT2;
    }

    /// <summary>
    /// The transmission frequency, in kHz.
    /// </summary>
    public int Frequency;

    /// <summary>
    /// The transmission bandwidth, in kHz.
    /// </summary>
    public int Bandwidth;

    /// <summary>
    /// The transmission frequency offset, in kHz.
    /// </summary>
    public int Offset;

    /// <summary>
    /// The transmitter's cell identifier.
    /// </summary>
    public int CellId;

    /// <summary>
    /// The transmitter's cell identifier extension.
    /// </summary>
    public int CellIdExtension;

    /// <summary>
    /// Indicates whether the transmission complies with the second generation DVB terrestrial
    /// broadcasting standard (DVB-T2 EN 302 755).
    /// </summary>
    public bool IsDvbT2;

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("freq:{0}/{2} bandwidth:{1}", Frequency, Bandwidth, Offset);
    }
  }
  
  /// <summary>
  /// class holding all tuning details for DVBT
  /// </summary>  
  [DataContract]
  public class DVBTChannel : DVBBaseChannel
  {
    #region variables

    [DataMember]
    private int _bandWidth;

    [DataMember]
    private int _offset;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBTChannel"/> class.
    /// </summary>
    public DVBTChannel()
    {
      Bandwidth = 8000;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DVBTChannel"/> class.
    /// </summary>
    /// <param name="tuning">Tuning detail</param>
    public DVBTChannel(DVBTTuning tuning)
    {
      TuningInfo = tuning;
    }

    /// <summary>
    /// returns basic tuning info for current channel
    /// </summary>
    public DVBTTuning TuningInfo
    {
      get { return new DVBTTuning((int)Frequency, Bandwidth, Offset, 0, 0, false); }
      set
      {
        Frequency = value.Frequency;
        Bandwidth = value.Bandwidth;
        Offset = value.Offset;
      }
    }

    /// <summary>
    /// gets/sets the bandwidth for this channel
    /// </summary>
    public int Offset
    {
      get { return _offset; }
      set { _offset = value; }
    }

    /// <summary>
    /// gets/sets the bandwidth for this channel
    /// </summary>
    public int Bandwidth
    {
      get { return _bandWidth; }
      set { _bandWidth = value; }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      string line = String.Format("DVBT:{0} BandWidth:{1}", base.ToString(), Bandwidth);
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
      if ((obj as DVBTChannel) == null)
      {
        return false;
      }
      if (!base.Equals(obj))
      {
        return false;
      }
      DVBTChannel ch = obj as DVBTChannel;
      if (ch.Bandwidth != Bandwidth)
      {
        return false;
      }

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
      return base.GetHashCode() ^ _bandWidth.GetHashCode();
    }

    /// <summary>
    /// Checks if the given channel and this instance are on the different transponder
    /// </summary>
    /// <param name="channel">Channel to check</param>
    /// <returns>true, if the channels are on the same transponder</returns>
    public override bool IsDifferentTransponder(IChannel channel)
    {
      DVBTChannel dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel == null)
      {
        return true;
      }
      return dvbtChannel.Frequency != Frequency ||
             dvbtChannel.Bandwidth != Bandwidth;
    }
  }
}