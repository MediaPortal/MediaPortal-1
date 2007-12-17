using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLibrary
{
    public interface IMLDatabasePlugin : IMLPlugin
    {
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
