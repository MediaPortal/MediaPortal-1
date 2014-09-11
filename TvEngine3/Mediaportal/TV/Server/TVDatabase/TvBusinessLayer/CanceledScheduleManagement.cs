using System.Collections.Generic;
using System;
using System.Data.Objects;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

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

    public static void DeleteAllCancelledSeries()
	  {
	    DateTime date = DateTime.Now.AddDays(-5);

      IList<CanceledSchedule> CanceledScheduleList = ListAllCanceledSchedules();
	    using (IScheduleRepository scheduleRepository = new ScheduleRepository(true))
      {
        foreach (CanceledSchedule cs in CanceledScheduleList)
	      {
	        if (cs.CancelDateTime < date)
	        {
            Log.Debug("DeleteAllCancelledSeries: Removing {0}", cs.CancelDateTime);
            scheduleRepository.Delete(cs);
	        }
	      }
      }
	  }
  }
}
