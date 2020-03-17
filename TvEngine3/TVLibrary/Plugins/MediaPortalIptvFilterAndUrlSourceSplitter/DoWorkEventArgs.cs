using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents class for do work event args.
    /// </summary>
    /// <threadsafety static="true" instance="false" />
    public class DoWorkEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DoWorkEventArgs"/> class with specified finished flag and exception.
        /// </summary>
        /// <param name="finished"><see langword="true"/> if operation is finished, <see langword="false"/> otherwise.</param>
        /// <param name="exception">The exception occurred while executing operation. Can be <see langword="null"/> is not exception occurred.</param>
        public DoWorkEventArgs(Boolean finished, Exception exception)
        {
            this.IsFinished = finished;
            this.Exception = exception;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if executing operation is finished.
        /// </summary>
        /// <value><see langword="true"/> if operation is finished, <see langword="false"/> otherwise.</value>
        public Boolean IsFinished { get; set; }

        /// <summary>
        /// Tests if exception occurred while executing operation.
        /// </summary>
        /// <value><see langword="true"/> if exception occurred, <see langword="false"/> otherwise.</value>
        public Boolean IsError { get { return (this.Exception != null); } }

        /// <summary>
        /// Gets or sets the exception which occurred while executing operation.
        /// </summary>
        /// <value><see langword="null"/> if no exception occurred.</value>
        public Exception Exception { get; set; }

        #endregion
    }
}
