using System.IO;

namespace ProjectInfinity.Pictures
{
  public class Folder : MediaItem
  {
    private DirectoryInfo _info;

    public Folder(DirectoryInfo path) : base(path.FullName)
    {
      _info = path;
    }

    public int ItemCount
    {
      get { return _info.GetFiles().Length; }
    }

    public override string Name
    {
      get { return _info.Name; }
    }

    public DirectoryInfo Info
    {
      get { return _info; }
    }

    public override void Accept(IMediaVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}