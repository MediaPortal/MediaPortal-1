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

#if (UseCaptureCardDefinitions)

using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using DirectShowLib;
using MediaPortal.GUI.Library;

namespace DShowNET.Helper
{
  /// <summary>
  /// 
  /// </summary>
  public class VideoCaptureDevice : IDisposable
  {
    private ICaptureGraphBuilder2 _captureGraphBuilderInterface = null;
    private IGraphBuilder _graphBuilderInterface = null;
    private IBaseFilter _captureFilter = null;
    private IPin _pinCapture = null;
    private IPin _pinAudioPreview = null;
    private IPin _pinVideoPreview = null;
    private IPin _pinVideoPort = null;
    private IAMStreamConfig _interfaceStreamConfigVideoCapture = null;
    private IAMStreamConfig _interfaceStreamConfigPreview = null;
    private IAMStreamConfig _interfaceStreamConfigVideoPort = null;

    /// <summary>
    /// #MW#
    /// New VideoCaptureDevice.
    /// This one replaces the original one and hardly does anything anymore ;-)
    /// Has only been tested with PVR150MCEs, but might work for other MPEG2 cards too.
    /// 
    /// There is some redundancy in here, due to support of existing code and due to the
    /// fact that I cannot seem to pass TVCaptureDevice as a parameter. Using that object to
    /// access some of the variables would be easier, instead of copying for instance the IsMCECard
    /// variable everywhere ;-)
    /// 
    /// </summary>
    /// <param name="pGraphBuilder"></param>
    /// <param name="pCaptureGraphBuilder"></param>
    /// <param name="pCaptureFilter"></param>
    /// <param name="pEncoderFilter"></param>
    public VideoCaptureDevice(
      IGraphBuilder pGraphBuilder,
      ICaptureGraphBuilder2 pCaptureGraphBuilder,
      IBaseFilter pCaptureFilter,
      IBaseFilter pEncoderFilter)
    {
      int hr = 0;
      object o = null;

      // Fill in the required fields.
      _graphBuilderInterface = pGraphBuilder;
      _captureGraphBuilderInterface = pCaptureGraphBuilder;
      _captureFilter = pCaptureFilter;

      // Now get the output of the encoder filter and use that....
      // NOTE:
      // 1. The Encoder filter also might be the capture filter...
      // 2. I have no idea if the preview stuff should be reset to null or not to work
      //		with NON MCE devices. Aren't they needed for normal MPEG2 devices???

      _pinAudioPreview = null;
      _pinVideoPreview = null;
      _pinVideoPort = null;
      _pinCapture = DsFindPin.ByDirection(pEncoderFilter, PinDirection.Output, 0);
      if (_pinCapture != null)
      {
        Log.Info("VideoCaptureDevice: found output pin");
      }

      // get video stream interfaces
      Log.Info("VideoCaptureDevice:get Video stream control interface (IAMStreamConfig)");
      DsGuid cat = new DsGuid(PinCategory.Capture);
      Guid iid = typeof (IAMStreamConfig).GUID;
      hr = _captureGraphBuilderInterface.FindInterface(cat, null, _captureFilter, iid, out o);
      if (hr == 0)
      {
        _interfaceStreamConfigVideoCapture = o as IAMStreamConfig;
        Log.Info("VideoCaptureDevice:got IAMStreamConfig for Capture");
      }

      o = null;
      cat = new DsGuid(PinCategory.Preview);
      iid = typeof (IAMStreamConfig).GUID;
      hr = _captureGraphBuilderInterface.FindInterface(cat, null, _captureFilter, iid, out o);
      if (hr == 0)
      {
        _interfaceStreamConfigPreview = o as IAMStreamConfig;
        Log.Info("VideoCaptureDevice:got IAMStreamConfig for Preview");
      }

      o = null;
      cat = new DsGuid(PinCategory.VideoPort);
      iid = typeof (IAMStreamConfig).GUID;
      hr = _captureGraphBuilderInterface.FindInterface(cat, null, _captureFilter, iid, out o);
      if (hr == 0)
      {
        _interfaceStreamConfigVideoPort = o as IAMStreamConfig;
        Log.Info("VideoCaptureDevice:got IAMStreamConfig for VPort");
      }
    }

    public IPin CapturePin
    {
      get { return _pinCapture; }
    }

    public IPin PreviewVideoPin
    {
      get { return _pinVideoPreview; }
    }

    public IPin PreviewAudioPin
    {
      get { return _pinAudioPreview; }
    }

    public IPin VideoPort
    {
      get { return _pinVideoPort; }
    }

    public bool RenderPreview()
    {
      Log.Info("VideoCaptureDevice:render preview");
      int hr;
      if (null != _pinVideoPort)
      {
        Log.Info("VideoCaptureDevice:render videoport pin");
        hr = _graphBuilderInterface.Render(_pinVideoPort);
        if (hr == 0)
        {
          return true;
        }
        Log.Info("VideoCaptureDevice:FAILED render videoport pin:0x{0:X}", hr);
      }
      if (null != _pinVideoPreview)
      {
        Log.Info("VideoCaptureDevice:render preview pin");
        hr = _graphBuilderInterface.Render(_pinVideoPreview);
        if (hr == 0)
        {
          return true;
        }
        Log.Info("VideoCaptureDevice:FAILED render preview pin:0x{0:X}", hr);
      }
      if (null != _pinCapture)
      {
        Log.Info("VideoCaptureDevice:render capture pin");
        hr = _graphBuilderInterface.Render(_pinCapture);
        if (hr == 0)
        {
          return true;
        }
        Log.Info("VideoCaptureDevice:FAILED render capture pin:0x{0:X}", hr);
      }
      return false;
    }


    private IPin FindVideoPort(IBaseFilter filter, ref Guid mediaType)
    {
      IPin pPin;
      DsGuid cat = new DsGuid(PinCategory.VideoPort);
      int hr = _captureGraphBuilderInterface.FindPin(filter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0,
                                                     out pPin);
      if (hr >= 0 && pPin != null)
      {
        Log.Info("VideoCaptureDevice:Found videoport pin");
      }
      return pPin;
    }

    private IPin FindPreviewPin(IBaseFilter filter, ref Guid mediaType)
    {
      IPin pPin;
      DsGuid cat = new DsGuid(PinCategory.Preview);
      int hr = _captureGraphBuilderInterface.FindPin(filter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0,
                                                     out pPin);
      if (hr >= 0 && pPin != null)
      {
        Log.Info("VideoCaptureDevice:Found preview pin");
      }
      return pPin;
    }

    private IPin FindCapturePin(IBaseFilter filter, ref Guid mediaType)
    {
      IPin pPin = null;
      DsGuid cat = new DsGuid(PinCategory.Capture);
      int hr = _captureGraphBuilderInterface.FindPin(filter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0,
                                                     out pPin);
      if (hr >= 0 && pPin != null)
      {
        Log.Info("VideoCaptureDevice:Found capture pin");
      }
      return pPin;
    }


    public void Dispose()
    {
      int hr = 0;
      if (_pinCapture != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(_pinCapture)) > 0)
        {
          ;
        }
        _pinCapture = null;
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(_pinCapture):{0}", hr);
        }
      }

      if (_pinAudioPreview != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(_pinAudioPreview)) > 0)
        {
          _pinAudioPreview = null;
        }
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(_pinAudioPreview):{0}", hr);
        }
      }
      if (_pinVideoPreview != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(_pinVideoPreview)) > 0)
        {
          _pinVideoPreview = null;
        }
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(_pinVideoPreview):{0}", hr);
        }
      }
      if (_pinVideoPort != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(_pinVideoPort)) > 0)
        {
          _pinVideoPort = null;
        }
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(_pinVideoPort):{0}", hr);
        }
      }

      _interfaceStreamConfigVideoCapture = null;
      _interfaceStreamConfigVideoPort = null;
      _interfaceStreamConfigPreview = null;
      _captureFilter = null;
      _captureGraphBuilderInterface = null;
      _graphBuilderInterface = null;
    }

    public Size GetFrameSize()
    {
      if (_interfaceStreamConfigVideoCapture != null)
      {
        try
        {
          BitmapInfoHeader bmiHeader;
          object obj = getStreamConfigSetting(_interfaceStreamConfigVideoCapture, "BmiHeader");
          if (obj != null)
          {
            bmiHeader = (BitmapInfoHeader) obj;
            return new Size(bmiHeader.Width, bmiHeader.Height);
          }
        }
        catch (Exception)
        {
        }
      }

      if (_interfaceStreamConfigPreview != null)
      {
        try
        {
          BitmapInfoHeader bmiHeader;
          object obj = getStreamConfigSetting(_interfaceStreamConfigPreview, "BmiHeader");
          if (obj != null)
          {
            bmiHeader = (BitmapInfoHeader) obj;
            bmiHeader = (BitmapInfoHeader) obj;
            return new Size(bmiHeader.Width, bmiHeader.Height);
          }
        }
        catch (Exception)
        {
        }
      }

      if (_interfaceStreamConfigVideoPort != null)
      {
        try
        {
          BitmapInfoHeader bmiHeader;
          object obj = getStreamConfigSetting(_interfaceStreamConfigVideoPort, "BmiHeader");
          if (obj != null)
          {
            bmiHeader = (BitmapInfoHeader) obj;
            return new Size(bmiHeader.Width, bmiHeader.Height);
          }
        }
        catch (Exception)
        {
        }
      }
      return new Size(720, 576);
    }


    public void SetFrameSize(Size FrameSize)
    {
      if (FrameSize.Width > 0 && FrameSize.Height > 0)
      {
        if (_interfaceStreamConfigVideoCapture != null)
        {
          try
          {
            BitmapInfoHeader bmiHeader;
            object obj = getStreamConfigSetting(_interfaceStreamConfigVideoCapture, "BmiHeader");
            if (obj != null)
            {
              bmiHeader = (BitmapInfoHeader) obj;
              Log.Info("VideoCaptureDevice:change capture Framesize :{0}x{1} ->{2}x{3}", bmiHeader.Width,
                       bmiHeader.Height, FrameSize.Width, FrameSize.Height);
              bmiHeader.Width = FrameSize.Width;
              bmiHeader.Height = FrameSize.Height;
              setStreamConfigSetting(_interfaceStreamConfigVideoCapture, "BmiHeader", bmiHeader);
            }
          }
          catch (Exception)
          {
            Log.Info("VideoCaptureDevice:FAILED:could not set capture  Framesize to {0}x{1}!", FrameSize.Width,
                     FrameSize.Height);
          }
        }

        if (_interfaceStreamConfigPreview != null)
        {
          try
          {
            BitmapInfoHeader bmiHeader;
            object obj = getStreamConfigSetting(_interfaceStreamConfigPreview, "BmiHeader");
            if (obj != null)
            {
              bmiHeader = (BitmapInfoHeader) obj;
              Log.Info("VideoCaptureDevice:change preview Framesize :{0}x{1} ->{2}x{3}", bmiHeader.Width,
                       bmiHeader.Height, FrameSize.Width, FrameSize.Height);
              bmiHeader.Width = FrameSize.Width;
              bmiHeader.Height = FrameSize.Height;
              setStreamConfigSetting(_interfaceStreamConfigPreview, "BmiHeader", bmiHeader);
            }
          }
          catch (Exception)
          {
            Log.Info("VideoCaptureDevice:FAILED:could not set preview Framesize to {0}x{1}!", FrameSize.Width,
                     FrameSize.Height);
          }
        }

        if (_interfaceStreamConfigVideoPort != null)
        {
          try
          {
            BitmapInfoHeader bmiHeader;
            object obj = getStreamConfigSetting(_interfaceStreamConfigVideoPort, "BmiHeader");
            if (obj != null)
            {
              bmiHeader = (BitmapInfoHeader) obj;
              Log.Info("SWGraph:change vport Framesize :{0}x{1} ->{2}x{3}", bmiHeader.Width, bmiHeader.Height,
                       FrameSize.Width, FrameSize.Height);
              bmiHeader.Width = FrameSize.Width;
              bmiHeader.Height = FrameSize.Height;
              setStreamConfigSetting(_interfaceStreamConfigVideoPort, "BmiHeader", bmiHeader);
            }
          }
          catch (Exception)
          {
            Log.Info("VideoCaptureDevice:FAILED:could not set vport Framesize to {0}x{1}!", FrameSize.Width,
                     FrameSize.Height);
          }
        }
      }
    }

    public void SetFrameRate(double FrameRate)
    {
      /*
      // set the framerate
      if (FrameRate >= 1d && FrameRate < 30d)
      {
        if (_interfaceStreamConfigVideoCapture != null)
        {
          try
          {
            Log.Info("VideoCaptureDevice: capture FrameRate set to {0}", FrameRate);
            long avgTimePerFrame = (long)(10000000d / FrameRate);
            setStreamConfigSetting(_interfaceStreamConfigVideoCapture, "AvgTimePerFrame", avgTimePerFrame);
            Log.Info("VideoCaptureDevice: capture FrameRate done :{0}", FrameRate);
          }
          catch (Exception)
          {
            Log.Info("VideoCaptureDevice:captureFAILED:could not set FrameRate to {0}!", FrameRate);
          }
        }

        if (_interfaceStreamConfigPreview != null)
        {
          try
          {
            Log.Info("VideoCaptureDevice:preview FrameRate set to {0}", FrameRate);
            long avgTimePerFrame = (long)(10000000d / FrameRate);
            setStreamConfigSetting(_interfaceStreamConfigPreview, "AvgTimePerFrame", avgTimePerFrame);
            Log.Info("VideoCaptureDevice: preview FrameRate done :{0}", FrameRate);
          }
          catch (Exception)
          {
            Log.Info("VideoCaptureDevice:preview FAILED:could not set FrameRate to {0}!", FrameRate);
          }
        }

        if (_interfaceStreamConfigVideoPort != null)
        {
          try
          {
            Log.Info("VideoCaptureDevice:vport FrameRate set to {0}", FrameRate);
            long avgTimePerFrame = (long)(10000000d / FrameRate);
            setStreamConfigSetting(_interfaceStreamConfigVideoPort, "AvgTimePerFrame", avgTimePerFrame);
            Log.Info("VideoCaptureDevice: vport FrameRate done :{0}", FrameRate);
          }
          catch (Exception)
          {
            Log.Info("VideoCaptureDevice:vport FAILED:could not set FrameRate to {0}!", FrameRate);
          }
        }
      }*/
    }

    private object getStreamConfigSetting(IAMStreamConfig streamConfig, string fieldName)
    {
      object returnValue = null;
      try
      {
        if (streamConfig == null)
        {
          throw new NotSupportedException();
        }

        IntPtr pmt = IntPtr.Zero;
        AMMediaType mediaType = new AMMediaType();

        try
        {
          // Get the current format info
          mediaType.formatType = FormatType.VideoInfo2;
          int hr = streamConfig.GetFormat(out mediaType);
          if (hr != 0)
          {
            Log.Info("VideoCaptureDevice:getStreamConfigSetting() FAILED to get:{0} (not supported)", fieldName);
            Marshal.ThrowExceptionForHR(hr);
          }
          // The formatPtr member points to different structures
          // dependingon the formatType
          object formatStruct;
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() find formattype"); 
          if (mediaType.formatType == FormatType.WaveEx)
          {
            formatStruct = new WaveFormatEx();
          }
          else if (mediaType.formatType == FormatType.VideoInfo)
          {
            formatStruct = new VideoInfoHeader();
          }
          else if (mediaType.formatType == FormatType.VideoInfo2)
          {
            formatStruct = new VideoInfoHeader2();
          }
          else if (mediaType.formatType == FormatType.Mpeg2Video)
          {
            formatStruct = new MPEG2VideoInfo();
          }
          else if (mediaType.formatType == FormatType.None)
          {
            //Log.Info("VideoCaptureDevice:getStreamConfigSetting() FAILED no format returned");
            //throw new NotSupportedException("This device does not support a recognized format block.");
            return null;
          }
          else
          {
            //Log.Info("VideoCaptureDevice:getStreamConfigSetting() FAILED unknown fmt:{0} {1} {2}", mediaType.formatType, mediaType.majorType, mediaType.subType);
            //throw new NotSupportedException("This device does not support a recognized format block.");
            return null;
          }

          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() get formatptr");
          // Retrieve the nested structure
          Marshal.PtrToStructure(mediaType.formatPtr, formatStruct);

          // Find the required field
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() get field");
          Type structType = formatStruct.GetType();
          FieldInfo fieldInfo = structType.GetField(fieldName);
          if (fieldInfo == null)
          {
            //Log.Info("VideoCaptureDevice.getStreamConfigSetting() FAILED to to find member:{0}", fieldName);
            //throw new NotSupportedException("VideoCaptureDevice:FAILED to find the member '" + fieldName + "' in the format block.");
            return null;
          }

          // Extract the field's current value
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() get value");
          returnValue = fieldInfo.GetValue(formatStruct);
          //Log.Info("  VideoCaptureDevice.getStreamConfigSetting() done");	
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmt);
        }
      }
      catch (Exception)
      {
        Log.Info("  VideoCaptureDevice.getStreamConfigSetting() FAILED ");
      }
      return (returnValue);
    }

    private object setStreamConfigSetting(IAMStreamConfig streamConfig, string fieldName, object newValue)
    {
      try
      {
        object returnValue = null;
        IntPtr pmt = IntPtr.Zero;
        AMMediaType mediaType = new AMMediaType();

        try
        {
          // Get the current format info
          int hr = streamConfig.GetFormat(out mediaType);
          if (hr != 0)
          {
            Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED to set:{0} (getformat) hr:{1}", fieldName, hr);
            return null; //Marshal.ThrowExceptionForHR(hr);
          }
          //Log.Info("  VideoCaptureDevice:setStreamConfigSetting() get formattype");
          // The formatPtr member points to different structures
          // dependingon the formatType
          object formatStruct;
          if (mediaType.formatType == FormatType.WaveEx)
          {
            formatStruct = new WaveFormatEx();
          }
          else if (mediaType.formatType == FormatType.VideoInfo)
          {
            formatStruct = new VideoInfoHeader();
          }
          else if (mediaType.formatType == FormatType.VideoInfo2)
          {
            formatStruct = new VideoInfoHeader2();
          }
          else if (mediaType.formatType == FormatType.Mpeg2Video)
          {
            formatStruct = new MPEG2VideoInfo();
          }
          else if (mediaType.formatType == FormatType.None)
          {
            Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED no format returned");
            return null; // throw new NotSupportedException("This device does not support a recognized format block.");
          }
          else
          {
            Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED unknown fmt");
            return null; //throw new NotSupportedException("This device does not support a recognized format block.");
          }
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() get formatptr");
          // Retrieve the nested structure
          Marshal.PtrToStructure(mediaType.formatPtr, formatStruct);

          // Find the required field
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() get field");
          Type structType = formatStruct.GetType();
          FieldInfo fieldInfo = structType.GetField(fieldName);
          if (fieldInfo == null)
          {
            Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED to to find member:{0}", fieldName);
            //throw new NotSupportedException("FAILED to find the member '" + fieldName + "' in the format block.");
          }
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() set value");
          // Update the value of the field
          fieldInfo.SetValue(formatStruct, newValue);

          // PtrToStructure copies the data so we need to copy it back
          Marshal.StructureToPtr(formatStruct, mediaType.formatPtr, false);

          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() set format");
          // Save the changes
          hr = streamConfig.SetFormat(mediaType);
          if (hr != 0)
          {
            Log.Info("  VideoCaptureDevice:setStreamConfigSetting() FAILED to set:{0} {1}", fieldName, hr);
            return null; //Marshal.ThrowExceptionForHR(hr);
          }
          //else Log.Info("  VideoCaptureDevice.setStreamConfigSetting() set:{0}",fieldName);
          //Log.Info("  VideoCaptureDevice.setStreamConfigSetting() done");
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmt);
        }
        return (returnValue);
      }
      catch (Exception)
      {
        Log.Info("  VideoCaptureDevice.:setStreamConfigSetting() FAILED ");
      }
      return null;
    }
  }
}

#endif