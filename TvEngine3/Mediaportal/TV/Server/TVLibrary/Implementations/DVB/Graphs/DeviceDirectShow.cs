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
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  public abstract class DeviceDirectShow : TvCardBase, IDisposable
  {
    #region variables

    /// <summary>
    /// The DirectShow filter graph.
    /// </summary>
    protected IFilterGraph2 _graph = null;

    /// <summary>
    /// The capture graph builder for the graph.
    /// </summary>
    protected ICaptureGraphBuilder2 _captureGraphBuilder = null;

    /// <summary>
    /// The running object table entry for the graph.
    /// </summary>
    private DsROTEntry _runningObjectTableEntry = null;

    /// <summary>
    /// The DsDevice interface for the device.
    /// </summary>
    protected DsDevice _device = null;

    /// <summary>
    /// The main device filter.
    /// </summary>
    protected IBaseFilter _filterDevice = null;

    /// <summary>
    /// The MediaPortal TS writer/analyser filter.
    /// </summary>
    protected IBaseFilter _filterTsWriter = null;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="DeviceDirectShow"/> class.
    /// </summary>
    /// <param name="device">The DsDevice interface for the device.</param>
    protected DeviceDirectShow(DsDevice device)
      : base(device.Name, device.DevicePath)
    {
      _device = device;
    }

    /// <summary>
    /// Create and initialise the DirectShow graph.
    /// </summary>
    protected void InitialiseGraph()
    {
      _graph = (IFilterGraph2)new FilterGraph();
      _captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
      _captureGraphBuilder.SetFiltergraph(_graph);
      _runningObjectTableEntry = new DsROTEntry(_graph);
    }

    /// <summary>
    /// Complete the DirectShow graph.
    /// </summary>
    protected void CompleteGraph()
    {
      FilterGraphTools.SaveGraphFile(_graph, _name + " - " + _tunerType + " Graph.grf");
    }

    /// <summary>
    /// Add the main device filter to the DirectShow graph.
    /// </summary>
    protected void AddMainDeviceFilterToGraph()
    {
      this.LogDebug("DeviceDirectShow: add main device filter");
      if (!DevicesInUse.Instance.Add(_device))
      {
        throw new TvException("Main DirectShow device is in use.");
      }
      try
      {
        int hr = _graph.AddSourceFilterForMoniker(_device.Mon, null, _device.Name, out _filterDevice);
        HResult.ThrowException(hr, "Failed to add main DirectShow device filter to graph.");
      }
      catch (Exception ex)
      {
        Release.ComObject("main DirectShow device filter", ref _filterDevice);
        DevicesInUse.Instance.Remove(_device);
        throw new TvException("Failed to add main DirectShow device filter to graph.", ex);
      }
    }

    /// <summary>
    /// Add and connect the TS writer/analyser filter into the DirectShow graph.
    /// ...[upstream filter]->[TS writer/analyser]
    /// </summary>
    /// <param name="upstreamFilter">The filter to connect to the TS writer.</param>
    protected void AddAndConnectTsWriterIntoGraph(IBaseFilter upstreamFilter)
    {
      this.LogDebug("DeviceDirectShow: add TS writer/analyser filter");
      _filterTsWriter = FilterLoader.LoadFilterFromDll("TsWriter.ax", typeof(MediaPortalTsWriter).GUID, true);
      if (_filterTsWriter == null)
      {
        throw new TvException("Failed to marshal TS writer/analyser filter.");
      }
      FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterTsWriter, "MediaPortal TS Analyser", upstreamFilter, _captureGraphBuilder);
    }

    /// <summary>
    /// Set the state of the device.
    /// </summary>
    /// <param name="state">The state to apply to the device.</param>
    protected override void SetDeviceState(DeviceState state)
    {
      this.LogDebug("DeviceDirectShow: set device state, current state = {0}, requested state = {1}", state);

      if (state == _state)
      {
        this.LogDebug("DeviceDirectShow: device already in required state");
        return;
      }
      if (_graph == null)
      {
        throw new TvException("DirectShow device graph is null, can't set device state.");
      }

      int hr = 0;
      if (state == DeviceState.Stopped)
      {
        hr = (_graph as IMediaControl).Stop();
      }
      else if (state == DeviceState.Paused)
      {
        hr = (_graph as IMediaControl).Pause();
      }
      else
      {
        hr = (_graph as IMediaControl).Run();
      }
      HResult.ThrowException(hr, string.Format("Failed to change device state from {0} to {1}.", _state, state));
      _state = state;
    }

    /// <summary>
    /// Remove and release DirectShow main device and graph components.
    /// </summary>
    protected void CleanUpGraph()
    {
      this.LogDebug("DeviceDirectShow: clean up graph");
      if (_runningObjectTableEntry != null)
      {
        _runningObjectTableEntry.Dispose();
        _runningObjectTableEntry = null;
      }
      if (_graph != null)
      {
        // First remove the filters that we inserted.
        if (_filterTsWriter != null)
        {
          _graph.RemoveFilter(_filterTsWriter);
        }
        if (_filterDevice != null)
        {
          _graph.RemoveFilter(_filterDevice);
        }

        // Now check if there are any filters remaining in the graph.
        // Presence of such filters indicate there are badly behaved
        // classes or plugins present.
        IEnumFilters enumFilters = null;
        int hr = _graph.EnumFilters(out enumFilters);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogWarn("DeviceDirectShow: failed to enumerate remaining filters in graph");
        }
        else
        {
          try
          {
            IBaseFilter[] filters = new IBaseFilter[1];
            int fetched;
            while (enumFilters.Next(filters.Length, filters, out fetched) == 0)
            {
              string filterName = FilterGraphTools.GetFilterName(filters[0]);
              this.LogWarn("DeviceDirectShow: removing and releasing orphaned filter {0}", filterName);
              _graph.RemoveFilter(filters[0]);
              Release.ComObjectAllRefs("DirectShow device orphaned filter", ref filters[0]);
            }
          }
          finally
          {
            Release.ComObject("DirectShow device filter enumerator", ref enumFilters);
          }
        }
        Release.ComObject("DirectShow device graph", ref _graph);
      }
      Release.ComObject("DirectShow device graph builder", ref _captureGraphBuilder);
      Release.ComObject("main DirectShow device filter", ref _filterDevice);
      Release.ComObject("TS writer/analyser filter", ref _filterTsWriter);
      if (_device != null)
      {
        DevicesInUse.Instance.Remove(_device);
      }
    }
  }
}
