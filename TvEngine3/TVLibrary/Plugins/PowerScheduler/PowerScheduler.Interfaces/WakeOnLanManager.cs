#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
#endregion

namespace TvEngine.PowerScheduler
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
    struct MIB_IPNETROW
    {
      [MarshalAs(UnmanagedType.U4)]
      public int dwIndex;
      [MarshalAs(UnmanagedType.U4)]
      public int dwPhysAddrLen;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = PHYSADDR_MAXLEN)]
      public byte[] bPhysAddr;
      [MarshalAs(UnmanagedType.U4)]
      public int dwAddr;
      [MarshalAs(UnmanagedType.U4)]
      public int dwType;
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
        IntPtr currentBuffer = new IntPtr(buffer.ToInt64() + sizeof(int));
        table = new MIB_IPNETROW[entries];

        for (int i = 0; i < entries; i++)
        {
          table[i] = (MIB_IPNETROW) Marshal.PtrToStructure(
            new IntPtr(currentBuffer.ToInt64() + (i * Marshal.SizeOf(typeof(MIB_IPNETROW)))), typeof(MIB_IPNETROW)
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
        throw new ArgumentOutOfRangeException("hwAddress", hwAddress, "hwAddress must contain 6 bytes!");
      byte[] packet = new byte[102];
      // pad packet data with 6 0xFF bytes
      for (int i = 0; i < 6; i++)
        packet[i] = 0xFF;

      // write hwaddress (at least) 16 times to packet
      for (int i = 1; i < 17; i++)
        for (int x = 0; x < 6; x++)
          packet[i * 6 + x] = hwAddress[x];
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

    private bool Ping(IPAddress address, int timeout)
    {
      Ping p = new Ping();
      PingReply r = p.Send(address, timeout);
      if (r.Status == IPStatus.Success)
        return true;
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
            return addrTable[i].bPhysAddr;
        }
      }
      // Try sending an ARP request specifically for this address
      return SendArpRequest(address);
    }

    /// <summary>
    /// Gets the hardware ethernet address of the given IP address as an array of bytes. The address is searched
    /// for in the ARP table cache and (if not found there) and ARP request is transmitted to find out the address.
    /// </summary>
    /// <param name="address">IP address to find the hardware ethernet address for</param>
    /// <returns>byte[] containing the hardware ethernet address</returns>
    public byte[] GetHardwareAddress(string address)
    {
      return GetHardwareAddress(new IPAddress(GetIpBytes(address)));
    }

    /// <summary>
    /// Checks whether the given address is a valid hardware ethernet address
    /// </summary>
    /// <param name="hwAddress">hardware ethernet address to check</param>
    /// <returns>bool indicating if the given address is valid</returns>
    public bool IsValidEthernetAddress(byte[] hwAddress)
    {
      if (hwAddress == null)
        return false;
      if (hwAddress.Length != 6)
        return false;
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
    /// Sends an AMD "magic" packet for the given hardware ethernet address, with ipAddress as the target.
    /// </summary>
    /// <param name="hwAddress">hardware ethernet address to wake up</param>
    /// <param name="ipAddress">IP address to use as target</param>
    /// <returns>bool indicating if the packet was sent successfully</returns>
    public bool SendWakeOnLanPacket(byte[] hwAddress, IPAddress ipAddress)
    {
      if (hwAddress.Length != 6)
        throw new ArgumentOutOfRangeException("hwAddress", hwAddress, "must be 6 bytes");
      if (IsValidEthernetAddress(hwAddress))
      {
        byte[] magicPacket = GetWakeOnLanMagicPacket(hwAddress);
        SendMagicPacket(ipAddress, magicPacket);
        return true;
      }
      else
      {
        Console.WriteLine("Invalid ethernet address!");
        return false;
      }
    }

    /// <summary>
    /// Sends an AMD "magic" packet for the given ipAddress.
    /// The hardware ethernet address is expected to be in the ARP cache table or resolvable via an
    /// ARP request (even though the system is not reachable).
    /// </summary>
    /// <param name="ipAddress">IP address to use as target</param>
    /// <returns>bool indicating if the packet was sent successfully</returns>
    public bool SendWakeOnLanPacket(IPAddress ipAddress)
    {
      byte[] hwAddress = GetHardwareAddress(ipAddress);
      return SendWakeOnLanPacket(hwAddress, ipAddress);
    }

    /// <summary>
    /// Sends an AMD "magic" packet to the general broadcast address (255.255.255.255) for the given
    /// hardware ethernet address.
    /// </summary>
    /// <param name="hwAddress">hardware ethernet address to wake up</param>
    /// <returns>bool indicating if the packet was sent successfully</returns>
    public bool SendWakeOnLanPacket(byte[] hwAddress)
    {
      byte[] ipAddress = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
      return SendWakeOnLanPacket(hwAddress, new IPAddress(ipAddress));
    }

    /// <summary>
    /// Wakes up the given target system by sending a wake-on-lan packet for the specified hardware ethernet address.
    /// The packet is broadcasted to the general broadcast address (255.255.255.255). After the wake-on-lan packet
    /// is sent, the system invokes ping requests to the wakeupTarget to verify that the host is actually resumed
    /// until the given timeout has been reached.
    /// </summary>
    /// <param name="hwAddress">Hardware ethernet address to wakeup (format: 00:01:02:ff:fe:9a)</param>
    /// <param name="wakeupTarget">IP address of the system to wake up. Used to verify the system has resumed.</param>
    /// <param name="timeout">timeout (in seconds) for system resume verification</param>
    /// <returns>bool indication whether or not the system is available</returns>
    public bool WakeupSystem(string hwAddress, string wakeupTarget, int timeout)
    {
      return WakeupSystem(GetHwAddrBytes(hwAddress), new IPAddress(GetIpBytes(wakeupTarget)), timeout);
    }

    /// <summary>
    /// Wakes up the given target system by sending a wake-on-lan packet for the specified hardware ethernet address.
    /// The packet is broadcasted to the IP address given in packetTarget). After the wake-on-lan packet is sent,
    /// the system invokes ping requests to the wakeupTarget to verify that the host is actually resumed until the
    /// given timeout has been reached.
    /// </summary>
    /// <param name="hwAddress">Hardware ethernet address to wakeup (format: 00:01:02:ff:fe:9a)</param>
    /// <param name="wakeupTarget">IP address of the system to wake up. Used to verify the system has resumed.</param>
    /// <param name="packetTarget">IP address to send the packet to (usually a broadcast address)</param>
    /// <param name="timeout">timeout (in seconds) for system resume verification</param>
    /// <returns>bool indication whether or not the system is available</returns>
    public bool WakeupSystem(string hwAddress, string wakeupTarget, string packetTarget, int timeout)
    {
      return WakeupSystem(GetHwAddrBytes(hwAddress), new IPAddress(GetIpBytes(wakeupTarget)), new IPAddress(GetIpBytes(packetTarget)), timeout);
    }

    /// <summary>
    /// Wakes up the given target system by sending a wake-on-lan packet for the specified hardware ethernet address.
    /// The packet is broadcasted to the general broadcast address (255.255.255.255). After the wake-on-lan packet
    /// is sent, the system invokes ping requests to the wakeupTarget to verify that the host is actually resumed
    /// until the given timeout has been reached.
    /// </summary>
    /// <param name="hwAddress">byte[] containing the hardware ethernet address to wakeup</param>
    /// <param name="wakeupTarget">IP address of the system to wake up. Used to verify the system has resumed.</param>
    /// <param name="packetTarget">IP address to send the packet to (usually a broadcast address)</param>
    /// <param name="timeout">timeout (in seconds) for system resume verification</param>
    /// <returns>bool indication whether or not the system is available</returns>
    public bool WakeupSystem(byte[] hwAddress, IPAddress wakeupTarget, int timeout)
    {
      return WakeupSystem(hwAddress, wakeupTarget, IPAddress.Broadcast, timeout);
    }

    /// <summary>
    /// Wakes up the given target system by sending a wake-on-lan packet for the specified hardware ethernet address.
    /// The packet is broadcasted to the IP address given in packetTarget). After the wake-on-lan packet is sent,
    /// the system invokes ping requests to the wakeupTarget to verify that the host is actually resumed until the
    /// given timeout has been reached.
    /// </summary>
    /// <param name="hwAddress">byte[] containing the hardware ethernet address to wakeup</param>
    /// <param name="wakeupTarget">IP address of the system to wake up. Used to verify the system has resumed.</param>
    /// <param name="packetTarget">IP address to send the packet to (usually a broadcast address)</param>
    /// <param name="timeout">timeout (in seconds) for system resume verification</param>
    /// <returns>bool indication whether or not the system is available</returns>
    public bool WakeupSystem(byte[] hwAddress, IPAddress wakeupTarget, IPAddress packetTarget, int timeout)
    {
      int waited = 0;

      if (Ping(wakeupTarget, timeout))
        return true;

      if (!SendWakeOnLanPacket(hwAddress, packetTarget))
      {
        Console.WriteLine("FAILED to send wake-on-lan packet!");
        return false;
      }

      while (waited < timeout * 1000)
      {
        if (Ping(wakeupTarget, 1000))
          return true;
        Console.WriteLine("System {0} still not reachable, waiting...");
        System.Threading.Thread.Sleep(1000);
        waited += 2000;
      }
      return false;
    }

    /// <summary>
    /// Convert the given IP address into a byte[]
    /// </summary>
    /// <param name="address">string containing the IP address to convert (192.168.1.1)</param>
    /// <returns>byte[] containing the byte representation of this IP address</returns>
    public byte[] GetIpBytes(string address)
    {
      byte[] ipn = new byte[4];
      string[] ip = address.Split('.');
      if (ip.Length != 4)
        throw new ArgumentOutOfRangeException("address", address, "not a valid IP addresss");
      try
      {
        for (int i = 0; i < ip.Length; i++)
          ipn[i] = Convert.ToByte(ip[i]);
        return ipn;
      }
      catch (FormatException)
      {
        throw new ArgumentOutOfRangeException("address", address, "not a valid IP addresss");
      }
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
        throw new ArgumentOutOfRangeException("address", address, "not a valid hardware ethernet addresss");
      try
      {
        for (int i = 0; i < addr.Length; i++)
          addrn[i] = byte.Parse(addr[i], System.Globalization.NumberStyles.HexNumber);
        return addrn;
      }
      catch (FormatException)
      {
        throw new ArgumentOutOfRangeException("address", address, "not a valid hardware ethernet addresss");
      }
    }
    #endregion

    #region Main entrypoint (for testing)
    public static void Main(String[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        switch (args[i].ToLowerInvariant())
        {
          case "-gethwaddr":
            if (args.Length != 2)
              throw new ApplicationException("-gethwaddr needs one argument: <ipaddress>");
            GetHwAddr(args[++i]);
            break;
          case "-wakeup":
            if (args.Length == 5)
            {
              Wakeup(args[i + 1], args[i + 2], args[i + 3], args[i + 4]);
              i += 4;
            }
            else if (args.Length == 4)
            {
              Wakeup(args[i + 1], args[i + 2], args[i + 3]);
              i += 3;
            }
            else
              throw new ApplicationException("-gethwaddr: arguments: <hwaddr> <wakeupIP> [<targetIP>] <timeout in seconds>");
            break;
          default:
            Console.WriteLine("Usage: WakeOnLanManager -gethwaddr <ipaddress>");
            Console.WriteLine("Usage: WakeOnLanManager -wakeup <hwaddr> <wakeupIP> <targetIP> <timeout in seconds>");
            Console.WriteLine("Example: WakeOnLanManager -wakeup 00:81:32:fb:ae:c7 192.168.1.2 192.168.1.255 10");
            break;
        }
      }
    }

    private static void GetHwAddr(string ipAddress)
    {
      Console.WriteLine("Getting hardware ethernet address for: ({0})", ipAddress);
      WakeOnLanManager wolManager = new WakeOnLanManager();
      byte[] ip = wolManager.GetIpBytes(ipAddress);
      byte[] a = wolManager.GetHardwareAddress(new IPAddress(ip));
      Console.WriteLine("Hardware address: {0:x}:{1:x}:{2:x}:{3:x}:{4:x}:{5:x}", a[0], a[1], a[2], a[3], a[4], a[5]);
    }

    private static void Wakeup(string hwAddr, string wakeupIP, string timeout)
    {
      Console.WriteLine("Wakeup system: IP:({0}) hwAddr:{1})", wakeupIP, hwAddr);
      WakeOnLanManager wolManager = new WakeOnLanManager();
      if (wolManager.WakeupSystem(hwAddr, wakeupIP, Int32.Parse(timeout)))
        Console.WriteLine("wakeup successful");
      else
        Console.WriteLine("wakeup FAILED!");
    }
    private static void Wakeup(string hwAddr, string wakeupIP, string targetIP, string timeout)
    {
      Console.WriteLine("Wakeup system: IP:({0}) hwAddr:{1} to:{2})", wakeupIP, hwAddr, targetIP);
      WakeOnLanManager wolManager = new WakeOnLanManager();
      if (wolManager.WakeupSystem(hwAddr, wakeupIP, targetIP, Int32.Parse(timeout)))
        Console.WriteLine("wakeup successful");
      else
        Console.WriteLine("wakeup FAILED!");
    }
    #endregion

  }
}
