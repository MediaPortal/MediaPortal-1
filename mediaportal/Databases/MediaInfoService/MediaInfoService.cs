using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Services;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;
using MediaInfo;

namespace MediaPortal.MediaInfoService.Database
{
  public class MediaInfoService : IMediaInfoService
  {
    private MediaInfoDatabaseSqlLite _SqlLite;

    public MediaInfoService()
    {
      this._SqlLite = new MediaInfoDatabaseSqlLite();

      using (Settings xmlreader = new MPSettings())
      {
        this._SqlLite.EnabledCachingForBluray = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForBluray", true);
        this._SqlLite.EnabledCachingForDVD = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForDVD", true);
        this._SqlLite.EnabledCachingForVideo = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForVideo", true);
        this._SqlLite.EnabledCachingForAudio = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForAudio", false);
        this._SqlLite.EnabledCachingForPicture = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForPicture", false);
        this._SqlLite.EnabledCachingForImage = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForImage", false);
        this._SqlLite.EnabledCachingForAudioCD = xmlreader.GetValueAsBool("MediaInfo", "EnableCachingForAudioCD", false);
        this._SqlLite.RecordLifeTime = xmlreader.GetValueAsInt("MediaInfo", "RecordLifeTime", 0);
        

        Log.Debug("[MediaInfoService][ctor] Enabled for Bluray: " + this._SqlLite.EnabledCachingForBluray);
        Log.Debug("[MediaInfoService][ctor] Enabled for DVD: " + this._SqlLite.EnabledCachingForDVD);
        Log.Debug("[MediaInfoService][ctor] Enabled for Video: " + this._SqlLite.EnabledCachingForVideo);
        Log.Debug("[MediaInfoService][ctor] Enabled for Audio: " + this._SqlLite.EnabledCachingForAudio);
        Log.Debug("[MediaInfoService][ctor] Enabled for Picture: " + this._SqlLite.EnabledCachingForPicture);
        Log.Debug("[MediaInfoService][ctor] Enabled for AudioCD: " + this._SqlLite.EnabledCachingForAudioCD);
        Log.Debug("[MediaInfoService][ctor] Record Life Time: " + this._SqlLite.RecordLifeTime);
      }
    }

    public bool EnabledCachingForBluray
    {
      get { return this._SqlLite.EnabledCachingForBluray; }
      set { this._SqlLite.EnabledCachingForBluray = value; }
    }

    public bool EnabledCachingForDVD
    {
      get { return this._SqlLite.EnabledCachingForDVD; }
      set { this._SqlLite.EnabledCachingForDVD = value; }
    }

    public bool EnabledCachingForVideo
    {
      get { return this._SqlLite.EnabledCachingForVideo; }
      set { this._SqlLite.EnabledCachingForVideo = value; }
    }

    public bool EnabledCachingForAudio
    {
      get { return this._SqlLite.EnabledCachingForAudio; }
      set { this._SqlLite.EnabledCachingForAudio = value; }
    }

    public bool EnabledCachingForPicture
    {
      get { return this._SqlLite.EnabledCachingForPicture; }
      set { this._SqlLite.EnabledCachingForPicture = value; }
    }

    public bool EnabledCachingForImage
    {
      get { return this._SqlLite.EnabledCachingForImage; }
      set { this._SqlLite.EnabledCachingForImage = value; }
    }

    public bool EnabledCachingForAudioCD
    {
      get { return this._SqlLite.EnabledCachingForAudioCD; }
      set { this._SqlLite.EnabledCachingForAudioCD = value; }
    }


    public int RecordLifeTime
    {
      get { return this._SqlLite.RecordLifeTime; }
      set { this._SqlLite.RecordLifeTime = value; }
    }


    public MediaInfoWrapper Get(string strMediaFullPath)
    {
      return this._SqlLite.Get(strMediaFullPath);
    }

    public void Clear()
    {
      this._SqlLite.Clear();
    }
  }
}
