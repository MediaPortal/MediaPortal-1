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

namespace ConvertTuningDetails
{
  class DVBT
  {
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

    public static bool ConvertList(String fileName)
    {
      string path = System.IO.Path.GetDirectoryName(fileName);
      string saveName;
      int frequencyOffset = 0;
      List<DVBTTuning> _dvbtChannels = new List<DVBTTuning>();
      Dictionary<int, int> frequencies = new Dictionary<int, int>();
      XmlDocument doc = new XmlDocument();
      doc.Load(fileName);
      XmlNodeList countryList = doc.SelectNodes("/dvbt/country");
      if (countryList != null)
        foreach (XmlNode nodeCountry in countryList)
        {
          XmlNode nodeName = nodeCountry.Attributes.GetNamedItem("name");
          saveName = MakeDVBTName(nodeName.Value);
          XmlNode nodeOffset = nodeCountry.Attributes.GetNamedItem("offset");
          if (nodeOffset != null)
          {
            if (nodeOffset.Value != null)
            {
              if (Int32.TryParse(nodeOffset.Value, out frequencyOffset) == false)
              {
                frequencyOffset = 0;
              }
            }
          }
          XmlNodeList nodeFrequencyList = nodeCountry.SelectNodes("carrier");
          if (nodeFrequencyList != null)
            foreach (XmlNode nodeFrequency in nodeFrequencyList)
            {
              string frequencyText = nodeFrequency.Attributes.GetNamedItem("frequency").Value;
              string bandwidthText = "8";
              if (nodeFrequency.Attributes.GetNamedItem("bandwidth") != null)
              {
                bandwidthText = nodeFrequency.Attributes.GetNamedItem("bandwidth").Value;
              }
              int frequency = Int32.Parse(frequencyText);
              int bandWidth = Int32.Parse(bandwidthText);
              DVBTTuning tuning = new DVBTTuning(frequency, bandWidth, frequencyOffset);
              _dvbtChannels.Add(tuning);
            }

          String newPath = String.Format("{0}\\dvbt\\{1}.xml", System.IO.Path.GetDirectoryName(fileName), saveName);
          System.IO.TextWriter parFileXML = System.IO.File.CreateText(newPath);
          XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<DVBTTuning>));
          xmlSerializer.Serialize(parFileXML, _dvbtChannels);
          parFileXML.Close();

          _dvbtChannels.Clear();
        }

      return true;
    }
  }
}
