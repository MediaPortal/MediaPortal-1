using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Player.Teletext
{
    public interface IDVBTeletextDecoder
    {
        void OnTeletextPacket(byte[] data);

        void OnServiceInfo(int page, byte type, string iso_lang);

        bool AcceptsDataUnitID(byte id);

        void Reset();
    }
}
