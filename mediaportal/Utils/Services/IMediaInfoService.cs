using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaInfo;

namespace MediaPortal.Services
{
  public interface IMediaInfoService
  {
    /// <summary>
    /// Enable or disable caching for BluRay
    /// </summary>
    bool EnabledCachingForBluray { get; set; }

    /// <summary>
    /// Enable or disable caching for DVD
    /// </summary>
    bool EnabledCachingForDVD { get; set; }

    /// <summary>
    /// Enable or disable caching for video files
    /// </summary>
    bool EnabledCachingForVideo { get; set; }

    /// <summary>
    /// Enable or disable caching for audio files
    /// </summary>
    bool EnabledCachingForAudio { get; set; }

    /// <summary>
    /// Enable or disable caching for picture files
    /// </summary>
    bool EnabledCachingForPicture { get; set; }

    /// <summary>
    /// Enable or disable caching for image files
    /// </summary>
    bool EnabledCachingForImage { get; set; }

    /// <summary>
    /// Enable or disable caching for Audio CDs
    /// </summary>
    bool EnabledCachingForAudioCD { get; set; }

    /// <summary>
    /// Record lifetime in days (disabled <= 0)
    /// </summary>
    int RecordLifeTime { get; set; }

    /// <summary>
    /// Get MediaInfo
    /// </summary>
    /// <param name="strMediaFullPath"></param>
    /// <returns></returns>
    MediaInfoWrapper Get(string strMediaFullPath);

    /// <summary>
    /// Clear MediaInfo database
    /// </summary>
    void Clear();
  }
}
