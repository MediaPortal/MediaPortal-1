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
using System;
using System.Collections.Generic;
using System.Threading;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;

namespace TvLibrary.Implementations.Analog
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for analog cards
  /// </summary>
  public class AnalogScanning : ITVScanning, IAnalogChannelScanCallback
  {
    private readonly TvCardAnalog _card;
    private long _previousFrequency;
    private int _radioSensitivity = 1;
    private ManualResetEvent _event;
    private IAnalogChanelScan _scanner;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalogScanning"/> class.
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
    /// <param name="settings"></param>
    /// <returns>list of channels found</returns>
    public List<IChannel> Scan(IChannel channel, ScanParameters settings)
    {
      _card.IsScanning = true;
      _card.Tune(0, channel);
      if (_card.IsTunerLocked)
      {
        if (channel.IsTv)
        {
          if (_card.VideoFrequency == _previousFrequency)
            return new List<IChannel>();
          _previousFrequency = _card.VideoFrequency;
        }

        if (channel.IsTv)
        {
          try
          {
            _scanner = _card.GetChannelScanner();
            _event = new ManualResetEvent(false);
            _scanner.SetCallBack(this);
            _scanner.Start();
            _event.WaitOne(settings.TimeOutAnalog * 1000, true);

            IntPtr serviceName;
            _scanner.GetChannel(out serviceName);
            _scanner.Stop();
            string channelName = DvbTextConverter.Convert(serviceName, "");

            int pos = channelName.LastIndexOf("teletext", StringComparison.InvariantCultureIgnoreCase);
            if (pos != -1)
            {
              channelName = channelName.Substring(0, pos);
            }
            //Some times channel name includes program name after :
            pos = channelName.LastIndexOf(":");
            if (pos != -1)
            {
              channelName = channelName.Substring(0, pos);
            }
            channelName = channelName.TrimEnd(new char[] { '\'', '\"', '´', '`' });
            channelName = channelName.Trim();
            if (channelName != "")
            {
              channel.Name = "";
              for (int x = 0; x < channelName.Length; ++x)
              {
                char k = channelName[x];
                if (k < (char)32 || k > (char)127)
                  break;
                channel.Name += k.ToString();
              }
            }
          }
          finally
          {
            if (_scanner != null)
            {
              _scanner.SetCallBack(null);
              _scanner.Stop();
            }
            if (_event != null)
            {
              _event.Close();
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

    /// <summary>
    /// Tunes to channels based on the list the multiplexes that make up a DVB network.
    /// This information is obtained from the DVB NIT (Network Information Table)
    /// Not applicable for Analog.
    /// </summary>
    /// <param name="channel">channel to tune to</param>
    /// <param name="settings">ScanParameters to use while tuning</param>
    /// <returns></returns>
    public List<IChannel> ScanNIT(IChannel channel, ScanParameters settings)
    {
      return new List<IChannel>();
    }

    #region IAnalogChannelScanCallback Members
    /// <summary>
    /// Called when [scanner done].
    /// </summary>
    /// <returns></returns>
    public int OnScannerDone()
    {
      _event.Set();
      return 0;
    }
    #endregion
  }
}
