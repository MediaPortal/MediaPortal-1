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
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TvEngine.PowerScheduler.Interfaces;
using TvDatabase;
using TvLibrary.Log;
#endregion

namespace TvEngine.PowerScheduler.Handlers
{
  /// <summary>
  /// Generic IWakeupHandler implementation, can be reused or inherited
  /// </summary>
  public class XmlTvImportWakeupHandler : IWakeupHandler
  {
    #region Variables    
    private string _handlerName = "XmlTvImportWakeupHandler";
    #endregion

    #region Events   
    #endregion

    #region Constructor
    public XmlTvImportWakeupHandler()
    {            
    }
    #endregion

    #region Private methods

    #endregion

    #region Public methods

    #endregion

    #region IWakeupHandler implementation
    [MethodImpl(MethodImplOptions.Synchronized)]
    public DateTime GetNextWakeupTime(DateTime earliestWakeupTime)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      bool remoteSchedulerEnabled = (layer.GetSetting("xmlTvRemoteSchedulerEnabled", "false").Value == "true");
      if (!remoteSchedulerEnabled)
      {
        return DateTime.MaxValue;
      }

      DateTime now = DateTime.Now;      
      DateTime defaultRemoteScheduleTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
      string remoteScheduleTimeStr = layer.GetSetting("xmlTvRemoteScheduleTime", defaultRemoteScheduleTime.ToString()).Value;

      DateTime remoteScheduleTime = (DateTime)(System.ComponentModel.TypeDescriptor.GetConverter(new DateTime(now.Year, now.Month, now.Day)).ConvertFrom(remoteScheduleTimeStr));

      if (remoteScheduleTime == DateTime.MinValue)
      {
        remoteScheduleTime = defaultRemoteScheduleTime;
      }

      if ((now < remoteScheduleTime) && (remoteScheduleTime > DateTime.MinValue))
      {
        remoteScheduleTime.AddDays(1);
      }

      Log.Debug(this._handlerName + ".GetNextWakeupTime {0}", remoteScheduleTime);

      remoteScheduleTime.AddMinutes(-1); // resume 60sec before      

      return remoteScheduleTime;
    }
    public string HandlerName
    {
      get { return _handlerName; }
    }
    #endregion   
  }
}
