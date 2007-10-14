using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MediaPortal.GUI.Library;

namespace MediaPortal.Player
{        

    class AudioSelector
    {

      public AudioSelector(MediaPortal.Player.TSReaderPlayer.IAudioStream dvbStreams)
        {
            if (dvbStreams == null)
            {
              throw new Exception("Nullpointer input not allowed ( IAudioStream )");
            }            
            else {
                this.dvbStreams = dvbStreams;                
            }            
        }                        

        public void SetAudioLanguage(int audioIndex) {
          dvbStreams.SetAudioStream(audioIndex);
          Log.Debug("Audio is now " + audioIndex);
        }

        public int GetAudioLanguage()
        {
          int audioIndex = 0;
          dvbStreams.GetAudioStream(ref audioIndex);
          return audioIndex;
        }
       
        private MediaPortal.Player.TSReaderPlayer.IAudioStream dvbStreams;        
        
    }
}
