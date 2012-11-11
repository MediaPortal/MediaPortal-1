using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.DBaseContext
{
    public class SimpleDbContextStorage : IDbContextStorage
    {
        private Dictionary<string, System.Data.Entity.DbContext> _storage = new Dictionary<string, System.Data.Entity.DbContext>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDbContextStorage"/> class.
        /// </summary>
        public SimpleDbContextStorage() { }

        /// <summary>
        /// Returns the db context associated with the specified key or
		/// null if the specified key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public System.Data.Entity.DbContext GetDbContextForKey(string key)
        {
            System.Data.Entity.DbContext context;
            if (!_storage.TryGetValue(key, out context))
                return null;
            return context;
        }


        /// <summary>
        /// Stores the db context into a dictionary using the specified key.
        /// If an object context already exists by the specified key, 
        /// it gets overwritten by the new object context passed in.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="objectContext">The object context.</param>
        public void SetDbContextForKey(string key, System.Data.Entity.DbContext context)
        {
            _storage.Add(key, context);           
        }

        /// <summary>
        /// Returns all the values of the internal dictionary of db contexts.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<System.Data.Entity.DbContext> GetAllDbContexts()
        {
            return _storage.Values;
        }
    }
}
