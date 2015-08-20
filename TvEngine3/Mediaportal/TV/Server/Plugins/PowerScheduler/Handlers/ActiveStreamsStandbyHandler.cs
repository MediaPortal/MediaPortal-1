#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings

using System.Collections.Generic;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces;

#endregion

namespace Mediaportal.TV.Server.Plugins.PowerScheduler.Handlers
{
  /// <summary>
  /// Prevent standby when the TVController has active streaming clients
  /// </summary>
  public class ActiveStreamsStandbyHandler : IStandbyHandler
  {
    #region Variables

    private readonly IInternalControllerService _controllerService;

    #endregion

    #region Constructor

    public ActiveStreamsStandbyHandler(IInternalControllerService controllerService)
    {
      _controllerService = controllerService;
    }

    #endregion

    #region IStandbyHandler implementation

    public bool DisAllowShutdown
    {
      get
      {
        ICollection<RtspClient> clients = _controllerService.StreamingClients;
        return clients != null && clients.Count > 0;
      }
    }

    public void UserShutdownNow() {}

    public string HandlerName
    {
      get { return "Active Streams"; }
    }

    #endregion
  }
}