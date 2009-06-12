#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Runtime.InteropServices;
using TvLibrary.Log;
//using MediaPortal.GUI.Library;

namespace TvEngine
{
  /// <summary>
  /// Summary description for HCWIRBlaster.
  /// </summary>
  public class HCWIRBlaster
  {
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadLibraryEx(string fileName, IntPtr dummy, int flags);

    [DllImport("hcwIRblast.dll")]
    private static extern int UIR_Close();

    [DllImport("hcwIRblast.dll")]
    private static extern int UIR_GetConfig(int device, int codeset, ref UIR_CFG cfgPtr);

    [DllImport("hcwIRblast.dll")]
    private static extern int UIR_GotoChannel(int device, int codeset, int channel);

    [DllImport("hcwIRblast.dll")]
    private static extern ushort UIR_Open(uint bVerbose, ushort wIRPort);

    private static int HCWRetVal;
    private static UIR_CFG HCWIRConfig;

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct UIR_CFG
    {
      public int a;   // 0x38;
      public int b;
      public int c;   //Region 
      public int d;   //Device
      public int e;   //Vendor
      public int f;   //Code Set
      public int g;
      public int h;
      public int i;   //Minimum Digits
      public int j;   //Digit Delay
      public int k;   //Need Enter
      public int l;   //Enter Delay
      public int m;   //Tune Delay
      public int n;   //One Digit Delay
    }


    static HCWIRBlaster()
    {
      HCWRetVal = 0;
      HCWIRConfig = new UIR_CFG();
    }


    public void blast(string channel_data, bool ExLogging)
    {
      if (ExLogging)
        Log.Info("HCWBlaster: Changing channels: {0}", channel_data);

      int iChannel = Convert.ToInt32(channel_data);

      if (HCWRetVal == 0)
      {

        HCWRetVal = UIR_Open(0, 0);
        if (HCWRetVal == 0)
        {
          Log.Info("HCWBlaster: Failed to get Blaster Handle");
          return;
        }

        HCWIRConfig.a = 0x38;
        int RetCfg = UIR_GetConfig(-1, -1, ref HCWIRConfig);
        if (RetCfg == 0)
        {
          string devset1 = "Device       : " + HCWIRConfig.d + "    Vendor        : " + HCWIRConfig.e;
          string devset2 = "Region       : " + HCWIRConfig.c + "    Code set      : " + HCWIRConfig.f;
          string devset3 = "Digit Delay  : " + HCWIRConfig.j + "    Minimum Digits: " + HCWIRConfig.i;
          string devset4 = "OneDigitDelay: " + HCWIRConfig.n + "    Tune Delay    : " + HCWIRConfig.m;
          string devset5 = "Need Enter   : " + HCWIRConfig.k + "    Enter Delay   : " + HCWIRConfig.l;

          if (ExLogging)
          {
            Log.Info("HCWBlaster: " + devset1);
            Log.Info("HCWBlaster: " + devset2);
            Log.Info("HCWBlaster: " + devset3);
            Log.Info("HCWBlaster: " + devset4);
            Log.Info("HCWBlaster: " + devset5);
          }

        } else
        {
          UIR_Close();
          HCWRetVal = 0;
        }
      }
      int RetChg = UIR_GotoChannel(HCWIRConfig.d, HCWIRConfig.f, iChannel);
      if (RetChg != 0)
      {
        Log.Info("HCWBlaster: UIR_GotoChannel() failed: " + RetChg);
      }
      if (ExLogging)
        Log.Info("HCWBlaster: Finished Changing channels: {0}", channel_data);
    }

  }
}
