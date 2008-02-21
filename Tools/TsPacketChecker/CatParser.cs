using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TsPacketChecker
{
  class CatParser: SectionDecoder
  {
    private TreeNode baseNode;
    public bool IsReady;

    public CatParser(TreeNode nodeToAdd)
    {
      baseNode = nodeToAdd;
      baseNode.Tag = false;
      Pid = 1;
      TableId = 1;
      IsReady = false;
    }

    public override void OnNewSection(Section section)
    {
      if (section.table_id != 1) return;
      if (IsReady) return;

      int pos = 8;
      while (pos+2 < section.section_length)
      {
        int descriptor_tag = section.Data[pos];
        int decriptor_len = section.Data[pos + 1];
        if (descriptor_tag == 0x9)
        {
          int ca_system_id = (section.Data[pos + 2] << 8) + section.Data[pos+3];
          int ca_pid = ((section.Data[pos + 4] & 0x1f) << 8) + section.Data[pos + 5];
          baseNode.Nodes.Add("Pid: 0x" + ca_pid.ToString("x") + " "+ StringUtils.CA_System_ID2Str(ca_system_id));
        }
        pos += (decriptor_len + 2);
      }
      baseNode.Text="CAT ("+baseNode.Nodes.Count.ToString()+" pids)";
      IsReady=true;
    }
  }
}
