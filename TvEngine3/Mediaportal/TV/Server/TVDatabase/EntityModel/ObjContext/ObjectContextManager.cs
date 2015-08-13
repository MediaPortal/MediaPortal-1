using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Linq;
using System.Reflection;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
  public static class ObjectContextManager
  {
    public enum SupportedDatabaseType
    {
      None,
      MySQL,
      MSSQL,
      MSSQLCE,
      SQLite
    }

    #region Static fields

    public delegate DbConnection CreateConnectionDelegate();
    private static CreateConnectionDelegate _externalDbCreator;

    private static readonly object _syncObj = new object();
    private static MetadataWorkspace _metadataWorkspace = null;
    private static bool _initialized = false;
    private static SupportedDatabaseType _databaseType;

    #endregion

    #region Global initalization

    /// <summary>
    /// Sets an external <see cref="CreateConnectionDelegate"/> that returns a <see cref="DbConnection"/>. This can be used
    /// by hosting environments to supply their own database to be used for EF.
    /// </summary>
    /// <param name="creatorDelegate">Creator</param>
    public static void SetDbConnectionCreator(CreateConnectionDelegate creatorDelegate)
    {
      _externalDbCreator = creatorDelegate;
    }

    /// <summary>
    /// Creates the model, checks for existing database and creates it, if not present.
    /// </summary>
    private static void Initialize()
    {
      if (_initialized)
        return;
      try
      {
        var model = GetModel();
        _databaseType = DetectDatabase(model);
        if (CheckCreateDatabase(model))
        {
          Log.Info("DataBase does not exist. Creating database...");
          CreateIndexes(model);
          DropConstraints(model);
          SetupStaticValues(model);
        }
        _initialized = true;
      }
      catch (Exception ex)
      {
        Log.Error(ex, "ObjectContextManager : error opening database. Is the SQL engine running ?");
        throw;
      }
    }

    /// <summary>
    /// Checks if the database exists and creates it if required.
    /// </summary>
    /// <param name="model">Model</param>
    /// <returns><c>true</c> if database was created</returns>
    private static bool CheckCreateDatabase(Model model)
    {
      // This is a special workaround for System.Data.SQLite provider, it does not support creation of database.
      if (_databaseType == SupportedDatabaseType.SQLite)
      {
        // We supply a database with all structures, but without data. So we check if the database is already filled using a table 
        // that contains static values.
        int cnt = model.ExecuteStoreQuery<int>("SELECT COUNT(*) FROM LnbTypes").First();
        // If we get a count the table exists but is empty. So we have the same situation as after model.CreateDatabase();
        // If table does not exist, we will get an exception. Handling it would not help here, because we cannot create the tables using EF.
        return cnt == 0;
      }

      if (!model.DatabaseExists())
      {
        model.CreateDatabase();
        return true;
      }

      return false;
    }

    private static SupportedDatabaseType DetectDatabase(Model model)
    {
      return DetectDatabase(((EntityConnection)model.Connection).StoreConnection);
    }

    private static SupportedDatabaseType DetectDatabase(DbConnection connection)
    {
      string conType = connection.GetType().ToString();
      if (conType.Contains("MySqlClient"))
        return SupportedDatabaseType.MySQL;
      if (conType.Contains("SQLite"))
        return SupportedDatabaseType.SQLite;
      if (conType.Contains("SqlServerCe"))
        return SupportedDatabaseType.MSSQLCE;
      if (conType.Contains("SqlClient"))
        return SupportedDatabaseType.MSSQL;

      return SupportedDatabaseType.None;
    }

    private static void CreateIndexes(Model model)
    {
      TryCreateIndex(model, "CanceledSchedules", "IdSchedule");
      TryCreateIndex(model, "Channels", "MediaType");
      TryCreateIndex(model, "ChannelMaps", "IdTuner");
      TryCreateIndex(model, "ChannelMaps", "IdChannel");
      TryCreateIndex(model, "ChannelLinkageMaps", "IdLinkedChannel");
      TryCreateIndex(model, "ChannelLinkageMaps", "IdPortalChannel");
      TryCreateIndex(model, "Conflicts", "IdTuner");
      TryCreateIndex(model, "Conflicts", "IdChannel");
      TryCreateIndex(model, "Conflicts", "IdSchedule");
      TryCreateIndex(model, "Conflicts", "IdConflictingSchedule");
      TryCreateIndex(model, "DiseqcMotors", "IdTuner");
      TryCreateIndex(model, "DiseqcMotors", "IdSatellite");
      TryCreateIndex(model, "GroupMaps", "IdGroup");
      TryCreateIndex(model, "GroupMaps", "IdChannel");
      TryCreateIndex(model, "Histories", "IdChannel");
      TryCreateIndex(model, "Histories", "IdProgramCategory");
      TryCreateIndex(model, "Programs", "IdChannel");
      TryCreateIndex(model, "Programs", "IdProgramCategory");
      TryCreateIndex(model, "Programs", new List<string> { "StartTime", "EndTime" });
      TryCreateIndex(model, "ProgramCredits", "IdProgram");
      TryCreateIndex(model, "Recordings", "IdChannel");
      TryCreateIndex(model, "Recordings", "IdSchedule");
      TryCreateIndex(model, "Recordings", "IdProgramCategory");
      TryCreateIndex(model, "RecordingCredits", "IdRecording");
      TryCreateIndex(model, "Schedules", "IdChannel");
      TryCreateIndex(model, "Schedules", "IdParentSchedule");
      TryCreateIndex(model, "TuningDetails", "IdChannel");
    }

    private static void DropConstraints(Model model)
    {
      TryDropConstraint(model, "Recordings", "ChannelRecording");
      TryDropConstraint(model, "Recordings", "RecordingProgramCategory");
      TryDropConstraint(model, "Recordings", "ScheduleRecording");
    }

    /// <summary>
    /// Creates a <see cref="Model"/> instance, which can use the _externalDbCreator.
    /// </summary>
    /// <returns></returns>
    private static Model GetModel()
    {
      Model model = _externalDbCreator != null
        ? new Model(BuildEFConnection(_externalDbCreator()))
        : new Model();
      return model;
    }

    /// <summary>
    /// Helper method to construct a valid <see cref="EntityConnection"/> from a given <paramref name="dbConnection"/>.
    /// This method supports different database types: SQLServer, MySQL, SQLServerCE, the valid model metadata is chosen
    /// automatically.
    /// </summary>
    /// <param name="dbConnection">Connection</param>
    /// <returns>EntityConnection</returns>
    public static EntityConnection BuildEFConnection(DbConnection dbConnection)
    {
      var database = DetectDatabase(dbConnection);
      lock (_syncObj)
      {
        if (database != _databaseType || _metadataWorkspace == null)
        {
          List<string> metadata = new List<string>
          {
            "res://Mediaportal.TV.Server.TVDatabase.EntityModel/Model.csdl",
            "res://Mediaportal.TV.Server.TVDatabase.EntityModel/Model.msl",
            // Select matching ssdl based on the detected database type.
            string.Format("res://Mediaportal.TV.Server.TVDatabase.EntityModel/Mediaportal.TV.Server.TVDatabase.EntityModel.Model.{0}.ssdl", database)
          };
          _databaseType = database;
          _metadataWorkspace = new MetadataWorkspace(metadata, new[] { Assembly.GetExecutingAssembly() });
        }
        return new EntityConnection(_metadataWorkspace, dbConnection);
      }
    }

    #endregion

    public static void SetupStaticValues(Model ctx)
    {
      // We need at least one channel group for each media type in order for the UI to function correctly.
      ctx.ChannelGroups.AddObject(new ChannelGroup { GroupName = "Favourites", SortOrder = 9999, MediaType = (int)MediaType.Television });
      ctx.ChannelGroups.AddObject(new ChannelGroup { GroupName = "Favourites", SortOrder = 9999, MediaType = (int)MediaType.Radio });

      // Guide program categories.
      ctx.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Documentary" });
      ctx.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Kids" });
      ctx.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = true, Name = "Movie" });
      ctx.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Music" });
      ctx.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "News" });
      ctx.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Special" });
      ctx.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Sports" });

      // Known LNB types.
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 1, Name = "Universal", LowBandFrequency = 9750000, HighBandFrequency = 10600000, SwitchFrequency = 11700000, IsBandStacked = false });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 2, Name = "C-Band", LowBandFrequency = 5150000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 3, Name = "10700 MHz", LowBandFrequency = 10700000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 4, Name = "10750 MHz", LowBandFrequency = 10750000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 5, Name = "11250 MHz (NA Legacy)", LowBandFrequency = 11250000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 6, Name = "11300 MHz", LowBandFrequency = 11300000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 7, Name = "DishPro Band Stacked FSS", LowBandFrequency = 10750000, HighBandFrequency = 13850000, SwitchFrequency = -1, IsBandStacked = true });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 8, Name = "DishPro Band Stacked DBS", LowBandFrequency = 11250000, HighBandFrequency = 14350000, SwitchFrequency = -1, IsBandStacked = true });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 9, Name = "NA Band Stacked FSS", LowBandFrequency = 10750000, HighBandFrequency = 10175000, SwitchFrequency = -1, IsBandStacked = true });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 10, Name = "NA Band Stacked DBS", LowBandFrequency = 11250000, HighBandFrequency = 10675000, SwitchFrequency = -1, IsBandStacked = true });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 11, Name = "Sadoun Band Stacked", LowBandFrequency = 10100000, HighBandFrequency = 10750000, SwitchFrequency = -1, IsBandStacked = true });
      ctx.LnbTypes.AddObject(new LnbType { IdLnbType = 12, Name = "C-Band Band Stacked", LowBandFrequency = 5150000, HighBandFrequency = 5750000, SwitchFrequency = -1, IsBandStacked = true });

      // List of supported video and combined video/audio encoders.
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 1, Type = (int)SoftwareEncoderType.Automatic, Priority = 1, ClassId = string.Empty, Name = "Automatic" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 2, Type = (int)SoftwareEncoderType.Combined, Priority = 2, ClassId = "{4d997eb1-46f5-4818-b5ce-6900aa7ed43b}", Name = "AVerMedia" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 3, Type = (int)SoftwareEncoderType.Combined, Priority = 3, ClassId = "{0a775550-00f0-4b13-a454-3f8eab4ac0b6}", Name = "Hauppauge SoftMCE [combined MainConcept]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 4, Type = (int)SoftwareEncoderType.Video, Priority = 4, ClassId = "{ae5660ae-7e08-48e6-a738-7ee6338c614f}", Name = "Hauppauge SoftMCE [MainConcept]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 5, Type = (int)SoftwareEncoderType.Video, Priority = 5, ClassId = "{c251e660-2ff5-4db1-8235-ab35bdcaf95e}", Name = "Hauppauge SoftPVR [MainConcept]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 6, Type = (int)SoftwareEncoderType.Video, Priority = 6, ClassId = "{4ffff691-cc45-4372-9635-ca5151ba2bff}", Name = "KWorld [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 7, Type = (int)SoftwareEncoderType.Video, Priority = 7, ClassId = "{cf486bca-92de-4290-bcb4-ad4b5fef6668}", Name = "Medion PowerCinema [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 8, Type = (int)SoftwareEncoderType.Video, Priority = 8, ClassId = "{36b46e60-d240-11d2-8f3f-0080c84e9806}", Name = "Medion PowerVCR II [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 9, Type = (int)SoftwareEncoderType.Video, Priority = 9, ClassId = "{de37b729-3c95-44dc-a0e0-c0a956736fa6}", Name = "PCTV Systems" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 10, Type = (int)SoftwareEncoderType.Video, Priority = 10, ClassId = "{215817f3-b2ad-4b28-af04-252e8936dee1}", Name = "Pinnacle" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 11, Type = (int)SoftwareEncoderType.Video, Priority = 11, ClassId = "{bf94d1f7-fb26-4a6a-9d83-092d88b17a36}", Name = "TerraTec [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 12, Type = (int)SoftwareEncoderType.Video, Priority = 12, ClassId = "{fcb24ebf-b9d0-40d9-9076-f27b93dbb952}", Name = "Twinhan [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 13, Type = (int)SoftwareEncoderType.Video, Priority = 13, ClassId = "{758c0f02-df95-11d2-8e75-00104b93cf06}", Name = "ATI" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 14, Type = (int)SoftwareEncoderType.Video, Priority = 14, ClassId = "{ad6a3830-d190-4d98-9135-a74c942af32f}", Name = "CyberLink" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 15, Type = (int)SoftwareEncoderType.Video, Priority = 15, ClassId = "{09c8d515-5c6a-434d-ad92-fef7eb153310}", Name = "CyberLink [Power2Go]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 16, Type = (int)SoftwareEncoderType.Video, Priority = 16, ClassId = "{77a4e3a6-8a00-4e67-b0e3-705f59e9acd9}", Name = "CyberLink [PowerEncoder]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 17, Type = (int)SoftwareEncoderType.Video, Priority = 17, ClassId = "{ead7bc81-1ddf-4a50-bf5e-225deffff1d1}", Name = "CyberLink [PowerProducer]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 18, Type = (int)SoftwareEncoderType.Video, Priority = 18, ClassId = "{317ddb61-870e-11d3-9c32-00104b3801f6}", Name = "InterVideo" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 19, Type = (int)SoftwareEncoderType.Video, Priority = 19, ClassId = "{2be4d160-6f2e-4b3a-b0bd-e880917238dc}", Name = "MainConcept" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 20, Type = (int)SoftwareEncoderType.Combined, Priority = 20, ClassId = "{2be4d150-6f2e-4b3a-b0bd-e880917238dc}", Name = "MainConcept [combined]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 21, Type = (int)SoftwareEncoderType.Video, Priority = 21, ClassId = "{00098205-76cc-497e-98a1-6ef10d0bf26c}", Name = "MainConcept [Applian]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 22, Type = (int)SoftwareEncoderType.Video, Priority = 22, ClassId = "{42150cd9-ca9a-4ea5-9939-30ee037f6e74}", Name = "Microsoft" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 23, Type = (int)SoftwareEncoderType.Combined, Priority = 23, ClassId = "{5f5aff4a-2f7f-4279-88c2-cd88eb39d144}", Name = "Microsoft [combined]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 24, Type = (int)SoftwareEncoderType.Video, Priority = 24, ClassId = "{efd8a271-7391-11d4-8807-00e018a8539a}", Name = "nanocosmos" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 25, Type = (int)SoftwareEncoderType.Video, Priority = 25, ClassId = "{cf957f50-77fe-4192-a59f-95ca43bd04ba}", Name = "Ulead" });

      // List of supported audio encoders.
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 26, Type = (int)SoftwareEncoderType.Audio, Priority = 1, ClassId = "{5f5cebd4-cc63-4e85-bd37-534f62a658d0}", Name = "Hauppauge SoftMCE [MainConcept]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 27, Type = (int)SoftwareEncoderType.Audio, Priority = 2, ClassId = "{c251e670-2ff5-4db1-8235-ab35bdcaf95e}", Name = "Hauppauge SoftPVR [MainConcept]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 28, Type = (int)SoftwareEncoderType.Audio, Priority = 3, ClassId = "{20d30120-e975-45d7-ace2-8e2ac5581f33}", Name = "KWorld [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 29, Type = (int)SoftwareEncoderType.Audio, Priority = 4, ClassId = "{44bfff7d-47f3-4591-ac3a-60d94e80b2cc}", Name = "Medion PowerCinema [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 30, Type = (int)SoftwareEncoderType.Audio, Priority = 5, ClassId = "{a3d70ac0-9023-11d2-8d55-0080c84e9c68}", Name = "Medion PowerVCR II [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 31, Type = (int)SoftwareEncoderType.Audio, Priority = 6, ClassId = "{8928e446-2282-4192-9f10-6e2c563477da}", Name = "PCTV Systems" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 32, Type = (int)SoftwareEncoderType.Audio, Priority = 7, ClassId = "{695db666-361c-4be5-8219-9960be378101}", Name = "Pinnacle" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 33, Type = (int)SoftwareEncoderType.Audio, Priority = 8, ClassId = "{14c276b7-c71d-4f45-8439-1fc8bae6c6aa}", Name = "TerraTec [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 34, Type = (int)SoftwareEncoderType.Audio, Priority = 9, ClassId = "{649a74a7-879b-4a01-9178-3ca1c2d4e400}", Name = "Twinhan [CyberLink]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 35, Type = (int)SoftwareEncoderType.Audio, Priority = 10, ClassId = "{6467dd70-fbd5-11d2-b5b6-444553540000}", Name = "ATI" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 36, Type = (int)SoftwareEncoderType.Audio, Priority = 11, ClassId = "{e25fa630-52de-4bfa-9e98-b5cbd500396d}", Name = "CyberLink" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 37, Type = (int)SoftwareEncoderType.Audio, Priority = 12, ClassId = "{e8f36981-7d45-4af4-aca2-e7d960d5ad6f}", Name = "CyberLink [Power2Go]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 38, Type = (int)SoftwareEncoderType.Audio, Priority = 13, ClassId = "{5a9f9772-ccd3-4bac-81aa-d60574d67afb}", Name = "CyberLink [PowerEncoder]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 39, Type = (int)SoftwareEncoderType.Audio, Priority = 14, ClassId = "{5f15e71d-8066-4d26-bb0a-41a17339dc92}", Name = "CyberLink [PowerProducer]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 40, Type = (int)SoftwareEncoderType.Audio, Priority = 15, ClassId = "{0cd2e140-8d60-11d3-9c32-00104b3801f6}", Name = "InterVideo" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 41, Type = (int)SoftwareEncoderType.Audio, Priority = 16, ClassId = "{2be4d170-6f2e-4b3a-b0bd-e880917238dc}", Name = "MainConcept" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 42, Type = (int)SoftwareEncoderType.Audio, Priority = 17, ClassId = "{15bebb32-5bb5-42b6-b45a-ba49f78ba19f}", Name = "MainConcept [Applian]" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 43, Type = (int)SoftwareEncoderType.Audio, Priority = 18, ClassId = "{acd453bc-c58a-44d1-bbf5-bfb325be2d78}", Name = "Microsoft" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 44, Type = (int)SoftwareEncoderType.Audio, Priority = 19, ClassId = "{e6a3558a-932a-4720-97d6-dc5eda03a3f7}", Name = "nanocosmos" });
      ctx.SoftwareEncoders.AddObject(new SoftwareEncoder { IdSoftwareEncoder = 45, Type = (int)SoftwareEncoderType.Audio, Priority = 20, ClassId = "{cf957f70-77fe-4192-a59f-95ca43bd04ba}", Name = "Ulead" });

      ctx.SaveChanges();
    }

    private static readonly object _createDbContextLock = new object();

    public static Model CreateDbContext()
    {
      lock (_createDbContextLock)
      {
        // seems a new instance per WCF is the way to go, since a shared context will end up in EF errors.
        Initialize();
      }

      var model = GetModel();

      //model.ContextOptions.DefaultQueryPlanCachingSetting = true;
      model.ContextOptions.LazyLoadingEnabled = false;
      model.ContextOptions.ProxyCreationEnabled = false;

      model.AnalogTunerSettings.MergeOption = MergeOption.NoTracking;
      model.CanceledSchedules.MergeOption = MergeOption.NoTracking;
      model.ChannelGroups.MergeOption = MergeOption.NoTracking;
      model.ChannelLinkageMaps.MergeOption = MergeOption.NoTracking;
      model.ChannelMaps.MergeOption = MergeOption.NoTracking;
      model.Channels.MergeOption = MergeOption.NoTracking;
      model.Conflicts.MergeOption = MergeOption.NoTracking;
      model.DiseqcMotors.MergeOption = MergeOption.NoTracking;
      model.GroupMaps.MergeOption = MergeOption.NoTracking;
      model.Histories.MergeOption = MergeOption.NoTracking;
      model.PendingDeletions.MergeOption = MergeOption.NoTracking;
      model.ProgramCategories.MergeOption = MergeOption.NoTracking;
      model.ProgramCredits.MergeOption = MergeOption.NoTracking;
      model.Programs.MergeOption = MergeOption.NoTracking;
      model.RecordingCredits.MergeOption = MergeOption.NoTracking;
      model.Recordings.MergeOption = MergeOption.NoTracking;
      model.RuleBasedSchedules.MergeOption = MergeOption.NoTracking;
      model.Satellites.MergeOption = MergeOption.NoTracking;
      model.ScheduleRulesTemplates.MergeOption = MergeOption.NoTracking;
      model.Schedules.MergeOption = MergeOption.NoTracking;
      model.Settings.MergeOption = MergeOption.NoTracking;
      model.SoftwareEncoders.MergeOption = MergeOption.NoTracking;
      model.TunerGroups.MergeOption = MergeOption.NoTracking;
      model.TunerProperties.MergeOption = MergeOption.NoTracking;
      model.Tuners.MergeOption = MergeOption.NoTracking;
      model.TuningDetails.MergeOption = MergeOption.NoTracking;
      model.Versions.MergeOption = MergeOption.NoTracking;
      return model;
    }

    /// <summary>
    /// Tries to drop an existing constraint (i.e. if it was created internally by EF).
    /// </summary>
    /// <param name="model">Model</param>
    /// <param name="tableName">Table name</param>
    /// <param name="constraintName">Constraint name</param>
    private static void TryDropConstraint(Model model, string tableName, string constraintName)
    {
      var database = DetectDatabase(model);
      try
      {
        string sql = database == SupportedDatabaseType.MySQL ?
          "ALTER TABLE {0} DROP FOREIGN KEY {1}" : // MySQL syntax
          "ALTER TABLE {0} DROP CONSTRAINT {1}";   // Microsoft-SQLServer(+CE) syntax
        model.ExecuteStoreCommand(string.Format(sql, tableName, constraintName));
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "Failed to drop constraint {0}.{1}", tableName, constraintName);
      }
    }

    /// <summary>
    /// Tries to create an Index on given <paramref name="tableName"/> for a single column <paramref name="indexedColumn"/>.
    /// </summary>
    /// <param name="model">Model</param>
    /// <param name="tableName">Table name</param>
    /// <param name="indexedColumn">Column name</param>
    private static void TryCreateIndex(Model model, string tableName, string indexedColumn)
    {
      TryCreateIndex(model, tableName, new List<string> { indexedColumn });
    }

    /// <summary>
    /// Tries to create an Index on given <paramref name="tableName"/> for multiple columns (<paramref name="indexedColumns"/>).
    /// </summary>
    /// <param name="model">Model</param>
    /// <param name="tableName">Table name</param>
    /// <param name="indexedColumns">Column names</param>
    private static void TryCreateIndex(Model model, string tableName, IList<string> indexedColumns)
    {
      try
      {
        model.ExecuteStoreCommand(string.Format("CREATE INDEX IX_{0}_{1} ON {0} ({2})", tableName, string.Join("_", indexedColumns), string.Join(", ", indexedColumns)).ToUpperInvariant());
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "Failed to create index on table {0} for column(s) {1}", tableName, string.Join(", ", indexedColumns));
      }
    }

    /*public static void Init(string[] mappingAssemblies, bool recreateDatabaseIfExist = false, bool lazyLoadingEnabled = true)
      {
          Init(DefaultConnectionStringName, mappingAssemblies, recreateDatabaseIfExist, lazyLoadingEnabled);
      }        

      public static void Init(string connectionStringName, string[] mappingAssemblies, bool recreateDatabaseIfExist = false, bool lazyLoadingEnabled = true)
      {
          AddConfiguration(connectionStringName, mappingAssemblies, recreateDatabaseIfExist, lazyLoadingEnabled);
      }       

      public static void InitStorage(IObjectContextStorage storage)
      {
          if (storage == null) 
          {
              throw new ArgumentNullException("storage");
          }
          if ((Storage != null) && (Storage != storage))
          {
              throw new ApplicationException("A storage mechanism has already been configured for this application");
          }            
          Storage = storage;
      }

      /// <summary>
      /// The default connection string name used if only one database is being communicated with.
      /// </summary>
      public static readonly string DefaultConnectionStringName = "Model";        

      /// <summary>
      /// Used to get the current object context session if you're communicating with a single database.
      /// When communicating with multiple databases, invoke <see cref="CurrentFor()" /> instead.
      /// </summary>
      public static System.Data.Objects.ObjectContext Current
      {
          get
          {
              return CurrentFor(DefaultConnectionStringName);
          }
      }

      /// <summary>
      /// Used to get the current ObjectContext associated with a key; i.e., the key 
      /// associated with an object context for a specific database.
      /// 
      /// If you're only communicating with one database, you should call <see cref="Current" /> instead,
      /// although you're certainly welcome to call this if you have the key available.
      /// </summary>
      public static System.Data.Objects.ObjectContext CurrentFor(string key)
      {
          if (string.IsNullOrEmpty(key))
          {
              throw new ArgumentNullException("key");
          }

          if (Storage == null)
          {
              throw new ApplicationException("An IObjectContextStorage has not been initialized");
          }

          System.Data.Objects.ObjectContext context = null;
          lock (_syncLock)
          {
              if (!objectContextBuilders.ContainsKey(key))
              {
                  throw new ApplicationException("An ObjectContextBuilder does not exist with a key of " + key);
              }

              context = Storage.GetObjectContextForKey(key);

              if (context == null)
              {
                  context = objectContextBuilders[key].BuildObjectContext();
                  Storage.SetObjectContextForKey(key, context);
              }
          }

          return context;
      }

      /// <summary>
      /// This method is used by application-specific object context storage implementations
      /// and unit tests. Its job is to walk thru existing cached object context(s) and Close() each one.
      /// </summary>
      public static void CloseAllObjectContexts()
      {
          foreach (System.Data.Objects.ObjectContext ctx in Storage.GetAllObjectContexts())
          {
              if (ctx.Connection.State == System.Data.ConnectionState.Open)
                  ctx.Connection.Close();
          }
      }

      private static void AddConfiguration(string connectionStringName, string[] mappingAssemblies, bool recreateDatabaseIfExists = false, bool lazyLoadingEnabled = true)
      {
          if (string.IsNullOrEmpty(connectionStringName))
          {
              throw new ArgumentNullException("connectionStringName");
          }

          if (mappingAssemblies == null)
          {
              throw new ArgumentNullException("mappingAssemblies");
          }

          objectContextBuilders.Add(connectionStringName,
              new ObjectContextBuilder<System.Data.Objects.ObjectContext>(connectionStringName, mappingAssemblies, recreateDatabaseIfExists, lazyLoadingEnabled));
      }        

      /// <summary>
      /// An application-specific implementation of IObjectContextStorage must be setup either thru
      /// <see cref="InitStorage" /> or one of the <see cref="Init" /> overloads. 
      /// </summary>
      private static IObjectContextStorage Storage { get; set; }

      /// <summary>
      /// Maintains a dictionary of object context builders, one per database.  The key is a 
      /// connection string name used to look up the associated database, and used to decorate respective
      /// repositories. If only one database is being used, this dictionary contains a single
      /// factory with a key of <see cref="DefaultConnectionStringName" />.
      /// </summary>
      private static Dictionary<string, IObjectContextBuilder<System.Data.Objects.ObjectContext>> objectContextBuilders = new Dictionary<string, IObjectContextBuilder<System.Data.Objects.ObjectContext>>();

      private static object _syncLock = new object();*/
  }
}