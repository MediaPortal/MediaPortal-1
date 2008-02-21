using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Forms;

namespace TsPacketChecker
{
  class BaseChannel
  {
    public int transport_id;
    public int network_id;
    public int service_id;
  }
  class LinkedChannel : BaseChannel
  {
    public string displayName;
  }
  class PortalChannel: BaseChannel
  {
    public List<LinkedChannel> linkedChannels;

    public PortalChannel(int transport_id,int network_id,int service_id)
    {
      linkedChannels=new List<LinkedChannel>();
      this.transport_id = transport_id;
      this.network_id = network_id;
      this.service_id = service_id;
    }
  }
  class ChannelLinkageParser: SectionDecoder
  {
    public SortedDictionary<ulong, PortalChannel> channels;
    private TreeNode baseNode;

    public ChannelLinkageParser(TreeNode nodeToAdd)
    {
      Pid = 0x12;
      TableId = 0; // Set it to a value !=-1 to get all tables
      baseNode = nodeToAdd;
      channels = new SortedDictionary<ulong, PortalChannel>();
    }
    public override void OnNewSection(Section section)
    {
      if (section.table_id < 0x4e || section.table_id > 0x6f) return;

      int service_id = section.table_id_extension;
      int transport_id=(section.Data[8]<<8)+section.Data[9];
      int network_id=(section.Data[10]<<8)+section.Data[11];

      ulong nid = (ulong)network_id;
      ulong tid = (ulong)transport_id;
      ulong sid = (ulong)service_id;

      ulong key = (ulong)(nid << 32);
      key += (ulong)(tid << 16);
      key+=sid;

      if (channels.ContainsKey(key))
        return;
      PortalChannel pchannel = new PortalChannel(transport_id, network_id, service_id);
      channels.Add(key, pchannel);
      TreeNode pNode=baseNode.Nodes.Add("#"+service_id.ToString()+" nid: " + transport_id.ToString() + " nid: " + network_id.ToString() + " sid: " + service_id.ToString());

      int start = 14;
      while (start + 11 <= section.section_length+1)
      {
        int descriptors_len = ((section.Data[start + 10] & 0xF) << 8) + section.Data[start + 11];
        start += 12;
        int off = 0;
        while (off < descriptors_len)
        {
          if (start + off + 1 > section.section_length) return;

          int descriptor_tag = section.Data[start + off];
          int descriptor_len = section.Data[start + off + 1];

          if (start + off + descriptor_len + 2 > section.section_length) return;
          if (descriptor_len < 1) return;
          if (descriptor_tag == 0x4a)
          {
            LinkedChannel lchannel = new LinkedChannel();
            lchannel.transport_id = (section.Data[start + off + 2] << 8) + section.Data[start + off + 3];
            lchannel.network_id = (section.Data[start + off + 4] << 8) + section.Data[start + off + 5];
            lchannel.service_id = (section.Data[start + off + 6] << 8) + section.Data[start + off + 7];
            string name = "";
            for (int i = 0; i < descriptor_len - 7; i++)
              name += (char)section.Data[start + off +9+i];
            lchannel.displayName = name;
            pchannel.linkedChannels.Add(lchannel);
            pNode.Nodes.Add("["+name+"] tid: " + lchannel.transport_id.ToString() + " nid: " + lchannel.network_id.ToString() + " sid: " + lchannel.service_id.ToString());
          }
          off += descriptor_len+2;
        } // while (off < descriptors_len)
        start += descriptors_len;
      } // while (start + 11 <= section.section_length)
      if (pNode.Nodes.Count == 0)
        pNode.Remove();
    }
  }
}
