using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.DVB.Structures
{
  public struct PidInfo
  {
    public int stream_type;
    public int reserved_1;
    public int pid;
    public int reserved_2;
    public int ES_info_length;
    public string language;
    public bool isAC3Audio;
    public bool isAudio;
    public bool isVideo;
    public bool isTeletext;
    public bool isDVBSubtitle;
    public string teletextLANG;
  }
}
