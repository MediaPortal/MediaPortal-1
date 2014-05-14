using System;
using System.IO;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class ThumbnailService : IThumbnailService
  {
    public byte[] GetThumbnailForRecording(int idRecording)
    {
      // TODO is the implementation allowed to be here?
      try
      {
        Recording r = ServiceManager.Instance.RecordingService.GetRecording(idRecording);
        string fileAndPath = string.Empty;//PathManager.GetDataPath

        if (!File.Exists(fileAndPath))
        {
          return new byte[0];
        }
        return File.ReadAllBytes(fileAndPath);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "thumbnail service: failed to read thumbnail for recording {0}", idRecording);
      }
      return new byte[0];
    }

    public void DeleteAllThumbnails()
    {
    }
  }
}