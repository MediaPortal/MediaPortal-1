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
using System.Collections;

using System.Runtime.InteropServices;

using DirectShowLib;
#pragma warning disable 618
namespace DShowNET.Helper
{
	public class FilterHelper
	{
    public static ArrayList GetVideoCompressors()
    {
      ArrayList compressors = new ArrayList();

      Filters filters = new Filters();
			
      foreach(Filter compressor in filters.VideoCompressors)
      {
        compressors.Add(compressor.Name);
      }

      return compressors;
    }

    public static ArrayList GetAudioCompressors()
    {
      ArrayList compressors = new ArrayList();

      Filters filters = new Filters();
			
      foreach(Filter compressor in filters.AudioCompressors)
      {
        compressors.Add(compressor.Name);
      }

      return compressors;
    }

    public static ArrayList GetAudioInputDevices()
    {
      ArrayList devices = new ArrayList();

      Filters filters = new Filters();
			
      foreach(Filter device in filters.AudioInputDevices)
      {
        devices.Add(device.Name);
      }

      return devices;
    }

		/// <summary>
		/// #MW#.
		/// Dirty, but simple hack to get the monikernames of the videoinput devices
		/// The other call already gets me the friendly names, but I DO need the
		/// monikers also, as many cards have the same friendly name (PVR150/250/350 etc.)
		/// </summary>
		/// <returns></returns>
		public static ArrayList GetVideoInputDeviceMonikers()
		{
			ArrayList devices = new ArrayList();

			Filters filters = new Filters();
			
			foreach(Filter device in filters.VideoInputDevices)
			{
				devices.Add(device.MonikerString);
			}
			//b2c2
			foreach(Filter device in filters.LegacyFilters)
			{
				if(device.Name=="B2C2 MPEG-2 Source")
					devices.Add(device.MonikerString);
			}
			
			foreach(Filter device in filters.BDAReceivers)
			{
				if (device.Name.ToLower() =="bda slip de-framer") continue;
				if (device.Name.ToLower() =="bda mpe filter") continue;	
				if (device.Name.ToLower() =="bda mpe-filter") continue;	
				devices.Add(device.MonikerString);
			}
			

			return devices;
		}

		public static ArrayList GetVideoInputDevices()
		{
			ArrayList devices = new ArrayList();

			Filters filters = new Filters();
			
			foreach(Filter device in filters.VideoInputDevices)
			{
				devices.Add(device.Name);
			}
			//b2c2
			foreach(Filter device in filters.LegacyFilters)
			{
				if(device.Name=="B2C2 MPEG-2 Source")
					devices.Add(device.Name);
			}
			
			foreach(Filter device in filters.BDAReceivers)
			{
				if (device.Name.ToLower() =="bda slip de-framer") continue;	
				if (device.Name.ToLower() =="bda mpe filter") continue;	
				if (device.Name.ToLower() =="bda mpe-filter") continue;	
				devices.Add(device.Name);
			}
			
			
			return devices;
		}

		public static ArrayList GetAudioRenderers()
		{
			ArrayList renderers = new ArrayList();

			Filters filters = new Filters();

			foreach(Filter audioRenderer in filters.AudioRenderers) 
			{
				renderers.Add(audioRenderer.Name);
			}

			return renderers;
		}

		public static ArrayList GetDVDNavigators()
		{
			ArrayList navigators = new ArrayList();

			Filters filters = new Filters();

			foreach (Filter filter in filters.LegacyFilters) 
			{
				//Now use only DVD Navigator as a selectable option to avoid issues
                if ( String.Compare(filter.Name, "DVD Navigator", true) == 0) //||
                    /*String.Compare(filter.Name, "InterVideo Navigator", true) == 0 ||
                    String.Compare(filter.Name, "NVIDIA Navigator", true) == 0 ||
                    String.Compare(filter.Name, "CyberLink DVD Navigator", true) == 0 ||
                    String.Compare(filter.Name, "Cyberlink DVD Navigator (ATI)", true) == 0 ||
                    String.Compare(filter.Name, "CyberLink DVD Navigator (PDVD6)", true) == 0)*/
				{
					navigators.Add(filter.Name);      
				}
			}
			
			return navigators;
    }

    public static void GetMPEG2AudioEncoders(ArrayList list)
    {
      Filters filters = new Filters();

      foreach (Filter filter in filters.LegacyFilters) 
      {
        
        bool add=false;
        //Cyberlink MPEG Audio encoder
        if (filter.MonikerString==@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{A3D70AC0-9023-11D2-8D55-0080C84E9C68}") add=true;
        if (add)
        {
          list.Add(filter.Name);      
        }
      }
    }

    public static void GetMPEG2VideoEncoders(ArrayList list)
    {
      Filters filters = new Filters();

      foreach (Filter filter in filters.LegacyFilters) 
      {
        
        bool add=false;
        //Cyberlink MPEG Video encoder
        if (filter.MonikerString==@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{36B46E60-D240-11D2-8F3F-0080C84E9806}") add=true;
        if (add)
        {
          list.Add(filter.Name);      
        }
      }
    }

		public static ArrayList GetFilters(Guid mediaType, Guid mediaSubType)
		{
			ArrayList filters = new ArrayList();

			UCOMIEnumMoniker enumMoniker = null;
      UCOMIMoniker[] moniker = new UCOMIMoniker[1];

      IFilterMapper2 mapper = (IFilterMapper2) new FilterMapper2();

			if(mapper != null)
			{						
				int hResult = mapper.EnumMatchingFilters(
					 out enumMoniker,
					0,
					true,
					(Merit)0x080001,
					true,
					1,
					new Guid[] {mediaType, mediaSubType},
					null,
          null,
					false,
					true,
					0,
					new Guid[0],
          null,
          null);

        int fetched=-1;
				do
				{
          hResult = enumMoniker.Next(1, moniker, out fetched);

					if((moniker[0] == null))
					{
						break;
					}
					
					string filterName = DirectShowUtil.GetFriendlyName(moniker[0]);
					filters.Add(filterName);
					
					moniker[0] = null;
				}
				while(true);
			}

			return filters;
			
		}
	}
}
