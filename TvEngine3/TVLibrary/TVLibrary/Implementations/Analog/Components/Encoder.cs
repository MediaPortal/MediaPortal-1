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
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Analog.Components
{
  internal class Encoder
  {
    #region constants
    //KSCATEGORY_ENCODER
    private static readonly Guid AMKSEncoder = new Guid("19689BF6-C384-48fd-AD51-90E58C79F70B");
    //STATIC_KSCATEGORY_MULTIPLEXER
    private static readonly Guid AMKSMultiplexer = new Guid("7A5DE1D3-01A1-452c-B481-4FA2B96271E8");
    private static readonly Guid AudioCompressorCategory = new Guid(0x33d9a761, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    private static readonly Guid VideoCompressorCategory = new Guid(0x33d9a760, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    private static readonly Guid LegacyAmFilterCategory = new Guid(0x083863F1, 0x70DE, 0x11d0, 0xBD, 0x40, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);
    private static readonly Guid AMKSMultiplexerSW = new Guid("236C9559-ADCE-4736-BF72-BAB34E392196");
    private static readonly Guid MediaSubtype_Plextor = new Guid(0x30355844, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    #endregion

    #region variables
    /// <summary>
    /// The capture pin = MPEG2 PS pin
    /// </summary>
    private IPin _pinCapture;
    /// <summary>
    /// The video encoder device
    /// </summary>
    private DsDevice _videoEncoderDevice;
    /// <summary>
    /// The audio encoder device
    /// </summary>
    private DsDevice _audioEncoderDevice;
    /// <summary>
    /// The multiplexer device
    /// </summary>
    private DsDevice _multiplexerDevice;
    /// <summary>
    /// The hw video encoder filter
    /// </summary>
    private IBaseFilter _filterVideoEncoder;
    /// <summary>
    /// The hw audio encoder filter
    /// </summary>
    private IBaseFilter _filterAudioEncoder;
    /// <summary>
    /// The hw/Sw multipler filter
    /// </summary>
    private IBaseFilter _filterMultiplexer;
    /// <summary>
    /// The MPEG2-Demux filter
    /// </summary>
    private IBaseFilter _filterMpeg2Demux;
    /// <summary>
    /// The analog mpeg muxer filter
    /// </summary>
    private IBaseFilter _filterAnalogMpegMuxer;
    /// <summary>
    /// The sw audio encoder filter
    /// </summary>
    private IBaseFilter _filterAudioCompressor;
    /// <summary>
    /// The sw video encoder filter
    /// </summary>
    private IBaseFilter _filterVideoCompressor;
    /// <summary>
    /// The video pin on the demux
    /// </summary>
    private IPin _pinVideo;
    /// <summary>
    /// The audio pin on the demux
    /// </summary>
    private IPin _pinAudio;
    /// <summary>
    /// The lpcm pin on the demux
    /// </summary>
    private IPin _pinLPCM;
    /// <summary>
    /// The analog audio pin
    /// </summary>
    private IPin _pinAnalogAudio;
    /// <summary>
    /// The analog video pin
    /// </summary>
    private IPin _pinAnalogVideo;
    /// <summary>
    /// Indicates, if it is a Plextore ConvertX card
    /// </summary>
    private bool _isPlextorConvertX;
    /// <summary>
    /// The mpeg muxer
    /// </summary>
    private IBaseFilter _filterMpegMuxer;
    /// <summary>
    /// Indicates if the video pin is connected to the laster mpeg muxer
    /// </summary>
    private bool _pinVideoConnected;
    #endregion

    #region properties
    /// <summary>
    /// Gets the hw video encoder filter
    /// </summary>
    public IBaseFilter VideoEncoderFilter
    {
      get { return _filterVideoEncoder; }
    }

    /// <summary>
    /// Gets the hw/sw multiplexer filter
    /// </summary>
    public IBaseFilter MultiplexerFilter
    {
      get { return _filterMpegMuxer; }
    }

    /// <summary>
    /// Gets the sw video encoder filter
    /// </summary>
    public IBaseFilter VideoCompressorFilter
    {
      get { return _filterVideoCompressor; }
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
      if (_filterVideoEncoder != null)
      {
        while (Marshal.ReleaseComObject(_filterVideoEncoder) > 0)
        {
        }
        _filterVideoEncoder = null;
      }
      if (_filterAudioEncoder != null)
      {
        while (Marshal.ReleaseComObject(_filterAudioEncoder) > 0)
        {
        }
        _filterAudioEncoder = null;
      }
      if (_filterMpeg2Demux != null)
      {
        Release.ComObject("mpeg2 demux filter", _filterMpeg2Demux);
        _filterMpeg2Demux = null;
      }
      if (_filterAnalogMpegMuxer != null)
      {
        Release.ComObject("MPEG2 analog mux filter", _filterAnalogMpegMuxer);
        _filterAnalogMpegMuxer = null;
      }
      if (_filterMpegMuxer != null)
      {
        Release.ComObject("MPEG2 mux filter", _filterMpegMuxer);
        _filterMpegMuxer = null;
      }
      if (_filterMultiplexer != null)
      {
        Release.ComObject("multiplexer filter", _filterMultiplexer);
        _filterMultiplexer = null;
      }
      if (_filterAudioCompressor != null)
      {
        Release.ComObject("_filterAudioCompressor", _filterAudioCompressor);
        _filterAudioCompressor = null;
      }
      if (_filterVideoCompressor != null)
      {
        Release.ComObject("_filterVideoCompressor", _filterVideoCompressor);
        _filterVideoCompressor = null;
      }
      if (_pinCapture != null)
      {
        Release.ComObject("capturepin filter", _pinCapture);
        _pinCapture = null;
      }
      if (_pinAnalogAudio != null)
      {
        Release.ComObject("_pinAnalogAudio", _pinAnalogAudio);
        _pinAnalogAudio = null;
      }
      if (_pinAnalogVideo != null)
      {
        Release.ComObject("_pinAnalogVideo", _pinAnalogVideo);
        _pinAnalogVideo = null;
      }
      if (_pinVideo != null)
      {
        Release.ComObject("videopin filter", _pinVideo);
        _pinVideo = null;
      }
      if (_pinAudio != null)
      {
        Release.ComObject("audiopin filter", _pinAudio);
        _pinAudio = null;
      }
      if (_pinLPCM != null)
      {
        Release.ComObject("lpcmpin filter", _pinLPCM);
        _pinLPCM = null;
      }
      if (_videoEncoderDevice != null)
      {
        DevicesInUse.Instance.Remove(_videoEncoderDevice);
        _videoEncoderDevice = null;
      }
      if (_audioEncoderDevice != null)
      {
        DevicesInUse.Instance.Remove(_audioEncoderDevice);
        _audioEncoderDevice = null;
      }
      if (_multiplexerDevice != null)
      {
        DevicesInUse.Instance.Remove(_multiplexerDevice);
        _multiplexerDevice = null;
      }

    }
    #endregion

    #region CreateFilterInstance method
    /// <summary>
    /// Creates the encoder component
    /// </summary>
    /// <param name="_graphBuilder">The graph builder</param>
    /// <param name="_tuner">The tuner component</param>
    /// <param name="_tvAudio">The tvaudio component</param>
    /// <param name="_crossbar">The crossbar component</param>
    /// <param name="_capture">The capture component</param>
    /// <returns>true, if the building was successful; false otherwise</returns>
    public bool CreateFilterInstance(IFilterGraph2 _graphBuilder, Tuner _tuner, TvAudio _tvAudio, Crossbar _crossbar, Capture _capture)
    {
      // now things get difficult.
      // Here we can have the following situations:
      // 1. we're done, the video capture filter has a mpeg-2 audio output pin
      // 2. we need to add 1 encoder filter which converts both the audio/video output pins
      //    of the video capture filter to mpeg-2
      // 3. we need to potentially mux the mpeg-2 video with audio. i.e. Nvidia NVTV Dual Tuner capture cards.
      // 4. we need to add 2 mpeg-2 encoder filters for software cards. One for audio and one for video 
      //    after the 2 encoder filters, a multiplexer will be added which takes the output of both
      //    encoders and generates mpeg-2


      //situation 1. we look if the video capture device has an mpeg-2 output pin (media type:stream)
      FindCapturePin(MediaType.Stream, MediaSubType.Null,_capture.VideoFilter);
      //specific workaround for the Plextor COnvertX devices
      if (_tuner.IsPlextorCard())
      {
        Log.Log.Info("analog: Plextor ConvertX TV402U detected");
        _isPlextorConvertX = true;
        //fake the capture pin to the Plextor media type & subtype
        FindCapturePin(MediaType.Video, MediaSubtype_Plextor, _capture.VideoFilter);
        //Find the audio pin
        FindAudioVideoPins(_capture);
        //Add the audio encoder
        AddAudioCompressor(_graphBuilder);
        //Add the Plextor specific InterVideo mux & gets the new capture pin.
        AddInterVideoMuxer(_graphBuilder,_capture);
      }
      if (_pinCapture == null)
      {
        // no it does not. So we have situation 2, 3 or 4 and first need to add 1 or more encoder filters
        // First we try only to add encoders where the encoder pin names are the same as the
        // output pins of the capture filters and we search only for filter which have an mpeg2-program stream output pin
        if (!AddTvEncoderFilter(true, true,_graphBuilder,_tuner,_tvAudio,_crossbar,_capture))
        {
          //if that fails, we try any encoder filter with an mpeg2-program stream output pin
          if (!AddTvEncoderFilter(false, true, _graphBuilder, _tuner, _tvAudio, _crossbar, _capture))
          {
            // If that fails, we try to add encoder where the encoder pin names are the same as the
            // output pins of the capture filters, and now except encoders except with mpeg2-ts output pin
            if (!AddTvEncoderFilter(true, false, _graphBuilder, _tuner, _tvAudio, _crossbar, _capture))
            {
              // If that fails, we try every encoder except encoders except with mpeg2-ts output pin
              AddTvEncoderFilter(false, false, _graphBuilder, _tuner, _tvAudio, _crossbar, _capture);
            }
          }
        }
        // 1 or 2 encoder filters have been added. 
        // check if the encoder filters supply a mpeg-2 output pin
        FindCapturePin(MediaType.Stream, MediaSubType.Null, _filterVideoEncoder);
        // not as a stream, but perhaps its supplied with another media type
        if (_pinCapture == null)
          FindCapturePin(MediaType.Video, MediaSubType.Mpeg2Program, _filterVideoEncoder);
        if (_pinCapture == null)
        {
          //still no mpeg output found, we move on to situation 3. We need to add a multiplexer
          // First we try only to add multiplexers where the multiplexer pin names are the same as the
          // output pins of the encoder filters
          //for the NVTV filter the pin names dont match .. so check first in bool eval and thus skips 
          // trying AddTvMultiPlexer with matching pinnames when using NVTV
          if (_tuner.IsNvidiaCard() || !AddTvMultiPlexer(true, _graphBuilder, _tuner, _tvAudio, _crossbar, _capture))
          {
            //if that fails, we try any multiplexer filter
            AddTvMultiPlexer(false, _graphBuilder, _tuner, _tvAudio, _crossbar, _capture);
          }
        }
      }
      // multiplexer filter now has been added.
      // check if the encoder multiplexer supply a mpeg-2 output pin
      if (_pinCapture == null)
      {
        FindCapturePin(MediaType.Stream, MediaSubType.Null, _filterMultiplexer);
        if (_pinCapture == null)
          FindCapturePin(MediaType.Video, MediaSubType.Mpeg2Program, _filterMultiplexer);
      }
      if (_pinCapture == null)
      {
        // Still no mpeg-2 output pin found
        // looks like this is a s/w encoding card
        if (!FindAudioVideoPins(_capture))
        {
          Log.Log.WriteFile("analog:   failed to find audio/video pins");
          throw new Exception("No analog audio/video pins found");
        }
        if (!AddAudioCompressor(_graphBuilder))
        {
          Log.Log.WriteFile("analog:   failed to add audio compressor. you must install a supported audio encoder!");
          throw new TvExceptionSWEncoderMissing("No audio compressor filter found");
        }
        if (!AddVideoCompressor(_graphBuilder))
        {
          Log.Log.WriteFile("analog:   failed to add video compressor");
          throw new TvExceptionSWEncoderMissing("No video compressor filter found. you must install a supported video encoder!");
        }
        if (FilterGraphTools.GetFilterName(_filterAudioCompressor).Contains("InterVideo Audio Encoder"))
        {
          if (!AddInterVideoMuxer(_graphBuilder,_capture))
          {
            Log.Log.WriteFile("analog:   failed to add intervideo muxer");
            throw new Exception("No intervideo muxer filter found");
          }
        }
        else
        {
          if (!AddAnalogMuxer(_graphBuilder))
          {
            Log.Log.WriteFile("analog:   failed to add analog muxer");
            throw new Exception("No analog muxer filter found");
          }
        }
      }
      //Certain ATI cards have pin names which don't match etc.
      if (_capture.VideoCaptureName.Contains("ATI AVStream Analog Capture") || _capture.AudioCaptureName.Contains("ATI AVStream Analog Capture"))
      {
        Log.Log.WriteFile("analog: ATI AVStream Analog Capture card detected adding mux");
        AddTvMultiPlexer(false, _graphBuilder, _tuner, _tvAudio, _crossbar, _capture);
        FindCapturePin(MediaType.Stream, MediaSubType.Mpeg2Program, _filterMultiplexer);
      }

      //add the mpeg-2 demultiplexer filter
      AddMpeg2Demultiplexer(_graphBuilder);

      //FilterGraphTools.SaveGraphFile(_graphBuilder, "hp.grf");
      if (!AddMpegMuxer(_graphBuilder,_capture))
      {
        throw new TvException("Analog: unable to add mpeg muxer");
      }

      return true;
    }
    #endregion

    #region private helper methods
    /// <summary>
    /// Find a pin on the multiplexer, video encoder or capture filter
    /// which can supplies the mediatype and mediasubtype specified
    /// if found the pin is stored in _pinCapture
    /// When a multiplexer is present then this method will try to find the capture pin on the multiplexer filter
    /// If no multiplexer is present then this method will try to find the capture pin on the video encoder filter
    /// If no video encoder is present then this method will try to find the capture pin on the video capture filter
    /// </summary>
    /// <param name="mediaType">Type of the media.</param>
    /// <param name="mediaSubtype">The media subtype.</param>
    /// <param name="filter">The filter to check</param>
    private void FindCapturePin(Guid mediaType, Guid mediaSubtype, IBaseFilter filter)
    {
      if(filter==null)
      {
        return;
      }
      IEnumPins enumPins;
      filter.EnumPins(out enumPins);
      // loop through all pins
      while (true)
      {
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1)
          break;
        //first check if the pindirection matches
        PinDirection pinDirection;
        pins[0].QueryDirection(out pinDirection);
        if (pinDirection != PinDirection.Output)
          continue;
        //next check if the pin supports the media type requested
        IEnumMediaTypes enumMedia;
        AMMediaType[] media = new AMMediaType[2];
        pins[0].EnumMediaTypes(out enumMedia);
        while (true)
        {
          int fetchedMedia;
          enumMedia.Next(1, media, out fetchedMedia);
          if (fetchedMedia != 1)
            break;
          if (media[0].majorType == mediaType)
          {
            //Log.Log.WriteFile("analog: FindCapturePin major:{0}", media[0].majorType);
            if (media[0].subType == mediaSubtype || media[0].subType == MediaSubType.Mpeg2Program)
            {
              //it does... we're done
              _pinCapture = pins[0];
              //Log.Log.WriteFile("analog: FindCapturePin pin:{0}", FilterGraphTools.LogPinInfo(pins[0]));
              //Log.Log.WriteFile("analog: FindCapturePin   major:{0} sub:{1}", media[0].majorType, media[0].subType);
              Log.Log.WriteFile("analog: FindCapturePin succeeded");
              DsUtils.FreeAMMediaType(media[0]);
              return ;
            }
            //Log.Log.WriteFile("analog: FindCapturePin subtype:{0}", media[0].subType);
          }
          DsUtils.FreeAMMediaType(media[0]);
        }
        Release.ComObject("capture pin", pins[0]);
      }
    }

    #region encoder and multiplexer graph building
    /// <summary>
    /// This method tries to connect a encoder filter to the capture filter
    /// See the remarks in AddTvEncoderFilter() for the possible options
    /// </summary>
    /// <param name="filterEncoder">The filter encoder.</param>
    /// <param name="isVideo">if set to <c>true</c> the filterEncoder is used for video.</param>
    /// <param name="isAudio">if set to <c>true</c> the filterEncoder is used for audio.</param>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the encoder filter should match the pin names of the capture filter.</param>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <param name="_capture">Capture</param>
    /// <returns>
    /// true if encoder is connected correctly, otherwise false
    /// </returns>
    private static bool ConnectEncoderFilter(IBaseFilter filterEncoder, bool isVideo, bool isAudio, bool matchPinNames, IFilterGraph2 _graphBuilder, Capture _capture)
    {
      Log.Log.WriteFile("analog: ConnectEncoderFilter video:{0} audio:{1}", isVideo, isAudio);
      //find the inputs of the encoder. could be 1 or 2 inputs.
      IPin pinInput1 = DsFindPin.ByDirection(filterEncoder, PinDirection.Input, 0);
      IPin pinInput2 = DsFindPin.ByDirection(filterEncoder, PinDirection.Input, 1);
      //log input pins
      if (pinInput1 != null)
        Log.Log.WriteFile("analog:  found pin#0 {0}", FilterGraphTools.LogPinInfo(pinInput1));
      if (pinInput2 != null)
        Log.Log.WriteFile("analog:  found pin#1 {0}", FilterGraphTools.LogPinInfo(pinInput2));
      string pinName1 = FilterGraphTools.GetPinName(pinInput1);
      string pinName2 = FilterGraphTools.GetPinName(pinInput2);
      int pinsConnected = 0;
      int pinsAvailable = 0;
      IPin[] pins = new IPin[20];
      IEnumPins enumPins = null;
      try
      {
        // for each output pin of the capture device
        _capture.VideoFilter.EnumPins(out enumPins);
        enumPins.Next(20, pins, out pinsAvailable);
        Log.Log.WriteFile("analog:  pinsAvailable on capture filter:{0}", pinsAvailable);
        for (int i = 0; i < pinsAvailable; ++i)
        {
          int hr;
          // check if this is an output pin
          PinDirection pinDir;
          pins[i].QueryDirection(out pinDir);
          if (pinDir == PinDirection.Input)
            continue;
          //log the pin info...
          Log.Log.WriteFile("analog:  capture pin:{0} {1}", i, FilterGraphTools.LogPinInfo(pins[i]));
          string pinName = FilterGraphTools.GetPinName(pins[i]);
          // first lets try to connect this output pin of the capture filter to the 1st input pin
          // of the encoder
          // only try to connect when pin name matching is turned off
          // or when the pin names are the same
          if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
          {
            //try to connect the output pin of the capture filter to the first input pin of the encoder
            hr = _graphBuilder.Connect(pins[i], pinInput1);
            if (hr == 0)
            {
              //succeeded!
              Log.Log.WriteFile("analog:  connected pin:{0} {1} with pin0", i, pinName);
              pinsConnected++;
            }
            //check if all pins are connected
            if (pinsConnected == 1 && (isAudio == false || isVideo == false))
            {
              //yes, then we are done
              Log.Log.WriteFile("analog: ConnectEncoderFilter succeeded");
              return true;
            }
          }
          // next lets try to connect this output pin of the capture filter to the 2nd input pin
          // of the encoder
          // only try to connect when pin name matching is turned off
          // or when the pin names are the same
          if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
          {
            //try to connect the output pin of the capture filter to the 2nd input pin of the encoder
            hr = _graphBuilder.Connect(pins[i], pinInput2);
            if (hr == 0)
            {
              //succeeded!
              Log.Log.WriteFile("analog:  connected pin:{0} {1} with pin1", i, pinName);
              pinsConnected++;
            }
            //check if all pins are connected
            if (pinsConnected == 2)
            {
              //yes, then we are done
              Log.Log.WriteFile("analog: ConnectEncoderFilter succeeded");
              return true;
            }
            //Log.Log.WriteFile("analog:  ConnectEncoderFilter to Capture {0} failed", pinName2);
          }
        }
      }
      finally
      {
        if (enumPins != null)
          Release.ComObject("ienumpins", enumPins);
        if (pinInput1 != null)
          Release.ComObject("encoder pin0", pinInput1);
        if (pinInput2 != null)
          Release.ComObject("encoder pin1", pinInput2);
        for (int i = 0; i < pinsAvailable; ++i)
        {
          if (pins[i] != null)
            Release.ComObject("capture pin" + i, pins[i]);
        }
      }
      Log.Log.Write("analog: ConnectEncoderFilter failed (matchPinNames:{0})", matchPinNames);
      return false;
    }

    /// <summary>
    /// This method tries to connect a multiplexer filter to the encoder filters (or capture filter)
    /// See the remarks in AddTvMultiPlexer() for the possible options
    /// </summary>
    /// <param name="filterMultiPlexer">The multiplexer.</param>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the multiplexer filter should match the pin names of the encoder filter.</param>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <param name="_tuner">Tuner</param>
    /// <param name="_capture">Capture</param>
    /// <returns>true if multiplexer is connected correctly, otherwise false</returns>
    private bool ConnectMultiplexer(IBaseFilter filterMultiPlexer, bool matchPinNames, IFilterGraph2 _graphBuilder, Tuner _tuner, Capture _capture)
    {
      //Log.Log.WriteFile("analog: ConnectMultiplexer()");
      // get the input pins of the multiplexer filter (can be 1 or 2 input pins)
      IPin pinInput1 = DsFindPin.ByDirection(filterMultiPlexer, PinDirection.Input, 0);
      IPin pinInput2 = DsFindPin.ByDirection(filterMultiPlexer, PinDirection.Input, 1);
      //log the info for each input pin
      if (pinInput1 != null)
        Log.Log.WriteFile("analog:  found pin#0 {0}", FilterGraphTools.LogPinInfo(pinInput1));
      if (pinInput2 != null)
        Log.Log.WriteFile("analog:  found pin#1 {0}", FilterGraphTools.LogPinInfo(pinInput2));
      string pinName1 = FilterGraphTools.GetPinName(pinInput1);
      string pinName2 = FilterGraphTools.GetPinName(pinInput2);
      try
      {
        if (_filterAudioEncoder != null)
          Log.Log.WriteFile("analog: AudioEncoder available");
        if (_filterVideoEncoder != null)
          Log.Log.WriteFile("analog: VideoEncoder available");
        int pinsConnectedOnMultiplexer = 0;
        // if we have no encoder filters, the multiplexer should be connected directly to the capture filter
        if (_filterAudioEncoder == null || _filterVideoEncoder == null)
        {
          Log.Log.WriteFile("analog: ConnectMultiplexer to capture filter");
          //option 1, connect the multiplexer to the capture filter
          int pinsConnected = 0;
          int pinsAvailable = 0;
          IPin[] pins = new IPin[20];
          IEnumPins enumPins = null;
          try
          {
            // for each output pin of the capture filter
            _capture.VideoFilter.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  capture pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              int hr;
              // check if this is an outpin pin on the capture filter
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input)
                continue;
              //log the pin info
              Log.Log.WriteFile("analog:  capture pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));
              string pinName = FilterGraphTools.GetPinName(pins[i]);
              // try to connect this output pin of the capture filter to the 1st input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                //try to connect the output pin of the capture filter to the 1st input pin of the multiplexer
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  //succeeded
                  Log.Log.WriteFile("analog:  connected pin:{0} {1} to pin1:{2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput1));
                  pinsConnected++;
                }
              }
              // next try to connect this output pin of the capture filter to the 2nd input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
              {
                // check if multiplexer has 2 input pins
                if (pinInput2 != null)
                {
                  //try to connect the output pin of the capture filter to the 2nd input pin of the multiplexer
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    //succeeded
                    Log.Log.WriteFile("analog:  connected pin:{0} {1} to pin2:{2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput2));
                    pinsConnected++;
                  }
                }
              }
              if (_tuner.IsNvidiaCard() && (pinsConnected == 1) && (_filterVideoEncoder != null))
              {
                Log.Log.WriteFile("analog: ConnectMultiplexer step 1 software audio encoder connected and no need for a software video encoder");
                break;
              }
              if (pinsConnected == 2)
              {
                //if both pins are connected, we're done..
                Log.Log.WriteFile("analog: ConnectMultiplexer succeeded at step 1");
                return true;
              }
              else
              {
                Log.Log.WriteFile("analog: ConnectMultiplexer no succes yet at step 1 only connected:" + pinsConnected + " pins");
              }
            }
            pinsConnectedOnMultiplexer += pinsConnected;
          }
          finally
          {
            if (enumPins != null)
              Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null)
                Release.ComObject("capture pin" + i, pins[i]);
            }
          }
        }
        //if we only have a single video encoder
        if (_filterAudioEncoder == null && _filterVideoEncoder != null)
        {
          //option 1, connect the multiplexer to a single encoder filter
          Log.Log.WriteFile("analog: ConnectMultiplexer to video encoder filter");
          int pinsConnected = 0;
          int pinsAvailable = 0;
          IPin[] pins = new IPin[20];
          IEnumPins enumPins = null;
          try
          {
            // for each output pin of the video encoder filter
            _filterVideoEncoder.EnumPins(out enumPins);
            enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  video encoder pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              int hr;
              // check if this is an outpin pin on the video encoder filter
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input)
                continue;
              //log the pin info
              Log.Log.WriteFile("analog:  videoencoder pin:{0} {1}", i, FilterGraphTools.LogPinInfo(pins[i]));
              string pinName = FilterGraphTools.GetPinName(pins[i]);
              // try to connect this output pin of the video encoder filter to the 1st input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                //try to connect the output pin of the video encoder filter to the 1st input pin of the multiplexer filter
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  //succeeded
                  Log.Log.WriteFile("analog:  connected pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput1));
                  pinsConnected++;
                }
              }
              //if the multiplexer has 2 input pins
              if (pinInput2 != null)
              {
                // next try to connect this output pin of the video encoder to the 2nd input pin
                // of the multiplexer
                // only try to connect when pin name matching is turned off
                // or when the pin names are the same
                if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                {
                  //try to connect the output pin of the video encoder filter to the 1st input pin of the multiplexer filter
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    //succeeded
                    Log.Log.WriteFile("analog:  connected pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput2));
                    pinsConnected++;
                  }
                }
              }
              if (pinsConnected == 1)
              {
                // add the already connected pin from the previous step (ConnectMultiplexer to capture filter)
                pinsConnected += pinsConnectedOnMultiplexer;
              }
              if (pinsConnected == 2)
              {
                //succeeded and done...
                Log.Log.WriteFile("analog: ConnectMultiplexer succeeded at step 2");
                return true;
              }
              else
              {
                Log.Log.WriteFile("analog: ConnectMultiplexer no succes yet at step 2 only connected:" + pinsConnected + " pins");
              }
            }
          }
          finally
          {
            if (enumPins != null)
              Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null)
                Release.ComObject("encoder pin" + i, pins[i]);
            }
          }
        }
        //if we have a video encoder and an audio encoder filter
        if (_filterAudioEncoder != null || _filterVideoEncoder != null)
        {
          Log.Log.WriteFile("analog: ConnectMultiplexer to audio/video encoder filters");
          //option 3, connect the multiplexer to the audio/video encoder filters
          int pinsConnected = 0;
          int pinsAvailable = 0;
          IPin[] pins = new IPin[20];
          IEnumPins enumPins = null;
          try
          {
            // for each output pin of the video encoder filter
            if (_filterVideoEncoder != null)
              _filterVideoEncoder.EnumPins(out enumPins);
            if (enumPins != null)
              enumPins.Next(20, pins, out pinsAvailable);
            Log.Log.WriteFile("analog:  videoencoder pins available:{0}", pinsAvailable);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              int hr;
              // check if this is an outpin pin on the video encoder filter
              PinDirection pinDir;
              pins[i].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Input)
                continue;
              //log the pin info
              Log.Log.WriteFile("analog:   videoencoder pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));
              string pinName = FilterGraphTools.GetPinName(pins[i]);
              // try to connect this output pin of the video encoder filter to the 1st input pin
              // of the multiplexer
              // only try to connect when pin name matching is turned off
              // or when the pin names are the same
              if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
              {
                //try to connect the output pin of the video encoder filter to the 1st input pin of the multiplexer filter
                hr = _graphBuilder.Connect(pins[i], pinInput1);
                if (hr == 0)
                {
                  //succeeded
                  Log.Log.WriteFile("analog:  connected pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput1));
                  pinsConnected++;
                }
                else
                {
                  Log.Log.WriteFile("Cant connect 0x{0:x}", hr);
                  Log.Log.WriteFile("pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput1));
                }
              }
              //if multiplexer has 2 inputs..
              if (pinInput2 != null)
              {
                // next try to connect this output pin of the video encoder to the 2nd input pin
                // of the multiplexer
                // only try to connect when pin name matching is turned off
                // or when the pin names are the same
                if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                {
                  //try to connect the output pin of the video encoder filter to the 2nd input pin of the multiplexer filter
                  hr = _graphBuilder.Connect(pins[i], pinInput2);
                  if (hr == 0)
                  {
                    //succeeded
                    Log.Log.WriteFile("analog:  connected pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput2));
                    pinsConnected++;
                  }
                  else
                  {
                    Log.Log.WriteFile("Cant connect 0x{0:x}", hr);
                    Log.Log.WriteFile("pin:{0} {1} to {2}", i, FilterGraphTools.LogPinInfo(pins[i]), FilterGraphTools.LogPinInfo(pinInput2));
                  }
                }
              }
              if (pinsConnected == 1)
              {
                //we are done with the video encoder when there is 1 connection between video encoder filter and multiplexer
                //next, continue with the audio encoder...
                Log.Log.WriteFile("analog: ConnectMultiplexer part 1 succeeded");
                break;
              }
            }
            if (pinsConnected == 0)// video encoder is not connected, so we fail
            {
              Log.Log.WriteFile("analog: Video not connected to multiplexer (pinsConnected == 0) FAILURE");
              return false;
            }
            Log.Log.WriteFile("analog: (pinsConnected: {0})", pinsConnected);

            if (_filterAudioEncoder != null)
            {
              // for each output pin of the audio encoder filter
              _filterAudioEncoder.EnumPins(out enumPins);
              enumPins.Next(20, pins, out pinsAvailable);
              Log.Log.WriteFile("analog:  audioencoder pins available:{0}", pinsAvailable);
              for (int i = 0; i < pinsAvailable; ++i)
              {
                int hr;
                // check if this is an outpin pin on the audio encoder filter
                PinDirection pinDir;
                pins[i].QueryDirection(out pinDir);
                if (pinDir == PinDirection.Input)
                  continue;
                Log.Log.WriteFile("analog: audioencoder  pin:{0} {1} {2}", i, pinDir, FilterGraphTools.LogPinInfo(pins[i]));
                string pinName = FilterGraphTools.GetPinName(pins[i]);
                // try to connect this output pin of the audio encoder filter to the 1st input pin
                // of the multiplexer
                // only try to connect when pin name matching is turned off
                // or when the pin names are the same
                if (matchPinNames == false || (String.Compare(pinName, pinName1, true) == 0))
                {
                  //try to connect the output pin of the audio encoder filter to the 1st input pin of the multiplexer filter
                  hr = _graphBuilder.Connect(pins[i], pinInput1);
                  if (hr == 0)
                  {
                    //succeeded
                    Log.Log.WriteFile("analog:  connected pin:{0}", i);
                    pinsConnected++;
                  }
                }
                //if multiplexer has 2 input pins
                if (pinInput2 != null)
                {
                  // next try to connect this output pin of the audio encoder to the 2nd input pin
                  // of the multiplexer
                  // only try to connect when pin name matching is turned off
                  // or when the pin names are the same
                  if (matchPinNames == false || (String.Compare(pinName, pinName2, true) == 0))
                  {
                    //try to connect the output pin of the audio encoder filter to the 2nd input pin of the multiplexer filter
                    hr = _graphBuilder.Connect(pins[i], pinInput2);
                    if (hr == 0)
                    {
                      //succeeded
                      Log.Log.WriteFile("analog:  connected pin:{0}", i);
                      pinsConnected++;
                    }
                  }
                }
                //when both pins on the multiplexer are connected, we're done
                if (pinsConnected == 2)
                {
                  Log.Log.WriteFile("analog:  part 2 succeeded");
                  return true;
                }
              }
            }
          }
          finally
          {
            if (enumPins != null)
              Release.ComObject("ienumpins", enumPins);
            for (int i = 0; i < pinsAvailable; ++i)
            {
              if (pins[i] != null)
                Release.ComObject("audio encoder pin" + i, pins[i]);
            }
          }
        }
      }
      finally
      {
        if (pinInput1 != null)
          Release.ComObject("multiplexer pin0", pinInput1);
        if (pinInput2 != null)
          Release.ComObject("multiplexer pin1", pinInput2);
      }
      Log.Log.Error("analog: ConnectMultiplexer failed");
      return false;
    }

    /// <summary>
    /// Adds the multiplexer filter to the graph.
    /// several posibilities
    ///  1. no tv multiplexer needed
    ///  2. tv multiplexer filter which is connected to a single encoder filter
    ///  3. tv multiplexer filter which is connected to two encoder filter (audio/video)
    ///  4. tv multiplexer filter which is connected to the capture filter
    /// at the end this method the graph looks like this:
    /// 
    ///  option 2: single encoder filter
    ///    [                ]----->[                ]      [             ]
    ///    [ capture filter ]      [ encoder filter ]----->[ multiplexer ]
    ///    [                ]----->[                ]      [             ]
    ///
    ///
    ///  option 3: dual encoder filters
    ///    [                ]----->[   video        ]    
    ///    [ capture filter ]      [ encoder filter ]------>[             ]
    ///    [                ]      [                ]       [             ]
    ///    [                ]                               [ multiplexer ]
    ///    [                ]----->[   audio        ]------>[             ]
    ///                            [ encoder filter ]      
    ///                            [                ]
    ///
    ///  option 4: no encoder filter
    ///    [                ]----->[             ]
    ///    [ capture filter ]      [ multiplexer ]
    ///    [                ]----->[             ]
    /// </summary>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the multiplexer filter should match the pin names of the encoder filter.</param>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <param name="_tuner">Tuner</param>
    /// <param name="_tvAudio">TvAudio</param>
    /// <param name="_crossbar">Crossbar</param>
    /// <param name="_capture">Capture</param>
    /// <returns>true if encoder filters are added, otherwise false</returns>
    private bool AddTvMultiPlexer(bool matchPinNames, IFilterGraph2 _graphBuilder, Tuner _tuner, TvAudio _tvAudio, Crossbar _crossbar, Capture _capture)
    {
      //Log.Log.WriteFile("analog: AddTvMultiPlexer");
      DsDevice[] devicesHW;
      DsDevice[] devicesSW;
      DsDevice[] devices;
      //get a list of all multiplexers available on this system
      try
      {
        devicesHW = DsDevice.GetDevicesOfCat(AMKSMultiplexer);
        devicesHW = DeviceSorter.Sort(devicesHW, _tuner.TunerName, _tvAudio.TvAudioName, _crossbar.CrossBarName, _capture.VideoCaptureName, _capture.AudioCaptureName, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
        // also add the SoftWare Multiplexers in case no compatible HardWare multiplexer is found (NVTV cards)
        devicesSW = _tuner.IsNvidiaCard() ? DsDevice.GetDevicesOfCat(AMKSMultiplexerSW)
                      : new DsDevice[0];

        devices = new DsDevice[devicesHW.Length + devicesSW.Length];
        int nr = 0;
        for (int i = 0; i < devicesHW.Length; ++i)
          devices[nr++] = devicesHW[i];
        for (int i = 0; i < devicesSW.Length; ++i)
          devices[nr++] = devicesSW[i];
      } catch (Exception ex)
      {
        Log.Log.WriteFile("analog: AddTvMultiPlexer no multiplexer devices found (Exception) " + ex.Message);
        return false;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddTvMultiPlexer no multiplexer devices found");
        return false;
      }
      //for each multiplexer
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        Log.Log.WriteFile("analog: AddTvMultiPlexer try:{0} {1}", devices[i].Name, i);
        // if multiplexer is in use, we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        int hr;
        try
        {
          //add multiplexer to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter to graph");
          continue;
        }
        if (hr != 0)
        {
          //failed to add it to graph, continue with the next multiplexer
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("multiplexer filter", tmp);
          }
          continue;
        }
        // try to connect the multiplexer to encoders/capture devices
        if (ConnectMultiplexer(tmp, matchPinNames,_graphBuilder,_tuner,_capture))
        {
          // succeeded, we're done
          _filterMultiplexer = tmp;
          _multiplexerDevice = devices[i];
          DevicesInUse.Instance.Add(_multiplexerDevice);
          Log.Log.WriteFile("analog: AddTvMultiPlexer succeeded");
          break;
        }
        // unable to connect it, remove the filter and continue with the next one
        _graphBuilder.RemoveFilter(tmp);
        Release.ComObject("multiplexer filter", tmp);
      }
      if (_filterMultiplexer == null)
      {
        Log.Log.WriteFile("analog: no TvMultiPlexer found");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Adds one or 2 encoder filters to the graph
    ///  several posibilities
    ///  1. no encoder filter needed
    ///  2. single encoder filter with seperate audio/video inputs and 1 (mpeg-2) output
    ///  3. single encoder filter with a mpeg2 program stream input (I2S)
    ///  4. two encoder filters. one for audio and one for video
    ///
    ///  At the end of this method the graph looks like:
    ///
    ///  option 2: one encoder filter, with 2 inputs
    ///    [                ]----->[                ]
    ///    [ capture filter ]      [ encoder filter ]
    ///    [                ]----->[                ]
    ///
    ///
    ///  option 3: one encoder filter, with 1 input
    ///    [                ]      [                ]
    ///    [ capture filter ]----->[ encoder filter ]
    ///    [                ]      [                ]
    ///
    ///
    ///  option 4: 2 encoder filters one for audio and one for video
    ///    [                ]----->[   video        ]
    ///    [ capture filter ]      [ encoder filter ]
    ///    [                ]      [                ]
    ///    [                ]   
    ///    [                ]----->[   audio        ]
    ///                            [ encoder filter ]
    ///                            [                ]
    ///
    /// </summary>
    /// <param name="matchPinNames">if set to <c>true</c> the pin names of the encoder filter should match the pin names of the capture filter.</param>
    /// <param name="mpeg2ProgramFilter">if set to <c>true</c> than only encoders with an mpeg2 program output pins are accepted</param>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <param name="_tuner">Tuner</param>
    /// <param name="_tvAudio">TvAudio</param>
    /// <param name="_crossbar">Crossbar</param>
    /// <param name="_capture">Capture</param>
    /// <returns>true if encoder filters are added, otherwise false</returns>
    private bool AddTvEncoderFilter(bool matchPinNames, bool mpeg2ProgramFilter, IFilterGraph2 _graphBuilder, Tuner _tuner, TvAudio _tvAudio, Crossbar _crossbar, Capture _capture)
    {
      Log.Log.WriteFile("analog: AddTvEncoderFilter - MatchPinNames: {0} - MPEG2ProgramFilter: {1}", matchPinNames, mpeg2ProgramFilter);
      bool finished = false;
      DsDevice[] devices;
      // first get all encoder filters available on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(AMKSEncoder);
        devices = DeviceSorter.Sort(devices, _tuner.TunerName, _tvAudio.TvAudioName, _crossbar.CrossBarName, _capture.VideoCaptureName, _capture.AudioCaptureName, _videoEncoderDevice, _audioEncoderDevice, _multiplexerDevice);
      } catch (Exception)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder devices found (Exception)");
        return false;
      }
      if (devices == null)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder devices found (devices == null)");
        return false;
      }
      if (devices.Length == 0)
      {
        Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder devices found");
        return false;
      }
      //for each encoder
      Log.Log.WriteFile("analog: AddTvEncoderFilter found:{0} encoders", devices.Length);
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        //if encoder is in use, we can skip it
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          Log.Log.WriteFile("analog:  skip :{0} (inuse)", devices[i].Name);
          continue;
        }
        Log.Log.WriteFile("analog:  try encoder:{0} {1}", devices[i].Name, i);
        int hr;
        try
        {
          //add encoder filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter {0} to graph", devices[i].Name);
          continue;
        }
        if (hr != 0)
        {
          //failed to add filter to graph, continue with the next one
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("TvEncoderFilter", tmp);
          }
          continue;
        }
        if (tmp == null)
          continue;
        // Encoder has been added to the graph
        // Now some cards have 2 encoder types, one for mpeg-2 transport stream and one for
        // mpeg-2 program stream. We dont want the mpeg-2 transport stream !
        // So first we check the output pins...
        // and dont accept filters which have a mpeg-ts output pin..
        // get the output pin
        bool isTsFilter = mpeg2ProgramFilter;
        IPin pinOut = DsFindPin.ByDirection(tmp, PinDirection.Output, 0);
        if (pinOut != null)
        {
          //check which media types it support
          IEnumMediaTypes enumMediaTypes;
          pinOut.EnumMediaTypes(out enumMediaTypes);
          if (enumMediaTypes != null)
          {
            int fetched;
            AMMediaType[] mediaTypes = new AMMediaType[20];
            enumMediaTypes.Next(20, mediaTypes, out fetched);
            if (fetched > 0)
            {
              for (int media = 0; media < fetched; ++media)
              {

                //check if media is mpeg-2 transport
                if (mediaTypes[media].majorType == MediaType.Stream &&
                    mediaTypes[media].subType == MediaSubType.Mpeg2Transport)
                {
                  isTsFilter = true;
                }
                //check if media is mpeg-2 program
                if (mediaTypes[media].majorType == MediaType.Stream &&
                    mediaTypes[media].subType == MediaSubType.Mpeg2Program)
                {
                  isTsFilter = false;
                  break;
                }

                // NVTV dual tuner needs this one to make it work so dont skip it
                if (mediaTypes[media].majorType == MediaType.Video &&
                    mediaTypes[media].subType == new Guid("be626472-fe7c-4a21-9f0b-d8f18b5ab441")) /*MediaSubType.?? */
                {
                  isTsFilter = false;
                  break;
                }
              }
            }
          }
          Release.ComObject("pinout", pinOut);
        }
        //if encoder has mpeg-2 ts output pin, then we skip it and continue with the next one
        if (isTsFilter)
        {
          Log.Log.WriteFile("analog:  filter {0} does not have mpeg-2 ps output or is a mpeg-2 ts filters", devices[i].Name);
          _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("TvEncoderFilter", tmp);
          continue;
        }
        // get the input pins of the encoder (can be 1 or 2 inputs)
        IPin pin1 = DsFindPin.ByDirection(tmp, PinDirection.Input, 0);
        IPin pin2 = DsFindPin.ByDirection(tmp, PinDirection.Input, 1);
        if (pin1 != null)
          Log.Log.WriteFile("analog: encoder in-pin1:{0}", FilterGraphTools.LogPinInfo(pin1));
        if (pin2 != null)
          Log.Log.WriteFile("analog: encoder in-pin2:{0}", FilterGraphTools.LogPinInfo(pin2));
        // if the encoder has 2 input pins then this means it has seperate inputs for audio and video
        if (pin1 != null && pin2 != null)
        {
          // try to connect the capture device -> encoder filters..
          if (ConnectEncoderFilter(tmp, true, true, matchPinNames,_graphBuilder,_capture))
          {
            //succeeded, encoder has been added and we are done
            _filterVideoEncoder = tmp;
            _videoEncoderDevice = devices[i];
            DevicesInUse.Instance.Add(_videoEncoderDevice);
            Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded (encoder with 2 inputs)");
            //            success = true;
            finished = true;
            tmp = null;
          }
        }
        else if (pin1 != null)
        {
          //encoder filter only has 1 input pin.
          //First we get the media type of this pin to determine if its audio of video
          IEnumMediaTypes enumMedia;
          AMMediaType[] media = new AMMediaType[20];
          int fetched;
          pin1.EnumMediaTypes(out enumMedia);
          enumMedia.Next(1, media, out fetched);
          if (fetched == 1)
          {
            //media type found
            Log.Log.WriteFile("analog: AddTvEncoderFilter encoder output major:{0} sub:{1}", media[0].majorType, media[0].subType);
            //is it audio?
            if (media[0].majorType == MediaType.Audio)
            {
              //yes, pin is audio
              //then connect the encoder to the audio output pin of the capture filter
              if (ConnectEncoderFilter(tmp, false, true, matchPinNames,_graphBuilder,_capture))
              {
                //this worked. but we're not done yet. We probably need to add a video encoder also
                _filterAudioEncoder = tmp;
                _audioEncoderDevice = devices[i];
                DevicesInUse.Instance.Add(_audioEncoderDevice);
                //                success = true;
                Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded (audio encoder)");
                // if video encoder was already added, then we're done.
                if (_filterVideoEncoder != null)
                  finished = true;
                tmp = null;
              }
            }
            else
            {
              //pin is video
              //then connect the encoder to the video output pin of the capture filter
              if (ConnectEncoderFilter(tmp, true, false, matchPinNames,_graphBuilder,_capture))
              {
                //this worked. but we're not done yet. We probably need to add a audio encoder also
                _filterVideoEncoder = tmp;
                _videoEncoderDevice = devices[i];
                DevicesInUse.Instance.Add(_videoEncoderDevice);
                //                success = true;
                Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded (video encoder)");
                // if audio encoder was already added, then we're done.
                if (_filterAudioEncoder != null)
                  finished = true;
                tmp = null;
              }
            }
            DsUtils.FreeAMMediaType(media[0]);
          }
          else
          {
            // filter does not report any media type (which is strange)
            // we must do something, so we treat it as a video input pin
            Log.Log.WriteFile("analog: AddTvEncoderFilter no media types for pin1"); //??
            if (ConnectEncoderFilter(tmp, true, false, matchPinNames,_graphBuilder,_capture))
            {
              _filterVideoEncoder = tmp;
              _videoEncoderDevice = devices[i];
              DevicesInUse.Instance.Add(_videoEncoderDevice);
              //              success = true;
              Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded");
              finished = true;
              tmp = null;
            }
          }
        }
        else
        {
          Log.Log.WriteFile("analog: AddTvEncoderFilter no pin1");
        }
        if (pin1 != null)
          Release.ComObject("encoder pin0", pin1);
        if (pin2 != null)
          Release.ComObject("encoder pin1", pin2);
        if (tmp != null)
        {
          _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("encoder filter", tmp);
        }
        if (finished)
        {
          Log.Log.WriteFile("analog: AddTvEncoderFilter succeeded 3");
          return true;
        }
      }//for (int i = 0; i < devices.Length; i++)
      Log.Log.WriteFile("analog: AddTvEncoderFilter no encoder found");
      return false;
    }
    #endregion

    #region s/w encoding card specific graph building
    /// <summary>
    /// Find a pin on the filter specified
    /// which can supplies the mediatype and mediasubtype specified
    /// if found the pin is returned
    /// </summary>
    /// <param name="filter">The filter to find the pin on.</param>
    /// <param name="mediaType">Type of the media.</param>
    /// <param name="mediaSubtype">The media subtype.</param>
    private static IPin FindMediaPin(IBaseFilter filter, Guid mediaType, Guid mediaSubtype)
    {
      IEnumPins enumPins;
      filter.EnumPins(out enumPins);
      // loop through all pins
      int pinNr = -1;
      while (true)
      {
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1)
          break;
        //first check if the pindirection matches
        PinDirection pinDirection;
        pins[0].QueryDirection(out pinDirection);
        if (pinDirection != PinDirection.Output)
          continue;
        pinNr++;
        //next check if the pin supports the media type requested
        IEnumMediaTypes enumMedia;
        AMMediaType[] media = new AMMediaType[2];
        pins[0].EnumMediaTypes(out enumMedia);
        while (true)
        {
          int fetchedMedia;
          enumMedia.Next(1, media, out fetchedMedia);
          if (fetchedMedia != 1)
            break;
          if (media[0].majorType == mediaType)
          {
            if (media[0].subType == mediaSubtype || mediaSubtype == MediaSubType.Null)
            {
              //it does... we're done
              Log.Log.WriteFile("analog: FindMediaPin pin:#{0} {1}", pinNr, FilterGraphTools.LogPinInfo(pins[0]));
              Log.Log.WriteFile("analog: FindMediaPin   major:{0} sub:{1}", media[0].majorType, media[0].subType);
              Log.Log.WriteFile("analog: FindMediaPin succeeded");
              DsUtils.FreeAMMediaType(media[0]);
              return pins[0];
            }
          }
          DsUtils.FreeAMMediaType(media[0]);
        }
        Release.ComObject("capture pin", pins[0]);
      }
      return null;
    }

    /// <summary>
    /// Finds the analog audio/video output pins
    /// </summary>
    /// <param name="_capture">Capture</param>
    /// <returns></returns>
    private bool FindAudioVideoPins(Capture _capture)
    {
      Log.Log.WriteFile("analog: FindAudioVideoPins");
      if (_filterMultiplexer != null)
      {
        Log.Log.WriteFile("analog:   find pins on multiplexer");
        if (_pinAnalogAudio == null)
          _pinAnalogAudio = FindMediaPin(_filterMultiplexer, MediaType.Audio, MediaSubType.Null);
        if (_pinAnalogVideo == null)
          _pinAnalogVideo = FindMediaPin(_filterMultiplexer, MediaType.Video, MediaSubType.Null);
      }
      if (_filterVideoEncoder != null)
      {
        Log.Log.WriteFile("analog:   find pins on video encoder");
        if (_pinAnalogAudio == null)
          _pinAnalogAudio = FindMediaPin(_filterVideoEncoder, MediaType.Audio, MediaSubType.Null);
        if (_pinAnalogVideo == null)
          _pinAnalogVideo = FindMediaPin(_filterVideoEncoder, MediaType.Video, MediaSubType.Null);
      }
      if (_filterAudioEncoder != null)
      {
        Log.Log.WriteFile("analog:   find pins on audio encoder");
        if (_pinAnalogAudio == null)
          _pinAnalogAudio = FindMediaPin(_filterAudioEncoder, MediaType.Audio, MediaSubType.Null);
        if (_pinAnalogVideo == null)
          _pinAnalogVideo = FindMediaPin(_filterAudioEncoder, MediaType.Video, MediaSubType.Null);
      }
      if (_capture.VideoFilter != null)
      {
        Log.Log.WriteFile("analog:   find pins on capture filter");
        if (_pinAnalogAudio == null)
          _pinAnalogAudio = FindMediaPin(_capture.VideoFilter, MediaType.Audio, MediaSubType.Null);
        if (_pinAnalogVideo == null)
          _pinAnalogVideo = FindMediaPin(_capture.VideoFilter, MediaType.Video, MediaSubType.Null);
      }
      if (_pinAnalogVideo == null || _pinAnalogAudio == null)
        return false;
      return true;
    }

    /// <summary>
    /// Adds the audio compressor.
    /// </summary>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <returns></returns>
    private bool AddAudioCompressor(IFilterGraph2 _graphBuilder)
    {
      Log.Log.WriteFile("analog: AddAudioCompressor {0}", FilterGraphTools.LogPinInfo(_pinAnalogAudio));
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(AudioCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(LegacyAmFilterCategory);
      string[] audioEncoders = new string[] { "InterVideo Audio Encoder", "Ulead MPEG Audio Encoder", "MainConcept MPEG Audio Encoder", "MainConcept Demo MPEG Audio Encoder", "CyberLink Audio Encoder", "CyberLink Audio Encoder(Twinhan)", "Pinnacle MPEG Layer-2 Audio Encoder", "MainConcept (Hauppauge) MPEG Audio Encoder", "NVIDIA Audio Encoder" };
      DsDevice[] audioDevices = new DsDevice[audioEncoders.Length];
      for (int x = 0; x < audioEncoders.Length; ++x)
      {
        audioDevices[x] = null;
      }
      for (int i = 0; i < devices1.Length; i++)
      {
        for (int x = 0; x < audioEncoders.Length; ++x)
        {
          if (audioEncoders[x] == devices1[i].Name)
          {
            audioDevices[x] = devices1[i];
            break;
          }
        }
      }
      for (int i = 0; i < devices2.Length; i++)
      {
        for (int x = 0; x < audioEncoders.Length; ++x)
        {
          if (audioEncoders[x] == devices2[i].Name)
          {
            audioDevices[x] = devices2[i];
            break;
          }
        }
      }
      //for each compressor
      Log.Log.WriteFile("analog: AddAudioCompressor found:{0} compressor", audioDevices.Length);
      for (int i = 0; i < audioDevices.Length; ++i)
      {
        IBaseFilter tmp;
        if (audioDevices[i] == null)
          continue;
        Log.Log.WriteFile("analog:  try compressor:{0}", audioDevices[i].Name);
        int hr;
        try
        {
          //add compressor filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(audioDevices[i].Mon, null, audioDevices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter {0} to graph", audioDevices[i].Name);
          continue;
        }
        if (hr != 0)
        {
          //failed to add filter to graph, continue with the next one
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("audiocompressor", tmp);
          }
          continue;
        }
        if (tmp == null)
          continue;

        Log.Log.WriteFile("analog: connect audio pin->audio compressor");
        // check if this compressor filter has an mpeg audio output pin
        IPin pinAudio = DsFindPin.ByDirection(tmp, PinDirection.Input, 0);
        if (pinAudio == null)
        {
          Log.Log.WriteFile("analog: cannot find audio pin on compressor");
          _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("audiocompressor", tmp);
          continue;
        }
        // we found a nice compressor, lets try to connect the analog audio pin to the compressor
        hr = _graphBuilder.Connect(_pinAnalogAudio, pinAudio);
        if (hr != 0)
        {
          Log.Log.WriteFile("analog: failed to connect audio pin->audio compressor:{0:X}", hr);
          //unable to connec the pin, remove it and continue with next compressor
          _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("audiocompressor", tmp);
          continue;
        }
        Log.Log.WriteFile("analog: connected audio pin->audio compressor");
        //succeeded.
        _filterAudioCompressor = tmp;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Adds the video compressor.
    /// </summary>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <returns></returns>
    private bool AddVideoCompressor(IFilterGraph2 _graphBuilder)
    {
      Log.Log.WriteFile("analog: AddVideoCompressor");
      DsDevice[] devices1 = DsDevice.GetDevicesOfCat(VideoCompressorCategory);
      DsDevice[] devices2 = DsDevice.GetDevicesOfCat(LegacyAmFilterCategory);
      string[] videoEncoders = new string[] { "InterVideo Video Encoder", "Ulead MPEG Encoder", "MainConcept MPEG Video Encoder", "MainConcept Demo MPEG Video Encoder", "CyberLink MPEG Video Encoder", "CyberLink MPEG Video Encoder(Twinhan)", "MainConcept (Hauppauge) MPEG Video Encoder", "nanocosmos MPEG Video Encoder", "Pinnacle MPEG 2 Encoder" };
      DsDevice[] videoDevices = new DsDevice[videoEncoders.Length];
      for (int x = 0; x < videoEncoders.Length; ++x)
      {
        videoDevices[x] = null;
      }
      for (int i = 0; i < devices1.Length; i++)
      {
        for (int x = 0; x < videoEncoders.Length; ++x)
        {
          if (videoEncoders[x] == devices1[i].Name)
          {
            videoDevices[x] = devices1[i];
            break;
          }
        }
      }
      for (int i = 0; i < devices2.Length; i++)
      {
        for (int x = 0; x < videoEncoders.Length; ++x)
        {
          if (videoEncoders[x] == devices2[i].Name)
          {
            videoDevices[x] = devices2[i];
            break;
          }
        }
      }
      //for each compressor
      Log.Log.WriteFile("analog: AddVideoCompressor found:{0} compressor", videoDevices.Length);
      for (int i = 0; i < videoDevices.Length; i++)
      {
        IBaseFilter tmp;
        if (videoDevices[i] == null)
          continue;
        Log.Log.WriteFile("analog:  try compressor:{0}", videoDevices[i].Name);
        int hr;
        try
        {
          //add compressor filter to graph
          hr = _graphBuilder.AddSourceFilterForMoniker(videoDevices[i].Mon, null, videoDevices[i].Name, out tmp);
        } catch (Exception)
        {
          Log.Log.WriteFile("analog: cannot add filter {0} to graph", videoDevices[i].Name);
          continue;
        }
        if (hr != 0)
        {
          //failed to add filter to graph, continue with the next one
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("videocompressor", tmp);
          }
          continue;
        }
        if (tmp == null)
          continue;
        // check if this compressor filter has an mpeg audio output pin
        Log.Log.WriteFile("analog:  connect video pin->video compressor");
        IPin pinVideo = DsFindPin.ByDirection(tmp, PinDirection.Input, 0);
        // we found a nice compressor, lets try to connect the analog video pin to the compressor
        hr = _graphBuilder.Connect(_pinAnalogVideo, pinVideo);
        if (hr != 0)
        {
          Log.Log.WriteFile("analog: failed to connect video pin->video compressor");
          //unable to connec the pin, remove it and continue with next compressor
          _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("videocompressor", tmp);
          continue;
        }
        //succeeded.
        _filterVideoCompressor = tmp;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Adds the mpeg muxer
    /// </summary>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <returns></returns>
    private bool AddAnalogMuxer(IFilterGraph2 _graphBuilder)
    {
      Log.Log.Info("analog:AddAnalogMuxer");
      const string monikerPowerDirectorMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7F2BBEAF-E11C-4D39-90E8-938FB5A86045}";
      _filterAnalogMpegMuxer = Marshal.BindToMoniker(monikerPowerDirectorMuxer) as IBaseFilter;
      int hr = _graphBuilder.AddFilter(_filterAnalogMpegMuxer, "Analog MPEG Muxer");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:AddAnalogMuxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add AddAnalogMuxer");
      }
      // next connect audio compressor->muxer
      IPin pinOut = DsFindPin.ByDirection(_filterAudioCompressor, PinDirection.Output, 0);
      IPin pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 1);
      if (pinOut == null)
      {
        Log.Log.Info("analog:no output pin found on audio compressor");
        throw new TvException("no output pin found on audio compressor");
      }
      if (pinIn == null)
      {
        Log.Log.Info("analog:no input pin found on analog muxer");
        throw new TvException("no input pin found on muxer");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:unable to connect audio compressor->muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add unable to connect audio compressor->muxer");
      }
      Log.Log.WriteFile("analog:  connected audio -> muxer");
      // next connect video compressor->muxer
      pinOut = DsFindPin.ByDirection(_filterVideoCompressor, PinDirection.Output, 0);
      pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 0);
      if (pinOut == null)
      {
        Log.Log.Info("analog:no output pin found on video compressor");
        throw new TvException("no output pin found on video compressor");
      }
      if (pinIn == null)
      {
        Log.Log.Info("analog:no input pin found on analog muxer");
        throw new TvException("no input pin found on muxer");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:unable to connect video compressor->muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add unable to connect video compressor->muxer");
      }
      //and finally we have a capture pin...
      Log.Log.WriteFile("analog:  connected video -> muxer");
      _pinCapture = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Output, 0);
      if (_pinCapture == null)
      {
        Log.Log.WriteFile("analog:unable find capture pin");
        throw new TvException("unable find capture pin");
      }
      return true;
    }

    /// <summary>
    /// Adds the InterVideo muxer and connects the compressor to it.
    /// This is the preferred muxer for Plextor cards and others.
    /// It will be used if the InterVideo Audio Encoder is used also.
    /// </summary>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <param name="_capture">Capture</param>
    /// <returns></returns>
    private bool AddInterVideoMuxer(IFilterGraph2 _graphBuilder, Capture _capture)
    {
      IPin pinOut;
      Log.Log.Info("analog:  using intervideo muxer");
      string muxVideoIn = "video compressor";
      const string monikerInterVideoMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{317DDB63-870E-11D3-9C32-00104B3801F7}";
      _filterAnalogMpegMuxer = Marshal.BindToMoniker(monikerInterVideoMuxer) as IBaseFilter;
      int hr = _graphBuilder.AddFilter(_filterAnalogMpegMuxer, "InterVideo MPEG Muxer");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:  add intervideo muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add InterVideo Muxer");
      }
      Log.Log.Info("analog:  add intervideo muxer successful");
      // next connect video compressor->muxer
      if (_isPlextorConvertX)
      {
        muxVideoIn = "Plextor ConvertX";
        //no video compressor needed with the Plextor device so we use the first capture pin
        pinOut = DsFindPin.ByDirection(_capture.VideoFilter, PinDirection.Output, 0);
      }
      else
      {
        pinOut = DsFindPin.ByDirection(_filterVideoCompressor, PinDirection.Output, 0);
      }
      IPin pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 0);
      if (pinOut == null)
      {
        Log.Log.Info("analog:  no output pin found on {0}", muxVideoIn);
        throw new TvException("no output pin found on video out");
      }
      if (pinIn == null)
      {
        Log.Log.Info("analog:  no input pin found on intervideo muxer");
        throw new TvException("no input pin found on intervideo muxer");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:  unable to connect {0}-> intervideo muxer returns:0x{1:X}", muxVideoIn, hr);
        throw new TvException("Unable to add unable to connect to video in on intervideo muxer");
      }
      Log.Log.WriteFile("analog:  connected video -> intervideo muxer");
      // next connect audio compressor->muxer
      pinOut = DsFindPin.ByDirection(_filterAudioCompressor, PinDirection.Output, 0);
      pinIn = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Input, 1);
      if (pinOut == null)
      {
        Log.Log.Info("analog:  no output pin found on audio compressor");
        throw new TvException("no output pin found on audio compressor");
      }
      if (pinIn == null)
      {
        Log.Log.Info("analog:  no input pin found on intervideo muxer");
        throw new TvException("no input pin found on intervideo muxer");
      }
      hr = _graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog:unable to connect audio compressor->intervideo muxer returns:0x{0:X}", hr);
        throw new TvException("Unable to add unable to connect audio compressor->intervideo muxer");
      }
      Log.Log.WriteFile("analog:  connected audio -> intervideo muxer");
      //and finally we have a capture pin...
      _pinCapture = DsFindPin.ByDirection(_filterAnalogMpegMuxer, PinDirection.Output, 0);
      if (_pinCapture == null)
      {
        Log.Log.WriteFile("analog:unable find capture pin");
        throw new TvException("unable find capture pin");
      }
      return true;
    }
    #endregion

    /// <summary>
    /// Adds a mpeg2 demultiplexer to the graph
    /// </summary>
    /// <param name="_graphBuilder">The graph builder</param>
    private void AddMpeg2Demultiplexer(IFilterGraph2 _graphBuilder)
    {
      Log.Log.WriteFile("analog: AddMpeg2Demultiplexer");
      if (_filterMpeg2Demux != null)
        return;
      if (_pinCapture == null)
        return;
      _filterMpeg2Demux = (IBaseFilter)new MPEG2Demultiplexer();
      int hr = _graphBuilder.AddFilter(_filterMpeg2Demux, "MPEG2 Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("analog: AddMPEG2DemuxFilter returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer");
      }
      Log.Log.WriteFile("analog: connect capture->mpeg2 demux");
      IPin pin = DsFindPin.ByDirection(_filterMpeg2Demux, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(_pinCapture, pin);
      if (hr != 0)
      {
        Log.Log.WriteFile("analog: ConnectFilters returns:0x{0:X}", hr);
        throw new TvException("Unable to connect capture-> MPEG2 demultiplexer");
      }
      IMpeg2Demultiplexer demuxer = (IMpeg2Demultiplexer)_filterMpeg2Demux;
      demuxer.CreateOutputPin(FilterGraphTools.GetVideoMpg2Media(), "Video", out _pinVideo);
      demuxer.CreateOutputPin(FilterGraphTools.GetAudioMpg2Media(), "Audio", out _pinAudio);
      demuxer.CreateOutputPin(FilterGraphTools.GetAudioLPCMMedia(), "LPCM", out _pinLPCM);
      IMPEG2StreamIdMap map = (IMPEG2StreamIdMap)_pinVideo;
      map.MapStreamId(224, MPEG2Program.ElementaryStream, 0, 0);
      map = (IMPEG2StreamIdMap)_pinAudio;
      map.MapStreamId(0xC0, MPEG2Program.ElementaryStream, 0, 0);
      map = (IMPEG2StreamIdMap)_pinLPCM;
      map.MapStreamId(0xBD, MPEG2Program.ElementaryStream, 0xA0, 7);
    }

    /// <summary>
    /// Adds the MPEG muxer filter
    /// </summary>
    /// <param name="_graphBuilder">GraphBuilder</param>
    /// <param name="_capture">Capture</param>
    /// <returns></returns>
    private bool AddMpegMuxer(IFilterGraph2 _graphBuilder, Capture _capture)
    {
      Log.Log.WriteFile("analog:AddMpegMuxer()");
      try
      {
        const string monikerPowerDirectorMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7F2BBEAF-E11C-4D39-90E8-938FB5A86045}";
        const string monikerPowerDvdMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{6770E328-9B73-40C5-91E6-E2F321AEDE57}";
        const string monikerPowerDvdMuxer2 = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{370E9701-9DC5-42C8-BE29-4E75F0629EED}";
        _filterMpegMuxer = Marshal.BindToMoniker(monikerPowerDirectorMuxer) as IBaseFilter;
        int hr = _graphBuilder.AddFilter(_filterMpegMuxer, "CyberLink MPEG Muxer");
        if (hr != 0)
        {
          _filterMpegMuxer = Marshal.BindToMoniker(monikerPowerDvdMuxer) as IBaseFilter;
          hr = _graphBuilder.AddFilter(_filterMpegMuxer, "CyberLink MPEG Muxer");
          if (hr != 0)
          {
            _filterMpegMuxer = Marshal.BindToMoniker(monikerPowerDvdMuxer2) as IBaseFilter;
            hr = _graphBuilder.AddFilter(_filterMpegMuxer, "CyberLink MPEG Muxer");
            if (hr != 0)
            {
              Log.Log.WriteFile("analog:AddMpegMuxer returns:0x{0:X}", hr);
              //throw new TvException("Unable to add Cyberlink MPEG Muxer");
            }
          }
        }
        Log.Log.WriteFile("analog:connect pinvideo {0} ->mpeg muxer", FilterGraphTools.LogPinInfo(_pinVideo));
        if (!FilterGraphTools.ConnectPin(_graphBuilder, _pinVideo, _filterMpegMuxer, 0))
        {
          Log.Log.WriteFile("analog: unable to connect pinvideo->mpeg muxer");
          throw new TvException("Unable to connect pins");
        }
        _pinVideoConnected = true;
        Log.Log.WriteFile("analog: connected pinvideo->mpeg muxer");
        //Adaptec devices use the LPCM pin for audio so we check this can connect if applicable.
        bool isAdaptec = false;
        if (_capture.VideoCaptureName.Contains("Adaptec USB Capture Device") || _capture.VideoCaptureName.Contains("Adaptec PCI Capture Device")
          || _capture.AudioCaptureName.Contains("Adaptec USB Capture Device") || _capture.AudioCaptureName.Contains("Adaptec PCI Capture Device"))
        {
          Log.Log.WriteFile("analog: AddMpegMuxer, Adaptec device found using LPCM");
          isAdaptec = true;
        }
        if (isAdaptec)
        {
          if (!FilterGraphTools.ConnectPin(_graphBuilder, _pinLPCM, _filterMpegMuxer, 1))
          {
            Log.Log.WriteFile("analog: AddMpegMuxer, unable to connect pinLPCM->mpeg muxer");
            throw new TvException("Unable to connect pins");
          }
          Log.Log.WriteFile("analog: AddMpegMuxer, connected pinLPCM->mpeg muxer");
        }
        else
        {
          Log.Log.WriteFile("analog:connect pinaudio {0} ->mpeg muxer", FilterGraphTools.LogPinInfo(_pinAudio));
          if (!FilterGraphTools.ConnectPin(_graphBuilder, _pinAudio, _filterMpegMuxer, 1))
          {
            Log.Log.WriteFile("analog:AddMpegMuxer, unable to connect pinaudio->mpeg muxer");
            throw new TvException("Unable to connect pins");
          }
          Log.Log.WriteFile("analog:AddMpegMuxer, connected pinaudio->mpeg muxer");
        }
        return true;
      } catch (Exception ex)
      {
        throw new TvException("Cyberlink MPEG Muxer filter (mpgmux.ax) not installed " + ex.Message);
      }
    }
    #endregion

    #region public methods
    /// <summary>
    /// Updates the video pin to guarantee, that for tv both streams are in the mux
    /// </summary>
    /// <param name="isTv">true, when tv is on; false for radio</param>
    /// <param name="graphBuilder">GraphBuilder</param>
    public void UpdatePinVideo(bool isTv,IFilterGraph2 graphBuilder)
    {
      if (isTv == _pinVideoConnected)
        return;
      _pinVideoConnected = isTv;
      if (_pinVideoConnected)
      {
        Log.Log.Write("analog: Update pin video: connect");
        if(!FilterGraphTools.ConnectPin(graphBuilder, _pinVideo, _filterMpegMuxer, 0))
        {
          throw new TvException("Unable to connect pins");
        }
      }
      else
      {
        Log.Log.Write("analog: Update pin video: disconnect");
        _pinVideo.Disconnect();
      }
    }
    #endregion
  }
}
