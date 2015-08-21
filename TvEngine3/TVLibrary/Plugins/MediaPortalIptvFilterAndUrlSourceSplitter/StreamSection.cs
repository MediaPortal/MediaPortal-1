using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    internal sealed class StreamSection
    {
        #region Private fields
        #endregion

        #region Constructors

        public StreamSection(Section section)
        {
            this.Section = section;
            this.StreamSections = new StreamSectionCollection();
        }

        #endregion

        #region Properties

        public Section Section { get; protected set; }

        public StreamSectionCollection StreamSections { get; protected set; }

        #endregion

        #region Methods
        #endregion

        #region Constructors
        #endregion
    }
}
