using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Services
{
  public class CanceledScheduleService : ICanceledScheduleService
  {
    public CanceledSchedule SaveCanceledSchedule(CanceledSchedule canceledSchedule)
    {
      return CanceledScheduleManagement.SaveCanceledSchedule(canceledSchedule);
    }
  }  
}
