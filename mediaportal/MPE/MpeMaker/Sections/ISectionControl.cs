using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Classes;
using MpeCore;

namespace MpeMaker.Sections
{
  public interface ISectionControl
  {
    void Set(PackageClass pak);
    PackageClass Get();
  }
}