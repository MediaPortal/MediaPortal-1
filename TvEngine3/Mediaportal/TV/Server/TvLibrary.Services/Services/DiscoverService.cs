using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Services
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
