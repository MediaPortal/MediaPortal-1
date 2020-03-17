using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class for program element for transport stream program map section.
    /// </summary>
    internal class ProgramElement
    {
        #region Private fields

        private int programElementPID;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ProgramElement"/> class.
        /// </summary>
        public ProgramElement()
        {
            this.ProgramElementPID = 0;
            this.LeaveProgramElement = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Specifies the program element PID.
        /// </summary>
        public int ProgramElementPID
        {
            get { return this.programElementPID; }
            set
            {
                if (((value < 0) || (value > ProgramElement.MaximumProgramElementPID)))
                {
                    throw new ArgumentOutOfRangeException("ProgramElementPID", value, "Must be greater than or equal to zero and lower than 8192.");
                }

                this.programElementPID = value;
            }
        }

        /// <summary>
        /// Specifies if program element have to be left in stream.
        /// </summary>
        public Boolean LeaveProgramElement { get; set; }

        #endregion

        #region Constants

        /// <summary>
        /// Maximum program element PID.
        /// </summary>
        public const int MaximumProgramElementPID = 0x1FFF;

        #endregion
    }
}
