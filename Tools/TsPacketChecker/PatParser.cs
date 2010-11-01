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
    private bool addToNode;
    private List<PmtParser> pmtParsers;

    public PatParser(TreeNode nodeToAdd)
    {
      baseNode = nodeToAdd;
      baseNode.Tag = false;
      IsReady = false;
      patReady = false;
      pmtReady = false;
      addToNode = true;
      pmtParsers = new List<PmtParser>();
      Pid = 0;
      TableId = 0;
    }

    public void Reset()
    {
      base.Reset();
      IsReady = false;
      patReady = false;
      pmtReady = false;
      addToNode = false;
      foreach (PmtParser pmtp in pmtParsers)
        pmtp.Reset();
    }
    public List<ushort> GetPmtStreamPids()
    {
      List<ushort> streamPids = new List<ushort>();
      foreach (PmtParser pmtp in pmtParsers)
        streamPids.AddRange(pmtp.streamPids);
      return streamPids;
    }

    public override void OnTsPacket(byte[] tsPacket)
    {
      if (IsReady) 
        return;
      if (patReady)
      {
        pmtReady = true;
        foreach (PmtParser pmtp in pmtParsers)
        {
          pmtp.OnTsPacket(tsPacket);
          if (!pmtp.IsReady)
            pmtReady = false;
        }
        if (pmtReady)
          IsReady = true;
      }
      else       
        base.OnTsPacket(tsPacket);
    }

    public override void OnNewSection(Section section)
    {
      if (section.table_id != TableId) return;
      int pmtCount = 0;
      int loop = (section.section_length - 9) / 4;
      for (int i=0;i<loop;i++)
      {
        int offset = (8 +(i * 4));
        int program_nr=((section.Data[offset] )<<8) + section.Data[offset+1];
	      int pmt_pid = ((section.Data[offset+2] & 0x1F)<<8) + section.Data[offset+3];

        if (pmt_pid <= 0x10 || pmt_pid > 0x1FFF) continue;
        if (addToNode)
        {
          TreeNode pmtNode = baseNode.Nodes.Add("# " + program_nr.ToString() + " pmt pid: 0x" + pmt_pid.ToString("x"));
          pmtNode.ForeColor = System.Drawing.Color.Red;
          pmtNode = pmtNode.Nodes.Add("PMT");
          //if (pmtParsers.Count==0)
          pmtParsers.Add(new PmtParser(pmt_pid,program_nr,pmtNode));
        }
        pmtCount++;
      }
      patReady = true;
      if (addToNode)
        baseNode.Text = "PAT (" + pmtParsers.Count.ToString() + " programs)";
    }
  }
}
