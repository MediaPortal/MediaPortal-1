using System;
using System.Data;
using System.Text;

namespace MediaLibrary
{
    //TODO change this to a DataTable, A DataSet is too much to expose
    #region public interface IMLDataSet
    /// <summary>
    /// The IMLDataSet provides a basic DataSet to every item in the 
    /// library.  Operations are limited to items.  Tags can
    /// not be added using a DataSet.
    /// </summary>
    public interface IMLDataSet
    {
        #region Properties

        #region DataSet Dataset
        /// <summary>
        /// Returns a DataSet of all items in the library.
        /// </summary>
        /// <value></value>
        DataSet Dataset
        {
            get;
        }
        #endregion

        #endregion

        #region IMLDataSet Methods

        #region void Reload()
        /// <summary>
        /// Loads changes made to library that the DataSet can not
        /// perform such as adding Tags to the library.
        /// </summary>
        void Reload();
        #endregion

        #region bool SaveChanges()
        /// <summary>
        /// Saves all changes made to the DataSet back to the library
        /// </summary>
        /// <returns>Returns true if library was updated successfully</returns>
        bool SaveChanges();
        #endregion

        #endregion
    }
    #endregion
}
