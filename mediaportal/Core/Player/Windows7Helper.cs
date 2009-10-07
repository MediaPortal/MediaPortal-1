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

#region using
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
#endregion

namespace MediaPortal.Player
{
  public class W7RefreshRateHelper
  {
    #region consts
    private const uint SIZE_OF_DISPLAYCONFIG_PATH_INFO = 72;
    private const uint SIZE_OF_DISPLAYCONFIG_MODE_INFO = 64;

    private const uint QDC_ALL_PATHS = 1;
    private const uint QDC_ONLY_ACTIVE_PATHS = 2;
    private const uint QDC_DATABASE_CURRENT = 4;

    private const uint SDC_TOPOLOGY_INTERNAL = 1;
    private const uint SDC_TOPOLOGY_CLONE = 2;
    private const uint SDC_TOPOLOGY_EXTEND = 4;
    private const uint SDC_TOPOLOGY_EXTERNAL = 0x00000008;
    private const uint SDC_TOPOLOGY_SUPPLIED = 0x00000010;
    private const uint SDC_USE_DATABASE_CURRENT = (SDC_TOPOLOGY_INTERNAL | SDC_TOPOLOGY_CLONE | SDC_TOPOLOGY_EXTEND | SDC_TOPOLOGY_EXTERNAL);
    private const uint SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x00000020;
    private const uint SDC_VALIDATE = 0x00000040;
    private const uint SDC_APPLY = 0x00000080;
    private const uint SDC_NO_OPTIMIZATION = 0x00000100;
    private const uint SDC_SAVE_TO_DATABASE = 0x00000200;
    private const uint SDC_ALLOW_CHANGES = 0x00000400;
    private const uint SDC_PATH_PERSIST_IF_REQUIRED = 0x00000800;
    private const uint SDC_FORCE_MODE_ENUMERATION = 0x00001000;
    private const uint SDC_ALLOW_PATH_ORDER_CHANGES = 0x00002000;
    #endregion

    #region DLL imports
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private extern static int GetDisplayConfigBufferSizes([In] uint flags, [Out] out uint numPathArrayElements, [Out] out uint numModeArrayElements);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private extern static int QueryDisplayConfig([In] uint flags, ref uint numPathArrayElements, IntPtr pathArray, ref uint numModeArrayElements, IntPtr modeArray, IntPtr currentTopologyId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private extern static int SetDisplayConfig(uint numPathArrayElements, IntPtr pathArray, uint numModeArrayElements, IntPtr modeArray, uint flags);
    #endregion

    #region private members
    private static int GetModeInfoOffsetForMonitorId(uint monitorIndex, IntPtr pModeArray, uint numModeArrayElements)
    {
      uint modeArraySize = numModeArrayElements * SIZE_OF_DISPLAYCONFIG_MODE_INFO;
      int offset = (int)((monitorIndex*2) * SIZE_OF_DISPLAYCONFIG_MODE_INFO); // There are always modeinfos per monitor. One for target info and one for source info
      if (offset+(SIZE_OF_DISPLAYCONFIG_MODE_INFO)>=modeArraySize)
        return -1;
      uint infoType = (uint)Marshal.ReadInt32(pModeArray, offset);
      if (infoType!=2) // DISPLAYCONFIG_MODE_INFO_TYPE_TARGET
        offset+=(int)SIZE_OF_DISPLAYCONFIG_MODE_INFO;
      infoType = (uint)Marshal.ReadInt32(pModeArray, offset);
      if (infoType!=2)
        return -1; // both mode infos didn't contain target info. This shouldn't happen
      return offset;
    }
    #endregion

    #region public members
    public static UInt32 GetRefreshRate(uint monitorIndex)
    {
      uint numPathArrayElements;
      uint numModeArrayElements;
      IntPtr pPathArray;
      IntPtr pModeArray;

      int ret = GetDisplayConfigBufferSizes(QDC_ALL_PATHS, out numPathArrayElements, out numModeArrayElements);
      if (ret != 0)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: GetDisplayConfigBufferSizes(...) returned " + ret.ToString());
        return 0;
      }
      pPathArray = Marshal.AllocHGlobal((Int32)(numPathArrayElements * SIZE_OF_DISPLAYCONFIG_PATH_INFO));
      pModeArray = Marshal.AllocHGlobal((Int32)(numModeArrayElements * SIZE_OF_DISPLAYCONFIG_MODE_INFO));

      ret = QueryDisplayConfig(QDC_ALL_PATHS, ref numPathArrayElements, pPathArray, ref numModeArrayElements, pModeArray, IntPtr.Zero);
      if (ret != 0)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: QueryDisplayConfig(...) returned " + ret.ToString());
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return 0;
      }
      if (numModeArrayElements < 2)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: QueryDisplayConfig(...) returned numModeArrays="+numModeArrayElements.ToString()+". We need at least 2");
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return 0;
      }
      int offset=GetModeInfoOffsetForMonitorId(monitorIndex,pModeArray,numModeArrayElements);
      if (offset==-1)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: Couldn't find a suitable target mode info for monitor {0}",monitorIndex);
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return 0;
      }
      UInt32 numerator = (UInt32)Marshal.ReadInt32(pModeArray,offset+32);
      UInt32 denominator = (UInt32)Marshal.ReadInt32(pModeArray,offset+36);
      UInt32 rr = numerator / denominator;
      Marshal.FreeHGlobal(pPathArray);
      Marshal.FreeHGlobal(pModeArray);
      return rr;
    }

    public static bool SetRefreshRate(uint monitorIndex,uint rr)
    {
      uint numPathArrayElements;
      uint numModeArrayElements;
      IntPtr pPathArray;
      IntPtr pModeArray;

      int ret = GetDisplayConfigBufferSizes(QDC_ALL_PATHS, out numPathArrayElements, out numModeArrayElements);
      if (ret != 0)
      {
        Log.Error("W7RefreshRateHelper.SetRefreshRate: GetDisplayConfigBufferSizes(...) returned " + ret.ToString());
        return false;
      }
      pPathArray = Marshal.AllocHGlobal((Int32)(numPathArrayElements * SIZE_OF_DISPLAYCONFIG_PATH_INFO));
      pModeArray = Marshal.AllocHGlobal((Int32)(numModeArrayElements * SIZE_OF_DISPLAYCONFIG_MODE_INFO));

      ret = QueryDisplayConfig(QDC_ALL_PATHS, ref numPathArrayElements, pPathArray, ref numModeArrayElements, pModeArray, IntPtr.Zero);
      if (ret != 0)
      {
        Log.Error("W7RefreshRateHelper.SetRefreshRate: QueryDisplayConfig(...) returned " + ret.ToString());
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return false;
      }
      if (numModeArrayElements < 2)
      {
        Log.Error("W7RefreshRateHelper.SetRefreshRate: QueryDisplayConfig(...) returned numModeArrays=" + numModeArrayElements.ToString() + ". We need at least 2");
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return false;
      }
      int offset=GetModeInfoOffsetForMonitorId(monitorIndex,pModeArray,numModeArrayElements);
      if (offset==-1)
      {
        Log.Error("W7RefreshRateHelper.SetRefreshRate: Couldn't find a suitable target mode info for monitor {0}",monitorIndex);
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return false;
      }
      Marshal.WriteInt32(pModeArray,offset+32, (int)rr);
      Marshal.WriteInt32(pModeArray,offset+36, 1);

      ret = SetDisplayConfig(numPathArrayElements, pPathArray, numModeArrayElements, pModeArray, SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_FORCE_MODE_ENUMERATION | SDC_ALLOW_CHANGES);
      if (ret != 0)
      {
        Log.Error("W7RefreshRateHelper.SetRefreshRate: SetDisplayConfig(...) returned " + ret.ToString());
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return false;
      }
      Marshal.FreeHGlobal(pPathArray);
      Marshal.FreeHGlobal(pModeArray);
      return true;
    }      
    #endregion
  }
}
