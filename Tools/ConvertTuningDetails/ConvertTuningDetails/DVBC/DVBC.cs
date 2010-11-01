using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using DirectShowLib.BDA;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace ConvertTuningDetails
{
  class DVBC
  {
    static public string MakeDVBCName(string fileName)
    {
      String safeName = DVBT.MakeFileName(fileName);
      if (safeName.Contains("-"))
      {
        Regex re = new Regex("-");
        safeName = re.Replace(safeName, ".", 1);
      }
      else
      {
        safeName = "Global."+safeName;
      }
      return safeName;
    }


    public static bool ConvertList(String fileName)   
    {
      List<DVBCTuning> _dvbcChannels = new List<DVBCTuning>();
      string line;
      string[] tpdata;
      System.IO.TextReader tin = System.IO.File.OpenText(fileName);
      do
      {
        line = tin.ReadLine();
        if (line != null)
        {
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length != 3)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 3)
            {
              try
              {
                DVBCTuning tuning = new DVBCTuning();
                tuning.Frequency = Int32.Parse(tpdata[0]);
                string mod = tpdata[1].ToUpper();
                switch (mod)
                {
                  case "1024QAM":
                    tuning.ModulationType = ModulationType.Mod1024Qam;
                    break;
                  case "112QAM":
                    tuning.ModulationType = ModulationType.Mod112Qam;
                    break;
                  case "128QAM":
                    tuning.ModulationType = ModulationType.Mod128Qam;
                    break;
                  case "160QAM":
                    tuning.ModulationType = ModulationType.Mod160Qam;
                    break;
                  case "16QAM":
                    tuning.ModulationType = ModulationType.Mod16Qam;
                    break;
                  case "16VSB":
                    tuning.ModulationType = ModulationType.Mod16Vsb;
                    break;
                  case "192QAM":
                    tuning.ModulationType = ModulationType.Mod192Qam;
                    break;
                  case "224QAM":
                    tuning.ModulationType = ModulationType.Mod224Qam;
                    break;
                  case "256QAM":
                    tuning.ModulationType = ModulationType.Mod256Qam;
                    break;
                  case "320QAM":
                    tuning.ModulationType = ModulationType.Mod320Qam;
                    break;
                  case "384QAM":
                    tuning.ModulationType = ModulationType.Mod384Qam;
                    break;
                  case "448QAM":
                    tuning.ModulationType = ModulationType.Mod448Qam;
                    break;
                  case "512QAM":
                    tuning.ModulationType = ModulationType.Mod512Qam;
                    break;
                  case "640QAM":
                    tuning.ModulationType = ModulationType.Mod640Qam;
                    break;
                  case "64QAM":
                    tuning.ModulationType = ModulationType.Mod64Qam;
                    break;
                  case "768QAM":
                    tuning.ModulationType = ModulationType.Mod768Qam;
                    break;
                  case "80QAM":
                    tuning.ModulationType = ModulationType.Mod80Qam;
                    break;
                  case "896QAM":
                    tuning.ModulationType = ModulationType.Mod896Qam;
                    break;
                  case "8VSB":
                    tuning.ModulationType = ModulationType.Mod8Vsb;
                    break;
                  case "96QAM":
                    tuning.ModulationType = ModulationType.Mod96Qam;
                    break;
                  case "AMPLITUDE":
                    tuning.ModulationType = ModulationType.ModAnalogAmplitude;
                    break;
                  case "FREQUENCY":
                    tuning.ModulationType = ModulationType.ModAnalogFrequency;
                    break;
                  case "BPSK":
                    tuning.ModulationType = ModulationType.ModBpsk;
                    break;
                  case "OQPSK":
                    tuning.ModulationType = ModulationType.ModOqpsk;
                    break;
                  case "QPSK":
                    tuning.ModulationType = ModulationType.ModQpsk;
                    break;
                  default:
                    tuning.ModulationType = ModulationType.ModNotSet;
                    break;
                }
                tuning.SymbolRate = Int32.Parse(tpdata[2]) / 1000;
                _dvbcChannels.Add(tuning);
              }
              catch
              {
              }
            }
          }
        }
      } while (!(line == null));
      tin.Close();

      String newPath = String.Format("{0}\\dvbc\\{1}.xml", System.IO.Path.GetDirectoryName(fileName), MakeDVBCName(System.IO.Path.GetFileNameWithoutExtension(fileName)));
      System.IO.TextWriter parFileXML = System.IO.File.CreateText(newPath);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DVBCTuning>));
      xmlSerializer.Serialize(parFileXML, _dvbcChannels);
      parFileXML.Close();
      return true;
    }
  }
}
