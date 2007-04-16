using System;

namespace ProjectInfinity.Plugins
{
  public interface IMenuCommand : IDisposable
  {
    void Run();
  }
}