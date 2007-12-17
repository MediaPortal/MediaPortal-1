

namespace MediaLibrary
{
    #region public interface IMLImports
    /// <summary>
    /// The IMLImports collection contains a list of all defined imports for the Media Library
    /// </summary>
    public interface IMLImports
    {
        #region IMLImports Properties

        #region int Count
        /// <summary>
        /// Gets the number of items present in the IMLImports collection
        /// </summary>
        /// <value></value>
        int Count
        {
            get;
        }
        #endregion

        #endregion

        #region IMLImports Methods

        #region IMLImport AddNewImport()
        /// <summary>
        /// Creates a new IMLImport object and adds it to the IMLImports collection
        /// </summary>
        /// <returns></returns>
        IMLImport AddNewImport();
        #endregion

        #region void DeleteImport(IMLImport Import)
        /// <summary>
        /// Deletes the speficied IMLImport from the IMLImports collection
        /// </summary>
        /// <param name="Import">IMLImport object to delete</param>
        void DeleteImport(IMLImport Import);
        #endregion

        #region IMLImport FindImport(int ImportID)
        /// <summary>
        /// Locates, using the ID specified, an import in the IMLImports collection and returns it
        /// </summary>
        /// <param name="ImportID"></param>
        /// <returns></returns>
        IMLImport FindImport(int ImportID);
        #endregion

        #region IMLImport Imports(int Index)
        /// <summary>
        /// Returns element Index of the IMLImports collection
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        IMLImport Imports(int Index);
        #endregion

        #endregion

    }
    #endregion
}