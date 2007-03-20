using System;

namespace ProjectInfinity.Plugins
{
  public interface IPlugin : IDisposable
  {
    void Initialize();
  }
}