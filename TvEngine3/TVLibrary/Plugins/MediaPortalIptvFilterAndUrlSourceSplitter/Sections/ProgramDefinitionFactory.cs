using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.StreamSections
{
    internal static class ProgramDefinitionFactory
    {
        public static ProgramDefinition CreateProgramDefinition(Byte[] data, int start)
        {
            // insert more specific program definitions

            ProgramDefinition general = new ProgramDefinition();
            general.Parse(data, start);

            return general;
        }
    }
}
