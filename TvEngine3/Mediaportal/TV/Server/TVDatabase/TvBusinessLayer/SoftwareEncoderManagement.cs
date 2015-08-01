using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class SoftwareEncoderManagement
  {
    public static IList<SoftwareEncoder> ListAllSofwareEncodersVideo()
    {
      using (IRepository<Model> repository = new GenericRepository<Model>())
      {
        return repository.GetQuery<SoftwareEncoder>(e => e.Type != (int)SoftwareEncoderType.Audio).OrderBy(e => e.Priority).ToList();
      }
    }

    public static IList<SoftwareEncoder> ListAllSofwareEncodersAudio()
    {
      using (IRepository<Model> repository = new GenericRepository<Model>())
      {
        return repository.GetQuery<SoftwareEncoder>(e => e.Type == (int)SoftwareEncoderType.Audio || e.Type == (int)SoftwareEncoderType.Automatic).OrderBy(e => e.Priority).ToList();
      }
    }
  }
}