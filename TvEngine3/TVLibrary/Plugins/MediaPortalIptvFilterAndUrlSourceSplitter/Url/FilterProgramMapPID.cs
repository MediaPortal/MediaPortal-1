using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class for filtering program elements in transport stream program map with specified PID.
    /// </summary>
    internal class FilterProgramMapPID
    {
        #region Private fields

        private int programMapPID;
        private ProgramElementCollection programElements;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="FilterProgramMapPID"/> class.
        /// </summary>
        public FilterProgramMapPID()
        {
            this.ProgramMapPID = Mpeg2TsParser.DefaultMpeg2TsProgramMapPID;
            this.AllowFilteringProgramElements = false;
            this.programElements = new ProgramElementCollection();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if filtering program elements is allowed.
        /// </summary>
        public Boolean AllowFilteringProgramElements { get; set; }

        /// <summary>
        /// Specifies the PID of packet containing transport stream program section (PMT).
        /// </summary>
        public int ProgramMapPID
        {
            get { return this.programMapPID; }
            set
            {
                if (((value < 0) || (value > FilterProgramMapPID.MaximumProgramMapPID)) && (value != Mpeg2TsParser.DefaultMpeg2TsProgramMapPID))
                {
                    throw new ArgumentOutOfRangeException("ProgramMapPID", value, "Must be greater than or equal to zero and lower than 8192.");
                }

                this.programMapPID = value;
            }
        }

        /// <summary>
        /// Gets program elements in stream.
        /// </summary>
        public ProgramElementCollection ProgramElements
        {
            get { return this.programElements; }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Maximum program map PID.
        /// </summary>
        public const int MaximumProgramMapPID = 0x1FFF;

        #endregion
    }
}
