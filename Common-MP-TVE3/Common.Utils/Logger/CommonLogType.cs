using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Common.Utils.Logger
{
  public enum CommonLogType
  {
    Log =1,
    Error = 2,
    Config = 3 ,
    Recorder = 100,
    EPG =101,
    VMR9 = 102,
    MusicShareWatcher =103,
    WebEPG = 104,
    Tv =200,
    TvConfig = 201,
    TvPlugIn = 202,
    PS = 300,
  }
}
