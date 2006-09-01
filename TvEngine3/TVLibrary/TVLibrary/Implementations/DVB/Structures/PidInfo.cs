using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.DVB.Structures
{
  /// <summary>
  /// Structure holding all information about a single pid
  /// </summary>
  public struct PidInfo
  {
    /// <summary>
    /// stream type
    /// </summary>
    public int stream_type;
    /// <summary>
    /// reserved
    /// </summary>
    public int reserved_1;
    /// <summary>
    /// pid
    /// </summary>
    public int pid;

    /// <summary>
    /// reserved
    /// </summary>
    public int reserved_2;

    /// <summary>
    /// es info length
    /// </summary>
    public int ES_info_length;

    /// <summary>
    /// audio language
    /// </summary>
    public string language;
    /// <summary>
    /// true if pid contains ac3 audio
    /// </summary>
    public bool isAC3Audio;
    /// <summary>
    /// true if pid contains mpeg1/2 audio
    /// </summary>
    public bool isAudio;
    /// <summary>
    /// true if pid contains video
    /// </summary>
    public bool isVideo;
    /// <summary>
    /// true if pid contains teletext
    /// </summary>
    public bool isTeletext;
    /// <summary>
    /// true if pid contains dvb subtitles
    /// </summary>
    public bool isDVBSubtitle;
    /// <summary>
    /// teletext language
    /// </summary>
    public string teletextLANG;

    /// <summary>
    /// Ctor for an audio pid
    /// </summary>
    /// <param name="audioPid">The audio pid.</param>
    /// <param name="audioLanguage">The audio language.</param>
    public void AudioPid(int audioPid, string audioLanguage)
    {
      pid = audioPid;
      language = audioLanguage;
      stream_type = 3;
      isAudio = true;
    }

    /// <summary>
    /// Ctor for an ac3 pid
    /// </summary>
    /// <param name="ac3Pid">The ac3 pid.</param>
    /// <param name="audioLanguage">The audio language.</param>
    public void Ac3Pid(int ac3Pid, string audioLanguage)
    {
      pid = ac3Pid;
      language = audioLanguage;
      stream_type = 0;
      isAC3Audio = true;
    }

    /// <summary>
    /// Ctor for an video pid
    /// </summary>
    /// <param name="videoPid">The video pid.</param>
    public void VideoPid(int videoPid)
    {
      pid = videoPid;
      language = "";
      stream_type = 1;
      isVideo = true;
    }
    /// <summary>
    /// ctor for a teletext pid
    /// </summary>
    /// <param name="teletextPid">The teletext pid.</param>
    public void TeletextPid(int teletextPid)
    {
      pid = teletextPid;
      language = "";
      isTeletext = true;
    }
    /// <summary>
    /// ctor for a subtitle pid
    /// </summary>
    /// <param name="subtitlePid">The subtitle pid.</param>
    public void SubtitlePid(int subtitlePid)
    {
      pid = subtitlePid;
      language = "";
      isDVBSubtitle = true;
    }
    /// <summary>
    /// Returns the fully qualified type name of this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> containing a fully qualified type name.
    /// </returns>
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
