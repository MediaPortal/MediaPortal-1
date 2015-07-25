using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.StreamSections;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal sealed class TransportStreamProgramMapSection : Section
    {
        #region Private fields
        #endregion

        #region Constructors

        public TransportStreamProgramMapSection()
            : base()
        {
            this.Programs = new ProgramDefinitionCollection();
            this.Descriptors = new DescriptorCollection();
            this.ProgramMapPID = 0;
            this.ProgramNumber = 0;
            this.VersionNumber = 0;
            this.CurrentNextIndicator = false;
            this.SectionNumber = 0;
            this.LastSectionNumber = 0;
            this.PcrPID = 0;
        }

        #endregion

        #region Properties

        public uint ProgramNumber { get; private set; }

        public uint VersionNumber { get; private set; }

        public Boolean CurrentNextIndicator { get; private set; }

        public uint SectionNumber { get; private set; }

        public uint LastSectionNumber { get; private set; }

        public uint PcrPID { get; private set; }

        public DescriptorCollection Descriptors { get; private set; }

        public ProgramDefinitionCollection Programs { get; private set; }

        public uint ProgramMapPID { get; set; }

        public override string ShortName
        {
            get { return TransportStreamProgramMapSection.GetShortName(this.ProgramMapPID, this.ProgramNumber); }
        }

        #endregion

        #region Methods

        public override void Parse(byte[] sectionData)
        {
            this.ProgramNumber = 0;
            this.VersionNumber = 0;
            this.CurrentNextIndicator = false;
            this.SectionNumber = 0;
            this.LastSectionNumber = 0;
            this.PcrPID = 0;
            this.Descriptors.Clear();
            this.Programs.Clear();

            base.Parse(sectionData);

            int position = (int)Section.HeaderSize;

            this.ProgramNumber = this.payload.ReadBigEndian16(position);
            position += 2;

            uint reservedVersionNumberCurrentNextIndicator = this.payload.ReadBigEndian8(position);
            position++;

            this.VersionNumber = ((reservedVersionNumberCurrentNextIndicator >> TransportStreamProgramMapSection.VersionNumberShift) & TransportStreamProgramMapSection.VersionNumberMask);
            this.CurrentNextIndicator = (((reservedVersionNumberCurrentNextIndicator >> TransportStreamProgramMapSection.CurrentNextIndicatorShift) & TransportStreamProgramMapSection.CurrentNextIndicatorMask) != 0);

            this.SectionNumber = this.payload.ReadBigEndian8(position);
            position++;

            this.LastSectionNumber = this.payload.ReadBigEndian8(position);
            position++;

            this.PcrPID = ((this.payload.ReadBigEndian16(position) >> TransportStreamProgramMapSection.PcrPIDShift) & TransportStreamProgramMapSection.PcrPIDMask);
            position += 2;

            // parse descriptors (if needed)
            uint descriptorSize = this.payload.ReadBigEndian16(position);
            position += 2;
            
            descriptorSize = ((descriptorSize >> TransportStreamProgramMapSection.DescriptorsSizeShift) & TransportStreamProgramMapSection.DescriptorsSizeMask);

            if (descriptorSize != 0)
            {
                uint processed = 0;

                while (processed < descriptorSize)
                {
                    Descriptor descriptor = DescriptorFactory.CreateDescriptor(this.payload, position + (int)processed);

                    this.Descriptors.Add(descriptor);
                    processed += descriptor.Size;
                }

                position += (int)processed;
            }
            
            // program definition size is not constant (it depends on ES info size)
            while (position < (this.SectionSize - Section.Crc32Size))
            {
                ProgramDefinition programDefinition = ProgramDefinitionFactory.CreateProgramDefinition(this.payload, position);

                this.Programs.Add(programDefinition);
                position += (int)programDefinition.Size;
            }
        }

        protected override bool CheckTableId()
        {
            return (this.TableId == TransportStreamProgramMapSection.SectionTableId);
        }

        public override List<string> ToHumanReadable(string indent)
        {
            List<String> result = new List<string>();

            result.Add(String.Format("{0}Transport Stream Program Map Section (PMT) - PID {1} (0x{1:X4})", indent, this.ProgramMapPID));
            result.Add(String.Format("{0}Section size: {1}", indent, this.SectionSize));
            result.Add(String.Format("{0}Table ID: {1}", indent, this.TableId));
            result.Add(String.Format("{0}Section syntax indicator: {1}", indent, this.SectionSyntaxIndicator));
            result.Add(String.Format("{0}Private indicator: {1}", indent, this.PrivateIndicator));
            result.Add(String.Format("{0}Section length: {1}", indent, this.SectionLength));
            result.Add(String.Format("{0}Program number: {1} (0x{1:X4})", indent, this.ProgramNumber));
            result.Add(String.Format("{0}Version number: {1}", indent, this.VersionNumber));
            result.Add(String.Format("{0}Current next indicator: {1}", indent, this.CurrentNextIndicator));
            result.Add(String.Format("{0}Section number: {1}", indent, this.SectionNumber));
            result.Add(String.Format("{0}Last section number: {1}", indent, this.LastSectionNumber));
            result.Add(String.Format("{0}PCR PID: {1} (0x{1:X4})", indent, this.PcrPID));
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

            result.Add(String.Format("{0}Program elements: {1}", indent, this.Programs.Count));
            foreach (var program in this.Programs)
            {
                result.Add(String.Empty);
                result.Add(String.Format("{0}Program:", indent + Section.Indent));
                result.AddRange(program.ToHumanReadable(indent + Section.Indent + Section.Indent));
            }

            if (this.Programs.Count != 0)
            {
                result.Add(String.Empty);
            }

            result.Add(String.Format("CRC32: 0x{0:X8}", this.Crc32));

            return result;
        }

        public static String GetShortName(uint programMapPID, uint programNumber)
        {
            return String.Format("PMT - PID {0} (0x{0:X4}) Program number {1} (0x{1:X4})", programMapPID, programNumber);
        }

        #endregion

        #region Contants

        public static readonly uint SectionTableId = 0x00000002;

        public static readonly uint VersionNumberMask = 0x0000001F;
        public const int VersionNumberShift = 1;

        public static readonly uint CurrentNextIndicatorMask = 0x00000001;
        public const int CurrentNextIndicatorShift = 0;

        public static readonly uint PcrPIDMask = 0x00001FFF;
        public const int PcrPIDShift = 0;

        public static readonly uint DescriptorsSizeMask = 0x00000FFF;
        public const int DescriptorsSizeShift = 0;

        #endregion
    }
}
