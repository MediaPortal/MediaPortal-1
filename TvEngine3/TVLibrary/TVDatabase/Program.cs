using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using IdeaBlade.Persistence;
using IdeaBlade.Util;

namespace TvDatabase {
  [Serializable]
  public sealed class Program : ProgramDataRow {
  
  #region Constructors -- DO NOT MODIFY
    // Do not create constructors for this class
    // Developers cannot create instances of this class with the "new" keyword
    // You should write a static Create method instead; see the "Suggested Customization" region
    // Please review the documentation to learn about entity creation and inheritance

    // This private constructor prevents accidental instance creation via 'new'
    private Program() : this(null) {}

    // Typed DataSet constructor (needed by framework) - DO NOT REMOVE
    public Program(DataRowBuilder pRowBuilder) 
      : base(pRowBuilder) {
    }

  #endregion
    
  #region Suggested Customizations
  
//    // Use this factory method template to create new instances of this class
//    public static Program Create(PersistenceManager pManager,
//      ... your creation parameters here ...) { 
//
//      Program aProgram = pManager.CreateEntity<Program>();
//
//      // if this object type requires a unique id and you have implemented
//      // the IIdGenerator interface implement the following line
//      pManager.GenerateId(aProgram, // add id column here //);
//
//      // Add custom code here
//
//      aProgram.AddToManager();
//      return aProgram;
//    }

//    // Implement this method if you want your object to sort in a specific order
//    public override int CompareTo(Object pObject) {
//    }

//    // Implement this method to customize the null object version of this class
//    protected override void UpdateNullEntity() {
//    }

  #endregion


    public static Program Create()
    {
      Program program = (Program)DatabaseManager.Instance.CreateEntity(typeof(Program));
      DatabaseManager.Instance.GenerateId(program, Program.IdProgramEntityColumn);
      program.StartTime = new DateTime(2000, 1, 1, 0, 0, 0);
      program.EndTime = new DateTime(2000, 1, 1, 0, 0, 0);
      program.Title = "";
      program.Description = "";
      program.Genre = "";
      DatabaseManager.Instance.AddEntity(program);
      return program;
    }

    public static Program New()
    {
      Program program = (Program)DatabaseManager.Instance.CreateEntity(typeof(Program));
      DatabaseManager.Instance.GenerateId(program, Program.IdProgramEntityColumn);
      program.StartTime = new DateTime(2000, 1, 1, 0, 0, 0);
      program.EndTime = new DateTime(2000, 1, 1, 0, 0, 0);
      program.Title = "";
      program.Description = "";
      program.Genre = "";
      return program;
    }
    /// <summary>
    /// Checks if the program is running between the specified start and end time/dates
    /// </summary>
    /// <param name="tStartTime">Start date and time</param>
    /// <param name="tEndTime">End date and time</param>
    /// <returns>true if program is running between tStartTime-tEndTime</returns>
    public bool RunningAt(DateTime tStartTime, DateTime tEndTime)
    {
      DateTime dtStart = StartTime;
      DateTime dtEnd = EndTime;

      bool bRunningAt = false;
      if (dtEnd >= tStartTime && dtEnd <= tEndTime) bRunningAt = true;
      if (dtStart >= tStartTime && dtStart <= tEndTime) bRunningAt = true;
      if (dtStart <= tStartTime && dtEnd >= tEndTime) bRunningAt = true;
      return bRunningAt;
    }

    /// <summary>
    /// Checks if the program is running at the specified date/time
    /// </summary>
    /// <param name="tCurTime">date and time</param>
    /// <returns>true if program is running at tCurTime</returns>
    public bool IsRunningAt(DateTime tCurTime)
    {
      bool bRunningAt = false;
      if (tCurTime >= StartTime && tCurTime <= EndTime) bRunningAt = true;
      return bRunningAt;
    }
    public void DeleteAll()
    {
      while (Favorites.Count > 0)
      {
        Favorites[0].Delete();
      }
      this.Delete();
    }
  }
  
}
