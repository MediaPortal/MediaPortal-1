using System;
using System.Collections;

using System.Runtime.InteropServices;

using DShowNET;
using DShowNET.Device;

namespace MediaPortal.Configuration
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

		public static ArrayList GetVideoInputDevices()
		{
			ArrayList devices = new ArrayList();

			Filters filters = new Filters();
			
			foreach(Filter device in filters.VideoInputDevices)
			{
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
				if ( String.Compare(filter.Name, "DVD Navigator", true) == 0 ||
					String.Compare(filter.Name, "InterVideo Navigator", true) == 0 ||
					String.Compare(filter.Name, "CyberLink DVD Navigator", true) == 0)
				{
					navigators.Add(filter.Name);      
				}
			}
			
			return navigators;
		}

		public static ArrayList GetFilters(Guid mediaType, Guid mediaSubType)
		{
			ArrayList filters = new ArrayList();

			Type filterMapperType = Type.GetTypeFromCLSID(Clsid.Clsid_FilterMapper2);
			if(filterMapperType != null)
			{
				int hResult;
				object comObject = null;

				UCOMIEnumMoniker enumMoniker = null;
				UCOMIMoniker[] moniker = new UCOMIMoniker[1];

				comObject = Activator.CreateInstance(filterMapperType);
				IFilterMapper2 mapper = comObject as IFilterMapper2;

				if(mapper != null)
				{						
					hResult = mapper.EnumMatchingFilters(
						out enumMoniker,
						0,
						true,
						0x080001,
						true,
						1,
						new Guid[] {mediaType, mediaSubType},
						IntPtr.Zero,
						IntPtr.Zero,
						false,
						true,
						0,
						new Guid[0],
						IntPtr.Zero,
						IntPtr.Zero);
					
					do
					{
						int dummy;
						hResult = enumMoniker.Next(1, moniker, out dummy);

						if((moniker[0] == null))
						{
							break;
						}
						
						string filterName = DShowNET.DsUtils.GetFriendlyName(moniker[0]);
						filters.Add(filterName);
						
						moniker[0] = null;
					}
					while(true);
				}
			}

			return filters;
			
		}
	}
}
