using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
    /// <summary>
    /// Simple object context storage implementation
    /// </summary>
    public class SimpleObjectContextStorage : IObjectContextStorage
    {
        private Dictionary<string, System.Data.Objects.ObjectContext> storage = new Dictionary<string, System.Data.Objects.ObjectContext>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleObjectContextStorage"/> class.
        /// </summary>
        public SimpleObjectContextStorage() { }

        /// <summary>
        /// Returns the object context associated with the specified key or
		/// null if the specified key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public System.Data.Objects.ObjectContext GetObjectContextForKey(string key)
        {
            System.Data.Objects.ObjectContext context;
            if (!storage.TryGetValue(key, out context))
                return null;
            return context;
        }


        /// <summary>
        /// Stores the object context into a dictionary using the specified key.
        /// If an object context already exists by the specified key, 
        /// it gets overwritten by the new object context passed in.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="objectContext">The object context.</param>
        public void SetObjectContextForKey(string key, System.Data.Objects.ObjectContext objectContext)
        {           
            storage.Add(key, objectContext);           
        }

        /// <summary>
        /// Returns all the values of the internal dictionary of object contexts.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<System.Data.Objects.ObjectContext> GetAllObjectContexts()
        {
            return storage.Values;
        }
    }
}
