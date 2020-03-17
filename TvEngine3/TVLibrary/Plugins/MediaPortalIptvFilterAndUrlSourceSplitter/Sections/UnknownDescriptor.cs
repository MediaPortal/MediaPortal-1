using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal class UnknownDescriptor : Descriptor
    {
        #region Private fields
        #endregion

        #region Constructors

        public UnknownDescriptor()
            : base()
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Methods

        protected override bool CheckTag()
        {
            return true;
        }

        public override List<string> ToHumanReadable(string indent)
        {
            List<String> result = new List<string>();

            result.Add(String.Format("{0}Tag: {1} (0x{1:X2}, Unknown)", indent, this.Tag));
            result.Add(String.Format("{0}Descriptor size: {1}", indent, this.Payload.Length));

            return result;
        }

        #endregion
    }
}
