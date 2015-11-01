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
using Mediaportal.TV.Server.Plugins.PowerScheduler.Handlers;
using Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

#endregion

namespace Mediaportal.TV.Server.Plugins.PowerScheduler
{
  /// <summary>
  /// Factory for creating various IStandbyHandlers/IWakeupHandlers
  /// </summary>
  public class PowerSchedulerFactory
  {
    #region Variables

    /// <summary>
    /// List of all standby handlers
    /// </summary>
    private List<IStandbyHandler> _standbyHandlers;

    /// <summary>
    /// List of all wakeup handlers
    /// </summary>
    private List<IWakeupHandler> _wakeupHandlers;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new PowerSchedulerFactory
    /// </summary>
    /// <param name="controllerService">Reference to tvservice's TVController</param>
    public PowerSchedulerFactory(IInternalControllerService controllerService)
    {
      this.LogInfo("PS: PowerSchedulerFactory");

      // Add handlers for preventing the system from entering standby
      _standbyHandlers = new List<IStandbyHandler>();
      _standbyHandlers.Add(new ActiveStreamsStandbyHandler(controllerService));
      _standbyHandlers.Add(new ControllerActiveStandbyHandler(controllerService));
      _standbyHandlers.Add(new ProcessActiveStandbyHandler());
      _standbyHandlers.Add(new ActiveNetworkStandbyHandler());
      _standbyHandlers.Add(new ActiveSharesStandbyHandler());
      _standbyHandlers.Add(new PingStandbyHandler());
      _standbyHandlers.Add(new ActivePortsHandler());

      // Add handlers for resuming from standby
      _wakeupHandlers = new List<IWakeupHandler>();
      _wakeupHandlers.Add(new ScheduledRecordingsWakeupHandler(controllerService));
      _wakeupHandlers.Add(new XmlTvImportWakeupHandler());

      // Activate the handlers which register/unregister themselves dynamically
      IWakeupHandler wakeupHandler = new EpgGrabbingStandbyWakeupHandler(controllerService);
      wakeupHandler = new RebootWakeupHandler();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Create/register the default set of standby/wakeup handlers
    /// </summary>
    public void CreateDefaultSet()
    {
      IPowerScheduler powerScheduler = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      foreach (IStandbyHandler handler in _standbyHandlers)
        powerScheduler.Register(handler);
      foreach (IWakeupHandler handler in _wakeupHandlers)
        powerScheduler.Register(handler);
    }

    /// <summary>
    /// Unregister the default set of standby/wakeup handlers
    /// </summary>
    public void RemoveDefaultSet()
    {
      IPowerScheduler powerScheduler = GlobalServiceProvider.Instance.Get<IPowerScheduler>();
      foreach (IStandbyHandler handler in _standbyHandlers)
        powerScheduler.Unregister(handler);
      foreach (IWakeupHandler handler in _wakeupHandlers)
        powerScheduler.Unregister(handler);
    }

    #endregion
  }
}