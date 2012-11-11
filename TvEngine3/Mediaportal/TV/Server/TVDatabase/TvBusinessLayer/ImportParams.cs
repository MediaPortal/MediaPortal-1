using System.Threading;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public class ImportParams
  {
    public ProgramList ProgramList;
    public DeleteBeforeImportOption ProgamsToDelete;
    public string ConnectString;
    public ThreadPriority Priority;
    public int SleepTime;
  } ;
}
