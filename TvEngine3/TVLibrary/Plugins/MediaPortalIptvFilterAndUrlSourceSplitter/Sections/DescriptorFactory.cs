using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal static class DescriptorFactory
    {
        #region Methods

        public static Descriptor CreateDescriptor(Byte[] data, int position)
        {
            try
            {
                ConditionalAccessDescriptor descriptor = new ConditionalAccessDescriptor();

                descriptor.Parse(data, position);

                return descriptor;
            }
            catch (InvalidDescriptorTagException)
            {
            }

            try
            {
                UnknownDescriptor descriptor = new UnknownDescriptor();

                descriptor.Parse(data, position);

                return descriptor;
            }
            catch (InvalidDescriptorTagException)
            {
            }

            return null;
        }

        #endregion
    }
}
