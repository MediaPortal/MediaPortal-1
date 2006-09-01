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
using System.Runtime.InteropServices;
using System.Windows.Forms;

using TvLibrary.Interfaces;
using DirectShowLib;

namespace TvLibrary.Implementations.Analog
{
  
  /// <summary>
  /// Class which implements scanning for tv/radio channels for analog cards
  /// </summary>
  public class AnalogScanning : ITVScanning
  {
    TvCardAnalog _card;
    long _previousFrequency = 0;
    int _radioSensitivity = 1;
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AnalogScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public AnalogScanning(TvCardAnalog card)
    {
      _card = card;
    }

    /// <summary>
    /// returns the tv card used
    /// </summary>
    /// <value></value>
    public ITVCard TvCard
    {
      get
      {
        return _card;
      }
    }
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// resets the scanner
    /// </summary>
    public void Reset()
    {
      _previousFrequency = 0;
    }

    /// <summary>
    /// Property to set Radio tuning sensitivity.
    /// sensitivity range from 1MHz for value 1 to 0.1MHZ for value 10
    /// </summary>
    public int RadioSensitivity
    {
      get
      {
        return _radioSensitivity;
      }
      set
      {
        _radioSensitivity = value;
      }
    }

    /// <summary>
    /// Tunes to the channel specified and will start scanning for any channel
    /// </summary>
    /// <param name="channel">channel to tune to</param>
    /// <returns>list of channels found</returns>
    public List<IChannel> Scan(IChannel channel)
    {
      _card.IsScanning = true;
      AnalogChannel analogChannel = (AnalogChannel)channel;
      _card.Tune(channel);
      _card.GrabTeletext = true;
      if (_card.IsTunerLocked)
      {
        if (channel.IsTv)
        {
          if (_card.VideoFrequency == _previousFrequency) return new List<IChannel>();
          _previousFrequency = _card.VideoFrequency;
        }

        if (_card.GrabTeletext)
        {
          _card.TeletextDecoder.ClearTeletextChannelName();
          for (int i = 0; i < 20; ++i)
          {
            System.Threading.Thread.Sleep(100);
            string channelName = _card.TeletextDecoder.GetTeletextChannelName();
            if (channelName != "")
            {
              channel.Name = channelName;
              break;
            }
          }
        }
        List<IChannel> list = new List<IChannel>();
        list.Add(channel);
        _card.IsScanning = false;
        return list;
      }
      _card.IsScanning = false;
      return null;
    }
  }
}
