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

    public void AudioPid(int audioPid, string audioLanguage)
    {
      pid = audioPid;
      language = audioLanguage;
      stream_type = 3;
      isAudio = true;
    }

    public void Ac3Pid(int ac3Pid, string audioLanguage)
    {
      pid = ac3Pid;
      language = audioLanguage;
      stream_type = 0;
      isAC3Audio = true;
    }

    public void VideoPid(int videoPid)
    {
      pid = videoPid;
      language = "";
      stream_type = 1;
      isVideo = true;
    }
    public void TeletextPid(int teletextPid)
    {
      pid = teletextPid;
      language = "";
      isTeletext = true;
    }
    public void SubtitlePid(int subtitlePid)
    {
      pid = subtitlePid;
      language = "";
      isDVBSubtitle = true;
    }
    public override string ToString()
    {
      if (isVideo) return String.Format("pid:{0:X} video", pid);
      if (isAC3Audio) return String.Format("pid:{0:X} ac3 lang:{1} type:{2}", pid, language, stream_type);
      if (isAudio) return String.Format("pid:{0:X} audio lang:{1} type:{2}", pid, language, stream_type);
      if (isTeletext) return String.Format("pid:{0:X} teletext", pid);
      if (isDVBSubtitle) return String.Format("pid:{0:X} subtitle", pid);
      return string.Format("pid:{0:X} type unknown", pid);
    }
  }
}
