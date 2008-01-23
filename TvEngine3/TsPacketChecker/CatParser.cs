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

    private string CA_System_ID2Str(int ca_id)
    {
      if (ca_id==0)
        return "Reserved";
      if (ca_id<0x100)
        return "Standardized Systems";
      if (ca_id<0x200)
        return "Canal Plus (Seca/MediaGuard)";
      if (ca_id<0x300)
        return "CCETT";
      if (ca_id<0x400)
        return "MSG MediaServices GmbH";
      if (ca_id<0x500)
        return "Eurodec";
      if (ca_id<0x600)
        return "France Telecom (Viaccess)";
      if (ca_id<0x700)
        return "Irdeto";
      if (ca_id<0x8ff)
        return "Jerrold/GI/Motorola";
      if (ca_id<0x900)
        return "Matra Communication";
      if (ca_id<0xa00)
        return "News Datacom (Videoguard)";
      if (ca_id<0xb00)
        return "Nokia";
      if (ca_id<0xc00)
        return "Norwegian Telekom (Conax)";
      if (ca_id<0xd00)
        return "NTL";
      if (ca_id<0xe00)
        return "Philips (Cryptoworks)";
      if (ca_id<0xf00)
        return "Scientific Atlanta (Power VU)";
      if (ca_id<0x1000)
        return "Sony";
      if (ca_id<0x1100)
        return "Tandberg Television";
      if (ca_id<0x1200)
        return "Thompson";
      if (ca_id<0x1300)
        return "TV/COM";
      if (ca_id<0x1400)
        return "HPT - Croatian Post and Telecommunications";
      if (ca_id<0x1500)
        return "HRT - Croatian Radio and Television";
      if (ca_id<0x1600)
        return "IBM";
      if (ca_id<0x1700)
        return "Nera";
      if (ca_id<0x1800)
        return "Beta Technik (Betacrypt)";
      if (ca_id<0x1900)
        return "Kudelski SA";
      if (ca_id<0x1a00)
        return "Titan Information Systems";
      if (ca_id>=0x2000 && ca_id <=0x20ff)
        return "Telefónica Servicios Audiovisuales";
      if (ca_id >= 0x2100 && ca_id <= 0x21ff)
        return "STENTOR (France Telecom, CNES and DGA)";
      if (ca_id>=0x2200 && ca_id <=0x22ff)
        return "Scopus Network Technologies";
      if (ca_id>=0x2300 && ca_id <=0x23ff)
        return "BARCO AS";
      if (ca_id>=0x2400 && ca_id <=0x24ff)
        return "StarGuide Digital Networks";
      if (ca_id>=0x2500 && ca_id <=0x25ff)
        return "Mentor Data System, Inc.";
      if (ca_id>=0x2600 && ca_id <=0x26ff)
        return "European Broadcasting Union";
      if (ca_id>=0x4700 && ca_id <=0x47ff)
        return "General Instrument";
      if (ca_id>=0x4800 && ca_id <=0x48ff)
        return "Telemann";
      if (ca_id>=0x4900 && ca_id <=0x49ff)
        return "Digital TV Industry Alliance of China";
      if (ca_id>=0x4a00 && ca_id <=0x4aff)
        return "Tsinghua TongFang";
      if (ca_id>=0x4a10 && ca_id <=0x4a1f)
        return "Easycas";
      if (ca_id>=0x4a20 && ca_id <=0x4a2f)
        return "AlphaCrypt";
      if (ca_id>=0x4a30 && ca_id <=0x4a3f)
        return "DVN Holdings";
      if (ca_id>=0x4a40 && ca_id <=0x4a4f)
        return "Shanghai Advanced Digital Technology Co. Ltd. (ADT)";
      if (ca_id>=0x4a50 && ca_id <=0x4a5f)
        return "Shenzhen Kingsky Company (China) Ltd";
      if (ca_id>=0x4a60 && ca_id <=0x4a6f)
        return "@SKY";
      if (ca_id>=0x4a70 && ca_id <=0x4a7f)
        return "DreamCrypt";
      if (ca_id>=0x4a80 && ca_id <=0x4a8f)
        return "THALESCrypt";
      if (ca_id>=0x4a90 && ca_id <=0x4a9f)
        return "Runcom Technologies";
      if (ca_id>=0x4aa0 && ca_id <=0x4aaf)
        return "SIDSA";
      if (ca_id>=0x4ab0 && ca_id <=0x4abf)
        return "Beijing Compunicate Technology Inc.";
      if (ca_id >= 0x4ac0 && ca_id <= 0x4acf)
        return "Latens Systems Ltd";

      return "Unknown CA_System_ID (0x"+ca_id.ToString("x")+")";
    }

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
          baseNode.Nodes.Add("Pid: 0x" + ca_pid.ToString("x") + " "+ CA_System_ID2Str(ca_system_id));
        }
        pos += (decriptor_len + 2);
      }
      baseNode.Text="CAT ("+baseNode.Nodes.Count.ToString()+" pids)";
      IsReady=true;
    }
  }
}
