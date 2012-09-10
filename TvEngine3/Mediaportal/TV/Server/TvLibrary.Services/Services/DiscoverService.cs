using System;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class DiscoverService : IDiscoverService
  {
    #region Implementation of IDiscoverService

    public DateTime Ping()
    {
      return DateTime.Now;
    }

    #endregion
  }
}
