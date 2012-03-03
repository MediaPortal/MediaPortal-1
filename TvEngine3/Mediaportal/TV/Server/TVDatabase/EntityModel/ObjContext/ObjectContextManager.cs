using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using Mediaportal.TV.Server.TVDatabase.Entities;


namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
    public static class ObjectContextManager
    {
      static ObjectContextManager ()
      {
        var model = new Model();
        DropConstraint(model, "Recordings", "FK_ChannelRecording");
        DropConstraint(model, "Recordings", "FK_RecordingProgramCategory");
        DropConstraint(model, "Recordings", "FK_ScheduleRecording");          
      }

      public static Model CreateDbContext
      {
        // seems a new instance per WCF is the way to go, since a shared context will end up in EF errors.
        get
        {          
          var model = new Model();

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
