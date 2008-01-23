using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TsPacketChecker
{
  class PatParser: SectionDecoder
  {
    private TreeNode baseNode;
    public bool IsReady;
    private bool patReady;
    private bool pmtReady;
    private List<PmtParser> pmtParsers;

    public PatParser(TreeNode nodeToAdd)
    {
      baseNode = nodeToAdd;
      baseNode.Tag = false;
      IsReady = false;
      patReady = false;
      pmtReady = false;
      pmtParsers = new List<PmtParser>();
      Pid = 0;
      TableId = 0;
    }

    public override void OnTsPacket(byte[] tsPacket)
    {
      if (IsReady) return;
      if (patReady)
      {
        foreach (PmtParser pmtp in pmtParsers)
        {
          pmtp.OnTsPacket(tsPacket);
          pmtReady = true;
          if (!pmtp.IsReady)
          {
            pmtReady = false;
            break;
          }
        }
        if (pmtReady)
          IsReady = true;
        return;
      }
      base.OnTsPacket(tsPacket);
    }

    public override void OnNewSection(Section section)
    {
      int pmtCount = 0;
      int loop = (section.section_length - 9) / 4;
      for (int i=0;i<loop;i++)
      {
        int offset = (8 +(i * 4));
        int program_nr=((section.Data[offset] )<<8) + section.Data[offset+1];
	      int pmt_pid = ((section.Data[offset+2] & 0x1F)<<8) + section.Data[offset+3];

        if (pmt_pid <= 0x11 || pmt_pid > 0x1FFF) continue;
        TreeNode pmtNode=baseNode.Nodes.Add("# " + program_nr.ToString() + " pmt pid: 0x" + pmt_pid.ToString("x"));
        pmtNode=pmtNode.Nodes.Add("PMT");
        pmtParsers.Add(new PmtParser(pmt_pid, pmtNode));
        pmtCount++;
      }
      patReady = true;
      baseNode.Text = "PAT (" + pmtParsers.Count.ToString() + " programs)";
    }
  }
}
