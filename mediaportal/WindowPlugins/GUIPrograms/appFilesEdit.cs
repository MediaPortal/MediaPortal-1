using SQLite.NET;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for appFilesEdit.
  /// </summary>
  public class appFilesEdit: AppItem
  {
    public appFilesEdit(SQLiteClient initSqlDB): base(initSqlDB)
    {
      // some nice defaults...
      UseShellExecute = true;
      UseQuotes = true;
      Startupdir = "%FILEDIR%";
    }

  }

}
