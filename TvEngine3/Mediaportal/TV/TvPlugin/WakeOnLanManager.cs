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

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.TvPlugin
{
  public class WakeOnLanManager
  {
    #region constants

    private const int HW_ADDRESS_SIZE = 6;

    #endregion

    #region structs

    // Define the MIB_IPNETROW structure.
    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_IPNETROW
    {
      public int dwIndex;
      public int dwPhysAddrLen;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] bPhysAddr;
      public int dwAddr;
      public int dwType;
    }

    #endregion

    #region external methods

    [DllImport("iphlpapi.dll")]
    [return: MarshalAs(UnmanagedType.U4)]
    private static extern int GetIpNetTable(IntPtr ipNetTable, [MarshalAs(UnmanagedType.U4)] ref int size, bool order);

    [DllImport("iphlpapi.dll")]
    [return: MarshalAs(UnmanagedType.U4)]
    private static extern int SendARP(int destIP, int srcIP, [Out] byte[] macAddr, ref int phyAddrLen);

    #endregion

    #region private methods

    private static MIB_IPNETROW[] GetHardwareAddressTable()
    {
      int bytesNeeded = 0;
      int result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);
      if (result != (int)NativeMethods.SystemErrorCode.ERROR_INSUFFICIENT_BUFFER)
      {
        throw new ApplicationException(Convert.ToString(result));
      }

      MIB_IPNETROW[] table = null;
      IntPtr buffer = Marshal.AllocCoTaskMem(bytesNeeded);
      try
      {
        result = GetIpNetTable(buffer, ref bytesNeeded, false);
        if (result != (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS)
        {
          throw new ApplicationException(Convert.ToString(result));
        }

        int entryCount = Marshal.ReadInt32(buffer);
        IntPtr currentBuffer = IntPtr.Add(buffer, sizeof(int));
        table = new MIB_IPNETROW[entryCount];
        int arpEntrySize = Marshal.SizeOf(typeof(MIB_IPNETROW));
        for (int i = 0; i < entryCount; i++)
        {
          table[i] = (MIB_IPNETROW)Marshal.PtrToStructure(currentBuffer, typeof(MIB_IPNETROW));
          currentBuffer = IntPtr.Add(currentBuffer, arpEntrySize);
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }
      return table;
    }

    private static byte[] GetWakeOnLanMagicPacket(byte[] hwAddress)
    {
      // Refer to http://support.amd.com/TechDocs/20213.pdf
      if (hwAddress == null || hwAddress.Length != HW_ADDRESS_SIZE)
      {
        throw new ArgumentOutOfRangeException("hwAddress", hwAddress, string.Format("hwAddress must contain {0} bytes!", HW_ADDRESS_SIZE));
      }

      byte[] packet = new byte[HW_ADDRESS_SIZE * 17];

      // Write padding.
      int offset = 0;
      for (int i = 0; i < HW_ADDRESS_SIZE; i++)
      {
        packet[offset++] = 0xff;
      }

      // Write the hardware address at least 16 times.
      for (int i = 0; i < 16; i++)
      {
        for (int x = 0; x < HW_ADDRESS_SIZE; x++)
        {
          packet[offset++] = hwAddress[x];
        }
      }
      return packet;
    }

    private static void SendUdpPacket(IPAddress address, byte[] data)
    {
      using (UdpClient client = new UdpClient())
      {
        client.Connect(address, 1234);
        client.Send(data, data.Length);
        client.Close();
      }
    }

    private static bool Ping(string hostName, int timeLimitMilliSeconds)
    {
      Log.Debug("WOL manager: ping, host name = {0}, time limit = {1} ms", hostName, timeLimitMilliSeconds);
      using (Ping p = new Ping())
      {
        try
        {
          return p.Send(hostName, timeLimitMilliSeconds).Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
          Log.Error(ex, "WOL manager: ping failed, host name = {0}", hostName);
        }
      }
      return false;
    }

    private static bool IsValidHardwareAddress(byte[] hwAddress)
    {
      if (hwAddress == null || hwAddress.Length != HW_ADDRESS_SIZE)
      {
        return false;
      }

      for (int i = 0; i < hwAddress.Length; i++)
      {
        if (hwAddress[i] != 0x00)
        {
          return true;
        }
      }
      return false;
    }

    #endregion

    #region public methods

    /// <summary>
    /// Get the hardware ethernet (MAC) address of the given IP address as an
    /// array of bytes. The address is either retrieved from the ARP table
    /// cache, or determined by transmission of an ARP request.
    /// </summary>
    /// <param name="address">The IP address to find the hardware ethernet address for.</param>
    /// <returns>a byte array containing the hardware ethernet address for the IP address</returns>
    public static byte[] GetHardwareAddress(IPAddress address)
    {
      byte[] hardwareAddress = null;
      try
      {
        int addr = BitConverter.ToInt32(address.GetAddressBytes(), 0);

        // Try fetching the address from the ARP table.
        MIB_IPNETROW[] addressTable = GetHardwareAddressTable();
        if (addressTable != null)
        {
          foreach (var row in addressTable)
          {
            if (addr == row.dwAddr)
            {
              hardwareAddress = new byte[row.dwPhysAddrLen];
              Array.Copy(row.bPhysAddr, hardwareAddress, row.dwPhysAddrLen);
              return hardwareAddress;
            }
          }
        }

        // Try sending an ARP request for the address.
        int addressSize = HW_ADDRESS_SIZE;
        hardwareAddress = new byte[addressSize];
        if (SendARP(addr, 0, hardwareAddress, ref addressSize) != (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS || !IsValidHardwareAddress(hardwareAddress))
        {
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex, "WOL manager: failed to determine hardware address, IP address = {0}", address);
      }
      return hardwareAddress;
    }

    /// <summary>
    /// Wake a target system by sending a wake-on-LAN packet for the
    /// corresponding hardware ethernet address. Ping is used to verify that
    /// the system has woken.
    /// </summary>
    /// <param name="hwAddress">A byte array containing the hardware ethernet address of the target system.</param>
    /// <param name="hostName">The host name of the target system.</param>
    /// <param name="timeLimitSeconds">The time limit for the wake process. The unit is seconds.</param>
    /// <returns><c>true</c> if the target system was woken successfully, otherwise <c>false</c></returns>
    public static bool WakeSystem(byte[] hwAddress, string hostName, int timeLimitSeconds)
    {
      Log.Debug("WOL manager: wake system, host name = {0}, hardware address = {1}, time limit = {2} s", hostName, BitConverter.ToString(hwAddress).Replace("-", ":"), timeLimitSeconds);
      if (!IsValidHardwareAddress(hwAddress))
      {
        Log.Error("WOL manager: invalid hardware ethernet address, address = {0}", BitConverter.ToString(hwAddress).Replace("-", ":"));
        return false;
      }

      timeLimitSeconds *= 1000;   // to milli-seconds
      DateTime start = DateTime.Now;

      if (Ping(hostName, 1000))
      {
        Log.Info("WOL manager: system already awake, host name = {0}", hostName);
        return true;
      }

      byte[] packet = GetWakeOnLanMagicPacket(hwAddress);
      while ((DateTime.Now - start).TotalMilliseconds < timeLimitSeconds)
      {
        Log.Debug("WOL manager: send WOL packet...");
        try
        {
          SendUdpPacket(IPAddress.Broadcast, packet);
        }
        catch (Exception ex)
        {
          Log.Error(ex, "WOL manager: WOL packet transmission failed");
        }

        Log.Debug("WOL manager: ping...");
        if (Ping(hostName, 1000))
        {
          Log.Info("WOL manager: system woken successfully, host name = {0}", hostName);
          return true;
        }

        Log.Debug("WOL manager: not yet reachable, waiting 1 second...");
        System.Threading.Thread.Sleep(1000);
      }

      Log.Error("WOL manager: failed to wake system within time limit, host name = {0}, hardware address = {1}, time limit = {2} ms", hostName, BitConverter.ToString(hwAddress).Replace("-", ":"), timeLimitSeconds);
      return false;
    }

    #endregion
  }
}