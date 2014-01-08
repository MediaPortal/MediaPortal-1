using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class CanceledScheduleManagement
  {
    public static CanceledSchedule SaveCanceledSchedule(CanceledSchedule canceledSchedule)
    {
      using (var canceledScheduleRepository = new GenericRepository<Model>())
      {
        canceledScheduleRepository.AttachEntityIfChangeTrackingDisabled(canceledScheduleRepository.ObjectContext.CanceledSchedules, canceledSchedule);
        canceledScheduleRepository.ApplyChanges(canceledScheduleRepository.ObjectContext.CanceledSchedules, canceledSchedule);
        canceledScheduleRepository.UnitOfWork.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
      }

      ProgramManagement.SynchProgramStates(canceledSchedule.IdSchedule);
      return canceledSchedule;
    }

    public static IList<CanceledSchedule> ListAllCanceledSchedules()
    {
      using (var canceledScheduleRepository = new GenericRepository<Model>())
      {
        return canceledScheduleRepository.GetAll<CanceledSchedule>().ToList();
      }
    }
  }
}
