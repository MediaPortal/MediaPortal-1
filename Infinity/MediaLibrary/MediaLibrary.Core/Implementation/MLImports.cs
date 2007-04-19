using System;
using System.Collections;
using System.Text;

namespace MediaLibrary
{
    internal class MLImports : IMLImports
    {
        public ArrayList _Imports;
        private IMediaLibrary _Library;

        public IMediaLibrary Library
        {
            get { return _Library; }
            set
            {
                foreach (MLImport Import in _Imports)
                    Import._Library = value;
                _Library = value;
            }
        }
        
        private IMLImportDataSource _Database;
        internal IMLImportDataSource Database
        {
            get { return _Database; }
            set 
            {
                foreach (MLImport Import in _Imports)
                    Import._Database = value;
                _Database = value; 
            }
        }

        #region Properties

        public int Count
        {
            get { return _Imports.Count; }
        }

        #endregion

        #region Constructors

        public MLImports()
        {
            this._Imports = new ArrayList();
            this._Library = null;
            this._Database = null;
        }

        #endregion

        #region Methods

        public IMLImport AddNewImport()
        {
            MLImport NewImport = new MLImport();
            NewImport._Database = this._Database;
            NewImport._Library = this._Library;
            _Imports.Add(NewImport);
            return NewImport as IMLImport;
        }

        public void DeleteImport(IMLImport Import)
        {
            if (Import != null && Import.ID > 0)
                if(this._Database.DeleteImport(Import))
                    this._Imports.Remove(Import);
        }

        public IMLImport FindImport(int ImportID)
        {
            foreach (IMLImport Import in this._Imports)
                if (Import.ID == ImportID)
                    return Import;
            return null;
        }

        public IMLImport Imports(int Index)
        {
            if (Index >= 0 && Index < Count)
                return _Imports[Index] as IMLImport;
            return null;
        }

        #endregion

    }
}