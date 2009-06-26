#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal - diehard2
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
using DirectShowLib;
using TvLibrary.Implementations.Analog.GraphComponents;

namespace TvLibrary.Implementations.Analog.Components
{
  /// <summary>
  /// The teletext component of the graph
  /// </summary>
  internal class TeletextComponent : IDisposable
  {
    #region variable
    /// <summary>
    /// The see sink filter
    /// </summary>
    private IBaseFilter _teeSink;
    /// <summary>
    /// The teletext decoder filter
    /// </summary>
    private IBaseFilter _filterWstDecoder;
    /// <summary>
    /// The teletext output pin
    /// </summary>
    private IPin _pinWST_VBI;
    #endregion

    #region properties
    /// <summary>
    /// Gets the teletext output pin
    /// </summary>
    public IPin WST_VBI_Pin
    {
      get { return _pinWST_VBI; }
    }
    #endregion

    #region Dispose
    /// <summary>
    /// Disposes the teletext component
    /// </summary>
    public void Dispose()
    {
      if (_filterWstDecoder != null)
      {
        Release.ComObject("wst codec filter", _filterWstDecoder);
        _filterWstDecoder = null;
      }
      if (_teeSink != null)
      {
        Release.ComObject("teesink filter", _teeSink);
        _teeSink = null;
      }
      if(_pinWST_VBI != null)
      {
        Release.ComObject("wst/vbi codec pinout", _pinWST_VBI);
        _pinWST_VBI = null;
      }
    }
    #endregion

    #region CreateFilterInstance method
    /// <summary>
    /// Creates the teletext component in the graph. First we try to use the stored informations in the graph
    /// </summary>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphbuilder</param>
    /// <param name="capture">The capture component</param>
    /// <returns>true, if the building was successful; false otherwise</returns>
    public bool CreateFilterInstance(Graph graph, IFilterGraph2 graphBuilder, Capture capture)
    {
      Log.Log.WriteFile("analog: SetupTeletext()");
      Guid guidBaseFilter = typeof(IBaseFilter).GUID;
      object obj;
      //find and add tee/sink to sink filter
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSSplitter);
      devices[0].Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
      _teeSink = (IBaseFilter)obj;
      int hr = graphBuilder.AddFilter(_teeSink, devices[0].Name);
      if (hr != 0)
      {
        Log.Log.Error("analog:SinkGraphEx.SetupTeletext(): Unable to add tee/sink filter");
        return false;
      }
      //connect capture filter -> tee sink filter
      IPin pin = DsFindPin.ByDirection(_teeSink, PinDirection.Input, 0);
      hr = graphBuilder.Connect(capture.VBIPin, pin);
      Marshal.ReleaseComObject(pin);
      if (hr != 0)
      {
        //failed...
        Log.Log.Error("analog: unable  to connect capture->tee/sink");
        graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = null;
        return false;
      }
      if (!string.IsNullOrEmpty(graph.Teletext.Name))
      {
        Log.Log.WriteFile("analog: Using Teletext-Component configuration from stored graph");
        devices = DsDevice.GetDevicesOfCat(graph.Teletext.Category);
        foreach (DsDevice device in devices)
        {
          if (device.Name != null && device.Name.Equals(graph.Teletext.Name))
          {
            //found it, add it to the graph
            Log.Log.Info("analog:Using teletext component - {0}",graph.Teletext.Name);
            device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
            _filterWstDecoder = (IBaseFilter)obj;
            hr = graphBuilder.AddFilter(_filterWstDecoder, device.Name);
            if (hr != 0)
            {
              //failed...
              Log.Log.Error("analog:SinkGraphEx.SetupTeletext(): Unable to add WST Codec filter");
              graphBuilder.RemoveFilter(_filterWstDecoder);
              _filterWstDecoder = null;
            }
            break;
          }
        }
      }
      if (_filterWstDecoder == null)
      {
        Log.Log.WriteFile("analog: No stored or invalid graph for Teletext component - Trying to detect");

        //find the WST codec filter
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSVBICodec);
        foreach (DsDevice device in devices)
        {
          if (device.Name != null && device.Name.IndexOf("WST") >= 0)
          {
            //found it, add it to the graph
            Log.Log.Info("analog:Found WST Codec filter");
            device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
            _filterWstDecoder = (IBaseFilter) obj;
            hr = graphBuilder.AddFilter(_filterWstDecoder, device.Name);
            if (hr != 0)
            {
              //failed...
              Log.Log.Error("analog:Unable to add WST Codec filter");
              graphBuilder.RemoveFilter(_teeSink);
              Marshal.ReleaseComObject(_teeSink);
              _teeSink = _filterWstDecoder = null;
              return false;
            }
            graph.Teletext.Name = device.Name;
            graph.Teletext.Category = FilterCategory.AMKSVBICodec;
            break;
          }
        }
        //Look for VBI Codec for Vista users as Vista doesn't use WST Codec anymore
        if (_filterWstDecoder == null)
        {
          devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSMULTIVBICodec);
          foreach (DsDevice device in devices)
            if (device.Name != null && device.Name.IndexOf("VBI") >= 0)
            {
              //found it, add it to the graph
              Log.Log.Info("analog:Found VBI Codec filter");
              device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
              _filterWstDecoder = (IBaseFilter) obj;
              hr = graphBuilder.AddFilter(_filterWstDecoder, device.Name);
              if (hr != 0)
              {
                //failed...
                Log.Log.Error("analog:Unable to add VBI Codec filter");
                graphBuilder.RemoveFilter(_teeSink);
                Marshal.ReleaseComObject(_teeSink);
                _teeSink = _filterWstDecoder = null;
                return false;
              }
              graph.Teletext.Name = device.Name;
              graph.Teletext.Category = FilterCategory.AMKSMULTIVBICodec;
              break;
            }
        }
      }
      if (_filterWstDecoder == null)
      {
        Log.Log.Error("analog: unable to find WST Codec or VBI Codec filter");
        graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = null;
        return false;
      }
      //connect tee sink filter-> wst codec filter
      IPin pinOut = DsFindPin.ByDirection(_teeSink, PinDirection.Output, 0);
      pin = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Input, 0);
      hr = graphBuilder.Connect(pinOut, pin);
      Marshal.ReleaseComObject(pin);
      Marshal.ReleaseComObject(pinOut);
      if (hr != 0)
      {
        //failed
        Log.Log.Error("analog: unable  to tee/sink->wst codec");
        graphBuilder.RemoveFilter(_filterWstDecoder);
        graphBuilder.RemoveFilter(_teeSink);
        Marshal.ReleaseComObject(_filterWstDecoder);
        Marshal.ReleaseComObject(_teeSink);
        _teeSink = _filterWstDecoder = null;
        _teeSink = null;
        graph.Teletext.Name = null;
        graph.Teletext.Category = new Guid();
        return false;
      }
      //done
      Log.Log.WriteFile("analog: teletext setup");

      if (_filterWstDecoder != null)
      {
        Log.Log.WriteFile("analog:connect wst/vbi codec->tsfilesink");
        _pinWST_VBI = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Output, 0);
      }

      return true;
    }
    #endregion

  }
}
