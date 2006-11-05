using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Util
{
  public class StopWatch
  {
    bool _isRunning;
    uint _startTime;

    public void Stop()
    {
      _isRunning = false;
    }
    public void StartZero()
    {
      _isRunning = true;
      _startTime = (uint)(DXUtil.Timer(DirectXTimer.GetAbsoluteTime) * 1000.0);
    }
    public bool IsRunning
    {
      get
      {
        return _isRunning;
      }
    }
    public uint ElapsedMilliseconds
    {
      get
      {
        uint currentTime = (uint)(DXUtil.Timer(DirectXTimer.GetAbsoluteTime) * 1000.0);
        return (currentTime - _startTime);
      }
    }
  }
}
