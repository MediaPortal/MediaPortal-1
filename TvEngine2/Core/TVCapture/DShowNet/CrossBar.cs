#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using DirectShowLib;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace DShowNET.Helper
{
  public class CrossBar
  {
    static CrossBar()
    {
    }

    /// <summary>
    /// This function resets the crossbar filter(s)
    /// It makes sure that the tuner is routed to the video/audio capture device
    /// and that the video/audio outputs of the crossbars are connected
    /// </summary>
    /// <param name="graphbuilder">IGraphBuilder </param>
    /// <param name="m_captureGraphBuilder">ICaptureGraphBuilder2 </param>
    /// <param name="captureFilter">IBaseFilter containing the capture device filter</param>
    public static void Reset(IGraphBuilder graphbuilder, ICaptureGraphBuilder2 m_captureGraphBuilder,
                             IBaseFilter captureFilter)
    {
      if (graphbuilder == null)
      {
        return;
      }
      if (m_captureGraphBuilder == null)
      {
        return;
      }
      if (captureFilter == null)
      {
        return;
      }
      Route(graphbuilder, m_captureGraphBuilder, captureFilter, true, false, false, false, false, false);
    }

    /// <summary>
    /// FixCrossbarRouting() will search and configure all crossbar filters in the graph
    /// It will
    /// </summary>
    /// <param name="graphbuilder">IGraphBuilder</param>
    /// <param name="m_captureGraphBuilder">ICaptureGraphBuilder2</param>
    /// <param name="captureFilter">IBaseFilter containing the capture device filter</param>
    /// <param name="bTunerIn">configure the crossbars to use the tuner input as source</param>
    /// <param name="useCVBS1">configure the crossbars to use the 1st CVBS input as source</param>
    /// <param name="useCVBS2">configure the crossbars to use the 2nd CVBS input as source</param>
    /// <param name="useSVHS">configure the crossbars to use the SVHS input as source</param>
    /// <param name="logActions">true : log all actions in the logfile
    ///                          false: dont log
    /// </param>
    public static void Route(IGraphBuilder graphbuilder, ICaptureGraphBuilder2 m_captureGraphBuilder,
                             IBaseFilter captureFilter, bool useTuner, bool useCVBS1, bool useCVBS2, bool useSVHS,
                             bool useRgb, bool logActions)
    {
      if (graphbuilder == null)
      {
        return;
      }
      if (m_captureGraphBuilder == null)
      {
        return;
      }
      if (captureFilter == null)
      {
        return;
      }
      bool CvbsWanted = (useCVBS1 || useCVBS2);
      int iCVBSVideo = 0;
      int iCVBSAudio = 0;
      int iSVHSVideo = 0;
      int iRgbVideo = 0;

      if (logActions)
      {
        Log.Info("FixCrossbarRouting: use tuner:{0} use cvbs#1:{1} use cvbs#2:{2} use svhs:{3} use rgb:{4}", useTuner,
                 useCVBS1, useCVBS2, useSVHS, useRgb);
      }
      try
      {
        int icurrentCrossbar = 0;

        //start search upward from the video capture filter
        IBaseFilter searchfilter = captureFilter;
        while (true)
        {
          // Find next crossbar
          int hr = 0;
          DsGuid cat = new DsGuid(FindDirection.UpstreamOnly);
          Guid iid;
          object o = null;
          iid = typeof (IAMCrossbar).GUID;
          if (logActions)
          {
            Log.Info(" Find crossbar:#{0}", 1 + icurrentCrossbar);
          }
          hr = m_captureGraphBuilder.FindInterface(cat, null, searchfilter, iid, out o);
          if (hr == 0 && o != null)
          {
            // we found something, check if it is a crossbar
            IAMCrossbar crossbar = o as IAMCrossbar;

            // next loop, use this filter as start for searching for next crossbar
            searchfilter = o as IBaseFilter;
            if (crossbar != null)
            {
              // new crossbar found
              icurrentCrossbar++;
              if (logActions)
              {
                Log.Info("  crossbar found:{0}", icurrentCrossbar);
              }

              // get the number of input & output pins of the crossbar
              int iOutputPinCount, iInputPinCount;
              crossbar.get_PinCounts(out iOutputPinCount, out iInputPinCount);
              if (logActions)
              {
                Log.Info("    crossbar has {0} inputs and {1} outputs", iInputPinCount, iOutputPinCount);
              }

              int iPinIndexRelated; // pin related (routed) with this output pin
              int iPinIndexRelatedIn; // pin related (routed) with this input pin
              PhysicalConnectorType PhysicalTypeOut; // type of output pin
              PhysicalConnectorType PhysicalTypeIn; // type of input pin
              iCVBSVideo = 0;
              iCVBSAudio = 0;

              //for all output pins of the crossbar
              for (int iOut = 0; iOut < iOutputPinCount; ++iOut)
              {
                // get the information about this output pin
                crossbar.get_CrossbarPinInfo(false, iOut, out iPinIndexRelated, out PhysicalTypeOut);

                // for all input pins of the crossbar
                for (int iIn = 0; iIn < iInputPinCount; iIn++)
                {
                  // check if we can make a connection between the input pin -> output pin
                  hr = crossbar.CanRoute(iOut, iIn);
                  if (hr == 0)
                  {
                    // yes thats possible, now get the information of the input pin
                    crossbar.get_CrossbarPinInfo(true, iIn, out iPinIndexRelatedIn, out PhysicalTypeIn);
                    if (logActions)
                    {
                      Log.Info("     check:in#{0}->out#{1} / {2} -> {3}", iIn, iOut, PhysicalTypeIn.ToString(),
                               PhysicalTypeOut.ToString());
                    }


                    // boolean indicating if current input pin should be connected to the current output pin
                    bool bRoute = false;

                    // Check video input options
                    // if the input pin is a Tuner Input and we want to use the tuner, then connect this
                    if (useTuner && PhysicalTypeIn == PhysicalConnectorType.Video_Tuner)
                    {
                      bRoute = true;
                    }

                    // if the input pin is a CVBS input and we want to use CVBS then
                    if (CvbsWanted && PhysicalTypeIn == PhysicalConnectorType.Video_Composite)
                    {
                      // if this is the first CVBS input then connect
                      iCVBSVideo++;
                      if (iCVBSVideo == 1 && CvbsWanted)
                      {
                        bRoute = true;
                      }

                      // if this is the 2nd CVBS input and we want to use the 2nd CVBS input then connect
                      if (iCVBSVideo == 2 && useCVBS2)
                      {
                        bRoute = true;
                      }
                    }

                    // if the input pin is a SVHS input and we want to use SVHS then connect
                    if (useSVHS && PhysicalTypeIn == PhysicalConnectorType.Video_SVideo)
                    {
                      // make sure we only use the 1st SVHS input of the crossbar
                      // since the PVR150MCE crossbar has 2 SVHS inputs
                      iSVHSVideo++;
                      if (iSVHSVideo == 1)
                      {
                        bRoute = true;
                      }
                    }

                    // if the input pin is a RGB input and we want to use RGB then connect
                    if (useRgb && PhysicalTypeIn == PhysicalConnectorType.Video_RGB)
                    {
                      // make sure we only use the 1st SVHS input of the crossbar
                      // since the PVR150MCE crossbar has 2 SVHS inputs
                      iRgbVideo++;
                      if (iRgbVideo == 1)
                      {
                        bRoute = true;
                      }
                    }

                    // Check audio input options

                    // if this is the audio tuner input and we want to use the tuner, then connect
                    if (useTuner)
                    {
                      if (PhysicalTypeIn == PhysicalConnectorType.Audio_Tuner)
                      {
                        bRoute = true;
                      }
                    }
                    else
                    {
                      // if this is the audio line input
                      if ( /*PhysicalTypeIn==PhysicalConnectorType.Audio_AUX||*/
                        PhysicalTypeIn == PhysicalConnectorType.Audio_Line ||
                        PhysicalTypeIn == PhysicalConnectorType.Audio_AudioDecoder)
                      {
                        // if this is the first audio input then connect
                        iCVBSAudio++;
                        if (CvbsWanted && iCVBSAudio == 1)
                        {
                          bRoute = true;
                        }

                        // if this is the 2nd audio input and we want to use the 2nd CVBS input then connect
                        if (iCVBSAudio == 2 && useCVBS2)
                        {
                          bRoute = true;
                        }

                        // if we want to use SVHS then connect
                        if (useSVHS)
                        {
                          bRoute = true;
                        }

                        // if we want to use RGB then connect
                        if (useRgb)
                        {
                          bRoute = true;
                        }
                      }
                    }

                    //should current input pin be connected to current output pin?
                    if (bRoute)
                    {
                      //yes, then connect
                      if (logActions)
                      {
                        Log.Info("     connect");
                      }
                      hr = crossbar.Route(iOut, iIn);
                      if (logActions)
                      {
                        if (hr != 0)
                        {
                          Log.Info("    connect FAILED");
                        }
                        else
                        {
                          Log.Info("    connect success");
                        }
                      }
                    }
                  } //if (hr==0)
                } //for (int iIn=0; iIn < iInputPinCount; iIn++)
              } //for (int iOut=0; iOut < iOutputPinCount; ++iOut)
            } //if (crossbar!=null)
          } //if (hr ==0 && o != null)
          else
          {
            if (logActions)
            {
              Log.Info("  no more crossbars.:0x{0:X}", hr);
            }
            break;
          }
        } //while (true)
        if (logActions)
        {
          Log.Info("crossbar routing done");
        }
      }
      catch (Exception ex)
      {
        Log.Info("crossbar routing exception:{0}", ex.ToString());
      }
    }

    /// <summary>
    /// FixCrossbarRouting() will search and configure all crossbar filters in the graph
    /// It will
    /// </summary>
    /// <param name="graphbuilder">IGraphBuilder</param>
    /// <param name="m_captureGraphBuilder">ICaptureGraphBuilder2</param>
    /// <param name="captureFilter">IBaseFilter containing the capture device filter</param>
    /// <param name="bTunerIn">configure the crossbars to use the tuner input as source</param>
    /// <param name="useCVBS1">configure the crossbars to use the 1st CVBS input as source</param>
    /// <param name="useCVBS2">configure the crossbars to use the 2nd CVBS input as source</param>
    /// <param name="useSVHS">configure the crossbars to use the SVHS input as source</param>
    /// <param name="logActions">true : log all actions in the logfile
    ///                          false: dont log
    /// </param>
    public static void RouteEx(IGraphBuilder graphbuilder, ICaptureGraphBuilder2 m_captureGraphBuilder,
                               IBaseFilter captureFilter, bool useTuner, bool useCVBS1, bool useCVBS2, bool useSVHS,
                               bool useRgb, string cardName)
    {
      if (graphbuilder == null)
      {
        return;
      }
      if (m_captureGraphBuilder == null)
      {
        return;
      }
      if (captureFilter == null)
      {
        return;
      }
      bool CvbsWanted = (useCVBS1 || useCVBS2);
      int iCVBSVideo = 0;
      int iCVBSAudio = 0;
      int iSVHSVideo = 0;
      int iRGBVideo = 0;

      int audioCVBS1 = 1;
      int audioCVBS2 = 2;
      int audioSVHS = 1;
      int audioRgb = 1;
      int videoCVBS1 = 1;
      int videoCVBS2 = 2;
      int videoSVHS = 1;
      int videoRgb = 1;

      string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", cardName));
      using (Settings xmlreader = new Settings(filename))
      {
        audioCVBS1 = 1 + xmlreader.GetValueAsInt("mapping", "audio1", 0);
        audioCVBS2 = 1 + xmlreader.GetValueAsInt("mapping", "audio2", 1);
        audioSVHS = 1 + xmlreader.GetValueAsInt("mapping", "audio3", 0);
        audioRgb = 1 + xmlreader.GetValueAsInt("mapping", "audio4", 0);


        videoCVBS1 = 1 + xmlreader.GetValueAsInt("mapping", "video1", 0);
        videoCVBS2 = 1 + xmlreader.GetValueAsInt("mapping", "video2", 1);
        videoSVHS = 1 + xmlreader.GetValueAsInt("mapping", "video3", 0);
        videoRgb = 1 + xmlreader.GetValueAsInt("mapping", "video4", 0);
      }

      Log.Info("FixCrossbarRouting: use tuner:{0} use cvbs#1:{1} use cvbs#2:{2} use svhs:{3} use rgb:{4}", useTuner,
               useCVBS1, useCVBS2, useSVHS, useRgb);
      try
      {
        int icurrentCrossbar = 0;

        //start search upward from the video capture filter
        IBaseFilter searchfilter = captureFilter;
        while (true)
        {
          // Find next crossbar
          int hr = 0;
          //Guid cat;
          Guid iid;
          object o = null;
          //cat = FindDirection.UpstreamOnly;
          iid = typeof (IAMCrossbar).GUID;
          Log.Info(" Find crossbar:#{0}", 1 + icurrentCrossbar);
          DsGuid cat = new DsGuid(FindDirection.UpstreamOnly);
          hr = m_captureGraphBuilder.FindInterface(cat, null, searchfilter, iid, out o);
          if (hr == 0 && o != null)
          {
            // we found something, check if it is a crossbar
            IAMCrossbar crossbar = o as IAMCrossbar;

            // next loop, use this filter as start for searching for next crossbar
            searchfilter = o as IBaseFilter;
            if (crossbar != null)
            {
              // new crossbar found
              icurrentCrossbar++;
              Log.Info("  crossbar found:{0}", icurrentCrossbar);

              // get the number of input & output pins of the crossbar
              int iOutputPinCount, iInputPinCount;
              crossbar.get_PinCounts(out iOutputPinCount, out iInputPinCount);
              Log.Info("    crossbar has {0} inputs and {1} outputs", iInputPinCount, iOutputPinCount);

              int iPinIndexRelated; // pin related (routed) with this output pin
              int iPinIndexRelatedIn; // pin related (routed) with this input pin
              PhysicalConnectorType PhysicalTypeOut; // type of output pin
              PhysicalConnectorType PhysicalTypeIn; // type of input pin
              iCVBSVideo = 0;
              iCVBSAudio = 0;

              //for all output pins of the crossbar
              for (int iOut = 0; iOut < iOutputPinCount; ++iOut)
              {
                // get the information about this output pin
                crossbar.get_CrossbarPinInfo(false, iOut, out iPinIndexRelated, out PhysicalTypeOut);

                // for all input pins of the crossbar
                for (int iIn = 0; iIn < iInputPinCount; iIn++)
                {
                  // check if we can make a connection between the input pin -> output pin
                  hr = crossbar.CanRoute(iOut, iIn);
                  if (hr == 0)
                  {
                    // yes thats possible, now get the information of the input pin
                    crossbar.get_CrossbarPinInfo(true, iIn, out iPinIndexRelatedIn, out PhysicalTypeIn);
                    Log.Info("     check:in#{0}->out#{1} / {2} -> {3}", iIn, iOut, PhysicalTypeIn.ToString(),
                             PhysicalTypeOut.ToString());


                    // boolean indicating if current input pin should be connected to the current output pin
                    bool bRoute = false;

                    // Check video input options
                    // if the input pin is a Tuner Input and we want to use the tuner, then connect this
                    if (useTuner && PhysicalTypeIn == PhysicalConnectorType.Video_Tuner)
                    {
                      bRoute = true;
                    }

                    // if the input pin is a CVBS input and we want to use CVBS then
                    if (CvbsWanted && PhysicalTypeIn == PhysicalConnectorType.Video_Composite)
                    {
                      iCVBSVideo++;
                      if (useCVBS1 && iCVBSVideo == 1)
                      {
                        bRoute = true;
                      }
                      if (useCVBS1 && iCVBSVideo == videoCVBS1)
                      {
                        bRoute = true;
                      }
                      if (useCVBS2 && iCVBSVideo == videoCVBS2)
                      {
                        bRoute = true;
                      }
                    }

                    // if the input pin is a SVHS input and we want to use SVHS then connect
                    if (useSVHS && PhysicalTypeIn == PhysicalConnectorType.Video_SVideo)
                    {
                      iSVHSVideo++;
                      if (iSVHSVideo == 1)
                      {
                        bRoute = true;
                      }
                      if (iSVHSVideo == videoSVHS)
                      {
                        bRoute = true;
                      }
                    }

                    // if the input pin is a RGB input and we want to use RGB then connect
                    if (useRgb && PhysicalTypeIn == PhysicalConnectorType.Video_RGB)
                    {
                      iRGBVideo++;
                      if (iRGBVideo == 1)
                      {
                        bRoute = true;
                      }
                      if (iRGBVideo == videoRgb)
                      {
                        bRoute = true;
                      }
                    }

                    // Check audio input options
                    // if this is the audio tuner input and we want to use the tuner, then connect
                    if (useTuner)
                    {
                      if (PhysicalTypeIn == PhysicalConnectorType.Audio_Tuner)
                      {
                        bRoute = true;
                      }
                    }
                    else
                    {
                      // if this is the audio line input
                      if ( /*PhysicalTypeIn==PhysicalConnectorType.Audio_AUX||*/
                        PhysicalTypeIn == PhysicalConnectorType.Audio_Line ||
                        PhysicalTypeIn == PhysicalConnectorType.Audio_AudioDecoder)
                      {
                        // if this is the first audio input then connect
                        iCVBSAudio++;
                        if (useCVBS1 && iCVBSAudio == 1)
                        {
                          bRoute = true;
                        }
                        if (useCVBS2 && iCVBSAudio == 1)
                        {
                          bRoute = true;
                        }
                        if (useSVHS && iCVBSAudio == 1)
                        {
                          bRoute = true;
                        }

                        if (useCVBS1 && iCVBSAudio == audioCVBS1)
                        {
                          bRoute = true;
                        }
                        if (useCVBS2 && iCVBSAudio == audioCVBS2)
                        {
                          bRoute = true;
                        }
                        if (useSVHS && iCVBSAudio == audioSVHS)
                        {
                          bRoute = true;
                        }
                        if (useRgb && iCVBSAudio == audioRgb)
                        {
                          bRoute = true;
                        }
                      }
                    }

                    //should current input pin be connected to current output pin?
                    if (bRoute)
                    {
                      //yes, then connect
                      Log.Info("     connect");
                      hr = crossbar.Route(iOut, iIn);
                      if (hr != 0)
                      {
                        Log.Info("    connect FAILED");
                      }
                      else
                      {
                        Log.Info("    connect success");
                      }
                    }
                  } //if (hr==0)
                } //for (int iIn=0; iIn < iInputPinCount; iIn++)
              } //for (int iOut=0; iOut < iOutputPinCount; ++iOut)
            } //if (crossbar!=null)
          } //if (hr ==0 && o != null)
          else
          {
            Log.Info("  no more crossbars.:0x{0:X}", hr);
            break;
          }
        } //while (true)
        Log.Info("crossbar routing done");
      }
      catch (Exception ex)
      {
        Log.Info("crossbar routing exception:{0}", ex.ToString());
      }
    }
  }
}