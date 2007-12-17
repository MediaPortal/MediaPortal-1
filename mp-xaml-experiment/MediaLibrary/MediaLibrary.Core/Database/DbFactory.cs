using System;
using System.Collections;

namespace MediaLibrary.Database
{
    #region public enum ProviderType
    /// <summary>
    /// Defines the DataAccessLayer implemented data provider types.
    /// </summary>
    public enum ProviderType
    {
        MsSql,
        SQLite
    }
    #endregion

    #region public sealed class DbFactory
    /// <summary>
    /// Loads different data access layer provider depending on the caller defined data provider type.
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public sealed class DbFactory
    {
        // Since this class provides only static methods, make the default constructor private to prevent 
        // instances from being created with "new DataAccessLayerFactory()"
        private DbFactory() { }

        #region public static DbProvider GetProvider(ProviderType Provider, string Name, IMLHashItem Properties)
        /// <summary>
        /// Returns an instance of the specified provider provider.
        /// </summary>
        /// <param name="Provider"></param>
        /// <param name="Name"></param>
        /// <param name="Properties"></param>
        /// <returns></returns>
        public static DbProvider GetProvider(ProviderType Provider, string Name, IMLHashItem Properties)
        {
            // construct specific data access provider class
            switch (Provider)
            {
                case ProviderType.SQLite:
                    return new Providers.SQLiteDbProvider(Name, Properties);

                //case ProviderType.MsSql:
                //    return new Providers.MsSqlDbProvider(Name, Properties);

                default:
                    throw new ArgumentException("Invalid provider type.");
            }
        }
        #endregion

        #region public static ArrayList GetSections(ProviderType Provider, IMLHashItem Properties)
        /// <summary>
        /// Gets all the sections for a given provider
        /// </summary>
        /// <param name="Provider"></param>
        /// <param name="Properties"></param>
        /// <returns></returns>
        public static ArrayList GetSections(ProviderType Provider, IMLHashItem Properties)
        {
            DbProvider myProvider = GetProvider(Provider, null, Properties);
            return myProvider.GetSections(Properties);
        }
        #endregion

        #region public static bool DeleteSection(ProviderType Provider, string SectionName, IMLHashItem Properties)
        /// <summary>
        /// Deletes a database for the given provider
        /// </summary>
        /// <param name="Provider"></param>
        /// <param name="SectionName"></param>
        /// <param name="Properties"></param>
        /// <returns></returns>
        public static bool DeleteSection(ProviderType Provider, string SectionName, IMLHashItem Properties)
        {
            DbProvider myProvider = GetProvider(Provider, null, Properties);
            return myProvider.DeleteSection(SectionName, Properties);
        }
        #endregion
    } 
    #endregion
}
