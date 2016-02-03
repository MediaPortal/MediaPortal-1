#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Linq;

using DirectShowLib;
using DShowNET.Helper;

using MediaPortal.Player.MediaInfo;

namespace MediaPortal.Player
{
    public abstract class BaseDirectShowVideoPlayer : IPlayer
    {
        private readonly List<StreamInfo> _audioStreams;
        private readonly List<StreamInfo> _videoStreams;
        private int _currentAudioStream;
        private int _currentVideoStream;
        protected IGraphBuilder _graphBuilder = null;

        protected MediaInfoWrapper MediaInfo { get; set; }

        protected DirectShowHelper DirectShowHelper { get; private set; }

        protected BaseDirectShowVideoPlayer()
        {
            DirectShowHelper = new DirectShowHelper(StoreStream);
            _audioStreams = new List<StreamInfo>();
            _videoStreams = new List<StreamInfo>();
        }

        public override int AudioStreams
        {
            get { return _audioStreams.Count; }
        }

        public override int CurrentAudioStream
        {
            get { return _currentAudioStream; }
            set
            {
                if (_currentAudioStream != value)
                {
                    if (value < AudioStreams)
                    {
                        _currentAudioStream = value;
                        var info = _audioStreams[value];
                        EnableStream(info.Id, 0, info.Filter);
                        EnableStream(info.Id, AMStreamSelectEnableFlags.Enable, info.Filter);
                    }
                }
            }
        }

        public override AudioStream BestAudio
        {
            get { return MediaInfo != null ? MediaInfo.BestAudioStream : null; }
        }

        public override AudioStream CurrentAudio
        {
            get { return CurrentAudioStream < _audioStreams.Count ? _audioStreams[CurrentAudioStream].Stream as AudioStream : null; }
        }

        public override int VideoStreams
        {
            get { return _videoStreams.Count; }
        }

        public override int CurrentVideoStream
        {
            get { return _currentVideoStream; }
            set
            {
                if (_currentVideoStream != value)
                {
                    if (value < VideoStreams)
                    {
                        _currentVideoStream = value;
                        var info = _videoStreams[value];
                        EnableStream(info.Id, 0, info.Filter);
                        EnableStream(info.Id, AMStreamSelectEnableFlags.Enable, info.Filter);
                    }
                }
            }
        }

        public override VideoStream BestVideo
        {
            get { return MediaInfo != null ? MediaInfo.BestVideoStream : null; }
        }

        public override VideoStream CurrentVideo
        {
            get { return CurrentVideoStream < _videoStreams.Count ? _videoStreams[CurrentVideoStream].Stream as VideoStream : null; }
        }

        protected void ClearStreams()
        {
            MediaInfo = null;
            _audioStreams.Clear();
            _videoStreams.Clear();
        }

        private void StoreStream(string filterName, string name, int lcid, int id, DirectShowHelper.StreamType type, AMStreamSelectInfoFlags flag, IAMStreamSelect pStrm)
        {
            switch (type)
            {
                case DirectShowHelper.StreamType.Audio:
                    AnalyzeAudioStream(filterName, name, lcid, id);
                    break;
                case DirectShowHelper.StreamType.Video:
                    AnalyzeVideoStream(filterName, name, lcid, id);
                    break;
            }
        }

        private void AnalyzeAudioStream(string filterName, string name, int lcid, int id)
        {
            var stream = DirectShowHelper.MatchAudioStream(MediaInfo, filterName, name, lcid, id);
            var mediaStream = _audioStreams.FirstOrDefault(x => x.Id == id);
            if (mediaStream == null)
            {
                _audioStreams.Add(new StreamInfo { Filter = filterName, Id = id, Stream = stream });
            }
            else
            {
                mediaStream.Filter = filterName;
                mediaStream.Stream = stream;
            }
        }

        private void AnalyzeVideoStream(string filterName, string name, int lcid, int id)
        {
            var stream = DirectShowHelper.MatchVideoStream(MediaInfo, filterName, name, lcid, id);
            var mediaStream = _videoStreams.FirstOrDefault(x => x.Id == id);
            if (mediaStream == null)
            {
                _videoStreams.Add(new StreamInfo { Filter = filterName, Id = id, Stream = stream });
            }
            else
            {
                mediaStream.Filter = filterName;
                mediaStream.Stream = stream;
            }
        }

        protected bool EnableStream(int Id, AMStreamSelectEnableFlags dwFlags, string Filter)
        {
            try
            {
                var foundfilter = DirectShowUtil.GetFilterByName(_graphBuilder, Filter);
                if (foundfilter != null)
                {
                    var pStrm = foundfilter as IAMStreamSelect;
                    if (pStrm != null)
                    {
                        pStrm.Enable(Id, dwFlags);
                    }
                    DirectShowUtil.ReleaseComObject(foundfilter);
                }
            }
            catch { }
            return true;
        }

        protected void AddCustomVideoStream(VideoStream stream, int id, string filterName)
        {
            _videoStreams.Add(new StreamInfo { Filter = filterName, Id = id, Stream = stream });
        }

        protected void AddCustomAudioStream(AudioStream stream, int id, string filterName)
        {
            _audioStreams.Add(new StreamInfo { Filter = filterName, Id = id, Stream = stream });
        }

        private class StreamInfo
        {
            public MediaStream Stream { get; set; }

            public int Id { get; set; }

            public string Filter { get; set; }
        }
    }
}