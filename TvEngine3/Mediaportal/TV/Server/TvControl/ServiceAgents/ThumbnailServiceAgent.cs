using Mediaportal.TV.Server.TVControl.Interfaces.Services;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class ThumbnailServiceAgent : ServiceAgent<IThumbnailService>, IThumbnailService 
  {
    public ThumbnailServiceAgent(string hostname)
      : base(hostname)
    {
    }

    public byte[] GetThumbnailForRecording(int idRecording)
    {
      return _channel.GetThumbnailForRecording(idRecording);
    }

    public void DeleteAllThumbnails()
    {
      _channel.DeleteAllThumbnails();
    }
  }
}