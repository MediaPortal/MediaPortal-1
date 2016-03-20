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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.Thumbnailer
{
  /// <summary>
  /// Settings for the thumbnail encoder.
  /// </summary>
  public class ThumbnailSettings
  {
    public CompositingQuality CompositingQuality = CompositingQuality.Default;
    public EncoderParameters EncoderParams = null;
    public int HorizontalResolution = 400;
    public ImageCodecInfo ImageCodecInfo = null;
    public InterpolationMode InterpolationMode = InterpolationMode.Default;
    public SmoothingMode SmoothingMode = SmoothingMode.Default;

    public int ColumnCount = 1;
    public int RowCount = 1;
    public TimeSpan TimeOffset = new TimeSpan(0, 3, 0);

    public ThumbnailSettings(RecordingThumbnailQuality quality)
    {
      int encoderQualityPercentage;
      switch (quality)
      {
        case RecordingThumbnailQuality.Fastest:
          CompositingQuality = CompositingQuality.HighSpeed;
          HorizontalResolution = 400;
          InterpolationMode = InterpolationMode.NearestNeighbor;
          SmoothingMode = SmoothingMode.None;
          encoderQualityPercentage = 25;
          break;

        case RecordingThumbnailQuality.Fast:
          CompositingQuality = CompositingQuality.HighSpeed;
          HorizontalResolution = 400;
          InterpolationMode = InterpolationMode.Low;
          SmoothingMode = SmoothingMode.HighSpeed;
          encoderQualityPercentage = 33;
          break;

        case RecordingThumbnailQuality.Higher:
          CompositingQuality = CompositingQuality.AssumeLinear;
          HorizontalResolution = 500;
          InterpolationMode = InterpolationMode.High;
          SmoothingMode = SmoothingMode.HighQuality;
          encoderQualityPercentage = 77;
          break;

        case RecordingThumbnailQuality.Highest:
          CompositingQuality = CompositingQuality.HighQuality;
          HorizontalResolution = 600;
          InterpolationMode = InterpolationMode.HighQualityBicubic;
          SmoothingMode = SmoothingMode.HighQuality;
          encoderQualityPercentage = 97;
          break;

        default:
          CompositingQuality = CompositingQuality.Default;
          HorizontalResolution = 500;
          InterpolationMode = InterpolationMode.Default;
          SmoothingMode = SmoothingMode.Default;
          encoderQualityPercentage = 50;
          break;
      }

      // Get all image codecs that are available
      EncoderParams = new EncoderParameters(1);
      EncoderParams.Param[0] = new EncoderParameter(Encoder.Quality, encoderQualityPercentage);
    }
  }
}