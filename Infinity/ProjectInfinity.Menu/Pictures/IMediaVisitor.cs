using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Pictures
{
  public interface IMediaVisitor
  {
    void Visit(Folder folder);
    void Visit(Picture picture);
  }
}
