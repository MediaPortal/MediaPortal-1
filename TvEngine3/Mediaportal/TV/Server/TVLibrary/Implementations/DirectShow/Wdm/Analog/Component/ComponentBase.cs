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
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component
{
  /// <summary>
  /// A base class for WDM analog DirectShow graph components. Intended to be used for sharing
  /// semi-generic DirectShow graph functions.
  /// </summary>
  internal abstract class ComponentBase
  {
    /// <summary>
    /// Add and connect a filter into a DirectShow graph.
    /// </summary>
    /// <remarks>
    /// This function assumes that successful graph contruction requires that
    /// <paramref name="upstreamFilter">the supplied upstream filter</paramref> must be connected
    /// to [at least] one specific downstream filter. Therefore, it searches the specified device
    /// category for a filter that will connect to any of the upstream filter's output pins. Once
    /// it finds such a filter, the function makes as many connections as possible before
    /// returning.
    /// </remarks>
    /// <param name="graph">The graph.</param>
    /// <param name="category">The <see cref="DsDevice"/> category to search for a compatible
    ///   filter.</param>
    /// <param name="upstreamFilter">The upstream filter to connect the new filter to. This filter
    ///   should already be present in the graph.</param>
    /// <param name="productInstanceId">A <see cref="DsDevice"/> device path (<see
    ///   cref="System.Runtime.InteropServices.ComTypes.IMoniker"/> display name) section. Related
    ///   filters share common sections. This parameter is used to prioritise filter candidates.
    ///   <c>Null</c> is permitted.</param>
    /// <param name="filter">The filter that was added to the graph. If a filter was not
    ///   successfully added this parameter will be <c>null</c>.</param>
    /// <param name="device">The <see cref="DsDevice"/> instance corresponding with the filter that
    ///   was added to the graph. If a filter was not successfully added this parameter will be
    ///   <c>null</c>.</param>
    /// <returns>the number of pin connections between the upstream and new filter</returns>
    protected int AddAndConnectFilterFromCategory(IFilterGraph2 graph, Guid category, IBaseFilter upstreamFilter, string productInstanceId, out IBaseFilter filter, out DsDevice device)
    {
      filter = null;
      device = null;
      int connectedPinCount = 0;

      // Sort the devices in the category of interest based on whether the
      // device path contains all or part of the hardware identifier.
      DsDevice[] devices = DsDevice.GetDevicesOfCat(category);
      this.LogDebug("WDM analog component: add and connect filter from category {0}, device count = {1}", category, devices.Length);
      if (!string.IsNullOrEmpty(productInstanceId))
      {
        Array.Sort(devices, delegate(DsDevice d1, DsDevice d2)
        {
          bool d1Result = productInstanceId.Equals(d1.ProductInstanceIdentifier);
          bool d2Result = productInstanceId.Equals(d2.ProductInstanceIdentifier);
          if (d1Result && !d2Result)
          {
            return -1;
          }
          if (!d1Result && d2Result)
          {
            return 1;
          }
          return 0;
        });
      }

      // For each device...
      try
      {
        foreach (DsDevice d in devices)
        {
          if (!DevicesInUse.Instance.Add(d))
          {
            continue;
          }

          // Instantiate the corresponding filter.
          this.LogDebug("WDM analog component: attempt to add {0} {1}", d.Name, d.DevicePath);
          IBaseFilter f = null;
          try
          {
            f = FilterGraphTools.AddFilterFromDevice(graph, d);
          }
          catch (Exception ex)
          {
            this.LogDebug(ex, "WDM analog component: failed to add filter from category");
            DevicesInUse.Instance.Remove(d);
            continue;
          }

          try
          {
            connectedPinCount = ConnectFilters(graph, upstreamFilter, f);
            if (connectedPinCount != 0)
            {
              device = d;
              filter = f;
              break;
            }
          }
          finally
          {
            if (connectedPinCount == 0)
            {
              DevicesInUse.Instance.Remove(d);
              graph.RemoveFilter(f);
              Release.ComObject("WDM analog component filter from category", ref f);
            }
          }
        }
      }
      finally
      {
        foreach (DsDevice d in devices)
        {
          if (d != device)
          {
            d.Dispose();
          }
        }
      }

      return connectedPinCount;
    }

    /// <summary>
    /// Make as many direct connections as possible between two DirectShow filters.
    /// </summary>
    /// <param name="graph">The graph containing the two filters.</param>
    /// <param name="filterUpstream">The upstream filter.</param>
    /// <param name="filterDownstream">The downstream filter.</param>
    /// <returns>the number of pin connections between the upstream and downstream filter</returns>
    protected virtual int ConnectFilters(IFilterGraph2 graph, IBaseFilter filterUpstream, IBaseFilter filterDownstream)
    {
      this.LogDebug("WDM analog component: connect filters by pin name");
      int connectedPinCount = 0;
      int hr = (int)HResult.Severity.Success;
      int pinCount = 0;
      int pinIndex = 0;

      // Assemble a list of unconnected upstream output pins.
      List<IPin> pinsUpstream = new List<IPin>();
      try
      {
        IEnumPins pinEnumUpstream = null;
        hr = filterUpstream.EnumPins(out pinEnumUpstream);
        HResult.ThrowException(hr, "Failed to obtain pin enumerator for upstream filter.");
        try
        {
          IPin[] pinsUpstreamTemp = new IPin[2];
          while (pinEnumUpstream.Next(1, pinsUpstreamTemp, out pinCount) == (int)HResult.Severity.Success && pinCount == 1)
          {
            IPin pinUpstream = pinsUpstreamTemp[0];
            try
            {
              // We're not interested in input pins on the upstream filter.
              PinDirection direction;
              hr = pinUpstream.QueryDirection(out direction);
              HResult.ThrowException(hr, "Failed to query pin direction for upstream pin.");
              if (direction == PinDirection.Input)
              {
                this.LogDebug("WDM analog component: upstream pin {0} is an input pin", pinIndex++);
                Release.ComObject("WDM analog component upstream filter input pin", ref pinUpstream);
                continue;
              }

              // We can't use pins that are already connected.
              IPin tempPin = null;
              hr = pinUpstream.ConnectedTo(out tempPin);
              if (hr == (int)HResult.Severity.Success && tempPin != null)
              {
                this.LogDebug("WDM analog component: upstream output pin {0} already connected", pinIndex++);
                Release.ComObject("WDM analog component upstream filter connected pin", ref tempPin);
                continue;
              }

              pinsUpstream.Add(pinUpstream);
              pinIndex++;
            }
            catch
            {
              Release.ComObject("WDM analog component upstream filter exception pin", ref pinUpstream);
              throw;
            }
          }
        }
        finally
        {
          Release.ComObject("WDM analog component upstream filter pin enumerator", ref pinEnumUpstream);
        }

        // Okay, we have our upstream pins. Check if we can connect one or more
        // of the downstream filter's input pins to one or more of the upstream
        // filter's output pins.
        this.LogDebug("WDM analog component: upstream filter output pin count = {0}", pinsUpstream.Count);
        IEnumPins pinEnumDownstream;
        hr = filterDownstream.EnumPins(out pinEnumDownstream);
        HResult.ThrowException(hr, "Failed to obtain pin enumerator for downstream filter.");
        try
        {
          // Attempt to connect each input pin on the downstream filter. We
          // prefer to connect pins with matching names.
          pinIndex = 0;
          IPin[] pinsDownstream = new IPin[2];
          while (pinEnumDownstream.Next(1, pinsDownstream, out pinCount) == (int)HResult.Severity.Success && pinCount == 1)
          {
            int pinUpstreamConnectedIndex = -1;
            IPin pinDownstream = pinsDownstream[0];
            try
            {
              // We're not interested in output pins on the downstream filter.
              PinDirection direction;
              hr = pinDownstream.QueryDirection(out direction);
              HResult.ThrowException(hr, "Failed to query pin direction for downstream pin.");
              if (direction == PinDirection.Output)
              {
                this.LogDebug("WDM analog component: downstream pin {0} is an output pin", pinIndex++);
                continue;
              }

              // We can't use pins that are already connected.
              IPin tempPin = null;
              hr = pinDownstream.ConnectedTo(out tempPin);
              if (hr == (int)HResult.Severity.Success && tempPin != null)
              {
                this.LogDebug("WDM analog component: downstream input pin {0} already connected", pinIndex++);
                Release.ComObject("WDM analog component downstream filter connected pin", ref tempPin);
                continue;
              }

              string pinNameDownstream = FilterGraphTools.GetPinName(pinDownstream);
              this.LogDebug("WDM analog component: try to connect downstream input pin {0} {1}...", pinIndex++, pinNameDownstream);

              // Try to connect the upstream output pin with a downstream input
              // pin that has an identical name.
              IList<IPin> skippedPins = new List<IPin>();
              for (int p = pinsUpstream.Count - 1; p >= 0; p--)
              {
                IPin pinUpstream = pinsUpstream[p];

                // If the pin names don't match then skip the pin for now.
                string pinNameUpstream = FilterGraphTools.GetPinName(pinUpstream);
                this.LogDebug("WDM analog component: upstream output pin {0} name = {1}", pinsUpstream.Count - 1 - p, pinNameUpstream);
                if (!pinNameDownstream.Equals(pinNameUpstream))
                {
                  this.LogDebug("WDM analog component: skipped for now...");
                  skippedPins.Add(pinUpstream);
                  continue;
                }

                try
                {
                  hr = graph.ConnectDirect(pinUpstream, pinDownstream, null);
                  HResult.ThrowException(hr, "Failed to connect pins with matching names.");
                  this.LogDebug("WDM analog component: connected!");
                  pinUpstreamConnectedIndex = p;
                  break;
                }
                catch
                {
                  // Connection failed, move on to next upstream pin.
                }
                finally
                {
                  // The list of skipped pins must still be updated so that the
                  // entry indicies match with the upstream pin list.
                  skippedPins.Add(null);
                }
              }

              // Fallback: try to connect with the pins we skipped previously.
              if (pinUpstreamConnectedIndex < 0)
              {
                this.LogDebug("WDM analog component: fallback to non-matching pins");
                for (int p = skippedPins.Count - 1; p >= 0; p--)
                {
                  IPin pinUpstream = skippedPins[p];
                  if (pinsUpstream == null)
                  {
                    continue;
                  }
                  this.LogDebug("WDM analog component: previously-skipped upstream output pin {0}...", skippedPins.Count - 1 - p);
                  try
                  {
                    hr = graph.ConnectDirect(pinUpstream, pinDownstream, null);
                    HResult.ThrowException(hr, "Failed to connect pins with non-matching names.");
                    this.LogDebug("WDM analog component: connected!");
                    pinUpstreamConnectedIndex = p;
                    break;
                  }
                  catch
                  {
                    // Connection failed, move on to next skipped pin.
                  }
                }
              }

              // If we connected a pin, remove it from our list of upstream
              // output pins so we don't try it again.
              if (pinUpstreamConnectedIndex >= 0)
              {
                connectedPinCount++;
                IPin pinUpstream = pinsUpstream[pinUpstreamConnectedIndex];
                Release.ComObject("WDM analog component upstream filter connected matching output pin", ref pinUpstream);
                pinsUpstream.RemoveAt(pinUpstreamConnectedIndex);
                if (pinsUpstream.Count == 0)
                {
                  break;
                }
              }
            }
            finally
            {
              Release.ComObject("WDM analog component downstream filter pin", ref pinDownstream);
            }
          }
        }
        finally
        {
          Release.ComObject("WDM analog component downstream filter pin enumerator", ref pinEnumDownstream);
        }
      }
      finally
      {
        for (int p = 0; p < pinsUpstream.Count; p++)
        {
          IPin pinUpstream = pinsUpstream[p];
          Release.ComObject("WDM analog component upstream filter unconnected output pin", ref pinUpstream);
        }
      }

      this.LogDebug("WDM analog component: connected {0} pin(s)", connectedPinCount);
      return connectedPinCount;
    }

    /// <summary>
    /// Check if a pin is a video or audio pin.
    /// </summary>
    /// <param name="pin">The pin to check.</param>
    /// <param name="isVideo"><c>True</c> if the pin is a video pin.</param>
    /// <returns><c>true</c> if the pin is a video or audio pin, otherwise <c>false</c></returns>
    protected bool IsVideoOrAudioPin(IPin pin, out bool isVideo)
    {
      isVideo = false;

      // First try media types. They're more reliable.
      IEnumMediaTypes enumMediaTypes;
      int hr = pin.EnumMediaTypes(out enumMediaTypes);
      HResult.ThrowException(hr, "Failed to obtain media type enumerator for pin.");
      try
      {
        // For each pin media type...
        int mediaTypeCount;
        AMMediaType[] mediaTypes = new AMMediaType[2];
        while (enumMediaTypes.Next(1, mediaTypes, out mediaTypeCount) == (int)HResult.Severity.Success && mediaTypeCount == 1)
        {
          AMMediaType mediaType = mediaTypes[0];
          try
          {
            if (mediaType.majorType == MediaType.AnalogVideo || mediaType.majorType == MediaType.Video)
            {
              isVideo = true;
              return true;
            }
            else if (mediaType.majorType == MediaType.AnalogAudio || mediaType.majorType == MediaType.Audio)
            {
              isVideo = false;
              return true;
            }
          }
          finally
          {
            Release.AmMediaType(ref mediaType);
          }
        }
      }
      finally
      {
        Release.ComObject("encoder pin media type enumerator", ref enumMediaTypes);
      }

      // If media types don't tell us, check the pin name.
      string pinName = FilterGraphTools.GetPinName(pin);
      if (pinName != null && pinName.ToLowerInvariant().Contains("video"))
      {
        isVideo = true;
        return true;
      }

      this.LogWarn("WDM analog component: failed to determine pin type");
      return false;
    }
  }
}