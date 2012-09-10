using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class CanceledScheduleService : ICanceledScheduleService
  {
    public CanceledSchedule SaveCanceledSchedule(CanceledSchedule canceledSchedule)
    {
      return CanceledScheduleManagement.SaveCanceledSchedule(canceledSchedule);
    }
  }  
}
