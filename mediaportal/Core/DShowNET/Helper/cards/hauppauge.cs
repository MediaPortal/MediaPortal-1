/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using DirectShowLib;
namespace DShowNET
{

	public class Hauppauge : IksPropertyUtils
	{
		static public readonly Guid HauppaugeGuid = new Guid( 0x432a0da4, 0x806a, 0x43a0, 0xb4, 0x26, 0x4f, 0x2a, 0x23, 0x4a, 0xa6, 0xb8 );
		public enum PropertyId
		{
			StreamType				=   0, 
			VideoClosedOpenGop= 101, //bool											
			VideoBitRate			= 102,
			VideoGopSize			= 103,
			InverseTelecine		= 104,  //bool
			AudioBitRate			= 200, 							
			AudioSampleRate		= 201, 					
			AudioOutput				= 202,
			CardModel					= 400, 
			DriverVersion			= 401
		};

		public enum StreamType
		{
				Program			= 100,
				DVD					= 102,
				Mediacenter	= 103,
				SVCD				= 104,
				MPEG1				= 201,
				MPEG1_VCD		= 202,
		};

		[StructLayout(LayoutKind.Sequential,Pack=1), ComVisible(true)]
			public struct VideoBitRate
		{
			public uint size;
			public uint isnull;
			public uint isvbr;
			public uint bitrate; //kb/sec
			public uint maxBitrate;// kb/sec
		};

		[StructLayout(LayoutKind.Sequential,Pack=1), ComVisible(true)]
			public struct GopSize
		{
			public uint size;
			public uint isnull;
			public uint PictureCount;
			public uint SpaceBetweenPandB;
		};

		public enum AudioBitRateEnum
		{
			Khz32		= 1,
			Khz48		= 2,
			Khz56		= 3,
			Khz64		= 4,
			Khz80		= 5,
			Khz96		= 6,
			Khz112	= 7,
			Khz128	= 8,
			Khz160	= 9,
			Khz192	= 10,
			Khz224	= 11,
			Khz256	= 12,
			Khz320	= 13,
			Khz384	= 14,
		}

		[StructLayout(LayoutKind.Sequential,Pack=1), ComVisible(true)]
			public struct AudioBitRate
		{
			public uint size;
			public uint isnull;
			public uint audiolayer;
			public AudioBitRateEnum bitrate;
		}

		public enum AudioSampleRate
		{
			KHz_32   = 0,
			Khz_44_1 = 1,
			Khz_48   = 2
		};

		public enum AudioOutput
		{
			Stereo     =0,
			JointStereo=1,
			Dual			 =2,
			Mono			 =3
		};

		[StructLayout(LayoutKind.Sequential,Pack=1), ComVisible(true)]
		public struct driverVersion
		{
			public uint			size;			
			public uint			isnull;
			public uint			major;		
			public uint			minor;		
			public uint			revision;	
			public uint			build;		
		};

		public Hauppauge(IBaseFilter filter)
			:base(filter)
		{
		}
		
		
		public bool IsHauppage
		{
			get 
			{
				IKsPropertySet propertySet= captureFilter as IKsPropertySet;
				if (propertySet==null) return false;
				Guid propertyGuid=HauppaugeGuid;
				uint IsTypeSupported=0;
				int hr=propertySet.QuerySupported( ref propertyGuid, (uint)PropertyId.DriverVersion, out IsTypeSupported);
				if (hr!=0 || (IsTypeSupported & (uint)KsPropertySupport.Get)==0) 
				{
					return false;
				}
				return true;
			}
		}

		public bool GetVideoBitRate(out int minKbps, out int maxKbps,out bool isVBR)
		{
			VideoBitRate bitrate = new VideoBitRate();
			bitrate.size=(uint)Marshal.SizeOf(bitrate);
			object obj =GetStructure(HauppaugeGuid,(uint)PropertyId.VideoBitRate, typeof(VideoBitRate)) ;
			try
			{ 
				bitrate = (VideoBitRate)obj ;
			}
			catch (Exception){}
			isVBR = (bitrate.isvbr!=0);
			minKbps=(int)bitrate.bitrate;
			maxKbps=(int)bitrate.maxBitrate;
			Log.Write("hauppauge: current videobitrate: min:{0} max:{1} vbr:{2}",minKbps,maxKbps,isVBR);
			return true;
		}
		public void SetVideoBitRate(int minKbps, int maxKbps,bool isVBR)
		{
			Log.Write("hauppauge: setvideobitrate min:{0} max:{1} vbr:{2} {3}",minKbps,maxKbps,isVBR,Marshal.SizeOf(typeof(VideoBitRate)));
			VideoBitRate bitrate=new VideoBitRate();
			if (isVBR) bitrate.isvbr=1;
			else bitrate.isvbr=0;

			bitrate.size=(uint)Marshal.SizeOf(typeof(VideoBitRate));
			bitrate.bitrate=(uint)minKbps;
			bitrate.maxBitrate=(uint)maxKbps;
			SetStructure(HauppaugeGuid,(uint)PropertyId.VideoBitRate, typeof(VideoBitRate), (object)bitrate) ;

			GetVideoBitRate(out minKbps,out maxKbps,out isVBR);

		}
		
		public string VersionInfo
		{
			get
			{
				Log.Write("hauppauge: get version info {0}",Marshal.SizeOf(typeof(driverVersion) ));
				driverVersion version = new driverVersion();
				version.size=(uint)Marshal.SizeOf(typeof(driverVersion) );
				object obj =GetStructure(HauppaugeGuid,(uint)PropertyId.DriverVersion, typeof(driverVersion)) ;
				try
				{ 
					version = (driverVersion)obj ;
				}
				catch (Exception){}
				
				return String.Format("{0}.{1}.{2}.{3}",version.major,version.minor,version.revision,version.build);
			}
		}
	}
}