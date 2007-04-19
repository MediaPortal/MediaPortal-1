using System;
using MediaLibrary;
using System.Data;
using System.Text;

namespace SQLiteLibrary
{
    public class MLDataSet : IMLDataSet
    {
        private DataSet _Dataset;
        public IMLItemDataSource _Database;

        #region public DataSet Dataset
        /// <summary>
        /// Get/Sets the Dataset of the MLDataSet
        /// </summary>
        /// <value></value>
        public DataSet Dataset
        {
            get { return _Dataset; }
            set { _Dataset = value; }
        }
        #endregion

        #region private bool IsInitiated
        /// <summary>
        /// Gets the IsInitiated of the MLDataSet
        /// </summary>
        /// <value></value>
        private bool IsInitiated
        {
            get { return (_Database != null) && (_Dataset != null); }
        }
        #endregion

        #region public MLDataSet()
        /// <summary>
        /// Initializes a new instance of the <b>MLDataSet</b> class.
        /// </summary>
        public MLDataSet()
        {
            _Dataset = null;
            _Database = null;
        }
        #endregion

        #region public void Reload()
        /// <summary>
        /// 
        /// </summary>
        public void Reload()
        {
            _Database.ReloadDataSet(this);
        }
        #endregion

        #region public bool SaveChanges()
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SaveChanges()
        {
            if (IsInitiated)
                return _Database.UpdateDataSet(this._Dataset);
            return false;
        }
        #endregion

    }
}
