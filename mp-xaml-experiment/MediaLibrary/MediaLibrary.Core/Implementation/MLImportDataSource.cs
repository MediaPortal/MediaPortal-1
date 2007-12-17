using System;
using MediaLibrary;
using MediaLibrary.Database;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Collections;

namespace MediaLibrary
{
    internal class MLImportDataSource : IMLImportDataSource
    {
        private DbProvider sqlProvider;

        public MLImportDataSource(DbProvider sqlProvider)
        {
            this.sqlProvider = sqlProvider;
        }
        
        #region Methods

        public IMLImports GetImports()
        {
            IMLImports Imports = ImportDAL.GetImports(sqlProvider.CreateCommand());
            if (Imports != null)
                for (int i = 0; i < Imports.Count; i++)
                {
                    MLImport Import = Imports.Imports(i) as MLImport;
                    Import.PluginProperties = ImportDAL.GetImportProperties(sqlProvider.CreateCommand(), Import.ID);
                }
            return Imports;
        }

        public bool AddNewImport(IMLImport Import)
        {
            if (ImportDAL.InsertImport(sqlProvider.CreateCommand(), Import))
            {
                ((MLImport)Import).ID = ImportDAL.GetMaxImportID(sqlProvider.CreateCommand());
                if (Import.ID > 0)
                    return ImportDAL.InsertImportProperties(sqlProvider.CreateCommand(), Import);
            }
            return false;
        }

        public bool UpdateImport(IMLImport Import)
        {
            if (ImportDAL.UpdateImport(sqlProvider.CreateCommand(), Import))
            {
                return ImportDAL.DeleteImportProperties(sqlProvider.CreateCommand(), Import)
                    && ImportDAL.InsertImportProperties(sqlProvider.CreateCommand(), Import);
            }
            return false;
        }

        public bool DeleteImport(IMLImport Import)
        {
            return ImportDAL.DeleteImport(sqlProvider.CreateCommand(), Import);
        }

        #endregion
    }
}
