using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;


namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
    public static class ObjectContextManager
    {
      static ObjectContextManager ()
      {
        try
        {
          var model = new Model();

          if (!model.DatabaseExists())
          {
            Log.Info("DataBase does not exist. Creating database...");
            model.CreateDatabase();
            SetupStaticValues(model);
          }
          DropConstraint(model, "Recordings", "FK_ChannelRecording");
          DropConstraint(model, "Recordings", "FK_RecordingProgramCategory");
          DropConstraint(model, "Recordings", "FK_ScheduleRecording");    
        }
        catch(Exception ex)
        {
          Log.Error("ObjectContextManager : error opening database. Is the SQL engine running ?", ex);
          throw;
        }        
      }

      public static void SetupStaticValues(Model ctx)
      {
        LnbType lnbtype = new LnbType();
        lnbtype.IdLnbType = 1;
        lnbtype.Name = "Universal";
        lnbtype.LowBandFrequency = 9750000;
        lnbtype.HighBandFrequency = 10600000;
        lnbtype.SwitchFrequency = 11700000;
        lnbtype.IsBandStacked = false;
        lnbtype.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype);
        ctx.SaveChanges();
        LnbType lnbtype2 = new LnbType();
        lnbtype2.IdLnbType = 2;
        lnbtype2.Name = "C-Band";
        lnbtype2.LowBandFrequency = 5150000;
        lnbtype2.HighBandFrequency = 5650000;
        lnbtype2.SwitchFrequency = 18000000;
        lnbtype2.IsBandStacked = false;
        lnbtype2.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype2);
        ctx.SaveChanges();
        LnbType lnbtype3 = new LnbType();
        lnbtype3.IdLnbType = 3;
        lnbtype3.Name = "10750 MHz";
        lnbtype3.LowBandFrequency = 10750000;
        lnbtype3.HighBandFrequency = 11250000;
        lnbtype3.SwitchFrequency = 18000000;
        lnbtype3.IsBandStacked = false;
        lnbtype3.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype3);
        ctx.SaveChanges();
        LnbType lnbtype4 = new LnbType();
        lnbtype4.IdLnbType = 4;
        lnbtype4.Name = "11250 MHz (NA Legacy)";
        lnbtype4.LowBandFrequency = 11250000;
        lnbtype4.HighBandFrequency = 11750000;
        lnbtype4.SwitchFrequency = 18000000;
        lnbtype4.IsBandStacked = false;
        lnbtype4.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype4);
        ctx.SaveChanges();
        LnbType lnbtype5 = new LnbType();
        lnbtype5.IdLnbType = 5;
        lnbtype5.Name = "11300 MHz";
        lnbtype5.LowBandFrequency = 11300000;
        lnbtype5.HighBandFrequency = 11800000;
        lnbtype5.SwitchFrequency = 18000000;
        lnbtype5.IsBandStacked = false;
        lnbtype5.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype5);
        ctx.SaveChanges();
        LnbType lnbtype6 = new LnbType();
        lnbtype6.IdLnbType = 6;
        lnbtype6.Name = "DishPro Band Stacked FSS";
        lnbtype6.LowBandFrequency = 10750000;
        lnbtype6.HighBandFrequency = 13850000;
        lnbtype6.SwitchFrequency = 18000000;
        lnbtype6.IsBandStacked = true;
        lnbtype6.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype6);
        ctx.SaveChanges();
        LnbType lnbtype7 = new LnbType();
        lnbtype7.IdLnbType = 7;
        lnbtype7.Name = "DishPro Band Stacked DBS";
        lnbtype7.LowBandFrequency = 11250000;
        lnbtype7.HighBandFrequency = 14350000;
        lnbtype7.SwitchFrequency = 18000000;
        lnbtype7.IsBandStacked = true;
        lnbtype7.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype7);
        ctx.SaveChanges();
        LnbType lnbtype8 = new LnbType();
        lnbtype8.IdLnbType = 8;
        lnbtype8.Name = "NA Band Stacked FSS";
        lnbtype8.LowBandFrequency = 10750000;
        lnbtype8.HighBandFrequency = 10175000;
        lnbtype8.SwitchFrequency = 18000000;
        lnbtype8.IsBandStacked = true;
        lnbtype8.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype8);
        ctx.SaveChanges();
        LnbType lnbtype9 = new LnbType();
        lnbtype9.IdLnbType = 9;
        lnbtype9.Name = "NA Band Stacked DBS";
        lnbtype9.LowBandFrequency = 11250000;
        lnbtype9.HighBandFrequency = 10675000;
        lnbtype9.SwitchFrequency = 18000000;
        lnbtype9.IsBandStacked = true;
        lnbtype9.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype9);
        ctx.SaveChanges();
        LnbType lnbtype10 = new LnbType();
        lnbtype10.IdLnbType = 10;
        lnbtype10.Name = "Sadoun Band Stacked";
        lnbtype10.LowBandFrequency = 10100000;
        lnbtype10.HighBandFrequency = 10750000;
        lnbtype10.SwitchFrequency = 18000000;
        lnbtype10.IsBandStacked = true;
        lnbtype10.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype10);
        ctx.SaveChanges();
        LnbType lnbtype11 = new LnbType();
        lnbtype11.IdLnbType = 11;
        lnbtype11.Name = "C-Band Band Stacked";
        lnbtype11.LowBandFrequency = 5150000;
        lnbtype11.HighBandFrequency = 5750000;
        lnbtype11.SwitchFrequency = 18000000;
        lnbtype11.IsBandStacked = true;
        lnbtype11.IsToroidal = false;
        ctx.LnbTypes.AddObject(lnbtype11);
        ctx.SaveChanges();
      }

      public static Model CreateDbContext
      {
        // seems a new instance per WCF is the way to go, since a shared context will end up in EF errors.
        get
        {          
          var model = new Model();

          //model.ContextOptions.DefaultQueryPlanCachingSetting = true;
          model.ContextOptions.LazyLoadingEnabled = false;
          model.ContextOptions.ProxyCreationEnabled = false;

          model.CanceledSchedules.MergeOption = MergeOption.NoTracking;
          model.CardGroupMaps.MergeOption = MergeOption.NoTracking;
          model.CardGroups.MergeOption = MergeOption.NoTracking;
          model.Cards.MergeOption = MergeOption.NoTracking;
          model.ChannelGroups.MergeOption = MergeOption.NoTracking;
          model.ChannelLinkageMaps.MergeOption = MergeOption.NoTracking;
          model.ChannelMaps.MergeOption = MergeOption.NoTracking;
          model.Channels.MergeOption = MergeOption.NoTracking;
          model.Conflicts.MergeOption = MergeOption.NoTracking;
          model.DisEqcMotors.MergeOption = MergeOption.NoTracking;
          model.Favorites.MergeOption = MergeOption.NoTracking;
          model.GroupMaps.MergeOption = MergeOption.NoTracking;
          model.Histories.MergeOption = MergeOption.NoTracking;
          model.KeywordMaps.MergeOption = MergeOption.NoTracking;
          model.Keywords.MergeOption = MergeOption.NoTracking;
          model.PendingDeletions.MergeOption = MergeOption.NoTracking;
          model.PersonalTVGuideMaps.MergeOption = MergeOption.NoTracking;
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
          model.Timespans.MergeOption = MergeOption.NoTracking;
          model.TuningDetails.MergeOption = MergeOption.NoTracking;
          model.TvMovieMappings.MergeOption = MergeOption.NoTracking;
          model.Versions.MergeOption = MergeOption.NoTracking;

          return model;
        }
      }

      private static void DropConstraint(Model model, string tablename, string constraintname)
      {
        try
        {
          model.ExecuteStoreCommand("ALTER TABLE " + tablename + " DROP CONSTRAINT [" + constraintname + "]");
        }
        catch (Exception)
        {
          //ignore
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
