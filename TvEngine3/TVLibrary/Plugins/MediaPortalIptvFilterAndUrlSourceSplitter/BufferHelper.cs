using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    internal static class BufferHelper
    {
        public static uint ReadBigEndian8(this Byte[] data, int position)
        {
            return data[position];
        }

        public static uint ReadBigEndian16(this Byte[] data, int position)
        {
            return ((data.ReadBigEndian8(position) << 8) | data.ReadBigEndian8(position + 1));
        }

        public static uint ReadBigEndian24(this Byte[] data, int position)
        {
            return ((data.ReadBigEndian16(position) << 8)) | data.ReadBigEndian8(position + 2);
        }

        public static uint ReadBigEndian32(this Byte[] data, int position)
        {
            return ((data.ReadBigEndian16(position) << 16) | data.ReadBigEndian16(position + 2));
        }

        public static UInt64 ReadBigEndian64(this Byte[] data, int position)
        {
            return ((((UInt64)data.ReadBigEndian32(position)) << 32) | ((UInt64)data.ReadBigEndian32(position + 4)));
        }
    }
}
