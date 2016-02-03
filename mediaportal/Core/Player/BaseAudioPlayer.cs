using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MediaPortal.Player.MediaInfo;

namespace MediaPortal.Player
{
    public abstract class BaseAudioPlayer : IPlayer
    {
        public override int EditionStreams { get { return 0; } }

        public override int CurrentEditionStream
        {
            get { return 0; }
            set { }
        }

        public override int VideoStreams { get { return 0; } }

        public override int CurrentVideoStream
        {
            get { return 0; }
            set { }
        }

        public override VideoStream CurrentVideo { get { return null; } }

        public override VideoStream BestVideo { get { return null; } }

        public override int SubtitleStreams { get { return 0; } }

        public override int CurrentSubtitleStream 
        {
            get { return 0; }
            set { } 
        }
    }
}
