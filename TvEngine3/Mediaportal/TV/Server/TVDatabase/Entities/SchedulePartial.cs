using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Schedule 
  {
    public static DateTime MinSchedule = new DateTime(2000, 1, 1);
    public static readonly int HighestPriority = Int32.MaxValue;
  }
}
