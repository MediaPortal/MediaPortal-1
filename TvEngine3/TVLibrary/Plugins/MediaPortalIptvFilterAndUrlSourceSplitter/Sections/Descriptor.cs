using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal abstract class Descriptor
    {
        #region Private fields
        #endregion

        #region Constructors

        protected Descriptor()
        {
            this.Tag = Descriptor.UnknownTag;
            this.Payload = null;
        }

        #endregion

        #region Properties

        public virtual uint Tag { get; protected set; }

        public virtual Byte[] Payload { get; protected set; }

        public virtual uint Size
        {
            get
            {
                return (uint)(Descriptor.DescriptorHeaderSize + this.Payload.Length);
            }
        }

        public virtual void Parse(Byte[] data, int position)
        {
            if ((position + Descriptor.DescriptorHeaderSize) >= data.Length)
            {
                throw new ArgumentOutOfRangeException("position", position, "Not enough data for descriptor header.");
            }

            this.Tag = data.ReadBigEndian8(position);
            position++;

            if (!this.CheckTag())
            {
                throw new InvalidDescriptorTagException();
            }

            uint descriptorSize = data.ReadBigEndian8(position);
            position++;

            if ((position + descriptorSize) >= data.Length)
            {
                throw new ArgumentOutOfRangeException("descriptorSize", descriptorSize, "Not enough data for descriptor payload.");
            }
           
            this.Payload = new Byte[descriptorSize];

            for (int i = position; i < (position + descriptorSize); i++)
            {
                this.Payload[i - position] = data[i];
            }
        }

        protected abstract Boolean CheckTag();

        #endregion

        #region Methods

        public abstract List<String> ToHumanReadable(String indent);

        #endregion

        #region Constants

        public static readonly uint UnknownTag = 0xFFFFFFFF;

        public const int DescriptorHeaderSize = 2;

        #endregion
    }
}
