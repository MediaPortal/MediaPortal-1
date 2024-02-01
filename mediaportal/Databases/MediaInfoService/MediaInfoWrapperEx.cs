using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MediaPortal.Database;
using MediaInfo;
using MediaInfo.Model;
using SQLite.NET;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MediaPortal.MediaInfoService.Database
{
  public class MediaInfoWrapperEx : MediaInfoWrapper
  {
    private class PrivateResolver : DefaultContractResolver
    {
      protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
      {
        JsonProperty prop = base.CreateProperty(member, memberSerialization);
        if (!prop.Writable)
        {
          PropertyInfo property = member as PropertyInfo;
          prop.Writable = property?.GetSetMethod(true) != null;
        }
        return prop;
      }
    }

    public MediaInfoWrapperEx(SQLiteClient db, SQLiteResultSet dbResult, string strMediaFullPath, long lSize, ILogger logger)
        : base(lSize, logger)
    {
      int iDbId = DatabaseUtility.GetAsInt(dbResult, 0, "id");

      this.setProperty("Format", DatabaseUtility.Get(dbResult, 0, "format"));
      this.setProperty("IsStreamable", DatabaseUtility.GetAsInt(dbResult, 0, "isStreamable") > 0);
      this.setProperty("WritingApplication", DatabaseUtility.Get(dbResult, 0, "writingApplication"));
      this.setProperty("WritingLibrary", DatabaseUtility.Get(dbResult, 0, "writingLibrary"));
      this.setProperty("Attachments", DatabaseUtility.Get(dbResult, 0, "attachments"));
      this.setProperty("Profile", DatabaseUtility.Get(dbResult, 0, "profile"));
      this.setProperty("FormatVersion", DatabaseUtility.Get(dbResult, 0, "formatVersion"));
      this.setProperty("Codec", DatabaseUtility.Get(dbResult, 0, "codec"));
      this.setProperty("ScanType", DatabaseUtility.Get(dbResult, 0, "scanType"));
      this.setProperty("AspectRatio", DatabaseUtility.Get(dbResult, 0, "aspectRatio"));
      this.setProperty("Duration", DatabaseUtility.GetAsInt(dbResult, 0, "duration"));


      #region VideStreams
      int iStreamPack;
      string strSQL = "SELECT * FROM videoStreams WHERE idFile=" + iDbId;
      SQLiteResultSet results = db.Execute(strSQL);
      if (results != null && results.Rows.Count > 0)
      {
        for (int i = 0; i < results.Rows.Count; i++)
        {
          // language TEXT, lcid INTEGER, default BOOL, forced BOOL streamSize LONG,
          VideoStream s = new VideoStream()
          {
            Id = DatabaseUtility.GetAsInt(results, i, "streamId"),
            Name = DatabaseUtility.Get(results, i, "streamName"),
            StreamPosition = DatabaseUtility.GetAsInt(results, i, "streamPosition"),
            StreamNumber = DatabaseUtility.GetAsInt(results, i, "streamNumber"),
            Language = DatabaseUtility.Get(results, i, "language"),
            Lcid = DatabaseUtility.GetAsInt(results, i, "lcid"),
            Default = DatabaseUtility.GetAsInt(results, i, "flagDefault") > 0,
            Forced = DatabaseUtility.GetAsInt(results, i, "flagForced") > 0,
            StreamSize = DatabaseUtility.GetAsInt64(results, i, "streamSize"),
            FrameRate = (double)DatabaseUtility.GetAsDouble(results, i, "frameRate"),
            FrameRateMode = (FrameRateMode)Enum.Parse(typeof(FrameRateMode), DatabaseUtility.Get(results, i, "frameRateMode"), true),
            Width = DatabaseUtility.GetAsInt(results, i, "width"),
            Height = DatabaseUtility.GetAsInt(results, i, "height"),
            Bitrate = (double)DatabaseUtility.GetAsDouble(results, i, "bitRate"),
            AspectRatio = (AspectRatio)Enum.Parse(typeof(AspectRatio), DatabaseUtility.Get(results, i, "aspectRatio"), true),
            Interlaced = DatabaseUtility.GetAsInt(results, i, "interlaced") > 0,
            Stereoscopic = (StereoMode)Enum.Parse(typeof(StereoMode), DatabaseUtility.Get(results, i, "stereoMode"), true),
            Format = DatabaseUtility.Get(results, i, "format"),
            Codec = (VideoCodec)Enum.Parse(typeof(VideoCodec), DatabaseUtility.Get(results, i, "codec"), true),
            CodecName = DatabaseUtility.Get(results, i, "codecName"),
            CodecProfile = DatabaseUtility.Get(results, i, "codecProfile"),
            Standard = (VideoStandard)Enum.Parse(typeof(VideoStandard), DatabaseUtility.Get(results, i, "videoStandard"), true),
            ColorSpace = (ColorSpace)Enum.Parse(typeof(ColorSpace), DatabaseUtility.Get(results, i, "colorSpace"), true),
            TransferCharacteristics = (TransferCharacteristic)Enum.Parse(typeof(TransferCharacteristic), DatabaseUtility.Get(results, i, "transferCharacteristics"), true),
            SubSampling = (ChromaSubSampling)Enum.Parse(typeof(ChromaSubSampling), DatabaseUtility.Get(results, i, "chromaSubSampling"), true),
            Duration = new TimeSpan(DatabaseUtility.GetAsInt64(results, i, "duration")),
            Hdr = (Hdr)Enum.Parse(typeof(Hdr), DatabaseUtility.Get(results, i, "hdr"), true),
            BitDepth = DatabaseUtility.GetAsInt(results, i, "bitDepth"),
          };

          this.VideoStreams.Add(s);
          insertTags(db, s.Tags, "videoStreams", DatabaseUtility.GetAsInt(results, i, "id"));

          //Unpack
          iStreamPack = DatabaseUtility.GetAsInt(results, i, "streamPack");
          if (iStreamPack > 0)
            foreach (MediaStream ms in copyMediaStream(s, iStreamPack))
              this.VideoStreams.Add((VideoStream)ms);
        }
      }
      #endregion

      #region AudioStreams
      strSQL = "SELECT * FROM audioStreams WHERE idFile=" + iDbId;
      results = db.Execute(strSQL);
      if (results != null && results.Rows.Count > 0)
      {
        for (int i = 0; i < results.Rows.Count; i++)
        {
          AudioStream s = new AudioStream()
          {
            Id = DatabaseUtility.GetAsInt(results, i, "streamId"),
            Name = DatabaseUtility.Get(results, i, "streamName"),
            StreamPosition = DatabaseUtility.GetAsInt(results, i, "streamPosition"),
            StreamNumber = DatabaseUtility.GetAsInt(results, i, "streamNumber"),
            Language = DatabaseUtility.Get(results, i, "language"),
            Lcid = DatabaseUtility.GetAsInt(results, i, "lcid"),
            Default = DatabaseUtility.GetAsInt(results, i, "flagDefault") > 0,
            Forced = DatabaseUtility.GetAsInt(results, i, "flagForced") > 0,
            StreamSize = DatabaseUtility.GetAsInt64(results, i, "streamSize"),
            Codec = (AudioCodec)Enum.Parse(typeof(AudioCodec), DatabaseUtility.Get(results, i, "codec"), true),
            CodecName = DatabaseUtility.Get(results, i, "codecName"),
            CodecDescription = DatabaseUtility.Get(results, i, "codecDescription"),
            Duration = new TimeSpan(DatabaseUtility.GetAsInt64(results, i, "duration")),
            Bitrate = (double)DatabaseUtility.GetAsDouble(results, i, "bitRate"),
            Channel = DatabaseUtility.GetAsInt(results, i, "channel"),
            SamplingRate = (double)DatabaseUtility.GetAsDouble(results, i, "samplingRate"),
            BitDepth = DatabaseUtility.GetAsInt(results, i, "bitDepth"),
            BitrateMode = (BitrateMode)Enum.Parse(typeof(BitrateMode), DatabaseUtility.Get(results, i, "bitRateMode"), true),
            Format = DatabaseUtility.Get(results, i, "format"),
          };

          this.AudioStreams.Add(s);
          insertTags(db, s.Tags, "audioStreams", DatabaseUtility.GetAsInt(results, i, "id"));

          //Unpack
          iStreamPack = DatabaseUtility.GetAsInt(results, i, "streamPack");
          if (iStreamPack > 0)
            foreach (MediaStream ms in copyMediaStream(s, iStreamPack))
              this.AudioStreams.Add((AudioStream)ms);
        }
      }
      #endregion

      #region SubtileStreams
      strSQL = "SELECT * FROM subtitleStreams WHERE idFile=" + iDbId;
      results = db.Execute(strSQL);
      if (results != null && results.Rows.Count > 0)
      {
        for (int i = 0; i < results.Rows.Count; i++)
        {
          SubtitleStream s = new SubtitleStream()
          {
            Id = DatabaseUtility.GetAsInt(results, i, "streamId"),
            Name = DatabaseUtility.Get(results, i, "streamName"),
            StreamPosition = DatabaseUtility.GetAsInt(results, i, "streamPosition"),
            StreamNumber = DatabaseUtility.GetAsInt(results, i, "streamNumber"),
            Language = DatabaseUtility.Get(results, i, "language"),
            Lcid = DatabaseUtility.GetAsInt(results, i, "lcid"),
            Default = DatabaseUtility.GetAsInt(results, i, "flagDefault") > 0,
            Forced = DatabaseUtility.GetAsInt(results, i, "flagForced") > 0,
            StreamSize = DatabaseUtility.GetAsInt64(results, i, "streamSize"),
            Codec = (SubtitleCodec)Enum.Parse(typeof(SubtitleCodec), DatabaseUtility.Get(results, i, "codec"), true),
            Format = DatabaseUtility.Get(results, i, "format"),
          };

          this.Subtitles.Add(s);

          //Unpack
          iStreamPack = DatabaseUtility.GetAsInt(results, i, "streamPack");
          if (iStreamPack > 0)
            foreach (MediaStream ms in copyMediaStream(s, iStreamPack))
              this.Subtitles.Add((SubtitleStream)ms);
        }
      }
      #endregion

      #region ChapterStreams
      strSQL = "SELECT * FROM chapterStreams WHERE idFile=" + iDbId;
      results = db.Execute(strSQL);
      if (results != null && results.Rows.Count > 0)
      {
        for (int i = 0; i < results.Rows.Count; i++)
        {
          ChapterStream s = new ChapterStream(
              (double)DatabaseUtility.GetAsDouble(results, i, "offset"),
              DatabaseUtility.Get(results, i, "description"))
          {
            Id = DatabaseUtility.GetAsInt(results, i, "streamId"),
            Name = DatabaseUtility.Get(results, i, "streamName"),
            StreamPosition = DatabaseUtility.GetAsInt(results, i, "streamPosition"),
            StreamNumber = DatabaseUtility.GetAsInt(results, i, "streamNumber"),
          };

          this.Chapters.Add(s);

          //Unpack
          iStreamPack = DatabaseUtility.GetAsInt(results, i, "streamPack");
          if (iStreamPack > 0)
            foreach (MediaStream ms in copyMediaStream(s, iStreamPack))
              this.Chapters.Add((ChapterStream)ms);
        }
      }
      #endregion

      #region MenuStreams
      strSQL = "SELECT * FROM menuStreams WHERE idFile=" + iDbId;
      results = db.Execute(strSQL);
      if (results != null && results.Rows.Count > 0)
      {
        for (int i = 0; i < results.Rows.Count; i++)
        {
          MenuStream menu = new MenuStream()
          {
            Id = DatabaseUtility.GetAsInt(results, i, "streamId"),
            Name = DatabaseUtility.Get(results, i, "streamName"),
            StreamPosition = DatabaseUtility.GetAsInt(results, i, "streamPosition"),
            StreamNumber = DatabaseUtility.GetAsInt(results, i, "streamNumber"),
            Duration = new TimeSpan(DatabaseUtility.GetAsInt64(results, i, "duration")),
          };
          int iIdMenu = DatabaseUtility.GetAsInt(results, i, "id");

          SQLiteResultSet chapters = db.Execute("SELECT * FROM chapters WHERE idStream=" + iIdMenu);
          if (chapters != null && chapters.Rows.Count > 0)
          {
            for (int iCh = 0; iCh < chapters.Rows.Count; iCh++)
            {
              menu.Chapters.Add(new Chapter()
              {
                Position = new TimeSpan(DatabaseUtility.GetAsInt64(chapters, i, "position")),
                Name = DatabaseUtility.Get(chapters, i, "name"),
              });
            }
          }

          this.MenuStreams.Add(menu);
        }
      }
      #endregion

      this.setProperty("BestVideoStream", VideoStreams.OrderByDescending(
          x => (long)x.Width * x.Height * x.BitDepth * (x.Stereoscopic == StereoMode.Mono ? 1L : 2L) * x.FrameRate * (x.Bitrate <= 1e-7 ? 1 : x.Bitrate))
        .FirstOrDefault());
      this.setProperty("VideoCodec", this.BestVideoStream?.CodecName ?? string.Empty);
      this.setProperty("VideoRate", (int?)this.BestVideoStream?.Bitrate ?? 0);
      this.setProperty("VideoResolution", this.BestVideoStream?.Resolution ?? string.Empty);
      this.setProperty("Width", this.BestVideoStream?.Width ?? 0);
      this.setProperty("Height", this.BestVideoStream?.Height ?? 0);
      this.setProperty("IsInterlaced", this.BestVideoStream?.Interlaced ?? false);
      this.setProperty("Framerate", this.BestVideoStream?.FrameRate ?? 0);
      this.setProperty("BestAudioStream", this.AudioStreams.OrderByDescending(x => (x.Channel * 10000000) + x.Bitrate).FirstOrDefault());
      this.setProperty("AudioCodec", this.BestAudioStream?.CodecName ?? string.Empty);
      this.setProperty("AudioRate", (int?)this.BestAudioStream?.Bitrate ?? 0);
      this.setProperty("AudioSampleRate", (int?)this.BestAudioStream?.SamplingRate ?? 0);
      this.setProperty("AudioChannels", this.BestAudioStream?.Channel ?? 0);
      this.setProperty("AudioChannelsFriendly", this.BestAudioStream?.AudioChannelsFriendly ?? string.Empty);

      Type t = typeof(MediaInfoWrapper);
      t.GetField("<HasExternalSubtitles>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this,
        t.GetMethod("CheckHasExternalSubtitles", BindingFlags.NonPublic | BindingFlags.Static).Invoke(this, new object[] { strMediaFullPath }));

      if (strMediaFullPath.EndsWith(".ifo", StringComparison.OrdinalIgnoreCase))
        this.setProperty("IsDvd", true);
      else if (strMediaFullPath.EndsWith(".bdmv", StringComparison.OrdinalIgnoreCase))
        t.GetField("<IsBluRay>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, true);

      this.setProperty("Success", true);
    }

    private void setProperty(string strName, object value)
    {
      typeof(MediaInfoWrapper).GetProperty(strName).SetValue(this, value, null);
    }

    private static IEnumerable<MediaStream> copyMediaStream(MediaStream ms, int iCount)
    {
      Type t = ms.GetType();
      PropertyInfo[] props = t.GetProperties().Where(p => p.CanWrite && p.Name != "Tags").ToArray();
      int iOffset = 1;
      while (iCount-- > 0)
      {
        object o = Activator.CreateInstance(t);

        //Copy all props
        for (int i = 0; i < props.Length; i++)
        {
          PropertyInfo pi = props[i];

          if (pi.Name == "StreamNumber" || pi.Name == "StreamPosition")
            pi.SetValue(o, (int)pi.GetValue(ms, null) + iOffset, null);
          else
            pi.SetValue(o, pi.GetValue(ms, null), null);
        }

        yield return (MediaStream)o;

        iOffset++;
      }
    }

    private static void insertTags(SQLiteClient db, BaseTags tags,  string strStreamTable, int iIdStream)
    {
      string strSQL = string.Format("SELECT * FROM tags WHERE streamTable='{0}' AND streamId={1}", strStreamTable, iIdStream);
      SQLiteResultSet results = db.Execute(strSQL);
      if (results != null && results.Rows.Count > 0)
      {
        //Prepare dictionaries
        IDictionary dictGeneral = (IDictionary)typeof(BaseTags).GetProperty("GeneralTags", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tags, null);
        Type tKeyGeneral = dictGeneral.GetType().GetInterface("IDictionary`2").GetGenericArguments()[0];
        IDictionary dictVideo = null;
        Type tKeyVideo = null;
        IDictionary dictAudio = null;
        Type tKeyAudio = null;
        if (tags is VideoTags)
        {
          dictVideo = (IDictionary)typeof(VideoTags).GetProperty("VideoDataTags", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tags, null);
          tKeyVideo = dictVideo.GetType().GetInterface("IDictionary`2").GetGenericArguments()[0];
        }
        else if (tags is AudioTags)
        {
          dictAudio = (IDictionary)typeof(AudioTags).GetProperty("AudioDataTags", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tags, null);
          tKeyAudio = dictAudio.GetType().GetInterface("IDictionary`2").GetGenericArguments()[0];
        }

        for (int i = 0; i < results.Rows.Count; i++)
        {
          string strTagId = DatabaseUtility.Get(results, i, "tagId");
          string strTagValue = DatabaseUtility.Get(results, i, "tagValue");
          int iIdx = strTagValue.IndexOf(':');

          if (strTagId == "CoverInfo")
          {
            //Deserialize CoverInfo
            IEnumerable<CoverInfo> covers = JsonConvert.DeserializeObject<IEnumerable<CoverInfo>>(strTagValue.Substring(iIdx + 1), new JsonSerializerSettings()
            {
              ContractResolver = new PrivateResolver(),
              ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });

            tags.Covers = covers;
          }
          else
          {
            object oValue = Convert.ChangeType(strTagValue.Substring(iIdx + 1), Type.GetType(strTagValue.Substring(0, iIdx)));

            if (strTagId.StartsWith("General_"))
              dictGeneral[Enum.Parse(tKeyGeneral, strTagId)] = oValue;
            else if (strTagId.StartsWith("Video_"))
              dictVideo[Enum.Parse(tKeyVideo, strTagId)] = oValue;
            else if (strTagId.StartsWith("Audio_"))
              dictAudio[Enum.Parse(tKeyAudio, strTagId)] = oValue;
          }
        }
      }
    }
  }
}
