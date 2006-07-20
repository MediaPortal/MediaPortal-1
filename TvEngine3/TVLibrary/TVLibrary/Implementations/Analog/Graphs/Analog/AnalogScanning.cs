using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using TvLibrary.Interfaces;
using DirectShowLib;

namespace TvLibrary.Implementations.Analog
{
  
  public class AnalogScanning : ITVScanning
  {
    TvCardAnalog _card;
    public AnalogScanning(TvCardAnalog card)
    {
      _card = card;
    }

    public ITVCard TvCard
    {
      get
      {
        return _card;
      }
    }
    public void Dispose()
    {
    }

    public void Reset()
    {
    }

    public List<IChannel> Scan(IChannel channel)
    {
      _card.IsScanning = true;
      AnalogChannel analogChannel = (AnalogChannel)channel;
      _card.Tune(channel);
      _card.GrabTeletext = true;
      if (_card.IsTunerLocked)
      {
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
