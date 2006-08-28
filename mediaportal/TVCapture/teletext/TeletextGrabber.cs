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
using MediaPortal.GUI.Library;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Teletext
{
  /// <summary>
  /// Summary description for TeletextGrabber.
  /// </summary>
  public class TeletextGrabber
  {
    static DVBTeletext _teletextDecoder;
    static bool _grabbing = false;
    static TVCaptureDevice _device;

    static TeletextGrabber()
    {

      _teletextDecoder = new DVBTeletext();
      Recorder.OnTvViewingStarted += new MediaPortal.TV.Recording.Recorder.OnTvViewHandler(OnTvViewingStarted);
      Recorder.OnTvViewingStopped += new MediaPortal.TV.Recording.Recorder.OnTvViewHandler(OnTvViewingStopped);
      Recorder.OnTvChannelChanged += new MediaPortal.TV.Recording.Recorder.OnTvChannelChangeHandler(OnTvChannelChanged);
    }

    static private void OnTvViewingStarted(int card, TVCaptureDevice device)
    {
      _grabbing = false;
      device.GrabTeletext(_grabbing);
      _device = device;
      Log.Info("teletext: grab teletext for card:{0}", device.Graph.CommercialName);
    }

    static private void OnTvViewingStopped(int card, TVCaptureDevice device)
    {
      _grabbing = false;
      device.GrabTeletext(_grabbing);
      Log.Info("teletext: stop grabbing teletext for card:{0}", device.Graph.CommercialName);
    }

    static private void OnTvChannelChanged(string tvChannelName)
    {
      _teletextDecoder.ClearBuffer();
      Log.Info("teletext: clear teletext cache");
    }

    static public void SaveData(IntPtr dataPtr)
    {
      _teletextDecoder.SaveData(dataPtr);
    }
    static public void SaveAnalogData(IntPtr dataPtr, int len)
    {
      _teletextDecoder.SaveAnalogData(dataPtr, len);
    }
    static public DVBTeletext TeletextCache
    {
      get { return _teletextDecoder; }
    }
    static public bool Grab
    {
      get
      {
        return _grabbing;
      }
      set
      {
        _grabbing = value;
        if (!_grabbing)
        {
          _teletextDecoder.ClearBuffer();
          if (_device != null)
            _device.GrabTeletext(false);
        }
        else
        {
          _teletextDecoder.ClearBuffer();
          if (_device != null)
            _device.GrabTeletext(true);
        }
      }
    }
  }
}
