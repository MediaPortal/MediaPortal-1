#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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


using System;
using System.Collections;
using System.Text;

using TvControl;          // include the tvserver remote control interfaces
using TvLibrary.Channels; // include tv-channel types
using TvDatabase;         // include tv-server database 

namespace Example2
{
  /// <summary>
  /// example which connects to the tvserver 
  /// and starts timeshifting then waits 5 seconds and then stops timeshifting.
  /// </summary>
  class Program
  {
    static void Main(string[] args)
    {
      //set the hostname of the tvserver
      RemoteControl.HostName = "localhost";

      //get the location of the database..
      string databaseConnection = RemoteControl.Instance.DatabaseConnectionString;

      //set the connection string
      Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(databaseConnection);

      // Now get a list of all tv-channels
      IList channels = Channel.ListAll();
      
      //lets take the first channel
      Channel channel = (Channel)channels[0];
      Console.WriteLine("timeshifting channel:{0}", channel.Name);

      //start timeshifting
      TvServer server = new TvServer();
      VirtualCard card;
      TvResult result=server.StartTimeShifting(channel.Name, out card);
      if (result != TvResult.Succeeded)
      {
        //failed to start timeshifting
        Console.WriteLine("timeshifting failed:{0}", result);
      }
      else
      {

        Console.WriteLine("timeshifting succeeded");
        Console.WriteLine("  rtsp url:{0}", card.RTSPUrl);
        Console.WriteLine("  filename:{0}", card.TimeShiftFileName);

        //sleep 5 secs
        System.Threading.Thread.Sleep(5000);

        //stop timeshifting
        card.StopTimeShifting();
      }
    }
  }
}
