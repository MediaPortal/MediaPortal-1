using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLibrary
{
    internal interface IMLImportDataSource
    {
        #region IMLImportDataSource Methods

        IMLImports GetImports();

        bool UpdateImport(IMLImport Item);

        bool AddNewImport(IMLImport Item);

        bool DeleteImport(IMLImport Item);

        #endregion
    }
}
