#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

#endregion

namespace TvPlugin
{
  public class WakeOnLanManager
  {
    #region Constants

    // Maximum length of a physical address
    private const int PHYSADDR_MAXLEN = 8;
    // Insufficient buffer error
    private const int INSUFFICIENT_BUFFER = 122;

    #endregion

    #region Structs

    // Define the MIB_IPNETROW structure.
    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_IPNETROW
    {
      [MarshalAs(UnmanagedType.U4)] public int dwIndex;
      [MarshalAs(UnmanagedType.U4)] public int dwPhysAddrLen;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = PHYSADDR_MAXLEN)] public byte[] bPhysAddr;
      [MarshalAs(UnmanagedType.U4)] public int dwAddr;
      [MarshalAs(UnmanagedType.U4)] public int dwType;
    }

    #endregion

    #region External methods

    [DllImport("iphlpapi.dll")]
    [return: MarshalAs(UnmanagedType.U4)]
    private static extern int GetIpNetTable(IntPtr ipNetTable, [MarshalAs(UnmanagedType.U4)] ref int size, bool order);

    [DllImport("iphlpapi.dll")]
    [return: MarshalAs(UnmanagedType.U4)]
    private static extern int SendARP(int destIP, int srcIP, [Out] byte[] macAddr, ref int phyAddrLen);

    #endregion

    #region Private (lowlevel) methods

    private MIB_IPNETROW[] GetPhysicalAddressTable()
    {
      int bytesNeeded = 0;
      int result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);

      if (result != INSUFFICIENT_BUFFER)
      {
        throw new ApplicationException(Convert.ToString(result));
      }

      IntPtr buffer = IntPtr.Zero;
      MIB_IPNETROW[] table;

      try
      {
        buffer = Marshal.AllocCoTaskMem(bytesNeeded);

        result = GetIpNetTable(buffer, ref bytesNeeded, false);

        if (result != 0)
        {
          throw new ApplicationException(Convert.ToString(result));
        }

        int entries = Marshal.ReadInt32(buffer);
        IntPtr currentBuffer = new IntPtr(buffer.ToInt64() + sizeof (int));
        table = new MIB_IPNETROW[entries];

        for (int i = 0; i < entries; i++)
        {
          table[i] = (MIB_IPNETROW)Marshal.PtrToStructure(
                                     new IntPtr(currentBuffer.ToInt64() + (i * Marshal.SizeOf(typeof (MIB_IPNETROW)))),
                                     typeof (MIB_IPNETROW)
                                     );
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }
      return table;
    }

    private byte[] SendArpRequest(IPAddress address)
    {
      byte[] hwAddr = new byte[6];
      int len = hwAddr.Length;
      byte[] ipAddr = address.GetAddressBytes();
      int result = SendARP(BitConverter.ToInt32(ipAddr, 0), 0, hwAddr, ref len);
      ipAddr = null;
      return hwAddr;
    }

    private byte[] GetWakeOnLanMagicPacket(byte[] hwAddress)
    {
      if (hwAddress.Length != 6)
      {
        throw new ArgumentOutOfRangeException("hwAddress", hwAddress, "hwAddress must contain 6 bytes!");
      }
      byte[] packet = new byte[102];
      // pad packet data with 6 0xFF bytes
      for (int i = 0; i < 6; i++)
      {
        packet[i] = 0xFF;
      }

      // write hwaddress (at least) 16 times to packet
      for (int i = 1; i < 17; i++)
      {
        for (int x = 0; x < 6; x++)
        {
          packet[i * 6 + x] = hwAddress[x];
        }
      }
      return packet;
    }

    private void SendMagicPacket(IPAddress address, byte[] data)
    {
      UdpClient client = new UdpClient();
      client.Connect(address, 1234);
      client.Send(data, data.Length);
      client.Close();
      client = null;
    }

    /// <summary>
    /// Sends an AMD "magic" packet for the given hardware ethernet address, with ipAddress as the target.
    /// </summary>
    /// <param name="hwAddress">hardware ethernet address to wake up</param>
    /// <param name="ipAddress">IP address to use as target</param>
    /// <returns>bool indicating if the packet was sent successfully</returns>
    private bool SendWakeOnLanPacket(byte[] hwAddress, IPAddress ipAddress)
    {
      if (IsValidEthernetAddress(hwAddress))
      {
        byte[] magicPacket = GetWakeOnLanMagicPacket(hwAddress);
        SendMagicPacket(ipAddress, magicPacket);
        return true;
      }
      else
      {
        Log.Debug("WOLMgr: Invalid ethernet address!");
        return false;
      }
    }

    private bool Ping(string hostName, int timeout)
    {
      Ping p = new Ping();
      try
      {
        PingReply r = p.Send(hostName, timeout);
        if (r.Status == IPStatus.Success)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("WOLMgr: Ping failed - {0}", ex.Message);
      }

      return false;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Gets the hardware ethernet address of the given IP address as an array of bytes. The address is searched
    /// for in the ARP table cache and (if not found there) and ARP request is transmitted to find out the address.
    /// </summary>
    /// <param name="address">IP address to find the hardware ethernet address for</param>
    /// <returns>byte[] containing the hardware ethernet address</returns>
    public byte[] GetHardwareAddress(IPAddress address)
    {
      // Try fetching h/w ethernet address from the ARP table
      MIB_IPNETROW[] addrTable = GetPhysicalAddressTable();
      if (addrTable != null)
      {
        for (int i = 0; i < addrTable.Length; i++)
        {
          byte[] addr = address.GetAddressBytes();
          if (BitConverter.ToInt32(addr, 0) == addrTable[i].dwAddr)
          {
            byte[] physAddr = new byte[addrTable[i].dwPhysAddrLen];
            Array.Copy(addrTable[i].bPhysAddr, physAddr, addrTable[i].dwPhysAddrLen);
            return physAddr;
          }
        }
      }
      // Try sending an ARP request specifically for this address
      return SendArpRequest(address);
    }

    /// <summary>
    /// Checks whether the given address is a valid hardware ethernet address
    /// </summary>
    /// <param name="hwAddress">hardware ethernet address to check</param>
    /// <returns>bool indicating if the given address is valid</returns>
    public bool IsValidEthernetAddress(byte[] hwAddress)
    {
      if (hwAddress == null)
      {
        return false;
      }

      if (hwAddress.Length != 6)
      {
        return false;
      }

      bool valid = false;
      for (int i = 0; i < hwAddress.Length; i++)
      {
        if (hwAddress[i] != 0x00)
        {
          valid = true;
          break;
        }
      }
      return valid;
    }

    /// <summary>
    /// Wakes up the given target system by sending a wake-on-lan packet for the specified hardware ethernet address.
    /// The packet is broadcasted to the general broadcast address (255.255.255.255). After the wake-on-lan packet is sent,
    /// the system invokes ping requests to the wakeupTarget to verify that the host is actually resumed until the
    /// given timeout has been reached.
    /// </summary>
    /// <param name="hwAddress">byte[] containing the hardware ethernet address to wakeup</param>
    /// <param name="wakeupTarget">Hostname of the system to wake up. Used to verify the system has resumed.</param>
    /// <param name="timeout">timeout (in seconds) for system resume verification</param>
    /// <returns>bool indication whether or not the system is available</returns>
    public bool WakeupSystem(byte[] hwAddress, string wakeupTarget, int timeout)
    {
      int waited = 0;

      // we have to make sure the remoting system knows that we have resumed the server by means of WOL.
      // this will make sure the connection timeout for the remoting framework is increased.
      Log.Debug("WOLMgr: Increasing timeout for RemoteControl");
      TvControl.RemoteControl.UseIncreasedTimeoutForInitialConnection = true;

      Log.Debug("WOLMgr: Ping {0}", wakeupTarget);
      if (Ping(wakeupTarget, timeout))
      {
        Log.Debug("WOLMgr: {0} already started", wakeupTarget);
        return true;
      }

      if (!SendWakeOnLanPacket(hwAddress, IPAddress.Broadcast))
      {
        Log.Debug("WOLMgr: FAILED to send wake-on-lan packet!");
        return false;
      }

      while (waited < timeout * 1000)
      {
        Log.Debug("WOLMgr: Ping {0}", wakeupTarget);
        if (Ping(wakeupTarget, 1000))
        {
          return true;
        }
        Log.Debug("WOLMgr: System {0} still not reachable, waiting...", wakeupTarget);
        System.Threading.Thread.Sleep(1000);
        waited += 2000;
      }
      return false;
    }

    /// <summary>
    /// Convert the given hardware ethernet address into a byte[]
    /// </summary>
    /// <param name="address">string containing the hardware ethernet address to convert (00:01:02:03:04:05)</param>
    /// <returns>byte[] containing the byte representation of this hardware ethernet address</returns>
    public byte[] GetHwAddrBytes(string address)
    {
      byte[] addrn = new byte[6];
      string[] addr = address.Split(':');
      if (addr.Length != 6)
      {
        throw new ArgumentOutOfRangeException("address", address, "not a valid hardware ethernet addresss");
      }
      try
      {
        for (int i = 0; i < addr.Length; i++)
        {
          addrn[i] = byte.Parse(addr[i], System.Globalization.NumberStyles.HexNumber);
        }

        return addrn;
      }
      catch (FormatException)
      {
        throw new ArgumentOutOfRangeException("address", address, "not a valid hardware ethernet addresss");
      }
    }
  }
}

#endregion