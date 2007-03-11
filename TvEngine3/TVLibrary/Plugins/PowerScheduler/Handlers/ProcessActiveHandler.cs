#region Copyright (C) 2007 Team MediaPortal
/* 
 *	Copyright (C) 2007 Team MediaPortal
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using TvEngine.PowerScheduler.Interfaces;
#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Prevents standby when the given process is active
  /// </summary>
  public class ProcessActiveHandler : IStandbyHandler
  {
    #region Variables
    private string _processName = String.Empty;
    #endregion

    #region Constructor
    public ProcessActiveHandler(string processName)
    {
      _processName = processName;
    }
    #endregion

    #region IStandbyHandler implementation
    public bool DisAllowShutdown
    {
      get
      {
        Process[] processes = Process.GetProcessesByName("SetupTv");
        return (processes.Length > 0);
      }
    }
    public string HandlerName
    {
      get { return String.Format("ProcessActiveHandler:{0}", _processName); }
    }
    #endregion
  }
}
