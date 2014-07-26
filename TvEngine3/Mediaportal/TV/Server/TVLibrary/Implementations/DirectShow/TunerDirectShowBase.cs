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
using System.Collections.Generic;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow
{
  /// <summary>
  /// A base implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> for tuners implemented
  /// using a DirectShow graph.
  /// </summary>
  internal abstract class TunerDirectShowBase : TunerBase
  {
    #region variables

    /// <summary>
    /// The DirectShow filter graph.
    /// </summary>
    protected IFilterGraph2 _graph = null;

    /// <summary>
    /// The running object table entry for the graph.
    /// </summary>
    private DsROTEntry _runningObjectTableEntry = null;

    /// <summary>
    /// The main tuner component device.
    /// </summary>
    protected DsDevice _deviceMain = null;

    /// <summary>
    /// The main tuner component filter.
    /// </summary>
    protected IBaseFilter _filterMain = null;

    /// <summary>
    /// The MediaPortal TS writer/analyser filter.
    /// </summary>
    protected IBaseFilter _filterTsWriter = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    protected IChannelScannerInternal _channelScanner = null;

    /// <summary>
    /// The tuner's EPG grabber interface.
    /// </summary>
    protected IEpgGrabber _epgGrabber = null;

    #endregion

    #region constructors

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDirectShowBase"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="type">The tuner type.</param>
    protected TunerDirectShowBase(DsDevice device, CardType type)
      : base(device.Name, device.DevicePath, type)
    {
      _deviceMain = device;
      SetProductAndTunerInstanceIds(_deviceMain);
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerDirectShowBase"/> class.
    /// </summary>
    /// <param name="name">The tuner's name.</param>
    /// <param name="externalId">The external identifier for the tuner.</param>
    /// <param name="type">The tuner type.</param>
    protected TunerDirectShowBase(string name, string externalId, CardType type)
      : base(name, externalId, type)
    {
    }

    #endregion

    /// <summary>
    /// Set the tuner's product and tuner instance identifier attributes.
    /// </summary>
    /// <param name="device">The device instance to read the attributes from.</param>
    protected void SetProductAndTunerInstanceIds(DsDevice device)
    {
      if (device != null)
      {
        _productInstanceId = device.ProductInstanceIdentifier;
        int tunerInstanceId = device.TunerInstanceIdentifier;
        if (tunerInstanceId >= 0)
        {
          _tunerInstanceId = tunerInstanceId.ToString();
        }
      }
    }

    #region graph building

    /// <summary>
    /// Create and initialise the DirectShow graph.
    /// </summary>
    protected void InitialiseGraph()
    {
      this.LogDebug("DirectShow base: initialise graph");
      _graph = (IFilterGraph2)new FilterGraph();
      _runningObjectTableEntry = new DsROTEntry(_graph);
    }

    /// <summary>
    /// Load the <see cref="T:TvLibrary.Interfaces.ICustomDevice">extensions</see> for this tuner.
    /// </summary>
    /// <remarks>
    /// It is expected that this function will be called at some stage during tuner loading.
    /// This function may update the lastFilter reference parameter to insert filters for
    /// <see cref="IDirectShowAddOnDevice"/> extensions.
    /// </remarks>
    /// <param name="context">Any context required to initialise supported extensions.</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver
    ///   filter) to connect the [first] device filter to.</param>
    protected void LoadExtensions(object context, ref IBaseFilter lastFilter)
    {
      this.LogDebug("DirectShow base: load tuner extensions");
      base.LoadExtensions(context);

      if (lastFilter != null)
      {
        List<IDirectShowAddOnDevice> addOnsToDispose = new List<IDirectShowAddOnDevice>();
        foreach (ICustomDevice extension in _extensions)
        {
          IDirectShowAddOnDevice addOn = extension as IDirectShowAddOnDevice;
          if (addOn != null)
          {
            this.LogDebug("DirectShow base: add-on \"{0}\" found", extension.Name);
            if (!addOn.AddToGraph(_graph, ref lastFilter))
            {
              this.LogDebug("DirectShow base: failed to add filter(s) to graph");
              addOnsToDispose.Add(addOn);
              continue;
            }
          }
        }
        foreach (IDirectShowAddOnDevice addOn in addOnsToDispose)
        {
          addOn.Dispose();
          _extensions.Remove(addOn);
        }
      }
    }

    /// <summary>
    /// Complete the DirectShow graph.
    /// </summary>
    protected void CompleteGraph()
    {
      // For some reason this fails for graphs containing a stream source
      // filter. In any case we should never allow failure to prevent using the
      // tuner.
      try
      {
        FilterGraphTools.SaveGraphFile(_graph, Name + " - " + TunerType + " Graph.grf");
      }
      catch
      {
      }
      _channelScanner = new ChannelScannerDirectShowBase(this, new ChannelScannerHelperDvb(), _filterTsWriter as ITsChannelScan);
      _epgGrabber = new EpgGrabberDirectShow(_filterTsWriter as ITsEpgScanner);
    }

    /// <summary>
    /// Add the filter for the main tuner component to the DirectShow graph.
    /// </summary>
    protected void AddMainComponentFilterToGraph()
    {
      this.LogDebug("DirectShow base: add main component filter");
      if (!DevicesInUse.Instance.Add(_deviceMain))
      {
        throw new TvException("Main DirectShow tuner component is in use.");
      }
      try
      {
        _filterMain = FilterGraphTools.AddFilterFromDevice(_graph, _deviceMain);
      }
      catch (Exception ex)
      {
        DevicesInUse.Instance.Remove(_deviceMain);
        throw new TvException("Failed to add filter for DirectShow tuner main component to graph.", ex);
      }
    }

    /// <summary>
    /// Add and connect the TS writer/analyser filter into the DirectShow graph.
    /// ...[upstream filter]->[TS writer/analyser]
    /// </summary>
    /// <param name="upstreamFilter">The filter to connect to the TS writer/analyser filter.</param>
    protected void AddAndConnectTsWriterIntoGraph(IBaseFilter upstreamFilter)
    {
      this.LogDebug("DirectShow base: add TS writer/analyser filter");
      _filterTsWriter = ComHelper.LoadComObjectFromFile("TsWriter.ax", typeof(MediaPortalTsWriter).GUID, typeof(IBaseFilter).GUID, true) as IBaseFilter;
      FilterGraphTools.AddAndConnectFilterIntoGraph(_graph, _filterTsWriter, "MediaPortal TS Analyser", upstreamFilter);
    }

    /// <summary>
    /// Remove and release DirectShow main component and graph components.
    /// </summary>
    protected void CleanUpGraph()
    {
      this.LogDebug("DirectShow base: clean up graph");
      if (_runningObjectTableEntry != null)
      {
        _runningObjectTableEntry.Dispose();
        _runningObjectTableEntry = null;
      }
      if (_graph != null)
      {
        // First remove the filters that we inserted.
        _graph.RemoveFilter(_filterTsWriter);
        if (_filterMain != null)
        {
          _graph.RemoveFilter(_filterMain);
          Release.ComObject("DirectShow tuner main component filter", ref _filterMain);

          DevicesInUse.Instance.Remove(_deviceMain);
          // Do ***NOT*** dispose or release _deviceMain here. This would cause
          // reloading to fail. Deal with this in Dispose().
        }

        // Now check if there are any filters remaining in the graph.
        // Presence of such filters indicate there are badly behaved
        // classes or extensions present.
        IEnumFilters enumFilters = null;
        int hr = _graph.EnumFilters(out enumFilters);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogWarn("DirectShow base: failed to enumerate remaining filters in graph");
        }
        else
        {
          try
          {
            IBaseFilter[] filters = new IBaseFilter[1];
            int filterCount;
            while (enumFilters.Next(filters.Length, filters, out filterCount) == (int)HResult.Severity.Success && filterCount == 1)
            {
              IBaseFilter filter = filters[0];
              this.LogWarn("DirectShow base: removing and releasing orphaned filter {0}", FilterGraphTools.GetFilterName(filter));
              _graph.RemoveFilter(filter);
              Release.ComObjectAllRefs("DirectShow tuner orphaned filter", ref filter);
            }
          }
          finally
          {
            Release.ComObject("DirectShow tuner remaining filter enumerator", ref enumFilters);
          }
        }
        Release.ComObject("DirectShow tuner graph", ref _graph);
      }
      _epgGrabber = null;
      _channelScanner = null;
      Release.ComObject("TS writer/analyser filter", ref _filterTsWriter);
    }

    #endregion

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();

      this.LogDebug("DirectShow base: reload configuration");
      // TODO apply these settings to TsWriter here
      if (SettingsManagement.GetValue("tsWriterDisableCrcCheck", false))
      {
        this.LogDebug("DirectShow base: disable TsWriter CRC checking");
      }
      if (SettingsManagement.GetValue("tsWriterDumpInputs", false))
      {
        this.LogDebug("DirectShow base: enable TsWriter input dumping");
      }
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    public override void PerformSetTunerState(TunerState state)
    {
      this.LogDebug("DirectShow base: perform set tuner state");

      if (_graph == null)
      {
        throw new TvException("DirectShow graph is null, can't set tuner state.");
      }

      int hr = (int)HResult.Severity.Success;
      if (state == TunerState.Stopped)
      {
        hr = (_graph as IMediaControl).Stop();
      }
      else if (state == TunerState.Paused)
      {
        hr = (_graph as IMediaControl).Pause();
      }
      else if (state == TunerState.Started)
      {
        hr = (_graph as IMediaControl).Run();
      }
      else
      {
        hr = (int)HResult.Severity.Error;
      }
      HResult.ThrowException(hr, string.Format("Failed to change tuner state to {0}.", state));
    }

    #endregion

    #region tuning

    /// <summary>
    /// Allocate a new sub-channel instance.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <returns>the new sub-channel instance</returns>
    public override ITvSubChannel CreateNewSubChannel(int id)
    {
      return new SubChannelMpeg2Ts(id, _filterTsWriter as ITsFilter);
    }

    #endregion

    #region interfaces

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public override IChannelScannerInternal InternalChannelScanningInterface
    {
      get
      {
        return _channelScanner;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public override IEpgGrabber InternalEpgGrabberInterface
    {
      get
      {
        return _epgGrabber;
      }
    }

    #endregion

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      if (_deviceMain != null)
      {
        _deviceMain.Dispose();
        _deviceMain = null;
      }
    }

    #endregion
  }
}