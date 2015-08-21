using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.StreamSections
{
    internal class ProgramDefinition
    {
        #region Private fields
        #endregion

        #region Constructors

        public ProgramDefinition()
        {
            this.StreamType = 0;
            this.ElementaryPID = 0;
            this.Descriptor = null;
        }

        #endregion

        #region Properties

        public virtual uint StreamType { get; protected set; }

        public virtual String StreamTypeText
        {
            get { return ProgramDefinition.GetStreamType(this.StreamType); }
            protected set { }
        }

        public virtual uint ElementaryPID { get; protected set; }

        public virtual Byte[] Descriptor { get; protected set; }

        public virtual uint Size
        {
            get
            {
                return (5 + (uint)this.Descriptor.Length);
            }
        }

        public virtual String ShortName
        {
            get
            {
                return String.Format("ES PID: {0} (0x{0:X4})", this.ElementaryPID);
            }
        }

        #endregion

        #region Methods

        public virtual void Parse(Byte[] data, int position)
        {
            if ((position + ProgramDefinition.ProgramDefinitionHeaderSize) >= data.Length)
            {
                throw new ArgumentOutOfRangeException("position", position, "Not enough data for program definition header.");
            }

            uint streamType = data.ReadBigEndian8(position);
            position++;

            uint elementaryPID = data.ReadBigEndian16(position);
            position += 2;

            uint esInfoSize = data.ReadBigEndian16(position);
            position += 2;

            elementaryPID = (elementaryPID >> ProgramDefinition.ProgramDefinitionElementaryPIDShift) & ProgramDefinition.ProgramDefinitionElementaryPIDMask;
            esInfoSize = (esInfoSize >> ProgramDefinition.ProgramDefinitionEsInfoLengthShift) & ProgramDefinition.ProgramDefinitionEsInfoLengthMask;

            if ((position + esInfoSize) >= data.Length)
            {
                throw new ArgumentOutOfRangeException("esInfoSize", esInfoSize, "Not enough data for program definition descriptor.");
            }

            this.StreamType = streamType;
            this.ElementaryPID = elementaryPID;
            this.Descriptor = new Byte[esInfoSize];

            for (int i = position; i < (position + esInfoSize); i++)
            {
                this.Descriptor[i - position] = data[i];
            }
        }

        public virtual List<string> ToHumanReadable(string indent)
        {
            List<String> result = new List<string>();

            result.Add(String.Format("{0}Stream type: {1} (0x{1:X2}) ({2})", indent, this.StreamType, this.StreamTypeText));
            result.Add(String.Format("{0}Elementary PID: {1} (0x{1:X4})", indent, this.ElementaryPID));
            result.Add(String.Format("{0}Descriptor size: {1}", indent, this.Descriptor.Length));

            return result;
        }

        public static String GetStreamType(uint streamType)
        {
            switch (streamType)
            {
                case 0:
                    return "ITU-T | ISO/IEC Reserved";
                case 1:
                    return "ISO/IEC 11172 Video (MPEG-1 Video)";
                case 2:
                    return "ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream (MPEG-2 Video)";
                case 3:
                    return "ISO/IEC 11172 Audio (MPEG-1 Audio)";
                case 4:
                    return "ISO/IEC 13818-3 Audio (MPEG-2 Audio)";
                case 5:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 private_sections";
                case 6:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 PES packets containing private data";
                case 7:
                    return "ISO/IEC 13522 MHEG";
                case 8:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 Annex A DSM-CC";
                case 9:
                    return "ITU-T Rec. H.222.1";
                case 10:
                    return "ISO/IEC 13818-6 type A";
                case 11:
                    return "ISO/IEC 13818-6 type B";
                case 12:
                    return "ISO/IEC 13818-6 type C";
                case 13:
                    return "ISO/IEC 13818-6 type D";
                case 14:
                    return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 auxiliary";
                case 15:
                    return "ISO/IEC 13818-7 Audio with ADTS transport syntax";
                case 16:
                    return "ISO/IEC 14496-2 Visual";
                case 17:
                    return "ISO/IEC 14496-3 Audio with the LATM transport syntax as defined in ISO/IEC 14496-3 / AMD 1";
                case 18:
                    return "ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in PES packets";
                case 19:
                    return "ISO/IEC 14496-1 SL-packetized stream or FlexMux stream carried in ISO/IEC14496_sections";
                case 20:
                    return "ISO/IEC 13818-6 Synchronized Download Protocol";
                default:
                    return "Unknown";
            }
        }

        #endregion

        #region Constants

        public static readonly uint ProgramDefinitionElementaryPIDMask = 0x00001FFF;
        public const int ProgramDefinitionElementaryPIDShift = 0;

        public static readonly uint ProgramDefinitionEsInfoLengthMask = 0x00000FFF;
        public const int ProgramDefinitionEsInfoLengthShift = 0;

        public const int ProgramDefinitionHeaderSize = 5;

        #endregion
    }
}
