#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace DShowNET.Helper
{
  /// <summary>
  ///  A collection of sources (or physical connectors) on an 
  ///  audio or video device. This is used by the <see cref="Capture"/>
  ///  class to provide a list of available sources on the currently
  ///  selected audio and video devices. This class cannot be created
  ///  directly.  This class assumes there is only 1 video and 1 audio
  ///  crossbar and all input pins route to a single output pin on each 
  ///  crossbar.
  /// </summary>
  public class SourceCollection : CollectionBase, IDisposable
  {
    // ------------------ Constructors/Destructors -----------------------

    /// <summary> Initialize collection with no sources. </summary>
    public SourceCollection()
    {
      InnerList.Capacity = 1;
    }

    /// <summary> Initialize collection with sources from graph. </summary>
    public SourceCollection(ICaptureGraphBuilder2 graphBuilder, IBaseFilter deviceFilter, bool isVideoDevice)
    {
      addFromGraph(graphBuilder, deviceFilter, isVideoDevice);
    }

    /// <summary> Destructor. Release unmanaged resources. </summary>
    ~SourceCollection()
    {
      Dispose();
    }


    // -------------------- Public Properties -----------------------

    /// <summary> Get the source at the specified index. </summary>
    public Source this[int index]
    {
      get { return ((Source) InnerList[index]); }
    }

    /// <summary>
    ///  Gets or sets the source/physical connector currently in use.
    ///  This is marked internal so that the Capture class can control
    ///  how and when the source is changed.
    /// </summary>
    public Source CurrentSource
    {
      get
      {
        // Loop through each source and find the first
        // enabled source.
        foreach (Source s in InnerList)
        {
          if (s.Enabled)
          {
            return (s);
          }
        }

        return (null);
      }
      set
      {
        if (value == null)
        {
          // Disable all sources
          foreach (Source s in InnerList)
          {
            s.Enabled = false;
          }
        }
        else if (value is CrossbarSource)
        {
          // Enable this source
          // (this will automatically disable all other sources)
          value.Enabled = true;
        }
        else
        {
          // Disable all sources
          // Enable selected source
          foreach (Source s in InnerList)
          {
            s.Enabled = false;
          }
          value.Enabled = true;
        }
      }
    }


    // -------------------- Public methods -----------------------

    /// <summary> Empty the collection. </summary>
    public new void Clear()
    {
      for (int c = 0; c < InnerList.Count; c++)
      {
        this[c].Dispose();
      }
      InnerList.Clear();
    }

    /// <summary> Release unmanaged resources. </summary>
    public void Dispose()
    {
      Clear();
      InnerList.Capacity = 1;
    }


    // -------------------- Protected Methods -----------------------

    /// <summary> Populate the collection from a filter graph. </summary>
    protected void addFromGraph(ICaptureGraphBuilder2 graphBuilder, IBaseFilter deviceFilter, bool isVideoDevice)
    {
      Trace.Assert(graphBuilder != null);

      ArrayList crossbars = findCrossbars(graphBuilder, deviceFilter);
      foreach (IAMCrossbar crossbar in crossbars)
      {
        ArrayList sources = findCrossbarSources(graphBuilder, crossbar, isVideoDevice);
        InnerList.AddRange(sources);
      }

      if (!isVideoDevice)
      {
        if (InnerList.Count == 0)
        {
          ArrayList sources = findAudioSources(graphBuilder, deviceFilter);
          InnerList.AddRange(sources);
        }
      }
    }

    /// <summary>
    ///	 Retrieve a list of crossbar filters in the graph.
    ///  Most hardware devices should have a maximum of 2 crossbars, 
    ///  one for video and another for audio.
    /// </summary>
    protected ArrayList findCrossbars(ICaptureGraphBuilder2 graphBuilder, IBaseFilter deviceFilter)
    {
      ArrayList crossbars = new ArrayList();

      DsGuid category = new DsGuid(FindDirection.UpstreamOnly);
      //Guid type = new Guid();
      Guid riid = typeof (IAMCrossbar).GUID;
      int hr;

      object comObj = null;
      object comObjNext = null;

      // Find the first interface, look upstream from the selected device
      hr = graphBuilder.FindInterface(category, null, deviceFilter, riid, out comObj);
      while ((hr == 0) && (comObj != null))
      {
        // If found, add to the list
        if (comObj is IAMCrossbar)
        {
          crossbars.Add(comObj as IAMCrossbar);

          // Find the second interface, look upstream from the next found crossbar
          hr = graphBuilder.FindInterface(category, null, comObj as IBaseFilter, riid, out comObjNext);
          comObj = comObjNext;
        }
        else
        {
          comObj = null;
        }
      }

      return (crossbars);
    }

    /// <summary>
    ///  Populate the internal InnerList with sources/physical connectors
    ///  found on the crossbars. Each instance of this class is limited
    ///  to video only or audio only sources ( specified by the isVideoDevice
    ///  parameter on the constructor) so we check each source before adding
    ///  it to the list.
    /// </summary>
    protected ArrayList findCrossbarSources(ICaptureGraphBuilder2 graphBuilder, IAMCrossbar crossbar, bool isVideoDevice)
    {
      ArrayList sources = new ArrayList();
      int hr;
      int numOutPins;
      int numInPins;
      hr = crossbar.get_PinCounts(out numOutPins, out numInPins);
      if (hr < 0)
      {
        Marshal.ThrowExceptionForHR(hr);
      }

      // We loop through every combination of output and input pin
      // to see which combinations match.

      // Loop through output pins
      for (int cOut = 0; cOut < numOutPins; cOut++)
      {
        // Loop through input pins
        for (int cIn = 0; cIn < numInPins; cIn++)
        {
          // Can this combination be routed?
          hr = crossbar.CanRoute(cOut, cIn);
          if (hr == 0)
          {
            // Yes, this can be routed
            int relatedPin;
            PhysicalConnectorType connectorType;
            hr = crossbar.get_CrossbarPinInfo(true, cIn, out relatedPin, out connectorType);
            if (hr < 0)
            {
              Marshal.ThrowExceptionForHR(hr);
            }

            // Is this the correct type?, If so add to the InnerList
            CrossbarSource source = new CrossbarSource(crossbar, cOut, cIn, connectorType);
            if (connectorType < PhysicalConnectorType.Audio_Tuner)
            {
              if (isVideoDevice)
              {
                sources.Add(source);
              }
              else if (!isVideoDevice)
              {
                sources.Add(source);
              }
            }
          }
        }
      }

      // Some silly drivers (*cough* Nvidia *cough*) add crossbars
      // with no real choices. Every input can only be routed to
      // one output. Loop through every Source and see if there
      // at least one other Source with the same output pin.
      int refIndex = 0;
      while (refIndex < sources.Count)
      {
        bool found = false;
        CrossbarSource refSource = (CrossbarSource) sources[refIndex];
        for (int c = 0; c < sources.Count; c++)
        {
          CrossbarSource s = (CrossbarSource) sources[c];
          if ((refSource.OutputPin == s.OutputPin) && (refIndex != c))
          {
            found = true;
            break;
          }
        }
        if (found)
        {
          refIndex++;
        }
        else
        {
          sources.RemoveAt(refIndex);
        }
      }

      return (sources);
    }

    protected ArrayList findAudioSources(ICaptureGraphBuilder2 graphBuilder, IBaseFilter deviceFilter)
    {
      ArrayList sources = new ArrayList();
      IAMAudioInputMixer audioInputMixer = deviceFilter as IAMAudioInputMixer;
      if (audioInputMixer != null)
      {
        // Get a pin enumerator off the filter
        IEnumPins pinEnum;
        int hr = deviceFilter.EnumPins(out pinEnum);
        pinEnum.Reset();
        if ((hr == 0) && (pinEnum != null))
        {
          // Loop through each pin
          IPin[] pins = new IPin[1];
          int f;
          do
          {
            // Get the next pin
            hr = pinEnum.Next(1, pins, out f);
            if ((hr == 0) && (pins[0] != null))
            {
              // Is this an input pin?
              PinDirection dir = PinDirection.Output;
              hr = pins[0].QueryDirection(out dir);
              if ((hr == 0) && (dir == (PinDirection.Input)))
              {
                // Add the input pin to the sources list
                AudioSource source = new AudioSource(pins[0]);
                sources.Add(source);
              }
              pins[0] = null;
            }
          } while (hr == 0);

          DirectShowUtil.ReleaseComObject(pinEnum);
          pinEnum = null;
        }
      }

      // If there is only one source, don't return it
      // because there is nothing for the user to choose.
      // (Hopefully that single source is already enabled).
      if (sources.Count == 1)
      {
        sources.Clear();
      }

      return (sources);
    }
  }
}