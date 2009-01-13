using System;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public interface ISession : IDisposable
  {
    void Process();
  }
}