using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class AnalogManagement
  {
    #region SoftwareEncoders

    public static IList<SoftwareEncoder> GetSofwareEncodersVideo()
    {
      using (IRepository<Model> channelRepository = new GenericRepository<Model>())
      {
        List<SoftwareEncoder> sofwareEncodersVideo = channelRepository.GetQuery<SoftwareEncoder>(s => s.type == 0).OrderBy(s => s.priority).ToList();
        return sofwareEncodersVideo;
      }    
    }

    public static IList<SoftwareEncoder> GetSofwareEncodersAudio()
    {
      using (IRepository<Model> channelRepository = new GenericRepository<Model>())
      {
        List<SoftwareEncoder> sofwareEncodersAudio = channelRepository.GetQuery<SoftwareEncoder>(s => s.type == 1).OrderBy(s => s.priority).ToList();
        return sofwareEncodersAudio;
      }    
    }

    #endregion
  }
}
