using System;
using System.Collections.Generic;
using System.Text;

namespace TsPacketChecker
{
  //#define MAX_SECTION_LENGTH 4300
  
  public class Section
  {
    public static int MAX_SECTION_LENGTH = 4300;

    #region Public vars
    public int table_id;
    public int table_id_extension;
    public int section_length;
    public int section_number;
    public int version_number;
    public int section_syntax_indicator;
    public int last_section_number;

    public int BufferPos;
    public byte[] Data;
    #endregion

    public Section()
    {
      Data = new byte[MAX_SECTION_LENGTH * 5];
      Reset();
    }

    public void Reset()
    {
      table_id = -1;
      table_id_extension = -1;
      section_length = -1;
      section_number = -1;
      version_number = -1;
      section_syntax_indicator = -1;
      BufferPos = 0;
      for (int i = 0; i < Data.Length; i++)
        Data[i] = 0xFF;
    }
    public int CalcSectionLength(byte[] tsPacket,int start)
    {
      if (BufferPos < 3)
      {
        byte bHi=0;
        byte bLow=0;
        if (BufferPos==1)
        {
          bHi=tsPacket[start];
          bLow=tsPacket[start+1];
        }
        else if (BufferPos==2)
        {
          bHi=Data[1];
          bLow=tsPacket[start];
        }
        section_length=(int)(((bHi & 0xF) << 8) + bLow);
      }
      else
        section_length = (int)(((Data[1] & 0xF) << 8) + Data[2]);
      return section_length;
    }
    public bool DecodeHeader()
    {
      if (BufferPos < 8) 
        return false;
      table_id = (int)Data[0];
      section_syntax_indicator = (int)((Data[1] >> 7) & 1);
      if (section_length==-1)
        section_length=(int)(((Data[1] & 0xF) << 8) + Data[2]);
      table_id_extension=((Data[3] << 8) +Data[4]);
      version_number = (int)((Data[5] >> 1) & 0x1F);
      section_number = (int)Data[6];
      section_syntax_indicator = (int)((Data[1] >> 7) & 1);
      return true;
    }
    public bool SectionComplete()
    {
      if (!DecodeHeader() && BufferPos > section_length && section_length>0)
        return true;
      if (!DecodeHeader())
        return false;
      return (BufferPos >= section_length);
    }
  }
}
