using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DirectShowLib;
using DirectShowLib.BDA;
using TvDatabase;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;

namespace TvService
{
  public class Transponder 
  {
    TuningDetail _detail;
    List<Channel> _channels;
    int _currentChannelIndex;
    public Transponder(TuningDetail detail)
    {
      _channels = new List<Channel>();
      _detail = detail;
      _currentChannelIndex = -1;
    }


    public int Index
    {
      get
      {
        return _currentChannelIndex;
      }
      set
      {
        _currentChannelIndex = value;
      }
    }

    public List<Channel> Channels
    {
      get
      {
        return _channels;
      }
      set
      {
        _channels = value;
      }
    }
    public TuningDetail TuningDetail
    {
      get
      {
        return _detail;
      }
      set
      {
        _detail = value;
      }
    }
    public IChannel Tuning
    {
      get
      {
        if (Index<0 || Index >=Channels.Count) return null;
        TvBusinessLayer layer = new TvBusinessLayer();
        return layer.GetTuningChannelByType(Channels[Index],TuningDetail.ChannelType);
        

      }
    }

    public void OnTimeOut()
    {
      if (Index < 0 || Index >= Channels.Count) return;
      Channels[Index].LastGrabTime = DateTime.Now;
      Channels[Index].Persist();
      Log.Write("EPG: database updated for {0}", Channels[Index].Name);

    }

    public override bool  Equals(object obj)
    {
      Transponder other=(Transponder)obj;
      if (other.TuningDetail.ChannelType != TuningDetail.ChannelType) return false;
      if (other.TuningDetail.Frequency != TuningDetail.Frequency) return false;
      if (other.TuningDetail.Modulation != TuningDetail.Modulation) return false;
      if (other.TuningDetail.Symbolrate != TuningDetail.Symbolrate) return false;
      if (other.TuningDetail.Bandwidth != TuningDetail.Bandwidth) return false;
      if (other.TuningDetail.Polarisation != TuningDetail.Polarisation) return false;
      return true;
    }
    public void Dump()
    {
      Log.Write("Transponder:{0} {1} {2} {3} {4} {5}", _currentChannelIndex,TuningDetail.ChannelType, TuningDetail.Frequency, TuningDetail.Modulation, TuningDetail.Symbolrate, TuningDetail.Bandwidth, TuningDetail.Polarisation);
      foreach (Channel c in _channels)
      {
        Log.Write(" {0}", c.Name);
      }
    }
    public override string ToString()
    {
      return string.Format("type:{0} freq:{1} mod:{2} sr:{3} bw:{4} pol:{5}", 
        TuningDetail.ChannelType, TuningDetail.Frequency, 
        TuningDetail.Modulation, TuningDetail.Symbolrate, TuningDetail.Bandwidth, TuningDetail.Polarisation);
      
    }
  }
}
