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
    private bool addToNode;
    public List<ushort> streamPids;
    private TreeNode baseNode;
    private int service_id;
    #endregion

    public PmtParser(int pmtPid,int service_id,TreeNode nodeToAdd)
    {
      IsReady = false;
      baseNode = nodeToAdd;
      addToNode = true;
      this.service_id = service_id;
      TableId = 0x2;
      Pid = (ushort)pmtPid;
      streamPids = new List<ushort>();
    }

    public void Reset()
    {
      IsReady = false;
      addToNode = false;
      streamPids.Clear();
    }

    public override void OnNewSection(Section sections)
    {
      if (IsReady) return;

      if (sections.table_id_extension != service_id) return;
      if (sections.table_id_extension == 6033)
      {
        int xc = 564;
      }
      byte[] section = sections.Data;
      int section_length = sections.section_length;

     
      int pcrPid = ((section[8] & 0x1F) << 8) + section[9];
      if (addToNode)
      {
        baseNode.Parent.ForeColor = System.Drawing.Color.Black;
        baseNode.Nodes.Add("PCR pid: 0x" + pcrPid.ToString("x"));
      }
      int program_info_length = ((section[10] & 0xF) << 8) + section[11];

      // Skip the descriptors (if any).
	    int ndx = 12;
	    ndx += program_info_length;

	    // Now we have the actual program data.
      while (ndx < section_length - 3)
      {
        int stream_type = section[ndx++];
        int pid = ((section[ndx++] & 0x1f) << 8) + section[ndx++];
        int es_descriptors_length = ((section[ndx++] & 0x0f) << 8) + section[ndx++];
        if (addToNode)
        {
          TreeNode node = baseNode.Nodes.Add("pid: 0x" + pid.ToString("x") + " " + StringUtils.StreamTypeToStr(stream_type));
          if (es_descriptors_length > 0)
          {
            int off = 0;
            while (off < es_descriptors_length)
            {
              int descriptor_tag = section[ndx + off];
              int descriptor_len = section[ndx + off + 1];
              switch (descriptor_tag)
              {
                case 0x9: // CA Descriptor
                  int ca_system_id = (section[ndx+off+ 2] << 8) + section[ndx+off+3];
                  int ca_pid = ((section[ndx+off+4] & 0x1f) << 8) + section[ndx+off+5];
                  node.Nodes.Add("CA: Pid: 0x" + ca_pid.ToString("x") + " "+ StringUtils.CA_System_ID2Str(ca_system_id));
                  break;
                case 0x0A: // ISO_639_language
                  node.Nodes.Add("ISO_639_language: " + StringUtils.getString468A(section, ndx + off + 2, 3));
                  break;
                case 0x56: // Teletext
                  node.Text = "pid: 0x" + pid.ToString("x") + " [Teletext] " + StringUtils.StreamTypeToStr(stream_type);
                  break;
                case 0x59: // Subtitles
                  node.Text = "pid: 0x" + pid.ToString("x") + " [Subtitles] " + StringUtils.StreamTypeToStr(stream_type);
                  break;
                case 0x6A: // AC3
                  node.Text = "pid: 0x" + pid.ToString("x") + " [AC3-Audio] " + StringUtils.StreamTypeToStr(stream_type);
                  break;
                case 0x5F: // private data
                  node.Text = "pid: 0x" + pid.ToString("x") + " [Private Data] " + StringUtils.StreamTypeToStr(stream_type);
                  break;
                  default:
                  node.Nodes.Add("0x" + descriptor_tag.ToString("x"));
                  break;
              }
              off += descriptor_len + 2;
            }
          }
        }
        ndx += es_descriptors_length;
        if (!streamPids.Contains((ushort)pid))
          streamPids.Add((ushort)pid);
      }
      int streamcount=baseNode.Nodes.Count-1;
      if (addToNode)
        baseNode.Text = "PMT (" + streamcount.ToString() + " streams)";
      IsReady=true;
	  }
  }
}
