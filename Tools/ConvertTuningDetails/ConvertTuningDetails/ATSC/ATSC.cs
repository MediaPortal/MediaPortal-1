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

  [Serializable]
  public class ATSCTuning
  {
    public int frequency;		 // frequency
    public ATSCTuning()
    {

    }
    public ATSCTuning(int f)
    {
      frequency = f;
    }
  }

  class ATSC
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

    
    public static bool ConvertList(string fileName)
    {
      List<ATSCTuning> _atscChannels = new List<ATSCTuning>();
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
            if (tpdata.Length != 1)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 1)
            {
              try
              {
                _atscChannels.Add(new ATSCTuning(Int32.Parse(tpdata[0])));
              }
              catch
              {
              }
            }
          }
        }
      } while (!(line == null));
      tin.Close();

      String newPath = String.Format("{0}\\atsc\\{1}.xml", System.IO.Path.GetDirectoryName(fileName), System.IO.Path.GetFileNameWithoutExtension(fileName));
      System.IO.TextWriter parFileXML = System.IO.File.CreateText(newPath);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ATSCTuning>));
      xmlSerializer.Serialize(parFileXML, _atscChannels);
      parFileXML.Close();
      return true;
    }
  }
}
