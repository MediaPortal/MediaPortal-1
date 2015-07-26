using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class for filtering program elements in transport stream program map with specified program number.
    /// </summary>
    internal class FilterProgramNumber
    {
        #region Private fields

        private int programNumber;
        private ProgramElementCollection programElements;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="FilterProgramNumber"/> class.
        /// </summary>
        public FilterProgramNumber(int programNumber)
        {
            this.ProgramNumber = programNumber;
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
        /// Gets or sets the transport stream program map section program number.
        /// </summary>
        public int ProgramNumber
        {
            get { return this.programNumber; }
            set
            {
                if ((value < FilterProgramNumber.MinimumProgramNumber) || (value > FilterProgramNumber.MaximumProgramNumber))
                {
                    throw new ArgumentOutOfRangeException("ProgramNumber", value, "Must be greater than zero and lower than 65536.");
                }

                this.programNumber = value;
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
        /// The minimum program number.
        /// </summary>
        public const int MinimumProgramNumber = 0x0001;

        /// <summary>
        /// The maximum program number.
        /// </summary>
        public const int MaximumProgramNumber = 0xFFFF;

        #endregion
    }
}
