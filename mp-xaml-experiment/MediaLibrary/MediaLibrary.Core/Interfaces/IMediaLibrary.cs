namespace MediaLibrary
{
    /// <summary>
    /// The IMediaLibrary object provides access to the Media Library. Use it to access any section and its importers.
    /// </summary>
    public interface IMediaLibrary
    {
        IMLSystem SystemObject
        {
            get;
        }

        /// <summary>
        /// Deletes the section named SectionName from the Media Library. It also deletes any imports for the section.
        /// </summary>
        /// <param name="SectionName"></param>
        /// <returns></returns>
        bool DeleteSection(string SectionName);

        /// <summary>
        /// Returns an IMLSection object corresponding to the SectionName specified. If CreateIt is set to TRUE and the section named SectionName is not found in the Media Library it will be created. This function can return a NULL value, so always check it.
        /// </summary>
        /// <param name="SectionName"></param>
        /// <param name="CreateIt"></param>
        /// <returns></returns>
        IMLSection FindSection(string SectionName, bool CreateIt);

        /// <summary>
        /// Gets the list of imports defined for the Media Library
        /// </summary>
        /// <returns></returns>
        IMLImports GetImports();

        /// <summary>
        /// Executes the specified import.
        /// </summary>
        /// <param name="ImportID">ID number of the import to execute</param>
        /// <param name="Progress">IMLImportProgress object that returns information while the import is running. You can specify a NULL Progress object.</param>
        /// <param name="ErrorText">If the import fails, this variable will hold an informative message about why it failed</param>
        /// <returns>This function returns TRUE if the import was succesfully completed</returns>
        bool RunImport(int ImportID, IMLImportProgress Progress, out string ErrorText);

        /// <summary>
        /// Returns the number of sections defined in the Media Library
        /// </summary>
        int SectionCount
        {
            get;
        }

        // TODO: This needs a get{} accessor
        /// <summary>
        /// Returns the section name for the specified Index
        /// </summary>
        string Sections(int Index);

    }
}

