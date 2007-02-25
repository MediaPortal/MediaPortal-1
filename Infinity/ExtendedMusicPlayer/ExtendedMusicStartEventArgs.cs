namespace ProjectInfinity.Music
{
    public class ExtendedMusicStartEventArgs : MusicStartEventArgs
    {
        private int _rating;

        public int Rating
        {
            get { return _rating; }
            set { _rating = value; }
        }
    }
}