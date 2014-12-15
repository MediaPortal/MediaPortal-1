#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;


namespace WatchDogService
{
  public partial class WatchDogService : ServiceBase
  {
    EventLog _eventLog = new EventLog();
    HttpChannel _httpChannel;

    public WatchDogService()
    {
      InitializeComponent();
      _eventLog.Source = "WatchDogService";
    }

    protected override void OnStart(string[] args)
    {
      try
      {
        _httpChannel = new HttpChannel(9997);
        ChannelServices.RegisterChannel(_httpChannel, false);
      }
      catch (Exception ex)
      {
        _eventLog.WriteEntry("WatchDogService exception" + ex, EventLogEntryType.Error);
      }

      try
      {
        RemotingConfiguration.RegisterWellKnownServiceType(typeof(WatchDogServer), "WatchDogServer", WellKnownObjectMode.SingleCall);
      }
      catch (Exception ex)
      {
        _eventLog.WriteEntry("WatchDogService exception" + ex, EventLogEntryType.Error);
      }
    }

    protected override void OnStop()
    {
      try
      {
        ChannelServices.UnregisterChannel(_httpChannel);
      }
      catch (Exception ex)
      {
      }
    }

  }
}
