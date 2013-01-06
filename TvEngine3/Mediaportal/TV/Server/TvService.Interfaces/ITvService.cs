using System.Runtime.CompilerServices;
using Mediaportal.TV.Server.TVService.Interfaces.PowerEvents;

namespace Mediaportal.TV.Server.TVService.Interfaces
{
  public interface ITvService
  {
    [MethodImpl(MethodImplOptions.Synchronized)]
    void AddPowerEventHandler(PowerEventHandler handler);

    [MethodImpl(MethodImplOptions.Synchronized)]
    void RemovePowerEventHandler(PowerEventHandler handler);

    void Start();
    void Stop(int maxWaitMsecs);
  }
}