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
      _teletextDecoder.ClearBuffer();
      device.GrabTeletext(true);
      _device = device;
      Log.Write("teletext: grab teletext for card:{0}", device.Graph.CommercialName);
    }

    static private void OnTvViewingStopped(int card, TVCaptureDevice device)
    {
      _teletextDecoder.ClearBuffer();
      device.GrabTeletext(false);
      Log.Write("teletext: stop grabbing teletext for card:{0}", device.Graph.CommercialName);
    }

    static private void OnTvChannelChanged(string tvChannelName)
    {
      _teletextDecoder.ClearBuffer();
      Log.Write("teletext: clear teletext cache");
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
