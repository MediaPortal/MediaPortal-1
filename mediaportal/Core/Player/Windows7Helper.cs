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
    private const uint DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2;
    private const uint DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1;

    private const uint SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x00000020;
    private const uint SDC_VALIDATE = 0x00000040;
    private const uint SDC_APPLY = 0x00000080;
    private const uint SDC_ALLOW_CHANGES = 0x00000400;

    #endregion

    #region DLL imports

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern long GetDisplayConfigBufferSizes([In] uint flags, [Out] out uint numPathArrayElements,
                                                           [Out] out uint numModeArrayElements);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern long QueryDisplayConfig([In] uint flags, ref uint numPathArrayElements, IntPtr pathArray,
                                                  ref uint numModeArrayElements, IntPtr modeArray,
                                                  IntPtr currentTopologyId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern long SetDisplayConfig(uint numPathArrayElements, IntPtr pathArray, uint numModeArrayElements,
                                                IntPtr modeArray, uint flags);

    #endregion

    #region private members

    private static int GetModeInfoOffsetForDisplayId(uint displayIndex, IntPtr pModeArray, uint uNumModeArrayElements)
    {
      int offset;
      int modeInfoType;

      // there are always two mode infos per display (target and source)
      offset = (int)(displayIndex * SIZE_OF_DISPLAYCONFIG_MODE_INFO * 2);

      // out of bounds sanity check
      if (offset + SIZE_OF_DISPLAYCONFIG_MODE_INFO >= uNumModeArrayElements * SIZE_OF_DISPLAYCONFIG_MODE_INFO)
      {
        return -1;
      }

      // check which one of the two mode infos for the display is the target
      modeInfoType = Marshal.ReadInt32(pModeArray, offset);
      if (modeInfoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
      {
        return offset;
      }
      else
      {
        offset += (int)SIZE_OF_DISPLAYCONFIG_MODE_INFO;
      }

      modeInfoType = Marshal.ReadInt32(pModeArray, offset);
      if (modeInfoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
      {
        return offset;
      }
        // no target mode info found, this should never happen
      else
      {
        return -1;
      }
    }

    #endregion

    #region public members

    public static double GetRefreshRate(uint displayIndex)
    {
      uint uNumPathArrayElements = 0;
      uint uNumModeArrayElements = 0;
      IntPtr pPathArray = IntPtr.Zero;
      IntPtr pModeArray = IntPtr.Zero;
      IntPtr pCurrentTopologyId = IntPtr.Zero;
      long result;
      UInt32 numerator;
      UInt32 denominator;
      double refreshRate;

      // get size of buffers for QueryDisplayConfig
      result = GetDisplayConfigBufferSizes(QDC_ALL_PATHS, out uNumPathArrayElements, out uNumModeArrayElements);
      if (result != 0)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: GetDisplayConfigBufferSizes(...) returned {0}", result);
        return 0;
      }

      // allocate memory or QueryDisplayConfig buffers
      pPathArray = Marshal.AllocHGlobal((Int32)(uNumPathArrayElements * SIZE_OF_DISPLAYCONFIG_PATH_INFO));
      pModeArray = Marshal.AllocHGlobal((Int32)(uNumModeArrayElements * SIZE_OF_DISPLAYCONFIG_MODE_INFO));

      // get display configuration
      result = QueryDisplayConfig(QDC_ALL_PATHS,
                                  ref uNumPathArrayElements, pPathArray,
                                  ref uNumModeArrayElements, pModeArray,
                                  pCurrentTopologyId);
      // if failed log error message and free memory
      if (result != 0)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: QueryDisplayConfig(...) returned {0}", result);
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return 0;
      }

      // get offset for a display's target mode info
      int offset = GetModeInfoOffsetForDisplayId(displayIndex, pModeArray, uNumModeArrayElements);
      // if failed log error message and free memory
      if (offset == -1)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: Couldn't find a target mode info for display {0}", displayIndex);
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return 0;
      }

      // get refresh rate
      numerator = (UInt32)Marshal.ReadInt32(pModeArray, offset + 32);
      denominator = (UInt32)Marshal.ReadInt32(pModeArray, offset + 36);
      refreshRate = (double)numerator / (double)denominator;
      Log.Debug("W7RefreshRateHelper.GetRefreshRate: QueryDisplayConfig returned {0}/{1}", numerator, denominator);

      // free memory and return refresh rate
      Marshal.FreeHGlobal(pPathArray);
      Marshal.FreeHGlobal(pModeArray);
      return refreshRate;
    }


    public static bool SetRefreshRate(uint displayIndex, double refreshRate)
    {
      uint uNumPathArrayElements = 0;
      uint uNumModeArrayElements = 0;
      IntPtr pPathArray = IntPtr.Zero;
      IntPtr pModeArray = IntPtr.Zero;
      IntPtr pCurrentTopologyId = IntPtr.Zero;
      long result;
      UInt32 numerator;
      UInt32 denominator;
      UInt32 scanLineOrdering;
      UInt32 flags;

      // get size of buffers for QueryDisplayConfig
      result = GetDisplayConfigBufferSizes(QDC_ALL_PATHS, out uNumPathArrayElements, out uNumModeArrayElements);
      if (result != 0)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: GetDisplayConfigBufferSizes(...) returned {0}", result);
        return false;
      }

      // allocate memory or QueryDisplayConfig buffers
      pPathArray = Marshal.AllocHGlobal((Int32)(uNumPathArrayElements * SIZE_OF_DISPLAYCONFIG_PATH_INFO));
      pModeArray = Marshal.AllocHGlobal((Int32)(uNumModeArrayElements * SIZE_OF_DISPLAYCONFIG_MODE_INFO));

      // get display configuration
      result = QueryDisplayConfig(QDC_ALL_PATHS,
                                  ref uNumPathArrayElements, pPathArray,
                                  ref uNumModeArrayElements, pModeArray,
                                  pCurrentTopologyId);
      // if failed log error message and free memory
      if (result != 0)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: QueryDisplayConfig(...) returned {0}", result);
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return false;
      }

      // get offset for a display's target mode info
      int offset = GetModeInfoOffsetForDisplayId(displayIndex, pModeArray, uNumModeArrayElements);
      // if failed log error message and free memory
      if (offset == -1)
      {
        Log.Error("W7RefreshRateHelper.GetRefreshRate: Couldn't find a target mode info for display {0}", displayIndex);
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return false;
      }

      // TODO: refactor to private method
      // set proper numerator and denominator for refresh rate
      UInt32 newRefreshRate = (uint)(refreshRate * 1000);
      switch (newRefreshRate)
      {
        case 23976:
          numerator = 24000;
          denominator = 1001;
          scanLineOrdering = DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE;
          break;
        case 24000:
          numerator = 24000;
          denominator = 1000;
          scanLineOrdering = DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE;
          break;
        case 25000:
          numerator = 25000;
          denominator = 1000;
          scanLineOrdering = DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE;
          break;
        case 30000:
          numerator = 30000;
          denominator = 1000;
          scanLineOrdering = DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE;
          break;
        case 50000:
          numerator = 50000;
          denominator = 1000;
          scanLineOrdering = DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE;
          break;
        case 59940:
          numerator = 60000;
          denominator = 1001;
          scanLineOrdering = DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE;
          break;
        case 60000:
          numerator = 60000;
          denominator = 1000;
          scanLineOrdering = DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE;
          break;
        default:
          numerator = (uint)refreshRate;
          denominator = 1;
          scanLineOrdering = DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE;
          break;
      }

      // set refresh rate parameters in display config
      Marshal.WriteInt32(pModeArray, offset + 32, (int)numerator);
      Marshal.WriteInt32(pModeArray, offset + 36, (int)denominator);
      Marshal.WriteInt32(pModeArray, offset + 56, (int)scanLineOrdering);

      // validate new refresh rate
      flags = SDC_VALIDATE | SDC_USE_SUPPLIED_DISPLAY_CONFIG;
      result = SetDisplayConfig(uNumPathArrayElements, pPathArray, uNumModeArrayElements, pModeArray, flags);
      // adding SDC_ALLOW_CHANGES to flags if validation failed
      if (result != 0)
      {
        Log.Debug("W7RefreshRateHelper.SetDisplayConfig(...): SDC_VALIDATE of {0}/{1} failed", numerator, denominator);
        flags = SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_ALLOW_CHANGES;
      }
      else
      {
        Log.Debug("W7RefreshRateHelper.SetDisplayConfig(...): SDC_VALIDATE of {0}/{1} succesful", numerator, denominator);
        flags = SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG;
      }

      // configuring display
      result = SetDisplayConfig(uNumPathArrayElements, pPathArray, uNumModeArrayElements, pModeArray, flags);
      // if failed log error message and free memory
      if (result != 0)
      {
        Log.Error("W7RefreshRateHelper.SetDisplayConfig(...): SDC_APPLY returned {0}", result);
        Marshal.FreeHGlobal(pPathArray);
        Marshal.FreeHGlobal(pModeArray);
        return false;
      }

      // refresh rate change successful   
      Marshal.FreeHGlobal(pPathArray);
      Marshal.FreeHGlobal(pModeArray);
      return true;
    }

    #endregion
  }
}