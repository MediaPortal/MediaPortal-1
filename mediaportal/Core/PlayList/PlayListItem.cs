using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.TagReader;

namespace MediaPortal.Playlists
{
    [Serializable()]
    public class PlayListItem
    {
        public enum PlayListItemType
        {
            Unknown,
            Audio,
            Radio,
            AudioStream,
            VideoStream,
            Video,
            DVD,
            TV,
            Pictures
        }
        protected string _fileName = "";
        protected string _description = "";
        protected int _duration = 0;
        protected object _musicTag = null;
        bool _isPlayed = false;
        PlayListItemType _itemType = PlayListItemType.Unknown;

        public PlayListItem()
        {
        }

        public PlayListItem(string description, string fileName)
            : this(description, fileName, 0)
        {
        }

        public PlayListItem(string description, string fileName, int duration)
        {
            if (description == null)
                return;
            if (fileName == null)
                return;
            _description = description;
            _fileName = fileName;
            _duration = duration;
        }

        public PlayListItem.PlayListItemType Type
        {
            get { return _itemType; }
            set { _itemType = value; }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value == null)
                    return;
                _fileName = value;
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                if (value == null)
                    return;
                _description = value;
            }
        }
        public int Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }
        public bool Played
        {
            get { return _isPlayed; }
            set { _isPlayed = value; }
        }

        public object MusicTag
        {
            get { return _musicTag; }
            set { _musicTag = value; }
        }
    };

}
