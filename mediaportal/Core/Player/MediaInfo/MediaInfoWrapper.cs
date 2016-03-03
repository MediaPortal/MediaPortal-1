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

namespace MediaPortal.Player.MediaInfo
{
  public class MediaInfoWrapper
  {
    #region private vars

    private List<VideoStream> _videoStreams;
    private List<AudioStream> _audioStreams;
    private List<SubtitleStream> _subtitleStreams;

    //Video
    private int _videoDuration;
    private bool _DVDenabled = false;
    private string _ParseSpeed;

    //Subtitles
    private int _numsubtitles;
    private bool _hasExternalSubtitles;
    private static HashSet<string> _subTitleExtensions = new HashSet<string>();

    private bool _mediaInfoNotloaded = false;

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
      _videoStreams = new List<VideoStream>();
      _audioStreams = new List<AudioStream>();
      _subtitleStreams = new List<SubtitleStream>();

      if (!MediaInfoExist())
      {
        _mediaInfoNotloaded = true;
        return;
      }

      using (Settings xmlreader = new MPSettings())
      {
        _DVDenabled = xmlreader.GetValueAsBool("dvdplayer", "mediainfoused", false);
        _ParseSpeed = xmlreader.GetValueAsString("debug", "MediaInfoParsespeed", "0.3");
        // fix delay introduced after 0.7.26: http://sourceforge.net/tracker/?func=detail&aid=3013548&group_id=86862&atid=581181
      }
      bool isTV = Util.Utils.IsLiveTv(strFile);
      bool isRadio = Util.Utils.IsLiveRadio(strFile);
      bool isRTSP = Util.Utils.IsRTSP(strFile); //rtsp for live TV and recordings.
      bool isAVStream = Util.Utils.IsAVStream(strFile); //other AV streams
      var isNetwork = Util.Utils.IsNetwork(strFile);

      NumberFormatInfo providerNumber = new NumberFormatInfo();
      providerNumber.NumberDecimalSeparator = ".";

      MediaInfo mediaInfo = null;
      try
      {
        mediaInfo = new MediaInfo();
        mediaInfo.Option("ParseSpeed", _ParseSpeed);

        if (strFile.ToLowerInvariant().EndsWith(".wtv"))
        {
          Log.Debug("MediaInfoWrapper: WTV file is not handled");
          _mediaInfoNotloaded = true;
          return;
        }

        if (!(isTV || isRadio || isRTSP || isAVStream || isNetwork))
        {
          if (VirtualDirectory.IsImageFile(Path.GetExtension(strFile)))
          {
            strFile = DaemonTools.GetVirtualDrive() + @"\VIDEO_TS\VIDEO_TS.IFO";

            if (!File.Exists(strFile))
            {
              strFile = DaemonTools.GetVirtualDrive() + @"\BDMV\index.bdmv";

              if (!File.Exists(strFile))
              {
                _mediaInfoNotloaded = true;
                return;
              }
            }
          }

          if (strFile.EndsWith(".ifo", StringComparison.OrdinalIgnoreCase))
          {
            var path = Path.GetDirectoryName(strFile) ?? string.Empty;
            var bups = Directory.GetFiles(path, "*.BUP", SearchOption.TopDirectoryOnly);
            var programBlocks = new List<Tuple<string, int>>();
            foreach (string bupFile in bups)
            {
              using (var mi = new MediaInfo())
              {
                mi.Open(bupFile);
                var profile = mi.Get(StreamKind.General, 0, "Format_Profile");
                if (profile == "Program")
                {
                  double duration;
                  double.TryParse(mi.Get(StreamKind.Video, 0, "Duration"), NumberStyles.AllowDecimalPoint,
                    providerNumber, out duration);
                  programBlocks.Add(new Tuple<string, int>(bupFile, (int) duration));
                }
              }
            }
            // get all other info from main title's 1st vob
            if (programBlocks.Any())
            {
              _videoDuration = programBlocks.Max(x => x.Item2);
              strFile = programBlocks.First(x => x.Item2 == _videoDuration).Item1;
            }
          }
          else if (strFile.EndsWith(".bdmv", StringComparison.OrdinalIgnoreCase))
          {
            var path = Path.GetDirectoryName(strFile) + @"\STREAM";
            strFile = GetLargestFileInDirectory(path, "*.m2ts");
          }

          _hasExternalSubtitles = !string.IsNullOrEmpty(strFile) && checkHasExternalSubtitles(strFile);
        }

        if (string.IsNullOrEmpty(strFile))
        {
          _mediaInfoNotloaded = true;
          return;
        }

        Log.Debug("MediaInfoWrapper.MediaInfoWrapper: Opening file : {0}", strFile);
        mediaInfo.Open(strFile);

        //Video
        var videoStreamCount = mediaInfo.Count_Get(StreamKind.Video);
        for (var i = 0; i < videoStreamCount; ++i)
        {
          _videoStreams.Add(new VideoStream(mediaInfo, i));
        }

        if (_videoDuration == 0)
        {
          double duration;
          double.TryParse(mediaInfo.Get(StreamKind.Video, 0, "Duration"), NumberStyles.AllowDecimalPoint, providerNumber,
            out duration);
          _videoDuration = (int) duration;
        }

        //Audio
        var iAudioStreams = mediaInfo.Count_Get(StreamKind.Audio);
        for (var i = 0; i < iAudioStreams; ++i)
        {
          _audioStreams.Add(new AudioStream(mediaInfo, i));
        }

        //Subtitles
        _numsubtitles = mediaInfo.Count_Get(StreamKind.Text);

        for (var i = 0; i < _numsubtitles; ++i)
        {
          _subtitleStreams.Add(new SubtitleStream(mediaInfo, i));
        }
      }
      catch (Exception e)
      {
        Log.Error("MediaInfoWrapper.MediaInfoWrapper: Error occurred while scanning media: '{0}'. Error {1}", strFile,
          e.Message);
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

    private string GetLargestFileInDirectory(string targetDir, string fileMask)
    {
      string largestFile = null;
      long largestSize = 0;
      DirectoryInfo dir = new DirectoryInfo(targetDir);
      try
      {
        FileInfo[] files = dir.GetFiles(fileMask, SearchOption.TopDirectoryOnly);
        foreach (FileInfo file in files)
        {
          long fileSize = file.Length;
          if (fileSize > largestSize)
          {
            largestSize = fileSize;
            largestFile = file.FullName;
          }
        }
      }
      catch (Exception e)
      {
        Log.Error("Error while retrieving files for: {0} {1}" + targetDir, e.Message);
      }
      return largestFile;
    }

    private bool checkHasExternalSubtitles(string strFile)
    {
      if (_subTitleExtensions.Count == 0)
      {
        // load them in first time
        _subTitleExtensions.Add(".aqt");
        _subTitleExtensions.Add(".asc");
        _subTitleExtensions.Add(".ass");
        _subTitleExtensions.Add(".dat");
        _subTitleExtensions.Add(".dks");
        _subTitleExtensions.Add(".idx");
        _subTitleExtensions.Add(".js");
        _subTitleExtensions.Add(".jss");
        _subTitleExtensions.Add(".lrc");
        _subTitleExtensions.Add(".mpl");
        _subTitleExtensions.Add(".ovr");
        _subTitleExtensions.Add(".pan");
        _subTitleExtensions.Add(".pjs");
        _subTitleExtensions.Add(".psb");
        _subTitleExtensions.Add(".rt");
        _subTitleExtensions.Add(".rtf");
        _subTitleExtensions.Add(".s2k");
        _subTitleExtensions.Add(".sbt");
        _subTitleExtensions.Add(".scr");
        _subTitleExtensions.Add(".smi");
        _subTitleExtensions.Add(".son");
        _subTitleExtensions.Add(".srt");
        _subTitleExtensions.Add(".ssa");
        _subTitleExtensions.Add(".sst");
        _subTitleExtensions.Add(".ssts");
        _subTitleExtensions.Add(".stl");
        _subTitleExtensions.Add(".sub");
        _subTitleExtensions.Add(".txt");
        _subTitleExtensions.Add(".vkt");
        _subTitleExtensions.Add(".vsf");
        _subTitleExtensions.Add(".zeg");
      }
      string filenameNoExt = Path.GetFileNameWithoutExtension(strFile);
      try
      {
        foreach (string file in Directory.GetFiles(Path.GetDirectoryName(strFile), filenameNoExt + "*"))
        {
          System.IO.FileInfo fi = new FileInfo(file);
          if (_subTitleExtensions.Contains(fi.Extension.ToLowerInvariant()))
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
      get { return _videoDuration; }
    }

    public bool HasVideo
    {
      get { return _videoStreams.Count > 0; }
    }

    public IList<VideoStream> VideoStreams
    {
      get { return _videoStreams; }
    }

    public VideoStream BestVideoStream
    {
      get
      {
        return
          _videoStreams.OrderByDescending(
            x => (long) x.Width*(long) x.Height*x.BitDepth*(x.Stereoscopic == StereoMode.Mono ? 1L : 2L)*x.FrameRate)
            .FirstOrDefault();
      }
    }

    #endregion

    #region public audio related properties

    public IList<AudioStream> AudioStreams
    {
      get { return _audioStreams; }
    }

    public AudioStream BestAudioStream
    {
      get { return _audioStreams.OrderByDescending(x => x.Channel*10000000 + x.Bitrate).FirstOrDefault(); }
    }

    #endregion

    #region public subtitles related properties

    public IList<SubtitleStream> Subtitles
    {
      get { return _subtitleStreams; }
    }

    public bool HasSubtitles
    {
      get { return _hasExternalSubtitles || _subtitleStreams.Count > 0; }
    }

    public bool HasExternalSubtitles
    {
      get { return _hasExternalSubtitles; }
    }

    public bool MediaInfoNotloaded
    {
      get { return _mediaInfoNotloaded; }
    }

    #endregion
  }
}