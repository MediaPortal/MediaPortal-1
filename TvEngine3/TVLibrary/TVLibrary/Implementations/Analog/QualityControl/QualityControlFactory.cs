#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using TvLibrary.Interfaces;
using DirectShowLib;


namespace TvLibrary.Implementations.Analog.QualityControl
{
  /// <summary>
  /// Class to create the object that implements the IQuality interface for a card or channel
  /// </summary>
  public class QualityControlFactory
  {
    /// <summary>
    /// Creates the object that implements the IQuality interface
    /// </summary>
    public static IQuality createQualityControl(Configuration configuration, IBaseFilter filterVideoEncoder,
                                                IBaseFilter filterCapture, IBaseFilter filterMultiplexer,
                                                IBaseFilter filterVideoCompressor)
    {
      ICodecAPI codecAPI = checkCodecAPI(filterVideoEncoder, filterCapture, filterMultiplexer, filterVideoCompressor);

      if (codecAPI != null)
      {
        return new CodecAPIControl(configuration, codecAPI);
      }

      IVideoEncoder videoEncoder = checkVideoEncoder(filterVideoEncoder, filterCapture, filterMultiplexer,
                                                     filterVideoCompressor);
      if (videoEncoder != null)
      {
        return new VideoEncoderControl(configuration, videoEncoder);
      }

#pragma warning disable 618,612
      IEncoderAPI encoderAPI = checkEncoderAPI(filterVideoEncoder, filterCapture, filterMultiplexer,
                                               filterVideoCompressor);
      if (encoderAPI != null)
      {
        return new EncoderAPIControl(configuration, encoderAPI);
      }
#pragma warning restore 618,612

      return null;
    }

#pragma warning disable 618,612
    private static IEncoderAPI checkEncoderAPI(IBaseFilter filterVideoEncoder, IBaseFilter filterCapture,
                                               IBaseFilter filterMultiplexer, IBaseFilter filterVideoCompressor)
    {
      IEncoderAPI videoEncoder = null;

      if (filterVideoEncoder != null)
      {
        videoEncoder = filterVideoEncoder as IEncoderAPI;
      }

      if (videoEncoder == null && filterCapture != null)
      {
        videoEncoder = filterCapture as IEncoderAPI;
      }

      if (videoEncoder == null && filterMultiplexer != null)
      {
        videoEncoder = filterMultiplexer as IEncoderAPI;
      }

      if (videoEncoder == null && filterVideoCompressor != null)
      {
        videoEncoder = filterVideoCompressor as IEncoderAPI;
      }

      return videoEncoder;
    }
#pragma warning restore 618,612

    private static IVideoEncoder checkVideoEncoder(IBaseFilter filterVideoEncoder, IBaseFilter filterCapture,
                                                   IBaseFilter filterMultiplexer, IBaseFilter filterVideoCompressor)
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

      return videoEncoder;
    }

    private static ICodecAPI checkCodecAPI(IBaseFilter filterVideoEncoder, IBaseFilter filterCapture,
                                           IBaseFilter filterMultiplexer, IBaseFilter filterVideoCompressor)
    {
      ICodecAPI videoEncoder = null;

      if (filterVideoEncoder != null)
      {
        videoEncoder = filterVideoEncoder as ICodecAPI;
      }

      if (videoEncoder == null && filterCapture != null)
      {
        videoEncoder = filterCapture as ICodecAPI;
      }

      if (videoEncoder == null && filterMultiplexer != null)
      {
        videoEncoder = filterMultiplexer as ICodecAPI;
      }

      if (videoEncoder == null && filterVideoCompressor != null)
      {
        videoEncoder = filterVideoCompressor as ICodecAPI;
      }

      return videoEncoder;
    }
  }
}