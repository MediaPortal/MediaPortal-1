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

using System;
using TvEngine.PowerScheduler.Interfaces;

#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Wakeup handler for the IPowerController interface
  /// </summary>
  public class PowerControllerWakeupHandler : IWakeupHandler
  {
    #region Variables

    private DateTime _nextWakeupTime = DateTime.MaxValue;
    private string _handlerName = "PowerController";

    #endregion

    #region Public methods

    public void Update(DateTime nextWakeuptime, string handlerName)
    {
      _nextWakeupTime = nextWakeuptime;
      _handlerName = "PowerController (" + handlerName + ")";
    }

    #endregion

    #region IWakeupHandler implementation

    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      return _nextWakeupTime;
    }

    public string HandlerName
    {
      get { return _handlerName; }
    }

    #endregion
  }
}