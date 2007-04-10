using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectInfinity.Pictures
{
  public class ParentFolder : Folder
  {
    public ParentFolder(DirectoryInfo directoryInfo)
      : base(directoryInfo)
    {
    }

    public override string Name
    {
      get { return ".."; }
    }
  }
}
