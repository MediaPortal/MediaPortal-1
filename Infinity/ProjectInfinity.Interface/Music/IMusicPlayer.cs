using System;
using ProjectInfinity.Messaging;
using ProjectInfinity.Plugins;

namespace ProjectInfinity.Music
{
    /// <summary>
    /// Interface that all music players need to implement.
    /// </summary>
    public interface IMusicPlayer : IPlugin
    {
        /// <summary>
        /// Start playing the given file.
        /// </summary>
        /// <param name="file">the file to play</param>
        void Play(string file);

        /// <summary>
        /// Stops playback
        /// </summary>
        void Stop();

        /// <summary>
        /// Message to broadcast when playback has started
        /// </summary>
        [MessagePublication(SystemMessages.MusicStart)]
        event EventHandler<MusicStartEventArgs> MusicStart;

        /// <summary>
        /// Message to broadcast when playback has stopped
        /// </summary>
        [MessagePublication(SystemMessages.MusicStop)]
        event EventHandler MusicStop;
    }
}