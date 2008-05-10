using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using DirectShowLib;


namespace TvLibrary.Implementations.Analog.QualityControl
{
  public class QualityControlFactory
  {
    public static IQuality createQualityControl(Configuration configuration, IBaseFilter filterVideoEncoder, IBaseFilter filterCapture, IBaseFilter filterMultiplexer, IBaseFilter filterVideoCompressor)
    {
      IVideoEncoder videoEncoder = null;

      if (filterVideoEncoder != null)
      {
        videoEncoder = filterVideoEncoder as IVideoEncoder;
      }

      if (videoEncoder == null && filterCapture != null)
      {
        videoEncoder = filterCapture as IVideoEncoder;
      }

      if (videoEncoder == null && filterMultiplexer != null)
      {
        videoEncoder = filterMultiplexer as IVideoEncoder;
      }

      if (videoEncoder == null && filterVideoCompressor != null)
      {
        videoEncoder = filterVideoCompressor as IVideoEncoder;
      }

      if (videoEncoder != null)
      {
        return new VideoEncoderControl(configuration, videoEncoder);
      }
      return null;
    }
  }
}
