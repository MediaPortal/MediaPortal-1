using System;

namespace ProjectInfinity.MenuManager
{
  public interface IMenuCommand : IDisposable
  {
    void Run();
  }
}