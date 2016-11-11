using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class AnalogTunerSettingsManagement
  {
    public static AnalogTunerSettings GetAnalogTunerSettings(int idAnalogTunerSettings)
    {
      using (IAnalogTunerSettingsRepository analogTunerSettingsRepository = new AnalogTunerSettingsRepository())
      {
        var query = analogTunerSettingsRepository.GetQuery<AnalogTunerSettings>(s => s.IdAnalogTunerSettings == idAnalogTunerSettings);
        return analogTunerSettingsRepository.IncludeAllRelations(query).FirstOrDefault();
      }
    }

    public static AnalogTunerSettings SaveAnalogTunerSettings(AnalogTunerSettings settings)
    {
      using (IAnalogTunerSettingsRepository analogTunerSettingsRepository = new AnalogTunerSettingsRepository())
      {
        analogTunerSettingsRepository.AttachEntityIfChangeTrackingDisabled(analogTunerSettingsRepository.ObjectContext.AnalogTunerSettings, settings);
        analogTunerSettingsRepository.ApplyChanges(analogTunerSettingsRepository.ObjectContext.AnalogTunerSettings, settings);
        analogTunerSettingsRepository.UnitOfWork.SaveChanges();
        settings.AcceptChanges();
        return settings;
      }
    }
  }
}