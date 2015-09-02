using System.Threading;
using Mediaportal.TV.Server.Common.Types.Enum;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public class ImportParams
  {
    public ProgramList ProgramList;
    public EpgDeleteBeforeImportOption ProgamsToDelete;
    public ThreadPriority Priority;
    public int SleepTime;
  }
}