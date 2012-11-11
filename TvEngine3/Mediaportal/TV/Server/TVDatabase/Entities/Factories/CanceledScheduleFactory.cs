using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Factories
{
  public static class CanceledScheduleFactory
  {
    public static CanceledSchedule Clone(CanceledSchedule source)
    {
      return CloneHelper.DeepCopy<CanceledSchedule>(source);   
    }

    public static CanceledSchedule CreateCanceledSchedule(int idForScheduleToCancel, int idChannel, DateTime cancelDateTime)
    {
      var canceledSchedule = new CanceledSchedule
                       {
                         IdChannel = idChannel,
                         IdSchedule = idForScheduleToCancel,
                         CancelDateTime = cancelDateTime
                       };


      return canceledSchedule;
    }

   
     
      
  }
}
