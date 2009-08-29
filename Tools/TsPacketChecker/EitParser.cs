using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Forms;

namespace TsPacketChecker
{
  public class EitParser: SectionDecoder
  {
    #region Private variables
    Form1 frm;
    #endregion

    public EitParser(Form1 handle)
    {
      frm = handle;
      TableId = 0x2;
      Pid = (ushort)0x12;
    }

    public void Reset()
    {
    }

    public override void OnNewSection(Section sections)
    {
      int start = 14;
      byte[] buf = sections.Data;
      int service_id = (buf[3] << 8) + buf[4];
      if (service_id != 800)
        return;
      while (start + 11 <= sections.section_length + 1)
      {
        int descriptors_len = ((buf[start + 10] & 0xf) << 8) + buf[start + 11];
        start += 12;
        int off = 0;
        while (off < descriptors_len)
        {
          int descriptor_tag = buf[start + off];
          int descriptor_len = buf[start + off + 1];
          if (descriptor_len > 0)
          {
            switch (descriptor_tag)
            {
              case 0x4d: // short event
                byte[] data1 = new byte[buf.Length-(start+off-1)];
                Array.Copy(buf, start + off, data1, 0, buf.Length - (start + off));
                DecodeShortEventDescriptor(data1);
                //frm.WriteLog("short event");
                break;
              case 0x4e: // Extended event
                //frm.WriteLog("extended event");
                break;
              case 0x54: // genre
                //frm.WriteLog("genre");
                break;
              case 0x55: // parental rating
                byte[] data=new byte[buf.Length];
                Array.Copy(buf, start + off, data, 0, buf.Length - (start + off));
                DecodeParentalRating(data);
                break;
              case 0x5f: // private data
                //frm.WriteLog("private data");
                break;
              case 0x89: // MPAA rating
                frm.WriteLog("MPAA rating");
                break;
              default:
                //frm.WriteLog("Unknown descriptor ("+descriptor_tag.ToString()+")");
                break;
            }
          }
          off += descriptor_len + 2;
        }
        start += descriptors_len;
      }
	  }

    private string DecodeString(byte[] data, int start, int len)
    {
      string ret = "";
      for (int i = start; i < start+len; i++)
      {
        char c = (char)data[i];
        ret += c.ToString();
      }
      return ret;
    }
    private string DecodeLanguage(byte[] data, int start)
    {
      return DecodeString(data, start, 3);
    }
    private void DecodeShortEventDescriptor(byte[] buf)
    {
      int descriptor_tag = buf[0];
      int descriptor_len = buf[1];
      string lang = DecodeLanguage(buf, 2);
      int event_len = buf[5];
      string eventText = DecodeString(buf, 6, event_len);
      int off=6+event_len;
      int text_len=buf[off];
      string eventDescription = DecodeString(buf, off + 1, text_len);
      frm.WriteLog("[" + lang + "] Short event: title=" + eventText+" details="+eventDescription);
    }
    private void DecodeParentalRating(byte[] data)
    {
      int descriptor_length = data[1];
      int off = 2;
      while (off + 2 <= descriptor_length)
      {
        string lang = DecodeLanguage(data, off);
        uint rating=(uint)data[off+3];
        frm.WriteLog("["+lang+"] Rating=" + rating.ToString());
        off += 4;
      }
    }
  }
}
