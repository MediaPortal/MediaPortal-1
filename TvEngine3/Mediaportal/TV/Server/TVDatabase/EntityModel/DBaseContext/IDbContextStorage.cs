using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.DBaseContext
{
    /// <summary>
    /// Stores object context
    /// </summary>
    public interface IDbContextStorage
    {
        /// <summary>
        /// Gets the db context for key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        System.Data.Entity.DbContext GetDbContextForKey(string key);

        /// <summary>
        /// Sets the db context for key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="objectContext">The object context.</param>
        void SetDbContextForKey(string key, System.Data.Entity.DbContext objectContext);

        /// <summary>
        /// Gets all db contexts.
        /// </summary>
        /// <returns></returns>
        IEnumerable<System.Data.Entity.DbContext> GetAllDbContexts();
    }
}
