using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal class ConditionalAccessSection : Section
    {
        #region Private fields
        #endregion

        #region Constructors

        public ConditionalAccessSection()
            : base()
        {
            this.Descriptors = new DescriptorCollection();
            this.VersionNumber = 0;
            this.CurrentNextIndicator = false;
            this.SectionNumber = 0;
            this.LastSectionNumber = 0;
        }

        #endregion

        #region Properties

        public override string ShortName
        {
            get { return "CAT"; }
        }

        public uint VersionNumber { get; private set; }

        public Boolean CurrentNextIndicator { get; private set; }

        public uint SectionNumber { get; private set; }

        public uint LastSectionNumber { get; private set; }

        public DescriptorCollection Descriptors { get; private set; }

        #endregion

        #region Methods

        public override void Parse(byte[] sectionData)
        {
            this.VersionNumber = 0;
            this.CurrentNextIndicator = false;
            this.SectionNumber = 0;
            this.LastSectionNumber = 0;
            this.Descriptors.Clear();

            base.Parse(sectionData);

            int position = (int)Section.HeaderSize;

            position += 2;

            uint reservedVersionNumberCurrentNextIndicator = this.payload.ReadBigEndian8(position);
            position++;

            this.VersionNumber = ((reservedVersionNumberCurrentNextIndicator >> TransportStreamProgramMapSection.VersionNumberShift) & TransportStreamProgramMapSection.VersionNumberMask);
            this.CurrentNextIndicator = (((reservedVersionNumberCurrentNextIndicator >> TransportStreamProgramMapSection.CurrentNextIndicatorShift) & TransportStreamProgramMapSection.CurrentNextIndicatorMask) != 0);

            this.SectionNumber = this.payload.ReadBigEndian8(position);
            position++;

            this.LastSectionNumber = this.payload.ReadBigEndian8(position);
            position++;

            uint processed = 0;
            uint descriptorSize = this.SectionSize - (uint)position - Section.Crc32Size;

            while (processed < descriptorSize)
            {
                Descriptor descriptor = DescriptorFactory.CreateDescriptor(this.payload, position + (int)processed);

                this.Descriptors.Add(descriptor);
                processed += descriptor.Size;
            }

            position += (int)processed;
        }

        protected override bool CheckTableId()
        {
            return (this.TableId == ConditionalAccessSection.SectionTableId);
        }

        public override List<string> ToHumanReadable(string indent)
        {
            List<String> result = new List<string>();

            result.Add(String.Format("{0}Conditional Access Section", indent));
            result.Add(String.Format("{0}Section size: {1}", indent, this.SectionSize));
            result.Add(String.Format("{0}Table ID: {1}", indent, this.TableId));
            result.Add(String.Format("{0}Section syntax indicator: {1}", indent, this.SectionSyntaxIndicator));
            result.Add(String.Format("{0}Private indicator: {1}", indent, this.PrivateIndicator));
            result.Add(String.Format("{0}Section length: {1}", indent, this.SectionLength));
            result.Add(String.Format("{0}Version number: {1}", indent, this.VersionNumber));
            result.Add(String.Format("{0}Current next indicator: {1}", indent, this.CurrentNextIndicator));
            result.Add(String.Format("{0}Section number: {1}", indent, this.SectionNumber));
            result.Add(String.Format("{0}Last section number: {1}", indent, this.LastSectionNumber));
            result.Add(String.Format("{0}Descriptors: {1}", indent, this.Descriptors.Count));

            foreach (var descriptor in this.Descriptors)
            {
                result.Add(String.Empty);
                result.Add(String.Format("{0}Descriptor:", indent + Section.Indent));
                result.AddRange(descriptor.ToHumanReadable(indent + Section.Indent + Section.Indent));
            }

            if (this.Descriptors.Count != 0)
            {
                result.Add(String.Empty);
            }

            result.Add(String.Format("{0}CRC32: 0x{1:X8}", indent, this.Crc32)); 
            return result;
        }

        #endregion

        #region Constants

        public static readonly uint SectionTableId = 0x00000001;

        #endregion

    }
}
