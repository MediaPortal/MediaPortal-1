using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class SoftwareEncoderManagement
  {
    public static IList<VideoEncoder> ListAllSofwareEncodersVideo()
    {
      using (IRepository<Model> repository = new GenericRepository<Model>())
      {
        return repository.GetQuery<VideoEncoder>().OrderBy(e => e.Priority).ToList();
      }
    }

    public static IList<AudioEncoder> ListAllSofwareEncodersAudio()
    {
      using (IRepository<Model> repository = new GenericRepository<Model>())
      {
        return repository.GetQuery<AudioEncoder>().OrderBy(e => e.Priority).ToList();
      }
    }
  }
}