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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using DirectShowLib;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;

#pragma warning disable 618

namespace DShowNET.Helper
{
  /// <summary>
  /// 
  /// </summary>
  public class DirectShowUtil
  {
    private const int magicConstant = -759872593;

    static DirectShowUtil() {}

    public static IBaseFilter AddFilterToGraph(IGraphBuilder graphBuilder, string strFilterName)
    {
      try
      {
        IBaseFilter NewFilter = null;
        foreach (Filter filter in Filters.LegacyFilters)
        {
          if (String.Compare(filter.Name, strFilterName, true) == 0)
          {
            NewFilter = (IBaseFilter)Marshal.BindToMoniker(filter.MonikerString);

            int hr = graphBuilder.AddFilter(NewFilter, strFilterName);
            if (hr < 0)
            {
              Log.Error("Failed: Unable to add filter: {0} to graph", strFilterName);
              NewFilter = null;
            }
            else
            {
              Log.Info("Added filter: {0} to graph", strFilterName);
            }
            break;
          }
        }
        if (NewFilter == null)
        {
          Log.Error("Failed filter: {0} not found", strFilterName);
        }
        return NewFilter;
      }
      catch (Exception ex)
      {
        Log.Error("Failed filter: {0} not found {0}", strFilterName, ex.Message);
        return null;
      }
    }

    public static IBaseFilter AddAudioRendererToGraph(IGraphBuilder graphBuilder, string strFilterName,
                                                      bool setAsReferenceClock)
    {
      try
      {
        IPin pinOut = null;
        IBaseFilter NewFilter = null;
        IEnumFilters enumFilters;
        HResult hr = new HResult(graphBuilder.EnumFilters(out enumFilters));
        Log.Info("DirectShowUtils: First try to insert new audio renderer {0} ", strFilterName);
        // next add the new one...
        foreach (Filter filter in Filters.AudioRenderers)
        {
          if (String.Compare(filter.Name, strFilterName, true) == 0)
          {
            Log.Info("DirectShowUtils: Found audio renderer");
            NewFilter = (IBaseFilter)Marshal.BindToMoniker(filter.MonikerString);
            hr.Set(graphBuilder.AddFilter(NewFilter, strFilterName));
            if (hr < 0)
            {
              Log.Error("DirectShowUtils: unable to add filter:{0} to graph", strFilterName);
              NewFilter = null;
            }
            else
            {
              Log.Debug("DirectShowUtils: added filter:{0} to graph", strFilterName);
              if (pinOut != null)
              {
                hr.Set(graphBuilder.Render(pinOut));
                if (hr == 0)
                {
                  Log.Info(" pinout rendererd");
                }
                else
                {
                  Log.Error(" failed: pinout render");
                }
              }
              if (setAsReferenceClock)
              {
                hr.Set((graphBuilder as IMediaFilter).SetSyncSource(NewFilter as IReferenceClock));
                Log.Debug("setAsReferenceClock sync source " + hr.ToDXString());
              }
              return NewFilter;
            }
          } //if (String.Compare(filter.Name,strFilterName,true) ==0)
        } //foreach (Filter filter in filters.AudioRenderers)
        if (NewFilter == null)
        {
          Log.Error("DirectShowUtils: failed filter {0} not found", strFilterName);
        }
      }
      catch {}
      Log.Info("DirectShowUtils: First try to insert new audio renderer {0} failed ", strFilterName);

      try
      {
        IPin pinOut = null;
        IBaseFilter NewFilter = null;
        Log.Info("add filter:{0} to graph clock:{1}", strFilterName, setAsReferenceClock);

        //check first if audio renderer exists!
        bool bRendererExists = false;
        foreach (Filter filter in Filters.AudioRenderers)
        {
          if (String.Compare(filter.Name, strFilterName, true) == 0)
          {
            bRendererExists = true;
            Log.Info("DirectShowUtils: found renderer - {0}", filter.Name);
          }
        }
        if (!bRendererExists)
        {
          Log.Error("FAILED: audio renderer:{0} doesnt exists", strFilterName);
          return null;
        }

        // first remove all audio renderers
        bool bAllRemoved = false;
        bool bNeedAdd = true;
        IEnumFilters enumFilters;
        HResult hr = new HResult(graphBuilder.EnumFilters(out enumFilters));

        if (hr >= 0 && enumFilters != null)
        {
          int iFetched;
          enumFilters.Reset();
          while (!bAllRemoved)
          {
            IBaseFilter[] pBasefilter = new IBaseFilter[2];
            hr.Set(enumFilters.Next(1, pBasefilter, out iFetched));
            if (hr < 0 || iFetched != 1 || pBasefilter[0] == null)
            {
              break;
            }

            foreach (Filter filter in Filters.AudioRenderers)
            {
              Guid classId1;
              Guid classId2;

              pBasefilter[0].GetClassID(out classId1);
              //Log.Info("Filter Moniker string -  " + filter.Name);
              if (filter.Name == "ReClock Audio Renderer")
              {
                Log.Warn(
                  "Reclock is installed - if this method fails, reinstall and regsvr32 /u reclock and then uninstall");
                //   return null;
              }

              try
              {
                NewFilter = (IBaseFilter)Marshal.BindToMoniker(filter.MonikerString);
                if (NewFilter == null)
                {
                  Log.Info("NewFilter = null");
                  continue;
                }
              }
              catch (Exception e)
              {
                Log.Info("Exception in BindToMoniker({0}): {1}", filter.MonikerString, e.Message);
                continue;
              }
              NewFilter.GetClassID(out classId2);
              ReleaseComObject(NewFilter);
              NewFilter = null;

              if (classId1.Equals(classId2))
              {
                if (filter.Name == strFilterName)
                {
                  Log.Info("filter already in graph");

                  if (setAsReferenceClock)
                  {
                    hr.Set((graphBuilder as IMediaFilter).SetSyncSource(pBasefilter[0] as IReferenceClock));
                    Log.Info("setAsReferenceClock sync source " + hr.ToDXString());
                  }
                  ReleaseComObject(pBasefilter[0]);
                  pBasefilter[0] = null;
                  bNeedAdd = false;
                  break;
                }
                else
                {
                  Log.Info("remove " + filter.Name + " from graph");
                  pinOut = FindSourcePinOf(pBasefilter[0]);
                  graphBuilder.RemoveFilter(pBasefilter[0]);
                  bAllRemoved = true;
                  break;
                }
              } //if (classId1.Equals(classId2))
            } //foreach (Filter filter in filters.AudioRenderers)
            if (pBasefilter[0] != null)
            {
              ReleaseComObject(pBasefilter[0]);
            }
          } //while(!bAllRemoved)
          ReleaseComObject(enumFilters);
        } //if (hr>=0 && enumFilters!=null)
        Log.Info("DirectShowUtils: Passed removing audio renderer");
        if (!bNeedAdd)
        {
          return null;
        }
        // next add the new one...
        foreach (Filter filter in Filters.AudioRenderers)
        {
          if (String.Compare(filter.Name, strFilterName, true) == 0)
          {
            Log.Info("DirectShowUtils: Passed finding Audio Renderer");
            NewFilter = (IBaseFilter)Marshal.BindToMoniker(filter.MonikerString);
            hr.Set(graphBuilder.AddFilter(NewFilter, strFilterName));
            if (hr < 0)
            {
              Log.Error("failed:unable to add filter:{0} to graph", strFilterName);
              NewFilter = null;
            }
            else
            {
              Log.Debug("added filter:{0} to graph", strFilterName);
              if (pinOut != null)
              {
                hr.Set(graphBuilder.Render(pinOut));
                if (hr == 0)
                {
                  Log.Info(" pinout rendererd");
                }
                else
                {
                  Log.Error(" failed: pinout render");
                }
              }
              if (setAsReferenceClock)
              {
                hr.Set((graphBuilder as IMediaFilter).SetSyncSource(NewFilter as IReferenceClock));
                Log.Debug("setAsReferenceClock sync source " + hr.ToDXString());
              }
              return NewFilter;
            }
          } //if (String.Compare(filter.Name,strFilterName,true) ==0)
        } //foreach (Filter filter in filters.AudioRenderers)
        if (NewFilter == null)
        {
          Log.Error("failed filter:{0} not found", strFilterName);
        }
      }
      catch (Exception ex)
      {
        Log.Error("DirectshowUtil. Failed to add filter:{0} to graph :{1} {2} {3}",
                  strFilterName, ex.Message, ex.Source, ex.StackTrace);
      }
      return null;
    }

    public static IPin FindSourcePinOf(IBaseFilter filter)
    {
      int hr = 0;
      IEnumPins pinEnum;
      hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int f;
        do
        {
          // Get the next pin
          hr = pinEnum.Next(1, pins, out f);
          if ((hr == 0) && (pins[0] != null))
          {
            PinDirection pinDir;
            pins[0].QueryDirection(out pinDir);
            if (pinDir == PinDirection.Input)
            {
              IPin pSourcePin = null;
              hr = pins[0].ConnectedTo(out pSourcePin);
              if (hr >= 0)
              {
                ReleaseComObject(pinEnum);
                return pSourcePin;
              }
            }
            ReleaseComObject(pins[0]);
          }
        } while (hr == 0);
        ReleaseComObject(pinEnum);
      }
      return null;
    }

    private static void ListMediaTypes(IPin pin)
    {
      IEnumMediaTypes types;
      pin.EnumMediaTypes(out types);
      types.Reset();
      while (true)
      {
        AMMediaType[] mediaTypes = new AMMediaType[1];
        int typesFetched;
        int hr = types.Next(1, mediaTypes, out typesFetched);
        if (hr != 0 || typesFetched == 0)
        {
          break;
        }
        Log.Info("Has output type: {0}, {1}", mediaTypes[0].majorType,
                 mediaTypes[0].subType);
      }
      ReleaseComObject(types);
      Log.Info("-----EndofTypes");
    }

    private static bool TestMediaTypes(IPin pin, IPin receiver)
    {
      bool ret = false;
      IEnumMediaTypes types;
      pin.EnumMediaTypes(out types);
      types.Reset();
      while (true)
      {
        AMMediaType[] mediaTypes = new AMMediaType[1];
        int typesFetched;
        int hr = types.Next(1, mediaTypes, out typesFetched);
        if (hr != 0 || typesFetched == 0)
        {
          break;
        }
        //Log.Info("Check output type: {0}, {1}", mediaTypes[0].majorType,
        //  mediaTypes[0].subType);
        if (receiver.QueryAccept(mediaTypes[0]) == 0)
        {
          //Log.Info("Accepted!");
          ret = true;
          break;
        }
      }
      ReleaseComObject(types);
      //Log.Info("-----EndofTypes");
      return ret;
    }

    private static bool TryConnect(IGraphBuilder graphBuilder, string filtername, IPin outputPin)
    {
      return TryConnect(graphBuilder, filtername, outputPin, true);
    }

    private static bool CheckFilterIsLoaded(IGraphBuilder graphBuilder, String name)
    {
      int hr;
      bool ret = false;
      IEnumFilters enumFilters;
      graphBuilder.EnumFilters(out enumFilters);
      do
      {
        int ffetched;
        IBaseFilter[] filters = new IBaseFilter[1];
        hr = enumFilters.Next(1, filters, out ffetched);
        if (hr == 0 && ffetched > 0)
        {
          FilterInfo info;
          filters[0].QueryFilterInfo(out info);
          ReleaseComObject(info.pGraph);
          
          string filtername = info.achName;
          ReleaseComObject(filters[0]);
          if (filtername.Equals(name))
          {
            ret = true;
            break;
          }
        }
        else
        {
          break;
        }
      } while (true);
      ReleaseComObject(enumFilters);
      return ret;
    }

    private static bool HasConnection(IPin pin)
    {
      IPin pinInConnected;
      int hr = pin.ConnectedTo(out pinInConnected);
      if (hr != 0 || pinInConnected == null)
      {
        return false;
      }
      else
      {
        ReleaseComObject(pinInConnected);
        return true;
      }
    }

    private static bool TryConnect(IGraphBuilder graphBuilder, string filtername, IPin outputPin, IBaseFilter to)
    {
      bool ret = false;
      int hr;
      FilterInfo info;
      PinInfo outputInfo;
      
      to.QueryFilterInfo(out info);
      ReleaseComObject(info.pGraph);
      
      outputPin.QueryPinInfo(out outputInfo);
      DsUtils.FreePinInfo(outputInfo);

      if (info.achName.Equals(filtername))
      {
        return false; //do not connect to self
      }
      Log.Debug("Testing filter: {0}", info.achName);

      IEnumPins enumPins;
      IPin[] pins = new IPin[1];
      to.EnumPins(out enumPins);
      do
      {
        int pinsFetched;
        hr = enumPins.Next(1, pins, out pinsFetched);
        if (hr != 0 || pinsFetched == 0)
        {
          break;
        }
        PinDirection direction;
        pins[0].QueryDirection(out direction);
        if (direction == PinDirection.Input && !HasConnection(pins[0])) // && TestMediaTypes(outputPin, pins[0]))
        {
          PinInfo pinInfo;
          pins[0].QueryPinInfo(out pinInfo);
          DsUtils.FreePinInfo(pinInfo);
          Log.Debug("Testing compatibility to {0}",
                    pinInfo.name);
          //ListMediaTypes(pins[0]);
          //hr =  outputPin.Connect(pins[0], null);
          hr = graphBuilder.ConnectDirect(outputPin, pins[0], null);
          if (hr == 0)
          {
            Log.Debug("Connection succeeded");
            if (RenderOutputPins(graphBuilder, to))
            {
              Log.Info("Successfully rendered pin {0}:{1} to {2}:{3}.",
                       filtername, outputInfo.name, info.achName, pinInfo.name);
              ret = true;
              ReleaseComObject(pins[0]);
              break;
            }
            else
            {
              Log.Debug("Rendering got stuck. Trying next filter, and disconnecting {0}!", outputInfo.name);
              outputPin.Disconnect();
              pins[0].Disconnect();
            }
          }
          else
          {
            Log.Debug("Could not connect, filters are not compatible: {0:x}", hr);
          }
        }
        ReleaseComObject(pins[0]);
      } while (true);
      ReleaseComObject(enumPins);
      if (!ret)
      {
        Log.Debug("Dead end. Could not successfully connect pin {0} to filter {1}!", outputInfo.name, info.achName);
      }
      return ret;
    }

    private static uint ReverseByteArrayToDWORD(Byte[] ba)
    {
      //Log.Info("Reversing: {0:x}{1:x}{2:x}{3:x}", ba[0], ba[1], ba[2], ba[3]);
      return (uint)(((uint)ba[3] << 24) | ((uint)ba[2] << 16) | ((uint)ba[1] << 8) | ba[0]);
    }

    /// <summary>
    /// checks if the filter has any output pins. if so, returns false, otherwise true
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>true, if the given filter is a render filter, false otherwise</returns>
    private static bool IsRenderer(IBaseFilter filter)
    {
      IEnumPins pinEnum;
      int hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        try
        {
          //Log.Info("got pins");
          pinEnum.Reset();
          IPin[] pins = new IPin[1];
          int iFetched;
          int iPinNo = 0;
          do
          {
            // Get the next pin
            //Log.Info("  get pin:{0}",iPinNo);
            iPinNo++;
            hr = pinEnum.Next(1, pins, out iFetched);
            if (hr == 0 && iFetched == 1)
            {
              //Log.Info("  find pin info");
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                ReleaseComObject(pins[0]);
                //we got at least one output pin, this is not a render filter
                return false;
              }
              ReleaseComObject(pins[0]);
            }
            else
            {
              return true;
            }
          } while (true);
        }
        finally
        {
          ReleaseComObject(pinEnum);
        }
      }
      //No output pins found, this is a renderer
      return true;
    }

    private static Dictionary<Guid, Merit> _meritCache = new Dictionary<Guid, Merit>();

    private static Merit GetMerit(IBaseFilter filter)
    {
      Guid clsid;
      int hr;
      hr = filter.GetClassID(out clsid);
      if (hr != 0)
      {
        return Merit.DoNotUse;
      }
      //check cache
      if (_meritCache.ContainsKey(clsid))
      {
        return _meritCache[clsid];
      }
      //figure new value
      try
      {
        RegistryKey filterKey =
          Registry.ClassesRoot.OpenSubKey(@"CLSID\{083863F1-70DE-11d0-BD40-00A0C911CE86}\Instance\{" + clsid.ToString() +
                                          @"}");
        if (filterKey == null)
        {
          Log.Debug("Could not get merit value for clsid {0}, key not found!", clsid);
          _meritCache[clsid] = Merit.DoNotUse;
          return Merit.DoNotUse;
        }
        Byte[] filterData = (Byte[])filterKey.GetValue("FilterData", 0x0);
        if (filterData == null || filterData.Length < 8)
        {
          return Merit.DoNotUse;
        }
        Byte[] merit = new Byte[4];
        //merit is 2nd DWORD, reverse byte order
        Array.Copy(filterData, 4, merit, 0, 4);
        uint dwMerit = ReverseByteArrayToDWORD(merit);
        _meritCache[clsid] = (Merit)dwMerit;
        return (Merit)dwMerit;
      }
      catch (Exception e)
      {
        Log.Debug("Could not get merit value for clsid {0}. Error: {1}", clsid, e.Message);
        return Merit.DoNotUse;
      }
    }

    private static void LogFilters(ArrayList filters)
    {
      int nr = 1;
      foreach (IBaseFilter filter in filters)
      {
        FilterInfo i;
        filter.QueryFilterInfo(out i);
        Log.Debug("FILTER: {0}: {1}", nr++, i.achName);
        ReleaseComObject(i.pGraph);
      }
    }

    /// <summary>
    /// Try to sort the Filters that are currently loaded by Intermediate -> Renderer
    /// and then by Merit, desc.
    /// 
    /// </summary>
    /// <param name="graphBuilder"></param>
    /// <returns></returns>
    private static ArrayList GetFilters(IGraphBuilder graphBuilder)
    {
      //Sources+Intermediates
      ArrayList allMerits = new ArrayList();
      ArrayList allFilters = new ArrayList();
      //Renderers
      ArrayList allMeritsR = new ArrayList();
      ArrayList allFiltersR = new ArrayList();
      IEnumFilters enumFilters;
      graphBuilder.EnumFilters(out enumFilters);
      for (;;)
      {
        int ffetched;
        IBaseFilter[] filters = new IBaseFilter[1];
        int hr = enumFilters.Next(1, filters, out ffetched);
        if (hr == 0 && ffetched > 0)
        {
          uint m = (uint)GetMerit(filters[0]);
          //substract merit from uint.maxvalue to get reverse ordering from highest merit to lowest merit
          if (IsRenderer(filters[0]))
          {
            allMeritsR.Add(uint.MaxValue - m);
            allFiltersR.Add(filters[0]);
          }
          else
          {
            allMerits.Add(uint.MaxValue - m);
            allFilters.Add(filters[0]);
          }
        }
        else
        {
          break;
        }
      }
      ReleaseComObject(enumFilters);
      //if someone has a better way to sort the filters by their merits, PLEASE change the following
      //(i know there must be something more elegant)
      Array aM = allMerits.ToArray(typeof (uint));
      Array aF = allFilters.ToArray(typeof (IBaseFilter));
      Array aMR = allMeritsR.ToArray(typeof (uint));
      Array aFR = allFiltersR.ToArray(typeof (IBaseFilter));
      Array.Sort(aM, aF);
      Array.Sort(aMR, aFR);
      ArrayList ret = new ArrayList();
      //add all itermediate+sources first, then add renderers
      ret.AddRange(aF);
      ret.AddRange(aFR);
      LogFilters(ret);
      return ret;
    }

    private static void ReleaseFilters(ArrayList filters)
    {
      foreach (IBaseFilter filter in filters)
      {
        ReleaseComObject(filter);
      }
    }
    
    public static bool TryConnect(IGraphBuilder graphbuilder, IBaseFilter source, Guid mediaType, string targetFilter)
    {
      if (string.IsNullOrEmpty(targetFilter))
        return false;

      bool connected = false;
      IBaseFilter destination = null;
      destination = AddFilterToGraph(graphbuilder, targetFilter);

      if (destination == null)
        return false;

      if (!TryConnect(graphbuilder, source, mediaType, destination))
        graphbuilder.RemoveFilter(destination);
      else
        connected = true;

      ReleaseComObject(destination); destination = null;
      return connected;
    }

    public static bool TryConnect(IGraphBuilder graphbuilder, IBaseFilter source, Guid mediaType, IBaseFilter targetFilter)
    {
      bool connected = false;
      IEnumPins enumPins;
      int hr = source.EnumPins(out enumPins);
      DsError.ThrowExceptionForHR(hr);
      IPin[] pins = new IPin[1];
      int fetched = 0;
      while (enumPins.Next(1, pins, out fetched) == 0)
      {
        if (fetched != 1)
        {
          break;
        }
        PinDirection direction;
        pins[0].QueryDirection(out direction);
        if (direction == PinDirection.Output)
        {
          IEnumMediaTypes enumMediaTypes;
          pins[0].EnumMediaTypes(out enumMediaTypes);
          AMMediaType[] mediaTypes = new AMMediaType[20];
          int fetchedTypes;
          enumMediaTypes.Next(20, mediaTypes, out fetchedTypes);
          for (int i = 0; i < fetchedTypes; ++i)
          {
            if (mediaTypes[i].majorType == mediaType)
            {
              if (graphbuilder.ConnectDirect(pins[0], DsFindPin.ByDirection(targetFilter, PinDirection.Input, 0), null) >= 0)
              {
                connected = true;
                break;
              }
            }
          }
        }
        ReleaseComObject(pins[0]);
      }
      ReleaseComObject(enumPins);
      return connected;
    }

    public static bool TryConnect(IGraphBuilder graphBuilder, string filtername, IPin outputPin, bool TryNewFilters)
    {
      int hr;
      Log.Info("----------------TryConnect-------------");
      PinInfo outputInfo;
      outputPin.QueryPinInfo(out outputInfo);
      DsUtils.FreePinInfo(outputInfo);
      //ListMediaTypes(outputPin);
      ArrayList currentfilters = GetFilters(graphBuilder);
      foreach (IBaseFilter filter in currentfilters)
      {
        if (TryConnect(graphBuilder, filtername, outputPin, filter))
        {
          ReleaseFilters(currentfilters);
          return true;
        }
      }
      ReleaseFilters(currentfilters);
      //not found, try new filter from registry
      if (TryNewFilters)
      {
        Log.Info("No preloaded filter could be connected. Trying to load new one from registry");
        IEnumMediaTypes enumTypes;
        hr = outputPin.EnumMediaTypes(out enumTypes);
        if (hr != 0)
        {
          Log.Debug("Failed: {0:x}", hr);
          return false;
        }
        Log.Debug("Got enum");
        ArrayList major = new ArrayList();
        ArrayList sub = new ArrayList();

        Log.Debug("Getting corresponding filters");
        for (; ; )
        {
          AMMediaType[] mediaTypes = new AMMediaType[1];
          int typesFetched;
          hr = enumTypes.Next(1, mediaTypes, out typesFetched);
          if (hr != 0 || typesFetched == 0)
          {
            break;
          }
          major.Add(mediaTypes[0].majorType);
          sub.Add(mediaTypes[0].subType);
        }
        ReleaseComObject(enumTypes);
        Log.Debug("Found {0} media types", major.Count);
        Guid[] majorTypes = (Guid[])major.ToArray(typeof(Guid));
        Guid[] subTypes = (Guid[])sub.ToArray(typeof(Guid));
        Log.Debug("Loading filters");
        ArrayList filters = FilterHelper.GetFilters(majorTypes, subTypes, (Merit)0x00400000);
        Log.Debug("Loaded {0} filters", filters.Count);
        foreach (string name in filters)
        {
          if (!CheckFilterIsLoaded(graphBuilder, name))
          {
            Log.Debug("Loading filter: {0}", name);
            IBaseFilter f = AddFilterToGraph(graphBuilder, name);
            if (f != null)
            {
              if (TryConnect(graphBuilder, filtername, outputPin, f))
              {
                ReleaseComObject(f);
                return true;
              }
              else
              {
                graphBuilder.RemoveFilter(f);
                ReleaseComObject(f);
              }
            }
          }
          else
          {
            Log.Debug("Ignoring filter {0}. Already in graph.", name);
          }
        }
      }
      Log.Debug("TryConnect failed.");
      return outputInfo.name.StartsWith("~");
    }

    public static void RenderGraphBuilderOutputPins(IGraphBuilder graphBuilder, IBaseFilter baseFilter)
    {
      if (graphBuilder == null)
        return;
      if (baseFilter != null)
        RenderUnconnectedOutputPins(graphBuilder, baseFilter);

      int hr = 0;
      IEnumFilters enumFilters = null;
      ArrayList filtersArray = new ArrayList();
      hr = graphBuilder.EnumFilters(out enumFilters);
      DsError.ThrowExceptionForHR(hr);

      IBaseFilter[] filters = new IBaseFilter[1];
      int fetched;

      while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
      {
        filtersArray.Add(filters[0]);
      }

      foreach (IBaseFilter filter in filtersArray)
      {
        if (filter != baseFilter)
        {
          RenderUnconnectedOutputPins(graphBuilder, filter);
        }
        else
        {
          break;
        }
      }
      ReleaseComObject(enumFilters);
    }

    public static void RenderUnconnectedOutputPins(IGraphBuilder graphBuilder, IBaseFilter baseFilter)
    {
      if (baseFilter == null)
        return;

      int fetched;
      IEnumPins pinEnum;
      int hr = baseFilter.EnumPins(out pinEnum);
      DsError.ThrowExceptionForHR(hr);
      if (hr == 0 && pinEnum != null)
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        while (pinEnum.Next(1, pins, out fetched) == 0 && fetched > 0)
        {
          PinDirection pinDir;
          pins[0].QueryDirection(out pinDir);
          if (pinDir == PinDirection.Output && !HasConnection(pins[0]))
          {
            FilterInfo i;
            PinInfo pinInfo;
            string pinName = string.Empty;
            if (baseFilter.QueryFilterInfo(out i) == 0)
            {
              if (pins[0].QueryPinInfo(out pinInfo) == 0)
              {
                Log.Debug("Filter: {0} - try to connect: {1}", i.achName, pinInfo.name);
                pinName = pinInfo.name;
                DsUtils.FreePinInfo(pinInfo);
              }
            }

            ReleaseComObject(i.pGraph);
            hr = graphBuilder.Render(pins[0]);
            if (hr != 0)
              Log.Debug(" - failed");
          }
          ReleaseComObject(pins[0]);
        }
        ReleaseComObject(pinEnum);
      }
    }

    public static bool RenderOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter)
    {
      return RenderOutputPins(graphBuilder, filter, 100, true);
    }

    public static bool RenderOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter, bool tryAllFilters)
    {
      return RenderOutputPins(graphBuilder, filter, 100, tryAllFilters);
    }

    public static bool RenderOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter, int maxPinsToRender)
    {
      return RenderOutputPins(graphBuilder, filter, maxPinsToRender, true);
    }

    public static bool RenderOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter, int maxPinsToRender, bool tryAllFilters)
    {
      int pinsRendered = 0;
      bool bAllConnected = true;
      IEnumPins pinEnum;
      FilterInfo info;
      filter.QueryFilterInfo(out info);
      ReleaseComObject(info.pGraph);
      
      int hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        Log.Info("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo = 0;
        do
        {
          // Get the next pin
          //Log.Info("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next(1, pins, out iFetched);
          if (hr == 0)
          {
            if (iFetched == 1 && pins[0] != null)
            {
              PinInfo pinInfo = new PinInfo();
              hr = pins[0].QueryPinInfo(out pinInfo);
              DsUtils.FreePinInfo(pinInfo);
              if (hr == 0)
              {
                Log.Info("  got pin#{0}:{1}", iPinNo - 1, pinInfo.name);
              }
              else
              {
                Log.Info("  got pin:?");
              }
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                IPin pConnectPin = null;
                hr = pins[0].ConnectedTo(out pConnectPin);
                if (hr != 0 || pConnectPin == null)
                {
                  hr = 0;
                  if (TryConnect(graphBuilder, info.achName, pins[0], tryAllFilters))
                    //if ((hr=graphBuilder.Render(pins[0])) == 0)
                  {
                    Log.Info("  render ok");
                  }
                  else
                  {
                    Log.Error(" render {0} failed:{1:x}, trying alternative graph builder", pinInfo.name, hr);

                    if ((hr = graphBuilder.Render(pins[0])) == 0)
                    {
                      Log.Info(" render ok");
                    }
                    else
                    {
                      Log.Error(" render failed:{0:x}", hr);
                      bAllConnected = false;
                    }
                  }
                  pinsRendered++;
                }
                if (pConnectPin != null)
                {
                  ReleaseComObject(pConnectPin);
                }
                pConnectPin = null;
                //else Log.Info("pin is already connected");
              }
              ReleaseComObject(pins[0]);
            }
            else
            {
              iFetched = 0;
              Log.Info("no pins?");
              break;
            }
          }
          else
          {
            iFetched = 0;
          }
        } while (iFetched == 1 && pinsRendered < maxPinsToRender && bAllConnected);
        ReleaseComObject(pinEnum);
      }
      return bAllConnected;
    }

    public static void DisconnectOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter)
    {
      IEnumPins pinEnum;
      int hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        //Log.Info("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo = 0;
        do
        {
          // Get the next pin
          //Log.Info("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next(1, pins, out iFetched);
          if (hr == 0)
          {
            if (iFetched == 1 && pins[0] != null)
            {
              //Log.Info("  find pin info");
              PinInfo pinInfo = new PinInfo();
              hr = pins[0].QueryPinInfo(out pinInfo);
              DsUtils.FreePinInfo(pinInfo);
              if (hr >= 0)
              {
                Log.Info("  got pin#{0}:{1}", iPinNo - 1, pinInfo.name);
              }
              else
              {
                Log.Info("  got pin:?");
              }
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                //Log.Info("  is output");
                IPin pConnectPin = null;
                hr = pins[0].ConnectedTo(out pConnectPin);
                if (hr == 0 && pConnectPin != null)
                {
                  //Log.Info("  pin is connected ");
                  hr = pins[0].Disconnect();
                  if (hr == 0)
                  {
                    Log.Info("  disconnected ok");
                  }
                  else
                  {
                    Log.Error("  disconnected failed ({0:x})", hr);
                  }
                  ReleaseComObject(pConnectPin);
                  pConnectPin = null;
                }
                //else Log.Info("pin is already connected");
              }
              ReleaseComObject(pins[0]);
            }
            else
            {
              iFetched = 0;
              Log.Info("no pins?");
              break;
            }
          }
          else
          {
            iFetched = 0;
          }
        } while (iFetched == 1);
        ReleaseComObject(pinEnum);
      }
    }

    public static void RemoveUnusedFiltersFromGraph(IGraphBuilder graphBuilder)
    {
      if (graphBuilder == null)
        return;

      int hr = 0;
      IEnumFilters enumFilters = null;
      ArrayList filtersArray = new ArrayList();

      try
      {
        hr = graphBuilder.EnumFilters(out enumFilters);
        DsError.ThrowExceptionForHR(hr);

        IBaseFilter[] filters = new IBaseFilter[1];
        int fetched;

        while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
        {
          filtersArray.Add(filters[0]);
        }

        foreach (IBaseFilter filter in filtersArray)
        {
          FilterInfo info;
          filter.QueryFilterInfo(out info);
          Log.Debug("Check graph connections for: {0}", info.achName);

          IEnumPins pinEnum;
          hr = filter.EnumPins(out pinEnum);
          DsError.ThrowExceptionForHR(hr);

          if (hr == 0 && pinEnum != null)
          {
            bool filterUsed = false;
            bool hasOut = false;
            bool hasIn = false;            
            pinEnum.Reset();
            IPin[] pins = new IPin[1];
            while (pinEnum.Next(1, pins, out fetched) == 0)
            {
              if (fetched > 0)
              {
                PinDirection pinDir;
                hr = pins[0].QueryDirection(out pinDir);
                DsError.ThrowExceptionForHR(hr);
                if (pinDir == PinDirection.Output)
                  hasOut = true;
                else
                  hasIn = true;
                if (HasConnection(pins[0]))
                {
                  filterUsed = true;
                  break;
                }
              }
            }
            ReleaseComObject(pinEnum);
            if (!filterUsed && hasOut && hasIn)
            {
              hr = graphBuilder.RemoveFilter(filter);
              DsError.ThrowExceptionForHR(hr);
              if (hr == 0)
                Log.Debug(" - remove done");              
            }
          }
          ReleaseComObject(info.pGraph);
          ReleaseComObject(filter);
        }        
      }
      catch (Exception error)
      {
        Log.Error("DirectShowUtil: Remove unused filters failed - {0}", error.Message);
      }
      ReleaseComObject(enumFilters);
    }

    public static bool DisconnectAllPins(IGraphBuilder graphBuilder, IBaseFilter filter)
    {
      IEnumPins pinEnum;
      int hr = filter.EnumPins(out pinEnum);
      if (hr != 0 || pinEnum == null)
      {
        return false;
      }
      FilterInfo info;
      filter.QueryFilterInfo(out info);
      Log.Info("Disconnecting all pins from filter {0}", info.achName);
      DirectShowUtil.ReleaseComObject(info.pGraph);
      bool allDisconnected = true;
      for (;;)
      {
        IPin[] pins = new IPin[1];
        int fetched;
        hr = pinEnum.Next(1, pins, out fetched);
        if (hr != 0 || fetched == 0)
        {
          break;
        }
        PinInfo pinInfo;
        pins[0].QueryPinInfo(out pinInfo);
        DsUtils.FreePinInfo(pinInfo);
        if (pinInfo.dir == PinDirection.Output)
        {
          if (!DisconnectPin(graphBuilder, pins[0]))
          {
            allDisconnected = false;
          }
        }
        ReleaseComObject(pins[0]);
      }
      ReleaseComObject(pinEnum);
      return allDisconnected;
    }

    public static bool DisconnectPin(IGraphBuilder graphBuilder, IPin pin)
    {
      IPin other;
      int hr = pin.ConnectedTo(out other);
      bool allDisconnected = true;
      PinInfo info;
      pin.QueryPinInfo(out info);
      DsUtils.FreePinInfo(info);
      Log.Info("Disconnecting pin {0}", info.name);
      if (hr == 0 && other != null)
      {
        other.QueryPinInfo(out info);
        if (!DisconnectAllPins(graphBuilder, info.filter))
        {
          allDisconnected = false;
        }
        hr = pin.Disconnect();
        if (hr != 0)
        {
          allDisconnected = false;
          Log.Error("Error disconnecting: {0:x}", hr);
        }
        hr = other.Disconnect();
        if (hr != 0)
        {
          allDisconnected = false;
          Log.Error("Error disconnecting other: {0:x}", hr);
        }
        DsUtils.FreePinInfo(info);
        ReleaseComObject(other);
      }
      else
      {
        Log.Info("  Not connected");
      }
      return allDisconnected;
    }

    public static bool QueryConnect(IPin pin, IPin other)
    {
      IEnumMediaTypes enumTypes;
      int hr = pin.EnumMediaTypes(out enumTypes);
      if (hr != 0 || enumTypes == null)
      {
        return false;
      }
      int count = 0;
      for (;;)
      {
        AMMediaType[] types = new AMMediaType[1];
        int fetched;
        hr = enumTypes.Next(1, types, out fetched);
        if (hr != 0 || fetched == 0)
        {
          break;
        }
        count++;
        if (other.QueryAccept(types[0]) == 0)
        {
          return true;
        }
      }
      PinInfo info;
      PinInfo infoOther;
      pin.QueryPinInfo(out info);
      DsUtils.FreePinInfo(info);
      other.QueryPinInfo(out infoOther);
      DsUtils.FreePinInfo(infoOther);
      Log.Info("Pins {0} and {1} do not accept each other. Tested {2} media types", info.name, infoOther.name, count);
      return false;
    }

    //fullRebuild: if false, only pins that already had a connection will be rebuilt. dummy for now
    public static bool ReRenderAll(IGraphBuilder graphBuilder, IBaseFilter filter, bool fullRebuild)
    {
      int pinsRendered = 0;
      bool bAllConnected = true;
      IEnumPins pinEnum;
      FilterInfo info;
      filter.QueryFilterInfo(out info);
      ReleaseComObject(info.pGraph);
      
      int hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        Log.Info("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo = 0;
        do
        {
          // Get the next pin
          //Log.Info("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next(1, pins, out iFetched);
          if (hr == 0)
          {
            if (iFetched == 1 && pins[0] != null)
            {
              PinInfo pinInfo = new PinInfo();
              hr = pins[0].QueryPinInfo(out pinInfo);
              DsUtils.FreePinInfo(pinInfo);
              if (hr == 0)
              {
                Log.Info("  got pin#{0}:{1}", iPinNo - 1, pinInfo.name);
              }
              else
              {
                Log.Info("  got pin:?");
              }
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                if (DisconnectPin(graphBuilder, pins[0]))
                {
                  hr = 0;
                  if (TryConnect(graphBuilder, info.achName, pins[0]))
                    //if ((hr = graphBuilder.Render(pins[0])) == 0)
                  {
                    Log.Info("  render ok");
                  }
                  else
                  {
                    Log.Error("  render {0} failed:{1:x}", pinInfo.name, hr);
                    bAllConnected = false;
                  }
                  pinsRendered++;
                }
                //else Log.Info("pin is already connected");
              }
              ReleaseComObject(pins[0]);
            }
            else
            {
              iFetched = 0;
              Log.Info("no pins?");
              break;
            }
          }
          else
          {
            iFetched = 0;
          }
        } while (iFetched == 1);
        ReleaseComObject(pinEnum);
      }
      return bAllConnected;
    }

    public static bool ReConnectAll(IGraphBuilder graphBuilder, IBaseFilter filter)
    {
      bool bAllConnected = true;
      IEnumPins pinEnum;
      FilterInfo info;
      filter.QueryFilterInfo(out info);
      ReleaseComObject(info.pGraph);
      int hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        Log.Info("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo = 0;
        do
        {
          // Get the next pin
          //Log.Info("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next(1, pins, out iFetched);
          if (hr == 0)
          {
            if (iFetched == 1 && pins[0] != null)
            {
              PinInfo pinInfo = new PinInfo();
              hr = pins[0].QueryPinInfo(out pinInfo);
              DsUtils.FreePinInfo(pinInfo);
              if (hr == 0)
              {
                Log.Info("  got pin#{0}:{1}", iPinNo - 1, pinInfo.name);
              }
              else
              {
                Log.Info("  got pin:?");
              }
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                IPin other;
                hr = pins[0].ConnectedTo(out other);
                if (hr == 0 && other != null)
                {
                  Log.Info("Reconnecting {0}:{1}", info.achName, pinInfo.name);
                  hr = graphBuilder.Reconnect(pins[0]);
                  if (hr != 0)
                  {
                    Log.Warn("Reconnect failed: {0}:{1}, code: 0x{2:x}", info.achName, pinInfo.name, hr);
                  }
                }
              }
              ReleaseComObject(pins[0]);
            }
            else
            {
              iFetched = 0;
              Log.Info("no pins?");
              break;
            }
          }
          else
          {
            iFetched = 0;
          }
        } while (iFetched == 1);
        ReleaseComObject(pinEnum);
      }
      return bAllConnected;
    }

    /// <summary>
    /// Find the overlay mixer and/or the VMR9 windowless filters
    /// and tell them we dont want a fixed Aspect Ratio
    /// Mediaportal handles AR itself
    /// </summary>
    /// <param name="graphBuilder"></param>
    public static void SetARMode(IGraphBuilder graphBuilder, AspectRatioMode ARRatioMode)
    {
      int hr;
      IBaseFilter overlay;
      graphBuilder.FindFilterByName("Overlay Mixer2", out overlay);

      if (overlay != null)
      {
        IPin iPin;
        overlay.FindPin("Input0", out iPin);
        if (iPin != null)
        {
          IMixerPinConfig pMC = iPin as IMixerPinConfig;
          if (pMC != null)
          {
            AspectRatioMode mode;
            hr = pMC.SetAspectRatioMode(ARRatioMode);
            hr = pMC.GetAspectRatioMode(out mode);
            //ReleaseComObject(pMC);
          }
          ReleaseComObject(iPin);
        }
        ReleaseComObject(overlay);
      }


      IEnumFilters enumFilters;
      hr = graphBuilder.EnumFilters(out enumFilters);
      if (hr >= 0 && enumFilters != null)
      {
        int iFetched;
        enumFilters.Reset();
        IBaseFilter[] pBasefilter = new IBaseFilter[2];
        do
        {
          pBasefilter = null;
          hr = enumFilters.Next(1, pBasefilter, out iFetched);
          if (hr == 0 && iFetched == 1 && pBasefilter[0] != null)
          {
            IVMRAspectRatioControl pARC = pBasefilter[0] as IVMRAspectRatioControl;
            if (pARC != null)
            {
              pARC.SetAspectRatioMode(VMRAspectRatioMode.None);
            }
            IVMRAspectRatioControl9 pARC9 = pBasefilter[0] as IVMRAspectRatioControl9;
            if (pARC9 != null)
            {
              pARC9.SetAspectRatioMode(VMRAspectRatioMode.None);
            }

            IEnumPins pinEnum;
            hr = pBasefilter[0].EnumPins(out pinEnum);
            if ((hr == 0) && (pinEnum != null))
            {
              pinEnum.Reset();
              IPin[] pins = new IPin[1];
              int f;
              do
              {
                // Get the next pin
                hr = pinEnum.Next(1, pins, out f);
                if (f == 1 && hr == 0 && pins[0] != null)
                {
                  IMixerPinConfig pMC = pins[0] as IMixerPinConfig;
                  if (null != pMC)
                  {
                    pMC.SetAspectRatioMode(ARRatioMode);
                  }
                  ReleaseComObject(pins[0]);
                }
              } while (f == 1);
              ReleaseComObject(pinEnum);
            }
            ReleaseComObject(pBasefilter[0]);
          }
        } while (iFetched == 1 && pBasefilter[0] != null);
        ReleaseComObject(enumFilters);
      }
    }

    private static bool IsInterlaced(uint x)
    {
      return ((x) & ((uint)AMInterlace.IsInterlaced)) != 0;
    }

    private static bool IsSingleField(uint x)
    {
      return ((x) & ((uint)AMInterlace.OneFieldPerSample)) != 0;
    }

    private static bool IsField1First(uint x)
    {
      return ((x) & ((uint)AMInterlace.Field1First)) != 0;
    }

    private static VMR9SampleFormat ConvertInterlaceFlags(uint dwInterlaceFlags)
    {
      if (IsInterlaced(dwInterlaceFlags))
      {
        if (IsSingleField(dwInterlaceFlags))
        {
          if (IsField1First(dwInterlaceFlags))
          {
            return VMR9SampleFormat.FieldSingleEven;
          }
          else
          {
            return VMR9SampleFormat.FieldSingleOdd;
          }
        }
        else
        {
          if (IsField1First(dwInterlaceFlags))
          {
            return VMR9SampleFormat.FieldInterleavedEvenFirst;
          }
          else
          {
            return VMR9SampleFormat.FieldInterleavedOddFirst;
          }
        }
      }
      else
      {
        return VMR9SampleFormat.ProgressiveFrame; // Not interlaced.
      }
    }

    /// <summary>
    /// Find the overlay mixer and/or the VMR9 windowless filters
    /// and tell them we dont want a fixed Aspect Ratio
    /// Mediaportal handles AR itself
    /// </summary>
    /// <param name="graphBuilder"></param>
    public static void EnableDeInterlace(IGraphBuilder graphBuilder)
    {
      //not used anymore
    }

    public static IPin FindVideoPort(ref ICaptureGraphBuilder2 captureGraphBuilder, ref IBaseFilter videoDeviceFilter,
                                     ref Guid mediaType)
    {
      IPin pPin;
      DsGuid cat = new DsGuid(PinCategory.VideoPort);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0,
                                           out pPin);
      if (hr >= 0 && pPin != null)
      {
        Log.Info("Found videoport pin");
      }
      return pPin;
    }

    public static IPin FindPreviewPin(ref ICaptureGraphBuilder2 captureGraphBuilder, ref IBaseFilter videoDeviceFilter,
                                      ref Guid mediaType)
    {
      IPin pPin;
      DsGuid cat = new DsGuid(PinCategory.Preview);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0,
                                           out pPin);
      if (hr >= 0 && pPin != null)
      {
        Log.Info("Found preview pin");
      }
      return pPin;
    }

    public static IPin FindCapturePin(ref ICaptureGraphBuilder2 captureGraphBuilder, ref IBaseFilter videoDeviceFilter,
                                      ref Guid mediaType)
    {
      IPin pPin = null;
      DsGuid cat = new DsGuid(PinCategory.Capture);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0,
                                           out pPin);
      if (hr >= 0 && pPin != null)
      {
        Log.Info("Found capture pin");
      }
      return pPin;
    }

    public static IBaseFilter GetFilterByName(IGraphBuilder graphBuilder, string name)
    {
      int hr = 0;
      IEnumFilters ienumFilt = null;
      IBaseFilter[] foundfilter = new IBaseFilter[2];
      int iFetched = 0;
      try
      {
        hr = graphBuilder.EnumFilters(out ienumFilt);
        if (hr == 0 && ienumFilt != null)
        {
          ienumFilt.Reset();
          do
          {
            hr = ienumFilt.Next(1, foundfilter, out iFetched);
            if (hr == 0 && iFetched == 1)
            {
              FilterInfo filter_infos = new FilterInfo();
              foundfilter[0].QueryFilterInfo(out filter_infos);
              ReleaseComObject(filter_infos.pGraph);
              Log.Debug("GetFilterByName: {0}, {1}", name, filter_infos.achName);
              if (filter_infos.achName.LastIndexOf(name) != -1)
              {
                ReleaseComObject(ienumFilt);
                ienumFilt = null;
                return foundfilter[0];
              }
              ReleaseComObject(foundfilter[0]);
            }
          } while (iFetched == 1 && hr == 0);
          if (ienumFilt != null)
          {
            ReleaseComObject(ienumFilt);
          }
          ienumFilt = null;
        }
      }
      catch (Exception) {}
      finally
      {
        if (ienumFilt != null)
        {
          ReleaseComObject(ienumFilt);
        }
      }
      return null;
    }

    public static void RemoveFilters(IGraphBuilder graphBuilder)
    {
      RemoveFilters(graphBuilder, String.Empty);
    }

    public static void RemoveFilters(IGraphBuilder graphBuilder, string filterName)
    {
      if (graphBuilder == null)
      {
        return;
      }

      int hr = 0;
      IEnumFilters enumFilters = null;
      ArrayList filtersArray = new ArrayList();

      try
      {
        hr = graphBuilder.EnumFilters(out enumFilters);
        DsError.ThrowExceptionForHR(hr);

        IBaseFilter[] filters = new IBaseFilter[1];
        int fetched;

        while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
        {
          filtersArray.Add(filters[0]);
        }

        foreach (IBaseFilter filter in filtersArray)
        {
          FilterInfo info;
          filter.QueryFilterInfo(out info);
          ReleaseComObject(info.pGraph);
       
          try
          {
            if (!String.IsNullOrEmpty(filterName))
            {
              if (String.Equals(info.achName, filterName))
              {
                Log.Debug("Remove filter from graph: {0}", info.achName);
                hr = graphBuilder.RemoveFilter(filter);
                DsError.ThrowExceptionForHR(hr);

              }
            }
            else
            {
              Log.Debug("Remove filter from graph: {0}", info.achName);
              hr = graphBuilder.RemoveFilter(filter);
              DsError.ThrowExceptionForHR(hr);
                        
            }            
          }
          catch (Exception error)
          {
            Log.Error("Remove of filter: {0}, failed with code (HR): {1}, explanation: {2}", info.achName, hr.ToString(),
                      error.Message);
          }

        }
        try
        {
          foreach (IBaseFilter filter in filtersArray)
          {
            while ((hr = ReleaseComObject(filter)) > 0)
              Log.Debug("Decreasing ref count: {0}", hr);
          }
        }
        catch (Exception)
        {
        }
      }
      catch (Exception)
      {
        return;
      }
      finally
      {
        if (enumFilters != null)
        {
          ReleaseComObject(enumFilters);
        }
      }
    }

    public static IntPtr GetUnmanagedSurface(Surface surface)
    {
      return surface.GetObjectByValue(magicConstant);
    }

    public static IntPtr GetUnmanagedDevice(Device device)
    {
      return device.GetObjectByValue(magicConstant);
    }

    public static IntPtr GetUnmanagedTexture(Texture texture)
    {
      return texture.GetObjectByValue(magicConstant);
    }

    public static void FindFilterByClassID(IGraphBuilder m_graphBuilder, Guid classID, out IBaseFilter filterFound)
    {
      filterFound = null;

      if (m_graphBuilder == null)
      {
        return;
      }
      IEnumFilters ienumFilt = null;
      try
      {
        int hr = m_graphBuilder.EnumFilters(out ienumFilt);
        if (hr == 0 && ienumFilt != null)
        {
          int iFetched;
          IBaseFilter[] filter = new IBaseFilter[2];
          ienumFilt.Reset();
          do
          {
            hr = ienumFilt.Next(1, filter, out iFetched);
            if (hr == 0 && iFetched == 1)
            {
              Guid filterGuid;
              filter[0].GetClassID(out filterGuid);
              if (filterGuid == classID)
              {
                filterFound = filter[0];
                return;
              }
              ReleaseComObject(filter[0]);
              filter[0] = null;
            }
          } while (iFetched == 1 && hr == 0);
          if (ienumFilt != null)
          {
            ReleaseComObject(ienumFilt);
          }
          ienumFilt = null;
        }
      }
      catch (Exception) {}
      finally
      {
        if (ienumFilt != null)
        {
          ReleaseComObject(ienumFilt);
        }
      }
      return;
    }

    public static string GetFriendlyName(IMoniker mon)
    {
      if (mon == null)
      {
        return string.Empty;
      }
      object bagObj = null;
      IPropertyBag bag = null;
      try
      {
        IErrorLog errorLog = null;
        Guid bagId = typeof (IPropertyBag).GUID;
        mon.BindToStorage(null, null, ref bagId, out bagObj);
        bag = (IPropertyBag)bagObj;
        object val = "";
        int hr = bag.Read("FriendlyName", out val, errorLog);
        if (hr != 0)
        {
          Marshal.ThrowExceptionForHR(hr);
        }
        string ret = val as string;
        if ((ret == null) || (ret.Length < 1))
        {
          throw new NotImplementedException("Device FriendlyName");
        }
        return ret;
      }
      catch (Exception)
      {
        return null;
      }
      finally
      {
        bag = null;
        if (bagObj != null)
        {
          ReleaseComObject(bagObj);
        }
        bagObj = null;
      }
    }

    public static IPin FindPin(IBaseFilter filter, PinDirection dir, string strPinName)
    {
      int hr = 0;

      IEnumPins pinEnum;
      hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int f;
        do
        {
          // Get the next pin
          hr = pinEnum.Next(1, pins, out f);
          if ((hr == 0) && (pins[0] != null))
          {
            PinDirection pinDir;
            pins[0].QueryDirection(out pinDir);
            if (pinDir == dir)
            {
              PinInfo info;
              pins[0].QueryPinInfo(out info);
              DsUtils.FreePinInfo(info);
              if (String.Compare(info.name, strPinName) == 0)
              {
                ReleaseComObject(pinEnum);
                return pins[0];
              }
            }
            ReleaseComObject(pins[0]);
          }
        } while (hr == 0);
        ReleaseComObject(pinEnum);
      }
      return null;
    }

    public static void RemoveDownStreamFilters(IGraphBuilder graphBuilder, IBaseFilter fromFilter, bool remove)
    {
      IEnumPins enumPins;
      fromFilter.EnumPins(out enumPins);
      if (enumPins == null)
      {
        return;
      }
      IPin[] pins = new IPin[2];
      int fetched;
      while (enumPins.Next(1, pins, out fetched) == 0)
      {
        if (fetched != 1)
        {
          break;
        }
        PinDirection dir;
        pins[0].QueryDirection(out dir);
        if (dir != PinDirection.Output)
        {
          ReleaseComObject(pins[0]);
          continue;
        }
        IPin pinConnected;
        pins[0].ConnectedTo(out pinConnected);
        if (pinConnected == null)
        {
          ReleaseComObject(pins[0]);
          continue;
        }
        PinInfo info;
        pinConnected.QueryPinInfo(out info);

        if (info.filter != null)
        {
          RemoveDownStreamFilters(graphBuilder, info.filter, true);
        }
        DsUtils.FreePinInfo(info);
        ReleaseComObject(pins[0]);
      }
      if (remove)
      {
        graphBuilder.RemoveFilter(fromFilter);
      }
      ReleaseComObject(enumPins);
    }

    public static void RemoveDownStreamFilters(IGraphBuilder graphBuilder, IPin pin)
    {
      IPin pinConnected;
      pin.ConnectedTo(out pinConnected);
      if (pinConnected == null)
      {
        return;
      }
      PinInfo info;
      pinConnected.QueryPinInfo(out info);
      if (info.filter != null)
      {
        RemoveDownStreamFilters(graphBuilder, info.filter, true);
      }
      DsUtils.FreePinInfo(info);
    }

    public static int ReleaseComObject(object obj, int timeOut)
    {
      int returnVal = 1;

      if (obj != null)
      {
        Stopwatch stopwatch = Stopwatch.StartNew();        
        while (returnVal > 0 && stopwatch.ElapsedMilliseconds < timeOut)
        {
          returnVal = Marshal.ReleaseComObject(obj);
          if (returnVal > 0)
          {
            Thread.Sleep(50);
          }
          else
          {
            return returnVal;
          }
        }
      }      

      StackTrace st = new StackTrace(true);
      Log.Error("Exception while releasing COM object (NULL) - stacktrace: {0}", st);

      return 0;
    }

    public static int ReleaseComObject(object obj)
    {
      if (obj != null)
      {
        return Marshal.ReleaseComObject(obj);
      }

      StackTrace st = new StackTrace(true);
      Log.Error("Exception while releasing COM object (NULL) - stacktrace: {0}", st);

      return 0;
    }
  }
}