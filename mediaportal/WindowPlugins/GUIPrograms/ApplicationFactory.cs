using Programs.Utils;
using SQLite.NET;
using WindowPlugins.GUIPrograms;

namespace ProgramsDatabase
{
  /// <summary>
  /// Factory object that creates the matchin AppItem descendant class
  /// depending on the sourceType parameter
  /// Descendant classes differ in LOADING and REFRESHING filelists
  /// </summary>
  public class ApplicationFactory
  {
    static public ApplicationFactory AppFactory = new ApplicationFactory();

    // singleton. Dont allow any instance of this class
    private ApplicationFactory(){}

    static ApplicationFactory()
    {
      // nothing to create......
    }

    public AppItem GetAppItem(SQLiteClient sqlDB, myProgSourceType sourceType)
    {
      AppItem res = null;
      switch (sourceType)
      {
        case myProgSourceType.DIRBROWSE:
          res = new appItemDirBrowse(sqlDB);
          break;
        case myProgSourceType.DIRCACHE:
          res = new appItemDirCache(sqlDB);
          break;
        case myProgSourceType.MYFILEINI:
          res = new appItemMyFileINI(sqlDB);
          break;
        case myProgSourceType.MYFILEMEEDIO:
          res = new appItemMyFileMLF(sqlDB);
          break;
        case myProgSourceType.MAMEDIRECT:
          res = new appItemMameDirect(sqlDB);
          break;
        case myProgSourceType.FILELAUNCHER:
          res = new appFilesEdit(sqlDB);
          break;
        case myProgSourceType.GROUPER:
          res = new appGrouper(sqlDB);
          break;
      }
      return res;
    }

  }
}
