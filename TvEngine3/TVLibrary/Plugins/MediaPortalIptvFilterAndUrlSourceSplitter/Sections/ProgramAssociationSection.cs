using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal sealed class ProgramAssociationSection : Section
    {
        #region Private fields

        ProgramAssociationSectionProgramCollection programs;

        #endregion

        #region Constructors

        public ProgramAssociationSection()
            : base()
        {
            this.TransportStreamId = 0;
            this.VersionNumber = 0;
            this.CurrentNextIndicator = false;
            this.SectionNumber = 0;
            this.LastSectionNumber = 0;
            this.NetworkInformationTablePID = ProgramAssociationSection.NetworkInformationTablePIDUndefined;
            this.programs = new ProgramAssociationSectionProgramCollection();
        }

        #endregion

        #region Properties

        public uint TransportStreamId { get; private set; }

        public uint VersionNumber { get; private set; }

        public Boolean CurrentNextIndicator { get; private set;}

        public uint SectionNumber { get; private set; }

        public uint LastSectionNumber { get; private set; }

        public uint NetworkInformationTablePID { get; private set; }

        public ProgramAssociationSectionProgramCollection Programs { get { return this.programs; } }

        public override string ShortName
        {
            get { return "PAT"; }
        }

        #endregion

        #region Methods

        public override void Parse(byte[] sectionData)
        {
            this.TransportStreamId = 0;
            this.VersionNumber = 0;
            this.CurrentNextIndicator = false;
            this.SectionNumber = 0;
            this.LastSectionNumber = 0;
            this.Programs.Clear();
            this.NetworkInformationTablePID = ProgramAssociationSection.NetworkInformationTablePIDUndefined;

            base.Parse(sectionData);

            int position = (int)Section.HeaderSize;

            this.TransportStreamId = this.payload.ReadBigEndian16(position);
            position += 2;

            uint reservedVersionNumberCurrentNextIndicator = this.payload.ReadBigEndian8(position);
            position++;

            this.VersionNumber = ((reservedVersionNumberCurrentNextIndicator >> ProgramAssociationSection.VersionNumberShift) & ProgramAssociationSection.VersionNumberMask);
            this.CurrentNextIndicator = (((reservedVersionNumberCurrentNextIndicator >> ProgramAssociationSection.CurrentNextIndicatorShift) & ProgramAssociationSection.CurrentNextIndicatorMask) != 0);

            this.SectionNumber = this.payload.ReadBigEndian8(position);
            position++;

            this.LastSectionNumber = this.payload.ReadBigEndian8(position);
            position++;

            uint programCount = (this.SectionSize - Section.HeaderSize - Section.Crc32Size - ProgramAssociationSection.HeaderSize) / ProgramAssociationSection.ProgramSize;
            for (int i = 0; i < programCount; i++)
            {
                uint programNumber = this.payload.ReadBigEndian16(position);
                position += 2;

                uint programMapPID = (this.payload.ReadBigEndian16(position) & ProgramAssociationSection.ProgramMapPIDMask);
                position += 2;

                if (programNumber == 0)
                {
                    this.NetworkInformationTablePID = programMapPID;
                }
                else
                {
                    this.programs.Add(new ProgramAssociationSectionProgram(programNumber, programMapPID));
                }
            }
        }

        protected override bool CheckTableId()
        {
            return (this.TableId == ProgramAssociationSection.SectionTableId);
        }

        public override List<string> ToHumanReadable(string indent)
        {
            List<String> result = new List<string>();

            result.Add(String.Format("{0}Program Association Section (PAT)", indent));
            result.Add(String.Format("{0}Section size: {1}", indent, this.SectionSize));
            result.Add(String.Format("{0}Table ID: {1}", indent, this.TableId));
            result.Add(String.Format("{0}Section syntax indicator: {1}", indent, this.SectionSyntaxIndicator));
            result.Add(String.Format("{0}Private indicator: {1}", indent, this.PrivateIndicator));
            result.Add(String.Format("{0}Section length: {1}", indent, this.SectionLength));
            result.Add(String.Format("{0}Transport stream ID: {1} (0x{1:X4})", indent, this.TransportStreamId));
            result.Add(String.Format("{0}Version number: {1}", indent, this.VersionNumber));
            result.Add(String.Format("{0}Current next indicator: {1}", indent, this.CurrentNextIndicator));
            result.Add(String.Format("{0}Section number: {1}", indent, this.SectionNumber));
            result.Add(String.Format("{0}Last section number: {1}", indent, this.LastSectionNumber));
            result.Add(String.Format("{0}Network information table PID: {1} (0x{1:X4})", indent, this.NetworkInformationTablePID));

            result.Add(String.Format("{0}Programs: {1}", indent, this.Programs.Count));
            foreach (var program in this.Programs)
            {
                result.Add(String.Format("{0}{1}Program number: {2} (0x{2:X4})  Program map PID: {3} (0x{3:X4})", indent, Section.Indent, program.ProgramNumber, program.ProgramMapPID));
            }

            result.Add(String.Format("{0}CRC32: 0x{1:X8}", indent, this.Crc32)); 

            return result;
        }

        #endregion

        #region Constants

        public static readonly uint SectionTableId = 0x00000000;

        public static readonly uint VersionNumberMask = 0x0000001F;
        public const int VersionNumberShift = 1;

        public static readonly uint CurrentNextIndicatorMask = 0x00000001;
        public const int CurrentNextIndicatorShift = 0;

        public new static readonly uint HeaderSize = 5;
        public static readonly uint ProgramSize = 4;

        public static readonly uint ProgramMapPIDMask = 0x00001FFF;

        public static readonly uint NetworkInformationTablePIDUndefined = 0xFFFF;

        #endregion
    }
}
