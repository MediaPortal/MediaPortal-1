#region Copyright (C) 2007-2009 Team MediaPortal
/* 
 *	Copyright (C) 2007-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

#region Usings

using System.Collections.Generic;
using TvControl;
using TvLibrary.Interfaces;
using TvEngine.PowerScheduler.Handlers;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace TvEngine.PowerScheduler
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
    List<IStandbyHandler> _standbyHandlers;
    /// <summary>
    /// List of all wakeup handlers
    /// </summary>
    List<IWakeupHandler> _wakeupHandlers;
    /// <summary>
    /// Controls standby/wakeup of system for EPG grabbing
    /// </summary>
    EpgGrabbingHandler _epgHandler;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new PowerSchedulerFactory
    /// </summary>
    /// <param name="controller">Reference to tvservice's TVController</param>
    public PowerSchedulerFactory(IController controller)
    {
      IStandbyHandler standbyHandler;

      _standbyHandlers = new List<IStandbyHandler>();
      _wakeupHandlers = new List<IWakeupHandler>();

      // Add handlers for preventing the system from entering standby
      standbyHandler = new ActiveStreamsHandler(controller);
      _standbyHandlers.Add(standbyHandler);
      standbyHandler = new ControllerActiveHandler(controller);
      _standbyHandlers.Add(standbyHandler);
      standbyHandler = new ProcessActiveHandler();
      _standbyHandlers.Add(standbyHandler);

      ScheduledRecordingsHandler recHandler = new ScheduledRecordingsHandler();

      // Add handlers for resuming from standby
      _wakeupHandlers.Add(recHandler);

      // Activate the EPG grabbing handler
      _epgHandler = new EpgGrabbingHandler(controller);
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
