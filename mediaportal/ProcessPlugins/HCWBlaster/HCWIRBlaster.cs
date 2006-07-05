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

using System;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Utils.Services;

namespace MediaPortal.HCWBlaster
{
	/// <summary>
	/// Summary description for HCWIRBlaster.
	/// </summary>
	public class HCWIRBlaster
	{

		[DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern IntPtr LoadLibraryEx(string fileName, IntPtr dummy, int flags);

		[DllImport("hcwIRblast.dll")]
		private static extern int UIR_Close();

		[DllImport("hcwIRblast.dll")]
		private static extern int UIR_GetConfig(int device, int codeset, ref UIR_CFG cfgPtr);

		[DllImport("hcwIRblast.dll")]
		private static extern int UIR_GotoChannel(int device, int codeset, int channel);

		[DllImport("hcwIRblast.dll")]
		private static extern ushort UIR_Open(uint bVerbose, ushort wIRPort);

		private static int     HCWRetVal;
		private static UIR_CFG HCWIRConfig;

		[StructLayout(LayoutKind.Sequential, Pack=8)]
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

    protected ILog _log;

		public HCWIRBlaster()
		{
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
		}

		static HCWIRBlaster()
		{
			HCWIRBlaster.HCWRetVal = 0;
			HCWIRBlaster.HCWIRConfig = new UIR_CFG();
		}


		public void blast(string channel_data, bool ExLogging)
		{
			if (ExLogging == true)
				_log.Info("HCWBlaster: Changing channels: {0}", channel_data );

			int iChannel = Convert.ToInt32(channel_data.ToString());

			if (HCWIRBlaster.HCWRetVal == 0)
			{
                
				HCWIRBlaster.HCWRetVal = HCWIRBlaster.UIR_Open(0, 0);
				if (HCWIRBlaster.HCWRetVal == 0)
				{
					_log.Info("HCWBlaster: Failed to get Blaster Handle");
					return;
				}
                
				HCWIRBlaster.HCWIRConfig.a = 0x38;
				int RetCfg = HCWIRBlaster.UIR_GetConfig(-1, -1, ref HCWIRBlaster.HCWIRConfig);
				if (RetCfg == 0)
				{
					string devset1 = "Device       : " + HCWIRBlaster.HCWIRConfig.d.ToString() + "    Vendor        : " + HCWIRBlaster.HCWIRConfig.e.ToString();
					string devset2 = "Region       : " + HCWIRBlaster.HCWIRConfig.c.ToString() + "    Code set      : " + HCWIRBlaster.HCWIRConfig.f.ToString();
					string devset3 = "Digit Delay  : " + HCWIRBlaster.HCWIRConfig.j.ToString() + "    Minimum Digits: " + HCWIRBlaster.HCWIRConfig.i.ToString();
					string devset4 = "OneDigitDelay: " + HCWIRBlaster.HCWIRConfig.n.ToString() + "    Tune Delay    : " + HCWIRBlaster.HCWIRConfig.m.ToString();
					string devset5 = "Need Enter   : " + HCWIRBlaster.HCWIRConfig.k.ToString() + "    Enter Delay   : " + HCWIRBlaster.HCWIRConfig.l.ToString();

					if (ExLogging == true) 
					{
						_log.Info("HCWBlaster: " + devset1);
						_log.Info("HCWBlaster: " + devset2);
						_log.Info("HCWBlaster: " + devset3);
						_log.Info("HCWBlaster: " + devset4);
						_log.Info("HCWBlaster: " + devset5);
					}

				}
				else
				{
					HCWIRBlaster.UIR_Close();
					HCWIRBlaster.HCWRetVal = 0;
				}
			}
			int RetChg = HCWIRBlaster.UIR_GotoChannel(HCWIRBlaster.HCWIRConfig.d, HCWIRBlaster.HCWIRConfig.f, iChannel);
			if (RetChg != 0)
			{
				_log.Info("HCWBlaster: UIR_GotoChannel() failed: " + RetChg.ToString());
			}
			if (ExLogging == true)
				_log.Info("HCWBlaster: Finished Changing channels: {0}", channel_data );
		}

	}
}
