using System;
using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Channel = Mediaportal.TV.Server.TVDatabase.Entities.Channel;

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
        canceledScheduleRepository.UnitOfWork.SaveChanges();        
        canceledSchedule.AcceptChanges();
      }
      
      Schedule schedule = ScheduleManagement.GetSchedule(canceledSchedule.IdSchedule);
      ProgramManagement.SynchProgramStates(new ScheduleBLL(schedule));
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
