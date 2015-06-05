using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal abstract class Section
    {
        #region Private fields

        protected Byte[] payload;

        #endregion

        #region Constructors

        public Section()
        {
            this.payload = null;
        }

        #endregion

        #region Properties

        virtual public uint TableId
        {
            get
            {
                uint header = this.payload.ReadBigEndian24(0);

                return ((header >> Section.HeaderTableIdShift) & Section.HeaderTableIdMask);
            }
        }

        virtual public uint Crc32
        {
            get
            {
                return this.payload.ReadBigEndian32((int)(this.SectionSize - Section.Crc32Size));
            }
        }

        virtual public Boolean SectionSyntaxIndicator
        {
            get
            {
                uint header = this.payload.ReadBigEndian24(0);

                return (((header >> Section.HeaderSectionSyntaxIndicatorShift) & Section.HeaderSectionSyntaxIndicatorMask) != 0);
            }
        }

        virtual public Boolean PrivateIndicator
        {
            get
            {
                uint header = this.payload.ReadBigEndian24(0);

                return (((header >> Section.HeaderPrivateIndicatorShift) & Section.HeaderPrivateIndicatorMask) != 0);
            }
        }

        virtual public uint SectionLength
        {
            get
            {
                uint header = this.payload.ReadBigEndian24(0);
                header = ((header >> Section.HeaderSectionLengthShift) & Section.HeaderSectionLengthMask);

                return header;
            }
        }

        virtual public uint SectionSize
        {
            get
            {
                uint header = this.payload.ReadBigEndian24(0);
                header = ((header >> Section.HeaderSectionLengthShift) & Section.HeaderSectionLengthMask);

                header += (header != 0) ? Section.HeaderSize : 0;

                return header;
            }
        }

        virtual public Byte[] Payload
        {
            get
            {
                return this.payload;
            }
        }

        public abstract String ShortName { get; }

        #endregion

        #region Methods

        virtual public void Parse(Byte[] sectionData)
        {
            this.payload = sectionData;

            if (!this.CheckTableId())
            {
                throw new InvalidTableIdException();
            }
        }

        protected abstract Boolean CheckTableId();

        public abstract List<String> ToHumanReadable(String indent);

        #endregion

        #region Constants

        public static readonly uint HeaderSize = 3;

        public static readonly uint Crc32Size = 4;

        public static readonly uint HeaderTableIdMask = 0x000000FF;
        public const int HeaderTableIdShift = 16;

        public static readonly uint HeaderSectionSyntaxIndicatorMask = 0x00000001;
        public const int HeaderSectionSyntaxIndicatorShift = 15;

        public static readonly uint HeaderPrivateIndicatorMask = 0x00000001;
        public const int HeaderPrivateIndicatorShift = 14;

        public static readonly uint HeaderReservedMask = 0x00000003;
        public const int HeaderReservedShift = 12;

        public static readonly uint HeaderSectionLengthMask = 0x00000FFF;
        public const int HeaderSectionLengthShift = 0;

        public static readonly uint MaxSize = 0x00001000;

        public static readonly uint Crc32Undefined = 0xFFFFFFFF;

        public static readonly String Indent = "   ";

        #endregion
    }
}
