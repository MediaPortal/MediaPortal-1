using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Sections
{
    internal sealed class ConditionalAccessDescriptor : Descriptor
    {
        #region Properties

        public uint SystemId
        {
            get { return ((this.Payload.ReadBigEndian16(0) >> ConditionalAccessDescriptor.ConditionalAccessDescriptorShift) & ConditionalAccessDescriptor.ConditionalAccessDescriptorMask); }
        }

        public uint PID
        {
            get { return ((this.Payload.ReadBigEndian16(2) >> ConditionalAccessDescriptor.ConditionalAccessDescriptorPidShift) & ConditionalAccessDescriptor.ConditionalAccessDescriptorPidMask); }
        }

        public int PrivateDataSize
        {
            get { return (this.Payload.Length > ConditionalAccessDescriptor.ConditionalAccessDescriptorHeaderLength) ? (this.Payload.Length - ConditionalAccessDescriptor.ConditionalAccessDescriptorHeaderLength) : 0; }
        }

        #endregion


        #region Methods

        protected override bool CheckTag()
        {
            return (this.Tag == ConditionalAccessDescriptor.ConditionalAccessDescriptorTag);
        }

        public override List<string> ToHumanReadable(string indent)
        {
            List<String> result = new List<string>();

            result.Add(String.Format("{0}Tag: {1} (0x{1:X2}, Conditional Access Descriptor)", indent, this.Tag));
            result.Add(String.Format("{0}Descriptor size: {1}", indent, this.Payload.Length));

            result.Add(String.Format("{0}System ID: {1} (0x{1:X4})", indent, this.SystemId));
            result.Add(String.Format("{0}PID: {1} (0x{1:X4})", indent, this.PID));
            result.Add(String.Format("{0}Private data size: {1}", indent, this.PrivateDataSize));

            return result;
        }

        #endregion

        #region Constants

        public static readonly uint ConditionalAccessDescriptorTag = 0x00000009;

        public static readonly uint ConditionalAccessDescriptorMask = 0xFFFF;

        public static readonly int ConditionalAccessDescriptorShift = 0;

        public static readonly uint ConditionalAccessDescriptorPidMask = 0x1FFF;

        public static readonly int ConditionalAccessDescriptorPidShift = 0;
        
        public static readonly int ConditionalAccessDescriptorHeaderLength = 4;

        #endregion
    }
}
