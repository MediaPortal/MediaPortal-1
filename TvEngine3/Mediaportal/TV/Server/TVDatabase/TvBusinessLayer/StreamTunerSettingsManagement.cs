using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class StreamTunerSettingsManagement
  {
    public static StreamTunerSettings GetStreamTunerSettings(int idStreamTunerSettings)
    {
      using (IRepository<Model> repository = new GenericRepository<Model>())
      {
        return repository.GetQuery<StreamTunerSettings>(s => s.IdStreamTunerSettings == idStreamTunerSettings).FirstOrDefault();
      }
    }

    public static StreamTunerSettings SaveStreamTunerSettings(StreamTunerSettings settings)
    {
      using (IRepository<Model> repository = new GenericRepository<Model>())
      {
        repository.AttachEntityIfChangeTrackingDisabled(repository.ObjectContext.StreamTunerSettings, settings);
        repository.ApplyChanges(repository.ObjectContext.StreamTunerSettings, settings);
        repository.UnitOfWork.SaveChanges();
        settings.AcceptChanges();
        return settings;
      }
    }
  }
}