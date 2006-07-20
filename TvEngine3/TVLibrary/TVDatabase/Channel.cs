using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;


using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
namespace TvDatabase
{
  [Serializable]
  public sealed class Channel : ChannelDataRow
  {

    #region Constructors -- DO NOT MODIFY
    // Do not create constructors for this class
    // Developers cannot create instances of this class with the "new" keyword
    // You should write a static Create method instead; see the "Suggested Customization" region
    // Please review the documentation to learn about entity creation and inheritance

    // This private constructor prevents accidental instance creation via 'new'
    private Channel() : this(null) { }

    // Typed DataSet constructor (needed by framework) - DO NOT REMOVE
    public Channel(DataRowBuilder pRowBuilder)
      : base(pRowBuilder)
    {
    }

    #endregion

    #region Suggested Customizations

    //    // Use this factory method template to create new instances of this class
    //    public static Channel Create(PersistenceManager pManager,
    //      ... your creation parameters here ...) { 
    //
    //      Channel aChannel = pManager.CreateEntity<Channel>();
    //
    //      // if this object type requires a unique id and you have implemented
    //      // the IIdGenerator interface implement the following line
    //      pManager.GenerateId(aChannel, // add id column here //);
    //
    //      // Add custom code here
    //
    //      aChannel.AddToManager();
    //      return aChannel;
    //    }

    //    // Implement this method if you want your object to sort in a specific order
    //    public override int CompareTo(Object pObject) {
    //    }

    //    // Implement this method to customize the null object version of this class
    //    protected override void UpdateNullEntity() {
    //    }

    #endregion

    // Add additional logic to your business object here...
    Program _currentProgram = null;
    Program _nextProgram = null;

    // Add additional logic to your business object here...
    public static Channel Create()
    {
      Channel channel = (Channel)DatabaseManager.Instance.CreateEntity(typeof(Channel));
      channel.TimesWatched = 0;
      channel.TotalTimeWatched = new DateTime(2000, 1, 1, 0, 0, 0);
      channel.SortOrder = -1;
      channel.LastGrabTime = new DateTime(2000, 1, 1, 0, 0, 0);
      channel.GrabEpg = true;
      channel.VisibleInGuide = true;
      DatabaseManager.Instance.GenerateId(channel, Channel.IdChannelEntityColumn);
      DatabaseManager.Instance.AddEntity(channel);
      return channel;
    }
    public static Channel New()
    {
      Channel channel = (Channel)DatabaseManager.Instance.CreateEntity(typeof(Channel));
      channel.TimesWatched = 0;
      channel.TotalTimeWatched = new DateTime(2000, 1, 1, 0, 0, 0);
      channel.SortOrder = -1;
      channel.LastGrabTime = new DateTime(2000, 1, 1, 0, 0, 0);
      channel.GrabEpg = true;
      channel.VisibleInGuide = true;
      return channel;
    }

    public void DeleteAll()
    {
      //map Channel<->groupmap<->channelgroup
      while (GroupMaps.Count>0)
      {
        GroupMaps[0].Delete();
      }
      while (ChannelMaps.Count > 0)
      {
        ChannelMaps[0].Delete();
      }
      while (TuningDetails.Count > 0)
      {
        TuningDetails[0].Delete();
      }
      while (Recordings.Count > 0)
      {
        Recordings[0].Delete();
      }
      while (Schedules.Count > 0)
      {
        Schedules[0].DeleteAll();
      }
      while (Programs.Count > 0)
      {
        Programs[0].DeleteAll();
      }
      this.Delete();
    }

    public Program NextProgram
    {
      get
      {
        UpdateNowAndNext();
        return _nextProgram;
      }
    }
    public Program CurrentProgram
    {
      get
      {
        UpdateNowAndNext();
        return _currentProgram;
      }
    }

    public Program GetProgramAt(DateTime date)
    {
      DateTime startTime = DateTime.Now;
      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program),
        String.Format(
            "select top 1 * from program where program.idChannel={0} and  '{1}-{2}-{3} {4}' < endtime order by starttime asc"
        , IdChannel, date.Year, date.Month, date.Day, date.ToLongTimeString()));

      EntityList<Program> programs = DatabaseManager.Instance.GetEntities<Program>(query);
      if (programs.Count == 0)
      {
        return null;
      }
      return programs[0];
    }
    void UpdateNowAndNext()
    {
      if (_currentProgram != null)
      {
        if (DateTime.Now >= _currentProgram.StartTime && DateTime.Now <= _currentProgram.EndTime)
        {
          return;
        }
      }

      _currentProgram = null;
      _nextProgram = null;
      Program program = DatabaseManager.Instance.GetNullEntity<Program>();

      DateTime startTime = DateTime.Now;
      PassthruRdbQuery query = new PassthruRdbQuery(typeof(Program),
        String.Format(
            "select top 2 * from program where program.idChannel={0} and endtime >= GetDate()  order by starttime asc"
        , IdChannel));

      EntityList<Program> programs = DatabaseManager.Instance.GetEntities<Program>(query);
      if (programs.Count == 0)
      {
        return;
      }
      _currentProgram = programs[0];
      if (programs.Count == 2)
      {
        _nextProgram = programs[1];
      }
    }


  }

}
