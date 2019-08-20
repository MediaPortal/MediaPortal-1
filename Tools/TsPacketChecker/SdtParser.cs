using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TsPacketChecker
{
  class SdtParser
  {
    private TreeNode _baseNode;
    private List<ulong> _servicesReceived;
    private SectionDecoder _dec1;
    private SectionDecoder _dec2;
    private bool _useSubtables;
    private HashSet<int> _sectionsCompleted = new HashSet<int>();

    public bool IsReady { get; private set; }

    public SdtParser(TreeNode nodeToAdd,bool useSubtables)
    {
      _baseNode = nodeToAdd;
      _useSubtables = useSubtables;
      _servicesReceived = new List<ulong>();
      _dec1 = new SectionDecoder(0x11, 0x42);
      _dec1.OnSectionDecoded += OnNewSection;
      _dec2 = new SectionDecoder(0x11, 0x46);
      _dec2.OnSectionDecoded += OnNewSection;
    }
    public void OnTsPacket(byte[] tsPacket)
    {
      _dec1.OnTsPacket(tsPacket);
      _dec2.OnTsPacket(tsPacket);
    }
    private void DecodeServiceDescriptor(byte[] data, out string provider_name, out string service_name)
    {
      int pos = 0;
      int provider_name_length = data[pos];
      pos ++;
      provider_name = StringUtils.getString468A(data, pos, provider_name_length);
      pos += provider_name_length;
      service_name = "";
      if (pos < data.Length)
      {
        int service_name_length = data[pos];
        pos++;
        service_name = StringUtils.getString468A(data, pos, service_name_length);
      }
    }
    public int GetServiceCount()
    {
      return _servicesReceived.Count;
    }
    public void OnNewSection(Section section)
    {
      if(_useSubtables)
      {
        if (_sectionsCompleted.Count == section.last_section_number + 1)
        {
          IsReady = true;          
          return;
        }

        if (_sectionsCompleted.Contains(section.section_number))
        {          
          return;
        }
      }
      int transport_id = section.table_id_extension;
      int network_id=(section.Data[8]<<8)+section.Data[9];

      ulong nid = (ulong)network_id;
      ulong tid = (ulong)transport_id;

      //ulong key = (ulong)(nid << 32);
      //key += (ulong)(tid << 16);

      int start = 11;
      while (start < section.section_length)
      {
        int service_id = (section.Data[start] << 8) + section.Data[start + 1];
        ulong key = (ulong)service_id;
        int descriptor_loop_len = ((section.Data[start + 3] << 8) | (section.Data[start + 4])) & 0xfff;
        if (!_servicesReceived.Contains(key))
        {
          _servicesReceived.Add(key);
          start += 5;
          int off = 0;
          while (off < descriptor_loop_len)
          {
            int descriptor_tag = section.Data[start + off];
            int descriptor_len = section.Data[start + off + 1];
            if (descriptor_len > 0)
            {
              switch (descriptor_tag)
              {
                case 0x48: // service
                  byte[] desc = new byte[descriptor_len];
                  Array.Copy(section.Data, start + off + 3, desc, 0, descriptor_len - 1);
                  string provider; string service;
                  DecodeServiceDescriptor(desc, out provider, out service);
                  TreeNode node = _baseNode.Nodes.Add("#" + service_id.ToString() + " " + service + " (" + provider + ")");
                  node.Nodes.Add("transport_id: " + transport_id.ToString() + " nid: " + network_id.ToString() + " sid: " + service_id.ToString());
                  if (section.table_id == 0x46)
                    node.Nodes.Add("Other mux");
                  else
                    node.Nodes.Add("Same mux");
                  break;
              }
            }
            off += descriptor_len + 2;
          }
        }
        start += descriptor_loop_len;
      }
      if (_useSubtables)
      {
        _sectionsCompleted.Add(section.section_number);
      }
      if (!_useSubtables)
      { IsReady = true; }
    }
  }
}
