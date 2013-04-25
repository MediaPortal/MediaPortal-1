using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MpeCore.Interfaces;

namespace MpeCore.Classes.VersionProvider
{
  public class SkinVersion : IVersionProvider
  {
    public string DisplayName
    {
      get { return "Skin"; }
    }

    public bool Validate(DependencyItem componentItem)
    {
      if (componentItem.MinVersion.CompareTo(Version(componentItem.Id)) <= 0 &&
          componentItem.MaxVersion.CompareTo(Version(componentItem.Id)) >= 0)
        return true;
      return false;
    }

    public VersionInfo Version(string id)
    {
      return new VersionInfo(MediaPortal.Common.Utils.CompatibilityManager.SkinVersion);
    }
  }
}
