namespace ProjectInfinity.Pictures
{
  public abstract class MediaItem
  {
    private string _path;

    public MediaItem(string path)
    {
      _path = path;
    }

    public string Path
    {
      get { return _path; }
      set { _path = value; }
    }

    public abstract string Name { get; }

    public abstract void Accept(IMediaVisitor visitor);
  }
}