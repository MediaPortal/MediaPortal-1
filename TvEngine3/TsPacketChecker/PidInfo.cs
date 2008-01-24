using System;
using System.Collections.Generic;
using System.Text;

namespace TsPacketChecker
{
  public class PidInfo
  {
    public bool shouldCheck;
    public Pcr lastPcr;
    public Pcr lastPts;
    public byte continuityCounter;
    public List<string> pcrErrorTexts;
    public List<string> ptsErrorTexts;
    public List<string> payloadStartErrorTexts;
    public PidInfo()
    {
      shouldCheck = false;
      lastPcr = new Pcr();
      lastPts = new Pcr();
      continuityCounter = 0xFF;
      pcrErrorTexts = new List<string>();
      ptsErrorTexts = new List<string>();
      payloadStartErrorTexts = new List<string>();
    }
    public bool HasErrors()
    {
      return (pcrErrorTexts.Count > 0 || ptsErrorTexts.Count > 0 || payloadStartErrorTexts.Count>0);
    }
  }
}
