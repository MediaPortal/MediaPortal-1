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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Analog.GraphComponents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog.Components
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
      if (_pinWST_VBI != null)
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
      this.LogDebug("analog: SetupTeletext()");
      Guid guidBaseFilter = typeof(IBaseFilter).GUID;
      object obj;
      //find and add tee/sink to sink filter
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSSplitter);
      devices[0].Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
      _teeSink = (IBaseFilter)obj;
      int hr = graphBuilder.AddFilter(_teeSink, devices[0].Name);
      if (hr != 0)
      {
        this.LogError("analog:SinkGraphEx.SetupTeletext(): Unable to add tee/sink filter");
        return false;
      }
      //connect capture filter -> tee sink filter
      IPin pin = DsFindPin.ByDirection(_teeSink, PinDirection.Input, 0);
      hr = graphBuilder.Connect(capture.VBIPin, pin);
      Release.ComObject(pin);
      if (hr != 0)
      {
        //failed...
        this.LogError("analog: unable  to connect capture->tee/sink");
        graphBuilder.RemoveFilter(_teeSink);
        Release.ComObject(_teeSink);
        _teeSink = _filterWstDecoder = null;
        return false;
      }
      if (!string.IsNullOrEmpty(graph.Teletext.Name))
      {
        this.LogDebug("analog: Using Teletext-Component configuration from stored graph");
        devices = DsDevice.GetDevicesOfCat(graph.Teletext.Category);
        foreach (DsDevice device in devices)
        {
          if (device.Name != null && device.Name.Equals(graph.Teletext.Name))
          {
            //found it, add it to the graph
            this.LogInfo("analog:Using teletext component - {0}", graph.Teletext.Name);
            device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
            _filterWstDecoder = (IBaseFilter)obj;
            hr = graphBuilder.AddFilter(_filterWstDecoder, device.Name);
            if (hr != 0)
            {
              //failed...
              this.LogError("analog:SinkGraphEx.SetupTeletext(): Unable to add WST Codec filter");
              graphBuilder.RemoveFilter(_filterWstDecoder);
              _filterWstDecoder = null;
            }
            break;
          }
        }
      }
      if (_filterWstDecoder == null)
      {
        this.LogDebug("analog: No stored or invalid graph for Teletext component - Trying to detect");

        //find the WST codec filter
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSVBICodec);
        foreach (DsDevice device in devices)
        {
          if (device.Name != null && device.Name.IndexOf("WST") >= 0)
          {
            //found it, add it to the graph
            this.LogInfo("analog:Found WST Codec filter");
            device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
            _filterWstDecoder = (IBaseFilter)obj;
            hr = graphBuilder.AddFilter(_filterWstDecoder, device.Name);
            if (hr != 0)
            {
              //failed...
              this.LogError("analog:Unable to add WST Codec filter");
              graphBuilder.RemoveFilter(_teeSink);
              Release.ComObject(_teeSink);
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
          devices = DsDevice.GetDevicesOfCat(MediaPortalGuid.AMKSMULTIVBICodec);
          foreach (DsDevice device in devices)
            if (device.Name != null && device.Name.IndexOf("VBI") >= 0)
            {
              //found it, add it to the graph
              this.LogInfo("analog:Found VBI Codec filter");
              device.Mon.BindToObject(null, null, ref guidBaseFilter, out obj);
              _filterWstDecoder = (IBaseFilter)obj;
              hr = graphBuilder.AddFilter(_filterWstDecoder, device.Name);
              if (hr != 0)
              {
                //failed...
                this.LogError("analog:Unable to add VBI Codec filter");
                graphBuilder.RemoveFilter(_teeSink);
                Release.ComObject(_teeSink);
                _teeSink = _filterWstDecoder = null;
                return false;
              }
              graph.Teletext.Name = device.Name;
              graph.Teletext.Category = MediaPortalGuid.AMKSMULTIVBICodec;
              break;
            }
        }
      }
      if (_filterWstDecoder == null)
      {
        this.LogError("analog: unable to find WST Codec or VBI Codec filter");
        graphBuilder.RemoveFilter(_teeSink);
        Release.ComObject(_teeSink);
        _teeSink = _filterWstDecoder = null;
        return false;
      }
      //connect tee sink filter-> wst codec filter
      IPin pinOut = DsFindPin.ByDirection(_teeSink, PinDirection.Output, 0);
      pin = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Input, 0);
      hr = graphBuilder.Connect(pinOut, pin);
      Release.ComObject(pin);
      Release.ComObject(pinOut);
      if (hr != 0)
      {
        //failed
        this.LogError("analog: unable  to tee/sink->wst codec");
        graphBuilder.RemoveFilter(_filterWstDecoder);
        graphBuilder.RemoveFilter(_teeSink);
        Release.ComObject(_filterWstDecoder);
        Release.ComObject(_teeSink);
        _teeSink = _filterWstDecoder = null;
        _teeSink = null;
        graph.Teletext.Name = null;
        graph.Teletext.Category = new Guid();
        return false;
      }
      //done
      this.LogDebug("analog: teletext setup");

      if (_filterWstDecoder != null)
      {
        this.LogDebug("analog:connect wst/vbi codec->tsfilesink");
        _pinWST_VBI = DsFindPin.ByDirection(_filterWstDecoder, PinDirection.Output, 0);
      }

      return true;
    }

    #endregion
  }
}