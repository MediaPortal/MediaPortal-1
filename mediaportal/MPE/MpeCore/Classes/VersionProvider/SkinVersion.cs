using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MpeCore.Interfaces;

namespace MpeCore.Classes.VersionProvider
{
  public class SkinVersion : VersionProvider
  {
    public override string DisplayName
    {
      get { return "Skin"; }
    }

    public override VersionInfo Version(string id)
    {
      return new VersionInfo(MediaPortal.Common.Utils.CompatibilityManager.SkinVersion);
    }
  }
}
