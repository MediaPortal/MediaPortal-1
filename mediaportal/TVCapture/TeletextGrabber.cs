using System;
using MediaPortal.GUI.Library;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Summary description for TeletextGrabber.
  /// </summary>
  public class TeletextGrabber
  {
    static DVBTeletext _teletext;
    static bool _grabbing = false;
    static TVCaptureDevice _device;
    static TeletextGrabber()
    {

      _teletext = new DVBTeletext();
      Recorder.OnTvViewingStarted += new MediaPortal.TV.Recording.Recorder.OnTvViewHandler(OnTvViewingStarted);
      Recorder.OnTvViewingStopped += new MediaPortal.TV.Recording.Recorder.OnTvViewHandler(OnTvViewingStopped);
      Recorder.OnTvChannelChanged += new MediaPortal.TV.Recording.Recorder.OnTvChannelChangeHandler(OnTvChannelChanged);
    }

    static private void OnTvViewingStarted(int card, TVCaptureDevice device)
    {
      _teletext.ClearBuffer();
      device.GrabTeletext(true);
      _device = device;
      Log.Write("teletext: grab teletext for card:{0}", device.CommercialName);
    }

    static private void OnTvViewingStopped(int card, TVCaptureDevice device)
    {
      _teletext.ClearBuffer();
      device.GrabTeletext(false);
      Log.Write("teletext: stop grabbing teletext for card:{0}", device.CommercialName);
    }

    static private void OnTvChannelChanged(string tvChannelName)
    {
      _teletext.ClearBuffer();
      Log.Write("teletext: clear teletext cache");
    }

    static public void SaveData(IntPtr dataPtr)
    {
      _teletext.SaveData(dataPtr);
    }
    static public void SaveAnalogData(IntPtr dataPtr, int len)
    {
      _teletext.SaveAnalogData(dataPtr, len);
    }
    static public DVBTeletext TeletextCache
    {
      get { return _teletext; }
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
          _teletext.ClearBuffer();
          if (_device != null)
            _device.GrabTeletext(false);
        }
        else
        {
          _teletext.ClearBuffer();
          if (_device != null)
            _device.GrabTeletext(true);
        }
      }
    }
  }
}
