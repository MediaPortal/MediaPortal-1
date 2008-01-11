using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TsCutterPackage
{
  class BufferedTsFileReader
  {
    #region Consts
    const int SYNC_BYTE = 0x47;
    const int TS_PACKET_SIZE = 188;
    #endregion

    #region Variables
    private BufferedStream _reader=null;
    private long fileSize;
    #endregion

    #region Public members
    public bool Open(string tsFile,long packetsToBuffer)
    {
      if (_reader != null)
        return true;
      try
      {
        _reader = new BufferedStream(new FileStream(tsFile, FileMode.Open),(int)(packetsToBuffer*188));
      }
      catch (Exception)
      {
        return false;
      }
      fileSize = _reader.Length;
      return true;
    }
    public void Close()
    {
      if (_reader == null)
        return;
      _reader.Close();
      _reader.Dispose();
      _reader = null;
    }
    public int GetPositionInPercent()
    {
      return (int)(_reader.Position * 100 / fileSize);
    }

    public bool SeekToFirstPacket()
    {
      bool found = false;
      while (!found)
      {
        int ch = _reader.ReadByte();
        if (ch == -1)
          return false;
        byte b = (byte)ch;
        if (b == SYNC_BYTE)
        {
          _reader.Seek(-1, SeekOrigin.Current);
          return true;
        }
      }
      return false;
    }
    public bool GetNextPacket(out byte[] tsPacket, out bool isValid)
    {
      isValid = false;
      tsPacket = new byte[TS_PACKET_SIZE];
      if (_reader.Read(tsPacket, 0, TS_PACKET_SIZE) != TS_PACKET_SIZE)
        return false;
      //check for sync byte and transport error bit
      if (tsPacket[0] == SYNC_BYTE && (tsPacket[1] & 0x80) == 0)
        isValid = true;
      return true;
    }
    #endregion 
  }
}
