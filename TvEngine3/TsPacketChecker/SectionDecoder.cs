using System;
using System.Collections.Generic;
using System.Text;

namespace TsPacketChecker
{
  public class SectionDecoder
  {
    #region Variables
    private ushort m_pid;
    private int m_tableId;
    private Section m_section;
    public static uint incompleteSections=0;
    private int lastCC;
    private bool waitForNextPayloadStart;
    private long packetNr;
    #endregion

    public SectionDecoder()
    {
      m_pid = 0x1fff;
      m_tableId = -1;
      m_section = new Section();
      lastCC = -1;
      waitForNextPayloadStart = false;
      packetNr = 0;
    }
    public SectionDecoder(ushort pid, int table_id)
    {
      m_pid = pid;
      m_tableId = table_id;
      m_section = new Section();
      lastCC = -1;
      waitForNextPayloadStart = false;
    }

    public delegate void MethodOnSectionDecoded(Section section);
    public event MethodOnSectionDecoded OnSectionDecoded;

    #region Properties
    public ushort Pid
    {
      get
      {
        return m_pid;
      }
      set
      {
        m_pid = value;
      }
    }
    public int TableId
    {
      get
      {
        return m_tableId;
      }
      set
      {
        m_tableId = value;
      }
    }
    #endregion

    #region Public functions
    public void Reset()
    {
        m_section.Reset();
        lastCC = -1;
    }

    private int StartNewSection(byte[] tsPacket,int index,int sectionLen)
    {
      int newstart=-1;
      int len=-1;
      if (sectionLen > -1)
      {
        if (index + sectionLen < 185)
        {
          len = sectionLen + 3;
          newstart = index + sectionLen + 3;
        }
        else
        {
          newstart = 188;
          len = 188 - index;
        }
      }
      else
      {
        newstart = 188;
        len = 188 - index;
      }
      m_section.Reset();
      Array.Copy(tsPacket, index, m_section.Data, 0, len);
      m_section.BufferPos=len;
      m_section.DecodeHeader();
      return newstart;
    }
    private int AddToSection(byte[] tsPacket, int index, int sectionLen)
    {
      int newstart=-1;
      int len=-1;
      if (index+sectionLen < 185)
      {
        len=sectionLen+3;
        newstart = index+sectionLen+3;
      }
      else
      {
        newstart = 188;
        len=188-index;
      }
      Array.Copy(tsPacket, index, m_section.Data, m_section.BufferPos, len);
      m_section.BufferPos += len;
      m_section.DecodeHeader();
      return newstart;
    }
    private int SnapshotSectionLength(byte[] tsPacket,int start)
    {
      if (start > 184)
        return -1;
      return (int)(((tsPacket[start+1] & 0xF) << 8) + tsPacket[start+2]);
    }

    public virtual void OnTsPacket(byte[] tsPacket)
    {
      TsHeader header = new TsHeader(tsPacket);
      if (m_pid >= 0x1fff || m_tableId == -1) return;
      if (header.Pid != m_pid) return;
      if (!header.HasPayload) return;
      /*if (lastCC == -1)
      {
        lastCC = header.ContinuityCounter;
        if (lastCC == 0)
          lastCC = 15;
        else
          lastCC--;
      }
      int ccExp = lastCC + 1;
      if (ccExp == 16) ccExp = 0;
      if (ccExp != header.ContinuityCounter)
      {
        if (m_section.BufferPos != 0)
        {
          int hihi = 12;
        }
        waitForNextPayloadStart = true;
        return;
      }
      lastCC = header.ContinuityCounter;*/

      

      int start = header.PayLoadStart;
      #region ___
      /*if (header.PayloadUnitStart )
      {
        int n = tsPacket[start];
        start = start + n + 1;

        int section_length = SnapshotSectionLength(tsPacket, start);
        if (section_length != -1)
        {
          start = StartNewSection(tsPacket, start, section_length);
          if (m_section.SectionComplete())
          {
            OnNewSection(m_section);
            m_section.Reset();
          }
        }
      }
      else
        if (m_section.BufferPos == 0) // no  current section to add data to, wait for next payload start
          return;*/
      #endregion

      int pointer_field = 0;
      packetNr++;

      int po = 4;
      while (po < 187)
      {
        byte b1 = tsPacket[po];
        byte b2 = tsPacket[po+1];
        po++;
        int sid = (b1 << 8) + b2;
        if (sid == 6029)
        {
          int found = 1;
        }
      }
      if (header.PayloadUnitStart)
      {
        pointer_field = start + tsPacket[start]+1;
        if (m_section.BufferPos == 0)
          start += tsPacket[start] + 1;
        else
        {
          start++;
          if (waitForNextPayloadStart)
          {
            m_section.section_length = m_section.BufferPos - 1;
            m_section.DecodeHeader();
            OnNewSection(m_section);
            if (OnSectionDecoded != null)
              OnSectionDecoded(m_section);
            m_section.Reset();
            start = pointer_field;
          }
        }
      }
      waitForNextPayloadStart = false;
      while (start < 188)
      {
        if (m_section.BufferPos == 0)
        {
          if (!header.PayloadUnitStart) return;
          if (tsPacket[start] == 0xFF) return;
          //if (tsPacket[start] != m_tableId)
          //  return;
          int section_length = SnapshotSectionLength(tsPacket, start);
          start = StartNewSection(tsPacket, start, section_length);
        }
        else
        {
          if (m_section.section_length == -1)
            m_section.CalcSectionLength(tsPacket, start);
          int len = m_section.section_length - m_section.BufferPos;
          if (pointer_field != 0 && ((start + len) > pointer_field))
          {
            len = pointer_field - start;
            start = AddToSection(tsPacket, start, len);
            m_section.section_length = m_section.BufferPos - 1;
            start = pointer_field;
            incompleteSections++;
          }
          else
            start = AddToSection(tsPacket, start, len);
        }
        if (m_section.SectionComplete() && m_section.section_length>0)
        {
          if (m_section.version_number!=2 && m_section.version_number!=3)
          {
            int ha = 12;
          }
          OnNewSection(m_section);
          if (OnSectionDecoded != null)
            OnSectionDecoded(m_section);
          m_section.Reset();
        }
      
        pointer_field=0;
      }
    }
      

    public virtual void OnNewSection(Section section)
    {
    }
    #endregion
  }
}
