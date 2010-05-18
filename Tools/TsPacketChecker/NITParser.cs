using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TsPacketChecker
{
  enum NetworkType
  {
    DVB_S,
    DVB_C,
    DVB_T,
    Unknown
  }
  class NetworkInfo
  {
    public int network_id;
    public int transport_id;
    public int service_id;
    public int LCN;
    public int frequency;
    public int bandwidth;
    public int constellation;
    public int HirarchyInformation;
    public int CodeRateHPStream;
    public int CodeRateLPStream;
    public int GuardInterval;
    public int TransmissionMode;
    public int otherFrequencyFlag;
    public float orbitalPosition;
    public int WestEastFlag;
    public int polarisation;
    public int modulation;
    public int symbolrate;
    public int FECInner;
    public int FECOuter;
    public string NetworkName;
    public NetworkType netType;

    public NetworkInfo(string network_name,int transport_id,int network_id)
    {
      netType = NetworkType.Unknown;
      NetworkName = network_name;
      this.transport_id = transport_id;
      this.network_id = network_id;
      LCN = -1;
      service_id = -1;
    }

    #region ...ToStr methods
    private string PolarisationToStr()
    {
      if (polarisation == 0)
        return "HORIZONTAL";
      else
        return "VERTICAL";
    }
    private string FECinnerToStr()
    {
      switch (FECInner)
      {
        case 0:
          return "not defined";
          break;
        case 1:
          return "1/2 conv. code rate";
          break;
        case 2:
          return "2/3 conv. code rate";
          break;
        case 3:
          return "3/4 conv. code rate";
          break;
        case 4:
          return "5/6 conv. code rate";
          break;
        case 5:
          return "7/8 conv. code rate";
          break;
        case 6:
          return "8/9 conv. code rate";
          break;
      }
      return "no conv. coding";
    }
    private string FECouterToStr()
    {
      switch (FECOuter)
      {
        case 0:
          return "not defined";
          break;
        case 1:
          return "no outer FEC coding";
          break;
        case 2:
          return "RS(204/188)";
          break;
      }
      return "other (reserved)";
    }
    private string ModulationToStr()
    {
      switch (modulation)
      {
        case 0:
          return "not defined";
        case 1:
          return "16 QAM";
        case 2:
          return "32 QAM";
        case 3:
          return "64 QAM";
        case 4:
          return "128 QAM";
        case 5:
          return "256 QAM";
      }
      return "not set";
    }
    private string BandwidthToStr()
    {
      switch (bandwidth)
      {
        case 0:
          return "8 MHz";
        case 1:
          return "7 MHz";
        case 2:
          return "6 MHz";
      }
      return "unknown";
    }
    private string ConstellationToStr()
    {
      switch (constellation)
      {
        case 0:
          return "QPSK";
        case 1:
          return "16 QAM";
        case 2:
          return "64 QAM";
      }
      return "unknown";
    }
    private string HierarchyToStr()
    {
      switch (HirarchyInformation)
      {
        case 0:
          return "non hierarchical";
        case 1:
          return "1";
        case 2:
          return "2";
        case 3:
          return "4";
      }
      return "unknown";
    }
    private string CoderateToStr(int coderate)
    {
      switch (coderate)
      {
        case 0:
          return "1/2";
        case 1:
          return "2/3";
        case 2:
          return "3/4";
        case 3:
          return "5/6";
        case 4:
          return "7/8";
      }
      return "unknown";
    }
    private string GuardIntervalToStr()
    {
      switch (GuardInterval)
      {
        case 0:
          return "1/32";
        case 1:
          return "1/16";
        case 2:
          return "1/8";
        case 3:
          return "1/4";
      }
      return "unknown";
    }
    private string TransmissionModeToStr()
    {
      switch (TransmissionMode)
      {
        case 0:
          return "2k Mode";
        case 1:
          return "8k Mode";
      }
      return "unknown";
    }
    #endregion

    public void AddToNode(TreeNode cnode,bool other_network)
    {
      TreeNode node = cnode.Nodes.Add("Frequency: " + frequency.ToString()+" (nid: " + network_id.ToString() + " tid: " + transport_id.ToString() + " sid: " + service_id.ToString()+" LCN: "+LCN.ToString()+")");
      if (NetworkName != "")
        node.Text = "[" + NetworkName + "] " + node.Text;
      if (other_network)
        node.Text = "Other Network " + node.Text;
      switch (netType)
      {
        case NetworkType.DVB_S:
          node.Nodes.Add("Symbolrate: " + symbolrate.ToString());
          node.Nodes.Add("Modulation: " + ModulationToStr());
          node.Nodes.Add("FECinner: " + FECinnerToStr());
          node.Nodes.Add("Polarisation: " + PolarisationToStr());
          float opos = orbitalPosition / 10;
          node.Nodes.Add("OrbitalPosition: "+opos.ToString());
          node.Nodes.Add("WestEastFlag: " + WestEastFlag.ToString());
          break;
        case NetworkType.DVB_C:
          node.Nodes.Add("Symbolrate: " + symbolrate.ToString());
          node.Nodes.Add("Modulation: " + ModulationToStr());
          node.Nodes.Add("FECinner: " + FECinnerToStr());
          node.Nodes.Add("FECouter: " + FECouterToStr());
          break;
        case NetworkType.DVB_T:
          node.Nodes.Add("Bandwidth: " + BandwidthToStr());
          node.Nodes.Add("Constellation: " + ConstellationToStr());
          node.Nodes.Add("HirarchyInfo: " + HierarchyToStr());
          node.Nodes.Add("Coderate HP: " + CoderateToStr(CodeRateHPStream));
          node.Nodes.Add("Coderate LP: " + CoderateToStr(CodeRateLPStream));
          node.Nodes.Add("GuardInterval: " + GuardIntervalToStr());
          node.Nodes.Add("TransmissionMode: " + TransmissionModeToStr());
          node.Nodes.Add("OtherFrequencyFlag: " + otherFrequencyFlag.ToString());
          break;
      }
    }
  }

  class NITParser 
  {
    NetworkInfo netInfo;
    TreeNode baseNode;
    List<int> seenNITs;
    SectionDecoder dec1;
    SectionDecoder dec2;

    public NITParser(TreeNode nodeToAdd)
    {
      baseNode = nodeToAdd;
      seenNITs = new List<int>();
      dec1 = new SectionDecoder(0x10, 0x40);
      dec1.OnSectionDecoded += OnNewSection;
      dec2 = new SectionDecoder(0x10, 0x41);
      dec2.OnSectionDecoded += OnNewSection;
    }

    public void OnTsPacket(byte[] tsPacket)
    {
      dec1.OnTsPacket(tsPacket);
      dec2.OnTsPacket(tsPacket);
    }

    #region Descriptor decoders
    private void DVB_GetLogicalChannelNumber(int original_network_id, int transport_stream_id, byte[] buf, int start)
    {
      netInfo.service_id = (0x100 * buf[start]) + buf[start + 1];
      netInfo.LCN = (0x100 * (buf[start + 2] & 0x3)) + buf[start + 3];
      return;
      // 32 bits per record
      int n = buf[start + 1] / 4;
      if (n < 1)
        return;

      // desc id, desc len, (service id, service number)
      int pointer = start + 2;
      int ServiceID, LCN;
      for (int i = 0; i < n; i++)
      {
        //service id:16
        //visible_service_flag:1
        //reserved:5
        //logical channel number:10
        ServiceID = 0;
        LCN = 0;
        ServiceID = (buf[pointer + 0] << 8) + (buf[pointer + 1] & 0xff);
        LCN = ((buf[pointer + 2] & 0x03) << 8) + (buf[pointer + 3] & 0xff);
        if (LCN == 0)
          LCN = 10000;//undefined
        else
          if (LCN >= 1000)
            LCN = 10000;//reserved

        pointer += 4;
        if (original_network_id > 0 && transport_stream_id > 0 && ServiceID > 0 && LCN >= 0)
        {
          netInfo.LCN = LCN;
          netInfo.service_id = ServiceID;
        }
      }
    }
    private void DVB_GetSatDelivSys(byte[] buf, int start, int maxLen)
    {
      byte[] b = new byte[maxLen];
      Array.Copy(buf, start, b, 0, maxLen);
      if (b[0] != 0x43 || maxLen < 13) return;

      int descriptor_tag = b[0];
      int descriptor_length = b[1];

      if (descriptor_length > 13) return;

      netInfo.netType = NetworkType.DVB_S;

      netInfo.frequency = (10000000 * ((b[2] >> 4) & 0xf));
      netInfo.frequency += (1000000 * ((b[2] & 0xf)));
      netInfo.frequency += (100000 * ((b[3] >> 4) & 0xf));
      netInfo.frequency += (10000 * ((b[3] & 0xf)));
      netInfo.frequency += (1000 * ((b[4] >> 4) & 0xf));
      netInfo.frequency += (100 * ((b[4] & 0xf)));
      netInfo.frequency += (10 * ((b[5] >> 4) & 0xf));
      netInfo.frequency += (b[5] & 0xf);

      netInfo.orbitalPosition += (1000 * ((b[6] >> 4) & 0xf));
      netInfo.orbitalPosition += (100 * ((b[6] & 0xf)));
      netInfo.orbitalPosition += (10 * ((b[7] >> 4) & 0xf));
      netInfo.orbitalPosition += (b[7] & 0xf);

      netInfo.WestEastFlag = (b[8] & 0x80) >> 7;
      netInfo.polarisation = (b[8] & 0x60) >> 5;

      netInfo.modulation = (b[8] & 0x1F);
      netInfo.symbolrate = (1000000 * ((b[9] >> 4) & 0xf));
      netInfo.symbolrate += (100000 * ((b[9] & 0xf)));
      netInfo.symbolrate += (10000 * ((b[10] >> 4) & 0xf));
      netInfo.symbolrate += (1000 * ((b[10] & 0xf)));
      netInfo.symbolrate += (100 * ((b[11] >> 4) & 0xf));
      netInfo.symbolrate += (10 * ((b[11] & 0xf)));
      netInfo.symbolrate += (((b[12] >> 4) & 0xf));
      netInfo.FECInner = (b[12] & 0xF);
    }
    private void DVB_GetTerrestrialDelivSys(byte[] buf, int start, int maxLen)
    {
      byte[] b = new byte[maxLen];
      Array.Copy(buf, start, b, 0, maxLen);
      if (b[0] == 0x5A)
      {
        int descriptor_tag = b[0];
        int descriptor_length = b[1];
        if (descriptor_length > 11) return;

        netInfo.netType = NetworkType.DVB_T;

        netInfo.frequency = (b[2] << 24) + (b[3] << 16) + (b[4] << 8) + b[5];

        if (netInfo.frequency < 40000000 || netInfo.frequency > 900000000) return; // invalid frequency

        netInfo.bandwidth = (b[6] >> 5);

        netInfo.constellation = (b[7] >> 6);

        netInfo.HirarchyInformation = (b[7] >> 3) & 7;
        netInfo.CodeRateHPStream = (b[7] & 7);
        netInfo.CodeRateLPStream = (b[8] >> 5);


        netInfo.GuardInterval = (b[8] >> 3) & 3;


        netInfo.TransmissionMode = (b[8] >> 1) & 3;

        netInfo.otherFrequencyFlag = (b[8] & 3);
        // 0 - no other frequency in use
      }
    }
    private void DVB_GetCableDelivSys(byte[] buf, int start, int maxLen)
    {
      byte[] b = new byte[maxLen];
      Array.Copy(buf, start, b, 0, maxLen);

      if (b[0] != 0x44 || maxLen < 13) return;

      int descriptor_tag = b[0];
      int descriptor_length = b[1];
      if (descriptor_length > 13) return;

      netInfo.netType = NetworkType.DVB_C;

      netInfo.frequency = (10000000 * ((b[2] >> 4) & 0xf));
      netInfo.frequency += (1000000 * ((b[2] & 0xf)));
      netInfo.frequency += (100000 * ((b[3] >> 4) & 0xf));
      netInfo.frequency += (10000 * ((b[3] & 0xf)));
      netInfo.frequency += (1000 * ((b[4] >> 4) & 0xf));
      netInfo.frequency += (100 * ((b[4] & 0xf)));
      netInfo.frequency += (10 * ((b[5] >> 4) & 0xf));
      netInfo.frequency += (b[5] & 0xf);

      netInfo.FECOuter = (b[7] & 0xF);

      netInfo.modulation = b[8];

      netInfo.symbolrate = (1000000 * ((b[9] >> 4) & 0xf));
      netInfo.symbolrate += (100000 * ((b[9] & 0xf)));
      netInfo.symbolrate += (10000 * ((b[10] >> 4) & 0xf));
      netInfo.symbolrate += (1000 * ((b[10] & 0xf)));
      netInfo.symbolrate += (100 * ((b[11] >> 4) & 0xf));
      netInfo.symbolrate += (10 * ((b[11] & 0xf)));
      netInfo.symbolrate += (((b[12] >> 4) & 0xf));

      netInfo.FECInner = (b[12] & 0xF);
    }
    #endregion

    public int GetChannelCount()
    {
      return seenNITs.Count;
    }

    public void OnNewSection(Section section)
    {
      int network_descriptor_length = ((section.Data[8] & 0xF) << 8) + section.Data[9];

      int l1 = network_descriptor_length;
      int pointer = 10;
      string network_name = "";

      while (l1 > 0)
      {
        int descriptor_tag = section.Data[pointer];
        int x = section.Data[pointer + 1] + 2;
        if (descriptor_tag == 0x40)
          network_name = StringUtils.getString468A(section.Data, pointer + 2, x - 2);
        l1 -= x;
        pointer += x;
      }

      pointer = 10 + network_descriptor_length;

      if (pointer > section.section_length) return;

      int transport_stream_loop_length = ((section.Data[pointer] & 0xF) << 8) + section.Data[pointer + 1];
      l1 = transport_stream_loop_length;
      pointer += 2;

      while (l1 > 0)
      {
        if (pointer + 2 > section.section_length) return;

        int transport_stream_id = (section.Data[pointer] << 8) + section.Data[pointer + 1];
        int original_network_id = (section.Data[pointer + 2] << 8) + section.Data[pointer + 3];

        ulong key = (ulong)(original_network_id << 16);
        key += (ulong)transport_stream_id;

        int transport_descriptor_length = ((section.Data[pointer + 4] & 0xF) << 8) + section.Data[pointer + 5];
        netInfo = new NetworkInfo(network_name, transport_stream_id, original_network_id);
        pointer += 6;
        l1 -= 6;
        int l2 = transport_descriptor_length;

        while (l2 > 0)
        {
          if (pointer + 2 > section.section_length) return;

          int descriptor_tag = section.Data[pointer];
          int descriptor_length = section.Data[pointer + 1] + 2;

          switch (descriptor_tag)
          {
            case 0x43: // sat
              DVB_GetSatDelivSys(section.Data, pointer, descriptor_length);
              break;
            case 0x44: // cable
              DVB_GetCableDelivSys(section.Data, pointer, descriptor_length);
              break;
            case 0x5A: // terrestrial
              DVB_GetTerrestrialDelivSys(section.Data, pointer, descriptor_length);
              break;
            case 0x83: // logical channel number
              DVB_GetLogicalChannelNumber(original_network_id, transport_stream_id, section.Data, pointer);
              break;
          }
          pointer += descriptor_length;
          l2 -= descriptor_length;
          l1 -= descriptor_length;
        }
        if (netInfo.netType!=NetworkType.Unknown)
          baseNode.Text = "NIT " + netInfo.netType.ToString();
        if (!seenNITs.Contains(netInfo.frequency))
        {
          netInfo.AddToNode(baseNode,(section.table_id==0x41));
          seenNITs.Add(netInfo.frequency);
        }
      }
    }
  }
}
