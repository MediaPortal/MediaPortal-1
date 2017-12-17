#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;

#pragma warning disable 618

namespace DShowNET.Helper
{
  public class FilterHelper
  {
    public static ArrayList GetVideoCompressors()
    {
      ArrayList compressors = new ArrayList();
      foreach (Filter compressor in Filters.VideoCompressors)
      {
        compressors.Add(compressor.Name);
      }
      return compressors;
    }

    public static ArrayList GetAudioCompressors()
    {
      ArrayList compressors = new ArrayList();
      foreach (Filter compressor in Filters.AudioCompressors)
      {
        compressors.Add(compressor.Name);
      }
      return compressors;
    }

    public static ArrayList GetAudioInputDevices()
    {
      ArrayList devices = new ArrayList();
      foreach (Filter device in Filters.AudioInputDevices)
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
      foreach (Filter device in Filters.VideoInputDevices)
      {
        devices.Add(device.MonikerString);
      }
      //b2c2
      foreach (Filter device in Filters.LegacyFilters)
      {
        if (device.Name == "B2C2 MPEG-2 Source")
        {
          devices.Add(device.MonikerString);
        }
      }
      foreach (Filter device in Filters.BDAReceivers)
      {
        if (device.Name.ToLowerInvariant() == "bda slip de-framer")
        {
          continue;
        }
        if (device.Name.ToLowerInvariant() == "bda mpe filter")
        {
          continue;
        }
        if (device.Name.ToLowerInvariant() == "bda mpe-filter")
        {
          continue;
        }
        devices.Add(device.MonikerString);
      }
      return devices;
    }

    public static ArrayList GetVideoInputDevices()
    {
      ArrayList devices = new ArrayList();
      foreach (Filter device in Filters.VideoInputDevices)
      {
        devices.Add(device.Name);
      }
      //b2c2
      foreach (Filter device in Filters.LegacyFilters)
      {
        if (device.Name == "B2C2 MPEG-2 Source")
        {
          devices.Add(device.Name);
        }
      }
      foreach (Filter device in Filters.BDAReceivers)
      {
        if (device.Name.ToLowerInvariant() == "bda slip de-framer")
        {
          continue;
        }
        if (device.Name.ToLowerInvariant() == "bda mpe filter")
        {
          continue;
        }
        if (device.Name.ToLowerInvariant() == "bda mpe-filter")
        {
          continue;
        }
        devices.Add(device.Name);
      }
      return devices;
    }

    public static ArrayList GetAudioRenderers()
    {
      ArrayList renderers = new ArrayList();
      foreach (Filter audioRenderer in Filters.AudioRenderers)
      {
        renderers.Add(audioRenderer.Name);
      }
      return renderers;
    }

    public static void ReloadFilterCollection()
    {
      Filters.FilterCollectionReload();
    }

    public static ArrayList GetDVDNavigators()
    {
      ArrayList navigators = new ArrayList();

      foreach (Filter filter in Filters.LegacyFilters)
      {
        if (String.Compare(filter.Name, "DVD Navigator", true) == 0 ||
            String.Compare(filter.Name, "InterVideo Navigator", true) == 0 ||
            String.Compare(filter.Name, "NVIDIA Navigator", true) == 0 ||
            filter.Name.ToLowerInvariant().Contains("cyberlink dvd navigator"))
        {
          navigators.Add(filter.Name);
        }
      }

      return navigators;
    }

    public static void GetMPEG2AudioEncoders(ArrayList list)
    {
      foreach (Filter filter in Filters.LegacyFilters)
      {
        bool add = false;
        //Cyberlink MPEG Audio encoder
        if (filter.MonikerString ==
            @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{A3D70AC0-9023-11D2-8D55-0080C84E9C68}")
        {
          add = true;
        }
        if (add)
        {
          list.Add(filter.Name);
        }
      }
    }

    public static void GetMPEG2VideoEncoders(ArrayList list)
    {
      foreach (Filter filter in Filters.LegacyFilters)
      {
        bool add = false;
        //Cyberlink MPEG Video encoder
        if (filter.MonikerString ==
            @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{36B46E60-D240-11D2-8F3F-0080C84E9806}")
        {
          add = true;
        }
        if (add)
        {
          list.Add(filter.Name);
        }
      }
    }

    public static ArrayList GetFileSync()
    {
      ArrayList list = new ArrayList();
      foreach (Filter filter in Filters.LegacyFilters)
      {
        bool add = false;
        if (filter.MonikerString ==
            @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{E436EBB5-524F-11CE-9F53-0020AF0BA770}")
        {
          add = true;
        }
        if (add)
        {
          list.Add(filter.Name);
        }
      }
      return list;
    }

    public static ArrayList GetFilterSource()
    {
      ArrayList sources = new ArrayList();

      foreach (Filter filter in Filters.LegacyFilters)
      {
        if (filter.Name.ToLowerInvariant().Contains("source"))
        {
          sources.Add(filter.Name);
        }
      }
      return sources;
    }

    public static ArrayList GetFilterSplitter()
    {
      ArrayList sources = new ArrayList();

      foreach (Filter filter in Filters.LegacyFilters)
      {
        if (filter.Name.ToLowerInvariant().Contains("splitter"))
        {
          sources.Add(filter.Name);
        }
      }
      return sources;
    }

    public static ArrayList GetVideoCodec()
    {
      ArrayList list = FilterHelper.GetFilters(MediaType.Video, MediaSubType.Null);
      return list;
    }

    public static ArrayList GetAudioCodec()
    {
      ArrayList list = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Null);
      return list;
    }

    public static ArrayList GetFilters(Guid mediaType, Guid mediaSubType)
    {
      return GetFilters(mediaType, mediaSubType, (Merit) 0x080001);
    }

    public static ArrayList GetFilters(Guid mediaType, Guid mediaSubType, Merit merit)
    {
      return GetFilters(new Guid[] {mediaType}, new Guid[] {mediaSubType}, merit);
    }

    public static ArrayList GetFilters(Guid[] mediaType, Guid[] mediaSubType, Merit merit)
    {
      ArrayList filters = new ArrayList();
      IEnumMoniker enumMoniker = null;
      IMoniker[] moniker = new IMoniker[1];
      IFilterMapper2 mapper = (IFilterMapper2) new FilterMapper2();
      Guid[] types = new Guid[mediaType.Length*2];
      for (int i = 0; i < mediaType.Length; i++)
      {
        types[i*2] = mediaType[i];
        types[i*2 + 1] = mediaSubType[i];
      }
      try
      {
        if (mapper != null)
        {
          int hResult = mapper.EnumMatchingFilters(
            out enumMoniker,
            0,
            true,
            merit,
            true,
            mediaType.Length,
            types,
            null,
            null,
            false,
            true,
            0,
            new Guid[0],
            null,
            null);
          IntPtr fetched = IntPtr.Zero;
          do
          {
            try
            {
              enumMoniker.Next(1, moniker, IntPtr.Zero);
            }
            catch
            {
            }

            if ((moniker[0] == null))
              break;

            string filterName = DirectShowUtil.GetFriendlyName(moniker[0]);
            if (!String.IsNullOrEmpty(filterName))
              filters.Add(filterName);
            DirectShowUtil.TryRelease(ref moniker[0]);
          } while (true);
        }
        return filters;
      }
      finally
      {
        DirectShowUtil.ReleaseComObject(enumMoniker);
        DirectShowUtil.ReleaseComObject(mapper);
      }
    }
  }
}