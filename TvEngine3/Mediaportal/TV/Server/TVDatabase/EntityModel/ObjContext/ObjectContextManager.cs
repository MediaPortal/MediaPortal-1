using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
    public static class ObjectContextManager
    {

      public static Model CreateDbContext
      {
        // seems a new instance per WCF is the way to go, since a shared context will end up in EF errors.
        get
        {
          return new Model();
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
