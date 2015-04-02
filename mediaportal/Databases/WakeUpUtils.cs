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
using System.Net;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace MediaPortal.Database
{
  public class WakeupUtil
  {
    public static bool HandleWakeUpServer(string hostName)
    {
      String macAddress;
      byte[] hwAddress;
      int wolTimeout;
      WakeOnLanManager wakeOnLanManager = new WakeOnLanManager();

      IPAddress ipAddress = null;

      using (Settings xmlreader = new MPSettings())
      {
        macAddress = xmlreader.GetValueAsString("macAddress", hostName, null);
        wolTimeout = xmlreader.GetValueAsInt("WOL", "WolTimeout", 10);
      }

      if (wakeOnLanManager.Ping(hostName, 100) && !string.IsNullOrEmpty(macAddress))
      {
        Log.Debug("WakeUpServer: The {0} server already started and mac address is learnt!", hostName);
        return true;
      }

      // Check if we already have a valid IP address stored,
      // otherwise try to resolve the IP address
      if (!IPAddress.TryParse(hostName, out ipAddress) && string.IsNullOrEmpty(macAddress))
      {
        // Get IP address of the server
        try
        {
          IPAddress[] ips;

          ips = Dns.GetHostAddresses(hostName);

          Log.Debug("WakeUpServer: WOL - GetHostAddresses({0}) returns:", hostName);

          foreach (IPAddress ip in ips)
          {
            Log.Debug("    {0}", ip);

            ipAddress = ip;
            // Check for valid IP address
            if (ipAddress != null)
            {
              // Update the MAC address if possible
              hwAddress = wakeOnLanManager.GetHardwareAddress(ipAddress);

              if (wakeOnLanManager.IsValidEthernetAddress(hwAddress))
              {
                Log.Debug("WakeUpServer: WOL - Valid auto MAC address: {0:x}:{1:x}:{2:x}:{3:x}:{4:x}:{5:x}"
                          , hwAddress[0], hwAddress[1], hwAddress[2], hwAddress[3], hwAddress[4], hwAddress[5]);

                // Store MAC address
                macAddress = BitConverter.ToString(hwAddress).Replace("-", ":");

                Log.Debug("WakeUpServer: WOL - Store MAC address: {0}", macAddress);

                using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.MPSettings())
                {
                  xmlwriter.SetValue("macAddress", hostName, macAddress);
                }
              }
              else
              {
                Log.Debug("WakeUpServer: WOL - Not a valid IPv4 address: {0}", ipAddress);
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("WakeUpServer: WOL - Failed GetHostAddress - {0}", ex.Message);
        }
      }

      Log.Debug("WakeUpServer: WOL - Use stored MAC address: {0}", macAddress);

      try
      {
        hwAddress = wakeOnLanManager.GetHwAddrBytes(macAddress);

        // Finally, start up the server
        Log.Info("WakeUpServer: WOL - Start the {0} server", hostName);

        if (WakeupSystem(hwAddress, hostName, wolTimeout))
        {
          Log.Info("WakeUpServer: WOL - The {0} server started successfully!", hostName);
          return true;
        }
        else
        {
          Log.Error("WakeUpServer: WOL - Failed to start the {0} server", hostName);
        }
      }
      catch (Exception ex)
      {
        Log.Error("WakeUpServer: WOL - Failed to start the server - {0}", ex.Message);
      }
      return false;
    }

    private static bool WakeupSystem(byte[] hwAddress, string wakeupTarget, int timeout)
    {
      int waited = 0;

      WakeOnLanManager wakeOnLanManager = new WakeOnLanManager();

      Log.Debug("WOLMgr: Ping {0}", wakeupTarget);
      if (wakeOnLanManager.Ping(wakeupTarget, 200))
      {
        Log.Debug("WOLMgr: {0} already started", wakeupTarget);
        return true;
      }

      // First, try to send WOL Packet
      if (!wakeOnLanManager.SendWakeOnLanPacket(hwAddress, IPAddress.Broadcast))
      {
        Log.Debug("WOLMgr: FAILED to send the first wake-on-lan packet!");
      }

      int waittime;
      using (Settings xmlreader = new MPSettings())
      {
        waittime = xmlreader.GetValueAsInt("WOL", "WaitTimeAfterWOL", 0);
      }

      while (waited < timeout)
      {
        Log.Debug("WOLMgr: Ping {0}", wakeupTarget);
        if (wakeOnLanManager.Ping(wakeupTarget, 200))
        {
          if (waittime > 0)
          {
            System.Threading.Thread.Sleep(1000 * waittime);
          }
          return true;
        }
        // Send WOL Packet
        if (!wakeOnLanManager.SendWakeOnLanPacket(hwAddress, IPAddress.Broadcast))
        {
          Log.Debug("WOLMgr: Sending the wake-on-lan packet failed (local network maybe not ready)! {0}s", waited);
        }
        Log.Debug("WOLMgr: System {0} still not reachable, waiting... {1}s", wakeupTarget, waited);

        System.Threading.Thread.Sleep(1000);
        waited++;
      }

      // Timeout was reached and WOL packet can't be send (we stop here)
      Log.Debug("WOLMgr: FAILED to send wake-on-lan packet after the timeout {0}, try increase the value!", timeout);

      return false;
    }
  }
}
