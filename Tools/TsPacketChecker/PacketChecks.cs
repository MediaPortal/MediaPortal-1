using System;
using System.Collections.Generic;
using System.Text;

namespace TsPacketChecker
{
  class PacketChecker
  {
    private long droppedPackets;
    private long nullPackets;
    private long totalCCErrors;
    private long totalPcrErrors;
    private long totalPtsErrors;
    private long totalPayloadStartErrors;
    private double diffAllowed;
    private SortedDictionary<ushort, PidInfo> pids = new SortedDictionary<ushort, PidInfo>();

    #region Existence checks
    private bool PacketContainsPcr(TsHeader header, byte[] tsPacket)
    {
      return (header.HasAdaptionField && (tsPacket[5] & 0x10) == 0x10);
    }
    private ulong PacketContainsPtsDts(TsHeader header, byte[] tsPacket, ref PidInfo pi)
    {
      if (!header.PayloadUnitStart)
        return 0;
      if (header.PayLoadStart > 185 && header.HasPayload)
      {
        totalPayloadStartErrors++;
        pi.payloadStartErrorTexts.Add(" payloadStart=" + header.PayLoadStart.ToString() + " HasAdaption=" + header.HasAdaptionField.ToString() + " HasPayload=" + header.HasPayload.ToString() + " AdaptionFieldSize=" + tsPacket[4].ToString());
        return 0;
      }
      if (!header.HasPayload)
        return 0;
      return header.PayLoadStart;
    }
    #endregion

    #region Continuity checks
    private bool CheckContinuityCounter(byte tsPacket3, ref PidInfo pi)
    {
      bool isOk = true;
      byte cc = (byte)(tsPacket3 & 0xF);
      if (pi.continuityCounter != 0xFF)
      {
        byte afc = (byte)((tsPacket3 >> 4) & 0x3);
        byte expected;
        // ISO/IEC 13818-1: "The continuity_counter shall not be incremented when the adaptation_field_control of the packet equals '00' or '10'." (binary)
        if ((afc.Equals(0)) || (afc.Equals(2))) expected = (byte)(pi.continuityCounter);
        else expected = (byte)(pi.continuityCounter + 1);
        if (expected == 16) expected = 0;
        if (cc != expected)
        {
          totalCCErrors++;
          isOk = false;
        }
      }
      pi.continuityCounter = cc;
      return isOk;
    }
    private void CheckPcr(TsHeader header, byte[] tsPacket, ref PidInfo pi)
    {
      if (!pi.shouldCheck) return;
      if (!PacketContainsPcr(header, tsPacket)) return;
      Pcr pcr = new Pcr(tsPacket);
      if (pi.lastPcr.isValid)
      {
        TimeSpan diff = pcr.ToDateTime() - pi.lastPcr.ToDateTime();
        if (diff.TotalSeconds > diffAllowed)
        {
          pi.pcrErrorTexts.Add("last pcr: " + pi.lastPcr.ToDateTime().ToString("HH:MM:ss") + " current pcr: " + pcr.ToDateTime().ToString("HH:MM:ss"));
          totalPcrErrors++;
        }
      }
      pi.lastPcr = pcr;
    }
    private void CheckPtsDts(TsHeader header, byte[] tsPacket, ref PidInfo pi)
    {
      if (!pi.shouldCheck) return;
      ulong offset = PacketContainsPtsDts(header, tsPacket, ref pi);
      if (offset == 0) return;
      Pcr pts; Pcr dts;
      PcrUtils.DecodePtsDts(tsPacket, offset, out pts, out dts);
      if (pi.lastPts.isValid)
      {
        TimeSpan diff = pts.ToDateTime() - pi.lastPts.ToDateTime();
        if (diff.TotalSeconds > diffAllowed)
        {
          pi.ptsErrorTexts.Add("last pts: " + pi.lastPts.ToDateTime().ToString("HH:MM:ss") + " current pts: " + pts.ToDateTime().ToString("HH:MM:ss"));
          totalPtsErrors++;
        }
      }
      pi.lastPts = pts;
    }
    #endregion

    #region Constructor
    public PacketChecker(double maxAllowedPcrDiff)
    {
      droppedPackets = 0;
      nullPackets = 0;
      totalCCErrors = 0;
      totalPcrErrors = 0;
      totalPtsErrors = 0;
      totalPayloadStartErrors = 0;
      diffAllowed = maxAllowedPcrDiff;
      pids = new SortedDictionary<ushort, PidInfo>();
    }
    #endregion

    #region Public methods
    public void AddPidsToCheck(List<ushort> streamPids)
    {
      foreach (ushort pid in streamPids)
      {
        if (pids.ContainsKey(pid))
          pids[pid].shouldCheck = true;
      }
    }
    public void ProcessPacket(byte[] tsPacket, TsHeader header)
    {
      if (header.TransportError)
      {
        droppedPackets++;
        return;
      }
      if (!pids.ContainsKey(header.Pid))
        pids.Add(header.Pid, new PidInfo());

      PidInfo pi = pids[header.Pid];
      // ISO/IEC 13818-1: "In the case of a null packet the value of the continuity_counter is undefined."
      if (header.Pid == 0x1fff) nullPackets++;
      else CheckContinuityCounter((byte)(tsPacket[3]), ref pi); // check continuity_counter if no null packet
      if (header.Pid > 0x1f) // don't check pids which contain SI information
      {
        CheckPcr(header, tsPacket, ref pi);
        CheckPtsDts(header, tsPacket, ref pi);
      }
      pids[header.Pid] = pi;
    }

    public string GetStatistics()
    {
      return "null packets=" + nullPackets.ToString() + " dropped packets=" + droppedPackets.ToString() + " cc errors=" + totalCCErrors.ToString() + " pcr holes=" + totalPcrErrors.ToString() + " pts holes=" + totalPtsErrors.ToString() + " total payloadstart errors=" + totalPayloadStartErrors.ToString();
    }
    public string GetErrorDetails()
    {
      string log = "";
      foreach (ushort pid in pids.Keys)
      {
        if (pids[pid].HasErrors())
        {
          PidInfo pi = pids[pid];
          log += "Pid 0x" + pid.ToString("x") + Environment.NewLine;
          if (pi.pcrErrorTexts.Count > 0)
          {
            log += "  - pcr errors=" + pi.pcrErrorTexts.Count.ToString() + Environment.NewLine;
            foreach (string s in pi.pcrErrorTexts)
              log = "        ->" + s + Environment.NewLine;
          }
          if (pi.ptsErrorTexts.Count > 0)
          {
            log += "  - pts errors=" + pi.ptsErrorTexts.Count.ToString() + Environment.NewLine;
            foreach (string s in pi.ptsErrorTexts)
              log += "        ->" + s + Environment.NewLine;
          }
          if (pi.payloadStartErrorTexts.Count > 0)
          {
            log += "  - payloadStart errors=" + pi.payloadStartErrorTexts.Count.ToString() + Environment.NewLine;
            foreach (string s in pi.payloadStartErrorTexts)
              log += "        ->" + s + Environment.NewLine;
          }
        }
      }
      return log;
    }
    #endregion
  }
}

