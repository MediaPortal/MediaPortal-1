using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents class for finished work event args.
    /// </summary>
    /// <threadsafety static="true" instance="false" />
    public class FinishedWorkEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="FinishedWorkEventArgs"/> class with specified canceled flag and exception.
        /// </summary>
        /// <param name="canceled"><see langword="true"/> if operation was canceled, <see langword="false"/> otherwise.</param>
        /// <param name="exception">The exception occurred while executing operation. Can be <see langword="null"/> is no exception occurred.</param>
        public FinishedWorkEventArgs(Boolean canceled, Exception exception)
        {
            this.IsCanceled = canceled;
            this.Exception = exception;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tests if executing operation was canceled.
        /// </summary>
        /// <value><see langword="true"/> if operation was canceled, <see langword="false"/> otherwise.</value>
        public Boolean IsCanceled { get; protected set; }

        /// <summary>
        /// Tests if exception occurred while executing operation.
        /// </summary>
        /// <value><see langword="true"/> if exception occurred, <see langword="false"/> otherwise.</value>
        public Boolean IsError { get { return (this.Exception != null); } }

        /// <summary>
        /// Gets the exception which occurred while executing operation.
        /// </summary>
        /// <value><see langword="null"/> if no exception occurred.</value>
        public Exception Exception { get; protected set; }

        #endregion
    }
}
