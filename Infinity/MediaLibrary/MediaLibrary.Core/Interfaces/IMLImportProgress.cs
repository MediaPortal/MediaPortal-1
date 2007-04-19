

using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLibrary
{
    #region public interface IMLImportProgress
    /// <summary>
    /// The IMLImportProgress provides feedback about the running status of the import
    /// </summary>
    public interface IMLImportProgress
    {
        #region bool Progress(int PercentComplete, string Text)
        /// <summary>
        /// Reports the progress of an import to the UI
        /// </summary>
        /// <param name="PercentComplete">The percentage value at which the current import is</param>
        /// <param name="Text">Message set by the import plugin giving information about the import progress</param>
        /// <returns></returns>
        bool Progress(int PercentComplete, string Text);
        #endregion
    }
    #endregion
}
