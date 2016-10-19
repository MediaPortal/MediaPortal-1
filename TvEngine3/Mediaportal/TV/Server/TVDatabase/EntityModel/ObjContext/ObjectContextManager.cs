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
    // Note that these enum values must match the SSDL file suffix (eg.
    // Model.MySQL.ssdl). Refer to GetModel().
    private enum DatabaseEngine
    {
      None,
      MySQL,
      MSSQL,
      MSSQLCE,
      SQLite
    }

    #region Static fields

    public delegate DbConnection CreateConnectionDelegate();
    private static CreateConnectionDelegate _externalDbConnectionCreator;

    private static readonly object _syncObj = new object();
    private static readonly object _createDbContextLock = new object();
    private static MetadataWorkspace _metadataWorkspace = null;
    private static bool _initialised = false;
    private static DatabaseEngine _databaseEngine;

    #endregion

    /// <summary>
    /// Sets an external <see cref="CreateConnectionDelegate"/> that returns a <see cref="DbConnection"/>. This can be used
    /// by hosting environments to supply their own database to be used for EF.
    /// </summary>
    /// <param name="creatorDelegate">Creator</param>
    public static void SetDbConnectionCreator(CreateConnectionDelegate creatorDelegate)
    {
      _externalDbConnectionCreator = creatorDelegate;
    }

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
      model.AudioEncoders.MergeOption = MergeOption.NoTracking;
      model.CanceledSchedules.MergeOption = MergeOption.NoTracking;
      model.ChannelGroups.MergeOption = MergeOption.NoTracking;
      model.ChannelLinkageMaps.MergeOption = MergeOption.NoTracking;
      model.ChannelMaps.MergeOption = MergeOption.NoTracking;
      model.Channels.MergeOption = MergeOption.NoTracking;
      model.Conflicts.MergeOption = MergeOption.NoTracking;
      model.GroupMaps.MergeOption = MergeOption.NoTracking;
      model.GuideCategories.MergeOption = MergeOption.NoTracking;
      model.Histories.MergeOption = MergeOption.NoTracking;
      model.LnbTypes.MergeOption = MergeOption.NoTracking;
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
      model.TunerGroups.MergeOption = MergeOption.NoTracking;
      model.TunerProperties.MergeOption = MergeOption.NoTracking;
      model.TunerSatellites.MergeOption = MergeOption.NoTracking;
      model.Tuners.MergeOption = MergeOption.NoTracking;
      model.TuningDetails.MergeOption = MergeOption.NoTracking;
      model.Versions.MergeOption = MergeOption.NoTracking;
      model.VideoEncoders.MergeOption = MergeOption.NoTracking;
      return model;
    }

    /// <summary>
    /// Create the model and database (if it doesn't already exist).
    /// </summary>
    private static void Initialize()
    {
      if (_initialised)
      {
        return;
      }
      try
      {
        var model = GetModel();
        _databaseEngine = DetectDatabaseEngine(model);
        if (CreateDatabaseIfMissing(model))
        {
          Log.Info("object context manager: database does not exist, creating...");
          CreateIndexes(model);
          if (_databaseEngine != DatabaseEngine.SQLite)
          {
            DropConstraints(model);
          }
          InsertInitialValues(model);
        }
        _initialised = true;
      }
      catch (Exception ex)
      {
        Log.Error(ex, "object context manager: failed to connect to database, is the database engine running and accessible?");
        throw;
      }
    }

    private static Model GetModel()
    {
      if (_externalDbConnectionCreator == null)
      {
        return new Model();
      }

      DbConnection databaseConnection = _externalDbConnectionCreator();
      var databaseEngine = DetectDatabaseEngine(databaseConnection);
      EntityConnection connection;
      lock (_syncObj)
      {
        if (databaseEngine != _databaseEngine || _metadataWorkspace == null)
        {
          List<string> metadata = new List<string>
          {
            "res://Mediaportal.TV.Server.TVDatabase.EntityModel/Model.csdl",
            "res://Mediaportal.TV.Server.TVDatabase.EntityModel/Model.msl",
            // Select matching SSDL based on the detected database engine.
            string.Format("res://Mediaportal.TV.Server.TVDatabase.EntityModel/Mediaportal.TV.Server.TVDatabase.EntityModel.Model.{0}.ssdl", databaseEngine)
          };
          _databaseEngine = databaseEngine;
          _metadataWorkspace = new MetadataWorkspace(metadata, new[] { Assembly.GetExecutingAssembly() });
        }
        connection = new EntityConnection(_metadataWorkspace, databaseConnection);
      }
      return new Model(connection);
    }

    private static DatabaseEngine DetectDatabaseEngine(Model model)
    {
      return DetectDatabaseEngine(((EntityConnection)model.Connection).StoreConnection);
    }

    private static DatabaseEngine DetectDatabaseEngine(DbConnection connection)
    {
      string connectionType = connection.GetType().ToString();
      if (connectionType.Contains("MySqlClient"))
      {
        return DatabaseEngine.MySQL;
      }
      if (connectionType.Contains("SQLite"))
      {
        return DatabaseEngine.SQLite;
      }
      if (connectionType.Contains("SqlServerCe"))
      {
        return DatabaseEngine.MSSQLCE;
      }
      if (connectionType.Contains("SqlClient"))
      {
        return DatabaseEngine.MSSQL;
      }
      return DatabaseEngine.None;
    }

    /// <summary>
    /// Check if the database exists and create it if not.
    /// </summary>
    /// <param name="model">Model</param>
    /// <returns><c>true</c> if the database was created</returns>
    private static bool CreateDatabaseIfMissing(Model model)
    {
      // This is a special workaround for the System.Data.SQLite provider,
      // which does not support CreateDatabase(). Since we can't create the
      // database programatically, we supply a template with the required
      // structure. The template is equivalent to what CreateDatabase() would
      // produce if it were supported.
      if (_databaseEngine == DatabaseEngine.SQLite)
      {
        int rowCount = model.ExecuteStoreQuery<int>("SELECT COUNT(*) FROM LnbTypes").First();
        // rowCount == 0: bare template, treated as "database was missing, and we created it".
        // rowCount > 0: database exists and contains data.
        // An exception will be thrown if the LnbTypes table doesn't exist.
        // In that scenario there's absolutely nothing we can do.
        return rowCount == 0;
      }

      if (!model.DatabaseExists())
      {
        model.CreateDatabase();
        return true;
      }

      return false;
    }

    private static void CreateIndexes(Model model)
    {
      // Create an index for each foreign key.
      TryCreateIndex(model, "AnalogTunerSettings", "IdAudioEncoder");
      TryCreateIndex(model, "AnalogTunerSettings", "IdVideoEncoder");
      TryCreateIndex(model, "CanceledSchedules", "IdSchedule");
      TryCreateIndex(model, "ChannelMaps", "IdTuner");
      TryCreateIndex(model, "ChannelMaps", "IdChannel");
      TryCreateIndex(model, "ChannelLinkageMaps", "IdLinkedChannel");
      TryCreateIndex(model, "ChannelLinkageMaps", "IdPortalChannel");
      TryCreateIndex(model, "Conflicts", "IdTuner");
      TryCreateIndex(model, "Conflicts", "IdChannel");
      TryCreateIndex(model, "Conflicts", "IdSchedule");
      TryCreateIndex(model, "Conflicts", "IdConflictingSchedule");
      TryCreateIndex(model, "GroupMaps", "IdGroup");
      TryCreateIndex(model, "GroupMaps", "IdChannel");
      TryCreateIndex(model, "Histories", "IdChannel");
      TryCreateIndex(model, "Histories", "IdProgramCategory");
      TryCreateIndex(model, "Programs", "IdChannel");
      TryCreateIndex(model, "Programs", "IdProgramCategory");
      TryCreateIndex(model, "ProgramCategories", "IdGuideCategory");
      TryCreateIndex(model, "ProgramCredits", "IdProgram");
      TryCreateIndex(model, "Recordings", "IdChannel");
      TryCreateIndex(model, "Recordings", "IdSchedule");
      TryCreateIndex(model, "Recordings", "IdProgramCategory");
      TryCreateIndex(model, "RecordingCredits", "IdRecording");
      TryCreateIndex(model, "Schedules", "IdChannel");
      TryCreateIndex(model, "Schedules", "IdParentSchedule");
      TryCreateIndex(model, "Tuners", "IdTunerGroup");
      TryCreateIndex(model, "TunerProperties", "IdTuner");
      TryCreateIndex(model, "TunerSatellites", "IdLnbType");
      TryCreateIndex(model, "TunerSatellites", "IdSatellite");
      TryCreateIndex(model, "TunerSatellites", "IdTuner");
      TryCreateIndex(model, "TuningDetails", "IdChannel");
      TryCreateIndex(model, "TuningDetails", "IdSatellite");

      // Additional indicies for performance.
      TryCreateIndex(model, "Channels", "MediaType");
      TryCreateIndex(model, "Programs", new List<string> { "StartTime", "EndTime" });
    }

    /// <summary>
    /// Try to create an index on a <paramref name="columnName">single column</paramref> within a <paramref name="tableName">table</paramref>.
    /// </summary>
    /// <param name="model">Model</param>
    /// <param name="tableName">The name of the table in which the <paramref name="columnName">column</paramref> exists.</param>
    /// <param name="columnName">The name of the column to index.</param>
    private static void TryCreateIndex(Model model, string tableName, string columnName)
    {
      TryCreateIndex(model, tableName, new List<string> { columnName });
    }

    /// <summary>
    /// Try to create an index over one or more <paramref name="columnNames">columns</paramref> within a <paramref name="tableName">table</paramref>.
    /// </summary>
    /// <param name="model">Model</param>
    /// <param name="tableName">The name of the table in which the <paramref name="columnNames">column(s)</paramref> exist.</param>
    /// <param name="columnName">The name(s) of the column(s) to index.</param>
    private static void TryCreateIndex(Model model, string tableName, IList<string> columnNames)
    {
      try
      {
        model.ExecuteStoreCommand(string.Format("CREATE INDEX IX_{0}_{1} ON {0} ({2})", tableName, string.Join("_", columnNames), string.Join(", ", columnNames)).ToUpperInvariant());
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "object context manager: failed to create index on table {0} for column(s) {1}", tableName, string.Join(", ", columnNames));
      }
    }

    private static void DropConstraints(Model model)
    {
      // This code originates from this commit:
      // https://github.com/MediaPortal/MediaPortal-1/commit/1c7fe6a19e71cf2b1f898437e5a6ac0a5431e107
      // SQL Server should be able to handle NULL-valued foreign keys, so I
      // don't understand gibman's comment. Please try to remove in future.
      TryDropForeignKey(model, "Recordings", "ChannelRecording");
      TryDropForeignKey(model, "Recordings", "RecordingProgramCategory");
      TryDropForeignKey(model, "Recordings", "ScheduleRecording");
    }

    /// <summary>
    /// Try to drop an existing foreign key constraint.
    /// </summary>
    /// <param name="model">Model</param>
    /// <param name="tableName">The name of the table in which the <paramref name="constraintName">foreign key constraint</paramref> exists.</param>
    /// <param name="constraintName">The name of the foreign key constraint.</param>
    private static void TryDropForeignKey(Model model, string tableName, string constraintName)
    {
      try
      {
        string sql = _databaseEngine == DatabaseEngine.MySQL ?
          "ALTER TABLE {0} DROP FOREIGN KEY {1}" : // MySQL syntax
          "ALTER TABLE {0} DROP CONSTRAINT {1}";   // SQL Server, including CE
        model.ExecuteStoreCommand(string.Format(sql, tableName, constraintName));
      }
      catch (Exception ex)
      {
        Log.Warn(ex, "object context manager: failed to drop foreign key {0}.{1}", tableName, constraintName);
      }
    }

    private static void InsertInitialValues(Model model)
    {
      // We need at least one channel group for each media type in order for the UI to function correctly.
      model.ChannelGroups.AddObject(new ChannelGroup { GroupName = "Favourites", SortOrder = 9999, MediaType = (int)MediaType.Television });
      model.ChannelGroups.AddObject(new ChannelGroup { GroupName = "Favourites", SortOrder = 9999, MediaType = (int)MediaType.Radio });

      // Guide program categories.
      model.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Documentary" });
      model.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Kids" });
      model.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = true, Name = "Movie" });
      model.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Music" });
      model.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "News" });
      model.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Special" });
      model.GuideCategories.AddObject(new GuideCategory { IsEnabled = true, IsMovie = false, Name = "Sports" });

      // Known LNB types.
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 1, Name = "Universal", LowBandFrequency = 9750000, HighBandFrequency = 10600000, SwitchFrequency = 11700000, IsBandStacked = false, InputFrequencyMinimum = 10700000, InputFrequencyMaximum = 12750000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 2, Name = "C-Band 5150 MHz", LowBandFrequency = 5150000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false, InputFrequencyMinimum = 3400000, InputFrequencyMaximum = 4200000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 3, Name = "C-Band 5750 MHz", LowBandFrequency = 5750000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false, InputFrequencyMinimum = 3600000, InputFrequencyMaximum = 4800000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 4, Name = "Ku-Band 10700 MHz", LowBandFrequency = 10700000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false, InputFrequencyMinimum = 10700000, InputFrequencyMaximum = 12750000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 5, Name = "Ku-Band 10750 MHz", LowBandFrequency = 10750000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false, InputFrequencyMinimum = 10700000, InputFrequencyMaximum = 12750000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 6, Name = "Ku-Band 11250 MHz (NA Legacy)", LowBandFrequency = 11250000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false, InputFrequencyMinimum = 12200000, InputFrequencyMaximum = 12750000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 7, Name = "Ku-Band 11300 MHz", LowBandFrequency = 11300000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false, InputFrequencyMinimum = 12250000, InputFrequencyMaximum = 12750000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 8, Name = "Ka-Band 20450 MHz", LowBandFrequency = 20450000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false, InputFrequencyMinimum = 21400000, InputFrequencyMaximum = 22000000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 9, Name = "Ka-Band 21200 MHz", LowBandFrequency = 21200000, HighBandFrequency = -1, SwitchFrequency = -1, IsBandStacked = false, InputFrequencyMinimum = 19700000, InputFrequencyMaximum = 20200000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 10, Name = "DishPro Band-Stacked FSS", LowBandFrequency = 10750000, HighBandFrequency = 13850000, SwitchFrequency = -1, IsBandStacked = true, InputFrequencyMinimum = 11700000, InputFrequencyMaximum = 12200000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 11, Name = "DishPro Band-Stacked DBS", LowBandFrequency = 11250000, HighBandFrequency = 14350000, SwitchFrequency = -1, IsBandStacked = true, InputFrequencyMinimum = 12200000, InputFrequencyMaximum = 12700000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 12, Name = "NA Band-Stacked FSS", LowBandFrequency = 10750000, HighBandFrequency = 10175000, SwitchFrequency = -1, IsBandStacked = true, InputFrequencyMinimum = 11700000, InputFrequencyMaximum = 12200000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 13, Name = "NA Band-Stacked DBS", LowBandFrequency = 11250000, HighBandFrequency = 10675000, SwitchFrequency = -1, IsBandStacked = true, InputFrequencyMinimum = 12200000, InputFrequencyMaximum = 12700000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 14, Name = "Sadoun Band-Stacked", LowBandFrequency = 10100000, HighBandFrequency = 10750000, SwitchFrequency = -1, IsBandStacked = true, InputFrequencyMinimum = 11700000, InputFrequencyMaximum = 12200000 });
      model.LnbTypes.AddObject(new LnbType { IdLnbType = 15, Name = "C-Band Band-Stacked", LowBandFrequency = 5150000, HighBandFrequency = 5750000, SwitchFrequency = -1, IsBandStacked = true, InputFrequencyMinimum = 3700000, InputFrequencyMaximum = 4200000 });

      // List of supported video and combined video/audio encoders.
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 1, Priority = 1, ClassId = "{4d997eb1-46f5-4818-b5ce-6900aa7ed43b}", Name = "AVerMedia" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 2, Priority = 2, ClassId = "{0a775550-00f0-4b13-a454-3f8eab4ac0b6}", Name = "Hauppauge SoftMCE [combined MainConcept]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 3, Priority = 3, ClassId = "{ae5660ae-7e08-48e6-a738-7ee6338c614f}", Name = "Hauppauge SoftMCE [MainConcept]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 4, Priority = 4, ClassId = "{c251e660-2ff5-4db1-8235-ab35bdcaf95e}", Name = "Hauppauge SoftPVR [MainConcept]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 5, Priority = 5, ClassId = "{4ffff691-cc45-4372-9635-ca5151ba2bff}", Name = "KWorld [CyberLink]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 6, Priority = 6, ClassId = "{cf486bca-92de-4290-bcb4-ad4b5fef6668}", Name = "Medion PowerCinema [CyberLink]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 7, Priority = 7, ClassId = "{36b46e60-d240-11d2-8f3f-0080c84e9806}", Name = "Medion PowerVCR II [CyberLink]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 8, Priority = 8, ClassId = "{de37b729-3c95-44dc-a0e0-c0a956736fa6}", Name = "PCTV Systems" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 9, Priority = 9, ClassId = "{215817f3-b2ad-4b28-af04-252e8936dee1}", Name = "Pinnacle" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 10, Priority = 10, ClassId = "{bf94d1f7-fb26-4a6a-9d83-092d88b17a36}", Name = "TerraTec [CyberLink]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 11, Priority = 11, ClassId = "{fcb24ebf-b9d0-40d9-9076-f27b93dbb952}", Name = "Twinhan [CyberLink]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 12, Priority = 12, ClassId = "{758c0f02-df95-11d2-8e75-00104b93cf06}", Name = "ATI" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 13, Priority = 13, ClassId = "{ad6a3830-d190-4d98-9135-a74c942af32f}", Name = "CyberLink" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 14, Priority = 14, ClassId = "{09c8d515-5c6a-434d-ad92-fef7eb153310}", Name = "CyberLink [Power2Go]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 15, Priority = 15, ClassId = "{77a4e3a6-8a00-4e67-b0e3-705f59e9acd9}", Name = "CyberLink [PowerEncoder]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 16, Priority = 16, ClassId = "{ead7bc81-1ddf-4a50-bf5e-225deffff1d1}", Name = "CyberLink [PowerProducer]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 17, Priority = 17, ClassId = "{317ddb61-870e-11d3-9c32-00104b3801f6}", Name = "InterVideo" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 18, Priority = 18, ClassId = "{2be4d160-6f2e-4b3a-b0bd-e880917238dc}", Name = "MainConcept" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 19, Priority = 19, ClassId = "{2be4d150-6f2e-4b3a-b0bd-e880917238dc}", Name = "MainConcept [combined]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 20, Priority = 20, ClassId = "{00098205-76cc-497e-98a1-6ef10d0bf26c}", Name = "MainConcept [Applian]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 21, Priority = 21, ClassId = "{42150cd9-ca9a-4ea5-9939-30ee037f6e74}", Name = "Microsoft" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 22, Priority = 22, ClassId = "{5f5aff4a-2f7f-4279-88c2-cd88eb39d144}", Name = "Microsoft [combined]" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 23, Priority = 23, ClassId = "{efd8a271-7391-11d4-8807-00e018a8539a}", Name = "nanocosmos" });
      model.VideoEncoders.AddObject(new VideoEncoder { IdVideoEncoder = 24, Priority = 24, ClassId = "{cf957f50-77fe-4192-a59f-95ca43bd04ba}", Name = "Ulead" });

      // List of supported audio encoders.
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 1, Priority = 1, ClassId = "{5f5cebd4-cc63-4e85-bd37-534f62a658d0}", Name = "Hauppauge SoftMCE [MainConcept]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 2, Priority = 2, ClassId = "{c251e670-2ff5-4db1-8235-ab35bdcaf95e}", Name = "Hauppauge SoftPVR [MainConcept]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 3, Priority = 3, ClassId = "{20d30120-e975-45d7-ace2-8e2ac5581f33}", Name = "KWorld [CyberLink]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 4, Priority = 4, ClassId = "{44bfff7d-47f3-4591-ac3a-60d94e80b2cc}", Name = "Medion PowerCinema [CyberLink]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 5, Priority = 5, ClassId = "{a3d70ac0-9023-11d2-8d55-0080c84e9c68}", Name = "Medion PowerVCR II [CyberLink]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 6, Priority = 6, ClassId = "{8928e446-2282-4192-9f10-6e2c563477da}", Name = "PCTV Systems" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 7, Priority = 7, ClassId = "{695db666-361c-4be5-8219-9960be378101}", Name = "Pinnacle" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 8, Priority = 8, ClassId = "{14c276b7-c71d-4f45-8439-1fc8bae6c6aa}", Name = "TerraTec [CyberLink]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 9, Priority = 9, ClassId = "{649a74a7-879b-4a01-9178-3ca1c2d4e400}", Name = "Twinhan [CyberLink]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 10, Priority = 10, ClassId = "{6467dd70-fbd5-11d2-b5b6-444553540000}", Name = "ATI" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 11, Priority = 11, ClassId = "{e25fa630-52de-4bfa-9e98-b5cbd500396d}", Name = "CyberLink" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 12, Priority = 12, ClassId = "{e8f36981-7d45-4af4-aca2-e7d960d5ad6f}", Name = "CyberLink [Power2Go]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 13, Priority = 13, ClassId = "{5a9f9772-ccd3-4bac-81aa-d60574d67afb}", Name = "CyberLink [PowerEncoder]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 14, Priority = 14, ClassId = "{5f15e71d-8066-4d26-bb0a-41a17339dc92}", Name = "CyberLink [PowerProducer]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 15, Priority = 15, ClassId = "{0cd2e140-8d60-11d3-9c32-00104b3801f6}", Name = "InterVideo" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 16, Priority = 16, ClassId = "{2be4d170-6f2e-4b3a-b0bd-e880917238dc}", Name = "MainConcept" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 17, Priority = 17, ClassId = "{15bebb32-5bb5-42b6-b45a-ba49f78ba19f}", Name = "MainConcept [Applian]" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 18, Priority = 18, ClassId = "{acd453bc-c58a-44d1-bbf5-bfb325be2d78}", Name = "Microsoft" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 19, Priority = 19, ClassId = "{e6a3558a-932a-4720-97d6-dc5eda03a3f7}", Name = "nanocosmos" });
      model.AudioEncoders.AddObject(new AudioEncoder { IdAudioEncoder = 20, Priority = 20, ClassId = "{cf957f70-77fe-4192-a59f-95ca43bd04ba}", Name = "Ulead" });

      model.SaveChanges();
    }
  }
}