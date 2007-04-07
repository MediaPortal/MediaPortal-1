using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyVideos
{
    class VideoPlayerCollection
    {
        /*private static VideoPlayerCollection _instance = null;
        private List<VideoPlayerCollection> _players = new List<VideoPlayerCollection>();

        /// <summary>
        /// Gets a list of created instances of the VideoPlayer class.
        /// </summary>
        public static VideoPlayerCollection Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new VideoPlayerCollection();

                return _instance;
            }
        }

        /// <summary>
        /// Creates a new instance of VideoPlayer.
        /// </summary>
        /// <param name="fileName">File path to the object to add to the player.</param>
        /// <returns>The VideoPlayer instance.</returns>
        public VideoPlayer Get(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Dialogs.MpDialogOk md = new Dialogs.MpDialogOk();
                md.Header = "Error";
                md.Content = "File not found.";
                md.ShowDialog();
                return null;
            }

            Uri uri = new Uri(fileName, UriKind.Absolute);
            VideoPlayer player = new VideoPlayer(fileName);
            player.Open(uri);
            _players.Add(player);
            return player;
        }

        /// <summary>
        /// Releases an player from the collection.
        /// </summary>
        /// <param name="player">The VideoPlayer to release/dispose</param>
        public void Release(VideoPlayer player)
        {
            _players.Remove(player);
        }

        /// <summary>
        /// Gets the VideoPlayer at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VideoPlayer this[int index]
        {
            get { return _players[index]; }
        }

        /// <summary>
        /// Gets the number of active players
        /// </summary>
        public int Count
        {
            get { return _players.Count; }
        }

        /// <summary>
        /// Disposes all players
        /// </summary>
        public void Dispose()
        {
            List<VideoPlayer> players = new List<VideoPlayer>();

            foreach (VideoPlayer player in _players)
                players.Add(player);

            foreach (VideoPlayer player in players)
                player.Dispose();
        }

        /// <summary>
        /// Disposes a specified player
        /// </summary>
        /// <param name="player"></param>
        public void Dispose(VideoPlayer player)
        {
            player.Dispose();
        }*/
    }
}
