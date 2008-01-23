using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TsPacketChecker
{
  class SdtParser: SectionDecoder
  {
    private TreeNode baseNode;
    private List<ulong> servicesReceived;

    public SdtParser(TreeNode nodeToAdd)
    {
      Pid = 0x11;
      TableId = 0x42;
      baseNode = nodeToAdd;
      servicesReceived = new List<ulong>();
    }
    private void DecodeServiceDescriptor(byte[] data, out string provider_name, out string service_name)
    {
      int pos = 0;
      int provider_name_length = data[pos];
      pos ++;
      provider_name = StringUtils.getString468A(data, pos, provider_name_length);
      //for (int i = 0; i < provider_name_length; i++)
      //  provider_name += (char)data[pos + i];
      pos += provider_name_length;
      int service_name_length = data[pos];
      pos++;
      service_name = StringUtils.getString468A(data, pos, service_name_length);
      //for (int i = 0; i < service_name_length; i++)
      //  service_name += (char)data[pos + i];
    }
    public int GetServiceCount()
    {
      return servicesReceived.Count;
    }
    public override void OnNewSection(Section section)
    {
      if (section.table_id != 0x42 && section.table_id!=0x46) return;

      int transport_id = section.table_id_extension;
      int network_id=(section.Data[8]<<8)+section.Data[9];

      ulong nid = (ulong)network_id;
      ulong tid = (ulong)transport_id;
      ulong key = (ulong)(nid << 32);
      key += (ulong)(tid << 16);

      int start = 11;
      while (start < section.section_length)
      {
        int service_id = (section.Data[start] << 8) + section.Data[start + 1];
        key += (ulong)service_id;
        int descriptor_loop_len = ((section.Data[start + 3] << 8) | (section.Data[start + 4])) & 0xfff;
        if (!servicesReceived.Contains(key))
        {
          servicesReceived.Add(key);
          start += 5;
          int off = 0;
          while (off < descriptor_loop_len)
          {
            int descriptor_tag = section.Data[start + off];
            int descriptor_len = section.Data[start + off + 1];
            switch (descriptor_tag)
            {
              case 0x48: // service
                byte[] desc = new byte[descriptor_len];
                Array.Copy(section.Data, start + off + 3, desc, 0, descriptor_len - 1);
                string provider; string service;
                DecodeServiceDescriptor(desc, out provider, out service);
                TreeNode node=baseNode.Nodes.Add("#"+service_id.ToString()+" "+ service + " (" + provider + ")");
                node.Nodes.Add("tid: " + transport_id.ToString() + " nid: " + network_id.ToString() + " sid: " + service_id.ToString());
                if (section.table_id == 0x46)
                  node.Nodes.Add("Other mux");
                else
                  node.Nodes.Add("Same mux");
                break;
            }
            off += descriptor_len + 2;
          }
        }
        start += descriptor_loop_len;
      }
    }
  }
}
