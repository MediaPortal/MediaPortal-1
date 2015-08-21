using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal static class SectionFactory
    {
        #region Methods

        public static Section CreateSection(Byte[] sectionData)
        {
            try
            {
                ProgramAssociationSection section = new ProgramAssociationSection();

                section.Parse(sectionData);

                return section;
            }
            catch (InvalidTableIdException)
            {
            }

            try
            {
                TransportStreamProgramMapSection section = new TransportStreamProgramMapSection();

                section.Parse(sectionData);

                return section;
            }
            catch (InvalidTableIdException)
            {
            }

            try
            {
                ConditionalAccessSection section = new ConditionalAccessSection();

                section.Parse(sectionData);

                return section;
            }
            catch (InvalidTableIdException)
            {
            }

            try
            {
                UnknownSection section = new UnknownSection();

                section.Parse(sectionData);

                return section;
            }
            catch (InvalidTableIdException)
            {
            }

            return null;
        }

        #endregion
    }
}
