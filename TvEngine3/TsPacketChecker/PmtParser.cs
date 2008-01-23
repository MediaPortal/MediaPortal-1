using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Forms;

namespace TsPacketChecker
{
  public class PmtParser: SectionDecoder
  {
    #region Private variables
    public bool IsReady;
    private TreeNode baseNode;
    #endregion

    private string StreamTypeToStr(int streamType)
    {
      switch (streamType)
      {
        case 0x00:
          return "ITU-T | ISO/IEC reserved";
        case 0x01:
          return "[Video MPEG-1] ISO/IEC 11172-2 Video";
        case 0x02:
          return "[Video MPEG-2] (ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream)";
        case 0x03:
          return "[Audio MPEG-1] (ISO/IEC 11172-3 Audio)";
        case 0x04:
          return "[Audio MPEG-2] (ISO/IEC 13818-3 Audio)";
        case 0x5:
          return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 private_sections";
        case 0x06:
          return "[Teletext] ITU-T Rec. H.222.0 | ISO/IEC 13818-1 PES packets containing private data";
        case 0x07:
          return "[MHW-MHEG] ISO/IEC 13522 MHEG";
        case 0x08:
          return "Annex A - DSM CC";
        case 0x09:
          return "[DATA] ITU-T Rec. H.222.1";
        case 0x0A:
          return "ISO/IEC 13818-6 type A";
        case 0x0B:
          return "ISO/IEC 13818-6 type B";
        case 0x0C:
          return "ISO/IEC 13818-6 type C";
        case 0x0D:
          return "ISO/IEC 13818-6 type D";
        case 0x0E:
          return "ISO/IEC 13818-1 auxiliary";
      }
      if (streamType >= 0x0F && streamType <= 0x7F)
        return "ITU-T Rec. H.222.0 | ISO/IEC 13818-1 reserved";
      if (streamType > 0x80)
        return "User private";
      return "Unknown";
    }

    public PmtParser(int pmtPid,TreeNode nodeToAdd)
    {
      IsReady = false;
      baseNode = nodeToAdd;
      TableId = 0x2;
      Pid = (ushort)pmtPid;
    }

    public override void OnNewSection(Section sections)
    {
      if (IsReady) return;

      byte[] section = sections.Data;
      int section_length = sections.section_length;

      int program_nr = sections.table_id_extension;
      int pcrPid = ((section[8] & 0x1F) << 8) + section[9];
      baseNode.Nodes.Add("PCR pid: 0x"+pcrPid.ToString("x"));
      int program_info_length = ((section[10] & 0xF) << 8) + section[11];

      // Skip the descriptors (if any).
	    int ndx = 12;
	    ndx += program_info_length;

	    // Now we have the actual program data.
      while (ndx < section_length - 4)
      {
        int stream_type = section[ndx++];
        int pid = ((section[ndx++] & 0x1f) << 8) + section[ndx++];
        int es_descriptors_length = ((section[ndx++] & 0x0f) << 8) + section[ndx++];
        if (es_descriptors_length > 0)
        {
          int descriptor_tag = section[ndx];
          int descriptor_len = section[ndx+1];
        }
        ndx += es_descriptors_length;
        baseNode.Nodes.Add("pid: 0x" + pid.ToString("x") + " " + StreamTypeToStr(stream_type));
      }
      int streamcount=baseNode.Nodes.Count-1;
      baseNode.Text = "PMT (" + streamcount.ToString() + " streams)";
      IsReady=true;
	  }
  }
}
