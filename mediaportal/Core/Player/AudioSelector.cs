using System;

namespace MediaPortal.Player
{
  internal class AudioSelector
  {
    public AudioSelector(TSReaderPlayer.IAudioStream dvbStreams)
    {
      if (dvbStreams == null)
      {
        throw new Exception("Nullpointer input not allowed ( IAudioStream )");
      }
      else
      {
        this.dvbStreams = dvbStreams;
      }
    }

    public int GetAudioStream()
    {
      int audioIndex = 0;
      dvbStreams.GetAudioStream(ref audioIndex);
      return audioIndex;
    }

    private TSReaderPlayer.IAudioStream dvbStreams;
  }
}