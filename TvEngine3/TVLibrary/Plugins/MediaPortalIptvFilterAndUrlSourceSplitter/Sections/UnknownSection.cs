using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal class UnknownSection : Section
    {
        #region Private fields
        #endregion

        #region Constructors

        public UnknownSection()
            : base()
        {
        }

        #endregion

        #region Properties

        public override string ShortName
        {
            get { return "Unknown"; }
        }

        #endregion

        #region Methods

        public override void Parse(byte[] sectionData)
        {
            base.Parse(sectionData);
        }

        protected override bool CheckTableId()
        {
            return true;
        }

        public override List<string> ToHumanReadable(string indent)
        {
            List<String> result = new List<string>();

            result.Add(String.Format("{0}Unknown Section", indent));
            result.Add(String.Format("{0}Section size: {1}", indent, this.SectionSize));
            result.Add(String.Format("{0}Table ID: {1}", indent, this.TableId));
            result.Add(String.Format("{0}Section syntax indicator: {1}", indent, this.SectionSyntaxIndicator));
            result.Add(String.Format("{0}Private indicator: {1}", indent, this.PrivateIndicator));
            result.Add(String.Format("{0}Section length: {1}", indent, this.SectionLength));

            result.Add(String.Format("{0}CRC32: 0x{1:X8}", indent, this.Crc32)); 
            return result;
        }

        #endregion

        #region Constants
        #endregion
    }
}
