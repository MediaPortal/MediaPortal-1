using ProjectInfinity.Messaging;

namespace ProjectInfinity.Music
{
  /// <summary>
  /// Arguments for the MusicStart message
  /// </summary>
  public class MusicStartMessage : Message
  {
    private string _artist;
    private string _album;
    private int _trackNo;
    private string _title;

    public string Artist
    {
      get { return _artist; }
      set { _artist = value; }
    }

    public string Album
    {
      get { return _album; }
      set { _album = value; }
    }

    public int TrackNo
    {
      get { return _trackNo; }
      set { _trackNo = value; }
    }

    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }

    public override string ToString()
    {
      return string.Format("Artist={0},Title={1},TrackNo={2},Album={3}", _artist, _title, _trackNo, _album);
    }
  }
}