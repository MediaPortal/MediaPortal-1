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
using System.IO;
using System.Globalization;
using System.Linq;

using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace MediaPortal.Player
{
  public class MediaInfoWrapper
  {
    #region private vars

    private static readonly HashSet<string> SubTitleExtensions = new HashSet<string>();

    private readonly List<VideoStream> videoStreams;
    private readonly List<AudioStream> audioStreams;
    private readonly VideoStream bestVideo;
    private readonly AudioStream bestAudio;
    private readonly List<SubtitleStream> subtitleStreams;
    private readonly List<Chapter> chapters;
    private readonly string sourceLocation;

    //Video
    private readonly int videoDuration;
    private bool _DVDenabled = false;

    //Subtitles
    private readonly bool hasExternalSubtitles;

    #endregion

    #region ctor's

    public static bool MediaInfoExist()
    {
      string dll = Configuration.Config.GetFolder(Configuration.Config.Dir.Base) + "\\MediaInfo.dll";
      bool enable = File.Exists(dll);
      if (!enable)
      {
        Log.Warn("MediaInfoWrapper: disabled because \"{0}\" is missing", dll);
      }
      return enable;
    }

    public MediaInfoWrapper(string strFile)
    {
      string parseSpeed;
      sourceLocation = strFile;
      MediaInfoNotloaded = false;
      videoStreams = new List<VideoStream>();
      audioStreams = new List<AudioStream>();
      subtitleStreams = new List<SubtitleStream>();
      chapters = new List<Chapter>();

      if (!MediaInfoExist())
      {
        MediaInfoNotloaded = true;
        return;
      }

      using (Settings xmlreader = new MPSettings())
      {
        _DVDenabled = xmlreader.GetValueAsBool("dvdplayer", "mediainfoused", false);
        parseSpeed = xmlreader.GetValueAsString("debug", "MediaInfoParsespeed", "0.3");
        // fix delay introduced after 0.7.26: http://sourceforge.net/tracker/?func=detail&aid=3013548&group_id=86862&atid=581181
      }
      bool isTV = Util.Utils.IsLiveTv(strFile);
      bool isRadio = Util.Utils.IsLiveRadio(strFile);
      bool isRTSP = Util.Utils.IsRTSP(strFile); //rtsp for live TV and recordings.
      bool isAVStream = Util.Utils.IsAVStream(strFile); //other AV streams
      var isNetwork = Util.Utils.IsNetwork(strFile);

      //currently disabled for all tv/radio
      if (isTV || isRadio || isRTSP)
      {
        Log.Debug(
          "MediaInfoWrapper: isTv:{0}, isRadio:{1}, isRTSP:{2}, isAVStream:{3}",
          isTV,
          isRadio,
          isRTSP,
          isAVStream);
        Log.Debug("MediaInfoWrapper: disabled for this content");
        MediaInfoNotloaded = true;
        return;
      }

      if (strFile.EndsWith(".wtv", StringComparison.OrdinalIgnoreCase))
      {
        Log.Debug("MediaInfoWrapper: WTV file is not handled");
        MediaInfoNotloaded = true;
        return;
      }

      NumberFormatInfo providerNumber = new NumberFormatInfo { NumberDecimalSeparator = "." };

      MediaInfo mediaInfo = null;
      try
      {
        mediaInfo = new MediaInfo();
        mediaInfo.Option("ParseSpeed", parseSpeed);

        // Analyze local file for DVD and BD
        if (!(isAVStream || isNetwork))
        {
          if (VirtualDirectory.IsImageFile(Path.GetExtension(strFile)))
          {
            strFile = DaemonTools.GetVirtualDrive() + @"\VIDEO_TS\VIDEO_TS.IFO";

            if (!File.Exists(strFile))
            {
              strFile = DaemonTools.GetVirtualDrive() + @"\BDMV\index.bdmv";

              if (!File.Exists(strFile))
              {
                MediaInfoNotloaded = true;
                return;
              }
            }
          }

          if (strFile.EndsWith(".ifo", StringComparison.OrdinalIgnoreCase))
          {
            IsDVD = true;
            var path = Path.GetDirectoryName(strFile) ?? string.Empty;
            var bups = Directory.GetFiles(path, "*.BUP", SearchOption.TopDirectoryOnly);
            var programBlocks = new List<Tuple<string, int>>();
            foreach (var bupFile in bups)
            {
              using (var mi = new MediaInfo())
              {
                mi.Open(bupFile);
                var profile = mi.Get(StreamKind.General, 0, "Format_Profile");
                if (profile == "Program")
                {
                  double duration;
                  double.TryParse(mi.Get(StreamKind.Video, 0, "Duration"), NumberStyles.AllowDecimalPoint, providerNumber, out duration);
                  programBlocks.Add(new Tuple<string, int>(bupFile, (int)duration));
                }
              }
            }
            // get all other info from main title's 1st vob
            if (programBlocks.Any())
            {
              videoDuration = programBlocks.Max(x => x.Item2);
              strFile = programBlocks.First(x => x.Item2 == videoDuration).Item1;
            }
          }
          else if (strFile.EndsWith(".bdmv", StringComparison.OrdinalIgnoreCase))
          {
            IsBD = true;
            var path = Path.GetDirectoryName(strFile) + @"\STREAM";
            strFile = GetLargestFileInDirectory(path, "*.m2ts");
          }

          hasExternalSubtitles = !string.IsNullOrEmpty(strFile) && CheckHasExternalSubtitles(strFile);
        }

        if (string.IsNullOrEmpty(strFile))
        {
          MediaInfoNotloaded = true;
          return;
        }

        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Opening file : {0}", strFile);
        mediaInfo.Open(strFile);

        var streamNumber = 0;
        //Video
        var videoStreamCount = mediaInfo.Count_Get(StreamKind.Video);
        for (var i = 0; i < videoStreamCount; ++i)
        {
          videoStreams.Add(new VideoStream(mediaInfo, streamNumber++, i));
        }

        if (videoDuration == 0)
        {
          double duration;
          double.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Duration"), NumberStyles.AllowDecimalPoint, providerNumber, out duration);
          videoDuration = (int)duration;
        }

        //Audio
        var iAudioStreams = mediaInfo.Count_Get(StreamKind.Audio);
        for (var i = 0; i < iAudioStreams; ++i)
        {
          audioStreams.Add(new AudioStream(mediaInfo, streamNumber++, i));
        }

        //Subtitles
        var numsubtitles = mediaInfo.Count_Get(StreamKind.Text);

        for (var i = 0; i < numsubtitles; ++i)
        {
          subtitleStreams.Add(new SubtitleStream(mediaInfo, streamNumber++, i));
        }

        var numChapters = mediaInfo.Count_Get(StreamKind.Other);

        for (var i = 0; i < numChapters; ++i)
        {
          chapters.Add(new Chapter(mediaInfo, streamNumber++, i));
        }

        MediaInfoNotloaded = videoStreams.Count == 0 && audioStreams.Count == 0 && subtitleStreams.Count == 0;

        // Produce copability properties
        if (!MediaInfoNotloaded)
        {
          bestVideo = videoStreams.OrderByDescending(x => (long)x.Width * (long)x.Height * x.BitDepth * (x.Stereoscopic == StereoMode.Mono ? 1L : 2L) * x.FrameRate).FirstOrDefault();
          VideoCodec = bestVideo != null ? bestVideo.CodecName : string.Empty;
          VideoResolution = bestVideo != null ? bestVideo.Resolution : string.Empty;
          Width = bestVideo != null ? bestVideo.Width : 0;
          Height = bestVideo != null ? bestVideo.Height : 0;
          IsInterlaced = bestVideo != null && bestVideo.Interlaced;
          Framerate = bestVideo != null ? bestVideo.FrameRate : 0;
          ScanType = bestVideo != null ? mediaInfo.Get(StreamKind.Video, bestVideo.StreamPosition, "ScanType").ToLower() : string.Empty;
          AspectRatio = bestVideo != null ? mediaInfo.Get(StreamKind.Video, bestVideo.StreamPosition, "DisplayAspectRatio") : string.Empty;
          AspectRatio = AspectRatio == "4:3" || AspectRatio == "1.333" ? "fullscreen" : "widescreen";

          bestAudio = audioStreams.OrderByDescending(x => x.Channel * 10000000 + x.Bitrate).FirstOrDefault();
          AudioCodec = bestAudio != null ? bestAudio.CodecName : string.Empty;
          AudioRate = bestAudio != null ? (int)bestAudio.Bitrate : 0;
          AudioChannels = bestAudio != null ? bestAudio.Channel : 0;
          AudioChannelsFriendly = bestAudio != null ? bestAudio.AudioChannelsFriendly : string.Empty;
        }
        else
        {
          VideoCodec = string.Empty;
          VideoResolution = string.Empty;
          ScanType = string.Empty;
          AspectRatio = string.Empty;

          AudioCodec = string.Empty;
          AudioChannelsFriendly = string.Empty;
        }
      }
      catch (Exception e)
      {
        Log.Error("MediaInfoWrapper.MediaInfoWrapper: Error occurred while scanning media: '{0}'. Error {1}", strFile, e.Message);
      }
      finally
      {
        if (mediaInfo != null)
        {
          mediaInfo.Dispose();
        }
      }
    }

    #endregion

    #region private methods

    private static string GetLargestFileInDirectory(string targetDir, string fileMask)
    {
      string largestFile = null;
      long largestSize = 0;
      var dir = new DirectoryInfo(targetDir);
      try
      {
        var files = dir.GetFiles(fileMask, SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
          var fileSize = file.Length;
          if (fileSize > largestSize)
          {
            largestSize = fileSize;
            largestFile = file.FullName;
          }
        }
      }
      catch (Exception e)
      {
        Log.Error("MediaInfoWrapper: Error while retrieving files for: {0} {1}" + targetDir, e.Message);
      }

      return largestFile;
    }

    private static bool CheckHasExternalSubtitles(string strFile)
    {
      if (SubTitleExtensions.Count == 0)
      {
        // load them in first time
        SubTitleExtensions.Add(".aqt");
        SubTitleExtensions.Add(".asc");
        SubTitleExtensions.Add(".ass");
        SubTitleExtensions.Add(".dat");
        SubTitleExtensions.Add(".dks");
        SubTitleExtensions.Add(".idx");
        SubTitleExtensions.Add(".js");
        SubTitleExtensions.Add(".jss");
        SubTitleExtensions.Add(".lrc");
        SubTitleExtensions.Add(".mpl");
        SubTitleExtensions.Add(".ovr");
        SubTitleExtensions.Add(".pan");
        SubTitleExtensions.Add(".pjs");
        SubTitleExtensions.Add(".psb");
        SubTitleExtensions.Add(".rt");
        SubTitleExtensions.Add(".rtf");
        SubTitleExtensions.Add(".s2k");
        SubTitleExtensions.Add(".sbt");
        SubTitleExtensions.Add(".scr");
        SubTitleExtensions.Add(".smi");
        SubTitleExtensions.Add(".son");
        SubTitleExtensions.Add(".srt");
        SubTitleExtensions.Add(".ssa");
        SubTitleExtensions.Add(".sst");
        SubTitleExtensions.Add(".ssts");
        SubTitleExtensions.Add(".stl");
        SubTitleExtensions.Add(".sub");
        SubTitleExtensions.Add(".txt");
        SubTitleExtensions.Add(".vkt");
        SubTitleExtensions.Add(".vsf");
        SubTitleExtensions.Add(".zeg");
      }

      var filenameNoExt = Path.GetFileNameWithoutExtension(strFile);
      try
      {
        foreach (string file in Directory.GetFiles(Path.GetDirectoryName(strFile), filenameNoExt + "*"))
        {
          var fi = new FileInfo(file);
          if (SubTitleExtensions.Contains(fi.Extension.ToLowerInvariant()))
          {
            return true;
          }
        }
      }
      catch (Exception)
      {
        // most likley path not available
      }

      return false;
    }

    #endregion

    #region public video related properties

    public int VideoDuration
    {
      get { return videoDuration; }
    }

    public bool HasVideo
    {
      get { return videoStreams.Count > 0; }
    }

    public IList<VideoStream> VideoStreams
    {
      get { return videoStreams; }
    }

    public VideoStream BestVideoStream
    {
      get { return bestVideo; }
    }

    public string VideoCodec { get; private set; }

    public double Framerate { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public string AspectRatio { get; private set; }

    public string ScanType { get; private set; }

    public bool IsInterlaced { get; private set; }

    public string VideoResolution { get; private set; }

    #endregion

    #region public audio related properties

    public IList<AudioStream> AudioStreams
    {
      get { return audioStreams; }
    }

    public AudioStream BestAudioStream
    {
      get { return bestAudio; }
    }

    public string AudioCodec { get; private set; }

    public int AudioRate { get; private set; }

    public int AudioChannels { get; private set; }

    public string AudioChannelsFriendly { get; private set; }

    #endregion

    #region public subtitles related properties

    public IList<SubtitleStream> Subtitles
    {
      get { return subtitleStreams; }
    }

    public bool HasSubtitles
    {
      get { return hasExternalSubtitles || subtitleStreams.Count > 0; }
    }

    public bool HasExternalSubtitles
    {
      get { return hasExternalSubtitles; }
    }

    #endregion

    public bool IsDVD { get; private set; }

    public bool IsBD { get; private set; }

    public bool MediaInfoNotloaded { get; private set; }

    public void PrintInfo()
    {
      if (MediaInfoNotloaded)
      {
        Log.Debug("MediaInfoWrapper: Media info hasn't been loaded");
      }
      else
      {
        var mediaInfo = new MediaInfo();
        Log.Debug("MediaInfoWrapper: DLL Version      {0}", mediaInfo.Option("Info_Version"));
        Log.Debug("MediaInfoWrapper: Inspecting media {0}", sourceLocation);
        for (var i = 0; i < VideoStreams.Count; ++i)
        {
          Log.Debug("MediaInfoWrapper: ----------------------");
          Log.Debug("MediaInfoWrapper: Video stream #{0}", i);
          Log.Debug("MediaInfoWrapper: Codec            : {0}", VideoStreams[i].Format);
          Log.Debug("MediaInfoWrapper: Width            : {0}", VideoStreams[i].Width);
          Log.Debug("MediaInfoWrapper: Height           : {0}", VideoStreams[i].Height);
          Log.Debug("MediaInfoWrapper: FrameRate        : {0}", VideoStreams[i].FrameRate);
          Log.Debug("MediaInfoWrapper: AspectRatio      : {0}", VideoStreams[i].AspectRatio);
          Log.Debug("MediaInfoWrapper: IsInterlaced     : {0}", VideoStreams[i].Interlaced);
          Log.Debug("MediaInfoWrapper: VideoDuration    : {0}", VideoStreams[i].Duration);
          Log.Debug("MediaInfoWrapper: Name             : {0}", VideoStreams[i].Name);
        }

        for (var i = 0; i < AudioStreams.Count; ++i)
        {
          Log.Debug("MediaInfoWrapper: ----------------------");
          Log.Debug("MediaInfoWrapper: Audio stream #{0}", i);
          Log.Debug("MediaInfoWrapper: Codec            : {0}", AudioStreams[i].Format);
          Log.Debug("MediaInfoWrapper: Channels         : {0} ({1})", AudioStreams[i].Channel, AudioStreams[i].AudioChannelsFriendly);
          Log.Debug("MediaInfoWrapper: Rate             : {0}", AudioStreams[i].Bitrate);
          Log.Debug("MediaInfoWrapper: BitDepth         : {0}", AudioStreams[i].BitDepth);
          Log.Debug("MediaInfoWrapper: Language         : {0}", AudioStreams[i].Language);
          Log.Debug("MediaInfoWrapper: Name             : {0}", AudioStreams[i].Name);
        }

        for (var i = 0; i < Subtitles.Count; ++i)
        {
          Log.Debug("MediaInfoWrapper: ----------------------");
          Log.Debug("MediaInfoWrapper: Subtitle stream #{0}", i);
          Log.Debug("MediaInfoWrapper: Codec            : {0}", Subtitles[i].Format);
          Log.Debug("MediaInfoWrapper: Language         : {0}", Subtitles[i].Language);
        }
        Log.Debug("MediaInfoWrapper: ----------------------");
      }
    }
  }
}