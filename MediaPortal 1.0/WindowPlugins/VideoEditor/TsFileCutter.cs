using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WindowPlugins.VideoEditor;
using MediaPortal.GUI.Library;

namespace TsCutterPackage
{
  class TsFileCutter
  {
    #region Consts
    const long READER_PACKETS_TO_BUFFER = 50000;
    const long WRITER_PACKETS_TO_BUFFER = 70000;
    #endregion

    #region Variables
    private BufferedTsFileReader tsReader;
    private BufferedStream writer;
    private List<TimeDomain> _cutPoints;
    private TimeDomain lastCutpoint;
    private TimeDomain currentCutpoint;
    private TimeSpan pcrDiff=new TimeSpan(0);
    public SortedDictionary<ushort, byte> pids;
    #endregion 

    #region Events
    public delegate void Progress(int percentage);
    public event Progress OnProgress;
    public delegate void InitFailed();
    public event Finished OnInitFailed;
    public delegate void Finished();
    public event Finished OnFinished;
    #endregion

    #region Private members
    private bool PacketContainsPcr(byte[] tsPacket)
    {
      // a packet is considered to contain a pcr value if:
      // - the adaption control field is > 1 (or != PAYLOAD_ONLY)
      // - and the PCR flag is set

      // get the adaption control field
      int adaptionControl = (tsPacket[3] >> 4) & 0x3;
      // do we have an adaption field and is the pcr flag set?
      return (adaptionControl > 1 && (tsPacket[5] & 0x10) == 0x10);
    }
    private ulong PacketContainsPtsDts(byte[] tsPacket)
    {
      bool payloadUnitStart = ((tsPacket[1] & 0x40) == 0x40);
      if (!payloadUnitStart)
        return 0;
      ulong payloadStart = 4;
      int adaptionControl = (tsPacket[3] >> 4) & 0x3;
      if (adaptionControl > 1)
        payloadStart = (ulong)tsPacket[4] + 5;
      else
      {
        if (tsPacket[4] == 0 && tsPacket[5] == 0 && tsPacket[6] == 1)
          payloadStart = 4;
        else
          payloadStart = (ulong)tsPacket[4] + 5;
      }
      if (tsPacket[payloadStart] != 0 || tsPacket[payloadStart + 1] != 0 || tsPacket[payloadStart + 2] != 1)
        return 0;
      else
        return payloadStart;
    }
    private bool EvalCutPoints(TimeSpan currentPos)
    {
      foreach (TimeDomain td in _cutPoints)
      {
        if (currentPos>=td.StartTimeSp && currentPos <= td.EndTimeSp)
        {
          currentCutpoint = td;
          return false;
        }
      }
      
      if (lastCutpoint.StartTimeSp != currentCutpoint.StartTimeSp && lastCutpoint.EndTimeSp != currentCutpoint.EndTimeSp)
      {
        TimeSpan cutDiff = currentCutpoint.EndTimeSp - currentCutpoint.StartTimeSp;
        pcrDiff=pcrDiff.Add(cutDiff);
      }
      lastCutpoint = currentCutpoint;
      return true;
    }
    private void PatchContinuityCounter(ref byte[] tsPacket)
    {
      ushort pid = (ushort)(((tsPacket[1] << 8) + tsPacket[2]) & 0x1fff);
      byte continuityCounter = (byte)(tsPacket[3] & 0xF);
      if (!pids.ContainsKey(pid))
        pids[pid] = continuityCounter;
      else
          continuityCounter = pids[pid];
        if (continuityCounter == 15)
          continuityCounter = 0;
        else
          continuityCounter++;
      pids[pid] = continuityCounter;
      tsPacket[3] = (byte)((continuityCounter | (tsPacket[3]&0xF0)));
    }
    #endregion

    #region Public members
    public bool InitStreams(string inputFile, string outputFile,List<TimeDomain> cutPoints)
    {
      tsReader = new BufferedTsFileReader();
      Log.Info("TsFileCutter: Opening input file {0}...", inputFile);
      if (!tsReader.Open(inputFile, READER_PACKETS_TO_BUFFER))
      {
        Log.Error("TsFileCutter: Failed to open input file");
        if (OnInitFailed != null)
          OnInitFailed();
        return false;
      }
      Log.Info("TsFileCutter: Done. Seeking to begin of first packet...");
      if (!tsReader.SeekToFirstPacket())
      {
        Log.Error("TsFileCutter: No sync byte (0x47) found. Doesn't seem to be a .ts file");
        if (OnInitFailed != null)
          OnInitFailed();
        return false;
      }
      Log.Info("TsFileCutter: Done. Creating output file...");
      try
      {
        writer = new BufferedStream(new FileStream(outputFile, FileMode.Create),(int)WRITER_PACKETS_TO_BUFFER);
      }
      catch (Exception ex)
      {
        tsReader.Close();
        Log.Error("TsFileCutter: Failed to create output file with message {0}", ex.Message);
        if (OnInitFailed != null)
          OnInitFailed();
        return false;
      }
      Log.Info("TsFileCutter: Init done.");
      _cutPoints = cutPoints;
      currentCutpoint = new TimeDomain(0, 0);
      lastCutpoint = new TimeDomain(0, 0);
      return true;
    }
    public void Cut()
    {
      Log.Info("TsFileCutter: Starting to cut the file");
      byte[] tsPacket;
      bool isValid;
      Pcr startPcr = new Pcr();
      pcrDiff = new TimeSpan(0);
      bool writePacket = false;
      pids = new SortedDictionary<ushort, byte>();

      while (tsReader.GetNextPacket(out tsPacket, out isValid))
      {
        if (!isValid) continue;
        if (PacketContainsPcr(tsPacket))
        {
          Pcr pcr = new Pcr(tsPacket);
          if (!startPcr.isValid)
            startPcr = pcr;
          writePacket = EvalCutPoints(pcr.ToDateTime() - startPcr.ToDateTime());
          if (writePacket)
            PcrUtils.PatchPcr(ref tsPacket, pcr.ToDateTime().AddMilliseconds(pcrDiff.TotalMilliseconds * -1).TimeOfDay);
        }
        if (writePacket)
        {
          PatchContinuityCounter(ref tsPacket);
          ulong payloadStart = PacketContainsPtsDts(tsPacket);
          if (payloadStart > 0)
          {
            Pcr pts; Pcr dts;
            PcrUtils.DecodePtsDts(tsPacket, payloadStart, out pts, out dts);
            if (pts.isValid)
              PcrUtils.PatchPts(ref tsPacket, payloadStart, pts.ToDateTime().AddMilliseconds(pcrDiff.TotalMilliseconds * -1).TimeOfDay);
            if (dts.isValid)
              PcrUtils.PatchDts(ref tsPacket, payloadStart, dts.ToDateTime().AddMilliseconds(pcrDiff.TotalMilliseconds * -1).TimeOfDay);
          }
          writer.Write(tsPacket, 0, tsPacket.Length);
        }
        if (OnProgress != null)
          OnProgress(tsReader.GetPositionInPercent());
      }
      tsReader.Close();
      writer.Close();
      if (OnProgress != null)
        OnProgress(100);
      Log.Info("TsFileCutter: Finished cutting the file");
      if (OnFinished != null)
        OnFinished();
    }
    #endregion
  }
}
