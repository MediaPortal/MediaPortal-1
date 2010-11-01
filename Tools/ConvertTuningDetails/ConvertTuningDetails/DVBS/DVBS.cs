using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using DirectShowLib.BDA;
using TvLibrary.Log;
using System.IO;

namespace ConvertTuningDetails
{
    [Serializable]
    public class Transponder : IComparable<Transponder>
    {
      public int CarrierFrequency; // frequency
      public Polarisation Polarisation;  // polarisation 0=hori, 1=vert
      public int SymbolRate; // symbol rate
      public ModulationType Modulation = ModulationType.ModNotSet;
      public BinaryConvolutionCodeRate InnerFecRate = BinaryConvolutionCodeRate.RateNotSet;
      public Pilot Pilot = Pilot.NotSet;
      public RollOff Rolloff = RollOff.NotSet;

      public int CompareTo(Transponder other)
      {
        if (Polarisation < other.Polarisation)
          return 1;
        if (Polarisation > other.Polarisation)
          return -1;
        if (CarrierFrequency > other.CarrierFrequency)
          return 1;
        if (CarrierFrequency < other.CarrierFrequency)
          return -1;
        if (SymbolRate > other.SymbolRate)
          return 1;
        if (SymbolRate < other.SymbolRate)
          return -1;
        return 0;
      }
      public override string ToString()
      {
        return String.Format("{0} {1} {2} {3} {4}", CarrierFrequency, SymbolRate, Polarisation, Modulation, InnerFecRate);
      }
    }

  class DVBS
  {
    private static bool dvbs2 = false;

    static public string MakeFileName(string strText)
    {
      if (strText == null) return String.Empty;
      if (strText.Length == 0) return String.Empty;
      string strFName = strText.Replace(':', '_');
      strFName = strFName.Replace('/', '_');
      strFName = strFName.Replace('\\', '_');
      strFName = strFName.Replace('*', '_');
      strFName = strFName.Replace('?', '_');
      strFName = strFName.Replace('\"', '_');
      strFName = strFName.Replace('<', '_'); ;
      strFName = strFName.Replace('>', '_');
      strFName = strFName.Replace('|', '_');
      return strFName;
    }

    static public string MakeDVBTName(string fileName)
    {
      String safeName = MakeFileName(fileName);
      if (safeName.Contains(" - "))
      {
        Regex re = new Regex(" - ");
        safeName = re.Replace(safeName, ".", 1);
      }
      else
      {
        safeName += ".All Regions";
      }
      return safeName;
    }

    static List<Transponder> _transponders;
    public static void ConvertList(string tsfilename, bool AppendMode)
    {
      //set filename of transpoder file
      if (!AppendMode || _transponders == null)
      {
        _transponders = new List<Transponder>();
      }
      string line;
      string[] tpdata;
      // load transponder list and start scan
      // first check if we are to look for the dvbs2 scanning file with the -S2 extension
      if (!AppendMode)
      {
        Log.Info("DVBS: Also using DVB-S2 transponder scanning information");
        string transpondername = Path.GetFileNameWithoutExtension(tsfilename).ToLowerInvariant();
        //@"\Tuningparameters\"
        string ts2filename = String.Format(@"{0}\{1}-S2.ini", Path.GetDirectoryName(tsfilename), transpondername);
        if (File.Exists(ts2filename))
        {
          // joins both lists
          ConvertList(ts2filename, true);
        }
      }
      TextReader tin = File.OpenText(tsfilename);
      do
      {
        line = tin.ReadLine();
        if (line != null)
        {
          line = line.Trim();
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            if (line.Contains("S2"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length >= 3)
            {
              if (tpdata[0].IndexOf("=") >= 0)
              {
                tpdata[0] = tpdata[0].Substring(tpdata[0].IndexOf("=") + 1);
              }
              try
              {
                Transponder transponder = new Transponder();
                transponder.CarrierFrequency = Int32.Parse(tpdata[0]) * 1000;
                switch (tpdata[1].ToLowerInvariant())
                {
                  case "v":
                    transponder.Polarisation = Polarisation.LinearV;
                    break;
                  case "h":
                    transponder.Polarisation = Polarisation.LinearH;
                    break;
                  case "r":
                    transponder.Polarisation = Polarisation.CircularR;
                    break;
                  case "l":
                    transponder.Polarisation = Polarisation.CircularL;
                    break;
                  default:
                    transponder.Polarisation = Polarisation.LinearH;
                    break;
                }
                transponder.SymbolRate = Int32.Parse(tpdata[2]);
                for (int idx = 3; idx < tpdata.Length; ++idx)
                {
                  string fieldValue = tpdata[idx].ToLowerInvariant();
                  if (fieldValue == "8psk")
                    transponder.Modulation = ModulationType.Mod8Psk;
                  if (fieldValue == "qpsk")
                    transponder.Modulation = ModulationType.ModQpsk;
                  if (fieldValue == "16apsk")
                    transponder.Modulation = ModulationType.Mod16Apsk;
                  if (fieldValue == "32apsk")
                    transponder.Modulation = ModulationType.Mod32Apsk;

                  if (fieldValue == "12")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_2;
                  if (fieldValue == "23")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_3;
                  if (fieldValue == "34")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_4;
                  if (fieldValue == "35")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_5;
                  if (fieldValue == "45")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate4_5;
                  if (fieldValue == "511")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_11;
                  if (fieldValue == "56")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_6;
                  if (fieldValue == "78")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate7_8;
                  if (fieldValue == "14")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_4;
                  if (fieldValue == "13")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_3;
                  if (fieldValue == "25")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_5;
                  if (fieldValue == "67")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate6_7;
                  if (fieldValue == "89")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate8_9;
                  if (fieldValue == "910")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate9_10;

                  if (fieldValue == "1/2")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_2;
                  if (fieldValue == "2/3")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_3;
                  if (fieldValue == "3/4")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_4;
                  if (fieldValue == "3/5")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_5;
                  if (fieldValue == "4/5")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate4_5;
                  if (fieldValue == "5/11")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_11;
                  if (fieldValue == "5/6")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_6;
                  if (fieldValue == "7/8")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate7_8;
                  if (fieldValue == "1/4")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_4;
                  if (fieldValue == "1/3")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_3;
                  if (fieldValue == "2/5")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_5;
                  if (fieldValue == "6/7")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate6_7;
                  if (fieldValue == "8/9")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate8_9;
                  if (fieldValue == "9/10")
                    transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate9_10;

                  if (fieldValue == "off")
                    transponder.Pilot = Pilot.Off;
                  if (fieldValue == "on")
                    transponder.Pilot = Pilot.On;

                  if (fieldValue == "0.20")
                    transponder.Rolloff = RollOff.Twenty;
                  if (fieldValue == "0.25")
                    transponder.Rolloff = RollOff.TwentyFive;
                  if (fieldValue == "0.35")
                    transponder.Rolloff = RollOff.ThirtyFive;
                }
                _transponders.Add(transponder);
              }
              catch { }
            }
          }
        }
      }
      while (!(line == null));
      tin.Close();
      if (!AppendMode)
      {
        String newPath = String.Format("{0}\\dvbs\\{1}.xml", System.IO.Path.GetDirectoryName(tsfilename), Path.GetFileNameWithoutExtension(tsfilename));
        System.IO.TextWriter parFileXML = System.IO.File.CreateText(newPath);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Transponder>));
        xmlSerializer.Serialize(parFileXML, _transponders);
        parFileXML.Close();

        _transponders.Clear();
      }
    }
  }
}
