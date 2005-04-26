using MediaPortal.GUI.Library;
using SQLite.NET;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for appFilesEdit.
  /// </summary>
  public class appGrouper: AppItem
  {

    public appGrouper(SQLiteClient initSqlDB): base(initSqlDB){}

    public override bool SubItemsAllowed()
    {
      return true;
    }

    public override bool FileEditorAllowed()
    {
      return true; // files are in fact FILTERITEMS => links to files
    }

    override public bool FileAddAllowed()
    {
      return false; // no file adding allowed! links can only be added indirectly from another filelist
    }

    override public bool FilesCanBeFavourites()
    {
      return false; // links cannot be links again..... :)
    }

    public override void LaunchFile(FileItem curFile, bool MPGUIMode)
    {
      if (curFile is FilelinkItem)
      {
        base.LaunchFilelink((FilelinkItem)curFile, MPGUIMode);
      }
      else
      {
        Log.Write("myPrograms: appGrouper: internal error: Filelinkitem expected in LaunchFile");
      }
    }


  }

}
