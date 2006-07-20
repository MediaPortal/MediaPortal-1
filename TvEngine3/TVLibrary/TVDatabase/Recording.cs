using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

using IdeaBlade.Persistence;
using IdeaBlade.Util;

namespace TvDatabase {
  [Serializable]
  public sealed class Recording : RecordingDataRow {
  
  #region Constructors -- DO NOT MODIFY
    // Do not create constructors for this class
    // Developers cannot create instances of this class with the "new" keyword
    // You should write a static Create method instead; see the "Suggested Customization" region
    // Please review the documentation to learn about entity creation and inheritance

    // This private constructor prevents accidental instance creation via 'new'
    private Recording() : this(null) {}

    // Typed DataSet constructor (needed by framework) - DO NOT REMOVE
    public Recording(DataRowBuilder pRowBuilder) 
      : base(pRowBuilder) {
    }

  #endregion
    
  #region Suggested Customizations
  
//    // Use this factory method template to create new instances of this class
//    public static Recording Create(PersistenceManager pManager,
//      ... your creation parameters here ...) { 
//
//      Recording aRecording = pManager.CreateEntity<Recording>();
//
//      // if this object type requires a unique id and you have implemented
//      // the IIdGenerator interface implement the following line
//      pManager.GenerateId(aRecording, // add id column here //);
//
//      // Add custom code here
//
//      aRecording.AddToManager();
//      return aRecording;
//    }

//    // Implement this method if you want your object to sort in a specific order
//    public override int CompareTo(Object pObject) {
//    }

//    // Implement this method to customize the null object version of this class
//    protected override void UpdateNullEntity() {
//    }

  #endregion
    
    // Add additional logic to your business object here...

    public static Recording Create()
    {
      Recording recording = (Recording)DatabaseManager.Instance.CreateEntity(typeof(Recording));
      recording.StartTime = Schedule.MinSchedule;
      recording.EndTime = Schedule.MinSchedule;
      recording.FileName = "";
      recording.Title = "";
      recording.Description = "";
      recording.Genre = "";
      recording.KeepUntilDate = Schedule.MinSchedule;
      recording.KeepUntil = (int)KeepMethodType.UntilSpaceNeeded;
      recording.TimesWatched = 0;
      DatabaseManager.Instance.GenerateId(recording, Recording.IdRecordingEntityColumn);

      DatabaseManager.Instance.AddEntity(recording);
      return recording;
    }

    public static Recording New()
    {
      Recording recording = (Recording)DatabaseManager.Instance.CreateEntity(typeof(Recording));
      recording.StartTime = Schedule.MinSchedule;
      recording.EndTime = Schedule.MinSchedule;
      recording.FileName = "";
      recording.Title = "";
      recording.Description = "";
      recording.Genre = "";
      recording.KeepUntilDate = Schedule.MinSchedule;
      recording.KeepUntil = (int)KeepMethodType.UntilSpaceNeeded;
      recording.TimesWatched = 0;
      
      return recording;
    }
    public bool ShouldBeDeleted
    {
      get
      {
        if (KeepUntil != (int)KeepMethodType.TillDate) return false;
        if (KeepUntilDate.Date > DateTime.Now.Date) return false;
        return true;
      }
    }
  }
  
}
