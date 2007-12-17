
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLibrary
{
    #region public interface IMLImportPlugin
    /// <summary>
    /// The IMLImportPlugin is the interface you must implement for import plugins.
    /// </summary>
    public interface IMLImportPlugin : IMLPlugin
    {

        #region bool Import(IMLSection Section, IMLImportProgress Progress )
        /// <summary>
        /// Runs an Import on the specified Section
        /// </summary>
        /// <param name="Section">The IMLSection object used to for importing</param>
        /// <param name="Progress">The IMLImportProgress object used for reporting progress to the system</param>
        /// <returns></returns>
        bool Import(IMLSection Section, IMLImportProgress Progress);
        #endregion


    }
    #endregion
}
