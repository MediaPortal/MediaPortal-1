namespace ProjectInfinity.Pictures
{
  public abstract class MediaItem
  {
    private readonly string _path;

    public MediaItem(string path)
    {
      _path = path;
    }

    public string Path
    {
      get { return _path; }
    }

    public abstract string Name { get; }

    public abstract void Accept(IMediaVisitor visitor);
  }
}