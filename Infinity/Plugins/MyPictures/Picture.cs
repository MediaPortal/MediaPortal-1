using System;
using System.IO;

namespace ProjectInfinity.Pictures
{
  public class Picture : MediaItem
  {
    private FileInfo _info;

    public Picture(FileInfo path) : base(path.FullName)
    {
      _info = path;
    }

    public override string Name
    {
      get { return _info.Name; }
    }

    public Uri Uri
    {
      get
      {
        return new Uri(@"file://"+_info.FullName.Replace('\\','/'));
      }
    }

    public override void Accept(IMediaVisitor visitor)
    {
      visitor.Visit(this);
    }

  }
}