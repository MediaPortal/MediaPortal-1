using MpeCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MpeCore.Classes.VersionProvider
{
  public abstract class VersionProvider : IVersionProvider
  {
    public virtual bool Validate(DependencyItem dependency)
    {
      var version = Version(dependency.Id);
      return (version >= dependency.MinVersion && version <= dependency.MaxVersion);
    }

    public abstract string DisplayName { get; }

    public abstract VersionInfo Version(string id);
  }
}
