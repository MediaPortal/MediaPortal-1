using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal sealed class ProgramAssociationSectionProgram
    {
        #region Constructors

        public ProgramAssociationSectionProgram(uint programNumber, uint programMapPID)
        {
            this.ProgramNumber = programNumber;
            this.ProgramMapPID = programMapPID;
        }

        #endregion

        #region Properties

        public uint ProgramNumber { get; private set; }

        public uint ProgramMapPID { get; private set; }

        #endregion
    }
}
