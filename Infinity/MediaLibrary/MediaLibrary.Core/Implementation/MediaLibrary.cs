using System;
using System.IO;
using System.Collections;
using MediaLibrary;
using MediaLibrary.Database;
using MediaLibrary.Configuration;

namespace MediaLibrary
{
    #region public class MediaLibraryClass : IMediaLibrary
    /// <summary>
    /// MediaLibraryClass will load the required database plugin and use it for data access
    /// </summary>
    public class MediaLibraryClass : IMediaLibrary
    {
        #region Members

        private MLSystem _SystemObject;
        private IMLDatabasePlugin DbPlugin;
        private MLPluginDescription DbPluginDesc;

        #endregion

        #region Properties

        #region public IMLSystem SystemObject
        /// <summary>
        /// Gets the SystemObject of the MediaLibraryClass
        /// </summary>
        /// <value></value>
        public IMLSystem SystemObject
        {
            get { return _SystemObject; }
        }
        #endregion

        #region public int SectionCount
        /// <summary>
        /// Gets the SectionCount of the MediaLibraryClass
        /// </summary>
        /// <value></value>
        public int SectionCount
        {
            get
            {
                if (HasPlugin)
                    return this.DbPlugin.SectionCount;
                return 0;
            }
        }
        #endregion

        #region private bool HasPlugin
        /// <summary>
        /// Gets the HasPlugin of the MediaLibraryClass
        /// </summary>
        /// <value></value>
        private bool HasPlugin
        {
            get { return DbPlugin != null; }
        }
        #endregion

        #endregion

        #region Constructors

        #region public MediaLibraryClass()
        /// <summary>
        /// Initializes a new instance of the <b>MediaLibraryClass</b> class.
        /// </summary>
        public MediaLibraryClass()
        {
            LoadMediaLibrary(AppDomain.CurrentDomain.BaseDirectory);
        }
        #endregion

        #region public MediaLibraryClass(string ConfigPath)
        /// <summary>
        /// Initializes a new instance of the <b>MediaLibraryClass</b> class.
        /// </summary>
        /// <param name="ConfigPath"></param>
        public MediaLibraryClass(string ConfigPath)
        {
            LoadMediaLibrary(ConfigPath);
        }
        #endregion

        #endregion

        #region Methods

        #region Public Methods

        #region public bool DeleteSection(string section)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public bool DeleteSection(string section)
        {
            if (HasPlugin)
                return this.DbPlugin.DeleteSection(section);
            return false;
        }
        #endregion

        #region public IMLSection FindSection(string Section, bool Create)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Create"></param>
        /// <returns></returns>
        public IMLSection FindSection(string Section, bool Create)
        {
            if (HasPlugin)
                return this.DbPlugin.FindSection(Section, Create);
            return null;
        }
        #endregion

        #region public IMLImports GetImports()
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMLImports GetImports()
        {
            string path = Path.Combine(SystemObject.GetLibraryDirectory(null), "library.imports");
            DbProvider myProvider = DbFactory.GetProvider(ProviderType.SQLite, path, null);
            IMLImportDataSource db = new MLImportDataSource(myProvider) as IMLImportDataSource;
            MLImports ips = db.GetImports() as MLImports;
            ips.Library = this;
            ips.Database = db;
            return ips as IMLImports;
        }
        #endregion

        #region public bool RunImport(int ImportID, IMLImportProgress Progress, out string ErrorText)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ImportID"></param>
        /// <param name="Progress"></param>
        /// <param name="ErrorText"></param>
        /// <returns></returns>
        public bool RunImport(int ImportID, IMLImportProgress Progress, out string ErrorText)
        {
            ErrorText = "";
            return true;
        }
        #endregion

        #region public string Sections(int Index)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public string Sections(int Index)
        {
            if (HasPlugin)
                return this.DbPlugin.Sections(Index);
            return string.Empty;
        }
        #endregion

        #endregion

        #region Private Methods

        #region private void LoadMediaLibrary(string ConfigPath)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConfigPath"></param>
        private void LoadMediaLibrary(string ConfigPath)
        {
            string SetPropertiesError;
            if (string.IsNullOrEmpty(ConfigPath))
                ConfigPath = AppDomain.CurrentDomain.BaseDirectory;
            _SystemObject = MLSystem.Deserialize(ConfigPath);
            IMLHashItemList pluginlist = SystemObject.GetInstalledPlugins("database");
            for (int i = 0; i < pluginlist.Count; i++)
            {
                IMLHashItem item = pluginlist[i];
                MLPluginDescription desc = item["description"] as MLPluginDescription;
                if (desc != null && desc.information.plugin_id == (Guid)_SystemObject.Configuration.ActivePlugin.description["plugin_id"])
                {
                    DbPluginDesc = desc;
                    Type plugintype = MLSystem.GetTypeFromPlugin((string)item["path"]);
                    DbPlugin = (IMLDatabasePlugin)Activator.CreateInstance(plugintype);
                    MLExtentsions.SetProperties(DbPlugin, _SystemObject.Configuration.ActivePlugin.plugin_properties, out SetPropertiesError);
                    break;
                }
            }
        }
        #endregion

        #endregion

        #endregion
    } 
    #endregion
}
