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
    #endregion

    public SectionDecoder()
    {
      m_pid = 0x1fff;
      m_tableId = -1;
      m_section = new Section();
    }

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
    }

    private int StartNewSection(byte[] tsPacket,int index,int sectionLen)
    {
      int newstart=-1;
      int len=-1;
      if (index+sectionLen < 186)
      {
        len=sectionLen+3;
        newstart = index+sectionLen+3;
      }
      else
      {
        newstart = 188;
        len=188-index;
      }
      m_section.Reset();
      Array.Copy(tsPacket, index, m_section.Data, 0, len);
      m_section.BufferPos=len;
      return newstart;
    }
    private int AddToSection(byte[] tsPacket, int index, int sectionLen)
    {
      int newstart=-1;
      int len=-1;
      if (index+sectionLen < 186)
      {
        len=sectionLen+2;
        newstart = index+sectionLen+3;
      }
      else
      {
        newstart = 188;
        len=188-index;
      }
      if (len < 1)
        return 188;
      Array.Copy(tsPacket, index, m_section.Data, m_section.BufferPos, len);
      m_section.BufferPos += len;
      return newstart;
    }
    private int SnapshotSectionLength(byte[] tsPacket,int start)
    {
      if (start >= 185)
        return -1;
      return (int)(((tsPacket[start+1] & 0xF) << 8) + tsPacket[start+2]);
    }
    public virtual void OnTsPacket(byte[] tsPacket)
    {
      TsHeader header = new TsHeader(tsPacket);
      if (m_pid >= 0x1fff || m_tableId == -1) return;
      if (header.Pid != m_pid) return;

      if (!header.HasPayload) return;

      int start = header.PayLoadStart;

      if (header.PayloadUnitStart)
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
          return;

      while (start < 188)
      {
        if (tsPacket[start] == 0xFF) break; // Only stuffing bytes following
        if (m_section.BufferPos == 0)
        {
          int section_length = SnapshotSectionLength(tsPacket, start);
          if (section_length != -1)
            start += StartNewSection(tsPacket, start, section_length);
          else
            break;
        }
        else
          start = AddToSection(tsPacket, start, m_section.section_length - m_section.BufferPos);
        if (m_section.SectionComplete())
        {
          OnNewSection(m_section);
          m_section.Reset();
        }
      }
    }
      

    public virtual void OnNewSection(Section section)
    {
    }
    #endregion
  }
}
