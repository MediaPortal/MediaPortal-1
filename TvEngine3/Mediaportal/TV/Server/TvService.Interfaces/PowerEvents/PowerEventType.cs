namespace Mediaportal.TV.Server.TVService.Interfaces.PowerEvents
{
  public enum PowerEventType
  {
    QuerySuspend,
    QueryStandBy,
    QuerySuspendFailed,
    QueryStandByFailed,
    Suspend,
    StandBy,
    ResumeCritical,
    ResumeSuspend,
    ResumeStandBy,
    ResumeAutomatic
  }
}