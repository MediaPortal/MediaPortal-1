using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class TunerPropertyManagement
  {
    public static IList<TunerProperty> ListAllTunerPropertiesByTuner(int idTuner)
    {
      using (IRepository<Model> repository = new GenericRepository<Model>())
      {
        return repository.GetQuery<TunerProperty>(p => p.IdTuner == idTuner).ToList();
      }
    }

    public static IList<TunerProperty> SaveTunerProperties(IEnumerable<TunerProperty> properties)
    {
      using (IRepository<Model> repository = new GenericRepository<Model>())
      {
        repository.AttachEntityIfChangeTrackingDisabled(repository.ObjectContext.TunerProperties, properties);
        repository.ApplyChanges(repository.ObjectContext.TunerProperties, properties);
        repository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //tunerRepository.ObjectContext.AcceptAllChanges();
        foreach (TunerProperty property in properties)
        {
          property.AcceptChanges();
        }
        return properties.ToList();
      }
    }
  }
}